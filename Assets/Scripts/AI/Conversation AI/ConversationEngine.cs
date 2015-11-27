using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CharacterObjects;
using StellarObjects;
using PlanetObjects;
using CivObjects;
using System;
using HelperFunctions;
using Actions;

namespace ConversationAI
{
    public static class ConversationEngine
    {
        // lists of sentence types
        public static List<string> IntroGreetingSentences = new List<string>();
        public static List<string> IntroGenericSentences = new List<string>();
        public static List<string> PositiveResponses = new List<string>();
        public static List<string> NegativeResponses = new List<string>();

        // lists of adjectives that fit into the descriptions
        public static List<string> SportsTeamNames = new List<string>();
        public static List<string> Colors = new List<string>();
        public static List<string> PositiveAdjectives = new List<string>();
        public static List<string> NegativeAdjectives = new List<string>();

        // constants
        private const int sentenceMax = 3;

        public static void ReadConversationData()
        {
            Colors.Add("Blue");
            Colors.Add("Green");
            Colors.Add("Orange");
            Colors.Add("Yellow");
            Colors.Add("White");
            PositiveAdjectives.Add("awesome");
            PositiveAdjectives.Add("amazing");
            PositiveAdjectives.Add("wonderful");
            NegativeAdjectives.Add("terrible");
            NegativeAdjectives.Add("horrible");
            NegativeAdjectives.Add("awful");
            SportsTeamNames.Add("Bombers");
            SportsTeamNames.Add("Crushers");
            SportsTeamNames.Add("Demolishers");
            SportsTeamNames.Add("Devestators");

            ReadGenericSentenceData();
        }

        private static void ReadGenericSentenceData()
        {
            // generic sentence data
            try
            {
                string line = null;
                string path = Application.dataPath;
                bool fileEmpty = false;

                System.IO.TextReader readFile = new StreamReader(path + "/Resources/IntroGenericSentences.txt");
                while (!fileEmpty)
                {
                    line = readFile.ReadLine();
                    if (line != null)
                    {                       
                        string desc = "";
                        desc = line;
                        IntroGenericSentences.Add(desc);                     
                    }
                    else
                        fileEmpty = true;
                }
                readFile.Close();
                readFile = null;
            }
            catch (IOException ex)
            {
                Debug.LogError("Could not read file; error:" + ex.ToString());
            }

            try
            {
                string line = null;
                string path = Application.dataPath;
                bool fileEmpty = false;

                System.IO.TextReader readFile = new StreamReader(path + "/Resources/IntroGreetingSentences.txt");
                while (!fileEmpty)
                {
                    line = readFile.ReadLine();
                    if (line != null)
                    {
                        string desc = "";
                        desc = line;
                        IntroGreetingSentences.Add(desc);
                    }
                    else
                        fileEmpty = true;
                }
                readFile.Close();
                readFile = null;
            }
            catch (IOException ex)
            {
                Debug.LogError("Could not read file; error:" + ex.ToString());
            }
        }

        public static string GenerateResponse(Character cData, CharacterAction aData, float responseIndex, bool requiresDecision)
        {
            string activeString = "";
            if (requiresDecision)
            {
                if (responseIndex > 65)
                    activeString = GenerateAffirmativeResponse(cData, aData);
                else if (responseIndex > 35)
                    activeString = GenerateUndecidedResponse(cData, aData);
                else
                    activeString = GenerateDeclineResponse(cData, aData);
            }
            else
            {
                if (responseIndex > 65)
                    activeString = GeneratePositiveResponse(cData, aData); // change to positive response
                else if (responseIndex > 35)
                    activeString = GenerateNeutralResponse(cData, aData); // change to negative response
                else
                    activeString = GenerateNegativeResponse(cData, aData); // change to negative response
            }

            return activeString;
        }

        private static string GenerateAffirmativeResponse(Character cData, CharacterAction aData)
        {
            string activeString = "";
            //string completeString = "";
            //int choice = 0;

            activeString = "I will do that, Your Excellence.";

            return activeString; // stub to test reader

        }

        private static string GenerateUndecidedResponse(Character cData, CharacterAction aData)
        {
            string activeString = "";
            //string completeString = "";
            //int choice = 0;

            activeString = "I can't decide at this time, Your Excellence.";

            return activeString; // stub to test reader

        }

        private static string GenerateDeclineResponse(Character cData, CharacterAction aData)
        {
            string activeString = "";
            //string completeString = "";
            //int choice = 0;

            activeString = "I refuse to do that, Your Excellence.";

            return activeString; // stub to test reader

        }

        private static string GeneratePositiveResponse(Character cData, CharacterAction aData)
        {
            string activeString = "";
            //string completeString = "";
            //int choice = 0;

            activeString = "That sounds great, Your Excellence.";

            return activeString; // stub to test reader

        }

        private static string GenerateNeutralResponse(Character cData, CharacterAction aData)
        {
            string activeString = "";
            //string completeString = "";
            //int choice = 0;

            activeString = "I guess that sounds OK, Your Excellence.";

            return activeString; // stub to test reader

        }

        private static string GenerateNegativeResponse(Character cData, CharacterAction aData)
        {
            string activeString = "";
            //string completeString = "";
            //int choice = 0;

            activeString = "That is outrageous, Your Excellence!";

            return activeString; // stub to test reader

        }

        public static string GenerateInitialDialogue(Character cData)
        {
            int choices = IntroGenericSentences.Count;
            int choice = 0;
            int sentenceCount = 0;
            string activeString = "";
            string completeString = "";
            
            // get initial sentence first
            choice = UnityEngine.Random.Range(0, IntroGreetingSentences.Count);

            activeString = IntroGreetingSentences[choice];
            activeString = ParseSentence(activeString, cData);

            completeString += activeString + " ";

            List<int> choiceList = new List<int>();
            sentenceCount = UnityEngine.Random.Range(1, sentenceMax);

            for (int x = 0; x < sentenceCount; x++)
            {        
                if (choices > 0 && choiceList.Count < choices)
                {
                    chooseSentence:
                    choice = UnityEngine.Random.Range(0, choices);
                    if (choiceList.Contains(choice))
                        goto chooseSentence;
                    else
                    {
                        choiceList.Add(choice);
                        activeString = IntroGenericSentences[choice];
                        activeString = ParseSentence(activeString, cData);
                    }          
      
                    completeString += activeString + " ";
                }
                else
                    return "No applicable string found.";
            }

            return completeString;      
        }

        private static string ParseSentence(string activeString, Character cData)
        {
            if (activeString.Contains("[EMPEROR]"))
            {
                string newString;
                newString = activeString.Replace("[EMPEROR]", "Emperor " + HelperFunctions.DataRetrivalFunctions.GetCivLeader(cData.CivID).Name);
                activeString = newString;
            }

            if (activeString.Contains("[PLANET]"))
            {
                string newString;
                newString = activeString.Replace("[PLANET]", HelperFunctions.DataRetrivalFunctions.GetPlanet(cData.PlanetLocationID).Name);
                activeString = newString;
            }

            if (activeString.Contains("[HOUSE]"))
            {
                string newString;
                newString = activeString.Replace("[HOUSE]", cData.AssignedHouse.Name);
                activeString = newString;
            }

            if (activeString.Contains("[SPORTSTEAM]"))
            {
                string newString;
                bool positiveAdjective = false;
                newString = activeString.Replace("[SPORTSTEAM]", SportsTeamNames[UnityEngine.Random.Range(0, SportsTeamNames.Count)]);
                activeString = newString;

                if (activeString.Contains("[SPORTSRESULT"))
                {
                    string result = "";

                    int y = UnityEngine.Random.Range(0, 100);
                    if (y < 40)
                    {
                        result = "WON";
                        positiveAdjective = true;
                    }
                    else if (y < 80)
                    {
                        result = "LOST";
                        positiveAdjective = false;
                    }
                    else
                    {
                        result = "TIED";
                        positiveAdjective = false;
                    }
                    newString = activeString.Replace("[SPORTSRESULT]", result);
                    activeString = newString;
                }

                if (activeString.Contains("[ADJECTIVE]"))
                {

                    string adjective;

                    if (!positiveAdjective)
                    {
                        adjective = NegativeAdjectives[UnityEngine.Random.Range(0, NegativeAdjectives.Count)];
                    }
                    else
                    {
                        adjective = PositiveAdjectives[UnityEngine.Random.Range(0, PositiveAdjectives.Count)];
                    }

                    newString = activeString.Replace("[ADJECTIVE]", adjective);
                    activeString = newString;
                }
            }

            if (activeString.Contains("[ADJECTIVE]"))
            {
                string newString;
                int adjType = UnityEngine.Random.Range(0, 100);
                string adjective;

                if (adjType > 50)
                {
                    adjective = NegativeAdjectives[UnityEngine.Random.Range(0, NegativeAdjectives.Count)];
                }
                else
                {
                    adjective = PositiveAdjectives[UnityEngine.Random.Range(0, PositiveAdjectives.Count)];
                }

                newString = activeString.Replace("[ADJECTIVE]", adjective);
                activeString = newString;
            }

            if (activeString.Contains("[COLOR]"))
            {
                string newString;
                newString = activeString.Replace("[COLOR]", Colors[UnityEngine.Random.Range(0, Colors.Count)]);
                activeString = newString;
            }

            return activeString;
        }
    }
}
