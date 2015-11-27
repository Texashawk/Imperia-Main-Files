using UnityEngine;
using System.Collections;
using Assets.Scripts.Interfaces;

namespace Assets.Scripts.States
{
    public class GalaxyMapState : IStateBase
    {

        private StateManager manager;
        private GlobalGameData gGameData;
        public GalaxyMapState(StateManager managerRef)
        {
            manager = managerRef;
            gGameData = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
            Debug.Log("Constructing GalaxyMapState");

                   
        }

        public void StateUpdate()
        {
            //if (Input.GetKeyUp(KeyCode.Space))
            //{
            //    Application.LoadLevel("BeginningScene");  //load the beginning scene
            //    // there will need to be a function here to reset all the data, probably destroy the game manager
            //    manager.SwitchState(new BeginState(manager));  // and change the state manager to the beginning state
            //}

            if (Input.GetKeyUp(KeyCode.Space))
            {
                gGameData.RequestGraphicRefresh = true; // request refresh of redraw
                if (gGameData.DebugMode)
                    gGameData.DebugMode = false;
                else
                    gGameData.DebugMode = true;
            }
        }

        public void ShowIt()
        {
 
        }

        public void StateFixedUpdate()
        {

        }
    }
}
