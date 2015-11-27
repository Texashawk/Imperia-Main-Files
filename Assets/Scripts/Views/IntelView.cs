using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class IntelView : MonoBehaviour {

    private GlobalGameData gGameDataRef;
    public UnityEvent eventIntelSelect = new UnityEvent();

    void Awake()
    {
        gGameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>(); // tie the game camera script to the data
    }
	// Use this for initialization
	void Start () {
        
        eventIntelSelect.AddListener(ActivateIntelMode);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        if (gGameDataRef.uiSubMode == GlobalGameData.eSubMode.Intel)
            DisplayIntelScreen();
    }

    void DisplayIntelScreen()
    {
        //GUI.Label(new Rect(20, 90, 200, 25), "Submode: Intel View");
        GUI.Window(001, new Rect(20, 80, Screen.width - 40, Screen.height - 160), IntelWindow, "Intel Window");
    }

    public void ActivateIntelMode() // changes the global mode
    {
        if (gGameDataRef.uiSubMode != GlobalGameData.eSubMode.Intel)
            gGameDataRef.uiSubMode = GlobalGameData.eSubMode.Intel;
        Debug.Log("Intel SubMode Selected");

    }

    private void IntelWindow(int id) // the actual window stuff to be drawn
    {
        GUI.Label(new Rect(Screen.width/2 - 100,Screen.height/2 - 30, 200, 30), "This is the Intel Window!");
        GUI.DragWindow(new Rect(0,0,10000,50));
    }
}
