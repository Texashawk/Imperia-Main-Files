using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class WarView : MonoBehaviour {

    private GlobalGameData gGameDataRef;
    public UnityEvent eventIntelSelect = new UnityEvent();

    void Awake()
    {
        gGameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>(); // tie the game camera script to the data
    }
    // Use this for initialization
    void Start()
    {

        eventIntelSelect.AddListener(ActivateWarMode);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        if (gGameDataRef.uiSubMode == GlobalGameData.eSubMode.War)
            DisplayWarScreen();
    }

    void DisplayWarScreen()
    {
        //GUI.Label(new Rect(20, 90, 200, 25), "Submode: Intel View");
        GUI.Window(001, new Rect(20, 80, Screen.width - 40, Screen.height - 160), WarWindow, "War Window");
    }

    public void ActivateWarMode() // changes the global mode
    {
        if (gGameDataRef.uiSubMode != GlobalGameData.eSubMode.War)
            gGameDataRef.uiSubMode = GlobalGameData.eSubMode.War;
        Debug.Log("War SubMode Selected");

    }

    private void WarWindow(int id) // the actual window stuff to be drawn
    {
        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2, 200, 30), "This is the War Window!");
        GUI.DragWindow(new Rect(0, 0, 10000, 50));
    }
}
