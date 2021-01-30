
using System;

namespace Marin2.Decawave.Unity3d
{
    /// <summary>
    /// Null result is for failed situations where no actual point can be calculated
    /// </summary>
    public struct NullCalculationResult : ICalculationResult
    {
        /// <summary>
        /// Get the type of result
        /// </summary>
        public CalculationResultType ResultType
        {
            get
            {
                return CalculationResultType.Null;
            }
        }
    }
}