using System;
using UnityEngine;

namespace Marin2.Decawave.Unity3d
{
    /// <summary>
    /// Calculation result presenting only one point, so the result is a 3d-ball
    /// </summary>
    public struct SinglePointCalculationResult : ICalculationResult
    {

        internal SinglePointCalculationResult( AnchorData anchorData )
        {
            Distance = anchorData.Distance;
            Position = anchorData.Position;
        }
        /// <summary>
        /// Get the distance (or radius)
        /// </summary>
        public float Distance
        {
            get;
            private set;
        }
        /// <summary>
        /// Origin point
        /// </summary>
        public Vector3 Position
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
                return CalculationResultType.Single;
            }
        }
    }
}