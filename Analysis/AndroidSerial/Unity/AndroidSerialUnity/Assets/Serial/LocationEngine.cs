using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Marin2.Decawave.Unity3d
{
    public class LocationEngine
    {

        // Location engine stores positions and distances separately
        private Dictionary<int, Vector3> anchorPositions;
        private Dictionary<int, float> anchorDistances;

        /// <summary>
        /// Get or set the level of calculation
        /// </summary>
        public CalculationLevel CalculationLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Create location engine
        /// </summary>
        public LocationEngine()
        {
            anchorDistances = new Dictionary<int, float>();
            anchorPositions = new Dictionary<int, Vector3>();
        }

        /// <summary>
        /// Sets the position of the anchor
        /// </summary>
        /// <param name="id">Anchor id</param>
        /// <param name="position">Anchor position</param>
        public void SetAnchorPosition( int id, Vector3 position )
        {
            if ( anchorPositions.ContainsKey( id ) )
            {
                anchorPositions[id] = position;
            }
            else
            {
                anchorPositions.Add( id, position );
            }
        }

        /// <summary>
        /// Sets the anchor distance
        /// </summary>
        /// <param name="id">Anchor id</param>
        /// <param name="distance">Anchor distance</param>
        public void SetAnchorDistance( int id, float distance )
        {
            if ( anchorDistances.ContainsKey( id ) )
            {
                anchorDistances[id] = distance;
            }
            else
            {
                anchorDistances.Add( id, distance );
            }
        }

        /// <summary>
        /// Releases anchor distance
        /// </summary>
        /// <param name="id">The id of the anchor</param>
        public void UnsetAnchorDistance( int id )
        {
            anchorDistances.Remove( id );
        }

        /// <summary>
        /// Releases anchor position
        /// </summary>
        /// <param name="id">The id of the anchor</param>
        public void UnsetAnchorPosition( int id )
        {
            anchorPositions.Remove( id );
        }

        /// <summary>
        /// Calculate points
        /// </summary>
        /// <param name="level">Calculation level</param>
        /// <param name="anchorData">Given data to calculate from</param>
        /// <returns></returns>
        public static ICalculationResult Calculate( CalculationLevel level, params AnchorData[] anchorData )
        {
            // Get only points that are truly valid (distance ok etc.)
            List<AnchorData> realData = new List<AnchorData>( anchorData.Length );
            GetRealData( anchorData, realData );

            // This is how to handle calculation by numbers (case 4 and default may result in TrilinearCalculationResult also)
            switch ( realData.Count )
            {
                case 0:
                    return new NullCalculationResult();
                case 1:
                    return new SinglePointCalculationResult( realData[0] );
                case 2:
                    return new DoublePointCalculationResult( realData[0], realData[1] );
                case 3:
                    return new TrilinearCalculationResult( realData[0], realData[1], realData[2] );
                case 4:
                    return QuadlinearCalculationResult.Exact( realData[0], realData[1], realData[2], realData[3], level );
                default:
                    return QuadlinearCalculationResult.Sparse( realData, level );   
            }
        }

        /// <summary>
        /// Filters unwanted anchor data from given data
        /// </summary>
        /// <param name="anchorData">Source data</param>
        /// <param name="result">Filtered data, preallocated (and precleared)</param>
        private static void GetRealData( AnchorData[] anchorData, IList<AnchorData> result )
        {
            // failcounts are meant to track every anchor separately
            int[] failCounts = new int[anchorData.Length];
            for ( int i = 0; i < anchorData.Length; i++ )
            {
                for ( int j = i + 1; j < anchorData.Length; j++ )
                {
                    // Check if the anchors are too far away to actually create ball-to-ball intersection
                    float dist = anchorData[i].Distance + anchorData[j].Distance;
                    if ( ( anchorData[i].Position - anchorData[j].Position ).sqrMagnitude > dist * dist )
                    {
                        // increase fail counts for both
                        failCounts[i]++;
                        failCounts[j]++;
                    }
                }
            }

            // drop every anchor that have failed more than once ( or if there are only 2 points, then a failure removes a point immendiatelly)
            // not the best way to filter ( TODO: improve )
            if ( anchorData.Length < 3 )
            {
                for ( int i = 0; i < anchorData.Length; i++ )
                {
                    if ( failCounts[i] == 0 )
                        result.Add( anchorData[i] );
                }
            }
            else
            {
                for ( int i = 0; i < anchorData.Length; i++ )
                {
                    if ( failCounts[i] < 2 )
                        result.Add( anchorData[i] );
                }
            }

        }

        /// <summary>
        /// Calculate the current data
        /// </summary>
        /// <returns>Calculation result according to calculations</returns>
        public ICalculationResult Calculate()
        {
            // Before calculation, we need to check which anchors contain position and distance
            int count = 0;
            foreach ( int id in anchorPositions.Keys )
                if ( anchorDistances.ContainsKey( id ) )
                    count++;
            AnchorData[] anchorData = new AnchorData[count];
            int index = 0;
            foreach ( KeyValuePair<int, Vector3> kvp in anchorPositions )
            {
                if ( anchorDistances.ContainsKey( kvp.Key ) )
                {
                    anchorData[index++] = new AnchorData
                    {
                        Position = kvp.Value,
                        Distance = anchorDistances[kvp.Key],
                        Id = kvp.Key
                    };
                }
            }

            return Calculate( CalculationLevel, anchorData );
        }
    }
}
