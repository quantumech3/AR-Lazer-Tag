using System.Collections.Generic;
using Google.XR.ARCoreExtensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class AppController : MonoBehaviour
{
    public GameObject HostedPointPrefab;
    public GameObject ResolvedPointPrefab;
    public ARAnchorManager AnchorManager;
    public ARRaycastManager RaycastManager;
    public InputField InputField;
    public Text OutputText;

    private enum AppMode
    {
        TouchToHostCloudReferencePoint,
        WaitingForHostedReferencePoint,
        TouchToResolveCloudReferencePoint,
        WaitingForResolvedReferencePoint
    }

    private AppMode m_AppMode = AppMode.TouchToHostCloudReferencePoint;
    private ARCloudAnchor m_CloudAnchor;
    private string m_CloudReferenceId;

    // Start is called before the first frame update
    void Start()
    {
        InputField.onEndEdit.AddListener(OnInputEndEdit);
    }

    private void OnInputEndEdit(string text)
    {
        m_CloudReferenceId = text;

        m_CloudAnchor = AnchorManager.ResolveCloudAnchorId(text);
        
        if(m_CloudAnchor == null)
        {
            OutputText.text = "Could not resolve CloudAnchor with ID = " + text;
            return;
        }

        m_AppMode = AppMode.WaitingForResolvedReferencePoint;
    }

    // Update is called once per frame
    void Update()
    {
        if(m_AppMode == AppMode.TouchToHostCloudReferencePoint)
        {
            OutputText.text = m_AppMode.ToString();
            
            // If the user presses a non-ui part of the screen
            if(Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                // Raycast where the user touched the screen
                List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
                RaycastManager.Raycast(Input.GetTouch(0).position, hitResults);

                // If the user touched a surface in AR
                if(hitResults.Count > 0)
                {
                    // Create a cloud anchor where the user touches in world space
                    Pose pose = hitResults[0].pose;
                    ARAnchor m_anchor = AnchorManager.AddAnchor(pose);
                    m_CloudAnchor = AnchorManager.HostCloudAnchor(m_anchor);

                    if(m_CloudAnchor == null)
                    {
                        Debug.LogError("Create Failed!");
                        return;
                    }

                    // Flag program to wait for the cloud anchor to register
                    m_AppMode = AppMode.WaitingForHostedReferencePoint;
                }
            }
        }
        else if(m_AppMode == AppMode.WaitingForHostedReferencePoint)
        {
            OutputText.text = m_AppMode.ToString();

            CloudAnchorState cloudAnchorState = m_CloudAnchor.cloudAnchorState;

            OutputText.text += " - " + cloudAnchorState.ToString();

            if(cloudAnchorState == CloudAnchorState.Success)
            {
                // Place a HostedPointPrefab where the cloud anchor successfully spawned
                GameObject cloudAnchor = Instantiate(HostedPointPrefab, Vector3.zero, Quaternion.identity);
                cloudAnchor.transform.SetParent(m_CloudAnchor.transform, false);

                m_CloudReferenceId = m_CloudAnchor.cloudAnchorId;

                // Make the user resolve the anchor next
                m_AppMode = AppMode.TouchToResolveCloudReferencePoint;
            }
        }
        else if(m_AppMode == AppMode.TouchToResolveCloudReferencePoint)
        {
            OutputText.text = m_CloudReferenceId;

            // If the user taps the screen
            if(Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                // Attempt to resolve the anchor that you just instantiated
                m_CloudAnchor = AnchorManager.ResolveCloudAnchorId(m_CloudReferenceId);

                if(m_CloudAnchor == null)
                {
                    OutputText.text = "Resolve failed!";
                    m_CloudReferenceId = string.Empty;
                    m_AppMode = AppMode.TouchToHostCloudReferencePoint;
                    return;
                }

                m_CloudReferenceId = string.Empty;

                m_AppMode = AppMode.WaitingForResolvedReferencePoint;
            }
        }
        else if(m_AppMode == AppMode.WaitingForResolvedReferencePoint)
        {
            OutputText.text = m_AppMode.ToString();

            CloudAnchorState cloudAnchorState = m_CloudAnchor.cloudAnchorState;

            if(cloudAnchorState == CloudAnchorState.Success)
            {
                GameObject cloudAnchor = Instantiate(ResolvedPointPrefab, Vector3.zero, Quaternion.identity);
                cloudAnchor.transform.SetParent(m_CloudAnchor.transform, false);

                m_CloudAnchor = null;
                m_AppMode = AppMode.TouchToHostCloudReferencePoint;
            }
            else if(cloudAnchorState != CloudAnchorState.TaskInProgress)
            {
                OutputText.text = "Invalid ID";
                m_AppMode = AppMode.TouchToHostCloudReferencePoint;
            }

            Debug.Log(m_CloudReferenceId);
        }
    }
}
