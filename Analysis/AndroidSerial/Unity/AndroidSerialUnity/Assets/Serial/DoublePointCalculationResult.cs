using System;
using UnityEngine;

namespace Marin2.Decawave.Unity3d
{
    /// <summary>
    /// The result where two anchors and distances are available
    /// </summary>
    public struct DoublePointCalculationResult : ICalculationResult
    {
        /// <summary>
        /// Result creation work inside library
        /// </summary>
        /// <param name="anchorData1">First anchor data</param>
        /// <param name="anchorData2">Second anchor data</param>
        internal DoublePointCalculationResult( AnchorData anchorData1, AnchorData anchorData2 )
        {
            // Get the distance between anchors
            float c = ( anchorData1.Position - anchorData2.Position ).magnitude;

            // A vector between anchors is considered normal of the resulting plane
            Normal = anchorData1.Position - anchorData2.Position;
            Normal.Normalize();

            // half of the perimeter
            float s = .5f * ( anchorData1.Distance + anchorData2.Distance + c );

            // using herron's formula, we gain Area and therefor radius
            Radius = 2 * ( (float)Math.Sqrt( s * ( s - anchorData1.Distance ) * ( s - anchorData2.Distance ) * ( s - c ) ) ) / anchorData2.Distance;

            // The position is inside the circle
            Position = anchorData2.Position + Normal * (float)Math.Sqrt( anchorData2.Distance * anchorData2.Distance - Radius * Radius );
        }

        /// <summary>
        /// Normal direction of the resulting plane where the result circle is
        /// </summary>
        public Vector3 Normal
        {
            get;
            private set;
        }
        /// <summary>
        /// Origin position of the circle
        /// </summary>
        public Vector3 Position
        {
            get;
            private set;
        }
        /// <summary>
        /// The distance of the point from the origin
        /// </summary>
        public float Radius
        {
            get;
            private set;
        }
        /// <summary>
        /// Get the type of result
        /// </summary>
        public CalculationResultType ResultType
        {
            get
            {
                return CalculationResultType.Double;
            }
        }
    }
}