package com.marin2.decawave.unity3dplugin;

import java.util.LinkedList;

/**
 * Created by mhjtas on 19.1.2017.
 * Copyright: University of Turku & Mika Taskinen
 */
public class DecawaveParser {

    /**
     * The minimum buffer needs to be at least 4096
     */
    public static final int MINIMUM_BUFFER = 4096;

    /**
     * The buffer for incoming serial data
     */
    private final byte[] buffer;
    /**
     * Active count of the buffer
     */
    private int bufferCount;

    /**
     * The serial characters related to this parser
     */
    private final String serial;
    /**
     * Temporary holding station for packets
     */
    private final LinkedList<DecawavePacket> packets;

    /**
     * These (final) fields are for defining the packet
     * and are used to parsing the needed data out
     */
    private final int packet_ma_length = 4;
    private final int packet_t_length = 3;
    private final int packet_val1_length = 8;
    private final int packet_val2_length = 8;
    private final int packet_0f_length = 4;
    private final int packet_pair_length = 2;
    private final int packet_val3_length = 8;
    private final int packet_4044_1_length = 4;
    private final int packet_4044_2_length = 4;
    private final int packet_t2_length = 2;
    private final int packetLength = packet_ma_length + 1 + packet_t_length + 1 + packet_val1_length + 1 + packet_val2_length + 1 + packet_0f_length + 1 + packet_pair_length + 1
            + packet_val3_length + 1 + packet_4044_1_length + 1 + packet_4044_2_length + 1 + packet_t2_length;

    /**
     * Temporary data holders for the functions.
     * These values will be made into packet when done
     */
    private int deviceId;
    private int distanceInMillimeters;

    /**
     * Creates a DecawaveParser
     * @param serial The serial characters that define the device behing the parser
     */
    public DecawaveParser( String serial ) {
        this.serial = serial;
        packets = new LinkedList<>();
        buffer = new byte[MINIMUM_BUFFER];
    }

    /**
     * Creates a DecawaveParser with custom bufferCapacity
     * @param serial The serial characters that define the device behing the parser
     * @param bufferCapacity The custom buffer capacity. The minimum value cannot be lowered
     */
    public DecawaveParser( String serial, int bufferCapacity ) {
        this.serial = serial;
        packets = new LinkedList<>();
        buffer = new byte[Math.max(MINIMUM_BUFFER, bufferCapacity)];
    }

    /**
     * Inserts data into the buffer and finds packets from it
     * @param data Array of input data
     */
    public void put( byte[] data ) {
        // Copy the data to buffer
        System.arraycopy( data, 0, buffer, bufferCount, data.length );
        bufferCount += data.length;

        // The algorithm is run by the data as long as needed
        while ( true ) {
            // Find first recognizable characters
            int index = findIndexStart();
            if(index < 0) {
                // Index not found -> erase buffer and stop
                bufferCount = 0;
                break;
            }
            if(!hasRoom(index)){
                // If there is not enough data for a packet to exist ->
                // Clear unused data and wait for more (leave)
                bufferCount -= index;
                System.arraycopy( buffer, index, buffer, 0, bufferCount );
            }
            if(!isCorrectPacket(index)) {
                // Packet is not right -> skip 1 and try again
                bufferCount -= index + 1;
                System.arraycopy( buffer, index, buffer, 0, bufferCount );
                continue;
            }
            // Packet found -> save it to packet buffer
            packets.add(new DecawavePacket( deviceId, distanceInMillimeters ));

            // Clear and flush the buffer to the after position of the last packet
            // (and then continue)
            bufferCount -= index + packetLength;
            System.arraycopy(buffer, index + packetLength, buffer, 0, bufferCount);
        }
    }

    /**
     * Tests if buffer can hold a packet from given offset position forth
     * @param offset The start point of the test
     * @return true if there is enough data after the offset
     */
    private boolean hasRoom( int offset ) {
        return bufferCount >= offset + packetLength;
    }

    /**
     * Finds the starting position of the potential packet if there is one
     * @return The offset of potential packet, -1 otherwise
     */
    private int findIndexStart() {
        for(int i = 0; i < bufferCount; i++){
            if(buffer[i] == (byte)'m')
                return i;
        }
        return -1;
    }

    /**
     * Translates a value between 0 - 16 into hex character
     * @param value given value
     * @return hex character (as integer) or null character if given value was too large or small(?)
     */
    private int translateHex(byte value){
        return value >= '0' && value <= '9' ? value - '0' : value >= 'a' && value <= 'f' ? value - 'a' + 10 : value >= 'A' && value <= 'F' ? value - 'A' + 10 : 0;
    }

    /**
     * Checks if a given number represents an ascii hex character
     * @param v small number
     * @return true if v is ascii hex presentation
     */
    private boolean isHex( byte v ) {
        return ( v >= '0' && v <= '9' ) || ( v >= 'a' && v <= 'f' ) || ( v >= 'A' && v <= 'F' );
    }

    private int createIntegerHex(byte[] dataSource, int offset, int length) {
        int ret = 0;
        for ( int i = offset + length - 1, multi = 1; i >= offset; i--, multi <<= 4 )
        {
            ret += translateHex( dataSource[i] ) * multi;
        }
        return ret;
    }

    /**
     * Checks if a packet is a correct packet and stored data from it
     * @param index The offset index to start from
     * @return true if packet was correct
     */
    private boolean isCorrectPacket( int index ) {

        if ( buffer[index] != (byte)'m' || buffer[index +1] != (byte)'a' )
            return false;

        if ( !isHex( buffer[index + 2] ) || !isHex( buffer[index + 3] ) )
            return false;

        deviceId = createIntegerHex( buffer, index + 2, 2 );
        index += 5;

        if ( buffer[index] != (byte)'t' )
            return false;

        if ( !isHex( buffer[index + 1] ) || !isHex( buffer[index + 2] ) )
            return false;

        index += 4;

        for ( int i = 0, j = index; i < 8; i++, j++ )
        {
            if ( !isHex( buffer[j] ) )
                return false;
        }

        distanceInMillimeters = createIntegerHex( buffer, index, 8 );

        index += 9;

        for ( int i = 0, j = index; i < 8; i++, j++ )
        {
            if ( !isHex( buffer[j] ) )
                return false;
        }

        index += 9;

        for ( int i = 0, j = index; i < 4; i++, j++ )
        {
            if ( !isHex( buffer[j] ) )
                return false;
        }

        index += 5;

        for ( int i = 0, j = index; i < 2; i++, j++ )
        {
            if ( !isHex( buffer[j] ) )
                return false;
        }

        index += 3;

        for ( int i = 0, j = index; i < 8; i++, j++ )
        {
            if ( !isHex( buffer[j] ) )
                return false;
        }

        index += 9;

        for ( int i = 0, j = index; i < 4; i++, j++ )
        {
            if ( !isHex( buffer[j] ) )
                return false;
        }

        index += 5;

        for ( int i = 0, j = index; i < 4; i++, j++ )
        {
            if ( !isHex( buffer[j] ) )
                return false;
        }

        index += 5;

        if ( buffer[index] != (byte)'t' )
            return false;

        if ( !isHex( buffer[index + 1] ) )
            return false;

        return true;
    }

    /**
     * Takes one packet from the parser (takes it away from the parser)
     * @return a packet
     */
    public DecawavePacket popPacket() {
        return packets.pop();
    }

    /**
     * Checks if there are any packets in the packet buffer
     * @return true if there is at least one packet available
     */
    public boolean hasPacket() {
        return packets.size() > 0;
    }

}
