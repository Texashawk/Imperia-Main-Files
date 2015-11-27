using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class FinanceView : MonoBehaviour {

    private GlobalGameData gGameDataRef;
    public UnityEvent eventIntelSelect = new UnityEvent();

    void Awake()
    {
        gGameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>(); // tie the game camera script to the data
    }
    // Use this for initialization
    void Start()
    {

        eventIntelSelect.AddListener(ActivateFinanceMode);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        if (gGameDataRef.uiSubMode == GlobalGameData.eSubMode.Finance)
            DisplayFinanceScreen();
    }

    void DisplayFinanceScreen()
    {
        GUI.Window(001, new Rect(20, 80, Screen.width - 40, Screen.height - 160), FinanceWindow, "Finance Window");
    }

    public void ActivateFinanceMode() // changes the global mode
    {
        if (gGameDataRef.uiSubMode != GlobalGameData.eSubMode.Finance)
            gGameDataRef.uiSubMode = GlobalGameData.eSubMode.Finance;
        Debug.Log("Finance SubMode Selected");

    }

    private void FinanceWindow(int id) // the actual window stuff to be drawn
    {
        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2, 200, 30), "This is the Finance Window!");
        GUI.DragWindow(new Rect(0, 0, 10000, 50));
    }
}
