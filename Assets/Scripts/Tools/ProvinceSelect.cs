using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using StellarObjects;
using CameraScripts;
using HelperFunctions;

public class ProvinceSelect : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    GalaxyData gDataRef;
    GalaxyCameraScript galCameraRef;
    Color originalColor;

    void Awake()
    {
        gDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>(); // set the reference
        galCameraRef = GameObject.Find("Main Camera").GetComponent<GalaxyCameraScript>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        originalColor = GetComponent<Text>().color;
        GetComponent<Text>().color = Color.yellow;
    }

    public void OnPointerExit(PointerEventData eventData)
    {        
        GetComponent<Text>().color = originalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerId == -1) // for right mouse click
        {
            if (tag == "Province")
            {
                galCameraRef.provinceTarget = HelperFunctions.DataRetrivalFunctions.GetProvince(transform.name);
                galCameraRef.systemZoomActive = false;
                galCameraRef.planetZoomActive = false;
                galCameraRef.provinceZoomActive = true;
            }
        }
    }
}
