using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class EmperorView : MonoBehaviour
{

    private GlobalGameData gGameDataRef;
    public UnityEvent eventIntelSelect = new UnityEvent();

    void Awake()
    {
        gGameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>(); // tie the game camera script to the data
    }
    // Use this for initialization
    void Start()
    {

        eventIntelSelect.AddListener(ActivateEmperorMode);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        if (gGameDataRef.uiSubMode == GlobalGameData.eSubMode.Emperor)
            DisplayEmperorScreen();
    }

    void DisplayEmperorScreen()
    {
        GUI.Window(001, new Rect(20, 80, Screen.width - 40, Screen.height - 160), IntelWindow, "Emperor Window");
    }

    public void ActivateEmperorMode() // changes the global mode
    {
        if (gGameDataRef.uiSubMode != GlobalGameData.eSubMode.Emperor)
            gGameDataRef.uiSubMode = GlobalGameData.eSubMode.Emperor;
        Debug.Log("Emperor SubMode Selected");

    }

    private void IntelWindow(int id) // the actual window stuff to be drawn
    {
        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2, 200, 30), "This is the Emperor Window!");
        GUI.DragWindow(new Rect(0, 0, 10000, 50));
    }
}
