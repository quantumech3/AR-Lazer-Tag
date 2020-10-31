/*
 * Created by Mika Taskinen on 30.1.2017
 * Copyright: University of Turku & Mika Taskinen
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Marin2.Decawave.Unity3d
{
    public class Receiver : IEnumerable<Anchor>
    {
        /// <summary>
        /// Event method template for disconnect event
        /// </summary>
        /// <param name="receiver"></param>
        public delegate void DisconnectedEventHandler( Receiver receiver);
        /// <summary>
        /// This event lauches when receiver is disconnected
        /// </summary>
        public event DisconnectedEventHandler Disconnected;

        public delegate void AnchorAppearedEventHandler( Receiver receiver, Anchor anchor );
        /// <summary>
        /// This event lauches whenever a new anchor is detected
        /// </summary>
        public event AnchorAppearedEventHandler AnchorAppeared;

        // Storage of anchors detected by this receiver
        private Dictionary<int, Anchor> anchors = new Dictionary<int, Anchor>();
        // calculation system
        private LocationEngine locator;

        protected void OnDisconnected()
        {
            DisconnectedEventHandler handler = Disconnected;
            if ( handler != null )
                handler( this );
        }

        protected void OnAnchorAppeared( Anchor anchor )
        {
            AnchorAppearedEventHandler handler = AnchorAppeared;
            if ( handler != null )
                handler( this, anchor );
        }
        
        internal Receiver( DecawaveManager manager, string serial )
        {
            Manager = manager;
            Serial = serial;
            locator = new LocationEngine();
        }

        /// <summary>
        /// Get the serial number of the receiver
        /// </summary>
        public string Serial
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the manager that handles the receiver
        /// </summary>
        public DecawaveManager Manager
        {
            get;
            private set;
        }
        /// <summary>
        /// Get the information of the receiver status
        /// </summary>
        public bool IsDisconnected
        {
            get;
            private set;
        }

        /// <summary>
        /// Get or set the calculation level
        /// </summary>
        public CalculationLevel CalculationLevel
        {
            get
            {
                return locator.CalculationLevel;
            }
            set
            {
                locator.CalculationLevel = value;
            }
        }

        /// <summary>
        /// Get or set the update method (every anchor update or every receiver update or not at all)
        /// </summary>
        public UpdateLevel UpdateLevel
        {
            get;
            set;
        }
        /// <summary>
        /// Latest calculation result
        /// </summary>
        public ICalculationResult CalculationResult
        {
            get;
            private set;
        }

        /// <summary>
        /// Internal manager-to-receiver communication to fire an event
        /// </summary>
        internal void Disconnect()
        {
            OnDisconnected();
            IsDisconnected = true;
        }

        /// <summary>
        /// Calculates the current position
        /// Can always be used but best used when there is no 
        /// </summary>
        public void Calculate()
        {
            CalculationResult = locator.Calculate();
        }

        /// <summary>
        /// Set the physical position of anchor
        /// </summary>
        /// <param name="id">The id of the anchor</param>
        /// <param name="position">The position of the anchor</param>
        public void SetAnchorPosition( int id, Vector3 position )
        {
            locator.SetAnchorPosition( id, position );
        }

        /// <summary>
        /// Remove the physical position of the anchor
        /// </summary>
        /// <param name="id">The id of the anchor</param>
        public void UnsetAnchorPosition( int id )
        {
            locator.UnsetAnchorPosition( id );
        }

        /// <summary>
        /// Automatic update function called by manager
        /// </summary>
        /// <param name="parser">Receiver reference object from the android system</param>
        internal void Update( AndroidJavaObject parser )
        {
            // This date is used for removing anchors that are not up-to-date
            DateTime now = DateTime.Now;
            // This loop is done as long as the device has packets
            while ( parser.Call<bool>( "hasPacket" ) )
            {
                // Getting the packet
                AndroidJavaObject packet = parser.Call<AndroidJavaObject>( "popPacket" );
                // Taking the information
                int id = packet.Call<int>( "getAnchorId" );
                int distance = packet.Call<int>( "getDistanceInMillimeters" );

                // Anchor creation/get
                Anchor anchor;
                if ( anchors.ContainsKey( id ) )
                {
                    anchor = anchors[id];
                }
                else
                {
                    anchor = new Anchor( this, id );
                    anchors.Add( id, anchor );
                    // On new anchor -> inform
                    OnAnchorAppeared( anchor );
                }
                // Setting data
                anchor.Set( now, distance );
                locator.SetAnchorDistance( id, distance * .001f );

                // if superautoupdate => update
                if ( UpdateLevel == UpdateLevel.OnValueUpdate )
                {
                    CalculationResult = locator.Calculate();
                }

            }

            // Remove all anchors that are invalid (haven't updated in a "long" time)
            HashSet<int> removables = new HashSet<int>();
            foreach ( int anchorId in anchors.Keys )
            {
                if ( ( now - anchors[anchorId].Timestamp ) > Manager.DiscardInterval )
                {
                    anchors[anchorId].Remove();
                    anchors.Remove( anchorId );
                    locator.UnsetAnchorDistance( anchorId );
                }
            }

            // if autoupdate => update
            if ( UpdateLevel == UpdateLevel.OnUpdate )
            {
                CalculationResult = locator.Calculate();
            }
        }

        /// <summary>
        /// Get the enumeration of anchors
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Anchor> GetEnumerator()
        {
            return ( (IEnumerable<Anchor>)anchors ).GetEnumerator();
        }

        /// <summary>
        /// Get the enumeration of anchors
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return anchors.GetEnumerator();
        }

        /// <summary>
        /// Get an anchor by id
        /// </summary>
        /// <param name="id">The id of anchor</param>
        /// <returns>An anchor</returns>
        public Anchor this[int id]
        {
            get
            {
                return anchors[id];
            }
        }

        /// <summary>
        /// Get every anchor id available
        /// </summary>
        public IEnumerable<int> Ids
        {
            get
            {
                return anchors.Keys;
            }
        }

        /// <summary>
        /// Get every anchor available
        /// </summary>
        public IEnumerable<Anchor> Anchors
        {
            get
            {
                return anchors.Values;
            }
        }

        /// <summary>
        /// Get the count of anchors
        /// </summary>
        public int AnchorCount
        {
            get
            {
                return anchors.Count;
            }
        }
    }
}