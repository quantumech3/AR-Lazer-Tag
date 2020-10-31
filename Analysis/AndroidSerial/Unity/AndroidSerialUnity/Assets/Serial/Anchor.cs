/*
 * Created by Mika Taskinen on 30.1.2017
 * Copyright: University of Turku & Mika Taskinen
 */

using System;

namespace Marin2.Decawave.Unity3d
{
    public class Anchor
    {

        /// <summary>
        /// Creates Anchor object, usable only by library
        /// </summary>
        /// <param name="receiver">"Parent" receiver</param>
        /// <param name="id"></param>
        internal Anchor( Receiver receiver, int id )
        {
            Receiver = receiver;
            Id = id;
        }

        public delegate void DisappearedEventHandler( Anchor anchor );
        /// <summary>
        /// This event lauches when anchor is invalid after a time
        /// </summary>
        public event DisappearedEventHandler Disappeared;

        public delegate void UpdatedEventHandler( Anchor anchor, int newDistance, int oldDistance );
        /// <summary>
        /// This event lauches when data has been updated
        /// </summary>
        public event UpdatedEventHandler Updated;

        protected void OnUpdated( int newDistance, int oldDistance)
        {
            UpdatedEventHandler handler = Updated;
            if ( handler != null )
                handler( this, newDistance, oldDistance );
        }

        protected void OnDisappeared()
        {
            DisappearedEventHandler handler = Disappeared;
            if ( handler != null )
                handler( this );
        }

        /// <summary>
        /// Get the timestamp of last update
        /// </summary>
        public DateTime Timestamp
        {
            get;
            internal set;
        }

        /// <summary>
        /// Get the distance in millimeters
        /// </summary>
        public int Distance
        {
            get;
            private set;
        }
        /// <summary>
        /// Get the id of the anchor
        /// </summary>
        public int Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the parent receiver the anchor has been detected from
        /// </summary>
        public Receiver Receiver
        {
            get;
            private set;
        }

        /// <summary>
        /// Sets new distance and timestamp
        /// </summary>
        /// <param name="timestamp">The given timestamp</param>
        /// <param name="distance">The given distance</param>
        internal void Set( DateTime timestamp, int distance )
        {
            Timestamp = timestamp;
            int oldDistance = Distance;
            Distance = distance;
            if ( oldDistance != distance )
                OnUpdated( distance, oldDistance );
        }

        /// <summary>
        /// Internal workings of back-and-forth event system
        /// </summary>
        internal void Remove()
        {
            OnDisappeared();
        }
    }
}