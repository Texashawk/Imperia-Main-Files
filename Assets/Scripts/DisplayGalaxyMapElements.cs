using UnityEngine;
using System.Collections;
using CameraScripts;
using UnityEngine.UI;

public class DisplayGalaxyMapElements : MonoBehaviour
{

    private GameObject rosette;
    private GameObject UICamera;
    private GameObject mainCamera;
    private GameObject zoomValueText;

    // Use this for initialization
    void Start()
    {
        rosette = GameObject.Find("CompassRose");
        UICamera = GameObject.Find("UI Camera");
        mainCamera = GameObject.Find("Main Camera");
        zoomValueText = GameObject.Find("CameraZoomValue");
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        RemoveUIObjects();
    }

    void RemoveUIObjects()
    {
        if (mainCamera.GetComponent<GalaxyCameraScript>().zoomLevel >= GalaxyCameraScript.cameraZoomLevel.System)  // take away things that don't belong in the system view
        {
            rosette.SetActive(false);
            //zoomValueText.SetActive(false);
        }
        else
        {
            rosette.SetActive(true);
            zoomValueText.SetActive(true);
        }
    }
}
