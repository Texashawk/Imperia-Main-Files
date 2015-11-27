using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GameEvents;
using CameraScripts;
using StellarObjects;


public class EventButton : MonoBehaviour
{
    private string buttonID;
    private string characterID;
    private string planetLocationID;
    private string systemLocationID;
    private GraphicAssets gAssets;
    public Text ButtonText;
    public Image ButtonImage;
    public EventScrollView ScrollView;

    public void Awake()
    {
        gAssets = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
    }

    public void SetName(string name)
    {
        ButtonText.text = name;
    }

    public void SetAlertLevel(GameEvent.eEventLevel alertLevel)
    {
        Color orange = new Color(1f,.55f,0f);
        switch (alertLevel)
        {
            case GameEvent.eEventLevel.Informational:
                ButtonText.color = Color.white;
                break;

            case GameEvent.eEventLevel.Positive:
                ButtonText.color = Color.green;
                break;

            case GameEvent.eEventLevel.Moderate:
                ButtonText.color = Color.yellow;
                break;

            case GameEvent.eEventLevel.Serious:
                ButtonText.color = orange;
                break;

            case GameEvent.eEventLevel.Critical:
                ButtonText.color = Color.red;
                break;

            default:
                ButtonText.color = Color.white;
                break;
        }
    }

    public void SetID(string ID)
    {
        buttonID = ID;
    }

    public void SetPicture(GameEvent.eEventType picID)
    {
        ButtonImage.sprite = gAssets.EventPicList.Find(p => p.name.ToLower() == picID.ToString().ToLower());
    }

    public void SetLocation (string sLocation, string pLocation)
    {
        planetLocationID = pLocation;
        systemLocationID = sLocation;
    }

    public void Button_Click()
    {
        GalaxyData galData = GameObject.Find("GameManager").GetComponent<GalaxyData>();
        GlobalGameData gData = GameObject.Find("GameManager").GetComponent<GlobalGameData>();

        //invoke zoom sequence
        if (systemLocationID != null)
        {
            Transform star = galData.GalaxyStarList.Find(p => p.name == HelperFunctions.DataRetrivalFunctions.GetSystem(systemLocationID).Name).transform;
            if (star != null)
            {
                Camera.main.GetComponent<GalaxyCameraScript>().starTarget = star;
                star.GetComponent<Star>().tag = "Selected";
                Camera.main.GetComponent<GalaxyCameraScript>().systemZoomActive = true;
                Camera.main.GetComponent<GalaxyCameraScript>().planetZoomActive = false;
                Camera.main.GetComponent<GalaxyCameraScript>().ScrollWheelIsValid = true; // allow scrolling once again
                gData.StarSelected = true;
            }
        
        }
    }
}
