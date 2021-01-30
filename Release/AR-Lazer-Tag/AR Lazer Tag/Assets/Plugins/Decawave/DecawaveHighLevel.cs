using System;
using System.Collections.Generic;
using UnityEngine;
using Decawave.Common;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using System.Linq;

namespace Decawave
{
    namespace HighLevel
    {
        /// <summary>
        /// Contains basic math functions used for RF localization
        /// </summary>
        public class RF
        {
            private static Matrix EquationMatrix(float[,] p)
            {
                return DenseMatrix.OfArray(new float[,]{ { -2*p[0, 0] + 2*p[1, 0], -2*p[0, 1] + 2*p[1, 1], -2*p[0, 2] + 2*p[1, 2]},
                                                 { -2*p[1, 0] + 2*p[2, 0], -2*p[1, 1] + 2*p[2, 1], -2*p[1, 2] + 2*p[2, 2]},
                                                 { -2*p[2, 0] + 2*p[3, 0], -2*p[2, 1] + 2*p[3, 1], -2*p[2, 2] + 2*p[3, 2]} });
            }
            private static Vector TargetVector(float[,] p, float[] r)
            {
                return DenseVector.OfArray(new float[] {
                    -p[0, 0]*p[0, 0] - p[0, 1]*p[0, 1] - p[0, 2]*p[0, 2] + p[1, 0]*p[1, 0] + p[1, 1]*p[1, 1] + p[1, 2]*p[1, 2] + r[0]*r[0] - r[1]*r[1],
                    -p[1, 0]*p[1, 0] - p[1, 1]*p[1, 1] - p[1, 2]*p[1, 2] + p[2, 0]*p[2, 0] + p[2, 1]*p[2, 1] + p[2, 2]*p[2, 2] + r[1]*r[1] - r[2]*r[2],
                    -p[2, 0]*p[2, 0] - p[2, 1]*p[2, 1] - p[2, 2]*p[2, 2] + p[3, 0]*p[3, 0] + p[3, 1]*p[3, 1] + p[3, 2]*p[3, 2] + r[2]*r[2] - r[3]*r[3]
                });
            }

            /// <summary>
            /// Finds the closest solution for x satisfying Ax = b
            /// </summary>
            /// <param name="A"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static float[] Lstsq(Matrix A, Vector b)
            {
                return A.PseudoInverse().Multiply(b).AsArray();
            }

            private static float[] RawTrilaterate4(float[,] positions, float[] distances)
            {
                return Lstsq(EquationMatrix(positions), TargetVector(positions, distances));
            }

            /// <summary>
            /// Given the positions of each anchor and distances from those anchors, will return your position in RF space
            /// </summary>
            /// <param name="idsToPositions">Dictionary associating anchor ids with positions</param>
            /// <param name="r0">Distance from anchor</param>
            /// <param name="r1">Distance from anchor</param>
            /// <param name="r2">Distance from anchor</param>
            /// <param name="r3">Distance from anchor</param>
            /// <returns>Corrosponding position in RF space</returns>
            private static Vector3 Trilaterate4(Dictionary<int, Vector3> idsToPositions, AnchorData r0, AnchorData r1, AnchorData r2, AnchorData r3)
            {
                if (idsToPositions.Count != 4)
                    throw new HighLevelException("Trilaterate4 called with insufficient amount of anchors. Only 4 anchor positions are suppoased to be passed, but " + idsToPositions.Count + "were instead");


                // Transform ids and distances into dictionary
                Dictionary<int, double> idsToDistances = new Dictionary<int, double>();
                idsToDistances.Add(r0.id, r0.distance);
                idsToDistances.Add(r1.id, r1.distance);
                idsToDistances.Add(r2.id, r2.distance);
                idsToDistances.Add(r3.id, r3.distance);

                // Sort distances by id
                float[] distanceVector = idsToDistances.OrderBy(e => e.Key)
                                                   .Select(e => (float)e.Value) // Cast distances to float
                                                   .ToArray();

                // Transform positions into position matrix with each anchor coordinate sorted by id
                float[,] positionMatrix = new float[4, 3];
                KeyValuePair<int, Vector3>[] positionsSortedById = idsToPositions.OrderBy((k) => k.Key).ToArray();
                for (int i = 0; i < positionsSortedById.Length; i++)
                {
                    positionMatrix[i, 0] = positionsSortedById[i].Value.x;
                    positionMatrix[i, 1] = positionsSortedById[i].Value.y;
                    positionMatrix[i, 2] = positionsSortedById[i].Value.z;
                }

                // Trilaterate rf position and return result as vector
                float[] rfPosition = RawTrilaterate4(positionMatrix, distanceVector);
                return new Vector3(rfPosition[0], rfPosition[1], rfPosition[2]);
            }

            /// <summary>
            /// Given a sufficient, and supported amount of positions, will attempt to perform trilateration and throw HighLevelException on failure
            /// </summary>
            /// <param name="idsToPositions"></param>
            /// <param name="rs">Distances to anchors</param>
            /// <returns></returns>
            public static Vector3 Trilaterate(Dictionary<int, Vector3> idsToPositions, params AnchorData[] rs)
            {
                if (idsToPositions.Count != 4 || rs.Count() != idsToPositions.Count)
                    throw new HighLevelException("RF.Trilaterate() called with insufficient number of anchor positions or distances. #Anchors: " + idsToPositions.Count + " #Distances: " + rs.Count());

                return Trilaterate4(idsToPositions, rs[0], rs[1], rs[2], rs[3]);
            }

            /// <summary>
            /// Given positions derived from various localization method, will attempt to fuse them with Mallesh Dasari's algorithm
            /// </summary>
            /// <param name="rfDerived"></param>
            /// <param name="vioDerived"></param>
            /// <returns></returns>
            public static Vector3 ToFusion(Vector3 rfDerived, Vector3 vioDerived)
            { 
                return (rfDerived + vioDerived) / 2f;
            }

            /// <summary>
            /// Given transforms derived from various localization method, will attempt to fuse them with Mallesh Dasari's algorithm
            /// </summary>
            /// <param name="rfDerived"></param>
            /// <param name="vioDerived"></param>
            /// <returns></returns>
            public static Transform ToFusion(Transform rfDerived, Transform vioDerived)
            {
                Transform fusion = (new GameObject()).transform; // Instantiate new transform. This is the easiest way to do it
                fusion.position = (rfDerived.position + vioDerived.position) / 2f;
                fusion.rotation = rfDerived.rotation; // Both rotations are driven by VIO right now. 
                fusion.localScale = rfDerived.localScale; // Scale is never changed.

                return fusion;
            }
        }

        /// <summary>
        /// Thrown on failure of Decawave high level interface
        /// </summary>
        public class HighLevelException : Decawave.Common.DecawaveException
        {
            public HighLevelException(string message) : base(message)
            {

            }
        }
    }
}
