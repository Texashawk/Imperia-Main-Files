using UnityEngine;
using Assets.Scripts.Interfaces;

namespace Assets.Scripts.States
{
    public class BeginState : IStateBase
    {
        private StateManager manager;

        public BeginState(StateManager managerRef)   //constructor
        {
            manager = managerRef;
            Debug.Log("Constructing NewGame");
        }

        public void StateUpdate()
        {
            
        }

        public void ShowIt()
        {
            // draw main screen
            //GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height),
            //    manager.gameDataRef.mainGameScreen,
            //    ScaleMode.StretchToFill);

            GUI.Label(new Rect(Screen.width - 100, 10, 100, 30), "build: " + manager.gameDataRef.gameVersion);

            if (GUI.Button(new Rect((Screen.width / 2) - (150 / 2), Screen.height - 200, 150, 100), "New Game Test"))
                Switch();
        }

        public void StateFixedUpdate()
        {

        }

        void Switch()
        {
            Debug.Log("Setting up the galaxy");
            Application.LoadLevelAsync("NewGameScreen");
            manager.SwitchState(new NewGameState(manager));
        }
    }
}
