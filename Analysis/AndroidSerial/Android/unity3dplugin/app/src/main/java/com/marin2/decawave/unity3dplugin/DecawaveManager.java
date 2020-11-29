package com.marin2.decawave.unity3dplugin;

import android.annotation.TargetApi;
import android.app.Activity;
import android.content.Context;
import android.hardware.usb.UsbDevice;
import android.hardware.usb.UsbDeviceConnection;
import android.hardware.usb.UsbInterface;
import android.hardware.usb.UsbManager;
import android.os.Build;
import android.os.Handler;
import android.support.annotation.RequiresApi;

import com.felhr.deviceids.CH34xIds;
import com.felhr.deviceids.CP210xIds;
import com.felhr.deviceids.FTDISioIds;
import com.felhr.deviceids.PL2303Ids;
import com.felhr.usbserial.UsbSerialDevice;
import com.felhr.usbserial.UsbSerialInterface;

import java.util.HashMap;
import java.util.HashSet;
import java.util.LinkedList;

import static com.felhr.usbserial.UsbSerialDevice.isCdcDevice;

/**
 * Created by mhjtas on 19.1.2017.
 * Copyright: University of Turku & Mika Taskinen
 */

public class DecawaveManager {

    /**
     * Cendor and product ids are the same for every decawave device (possibly?)
     */
    private static final int vendorId = 4966;
    private static final int productId = 0261;

    /**
     * Singleton principle demands this (no actual constructor fo public use)
     */
    private static DecawaveManager instance;
    private final Activity activity;
    private final UsbManager usbManager;
    private final Runnable runnable;
    private int waitTimeMillis = 500;
    private final Handler handler;
    private final HashMap<String, UsbSerialDevice> serialDevices;
    private final LinkedList<String> log;
    private final HashMap<String, DecawaveParser> serialParsers;
    private boolean debug = true;

    private DecawaveManager( Activity activity ) {
        serialParsers = new HashMap<>();
        serialDevices = new HashMap<>();
        log = new LinkedList<>();
        this.activity = activity;
        this.usbManager = (UsbManager)activity.getSystemService( Context.USB_SERVICE );
        runnable = new Runnable() {
            @TargetApi( Build.VERSION_CODES.LOLLIPOP )
            @RequiresApi( api = Build.VERSION_CODES.LOLLIPOP )
            @Override
            public void run() {
                DecawaveManager.this.checkUsbDevices();
                handler.removeCallbacks( this );
                handler.postDelayed( this, waitTimeMillis );
            }
        };
        handler = new Handler();
        handler.removeCallbacks( runnable );
        handler.post( runnable );
    }

    /**
     * Gets the instance of the decawave manager.
     * @param activity Can only be set once.
     * @return The DecawaveManager instance.
     */
    public static DecawaveManager getInstance( Activity activity ) {

        if(instance == null) {
            instance = new DecawaveManager( activity );
        }

        return  instance;

    }

    /**
     * Get the wait time for usb device checker
     * @return
     */
    public int getWaitTimeMillis() {
        return waitTimeMillis;
    }

    /**
     * Set the wait time for usb device checker
     * @param waitTimeMillis
     */
    public void setWaitTimeMillis( int waitTimeMillis ) {
        if(waitTimeMillis <= 0)
            waitTimeMillis = 1;
        this.waitTimeMillis = waitTimeMillis;
    }

    @RequiresApi( api = Build.VERSION_CODES.LOLLIPOP )
    private void checkUsbDevices() {

        // Get all active usb devices attached
        HashMap<String, UsbDevice> foundDevices;
        try {
            foundDevices = usbManager.getDeviceList();
        }
        catch ( Exception exception ) {
            log.add( "Exception on usb manager: " + exception.getClass().getName() + ": " + exception.getMessage() );
            return;
        }

        // Filter all devices that have the correct vendor and product id
        HashMap<String, UsbDevice> filteredDevices = new HashMap<>();
        for( String key : foundDevices.keySet() ) {
            try {
                UsbDevice device = foundDevices.get( key );
                if ( device.getVendorId() == vendorId && device.getProductId() == productId ) {
                    filteredDevices.put( device.getSerialNumber(), device );
                }
            }
            catch ( Exception exception ) {
                log.add( "Exception on usb device: " + exception.getClass().getName() + ": " + exception.getMessage() );
                continue;
            }
        }

        // Determine which devices are new, previously unseen
        HashSet<String> addables = new HashSet<>();
        HashSet<String> removables = new HashSet<>();
        try {
            for ( String currentSerial : filteredDevices.keySet() ) {
                if ( !serialDevices.containsKey( currentSerial ) )
                    addables.add( currentSerial );
            }
        }
        catch ( Exception exception ) {
            log.add( "Exception on getting addables: " + exception.getClass().getName() + ": " + exception.getMessage() );
            return;
        }
        try {
            for ( String oldSerial : serialDevices.keySet() ) {
                if ( !filteredDevices.containsKey( oldSerial ) ) {
                    removables.add( oldSerial );
                }
            }
        }
        catch( Exception exception ) {
            log.add( "Exception on getting removables: " + exception.getClass().getName() + ": " + exception.getMessage() );
            return;
        }

        // Remove devices that have disappeared
        for( String serial : removables ) {
            removeSerial(serial);
        }

        // Add new devices
        for( String serial : addables ) {
            createSerial( serial, filteredDevices.get(serial));
        }


    }

    private void removeSerial( String serial ) {
        // try to shutdown the device
        shutdownDevice(serial);
        // remove device
        try {
            serialDevices.remove( serial );
            serialParsers.remove( serial );
        }
        catch ( Exception exception ) {
            log.add( "Exception on removing serial: " + exception.getClass().getName() + ": " + exception.getMessage() );
            return;
        }
        log.add( "Disconnected device \"" + serial + "\"" );
    }
    private void shutdownDevice( String serial ) {
        UsbSerialDevice device;
        try {
            device = serialDevices.get(serial);
        }
        catch ( Exception exception ) {
            log.add( "Exception on device get: " + exception.getClass().getName() + ": " + exception.getMessage() );
            return;
        }

        try {
            device.close();
        }
        catch ( Exception exception ) {
            log.add( "Exception on device closure: " + exception.getClass().getName() + ": " + exception.getMessage() );
        }

    }

    private void createSerial( final String serial, UsbDevice usbDevice ) {

        UsbDeviceConnection connection;
        UsbSerialDevice serialDevice;
        // initialize connection to device
        try {
            connection = usbManager.openDevice( usbDevice );
        }
        catch ( Exception exception ) {
            log.add( "Exception on usb connection creation: " + exception.getClass().getName() + ": " + exception.getMessage() );
            return;
        }

        // initialize serial contract
        try {
            serialDevice = UsbSerialDevice.createUsbSerialDevice( usbDevice, connection );
        }
        catch ( Exception exception ) {
            log.add( "Exception on serial creation: " + exception.getClass().getName() + ": " + exception.getMessage() );
            return;
        }

        // open serial connection
        try {
            if(!serialDevice.open()) {
                log.add( "Could not open device " + serial );
                return;
            }
        }
        catch ( Exception exception ) {
            log.add( "Exception on open query: " + exception.getClass().getName() + ": " + exception.getMessage() );
            return;
        }

        // default settings and data callback
        try {
            serialDevice.setBaudRate( 115200 );
            serialDevice.setDataBits( UsbSerialInterface.DATA_BITS_8);
            serialDevice.setStopBits(UsbSerialInterface.STOP_BITS_1);
            serialDevice.setParity(UsbSerialInterface.PARITY_NONE);
            serialDevice.setFlowControl(UsbSerialInterface.FLOW_CONTROL_OFF);
            serialDevice.read( new UsbSerialInterface.UsbReadCallback() {
                @Override
                public void onReceivedData( byte[] bytes ) {
                    try {
                        if ( !serialParsers.containsKey( serial ) ) {
                            serialParsers.put( serial, new DecawaveParser( serial, 4092 ) );
                        }
                        serialParsers.get( serial ).put( bytes );
                    }
                    catch ( Exception exception ) {
                        log.add( "Exception on byte parser: " + exception.getClass().getName() + ": " + exception.getMessage() );
                    }
                }
            } );
        }
        catch ( Exception exception ) {
            log.add( "Exception on setup: " + exception.getClass().getName() + ": " + exception.getMessage() );
            try {
                serialDevice.close();
            }
            catch ( Exception closingException ) {
                log.add( "Exception on shutdown: " + closingException.getClass().getName() + ": " + closingException.getMessage() );
            }
            return;
        }

        // add serial connection to list
        try {
            serialDevices.put( serial, serialDevice );
        }
        catch ( Exception exception ) {
            log.add( "Exception on serial adding: " + exception.getClass().getName() + ": " + exception.getMessage() );
        }
        if ( !serialParsers.containsKey( serial ) ) {
            serialParsers.put( serial, new DecawaveParser( serial, 4092 ) );
        }

    }

    /**
     * Get first message from log pool
     * @return
     */
    public String popLogMessage() {
        return log.pop();
    }

    /**
     * Check if there is a message in log pool
     * @return true if has message
     */
    public boolean hasLogMessage() { return log.size() > 0; }

    /**
     * Get the active device count
     * @return number of devices
     */
    public int activeDeviceCount() {
        return serialDevices.size();
    }

    /**
     * Get every device serial number
     * @return an array of serial device serial numbers
     */
    public String[] deviceSerials() {
        String[] serials = new String[serialParsers.size()];
        int index = 0;
        for(String serial : serialParsers.keySet())
            serials[index++] = serial;
        return serials;
    }

    /**
     * Get a serial device parser
     * @param serial The serial number of device
     * @return Serial parser
     */
    public DecawaveParser getParser( String serial ) {
        return serialParsers.containsKey( serial ) ? serialParsers.get(serial) : null;
    }

}
