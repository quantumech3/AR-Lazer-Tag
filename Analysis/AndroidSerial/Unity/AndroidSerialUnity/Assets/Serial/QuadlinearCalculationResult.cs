using Marin2.Decawave.Unity3d;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Marin2.Decawave.Unity3d
{
    /// <summary>
    /// A calculation result of 4 or more points
    /// </summary>
    public struct QuadlinearCalculationResult : ICalculationResult
    {
        /// <summary>
        /// Get the level of calculation used for this result
        /// </summary>
        public CalculationLevel CalculationLevel
        {
            get;
            private set;
        }
        /// <summary>
        /// The amount of source points used for this result (4 or more)
        /// </summary>
        public int CalculationOptions
        {
            get;
            private set;
        }
        /// <summary>
        /// Get the error status of the calculation
        /// </summary>
        public bool Errored
        {
            get;
            private set;
        }
        /// <summary>
        /// Get the position of the calculation
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
                return CalculationResultType.Quadlinear;
            }
        }

        internal static ICalculationResult Exact( AnchorData anchorData1, AnchorData anchorData2, AnchorData anchorData3, AnchorData anchorData4, CalculationLevel calculationLevel )
        {
            // The exact case does not NEED the calculation level as there are 4 points and calculation requires 4 points.
            // The algorithm works by getting the result from 3 points and then picking the correct one from them by using the 4th point.
            Vector3 res1, res2;
            if ( TrilinearCalculations.ThreePoint( anchorData1.Position, anchorData2.Position, anchorData3.Position, anchorData1.Distance, anchorData2.Distance, anchorData3.Distance, out res1, out res2 ) )
            {
                float d1 = ( anchorData4.Position - res1 ).sqrMagnitude;
                float d2 = ( anchorData4.Position - res2 ).sqrMagnitude;
                if ( Math.Abs( d1 - d2 ) < .000001f )
                {
                    return new TrilinearCalculationResult
                    {
                        Result1 = res1,
                        Result2 = res2
                    };
                }

                if ( Math.Abs( d1 - anchorData4.Distance ) < Math.Abs( d2 - anchorData4.Distance ) )
                {
                    return new QuadlinearCalculationResult
                    {
                        Position = res1,
                        CalculationLevel = calculationLevel,
                        CalculationOptions = 4
                    };
                }
                else
                {
                    return new QuadlinearCalculationResult
                    {
                        Position = res2,
                        CalculationLevel = calculationLevel,
                        CalculationOptions = 4
                    };
                }
            }
            else
            {
                return new QuadlinearCalculationResult
                {
                    Errored = true,
                    CalculationLevel = calculationLevel,
                    CalculationOptions = 4
                };
            }
        }

        internal static ICalculationResult Sparse( List<AnchorData> realData, CalculationLevel calculationLevel )
        {
            // This calculation happens when there are 5 or more points as it brings out options that can be handled in different ways
            switch ( calculationLevel )
            {
                case CalculationLevel.Low:
                    return LowCalculationResult( realData );
                case CalculationLevel.Medium:
                    return MediumCalculationResult( realData ); // <- Not implemented directly, does low level calculation
                default:
                    return HighCalculationResult( realData );
            }
        }

        private static ICalculationResult HighCalculationResult( List<AnchorData> realData )
        {
            // High calculation level works by getting all the options and calculating the average result.
            List<Vector3> results = new List<Vector3>();
            AnchorData[] anchorData = new AnchorData[4];
            Vector3 res1, res2;
            float d1, d2;
            for ( int i = 0; i < realData.Count; i++ )
            {
                anchorData[0] = realData[i];
                for ( int j = i + 1; j < realData.Count; j++ )
                {
                    anchorData[1] = realData[j];
                    for ( int k = j + 1; k < realData.Count; k++ )
                    {
                        anchorData[2] = realData[k];
                        for ( int l = k + 1; l < realData.Count; l++ )
                        {
                            anchorData[3] = realData[l];

                            if ( TrilinearCalculations.ThreePoint( anchorData[0].Position, anchorData[1].Position, anchorData[2].Position, anchorData[0].Distance, anchorData[1].Distance, anchorData[2].Distance, out res1, out res2 ) )
                            {
                                d1 = ( anchorData[3].Position - res1 ).sqrMagnitude;
                                d2 = ( anchorData[3].Position - res2 ).sqrMagnitude;
                                if ( Math.Abs( d1 - d2 ) < .000001f )
                                {
                                    continue;
                                }

                                if ( Math.Abs( d1 - anchorData[3].Distance ) < Math.Abs( d2 - anchorData[3].Distance ) )
                                {
                                    results.Add( res1 );
                                }
                                else
                                {
                                    results.Add( res2 );
                                }
                            }

                        }
                    }
                }
            }

            if ( results.Count == 0 )
            {
                return new QuadlinearCalculationResult
                {
                    Errored = true,
                    CalculationLevel = CalculationLevel.Low,
                    CalculationOptions = realData.Count
                };
            }
            Vector3 position = new Vector3();
            foreach ( Vector3 res in results )
            {
                position += res;
            }
            return new QuadlinearCalculationResult
            {
                Position = position / results.Count,
                CalculationLevel = CalculationLevel.High,
                CalculationOptions = realData.Count
            };
        }

        private static QuadlinearCalculationResult MediumCalculationResult( List<AnchorData> realData )
        {
            AnchorData[] anchorData = new AnchorData[4];
            Vector3 res1, res2;
            float d1, d2;
            for ( int i = 0; i < realData.Count; i++ )
            {
                anchorData[0] = realData[i];
                for ( int j = i + 1; j < realData.Count; j++ )
                {
                    anchorData[1] = realData[j];
                    for ( int k = j + 1; k < realData.Count; k++ )
                    {
                        anchorData[2] = realData[k];
                        for ( int l = k + 1; l < realData.Count; l++ )
                        {
                            anchorData[3] = realData[l];

                            if ( TrilinearCalculations.ThreePoint( anchorData[0].Position, anchorData[1].Position, anchorData[2].Position, anchorData[0].Distance, anchorData[1].Distance, anchorData[2].Distance, out res1, out res2 ) )
                            {
                                d1 = ( anchorData[3].Position - res1 ).sqrMagnitude;
                                d2 = ( anchorData[3].Position - res2 ).sqrMagnitude;
                                if ( Math.Abs( d1 - d2 ) < .000001f )
                                {
                                    continue;
                                }

                                if ( Math.Abs( d1 - anchorData[3].Distance ) < Math.Abs( d2 - anchorData[3].Distance ) )
                                {
                                    return new QuadlinearCalculationResult
                                    {
                                        Position = res1,
                                        CalculationLevel = CalculationLevel.Low,
                                        CalculationOptions = realData.Count
                                    };
                                }
                                else
                                {
                                    return new QuadlinearCalculationResult
                                    {
                                        Position = res2,
                                        CalculationLevel = CalculationLevel.Low,
                                        CalculationOptions = realData.Count
                                    };
                                }
                            }

                        }
                    }
                }
            }

            return new QuadlinearCalculationResult
            {
                Errored = true,
                CalculationLevel = CalculationLevel.Low,
                CalculationOptions = realData.Count
            };
        }

        private static QuadlinearCalculationResult LowCalculationResult( List<AnchorData> realData )
        {
            // Low level calculation tries to find a first match.
            AnchorData[] anchorData = new AnchorData[4];
            Vector3 res1, res2;
            float d1, d2;
            for ( int i = 0; i < realData.Count; i++ )
            {
                anchorData[0] = realData[i];
                for ( int j = i + 1; j < realData.Count; j++ )
                {
                    anchorData[1] = realData[j];
                    for ( int k = j + 1; k < realData.Count; k++ )
                    {
                        anchorData[2] = realData[k];
                        for ( int l = k + 1; l < realData.Count; l++ )
                        {
                            anchorData[3] = realData[l];

                            if ( TrilinearCalculations.ThreePoint( anchorData[0].Position, anchorData[1].Position, anchorData[2].Position, anchorData[0].Distance, anchorData[1].Distance, anchorData[2].Distance, out res1, out res2 ) )
                            {
                                d1 = ( anchorData[3].Position - res1 ).sqrMagnitude;
                                d2 = ( anchorData[3].Position - res2 ).sqrMagnitude;
                                if ( Math.Abs( d1 - d2 ) < .000001f )
                                {
                                    continue;
                                }

                                if ( Math.Abs( d1 - anchorData[3].Distance ) < Math.Abs( d2 - anchorData[3].Distance ) )
                                {
                                    return new QuadlinearCalculationResult
                                    {
                                        Position = res1,
                                        CalculationLevel = CalculationLevel.Low,
                                        CalculationOptions = realData.Count
                                    };
                                }
                                else
                                {
                                    return new QuadlinearCalculationResult
                                    {
                                        Position = res2,
                                        CalculationLevel = CalculationLevel.Low,
                                        CalculationOptions = realData.Count
                                    };
                                }
                            }

                        }
                    }
                }
            }

            return new QuadlinearCalculationResult
            {
                Errored = true,
                CalculationLevel = CalculationLevel.Low,
                CalculationOptions = realData.Count
            };

        }
    }
}