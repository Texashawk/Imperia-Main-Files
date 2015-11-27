using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CharacterObjects;
using Actions;
using CameraScripts;
using GameEvents;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EventScrollView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public GameObject Button_Template;
    private GlobalGameData gameDataRef;
    private List<GameObject> buttonList = new List<GameObject>();
    public bool listIsInitialized = false;
    private Camera mainCamera;
    

    void Awake()
    {
        gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    public void InitializeList()
    {    
        ClearList(); // clear out the list            
        foreach (GameEvent gEvent in gameDataRef.CivList[0].LastTurnEvents)
        {                   
            AddButton(gEvent);               
        }    
    }

    private void AddButton(GameEvent gEvent)
    {
        GameObject go = Instantiate(Button_Template) as GameObject;
        go.SetActive(true);
        go.name = gEvent.CivID + buttonList.Count.ToString();
        EventButton TB = go.GetComponent<EventButton>();
        TB.SetName(gEvent.Date.ToString("F1") + ": " + gEvent.Description.ToUpper());
        TB.SetAlertLevel(gEvent.Level);
        TB.SetLocation(gEvent.SystemLocationID, gEvent.PlanetLocationID);
        TB.SetID(go.name);
        TB.SetPicture(gEvent.Type);
        go.transform.SetParent(Button_Template.transform.parent);
        go.transform.localScale = new Vector3(1, 1, 1); // to offset canvas scaling
        go.transform.localPosition = new Vector3(go.transform.localPosition.x, go.transform.localPosition.y, 0);
        buttonList.Add(go);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mainCamera.GetComponent<GalaxyCameraScript>().ScrollWheelIsValid = false; // don't allow scrolling
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mainCamera.GetComponent<GalaxyCameraScript>().ScrollWheelIsValid = true; // allow scrolling once again
    }

    public void ClearList()
    {
        foreach (GameObject go in buttonList)
        {
            Destroy(go);
        }
    }

    public void ButtonClicked()
    {
       
    }
}