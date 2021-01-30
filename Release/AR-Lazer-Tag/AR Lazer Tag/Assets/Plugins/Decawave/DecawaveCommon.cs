using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Decawave
{
    namespace Common
    {
        /// <summary>
        /// Contains distances between TAG and RF anchor with ID of AnchorData.id.
        /// </summary>
        public struct AnchorData
        {
            private int _id;
            private double _distance;

            /// <summary>
            /// id of RF anchor measured
            /// </summary>
            public int id
            {
                get
                {
                    return _id;
                }

                set
                {
                    this._id = value;
                }
            }

            /// <summary>
            /// Distance from rf anchor in meters
            /// </summary>
            public double distance
            {
                get
                {
                    return _distance;
                }

                set
                {
                    this._distance = value;
                }
            }
        }

        public class DecawaveException : UnityException
        {
            public DecawaveException(string message) : base(message)
            {
            }

            /// <summary>
            /// Logs this exception with a "DecawaveException" prefix as a Unity error
            /// </summary>
            public void LogAsError()
            {
                Debug.LogError("DecawaveException " + Message);
            }
        }
    }
}

