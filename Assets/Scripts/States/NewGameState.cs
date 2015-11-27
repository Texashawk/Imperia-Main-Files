using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using Assets.Scripts.Interfaces;

namespace Assets.Scripts.States
{
    public class NewGameState : IStateBase
    {
        public static AsyncOperation async;
        private StateManager manager;
        private InputField starAmountInputField;
        private InputField AICountInputField;
        private InputField galaxySizeInputField;
        private InputField playerEmpireNameInputField;
        private GlobalGameData gameDataRef;
        private bool valuesLoaded = false;
        private bool valuesEntered = false;

        public NewGameState(StateManager managerRef)   //constructor
        {
            manager = managerRef;
            valuesLoaded = false; // reset
            valuesEntered = false;
            gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();     
            Debug.Log("Constructing New Game Screen");
        }

        public void StateUpdate()
        {
           if (valuesEntered)
           {
               Switch();
           }
        }

        public void LoadNextLevel()
        {
            Application.LoadLevel("BuildQuadrant"); // test
            //async.allowSceneActivation = false;
        }

        public void StartNextLevel()
        {
            //async.allowSceneActivation = true;
            //async = null; // because is static, have to clean up
        }

        public void ShowIt()
        {
            GUI.Label(new Rect(Screen.width - 100, 10, 100, 30), "build: " + manager.gameDataRef.gameVersion);
            if (GUI.Button(new Rect((Screen.width / 2) - (450 / 2), Screen.height - 200, 450, 90), "When you have entered the setup values, press to continue."))
            {
                valuesEntered = true;
            }
        }

        public void StateFixedUpdate()
        {

        }

        void Switch()
        {
            if (!valuesLoaded)
            {
                GrabUIValues();
                valuesLoaded = true;
            }

            Debug.Log("Moving to loading galaxy...");    
            LoadNextLevel(); // test
            manager.SwitchState(new SetupState(manager)); // holds the level load until completed (test)           
        }

        void GrabUIValues()
        {
            // grab the UI objects
            starAmountInputField = GameObject.Find("Star Input Field").GetComponent<InputField>();
            AICountInputField = GameObject.Find("AI Input Field").GetComponent<InputField>();
            galaxySizeInputField = GameObject.Find("Galaxy Size Input Field").GetComponent<InputField>();
            playerEmpireNameInputField = GameObject.Find("Player Empire Name Input Field").GetComponent<InputField>();

            // get the values
            gameDataRef.TotalSystems = int.Parse(starAmountInputField.text);
            gameDataRef.NumberOfAICivs = int.Parse(AICountInputField.text);
            gameDataRef.PlayerEmpireName = playerEmpireNameInputField.text;
            gameDataRef.GalaxySizeHeight = int.Parse(galaxySizeInputField.text);
            gameDataRef.GalaxySizeWidth = int.Parse(galaxySizeInputField.text);
        }
    }

    
}

