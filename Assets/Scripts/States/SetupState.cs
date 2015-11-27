using UnityEngine;
using System.Collections;
using Assets.Scripts.Interfaces;
using UnityEngine.UI;

namespace Assets.Scripts.States
{
    public class SetupState : IStateBase
    {

        private StateManager manager;
        private GalaxyData gData;
        private GlobalGameData gameData;
        private Text planetCount;
        private GameObject gamePanel;
        public GameObject[] starObjects;

        public SetupState(StateManager managerRef)
        {
            manager = managerRef;
            gData = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            gameData = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
            Debug.Log("In SetupState");        
        }

        public void Update()
        {
            StateUpdate();
        }

        public void StateUpdate()
        {
            if (gData.galaxyIsGenerated && gameData.CivsGenerated) // if all parameters are ready to load
            {
                Application.LoadLevel("GalaxyMap");
                manager.SwitchState(new GalaxyMapState(manager));  // and change the state manager to the beginning state
            }
        }

        public void ShowIt()
        {
            // draw stats
            GUI.Label(new Rect(Screen.width - 100, 10, 100, 30), "build: " + manager.gameDataRef.gameVersion);
        }

        public void StateFixedUpdate()
        {

        }

        
       
    }
}