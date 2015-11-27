using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

public class SubModeManager : MonoBehaviour { // only used to switch the sub mode; a 'route station' script only

    private GlobalGameData gGameDataRef;
    public UnityEvent eventIntelSelect = new UnityEvent();
    private Button intelButton;

    void Awake()
    {
        gGameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>(); // tie the game camera script to the data
        intelButton = GameObject.Find("Intel Button").GetComponent<Button>();
    }

    public void SwitchSubModeIntel()
    {
        if (gGameDataRef.uiSubMode != GlobalGameData.eSubMode.Intel)
            gGameDataRef.uiSubMode = GlobalGameData.eSubMode.Intel;
        else
            SwitchSubModeNone();
    }

    public void SwitchSubModeWar()
    {
        if (gGameDataRef.uiSubMode != GlobalGameData.eSubMode.War)
            gGameDataRef.uiSubMode = GlobalGameData.eSubMode.War;
        else
            SwitchSubModeNone();
    }

    public void SwitchSubModeEmperor()
    {
        if (gGameDataRef.uiSubMode != GlobalGameData.eSubMode.Emperor)
            gGameDataRef.uiSubMode = GlobalGameData.eSubMode.Emperor;
        else
            SwitchSubModeNone();
    }

    public void SwitchSubModeFinance()
    {
        if (gGameDataRef.uiSubMode != GlobalGameData.eSubMode.Finance)
            gGameDataRef.uiSubMode = GlobalGameData.eSubMode.Finance;
        else
            SwitchSubModeNone();
    }

    public void SwitchSubModeDiplomacy()
    {
        if (gGameDataRef.uiSubMode != GlobalGameData.eSubMode.Diplomacy)
            gGameDataRef.uiSubMode = GlobalGameData.eSubMode.Diplomacy;
        else
            SwitchSubModeNone();
    }

    public void SwitchSubModeScience()
    {
        if (gGameDataRef.uiSubMode != GlobalGameData.eSubMode.Science)
            gGameDataRef.uiSubMode = GlobalGameData.eSubMode.Science;
        else
            SwitchSubModeNone();
    }

    public void SwitchSubModeNone()
    {     
        gGameDataRef.uiSubMode = GlobalGameData.eSubMode.None;  
    }
}
