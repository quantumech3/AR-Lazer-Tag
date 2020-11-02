/*
 * Created by Mika Taskinen on 30.1.2017
 * Copyright: University of Turku & Mika Taskinen
 */

using System;
using UnityEngine;

namespace Marin2.Decawave.Unity3d
{
    public static class TrilinearCalculations
    {
        /// <summary>
        /// Calculate position from trilinear calculation using three points and distances
        /// </summary>
        /// <param name="anchor1Pos">Position of first anchor</param>
        /// <param name="anchor2Pos">Position of second anchor</param>
        /// <param name="anchor3Pos">Position of third anchor</param>
        /// <param name="anchor1dist">Distance from first anchor</param>
        /// <param name="anchor2dist">Distance from second anchor</param>
        /// <param name="anchor3dist">Distance from third anchor</param>
        /// <param name="result1">First result</param>
        /// <param name="result2">Second result</param>
        /// <returns>true if calculation successful</returns>
        public static bool ThreePoint( Vector3 anchor1Pos, Vector3 anchor2Pos, Vector3 anchor3Pos, float anchor1dist, float anchor2dist, float anchor3dist, out Vector3 result1, out Vector3 result2 )
        {
            
            // Calculate the vectors between every anchor
            Vector3 v12 = anchor2Pos - anchor1Pos;
            Vector3 v23 = anchor3Pos - anchor2Pos;
            Vector3 v31 = anchor1Pos - anchor3Pos;
            
            // All the anchors form a plane, this vector (pN) is a normal vector of that plane
            Vector3 pN = Vector3.Cross( v12, v23 );

            // Pre-calculation of distances and squared distances of every anchor vector as they are used frequently
            float v12sLen = v12.sqrMagnitude;
            float v23sLen = v23.sqrMagnitude;
            float v31sLen = v31.sqrMagnitude;
            float v12Len = (float)Math.Sqrt( v12sLen );
            float v23Len = (float)Math.Sqrt( v23sLen );
            float v31Len = (float)Math.Sqrt( v31sLen );

            // Also the anchor distances (from receiver) are squared in precalculation
            float d1s = anchor1dist * anchor1dist;
            float d2s = anchor2dist * anchor2dist;
            float d3s = anchor3dist * anchor3dist;
            
            // This part actually calculated many things at once
            // 1) Calculate a projected point in every line between anchors
            // 2) Calculate a normal line that intersects those projected points
            // 3) Calculate best possible intersection between those lines
            Vector3 bp;
            if ( !CalculateIntersection(
                anchor1Pos + ( -( d2s - v12sLen - d1s ) / ( 2 * v12Len ) ) * v12 / v12Len,
                Vector3.Cross( pN, v12 ),
                anchor2Pos + ( -( d3s - v23sLen - d2s ) / ( 2 * v23Len ) ) * v23 / v23Len,
                Vector3.Cross( pN, v23 ),
                anchor3Pos + ( -( d1s - v31sLen - d3s ) / ( 2 * v31Len ) ) * v31 / v31Len,
                Vector3.Cross( pN, v31 ), out bp ) )
            {
                result1 = new Vector3();
                result2 = new Vector3();
                return false;
            }
            // Now we have a projected point in the plane but we need our actual position(s)
            // The mod variable will contain a right length normal vector of the plane to be added and subtracted from the plane projection point
            Vector3 mod = ( (float)Math.Sqrt( Math.Abs( d1s - ( anchor1Pos - bp ).sqrMagnitude ) ) ) * pN / pN.magnitude;
            result1 = bp + mod;
            result2 = bp - mod;

            return true;
        }

        private static bool CalculateIntersection( Vector3 p12, Vector3 n12, Vector3 p23, Vector3 n23, Vector3 p31, Vector3 n31, out Vector3 bp )
        {
            // A 3-line-intersection calculation with 3D-lines gives us 9 ways to calculate intersection
            // The best result is given when all possibilities are calculated and the averaged with nearest results
            // This algorithm uses fast way instead where first possibility is used
            float ot;
            bool res = CalculateIntersection( p12.x, p12.y, n12.x, n12.y, p23.x, p23.y, n23.x, n23.y, out ot ) ||
                CalculateIntersection( p12.x, p12.z, n12.x, n12.z, p23.x, p23.z, n23.x, n23.z, out ot ) ||
                CalculateIntersection( p12.z, p12.y, n12.z, n12.y, p23.z, p23.y, n23.z, n23.y, out ot );

            if ( res )
            {
                bp = p23 + n23 * ot;
                return true;
            }

            res = CalculateIntersection( p12.x, p12.y, n12.x, n12.y, p31.x, p31.y, n31.x, n31.y, out ot ) ||
                CalculateIntersection( p12.x, p12.z, n12.x, n12.z, p31.x, p31.z, n31.x, n31.z, out ot ) ||
                CalculateIntersection( p12.z, p12.y, n12.z, n12.y, p31.z, p31.y, n31.z, n31.y, out ot );

            if ( res )
            {
                bp = p31 + n31 * ot;
                return true;
            }

            res = CalculateIntersection( p23.x, p23.y, n23.x, n23.y, p31.x, p31.y, n31.x, n31.y, out ot ) ||
                CalculateIntersection( p23.x, p23.z, n23.x, n23.z, p31.x, p31.z, n31.x, n31.z, out ot ) ||
                CalculateIntersection( p23.z, p23.y, n23.z, n23.y, p31.z, p31.y, n31.z, n31.y, out ot );

            if ( res )
            {
                bp = p31 + n31 * ot;
                return true;
            }

            bp = new Vector3();
            return false;
        }

        private static bool CalculateIntersection( float p1x, float p1y, float v1x, float v1y, float p2x, float p2y, float v2x, float v2y, out float t )
        {
            float div = v2x * v1y - v2y * v1x;
            if ( Math.Abs( div ) < .0001 )
            {
                t = 0;
                return false;
            }

            t = ( v1x * ( p2y - p1y ) + v1y * ( p1x - p2x ) ) / div;
            return true;
        }
    }
}
