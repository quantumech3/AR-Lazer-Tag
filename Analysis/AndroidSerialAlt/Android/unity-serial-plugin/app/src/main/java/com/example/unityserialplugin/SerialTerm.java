package com.example.unityserialplugin;

import android.app.Activity;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.hardware.usb.UsbDeviceConnection;
import android.hardware.usb.UsbManager;
import android.util.Log;

import com.hoho.android.usbserial.driver.CdcAcmSerialDriver;
import com.hoho.android.usbserial.driver.ProbeTable;
import com.hoho.android.usbserial.driver.UsbSerialDriver;
import com.hoho.android.usbserial.driver.UsbSerialPort;
import com.hoho.android.usbserial.driver.UsbSerialProber;
import com.hoho.android.usbserial.util.SerialInputOutputManager;

import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.util.List;
import java.util.concurrent.Executors;

import com.unity3d.player.UnityPlayer;

public class SerialTerm implements SerialInputOutputManager.Listener {
    private String gameObject;

    public SerialTerm(Context unityContext, String gameObject, String vendorID, String productID) {
        this.gameObject = gameObject;
        Thread thread = new Thread(() -> {
            openSerial(unityContext, vendorID, productID);
        });
        thread.start();
    }
    //Async function for initializing serial, has callbacks for different errors
    private void openSerial(Context unityContext, String vendorID, String productID) {
        //Get USB manager
        UsbManager manager = (UsbManager) unityContext.getSystemService(unityContext.USB_SERVICE);

        //Create probe table
        ProbeTable customTable = new ProbeTable();
        customTable.addProduct(Integer.parseInt(vendorID, 16), Integer.parseInt(productID, 16), CdcAcmSerialDriver.class);
        UsbSerialProber prober = new UsbSerialProber(customTable);

        //Get list of devices matching probe table
        List<UsbSerialDriver> availableDrivers = prober.findAllDrivers(manager);

        //Check if there is a device
        if (availableDrivers.isEmpty()) {
            UnityPlayer.UnitySendMessage(gameObject, "OnDeviceFailure", "");
            return;
        }
        UnityPlayer.UnitySendMessage(gameObject, "OnDeviceSuccess", "");

        //Get driver for first connected USB device (shouldn't be more than one), request permissions if needed
        UsbSerialDriver driver = availableDrivers.get(0);
        UsbDeviceConnection connection = manager.openDevice(driver.getDevice());
        if (connection == null) {
            manager.requestPermission(driver.getDevice(), PendingIntent.getBroadcast(unityContext, 0, new Intent("USB Permissions"), 0));
            //Retry every 1 second for 10 seconds
            for (int i = 0; i < 10; i++) {
                connection = manager.openDevice(driver.getDevice());
                if (connection == null) {
                    try {
                        Thread.sleep(1000);
                    } catch (InterruptedException e) {
                        UnityPlayer.UnitySendMessage(gameObject, "OnInitException", e.toString());
                    }
                } else {
                    //We have permissions, breakout of this loop
                    break;
                }
            }
            //If the connection is still null after 10 seconds, let Unity know and give up
            if (connection == null) {
                UnityPlayer.UnitySendMessage(gameObject, "OnPermissionFailure", "");
                return;
            }
        }
        //If we've reached this point, we have permissions. Let Unity know.
        UnityPlayer.UnitySendMessage(gameObject, "OnPermissionSuccess", "");

        //Open the serial port for the first device (Again, should only be one)
        UsbSerialPort port = driver.getPorts().get(0);
        try {
            port.open(connection);
            //This should be passed in from Unity in the final interface
            port.setParameters(115200, 8, UsbSerialPort.STOPBITS_1, UsbSerialPort.PARITY_NONE);
        } catch (IOException e) {
            UnityPlayer.UnitySendMessage(gameObject, "OnInitException", e.toString());
        }

        //Setup Java-side newData callback
        SerialInputOutputManager usbIoManager = new SerialInputOutputManager(port, this);
        Executors.newSingleThreadExecutor().submit(usbIoManager);

        //Let Unity know that the serial port has been initialized
        UnityPlayer.UnitySendMessage(gameObject, "OnConnection", "");
    }
    @Override
    //Callback to Unity when new serial data is received
    public void onNewData(byte[] data) {
        try {
            UnityPlayer.UnitySendMessage(gameObject,"OnData",new String(data, "utf-8"));
        } catch (UnsupportedEncodingException e) {
            UnityPlayer.UnitySendMessage(gameObject, "OnDataException", e.toString());
        }
    }

    @Override
    public void onRunError(Exception e) {
        UnityPlayer.UnitySendMessage(gameObject, "OnRunException", e.toString());
    }
}

