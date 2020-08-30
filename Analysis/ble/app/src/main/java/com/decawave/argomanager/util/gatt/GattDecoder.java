/*
 * LEAPS - Low Energy Accurate Positioning System.
 *
 * Copyright (c) 2016-2017, LEAPS. All rights reserved.
 */

package com.decawave.argomanager.util.gatt;

import android.support.annotation.NonNull;

import com.annimon.stream.function.BiFunction;
import com.decawave.argo.api.interaction.ErrorCode;
import com.decawave.argo.api.interaction.LocationData;
import com.decawave.argo.api.interaction.ProxyPosition;
import com.decawave.argo.api.struct.LocationDataMode;
import com.decawave.argo.api.struct.NetworkNode;
import com.decawave.argo.api.struct.NetworkNodeProperty;
import com.decawave.argo.api.struct.NodeStatistics;
import com.decawave.argo.api.struct.NodeType;
import com.decawave.argo.api.struct.OperatingFirmware;
import com.decawave.argo.api.struct.Position;
import com.decawave.argo.api.struct.RangingAnchor;
import com.decawave.argo.api.struct.ServiceData;
import com.decawave.argo.api.struct.UwbMode;
import com.decawave.argomanager.Constants;
import com.decawave.argomanager.argoapi.ble.BleConstants;
import com.decawave.argomanager.argoapi.ble.BleGattServiceRdonly;
import com.decawave.argomanager.argoapi.ble.ReadCharacteristicRequest;
import com.decawave.argomanager.argoapi.ble.SynchronousBleGatt;
import com.decawave.argomanager.argoapi.ble.connection.FwPollCommand;
import com.decawave.argomanager.argoapi.ext.NodeFactory;
import com.decawave.argomanager.ble.BleGattCharacteristic;
import com.decawave.argomanager.ble.BleGattService;
import com.decawave.argomanager.debuglog.ApplicationComponentLog;
import com.decawave.argomanager.debuglog.LogEntryTagFactory;
import com.decawave.argomanager.exception.GattCharacteristicDecodeException;
import com.decawave.argomanager.exception.GattRepresentationException;
import com.decawave.argomanager.util.Util;
import com.google.common.base.Preconditions;

import org.jetbrains.annotations.NotNull;
import org.jetbrains.annotations.Nullable;

import java.nio.BufferUnderflowException;
import java.nio.ByteBuffer;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.UUID;

import eu.kryl.android.common.Pair;
import eu.kryl.android.common.log.ComponentLog;
import rx.functions.Action4;

/**
 * Various GATT / BLE utility routines.
 */
public class GattDecoder {
    // logging
    public static final ComponentLog log = new ComponentLog(GattDecoder.class);
    public static final ApplicationComponentLog appLog = ApplicationComponentLog.newComponentLog(log, "GATT-DEC-ENC");
    // prebuilt commands
    private static final FwPollCommand.UploadComplete UPLOAD_COMPLETE = new FwPollCommand.UploadComplete();
    private static final FwPollCommand.UploadRefused UPLOAD_REFUSED = new FwPollCommand.UploadRefused(FwPollCommand.Type.FW_UPLOAD_REFUSED.ordinal());
    private static final FwPollCommand.SaveFailed SAVE_FAILED = new FwPollCommand.SaveFailed(FwPollCommand.Type.FW_SAVE_FAILED.ordinal());
    private static final FwPollCommand.SaveFailed SAVE_FAILED_INVALID_CHECKSUM = new FwPollCommand.SaveFailed(BleConstants.FW_POLL_COMMAND_SAVE_FAILED_INVALID_CHECKSUM);

    private final String deviceBleAddress;

    private final GattDecodeContext context;

    // package private
    GattDecoder(String deviceBleAddress, GattDecodeContext context) {
        this.deviceBleAddress = deviceBleAddress;
        this.context = context;
    }

    public GattDecodeContext getContext() {
        return context;
    }

    private Integer decodeMacStats(BleGattCharacteristic chMacStats) {
        if (chMacStats == null || chMacStats.emptyValue()) {
            return null;
        } // else:
        byte[] value = chMacStats.getValue();
        checkValueLength(4, value, chMacStats.getUuid());
        return chMacStats.getIntValue();
    }

    private List<Short> decodeAnchorList(BleGattCharacteristic chAnchorList) {
        if (chAnchorList == null || chAnchorList.emptyValue()) {
            return null;
        } // else: we will not return null
        byte[] value = chAnchorList.getValue();
        ByteBuffer buffer = Util.newByteBuffer(value);
        byte count = buffer.get();
        // check that the buffer is of proper size
        int remaining = buffer.remaining();
        if (remaining != count * 2) {
            throw GattCharacteristicDecodeException.newExactCharacteristicLength(deviceBleAddress, chAnchorList.getUuid(), count * 2 + 1, value.length);
        }
        List<Short> lst = new ArrayList<>(count);
        while (count-- > 0) {
            lst.add(decode2ByteNodeId(buffer));
        }
        return lst;
    }

    private static short decode2ByteNodeId(ByteBuffer buffer) {
        // decode a 2 byte node id
        return buffer.getShort();
    }

    private DeviceInfo decodeDeviceInfo(BleGattCharacteristic chDeviceInfo, NodeType nodeType) {
        if (chDeviceInfo == null || chDeviceInfo.emptyValue()) {
            return null;
        } // else:
        DeviceInfo deviceInfo = new DeviceInfo();
        byte[] value = chDeviceInfo.getValue();
        checkValueLength(29, 45, value, chDeviceInfo.getUuid());
        ByteBuffer buffer = Util.newByteBuffer(value);
        deviceInfo.nodeId = buffer.getLong();
        deviceInfo.hwVersion = buffer.getInt();
        deviceInfo.fw1Version = buffer.getInt();
        deviceInfo.fw2Version = buffer.getInt();
        deviceInfo.fw1Checksum = buffer.getInt();
        deviceInfo.fw2Checksum = buffer.getInt();
        if (buffer.remaining() > 1) {
            // this is IPV6 address
            byte[] ipv6Bytes = new byte[16];
            buffer.get(ipv6Bytes);
            // let's throw it away, we don't need it anymore
            appLog.we("deviceInfo contains obsolete property IPV6 address!", ErrorCode.GATT_REPRESENTATION, LogEntryTagFactory.getDeviceLogEntryTag(deviceBleAddress));
        }
        if (nodeType == NodeType.ANCHOR) {
            byte b = buffer.get();
            deviceInfo.bridge = (b & 0x80) != 0;
        }
        return deviceInfo;
    }

    private void checkValueLength(int expectedLength, byte[] value, UUID characteristicUuid) {
        if (value.length != expectedLength) {
            throw GattCharacteristicDecodeException.newExactCharacteristicLength(deviceBleAddress, characteristicUuid, expectedLength, value.length);
        }
    }

    @SuppressWarnings("SameParameterValue")
    private void checkValueLength(int expectedLength1, int expectedLength2, byte[] value, UUID characteristicUuid) {
        if (value.length != expectedLength1 && value.length != expectedLength2) {
            throw GattCharacteristicDecodeException.newExactCharacteristicLength(deviceBleAddress, characteristicUuid, expectedLength1, expectedLength2, value.length);
        }
    }

    private ClusterInfo decodeClusterInfo(BleGattCharacteristic chClusterInfo) {
        if (chClusterInfo == null || chClusterInfo.emptyValue()) {
            return null;
        }
        byte[] value = chClusterInfo.getValue();
        checkValueLength(5, value, chClusterInfo.getUuid());
        ClusterInfo r = new ClusterInfo();
        ByteBuffer buffer = Util.newByteBuffer(value);
        r.seatNumber = buffer.get();
        r.clusterMap = buffer.getShort();
        r.clusterNeighbourMap = buffer.getShort();
        return r;
    }

    private NodeStatistics decodeStatistics(BleGattCharacteristic chNodeStatistics) {
        if (chNodeStatistics == null || !chNodeStatistics.valueLoaded()) {
            return null;
        }
        byte[] val = chNodeStatistics.getValue();
        if (val.length < 176) {
            appLog.we("wrong statistics format, expecting at least 176 bytes, instead there is " + val.length
                    + " bytes", ErrorCode.GATT_REPRESENTATION_WARN, LogEntryTagFactory.getDeviceLogEntryTag(deviceBleAddress));
            // make sure that we do not return misleading info
            return new NodeStatistics();
        } // else:
        ByteBuffer bb = Util.newByteBuffer(val);
        NodeStatistics s = new NodeStatistics();
        
        /* System */
        s.uptime = getUInt32(bb);
        s.memfree = getUInt32(bb);
        s.drift_avg_rtc = bb.getFloat();
        s.mcu_temp = bb.get();

        /* API */
        s.api_err = bb.get();
        s.api_err_cnt = getUInt8(bb);
        s.api_dl_cnt = bb.get();

        /* BLE */
        s.ble_con_ok = getUInt16(bb);
        s.ble_dis_ok = getUInt16(bb);
        s.ble_err = getUInt16(bb);
        s.clk_sync = getUInt16(bb);

        /* Interfaces */
        s.uwb0_intr = getUInt32(bb);
        s.uwb0_rst = getUInt16(bb);
        s.uwb0_bpc = getUInt16(bb);
        s.rx_ok = getUInt32(bb);
        s.rx_err = getUInt32(bb);
        s.tx_err = getUInt32(bb);
        s.tx_errx = getUInt16(bb);

        /* MAC */
        s.reinit = getUInt16(bb);
        s.alma_tx_ok = getUInt32(bb);
        s.alma_rx_ok = getUInt32(bb);
        s.alma_tx_err = getUInt16(bb);
        s.bcn_tx_err = getUInt16(bb);
        s.bcn_tx_ok = getUInt32(bb);
        s.bcn_rx_ok = getUInt32(bb);
        s.cl_tx_ok = getUInt16(bb);
        s.cl_rx_ok = getUInt16(bb);
        s.cl_coll = getUInt16(bb);
        s.fwup_tx_ok = getUInt16(bb);
        s.fwup_tx_err = getUInt16(bb);
        s.fwup_rx_ok = getUInt16(bb);
        s.svc_tx_err = getUInt16(bb);
        s.svc_tx_ok = getUInt16(bb);
        s.svc_rx_ok = getUInt16(bb);
        s.bh_ev = getUInt16(bb);

        s.bh_rt = getUInt32(bb);
        s.bh_nort = getUInt32(bb);
        s.bh_buf_lost = new int[2];
        s.bh_buf_lost[0] = getUInt16(bb);
        s.bh_buf_lost[1] = getUInt16(bb);

        s.bh_tx_err = getUInt32(bb);
        s.bh_dl_err = getUInt32(bb);
        s.bh_dl_ok = getUInt32(bb);
        s.bh_ul_err = getUInt32(bb);
        s.bh_ul_ok = getUInt32(bb);
        s.fw_dl_tx_err = getUInt32(bb);
        s.fw_dl_iot_ok = getUInt32(bb);
        s.fw_ul_loc_ok = getUInt32(bb);
        s.fw_ul_iot_ok = getUInt32(bb);
        s.ul_tx_err = getUInt32(bb);
        s.dl_iot_ok = getUInt32(bb);
        s.ul_loc_ok = getUInt32(bb);
        s.ul_iot_ok = getUInt32(bb);
        s.sec_err = getUInt32(bb);

        /* Measurements / Data */
        s.tdoa_ok = getUInt32(bb);
        s.tdoa_err = getUInt32(bb);
        s.twr_ok = getUInt32(bb);
        s.twr_err = getUInt32(bb);

        int remaining = bb.remaining();
        if (remaining % 4 != 0) {
            appLog.we("wrong statistics format, expecting remaining bytes to be multiplication of 8, instead there is " + remaining
                     + " bytes", ErrorCode.GATT_REPRESENTATION_WARN, LogEntryTagFactory.getDeviceLogEntryTag(deviceBleAddress));
            // return at least the info which we have parsed so far
            return s;
        }
        s.reserved = new long[remaining / 4];
        int i = 0;
        while (bb.remaining() > 0) {
            s.reserved[i] = getUInt32(bb);
        }
        return s;
    }

    private static long getUInt32(ByteBuffer byteBuffer) {
        return 0x00000000ffffffffL & byteBuffer.getInt();
    }
    
    private static int getUInt16(ByteBuffer byteBuffer) {
        return 0x0000ffff & byteBuffer.getShort();
    }
    
    private static short getUInt8(ByteBuffer byteBuffer) {
        return (short) (0x00ff & byteBuffer.get());
    }

    private String decodeString(BleGattCharacteristic chLabel) {
        if (chLabel == null || !chLabel.valueLoaded()) {
            return null;
        }
        return chLabel.getStringValue();
    }

    private Pair<Integer,Integer> decodeUpdateRate(BleGattCharacteristic chUpdateRate) {
        if (chUpdateRate == null || chUpdateRate.emptyValue()) {
            return null;
        }
        int l = chUpdateRate.getValue().length;
        if (l != 8) {
            throw GattCharacteristicDecodeException.newExactCharacteristicLength(deviceBleAddress, chUpdateRate.getUuid(), 8, l);
        }
        ByteBuffer bb = Util.newByteBuffer(chUpdateRate.getValue());
        return new Pair<>(bb.getInt(), bb.getInt());
    }

    private LocationDataMode decodeLocationDataMode(BleGattCharacteristic chLocationDataMode) {
        if (chLocationDataMode == null || chLocationDataMode.emptyValue()) {
            return null;
        }
        byte val = chLocationDataMode.getValue()[0];
        switch (val) {
            case 0:
                return LocationDataMode.POSITION;
            case 1:
                return LocationDataMode.DISTANCES;
            case 2:
                return LocationDataMode.POSITION_AND_DISTANCES;
            default:
                throw new GattRepresentationException(deviceBleAddress, "unexpected location data mode value: " + val +", expecting 0,1 or 2");
        }
    }

    @Nullable
    public LocationData decodeLocationData(byte[] bytes) {
        if (bytes == null || bytes.length == 0) {
            return null;
        }
        ByteBuffer bb = Util.newByteBuffer(bytes);
        Position position = null;
        List<RangingAnchor> distances = null;
        try {
            //
            byte type = bb.get();
            switch (type) {
                case 0:
                    // position
                    position = decodePosition(bb);
                    break;
                case 1:
                    // distances
                    distances = decodeDistances(bb, bytes);
                    break;
                case 2:
                    // position and distances
                    position = decodePosition(bb);
                    distances = decodeDistances(bb, bytes);
                    break;
                default:
                    throw new GattRepresentationException(deviceBleAddress, "unexpected location data type: " + type + ", expecting 0,1 or 2. Content: " + GattEncoder.printByteArray(bytes));
            }
        } catch (BufferUnderflowException e) {
            throw new GattRepresentationException(deviceBleAddress, "unexpected location data content: buffer underflow. Content: " + GattEncoder.printByteArray(bytes), e);
        }
        return new LocationData(position, distances);
    }

    private List<RangingAnchor> decodeDistances(ByteBuffer buffer, byte[] bytes) {
        int count = 0x00FF & buffer.get();
        List<RangingAnchor> lst = new ArrayList<>(count);
        while (count-- > 0) {
            // decode node id first
            short nodeId = decode2ByteNodeId(buffer);
            int distance = buffer.getInt();
            byte qualityFactor = buffer.get();
            lst.add(new RangingAnchor(nodeId, distance, qualityFactor));
        }
        if (buffer.remaining() > 0) {
            throw new GattRepresentationException(deviceBleAddress, "unexpected location data content: buffer overflow. Content: " + GattEncoder.printByteArray(bytes));
        }
        return lst;
    }

    @NonNull
    private Position decodePosition(ByteBuffer buff) {
        int rX = buff.getInt();
        int rY = buff.getInt();
        int rZ = buff.getInt();
        Position r = new Position(rX, rY, rZ);
        r.qualityFactor = buff.get();
        return r;
    }


    private Short decodeShort(BleGattCharacteristic characteristic) {
        if (characteristic == null || characteristic.emptyValue()) {
            return null;
        }
        byte[] value = characteristic.getValue();
        checkValueLength(2, value, characteristic.getUuid());
        // decode as short
        ByteBuffer buf = Util.newByteBuffer(value);
        return buf.getShort();
    }

    public @Nullable GattOperationMode getOperationMode(SynchronousBleGatt synchronousBleGatt) throws GattRepresentationException {
        BleGattServiceRdonly networkNodeService = synchronousBleGatt.getService(BleConstants.SERVICE_UUID_NETWORK_NODE);
        if (networkNodeService == null) {
            throw new GattRepresentationException(deviceBleAddress, "GATT model problem: missing NETWORK_NODE service");
        }
        return decodeOperationMode(networkNodeService.getCharacteristic(BleConstants.CHARACTERISTIC_OPERATION_MODE));
    }

    public
    FwPollCommand decodeFwPollCommand(byte[] value) {
        if (value == null || value.length == 0) {
            return null;
        }
        ByteBuffer bb = Util.newByteBuffer(value);
        byte type = bb.get();
        switch (type) {
            case BleConstants.FW_POLL_COMMAND_UPLOAD_REFUSED:
                return UPLOAD_REFUSED;
            case BleConstants.FW_POLL_COMMAND_BUFFER_REQUEST:
                int offset = bb.getInt();
                int size = bb.getInt();
                return new FwPollCommand.BufferRequest(offset, size);
            case BleConstants.FW_POLL_COMMAND_UPLOAD_COMPLETE:
                return UPLOAD_COMPLETE;
            case BleConstants.FW_POLL_COMMAND_SAVE_FAILED:
                return SAVE_FAILED;
            case BleConstants.FW_POLL_COMMAND_SAVE_FAILED_INVALID_CHECKSUM:
                return SAVE_FAILED_INVALID_CHECKSUM;
            default:
                throw new GattRepresentationException(deviceBleAddress, "GATT model problem: unexpected poll command type " + type);
        }
    }

    public NetworkNode decode(SynchronousBleGatt gatt, Set<ReadCharacteristicRequest> characteristicsToDecode) {
        BleGattService nnService = gatt.getService(BleConstants.SERVICE_UUID_NETWORK_NODE);
        BleGattCharacteristic chOperationMode = nnService.getCharacteristic(BleConstants.CHARACTERISTIC_OPERATION_MODE);
        // retrieve operation mode first - this must be always present
        GattOperationMode gattOperationMode = decodeOperationMode(chOperationMode);
        if (gattOperationMode == null) {
            throw new GattRepresentationException(deviceBleAddress, "Operation Mode characteristic value not initialized!");
        }
        NodeType nodeType = gattOperationMode.nodeType;
        NodeFactory.NodeBuilder builder = NodeFactory.newBuilder(nodeType, null);
        // set the BLE address
        builder.setBleAddress(gatt.getDeviceAddress());
        // map characteristics to properties
        for (ReadCharacteristicRequest characteristic : characteristicsToDecode) {
            decodeCharacteristicAsPropertyValues(characteristic, gatt, builder, nodeType);
        }
        return builder.build();
    }

    private static final Map<ReadCharacteristicRequest, Action4<GattDecoder, BleGattCharacteristic, NodeFactory.NodeBuilder, NodeType>>
            CHARACTERISTIC_REQUEST_TO_BUILDER_INVOCATION;

    static {
        MappingBuilder builder = new MappingBuilder();
        builder.map(BleConstants.SERVICE_UUID_STD_GAP, BleConstants.CHARACTERISTIC_STD_LABEL,
                (gattDecoder,gattCharacteristic,nodeBuilder,nodeType) -> nodeBuilder.setLabel(gattDecoder.decodeString(gattCharacteristic)))
                .map(BleConstants.CHARACTERISTIC_DEVICE_INFO,
                (gattDecoder,gattCharacteristic,nodeBuilder,nodeType) -> {
                    DeviceInfo deviceInfo = gattDecoder.decodeDeviceInfo(gattCharacteristic, nodeType);
                    Preconditions.checkNotNull(deviceInfo);
                    nodeBuilder.setProperty(NetworkNodeProperty.ID, deviceInfo.getId())
                            .setProperty(NetworkNodeProperty.HW_VERSION, deviceInfo.getHwVersion())
                            .setProperty(NetworkNodeProperty.FW1_VERSION, deviceInfo.getFw1Version())
                            .setProperty(NetworkNodeProperty.FW2_VERSION, deviceInfo.getFw2Version())
                            .setProperty(NetworkNodeProperty.FW1_CHECKSUM, deviceInfo.getFw1Checksum())
                            .setProperty(NetworkNodeProperty.FW2_CHECKSUM, deviceInfo.getFw2Checksum());
                    if (nodeType == NodeType.ANCHOR) {
                        nodeBuilder.setProperty(NetworkNodeProperty.ANCHOR_BRIDGE, deviceInfo.getBridge());
                    }
                })
                .map(BleConstants.CHARACTERISTIC_NETWORK_ID, NetworkNodeProperty.NETWORK_ID, GattDecoder::decodeShort, false)
                .map(BleConstants.CHARACTERISTIC_LOCATION_DATA,
                    (gattDecoder,gattCharacteristic,nodeBuilder,nodeType) -> {
                        LocationData locationData = gattDecoder.decodeLocationData(gattCharacteristic.getValue());
                        // location data might be null (if they are not known)
                        if (locationData != null && !locationData.isEmpty()) {
                            if (nodeType == NodeType.TAG) {
                                ((NodeFactory.TagNodeBuilder) nodeBuilder).setLocationData(locationData);
                            } else {
                                // this is anchor - separate properties in one characteristic
                                NodeFactory.AnchorNodeBuilder anBuilder = (NodeFactory.AnchorNodeBuilder) nodeBuilder;
                                if (locationData.position != null)
                                    anBuilder.setPosition(locationData.position);
                                if (locationData.distances != null) {
                                    anBuilder.setDistances(locationData.distances);
                                }
                            }
                        }
                   })
                .map(BleConstants.CHARACTERISTIC_LOCATION_DATA_MODE, NetworkNodeProperty.LOCATION_DATA_MODE, GattDecoder::decodeLocationDataMode, true)
                .map(BleConstants.CHARACTERISTIC_TAG_UPDATE_RATE,
                (gattDecoder, bleGattCharacteristic, nodeBuilder, nodeType) -> {
                    if (Constants.DEBUG) {
                        Preconditions.checkState(nodeType == NodeType.TAG);
                    }
                    Pair<Integer, Integer> updateRate = gattDecoder.decodeUpdateRate(bleGattCharacteristic);
                    NodeFactory.TagNodeBuilder tagNodeBuilder = (NodeFactory.TagNodeBuilder) nodeBuilder;
                    tagNodeBuilder
                            .setUpdateRate(updateRate.first)
                            .setStationaryUpdateRate(updateRate.second);
                })
                .map(BleConstants.CHARACTERISTIC_STATISTICS, NetworkNodeProperty.NODE_STATISTICS, GattDecoder::decodeStatistics, true)
                .map(BleConstants.CHARACTERISTIC_PASSWORD, NetworkNodeProperty.PASSWORD, GattDecoder::decodeString, false)
                .map(BleConstants.CHARACTERISTIC_OPERATION_MODE,
                (gattDecoder, bleGattCharacteristic, nodeBuilder, nodeType) -> {
                    GattOperationMode gattOperationMode = gattDecoder.decodeOperationMode(bleGattCharacteristic);
                    Preconditions.checkNotNull(gattOperationMode);
                    nodeBuilder
                            // online is common
                            .setProperty(NetworkNodeProperty.UWB_MODE, gattOperationMode.uwbMode)
                            // operating firmware
                            .setProperty(NetworkNodeProperty.OPERATING_FIRMWARE, gattOperationMode.operatingFirmware)
                            // LED indication
                            .setProperty(NetworkNodeProperty.LED_INDICATION_ENABLE, gattOperationMode.ledIndicationEnable)
                            // firmware update
                            .setProperty(NetworkNodeProperty.FIRMWARE_UPDATE_ENABLE, gattOperationMode.firmwareUpdateEnable)
                            // BLE
                            .setProperty(NetworkNodeProperty.BLE_ENABLE, gattOperationMode.bleEnable)
                    ;
                    if (nodeType == NodeType.ANCHOR) {
                        nodeBuilder.setProperty(NetworkNodeProperty.ANCHOR_INITIATOR, gattOperationMode.initiator);
                    } else if (nodeType == NodeType.TAG) {
                        nodeBuilder.setProperty(NetworkNodeProperty.TAG_LOCATION_ENGINE_ENABLE, gattOperationMode.locationEngineEnable)
                            .setProperty(NetworkNodeProperty.TAG_LOW_POWER_MODE_ENABLE, gattOperationMode.lowPowerModeEnable)
                            .setProperty(NetworkNodeProperty.TAG_ACCELEROMETER_ENABLE, gattOperationMode.accelerometerEnable);
                    }
                })
                .map(BleConstants.CHARACTERISTIC_ANCHOR_MAC_STATS, NetworkNodeProperty.ANCHOR_MAC_STATS, GattDecoder::decodeMacStats, true)
                .map(BleConstants.CHARACTERISTIC_ANCHOR_CLUSTER_INFO,
                (gattDecoder, bleGattCharacteristic, nodeBuilder, nodeType) -> {
                    ClusterInfo clusterInfo = gattDecoder.decodeClusterInfo(bleGattCharacteristic);
                    Preconditions.checkNotNull(clusterInfo);
                    nodeBuilder.setProperty(NetworkNodeProperty.ANCHOR_SEAT, clusterInfo.seatNumber)
                            .setProperty(NetworkNodeProperty.ANCHOR_CLUSTER_MAP, clusterInfo.clusterMap)
                            .setProperty(NetworkNodeProperty.ANCHOR_CLUSTER_NEIGHBOUR_MAP, clusterInfo.clusterNeighbourMap);
                })
                .map(BleConstants.CHARACTERISTIC_ANCHOR_LIST, NetworkNodeProperty.ANCHOR_AN_LIST, GattDecoder::decodeAnchorList, true)
                .map(BleConstants.CHARACTERISTIC_ANCHOR_MAC_STATS, NetworkNodeProperty.ANCHOR_MAC_STATS, GattDecoder::decodeMacStats, true)
                .voidMap(BleConstants.CHARACTERISTIC_PROXY_POSITIONS);
        // build the mapping
        CHARACTERISTIC_REQUEST_TO_BUILDER_INVOCATION = builder.mapping;
    }

    public List<ProxyPosition> decodeProxyPositionData(byte[] value) {
        if (value == null || value.length == 0) {
            return null;
        }
        return decodeProxyPositions(value);
    }

    @NotNull
    private List<ProxyPosition> decodeProxyPositions(byte[] bytes) {
        ByteBuffer bb = Util.newByteBuffer(bytes);
        //
        byte size = bb.get();
        if (size < 0) {
            throw new GattRepresentationException(deviceBleAddress, "unexpected proxy position declared size: " + size + ", expecting positive value" + GattEncoder.printByteArray(bytes));
        }
        if (bb.remaining() != size * 15) {
            throw GattCharacteristicDecodeException.newExactCharacteristicLength(deviceBleAddress, BleConstants.CHARACTERISTIC_PROXY_POSITIONS, size * 15 + 1, bytes.length);
        }
        List<ProxyPosition> positions = new ArrayList<>(size);
        while (size-- > 0) {
            ProxyPosition Position = new ProxyPosition(decode2ByteNodeId(bb), decodePosition(bb));


            if((Position.position.x == 0) && (Position.position.y == 0) && (Position.position.z == 0))
                continue;
            positions.add(Position);
        }
        return positions;
    }

    private static class MappingBuilder {
        private Map<ReadCharacteristicRequest, Action4<GattDecoder, BleGattCharacteristic, NodeFactory.NodeBuilder, NodeType>> mapping;

        MappingBuilder() {
            mapping = new HashMap<>();
        }

        MappingBuilder map(UUID serviceUuid, UUID charUuid, Action4<GattDecoder, BleGattCharacteristic, NodeFactory.NodeBuilder, NodeType> action) {
            mapping.put(new ReadCharacteristicRequest(serviceUuid, charUuid), action);
            return this;
        }

        @SuppressWarnings("UnusedReturnValue")
        MappingBuilder voidMap(UUID charUuid) {
            mapping.put(new ReadCharacteristicRequest(BleConstants.SERVICE_UUID_NETWORK_NODE, charUuid), (gattDecoder, bleGattCharacteristic, nodeBuilder, nodeType) -> {});
            return this;
        }

        MappingBuilder map(UUID charUuid, Action4<GattDecoder, BleGattCharacteristic, NodeFactory.NodeBuilder, NodeType> action) {
            mapping.put(new ReadCharacteristicRequest(BleConstants.SERVICE_UUID_NETWORK_NODE, charUuid), action);
            return this;
        }

        // shortened version of simple characteristic to property mapping
        MappingBuilder map(UUID charUuid, NetworkNodeProperty property, BiFunction<GattDecoder,BleGattCharacteristic,Object> decodeFunction, boolean nonNull) {
            map(charUuid,
                    (gattDecoder,gattCharacteristic,nodeBuilder,nodeType) -> {
                        Object value = decodeFunction.apply(gattDecoder, gattCharacteristic);
                        if (nonNull && value == null) {
                            throw new GattRepresentationException(gattDecoder.deviceBleAddress,
                                    "value of " + BleConstants.MAP_CHARACTERISTIC_TITLE.get(gattCharacteristic.getUuid()) + " must not be null!");
                        }
                        nodeBuilder.setProperty(property, value);
                    });
            return this;
        }

    }


    private void decodeCharacteristicAsPropertyValues(
            ReadCharacteristicRequest readRequest,
            SynchronousBleGatt synchronousBleGatt,
            NodeFactory.NodeBuilder builder,
            NodeType nodeType) {
        Action4<GattDecoder, BleGattCharacteristic, NodeFactory.NodeBuilder, NodeType> invocation = CHARACTERISTIC_REQUEST_TO_BUILDER_INVOCATION.get(readRequest);
        if (Constants.DEBUG) {
            Preconditions.checkNotNull(invocation, "cannot find decode rule for " + readRequest);
        }
        BleGattServiceRdonly service = synchronousBleGatt.getService(readRequest.serviceUuid);
        if (service == null) {
            throw new GattRepresentationException(deviceBleAddress, "GATT model problem: missing " + readRequest.serviceUuid + " service");
        }
        BleGattCharacteristic characteristic = service.getCharacteristic(readRequest.characteristicUuid);
        if (characteristic == null) {
            throw new GattRepresentationException(deviceBleAddress, "GATT model problem: missing " + BleConstants.MAP_CHARACTERISTIC_TITLE.get(readRequest.characteristicUuid) + " characteristic");
        }
        // now call the decoding routine
        invocation.call(this, characteristic, builder, nodeType);
    }

    @SuppressWarnings("WeakerAccess")
    public static class GattOperationMode {
        public NodeType nodeType;
        public UwbMode uwbMode;
        public OperatingFirmware operatingFirmware;
        public Boolean firmwareUpdateEnable;
        public Boolean initiator;
        public Boolean bleEnable;
        public Boolean accelerometerEnable;
        public Boolean ledIndicationEnable;
        public Boolean lowPowerModeEnable;
        public Boolean locationEngineEnable;

    }

    private @Nullable GattOperationMode decodeOperationMode(BleGattCharacteristic characteristic) {
        if (characteristic == null || characteristic.emptyValue()) {
            return null;
        } // decode bit fields
        byte[] value = characteristic.getValue();
        if (value.length != 2) {
            throw GattCharacteristicDecodeException.newExactCharacteristicLength(deviceBleAddress, characteristic.getUuid(), 2, value.length);
        }
        // fill the encode context
        GattOperationMode om = decodeOperationMode(deviceBleAddress, value);
        // remember the operation mode
        context.setOperationMode(om);
        // return
        return om;
    }

    static @NotNull GattOperationMode decodeOperationMode(String deviceBleAddress, byte[] value) {
        short s = Util.newByteBuffer(value).getShort();
        GattOperationMode r = new GattOperationMode();
        // decode the operation mode
        byte b1 = (byte) (s & 0xFF);
        // first byte
        r.nodeType = (b1 & 0x80) != 0 ? NodeType.ANCHOR : NodeType.TAG;
        r.uwbMode = getUwbDecodeMode(deviceBleAddress, (byte) ((b1 & 0x60) >> 5));
        r.operatingFirmware = (b1 & 0x10) != 0 ? OperatingFirmware.FW2 : OperatingFirmware.FW1;
        r.accelerometerEnable = (b1 & 0x08) != 0;
        r.ledIndicationEnable = (b1 & 0x04) != 0;
        r.firmwareUpdateEnable = (b1 & 0x02) != 0;
        r.bleEnable = (b1 & 0x01) != 0;
        // second byte - node type specific
        byte b2 = (byte) (s >> 8);
        // anchor
        r.initiator = (b2 & 0x80) != 0;
        // tag
        r.lowPowerModeEnable = (b2 & 0x40) != 0;
        r.locationEngineEnable = (b2 & 0x20) != 0;
        // return the parsed operation mode
        return r;
    }

    private static UwbMode getUwbDecodeMode(String deviceBleAddress, byte number) {
        if (number >= UwbMode.values().length) {
            throw new GattRepresentationException(deviceBleAddress, "invalid UWB mode encoded in operationMode: "
                    + Util.formatAsHexa(number, true));
        }
        return UwbMode.values()[number];
    }

    /**
     * For now, we are able to decode node type/operation mode only.
     */
    public void decodeServiceData(byte[] serviceDataIn, ServiceData serviceDataOut) {
        if (serviceDataIn.length != 2) {
            throw GattCharacteristicDecodeException.newExactCharacteristicLength(deviceBleAddress, 2, serviceDataIn.length);
        }
        byte flags = serviceDataIn[0];
        serviceDataOut.operationMode = (0x80 & flags) == 0 ? NodeType.TAG : NodeType.ANCHOR;
        serviceDataOut.initiator = (0x04 & flags) != 0;
        serviceDataOut.bridge = (0x02 & flags) != 0;
        serviceDataOut.online = (0x01 & flags) != 0;
        serviceDataOut.changeCounter = serviceDataIn[1];
    }

}
