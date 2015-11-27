using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System;
using StellarObjects; // using classes generated for stellar objects
using PlanetObjects; // for tiles
using CharacterObjects; // for characters
using CivObjects; // for civs
using HelperFunctions; // helper functions
using Actions;
using GameEvents;
using ConversationAI;

    public class DataManager
    {
        public static List<House> houseDataList = new List<House>();
        public static List<CharacterAction> characterActionList = new List<CharacterAction>();
        public static List<PlanetTraits> planetTraitDataList = new List<PlanetTraits>();
        public static List<PlanetAttributeData> planetAttributeDataList = new List<PlanetAttributeData>();
        public static Dictionary<GameEvents.GameEvent.eEventType, string> Descriptions = new Dictionary<GameEvents.GameEvent.eEventType, string>();
        public static List<string> systemNameList = new List<string>();
        public static List<string> planetNameList = new List<string>();
        public static List<CharacterTrait> CharacterTraitList = new List<CharacterTrait>(); // master character trait data list
        public static List<string> civNameList = new List<string>();
        public static List<string> civSurNameList = new List<string>();
        public static List<string> characterFemaleFirstNameList = new List<string>();
        public static List<string> characterMaleFirstNameList = new List<string>();
        public static List<string> commonHouseNameList = new List<string>();

        private GlobalGameData gameDataRef;

        public void Awake()
        {
            gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
        }

        public static void InitializeData()
        {
            // clear all lists (for new game setup)
            houseDataList.Clear();
            planetTraitDataList.Clear();
            planetAttributeDataList.Clear();
            CharacterTraitList.Clear();
            characterActionList.Clear();
            systemNameList.Clear();
            planetNameList.Clear();
            civNameList.Clear();
            civSurNameList.Clear();
            characterFemaleFirstNameList.Clear();
            characterMaleFirstNameList.Clear();
            Descriptions.Clear();

            // now read all data anew
            ReadEventDescriptions();
            ReadCharacterTraitXMLFiles();
            ReadActionXMLFiles();
            ReadHouseXMLFiles();
            ReadXMLTraitFiles();
            PopulateObjectNameLists();
            PopulatePlanetTraitTables();
            ConversationEngine.ReadConversationData(); // conversation data is kept in a static AI class
        }

        public static void ReadEventDescriptions()
        {
            // civilization name lists
            try
            {
                string line = null;
                string path = Application.dataPath;
                bool fileEmpty = false;

                System.IO.TextReader readFile = new StreamReader(path + "/Resources/eventDescriptions.txt");
                while (!fileEmpty)
                {
                    line = readFile.ReadLine();
                    if (line != null)
                    {
                        GameEvents.GameEvent.eEventType eType;
                        string desc = "";

                        string[] eventDescriptions = line.Split(new Char[] { ':' });
                        eType = (GameEvents.GameEvent.eEventType)Enum.Parse(typeof(GameEvents.GameEvent.eEventType), eventDescriptions[0]);
                        desc = eventDescriptions[1];
                        Descriptions.Add(eType, desc);
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

        public static void ReadXMLTraitFiles()
        {
            string path = Application.dataPath;
            XmlDocument xmlDoc = new XmlDocument(); // creates the new document
            TextAsset planetData = null;
            planetData = Resources.Load("PlanetTraitXMLData") as TextAsset;  // load the XML file from the Resources folder
            xmlDoc.LoadXml(planetData.text); // and add it to the xmldoc object
            XmlNodeList traitsList = xmlDoc.GetElementsByTagName("Row"); // separate elements by type (trait, in this case)

            foreach (XmlNode traits in traitsList)
            {
                XmlNodeList traitContent = traits.ChildNodes;
                PlanetTraits currentTrait = new PlanetTraits();

                foreach (XmlNode trait in traitContent)
                {
                    if (trait.Name == "ID")
                    {
                        currentTrait.ID = trait.InnerText.ToLower();
                    }
                    if (trait.Name == "NAME")
                    {
                        currentTrait.Name = trait.InnerText.ToLower();
                    }
                    if (trait.Name == "TRAIT_DESC")
                    {
                        currentTrait.Description = trait.InnerText;
                    }
                    if (trait.Name == "CATEGORY")
                    {
                        currentTrait.Category = (PlanetTraits.eTraitCategory)int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "PTYPE_1")
                    {
                        currentTrait.PlanetType[0] = (PlanetData.ePlanetType)int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "PTYPE_2")
                    {
                        currentTrait.PlanetType[1] = (PlanetData.ePlanetType)int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "PTYPE_3")
                    {
                        currentTrait.PlanetType[2] = (PlanetData.ePlanetType)int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "PTYPE_4")
                    {
                        currentTrait.PlanetType[3] = (PlanetData.ePlanetType)int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "PTYPE_5")
                    {
                        currentTrait.PlanetType[4] = (PlanetData.ePlanetType)int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "MOONS_REQ")
                    {
                        currentTrait.MoonsNecessary = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "MIN_SIZE")
                    {
                        currentTrait.MinSize = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "HAB_MOD")
                    {
                        currentTrait.HabMod = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "PROD_MOD")
                    {
                        currentTrait.ProdMod = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "ENG_MOD")
                    {
                        currentTrait.EnergyMod = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "RES_MOD")
                    {
                        currentTrait.ResearchMod = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "CHANCE_DEST")
                    {
                        currentTrait.ChanceDestroyed = float.Parse(trait.InnerText);
                    }
                    if (trait.Name == "CHANCE_TERRA")
                    {
                        currentTrait.ChanceTerraformed = float.Parse(trait.InnerText);
                    }
                    if (trait.Name == "CHANCE_STELLAR_TERRA")
                    {
                        currentTrait.ChanceStellarTerraformed = float.Parse(trait.InnerText);
                    }
                    if (trait.Name == "BELT")
                    {
                        currentTrait.BeltEligible = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "DEST_TEST")
                    {
                        currentTrait.TextWhenDestroyed = trait.InnerText;
                    }
                    if (trait.Name == "TILE_MOD")
                    {
                        currentTrait.HabitableTilesMod = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "OFF_MOD")
                    {
                        currentTrait.AttackMod = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "DEF_MOD")
                    {
                        currentTrait.DefenceMod = int.Parse(trait.InnerText);
                    }
                }

                // add the trait once done
                planetTraitDataList.Add(currentTrait);
            }

        }

        public static void ReadCharacterTraitXMLFiles()
        {
            string path = Application.dataPath;
            GlobalGameData gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();

            XmlDocument xmlDoc = new XmlDocument(); // creates the new document
            TextAsset traitData = null;
            traitData = Resources.Load("Character Trait XML Data") as TextAsset;  // load the XML file from the Resources folder
            xmlDoc.LoadXml(traitData.text); // and add it to the xmldoc object
            XmlNodeList traitList = xmlDoc.GetElementsByTagName("Row"); // separate elements by type (trait, in this case)

            foreach (XmlNode Trait in traitList)
            {
                XmlNodeList traitContent = Trait.ChildNodes;
                CharacterTrait currentTrait = new CharacterTrait();

                foreach (XmlNode trait in traitContent)
                {
                    if (trait.Name == "ID")
                    {
                        currentTrait.ID = trait.InnerText;
                    }
                    if (trait.Name == "Name")
                    {
                        currentTrait.Name = trait.InnerText;
                    }
                    if (trait.Name == "Description")
                    {
                        currentTrait.Description = trait.InnerText;
                    }
                    if (trait.Name == "Opposite_Trait_ID")
                    {
                        currentTrait.OppositeTraitID = trait.InnerText;
                    }
                    if (trait.Name == "Change")
                    {
                        currentTrait.ChangeTendency = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "Goal_Focus")
                    {
                        currentTrait.GoalFocusTendency = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "Wealth")
                    {
                        currentTrait.WealthTendency = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "Pops")
                    {
                        currentTrait.PopsTendency = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "Budget")
                    {
                        currentTrait.BudgetTendency = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "Courage")
                    {
                        currentTrait.CourageTendency = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "Goal_Stability")
                    {
                        currentTrait.GoalStabilityTendency = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "Tax")
                    {
                        currentTrait.TaxTendency = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "Science")
                    {
                        currentTrait.ScienceTendency = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "Gluttony")
                    {
                        currentTrait.GluttonyTendency = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "Learning")
                    {
                        currentTrait.LearningTendency = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "Reserve")
                    {
                        currentTrait.ReserveTendency = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "Trader")
                    {
                        currentTrait.TraderTendency = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "Diplomacy")
                    {
                        currentTrait.DiplomacyTendency = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "Traveler")
                    {
                        currentTrait.TravelerTendency = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "Trust")
                    {
                        currentTrait.TrustTendency = int.Parse(trait.InnerText);
                    }
                    if (trait.Name == "Admin")
                    {
                        currentTrait.AdminTendency = int.Parse(trait.InnerText);
                    }
                }

                // add the trait once done
                CharacterTraitList.Add(currentTrait);
            }
            gameDataRef.CharacterTraitList = CharacterTraitList;
        }

        public static void ReadHouseXMLFiles()
        {
            string path = Application.dataPath;
            GlobalGameData gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();

            XmlDocument xmlDoc = new XmlDocument(); // creates the new document
            TextAsset houseData = null;
            houseData = Resources.Load("HouseTablesXMLData") as TextAsset;  // load the XML file from the Resources folder
            xmlDoc.LoadXml(houseData.text); // and add it to the xmldoc object
            XmlNodeList houseList = xmlDoc.GetElementsByTagName("Row"); // separate elements by type (trait, in this case)

            foreach (XmlNode House in houseList)
            {
                XmlNodeList houseContent = House.ChildNodes;
                House currentHouse = new House();

                foreach (XmlNode house in houseContent)
                {
                    if (house.Name == "ID")
                    {
                        currentHouse.ID = house.InnerText;
                    }
                    if (house.Name == "NAME")
                    {
                        currentHouse.Name = house.InnerText;
                    }
                    if (house.Name == "RANK")
                    {
                        currentHouse.Rank = (House.eHouseRank)int.Parse(house.InnerText);
                    }
                    if (house.Name == "LEADER_TITLE")
                    {
                        currentHouse.LeaderTitle = house.InnerText;
                    }
                    if (house.Name == "YEAR")
                    {
                        currentHouse.LeaderTitle = house.InnerText;
                    }
                    if (house.Name == "HISTORY")
                    {
                        currentHouse.History = house.InnerText;
                    }
                    if (house.Name == "SPECIALITY")
                    {
                        currentHouse.Specialty = (House.eHouseSpecialty)int.Parse(house.InnerText);
                    }
                    if (house.Name == "PERSONALITY")
                    {
                        currentHouse.Personality = (House.eHousePersonality)int.Parse(house.InnerText);
                    }
                    if (house.Name == "STABILITY")
                    {
                        currentHouse.Stability = (House.eHouseStability)int.Parse(house.InnerText);
                    }
                    if (house.Name == "WEALTH")
                    {
                        currentHouse.Wealth = (House.eHouseWealth)int.Parse(house.InnerText);
                    }
                    if (house.Name == "AMBITION")
                    {
                        currentHouse.Ambition = (House.eHouseAmbition)int.Parse(house.InnerText);
                    }
                    if (house.Name == "SCIENCE_TRADITION")
                    {
                        currentHouse.ScienceTradition = int.Parse(house.InnerText);
                    }
                    if (house.Name == "HIGH_TECH_TRADITION")
                    {
                        currentHouse.HighTechTradition = int.Parse(house.InnerText);
                    }
                    if (house.Name == "GOV_TRADITION")
                    {
                        currentHouse.GovernmentTradition = int.Parse(house.InnerText);
                    }
                    if (house.Name == "MAN_TRADITION")
                    {
                        currentHouse.ManufacturingTradition = int.Parse(house.InnerText);
                    }
                    if (house.Name == "WAR_TRADITION")
                    {
                        currentHouse.WarTradition = int.Parse(house.InnerText);
                    }
                    if (house.Name == "TRADE_TRADITION")
                    {
                        currentHouse.TradeTradition = int.Parse(house.InnerText);
                    }
                    if (house.Name == "FARMING_TRADITION")
                    {
                        currentHouse.FarmingTradition = int.Parse(house.InnerText);
                    }
                    if (house.Name == "MINING_TRADITION")
                    {
                        currentHouse.MiningTradition = int.Parse(house.InnerText);
                    }
                    if (house.Name == "SPECIALTY")
                    {
                        currentHouse.Specialty = (House.eHouseSpecialty)int.Parse(house.InnerText);
                    }
                }

                // add the trait once done
                houseDataList.Add(currentHouse);
            }
            gameDataRef.HouseList = houseDataList;
        }

        public static void ReadActionXMLFiles()
        {
            string path = Application.dataPath;
            GlobalGameData gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();

            XmlDocument xmlDoc = new XmlDocument(); // creates the new document
            TextAsset actionData = null;
            actionData = Resources.Load("Action XML Data") as TextAsset;  // load the XML file from the Resources folder
            xmlDoc.LoadXml(actionData.text); // and add it to the xmldoc object
            XmlNodeList actionList = xmlDoc.GetElementsByTagName("Row"); // separate elements by type (trait, in this case)

            foreach (XmlNode Action in actionList)
            {
                XmlNodeList actionContent = Action.ChildNodes;
                CharacterAction currentAction = new CharacterAction();

                foreach (XmlNode action in actionContent)
                {
                    if (action.Name == "ID")
                    {
                        currentAction.ID = action.InnerText;
                    }
                    if (action.Name == "NAME")
                    {
                        currentAction.Name = action.InnerText;
                    }
                    if (action.Name == "RANK")
                    {
                        currentAction.Category = (CharacterAction.eType)int.Parse(action.InnerText);
                    }
                    if (action.Name == "DESCRIPTION")
                    {
                        currentAction.Description = action.InnerText;
                    }
                    if (action.Name == "VIC_E")
                    {
                        currentAction.ViceroyValid = bool.Parse(action.InnerText);
                    }
                    if (action.Name == "SYSGOV_E")
                    {
                        currentAction.SysGovValid = bool.Parse(action.InnerText);
                    }
                    if (action.Name == "PROGOV_E")
                    {
                        currentAction.ProvGovValid = bool.Parse(action.InnerText);
                    }
                    if (action.Name == "PRIME_E")
                    {
                        currentAction.PrimeValid = bool.Parse(action.InnerText);
                    }
                    if (action.Name == "ALL_E")
                    {
                        currentAction.AllValid = bool.Parse(action.InnerText);
                    }
                    if (action.Name == "INQ_E")
                    {
                        currentAction.InquisitorValid = bool.Parse(action.InnerText);
                    }
                    if (action.Name == "EMP_NEAR")
                    {
                        currentAction.EmperorNearValid = bool.Parse(action.InnerText);
                    }
                    if (action.Name == "EMP_ACTION")
                    {
                        currentAction.EmperorAction = bool.Parse(action.InnerText);
                    }
                }

                // add the trait once done
                characterActionList.Add(currentAction);
            }
            gameDataRef.CharacterActionList = characterActionList;
        }

        public static void PopulatePlanetTraitTables()
        {
            try
            {
                string line = null;
                string path = Application.dataPath;
                bool fileEmpty = false;

                System.IO.TextReader readFile = new StreamReader(path + "/Resources/planetTraitsData.txt");

                while (!fileEmpty) // until the line hits a null object
                {
                    line = readFile.ReadLine();

                    if (line != null)
                    {
                        if (!line.StartsWith("TYPE"))
                        {
                            PlanetAttributeData pAttributeData = new PlanetAttributeData();
                            PlanetAttributeTable pAttributeTable = new PlanetAttributeTable();
                            string[] planetBaseTraitsString = line.Split(new Char[] { ',' });

                            //pull out each modifier
                            pAttributeData.planetType = (PlanetData.ePlanetType)int.Parse(planetBaseTraitsString[0]);
                            pAttributeTable.size = int.Parse(planetBaseTraitsString[1]);
                            pAttributeTable.sizeVar = int.Parse(planetBaseTraitsString[2]);
                            pAttributeTable.habitability = int.Parse(planetBaseTraitsString[3]);
                            pAttributeTable.habVar = int.Parse(planetBaseTraitsString[4]);
                            pAttributeTable.indMult = float.Parse(planetBaseTraitsString[5]);
                            pAttributeTable.ringChance = int.Parse(planetBaseTraitsString[6]);
                            pAttributeTable.moonChance = int.Parse(planetBaseTraitsString[7]);
                            pAttributeTable.maxMoons = int.Parse(planetBaseTraitsString[8]);
                            pAttributeTable.alpMaterial = int.Parse(planetBaseTraitsString[9]);
                            pAttributeTable.alpVar = int.Parse(planetBaseTraitsString[10]);
                            pAttributeTable.heavyMaterial = int.Parse(planetBaseTraitsString[11]);
                            pAttributeTable.heavVar = int.Parse(planetBaseTraitsString[12]);
                            pAttributeTable.rareMaterial = int.Parse(planetBaseTraitsString[13]);
                            pAttributeTable.rareVar = int.Parse(planetBaseTraitsString[14]);
                            pAttributeTable.energy = int.Parse(planetBaseTraitsString[15]);
                            for (int x = 0; x < 8; x++)
                            {
                                pAttributeTable.validRegions[x] = int.Parse(planetBaseTraitsString[16 + x]);
                            }

                            // add the table to the data object and then add to the attribute data list
                            pAttributeData.planetTraitsTable = pAttributeTable;
                            planetAttributeDataList.Add(pAttributeData);
                        }

                        else
                        {
                            continue;
                        }
                    }

                    else
                        fileEmpty = true;
                }

                readFile.Close();
                readFile = null;
                Debug.Log("Planet Attribute Tables successfully read!");
            }
            catch (IOException ex)
            {
                Debug.LogError("Could not read Planet Attribute Table file; error:" + ex.ToString());
            }
        }

        public static void PopulateObjectNameLists()
        {
            // planet name lists
            try
            {
                string line = null;
                string path = Application.dataPath;
                bool fileEmpty = false;

                System.IO.TextReader readFile = new StreamReader(path + "/Resources/systemNames.txt"); // change to planet names when file is added!
                while (!fileEmpty)
                {
                    line = readFile.ReadLine();
                    if (line != null)
                    {
                        planetNameList.Add(line);
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

            // system name lists
            try
            {
                string line = null;
                string path = Application.dataPath;
                bool fileEmpty = false;

                System.IO.TextReader readFile = new StreamReader(path + "/Resources/systemNames.txt");
                while (!fileEmpty)
                {
                    line = readFile.ReadLine();
                    if (line != null)
                    {
                        systemNameList.Add(line);
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

            // civilization name lists
            try
            {
                string line = null;
                string path = Application.dataPath;
                bool fileEmpty = false;

                System.IO.TextReader readFile = new StreamReader(path + "/Resources/civNames.txt");
                while (!fileEmpty)
                {
                    line = readFile.ReadLine();
                    if (line != null)
                    {
                        civNameList.Add(line);
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

            // civliization surname lists
            try
            {
                string line = null;
                string path = Application.dataPath;
                bool fileEmpty = false;

                System.IO.TextReader readFile = new StreamReader(path + "/Resources/civSurNames.txt");
                while (!fileEmpty)
                {
                    line = readFile.ReadLine();
                    if (line != null)
                    {
                        civSurNameList.Add(line);
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

            // character name lists
            try
            {
                string line = null;
                string path = Application.dataPath;
                bool fileEmpty = false;

                System.IO.TextReader readFile = new StreamReader(path + "/Resources/commonHouseNameList.txt");
                while (!fileEmpty)
                {
                    line = readFile.ReadLine();
                    if (line != null)
                    {
                        commonHouseNameList.Add(line);
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

                System.IO.TextReader readFile = new StreamReader(path + "/Resources/characterFirstNamesFemale.txt");
                while (!fileEmpty)
                {
                    line = readFile.ReadLine();
                    if (line != null)
                    {
                        characterFemaleFirstNameList.Add(line);
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

                System.IO.TextReader readFile = new StreamReader(path + "/Resources/characterFirstNamesMale.txt");
                while (!fileEmpty)
                {
                    line = readFile.ReadLine();
                    if (line != null)
                    {
                        characterMaleFirstNameList.Add(line);
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
    }

