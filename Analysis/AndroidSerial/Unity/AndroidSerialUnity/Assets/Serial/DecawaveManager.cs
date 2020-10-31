/*
 * Created by Mika Taskinen on 30.1.2017
 * Copyright: University of Turku & Mika Taskinen
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Marin2.Decawave.Unity3d
{
    public class DecawaveManager : IEnumerable<KeyValuePair<string, Receiver>>
    {
        /*
         * An event system for detecting new receivers
         */
        public delegate void ReceiverAppearedEventHandler( DecawaveManager manager, Receiver receiver );
        public event ReceiverAppearedEventHandler ReceiverAppeared;

        private static DecawaveManager instance;
        private AndroidJavaObject decawaveManager;
        private Dictionary<string, Receiver> receivers = new Dictionary<string, Receiver>();

        /// <summary>
        /// Get the manager instance
        /// </summary>
        public static DecawaveManager Instance
        {
            get
            {
                if ( instance == null )
                    instance = new DecawaveManager();
                return instance;
            }
        }

        /// <summary>
        /// Get or set the interval for removable anchors (i.e. how long it takes for the anchor message to become obsolete). Default is 5 seconds.
        /// </summary>
        public TimeSpan DiscardInterval
        {
            get;
            set;
        }

        /// <summary>
        /// DecawaveManager constructor can only be used locally
        /// </summary>
        private DecawaveManager()
        {
            // Init anchor cooldown to 5 seconds
            DiscardInterval = TimeSpan.FromSeconds( 5 );
            // Initialize android side classes in work
            AndroidJavaClass unityPlayer = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" );
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>( "currentActivity" );
            AndroidJavaClass decawaveManagerStatic = new AndroidJavaClass( "com.marin2.decawave.unity3dplugin.DecawaveManager" );
            decawaveManager = decawaveManagerStatic.CallStatic<AndroidJavaObject>( "getInstance", unityActivity );
        }

        protected void OnBeaconAppeared( Receiver receiver )
        {
            ReceiverAppearedEventHandler handler = ReceiverAppeared;
            if ( handler != null )
                handler( this, receiver );
        }

        /// <summary>
        /// Update anchor states
        /// </summary>
        public void Update()
        {
            // Find all current serials
            HashSet<string> serials = new HashSet<string>( decawaveManager.Call<string[]>( "deviceSerials" ) );

            // Remove all not found in active
            foreach ( string serial in receivers.Keys )
            {
                if ( !serials.Contains( serial ) )
                {
                    receivers[serial].Disconnect();
                    receivers.Remove( serial );
                }
            }

            // Add all new and handle data further (in receivers and anchors)
            foreach ( string serial in decawaveManager.Call<string[]>( "deviceSerials" ) )
            {
                AndroidJavaObject parser = decawaveManager.Call<AndroidJavaObject>( "getParser", serial );

                Receiver receiver;
                if ( receivers.ContainsKey( serial ) )
                {
                    receiver = receivers[serial];
                }
                else
                {
                    receiver = new Receiver(this, serial);
                    receivers.Add( serial, receiver );
                    OnBeaconAppeared( receiver );
                }

                receiver.Update( parser );

            }
        }

        /// <summary>
        /// Read a log message
        /// </summary>
        /// <returns>message as string or null</returns>
        public string ReadLogMessage()
        {
            return decawaveManager.Call<bool>("hasLogMessage") ? decawaveManager.Call<string>( "popLogMessage" ) : null;
        }

		/// <summary>
		/// Enumerate all receivers in system
		/// </summary>
		/// <returns>Enumerator to enumerate all receivers</returns>
        public IEnumerator<KeyValuePair<string, Receiver>> GetEnumerator()
        {
            return ( (IEnumerable<KeyValuePair<string, Receiver>>)receivers ).GetEnumerator();
        }

		/// <summary>
		/// Enumerate all receivers in system
		/// </summary>
		/// <returns>Enumerator to enumerate all receivers</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return receivers.GetEnumerator();
        }

        /// <summary>
        /// Get a receiver by serial number
        /// </summary>
        /// <param name="serial">Serial number of receiver</param>
        /// <returns>A receiver</returns>
        public Receiver this[string serial]
        {
            get
            {
                return receivers.ContainsKey( serial ) ? receivers[serial] : null;
            }
        }
        /// <summary>
        /// Get all serials of all receivers
        /// </summary>
        public IEnumerable<string> Serials
        {
            get
            {
                return receivers.Keys;
            }
        }
        /// <summary>
        /// Get all receivers
        /// </summary>
        public IEnumerable<Receiver> Receivers
        {
            get
            {
                return receivers.Values;
            }
        }
        /// <summary>
        /// Get the count of current receivers
        /// </summary>
        public int ReceiverCount
        {
            get
            {
                return receivers.Count;
            }
        }


    }
}
