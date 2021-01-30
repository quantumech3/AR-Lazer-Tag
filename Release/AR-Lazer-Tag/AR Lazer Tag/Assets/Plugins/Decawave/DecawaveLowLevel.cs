using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System;
using System.Threading;

namespace Decawave
{
    namespace LowLevel
    {
        /// <summary>
        /// Contains methods that provide a direct serial interface on Android
        /// </summary>
        public class Serial
        {
            private static bool javaSideIsInitialized = false;
            private static bool hasUsbPermissions = false;
            private static bool portIsOpen = false;
            private static AndroidJavaClass javaSerialInterface;


            
            /// <summary>
            /// Vendor ID of all Decawaves (probably)
            /// </summary>
            public const int VENDOR_ID = 0x1366;

            /// <summary>
            /// Product ID of all Decawaves (probably)
            /// </summary>
            public const int PRODUCT_ID = 0x0105;

            private class UnityCallback : AndroidJavaProxy
            {
                public UnityCallback() : base("com.arlazertag.decawavelowlevel.UnityCallback") { }

                public void onException(AndroidJavaObject exception)
                {
                    throw new LowLevelException(exception.Call<string>("toString"));
                }
            }

            private static AndroidJavaObject getUnityContext()
            {
                AndroidJavaObject unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject unityContext = unityActivity.Call<AndroidJavaObject>("getApplicationContext");

                return unityContext;
            }

            /// <summary>
            /// Returns true if the library has been initialized else false
            /// </summary>
            public static bool JavaSideIsInitialized()
            {
                return javaSideIsInitialized;
            }

            /// <summary>
            /// Attempts to initialize the internals of the Java component of the DecawaveLowLevel interface.
            /// </summary>
            /// <returns><code>true</code> on success, else <code>false</code></returns>
            public static void InitializeJavaSide()
            {
                javaSerialInterface = new AndroidJavaClass("com.arlazertag.decawavelowlevel.SerialInterface");

                // Inject c# callbacks
                javaSerialInterface.CallStatic("setCallbackListener", new UnityCallback());

                // Attempt to initialize Java side
                if (!javaSideIsInitialized)
                    javaSideIsInitialized = javaSerialInterface.CallStatic<bool>("initializeJavaSide", getUnityContext(), VENDOR_ID, PRODUCT_ID);
            }

            /// <returns><code>true</code> if permissions have already been granted, else <code>false</code></returns>
            public static bool HasUSBPermission()
            {
                return hasUsbPermissions;
            }

            /// <summary>
            /// Attempts to request USB permissions from Android OS. Throws exception if already initialized
            /// </summary>
            /// <returns><code>true</code> if successfully initialized, else false</returns>
            /// <exception cref="Decawave.LowLevel.LowLevelException"></exception>
            public static bool RequestUSBPermission()
            {
                if (hasUsbPermissions)
                    throw new LowLevelException("USB permissions already obtained");

                hasUsbPermissions = javaSerialInterface.CallStatic<bool>("requestUSBPermissions");

                return hasUsbPermissions;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns><code>true</code> if port has already been opened, else <code>false</code></returns>
            public static bool PortIsOpen()
            {
                return portIsOpen;
            }

            /// <summary>
            /// Attempts to open the port on the phones default COM port. Throws exception if already opened
            /// </summary>
            /// <returns><code>true</code> if port is successfully opened, else <code>false</code></returns>
            /// <exception cref="Decawave.LowLevel.LowLevelException"></exception>
            public static bool OpenPort(int baudRate)
            {
                if (portIsOpen)
                    throw new LowLevelException("Port is already open. Cannot open port twice");

                portIsOpen = javaSerialInterface.CallStatic<bool>("openPort", baudRate);

                return portIsOpen;
            }

            /// <summary>
            /// Writes data to serial if possible, else throws DecawaveLowLevelException
            /// </summary>
            /// <param name="data">Data to be written to serial</param>
            /// <param name="timeout">Timeout to write data before failure in ms</param>
            public static void Write(string data, int timeout)
            {
                if (!javaSerialInterface.CallStatic<bool>("write", Encoding.UTF8.GetBytes(data), timeout))
                    throw new LowLevelException("Writing failed");
            }

            /// <summary>
            /// Reads n bytes of data and returns a byte[] containing that data
            /// </summary>
            /// <param name="n">Number of bytes to be read</param>
            /// <param name="timeout">timeout in millis for each packet of data</param>
            /// <returns>Return the following (data buffer of size n, remainder of data not captured)</returns>
            public static (byte[], byte[]) Read(int n, int timeout)
            {
                byte[] data = new byte[n];
                int ptr = 0;

                byte[] remainder = new byte[0];
                byte[] temp = new byte[0];
                do
                {
                    temp = javaSerialInterface.CallStatic<byte[]>("read", timeout);

                    int _ptr = ptr;
                    for (; ptr < Mathf.Min(_ptr + temp.Length, data.Length); ptr++)
                    {
                        data[ptr] = temp[ptr - _ptr];
                    }

                    remainder = temp.Skip(ptr - _ptr).ToArray();
                }
                while (ptr < data.Length && temp.Length > 0);
                    
                return (data, remainder);
            }
            
            /// <summary>
            /// Reads the buffer multiple times to flush out any residual data (from previous runs of the app)
            /// </summary>
            /// <param name="timeout">Timeout in millis for each packet of data</param>
            /// <returns></returns>
            public static void Flush(int timeout) 
            {
                while (javaSerialInterface.CallStatic<byte[]>("read", timeout).Length != 0) ;
            }
        }
        /// <summary>
        /// Thrown on failure of Decawave low level interface
        /// </summary>
        public class LowLevelException : Decawave.Common.DecawaveException
        {
            public LowLevelException(string message) : base(message)
            {

            }
        }

        /// <summary>
        /// Contains static methods to initialize and get distances from RF anchors
        /// </summary>
        public class Interface
        {
            private static bool isInitialized = false;
            public const int BAUDRATE = 115200;
            public const string DWM_LOC_GET = "\x0c\x00";

            /// <summary>
            /// Synchronously executes the Decawave initialization protocol and throws a DecawaveLowLevelException on failure
            /// </summary>
            public static void Initialize()
            {
                if (isInitialized)
                    throw new LowLevelException("Low level interface cannot be initialized twice");

                isInitialized = true;

                Serial.InitializeJavaSide();
                isInitialized = isInitialized && Serial.JavaSideIsInitialized();

                Serial.RequestUSBPermission();
                isInitialized = isInitialized && Serial.HasUSBPermission();

                Serial.OpenPort(BAUDRATE);
                isInitialized = isInitialized && Serial.PortIsOpen();

                if (!isInitialized)
                    throw new LowLevelException("Initialization failed");

                Serial.Flush(1);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns>All raw data from dwm_loc_get call from Decawave</returns>
            private static byte[] getRawData()
            {
                (byte[] firstPart, byte[] remainder) = Serial.Read(20, 10);
                byte[] lastPart = Serial.Read(firstPart[firstPart.Length - 1] - remainder.Length, 1).Item1; // WTF WHY ARE TUPLES SO GROSS IN C#s

                return firstPart.Concat(remainder).Concat(lastPart).ToArray();
            }

            /// <summary>
            /// Read only attribute that returns a list of AnchorData objects containing the ID of each anchor and each associated distance if possible, else throws DecawaveLowLevelException.
            /// Returns empty array if data retrieved from Decawave is corrupt or incomplete
            /// </summary>
            /// <exception cref="LowLevelException"></exception>
            public static Common.AnchorData[] anchors
            {
                get
                {
                    try
                    {
                        // Invoke dwm_loc_get command
                        Serial.Write(DWM_LOC_GET, 0);

                        Thread.Sleep(10);

                        // Get data
                        byte[] rawData = getRawData();
                        int nDistancesRecorded = rawData[20];

                        Common.AnchorData[] anchors = new Common.AnchorData[nDistancesRecorded];

                        for (int i = 0; i < nDistancesRecorded; i++)
                        {
                            anchors[i].id = BitConverter.ToUInt16(rawData, 20 * (i + 1) + 1);
                            anchors[i].distance = ((double) BitConverter.ToUInt32(rawData, 20 * (i + 1) + 3)) / 1000f;
                        }

                        return anchors;
                    }
                    catch(IndexOutOfRangeException e)
                    {
                        Debug.Log("[LowLevelInterface] Dropped garbage distances");
                        Serial.Flush(1);
                        return new Common.AnchorData[0];
                    }
                }
            }
        }
    }
}
