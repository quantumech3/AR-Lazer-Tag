using System;
using UnityEngine;

namespace Marin2.Decawave.Unity3d
{
    /// <summary>
    /// TrilinearCalculationResult gives two possible resulting positions when the source data is comprised from 3 anchors
    /// </summary>
    public struct TrilinearCalculationResult : ICalculationResult
    {

        internal TrilinearCalculationResult( AnchorData anchorData1, AnchorData anchorData2, AnchorData anchorData3 )
        {
            Vector3 result1, result2;
            // Calculation is done by separate calculation function in a separate static class
            if ( TrilinearCalculations.ThreePoint( anchorData1.Position, anchorData2.Position, anchorData3.Position, anchorData1.Distance, anchorData2.Distance, anchorData3.Distance, out result1, out result2 ) )
            {
                Result1 = result1;
                Result2 = result2;
                CalculationErrored = false;
            }
            else
            {
                Result1 = Result2 = new Vector2();
                CalculationErrored = true;
            }
        }

        /// <summary>
        /// Get the error status of calculation
        /// </summary>
        public bool CalculationErrored
        {
            get;
            internal set;
        }
        /// <summary>
        /// Get the first possible result
        /// </summary>
        public Vector3 Result1
        {
            get;
            internal set;
        }
        /// <summary>
        /// Get the second possible result
        /// </summary>
        public Vector2 Result2
        {
            get;
            internal set;
        }
        /// <summary>
        /// Get the type of result
        /// </summary>
        public CalculationResultType ResultType
        {
            get
            {
                return CalculationResultType.Trilinear;
            }
        }
    }
}