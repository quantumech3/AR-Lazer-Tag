/*
 * Created by Mika Taskinen on 30.1.2017
 * Copyright: University of Turku & Mika Taskinen
 * 
 * THIS IS AN EXAMPLE MONOBEHAVIOR CLASS To WHO HOW TO USE THE LIBRARY
 */

using Marin2.Decawave.Unity3d;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DecawaveWorker : MonoBehaviour {
    
    private string dataString;
    private DecawaveManager manager;

    // Hard coded MR-lab anchor positions
    private static readonly Dictionary<int, Vector3> anchorPositions = new Dictionary<int, Vector3>
    {
        {0, new Vector3( 4.313f, 0.95f, 0.183f ) },
        {1, new Vector3( .14f, .955f, .28f ) },
        {2,  new Vector3( .135f, .94f, 6.718f ) },
        {3,  new Vector3( 4.269f, 1.843f, 5.064f ) }
    };

    // Use this for initialization
    void Start () {

        // Initialize decawave system and attach event on receiver discovery
        manager = DecawaveManager.Instance;
        manager.ReceiverAppeared += Manager_ReceiverAppeared;
    }

    private void Manager_ReceiverAppeared( DecawaveManager manager, Receiver receiver )
    {
        // Announce the physical anchor positions for the receiver
        foreach ( KeyValuePair<int, Vector3> anchorPosition in anchorPositions )
        {
            receiver.SetAnchorPosition( anchorPosition.Key, anchorPosition.Value );
        }
        // Set performance related preferences
        receiver.CalculationLevel = CalculationLevel.Low;
        receiver.UpdateLevel = UpdateLevel.OnUpdate;
    }

    // Update is called once per frame
    void Update ()
    {
        // Update manager to actively keep track of data (Use this as often as you want, but remember: The data will not disappear until popped by update)
        manager.Update();

        // Example string build for data
        StringBuilder builder = new StringBuilder();
        foreach ( Receiver receiver in manager.Receivers )
        {
            builder.AppendLine( "Receiver \"" + receiver.Serial + "\"" );

            // Get latest calculation result
            ICalculationResult result = receiver.CalculationResult;
            foreach ( Anchor anchor in receiver.Anchors )
            {
                if ( anchorPositions.ContainsKey( anchor.Id ) )
                {
                    builder.AppendLine( "Anchor " + anchor.Id + " distance: " + anchor.Distance + " (position: " + VectorString( anchorPositions[anchor.Id] ) + ")" );
                }
                else
                {
                    builder.AppendLine( "Anchor " + anchor.Id + " distance: " + anchor.Distance );
                }
            }
            builder.AppendLine( "----- RESULT -----" );

            // how to handle the result: Check the type from enum field and then cast
            switch ( result.ResultType )
            {
                case CalculationResultType.Null:
                    builder.AppendLine( "No result" );
                    break;
                case CalculationResultType.Single:
                    SinglePointCalculationResult singleResult = (SinglePointCalculationResult)result;
                    builder.AppendLine( "Single point" );
                    builder.AppendLine( "Position: " + VectorString( singleResult.Position ) );
                    builder.AppendLine( "Radius: " + singleResult.Distance );
                    break;
                case CalculationResultType.Double:
                    DoublePointCalculationResult doubleResult = (DoublePointCalculationResult)result;
                    builder.AppendLine( "Circle result" );
                    builder.AppendLine( "Center position: " + VectorString( doubleResult.Position ) );
                    builder.AppendLine( "Normal Direction: " + VectorString( doubleResult.Normal ) );
                    builder.AppendLine( "Radius: " + doubleResult.Radius );
                    break;
                case CalculationResultType.Trilinear:
                    TrilinearCalculationResult triResult = (TrilinearCalculationResult)result;
                    builder.AppendLine( "Trilinear result" );
                    if ( triResult.CalculationErrored )
                    {
                        builder.AppendLine( "ERROR: Trilinear calculation failed" );
                    }
                    else
                    {
                        builder.AppendLine( "Result 1: " + VectorString( triResult.Result1 ) );
                        builder.AppendLine( "Result 2: " + VectorString( triResult.Result2 ) );
                    }
                    break;
                case CalculationResultType.Quadlinear:
                    QuadlinearCalculationResult quadResult = (QuadlinearCalculationResult)result;
                    builder.AppendLine( "Quadlinear result" );
                    builder.AppendLine( "Calculated from " + quadResult.CalculationOptions + " points" );
                    builder.AppendLine( "Calculated with " + quadResult.CalculationLevel + " calculation level" );
                    if ( quadResult.Errored )
                    {
                        builder.AppendLine( "ERROR: Quadlinear calculation failed" );
                    }
                    else
                    {
                        builder.AppendLine( "Position: " + VectorString( quadResult.Position ) );
                    }
                    break;
            }
            builder.AppendLine();
        }

        // build to string
        dataString = builder.ToString();

    }

    void OnGUI()
    {
        // Print the data on GUI
        GUI.TextArea( new Rect( 10, 10, Screen.width - 20, Screen.height - 20 ), dataString );
    }

    /// <summary>
    /// Vectors in custom string format
    /// </summary>
    /// <param name="v">Given vector</param>
    /// <returns>String format of vector</returns>
    private string VectorString( Vector3 v )
    {
        return "[ " + v.x + ", " + v.y + ", " + v.z + " ]";
    }
}
