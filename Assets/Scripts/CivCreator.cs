using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// object namespaces
using StellarObjects;
using PlanetObjects;
using CivObjects;
using CharacterObjects;

// classes
using CameraScripts;
using Assets.Scripts.States;

namespace CivCreator
{
    public class CivCreator : MonoBehaviour
    {
        private GalaxyData gData;
        private GlobalGameData gameDataRef;

        void Start()
        {
            gData = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
            // now generate civilizations
            GenerateHumanCiv();
            GenerateAICivs();
            GenerateCommonHouses(); // generate houses
            GenerateHouseStats(); // generate the other house status
            GenerateCharacters(); // generate characters for the global pool
            GenerateCivLeaders();
            AssignHouses(); // assign houses
            GenerateRelationships(); // determine everyone's relationship to everyone else
            GenerateProvinces(); // generate provinces for each civilization
            DetermineSystemCapitals();
            GenerateStellarObjectLeaders(); // test; put back after generate primes if needed
            GenerateInfrastructure();
            SetPlanetTaxes();
            GeneratePrimes();                  
            GeneratePlanetIntelLevels(gameDataRef.CivList.Find(p => p.HumanCiv == true));
            gameDataRef.CivsGenerated = true; // sends flag to move to the galaxy screen
        }

        void GenerateAICivs()
        {
            for (int x = 0; x < gameDataRef.NumberOfAICivs; x++)
            {
                Civilization newCiv = new Civilization();
                newCiv = GenerateGameObject.CreateNewCivilization(); // may need to add parameters
                gameDataRef.AddNewCivilization(newCiv);
            }
        }

        void GenerateCharacters()
        {
            foreach (Civilization civ in gameDataRef.CivList)
            {
                int poolCount = UnityEngine.Random.Range(120 - ((int)civ.Size * 5), 120); // generate from 60-120 characters initially
                for (int x = 0; x < poolCount; x++)
                {
                    gameDataRef.CharacterList.Add(GenerateGameObject.GenerateNewCharacter(CharacterObjects.Character.eRole.Pool, civ.ID)); // assign the character to the global list
                }
            }
        }
        
        void GenerateRelationships()
        {
            Civilization humanCiv = gameDataRef.CivList[0]; // only the player empire has Houses
            List<Character> civCharList = new List<Character>();
            civCharList = gameDataRef.CharacterList.FindAll(p => p.CivID == humanCiv.ID);

            foreach (Character cData in civCharList)
            {
                foreach (Character otherCData in civCharList)
                {
                    float baseTrust = 0f;
                    float baseFear = 0f;
                    float trustModifier = 0f;
                    float fearModifier = 0f;

                    Relationship newRelationship = new Relationship();

                    if (cData.ID != otherCData.ID) // no entry for yourself!
                    {
                        if (cData.HouseID == otherCData.HouseID)
                        {
                            trustModifier += 70; // if you are in the same house, huge bonus
                            fearModifier -= 50;
                        }
                        else if (cData.AssignedHouse.Personality == otherCData.AssignedHouse.Personality)
                        {
                            trustModifier += 30; // if not in the same house, bonus if you share personalities
                            fearModifier -= 15;
                        }
                        else
                        {
                            trustModifier -= 30;
                            fearModifier += 10 * (cData.AssignedHouse.Rank - otherCData.AssignedHouse.Rank);  // more fearful of people from houses of higher rank
                        }

                        if (otherCData.Role == Character.eRole.Emperor) // adjustments for the Emperor's role (he is always feared a little)
                        {
                            trustModifier -= 15;
                            fearModifier += 10;
                        }

                        baseTrust = UnityEngine.Random.Range(0, 80);
                        baseFear = UnityEngine.Random.Range(0, 60);

                        baseTrust += trustModifier;
                        baseFear += fearModifier;

                        // adjust existing relationship pairs appropriately if needed
                        if (otherCData.NewRelationships.ContainsKey(cData.ID))
                        {
                            if (otherCData.NewRelationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Challengee)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Challenger;
                                baseTrust -= 20;
                            }

                            else if (otherCData.NewRelationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Challenger)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Challengee;
                                baseTrust -= 20;
                            }

                            else if (otherCData.NewRelationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Predator)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Prey;
                                baseTrust -= 40;
                                baseFear += 50;
                            }

                            else if (otherCData.NewRelationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Allies)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Allies;
                                baseTrust += 80;
                                baseFear -= 80;
                            }

                            else if (otherCData.NewRelationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Friends)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Friends;
                                baseTrust += 60;
                                baseFear -= 60;
                            }

                            else if (otherCData.NewRelationships[cData.ID].RelationshipState == Relationship.eRelationshipState.HangerOn)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.HungUpon;
                                baseFear -= 40;
                            }

                            else if (otherCData.NewRelationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Lovers)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Lovers;
                                baseFear -= 50;
                                baseTrust += 30;
                            }

                            else if (otherCData.NewRelationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Married)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Married;
                                baseFear -= 70;
                                baseTrust += 70;
                            }

                            else if (otherCData.NewRelationships[cData.ID].RelationshipState == Relationship.eRelationshipState.HungUpon)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.HangerOn;
                                baseTrust += 40;
                            }

                            else if (otherCData.NewRelationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Shunning)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Shunned;
                                baseTrust -= 20;
                            }

                            else if (otherCData.NewRelationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Inferior)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Superior;
                                baseFear -= 30;
                            }

                            else if (otherCData.NewRelationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Superior)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Inferior;
                                baseFear += 30;
                            }

                            else if (otherCData.NewRelationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Rivals)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Rivals;
                                baseTrust -= 50;
                                baseFear -= 20;
                            }

                            else if (otherCData.NewRelationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Vendetta)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Vendetta;
                                baseTrust -= 70;
                            }

                            else if (otherCData.NewRelationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Vengeance)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.VengeanceUpon;
                                baseTrust -= 40;
                                baseFear += 60;
                            }

                            else if (otherCData.NewRelationships[cData.ID].RelationshipState == Relationship.eRelationshipState.VengeanceUpon)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Vengeance;
                                baseTrust -= 90;
                            }

                            else if (otherCData.NewRelationships[cData.ID].RelationshipState == Relationship.eRelationshipState.Neutral)
                            {
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Neutral;
                            }

                            else
                            {
                                if (baseTrust > 60)
                                {
                                    int chance = UnityEngine.Random.Range(0, 3);
                                    if (chance == 0)
                                        newRelationship.RelationshipState = Relationship.eRelationshipState.Allies;
                                    else if (chance <= 2)
                                        newRelationship.RelationshipState = Relationship.eRelationshipState.Friends;
                                    else
                                        newRelationship.RelationshipState = Relationship.eRelationshipState.Neutral;
                                }

                                else if (baseTrust < 30)
                                {
                                    int chance = UnityEngine.Random.Range(0, 5);
                                    if (chance == 0)
                                        newRelationship.RelationshipState = Relationship.eRelationshipState.Rivals;
                                    else if (chance == 1)
                                        newRelationship.RelationshipState = Relationship.eRelationshipState.Vendetta;
                                    else if (chance == 2)
                                        newRelationship.RelationshipState = Relationship.eRelationshipState.Vengeance;
                                }
                                else
                                    newRelationship.RelationshipState = Relationship.eRelationshipState.Neutral;
                            }
                        }
                        else // if no existing states occur, or no reciprocal relationship yet, create one based on trust and fear factors
                        {
                            if (baseTrust > 70)
                            {
                                int chance = UnityEngine.Random.Range(0,15);
                                if (chance <= 2)
                                    newRelationship.RelationshipState = Relationship.eRelationshipState.Allies;
                                else if (chance <= 7)
                                    newRelationship.RelationshipState = Relationship.eRelationshipState.Friends;
                                else if (chance <= 8)
                                    newRelationship.RelationshipState = Relationship.eRelationshipState.Lovers;
                                else if (chance <= 9 && cData.Gender != otherCData.Gender) // no same-sex marriages here
                                    newRelationship.RelationshipState = Relationship.eRelationshipState.Married;
                                else
                                    newRelationship.RelationshipState = Relationship.eRelationshipState.Neutral;
                            }

                            else if (baseTrust < 30)
                            {
                                int chance = UnityEngine.Random.Range(0, 6) - (int)(baseTrust / 10);
                                if (chance == 0)
                                    if (cData.Influence < (otherCData.Influence * 1.5f))
                                        newRelationship.RelationshipState = Relationship.eRelationshipState.Predator;
                                    else if (cData.Influence > (otherCData.Influence * 1.5f))
                                        newRelationship.RelationshipState = Relationship.eRelationshipState.Prey;
                                    else
                                        newRelationship.RelationshipState = Relationship.eRelationshipState.Rivals;
                                else if (chance == 1)
                                    newRelationship.RelationshipState = Relationship.eRelationshipState.Vendetta;
                                else if (chance == 2)
                                    newRelationship.RelationshipState = Relationship.eRelationshipState.Vengeance;
                                else if (chance == 3)
                                    newRelationship.RelationshipState = Relationship.eRelationshipState.Shunning;
                                else
                                    newRelationship.RelationshipState = Relationship.eRelationshipState.Neutral;
                            }
                            else
                                newRelationship.RelationshipState = Relationship.eRelationshipState.Neutral; // UnityEngine.Random.Range(0, 21);
                        }

                        newRelationship.Trust = baseTrust;
                        newRelationship.Fear = baseFear;                   
                        cData.NewRelationships.Add(otherCData.ID, newRelationship);
                    }                   
                }
            }
        }
       

        void GenerateCivLeaders()
        {
            foreach (Civilization civ in gameDataRef.CivList)
            {
                List<Character> civCharList = new List<Character>();
                civCharList = gameDataRef.CharacterList.FindAll(p => p.CivID == civ.ID);

            tryAssignLeader:
                int choice = UnityEngine.Random.Range(0, civCharList.Count);
                if (civCharList[choice].Role == Character.eRole.Pool)
                {
                    civCharList[choice].Role = Character.eRole.Emperor;
                    civCharList[choice].BaseActionPoints = UnityEngine.Random.Range(3, 7); // temporary
                    if (civ.HumanCiv)
                    {
                        civCharList[choice].BenevolentInfluence = UnityEngine.Random.Range(1f, 5f);
                        civCharList[choice].PragmaticInfluence = UnityEngine.Random.Range(5f, 10f);
                        civCharList[choice].TyrannicalInfluence = UnityEngine.Random.Range(1f, 5f);
                    }
                    civCharList[choice].PlanetLocationID = civ.CapitalPlanetID;
                    civ.LeaderID = civCharList[choice].ID;
                }
                else // if already assigned, try again
                {
                    goto tryAssignLeader;
                }
            }
        }

        void GenerateCommonHouses()
        {

            int houseNumber = 0; // for ID generation

            foreach (string name in DataManager.commonHouseNameList)
            {
                int creationChance = UnityEngine.Random.Range(0, 100);
                if (creationChance > 90) // only one out of 5 houses will be generated
                {
                    House comHouse = new House(); // generate temp house
                    houseNumber += 1;
                    comHouse.Name = name;
                    comHouse.ID = "COM" + houseNumber.ToString("N0");
                    comHouse.Rank = House.eHouseRank.Common;
                    
                    comHouse.Power = UnityEngine.Random.Range(0, 10); // power from 1-10
                    comHouse.Influence = UnityEngine.Random.Range(0, 10); // influence from 1-10
                    comHouse.Age = UnityEngine.Random.Range(20, 500); // age of the house
                    comHouse.FarmingTradition = UnityEngine.Random.Range(0, 20);
                    comHouse.GovernmentTradition = UnityEngine.Random.Range(0, 10);
                    comHouse.HighTechTradition = UnityEngine.Random.Range(0, 10);
                    comHouse.ManufacturingTradition = UnityEngine.Random.Range(0, 15);
                    comHouse.MiningTradition = UnityEngine.Random.Range(0, 10);
                    comHouse.ScienceTradition = UnityEngine.Random.Range(0, 10);
                    comHouse.TradeTradition = UnityEngine.Random.Range(0, 10);
                    comHouse.WarTradition = UnityEngine.Random.Range(0, 10);
                    comHouse.Specialty = (House.eHouseSpecialty)UnityEngine.Random.Range(0, 7);
                    comHouse.History = "The common House of " + name + " has an undistinguished tradition. Established to no fanfare in the year " + (3050 - comHouse.Age).ToString("N0") + ", the House has continued as unremarkably as it started.";
                    Debug.Log("Common House of " + name + " Created");
                    gameDataRef.HouseList.Add(comHouse);
                }
            }
        }

        void GenerateHouseStats()
        {
            //Civilization humanCiv = gameDataRef.CivList[0]; // only the player empire has Houses
            List<House> greatHouseList = gameDataRef.HouseList.FindAll(p => p.Rank == House.eHouseRank.Great);

            foreach (House gHouse in greatHouseList)
            {
                gHouse.Influence = UnityEngine.Random.Range(40, 80); // temporary code; will be derived once Holdings and house Wealth are generated
            }
        }

        void AssignHouses()
        {
            Civilization humanCiv = gameDataRef.CivList[0]; // only the player empire has Houses
            List<Character> civCharList = new List<Character>();
            List<House>commonHouseList = gameDataRef.HouseList.FindAll(p => p.Rank == House.eHouseRank.Common);
            List<House> minorHouseList = gameDataRef.HouseList.FindAll(p => p.Rank == House.eHouseRank.Minor);
            List<House>greatHouseList = gameDataRef.HouseList.FindAll(p => p.Rank == House.eHouseRank.Great);
            civCharList = gameDataRef.CharacterList.FindAll(p => p.CivID == humanCiv.ID);

            foreach (Character cData in civCharList)
            {
                int houseTypeChance = UnityEngine.Random.Range(0, 100);

                if (houseTypeChance < Constants.Constants.CommonHouseChance)
                {
                    int houseAssignment = UnityEngine.Random.Range(0, commonHouseList.Count);
                    cData.HouseID = commonHouseList[houseAssignment].ID;
                    cData.Wealth = ((int)(commonHouseList[houseAssignment].Wealth + 1) * UnityEngine.Random.Range(1, 4)) / 25; // set wealth based on house
                }
                else if (houseTypeChance < Constants.Constants.MinorHouseChance + Constants.Constants.CommonHouseChance)
                {
                    int houseAssignment = UnityEngine.Random.Range(0, minorHouseList.Count);
                    cData.HouseID = minorHouseList[houseAssignment].ID;
                    cData.Wealth = ((int)(minorHouseList[houseAssignment].Wealth + 1) * UnityEngine.Random.Range(1, 7)) / 25; // set wealth based on house
                }
                else
                {
                    int houseAssignment = UnityEngine.Random.Range(0, greatHouseList.Count);
                    cData.HouseID = greatHouseList[houseAssignment].ID;
                    cData.Wealth = ((int)(greatHouseList[houseAssignment].Wealth + 1) * UnityEngine.Random.Range(1, 15)) / 25; // set wealth based on house
                }
                string creationType = "born to";
                if (cData.Lifeform == Character.eLifeformType.Machine || cData.Lifeform == Character.eLifeformType.Hybrid || cData.Lifeform == Character.eLifeformType.AI)
                {
                    creationType = "designed and manufactured for";
                }
                cData.History += cData.GenderPronoun + " was " + creationType + " the " + cData.AssignedHouse.Rank.ToString() + " House of " + cData.AssignedHouse.Name + ". ";

                if (cData.Traits.Exists(p=> p.Name.ToUpper() == "AVARICIOUS"))
                {
                    cData.Wealth = cData.Wealth * 5;
                }

                if (cData.Traits.Exists(p => p.Name.ToUpper() == "ASCETIC"))
                {
                    cData.Wealth = cData.Wealth / 20;
                }
            }
        }

        void GeneratePrimes()
        {
            Civilization humanCiv = gameDataRef.CivList[0]; // only the player empire has primes
            List<Character> civCharList = new List<Character>();

            civCharList = gameDataRef.CharacterList.FindAll(p => p.CivID == humanCiv.ID);

            tryAssignDomPrime:
                int choice = UnityEngine.Random.Range(0, civCharList.Count);
                if (civCharList[choice].Role == Character.eRole.Pool)
                {
                    civCharList[choice].Role = Character.eRole.DomesticPrime;
                    civCharList[choice].TimeInPosition = UnityEngine.Random.Range(0, (civCharList[choice].Age - 18));
                    civCharList[choice].PlanetLocationID = humanCiv.CapitalPlanetID;
                }
                else // if already assigned, try again
                {
                    goto tryAssignDomPrime;
                }

            tryAssignIntPrime:
                int intChoice = UnityEngine.Random.Range(0, civCharList.Count);
                if (civCharList[intChoice].Role == Character.eRole.Pool)
                {
                    civCharList[intChoice].Role = Character.eRole.IntelPrime;
                    civCharList[choice].TimeInPosition = UnityEngine.Random.Range(0,(civCharList[choice].Age - 18));
                    civCharList[intChoice].PlanetLocationID = humanCiv.CapitalPlanetID;
                }
                else // if already assigned, try again
                {
                    goto tryAssignIntPrime;
                }
        }

        void GenerateStellarObjectLeaders()
        {
            foreach (Civilization civ in gameDataRef.CivList)
            {
                List<PlanetData> pDataList = new List<PlanetData>();
                List<Province> provDataList = new List<Province>();
                List<StarData> sDataList = new List<StarData>();
                List<Character> civCharList = new List<Character>();

                // create the 3 character lists so that they can be weighted when they are chosen against
                List<Character> civGreatHouseCharacterList = new List<Character>();
                List<Character> civMinorHouseCharacterList = new List<Character>();
                List<Character> civCommonHouseCharacterList = new List<Character>();

                pDataList = HelperFunctions.DataRetrivalFunctions.GetCivPlanetList(civ);             
                civCharList = gameDataRef.CharacterList.FindAll(p => p.CivID == civ.ID);
                provDataList = HelperFunctions.DataRetrivalFunctions.GetCivProvinceList(civ);
                sDataList = HelperFunctions.DataRetrivalFunctions.GetCivSystemList(civ);

                if (civ.HumanCiv)
                {
                    civGreatHouseCharacterList = civCharList.FindAll(p => HelperFunctions.DataRetrivalFunctions.GetHouse(p.HouseID).Rank == House.eHouseRank.Great);
                    civMinorHouseCharacterList = civCharList.FindAll(p => HelperFunctions.DataRetrivalFunctions.GetHouse(p.HouseID).Rank == House.eHouseRank.Minor);
                    civCommonHouseCharacterList = civCharList.FindAll(p => HelperFunctions.DataRetrivalFunctions.GetHouse(p.HouseID).Rank == House.eHouseRank.Common);
                }

                // assign a province governor to each province
                if (civ.HumanCiv == true)
                {
                    foreach (Province pData in provDataList)
                    {
                 
                    tryAssignCharacter:
                        int houseType = UnityEngine.Random.Range(0, 100);
                        int choice = 0;
                        Character provGov = null;

                        if (houseType < Constants.Constants.ChanceProvinceGovernorGreat)
                        {
                            choice = UnityEngine.Random.Range(0, civGreatHouseCharacterList.Count);
                            provGov = civGreatHouseCharacterList[choice];
                        }
                        else if (houseType < Constants.Constants.ChanceProvinceGovernorGreat + Constants.Constants.ChanceProvinceGovernorMinor)
                        {
                            choice = UnityEngine.Random.Range(0, civMinorHouseCharacterList.Count);
                            provGov = civMinorHouseCharacterList[choice];
                        }
                        else
                        {
                            choice = UnityEngine.Random.Range(0, civCommonHouseCharacterList.Count);
                            provGov = civCommonHouseCharacterList[choice];
                        }

                        if (provGov.Role == Character.eRole.Pool)
                        {
                            provGov.Role = Character.eRole.ProvinceGovernor;
                            provGov.PlanetLocationID = pData.CapitalPlanetID;
                            provGov.ProvinceAssignedID = pData.ID;
                            provGov.TimeInPosition = UnityEngine.Random.Range(0, (provGov.Age - 18));
                            provGov.History += "In " + (gameDataRef.GameDate - provGov.TimeInPosition).ToString("G0") + ", " + provGov.GenderPronoun.ToLower() +" was assigned as provincial governor of the " + pData.Name + " province.";
                            Debug.Log("Province governor assigned to " + pData.Name + " province.");
                        }
                        else // if already assigned, try again
                        {
                            goto tryAssignCharacter;
                        }
                    }
                }
                else
                {
                    foreach (Province pData in provDataList)
                    {
                    tryAssignCharacter:
                        int choice = UnityEngine.Random.Range(0, civCharList.Count);
                        if (civCharList[choice].Role == Character.eRole.Pool)
                        {
                            civCharList[choice].Role = Character.eRole.ProvinceGovernor;
                            civCharList[choice].PlanetLocationID = pData.CapitalPlanetID;
                            civCharList[choice].ProvinceAssignedID = pData.ID;
                            civCharList[choice].History += civCharList[choice].GenderPronoun + " was assigned as provincial governor of the " + pData.Name + " province.";
                            Debug.Log("Province governor assigned to " + pData.Name + " province.");
                        }
                        else
                        {
                            goto tryAssignCharacter;
                        }
                    }
                }

                // assign a system governor to each province
                if (civ.HumanCiv == true)
                {
                    foreach (StarData sData in sDataList)
                    {

                    tryAssignCharacter:
                        int houseType = UnityEngine.Random.Range(0, 100);
                        int choice = 0;
                        Character sysGov = null;

                        if (houseType < Constants.Constants.ChanceSystemGovernorGreat)
                        {
                            choice = UnityEngine.Random.Range(0, civGreatHouseCharacterList.Count);
                            sysGov = civGreatHouseCharacterList[choice];
                        }
                        else if (houseType < Constants.Constants.ChanceSystemGovernorGreat + Constants.Constants.ChanceSystemGovernorMinor)
                        {
                            choice = UnityEngine.Random.Range(0, civMinorHouseCharacterList.Count);
                            sysGov = civMinorHouseCharacterList[choice];
                        }
                        else
                        {
                            choice = UnityEngine.Random.Range(0, civCommonHouseCharacterList.Count);
                            sysGov = civCommonHouseCharacterList[choice];
                        }

                        if (sysGov.Role == Character.eRole.Pool)
                        {
                            sysGov.Role = Character.eRole.SystemGovernor;
                            sysGov.PlanetLocationID = sData.SystemCapitalID;
                            sysGov.SystemAssignedID = sData.ID;
                            sysGov.TimeInPosition = UnityEngine.Random.Range(0, (sysGov.Age - 18));
                            sysGov.History += "In " + (gameDataRef.GameDate - UnityEngine.Random.Range(0, (sysGov.Age - 18))).ToString("G0") + ", " + sysGov.GenderPronoun.ToLower() + " was assigned as system governor of the " + sData.Name + " system.";
                            Debug.Log("System governor assigned to " + sData.Name);
                        }
                        else // if already assigned, try again
                        {
                            goto tryAssignCharacter;
                        }
                    }
                }
                else
                {
                    foreach (StarData sData in sDataList)
                    {
                    tryAssignCharacter:
                        int choice = UnityEngine.Random.Range(0, civCharList.Count);
                        if (civCharList[choice].Role == Character.eRole.Pool)
                        {
                            civCharList[choice].Role = Character.eRole.SystemGovernor;
                            civCharList[choice].PlanetLocationID = sData.SystemCapitalID;
                            civCharList[choice].SystemAssignedID = sData.ID;
                            Debug.Log("System governor assigned to " + sData.Name);
                        }
                        else
                        {
                            goto tryAssignCharacter;
                        }
                    }
                }

                // assign a viceroy to each planet
                if (civ.HumanCiv == true)
                {
                    foreach (PlanetData pData in pDataList)
                    {

                    tryAssignCharacter:
                        int houseType = UnityEngine.Random.Range(0, 100);
                        int choice = 0;
                        Character viceroy = null;

                        if (houseType < Constants.Constants.ChanceViceroyGreat)
                        {
                            choice = UnityEngine.Random.Range(0, civGreatHouseCharacterList.Count);
                            viceroy = civGreatHouseCharacterList[choice];
                        }
                        else if (houseType < Constants.Constants.ChanceViceroyGreat + Constants.Constants.ChanceViceroyMinor)
                        {
                            choice = UnityEngine.Random.Range(0, civMinorHouseCharacterList.Count);
                            viceroy = civMinorHouseCharacterList[choice];
                        }
                        else
                        {
                            choice = UnityEngine.Random.Range(0, civCommonHouseCharacterList.Count);
                            viceroy = civCommonHouseCharacterList[choice];
                        }

                        if (viceroy.Role == Character.eRole.Pool)
                        {
                            viceroy.Role = Character.eRole.Viceroy;
                            viceroy.PlanetLocationID = pData.ID;
                            viceroy.PlanetAssignedID = pData.ID;
                            pData.OverdriveMultiplier = 1; // initially assigned, test, remove eventually (this will be determined internally by the viceroy build plan)
                            viceroy.TimeInPosition = UnityEngine.Random.Range(0, (viceroy.Age - 18));
                            viceroy.History += "In " + (gameDataRef.GameDate - UnityEngine.Random.Range(0, (viceroy.Age - 18))).ToString("G0") + ", " + viceroy.GenderPronoun.ToLower() + " was assigned as planetary viceroy of " + pData.Name + ".";
                            Debug.Log("Viceroy assigned to " + pData.Name);
                        }
                        else // if already assigned, try again
                        {
                            goto tryAssignCharacter;
                        }
                    }
                }
                else
                {
                    foreach (PlanetData pData in pDataList)
                    {
                    tryAssignCharacter:
                        int choice = UnityEngine.Random.Range(0, civCharList.Count);
                        if (civCharList[choice].Role == Character.eRole.Pool)
                        {
                            civCharList[choice].Role = Character.eRole.Viceroy;
                            civCharList[choice].PlanetLocationID = pData.ID;
                            civCharList[choice].PlanetAssignedID = pData.ID;
                            civCharList[choice].History += "In " + (gameDataRef.GameDate - UnityEngine.Random.Range(0, (civCharList[choice].Age - 18))).ToString("G0") + ", " + civCharList[choice].GenderPronoun.ToLower() + " was assigned as planetary viceroy of " + pData.Name + ".";
                        }
                        else
                        {
                            goto tryAssignCharacter;
                        }
                    }
                }
            }
        }

        void SetPlanetTaxes()
        {
            foreach (Civilization civ in gameDataRef.CivList)
            {
                List<PlanetData> pDataList = new List<PlanetData>();
                pDataList = HelperFunctions.DataRetrivalFunctions.GetCivPlanetList(civ);

                foreach (PlanetData pData in pDataList)
                {
                    pData.PercentGPPForTax = UnityEngine.Random.Range(.15f, .25f);
                    pData.PercentGPPForImports = UnityEngine.Random.Range(.25f, Constants.Constants.MaxGPPAllocatedForImports);
                    pData.PercentGPPForTrade = UnityEngine.Random.Range(.1f, Constants.Constants.MaxGPPAllocatedForTrade);
                    pData.CommerceTax = UnityEngine.Random.Range(.3f, .5f);
                    pData.FoodRetailPercentHold = UnityEngine.Random.Range(.1f, .4f);
                }
            }
        }

        void GenerateInfrastructure()
        {
            foreach (Civilization civ in gameDataRef.CivList)
            {
                List<PlanetData> pDataList = new List<PlanetData>();
                pDataList = HelperFunctions.DataRetrivalFunctions.GetCivPlanetList(civ);

                foreach (PlanetData pData in pDataList)
                {
                    GenerateGameObject.AddDevelopmentToPlanet(pData); // add industrial development
                    GenerateGameObject.GenerateRegionInfrastructure(pData); // now add urban development
                    GenerateGameObject.AddPopsToPlanet(pData, civ); // finally, add pops
                    GenerateGameObject.AddStockpilesToPlanet(pData); // and add stock levels
                    GenerateGameObject.AddTradeInfrastructureToPlanet(pData); // add add starbases and trade infrastructure
                    pData.BuildPlan.Status = BuildPlan.eBuildPlanStatus.Pending; // set build plan status to pending
                }
            }
        }

        void GenerateHumanCiv()
        {
            Civilization newCiv = new Civilization();
            newCiv.Name = gameDataRef.PlayerEmpireName;
            newCiv.Color = Color.cyan; // the color of the empire's system and planet holdings
            newCiv.Type = Civilization.eCivType.PlayerEmpire;  // default empire
            newCiv.CivMaxProvinceSize = 5;
            newCiv.AdminRating = 18; // rating that determines maximum size of governable provinces
            newCiv.Treasury = Random.Range(1000000f, 5000000f);  // starting treasury
            newCiv.Size = Civilization.eCivSize.Major;
            newCiv.Range = (int)(gameDataRef.GalaxySizeWidth / 2.3f);
            newCiv.PlanetMinTolerance = 45;
            newCiv.AstronomyRating = UnityEngine.Random.Range(6,11) * 1000;
            newCiv.ID = "CIV0"; // use this to reference the player's civ
            newCiv.HumanCiv = true;

            PlanetData pData;
            List<PlanetData> terranSystemPlanetList = gData.GalaxyStarDataList.Find(p => p.Name == "Neo-Sirius").PlanetList;
            pData = terranSystemPlanetList[UnityEngine.Random.Range(0, terranSystemPlanetList.Count)];

            // create the human homeworld manually;
            pData.Name = "New Terra";
            pData.ID = "PLANEWT001";
            pData.PlanetSpriteNumber = 43; // the city type
            pData.Type = PlanetData.ePlanetType.Terran;
            pData.Rank = PlanetData.ePlanetRank.ImperialCapital;
            pData.Size = 60;
            pData.Bio = 90;
            pData.Energy = 70;
            pData.HeavyMaterials = 60;
            pData.MaxTiles = 30;
            pData.MaxHabitableTiles = 26;
            pData.RareMaterials = 30;
            pData.BasicMaterials = 88;
            pData.IsTradeHub = true;
            newCiv.CapitalPlanetID = pData.ID;         
            GenerateGameObject.GeneratePlanetRegions(pData); // rework the tiles
            newCiv.PlanetIDList.Add(pData.ID);

            // generate the rest of the planet claims for the player empire
            gameDataRef.AddNewCivilization(newCiv);
            GenerateGameObject.ClaimPlanetsForCiv(newCiv);

            // determine intel level for systems, scan level for planets
            foreach (StarData star in gData.GalaxyStarDataList)
            {
                foreach (StarData civStar in HelperFunctions.DataRetrivalFunctions.GetCivSystemList(newCiv))
                {
                    float range = HelperFunctions.Formulas.MeasureDistanceBetweenSystems(civStar, star);
                    if (star.IntelValue < (int)(newCiv.AstronomyRating / (range + 1)))
                        star.IntelValue = (int)(newCiv.AstronomyRating / (range + 1));
                }
            }      
        }

        void GeneratePlanetIntelLevels(Civilization humanCiv)
        {
            List<PlanetData> civPlanetList = HelperFunctions.DataRetrivalFunctions.GetCivPlanetList(humanCiv);
            foreach (StarData star in gData.GalaxyStarDataList)
            {
                foreach (PlanetData planet in star.PlanetList)
                {
                    if (civPlanetList.Exists(p => p.ID == planet.ID))
                    {
                        planet.IntelLevel = 10;
                        planet.ScanLevel = 1f;
                    }
                    else
                    {
                        planet.IntelLevel = star.IntelValue / 2;
                        planet.ScanLevel = (float)(star.IntelValue / 10f) * UnityEngine.Random.Range(.2f, 1f);
                    }
                }
            }
        }

        void DetermineSystemCapitals()
        {
            foreach (Civilization civ in gameDataRef.CivList)
            {
                List<StarData> sDataList = new List<StarData>();
                sDataList = HelperFunctions.DataRetrivalFunctions.GetCivSystemList(civ);

                // cycle through each system and determine if a system capital is available
                foreach (StarData sData in sDataList)
                {
                    int highPlanetValue = 0;
                    PlanetData proposedCapital = new PlanetData();

                    foreach (PlanetData pData in sData.PlanetList)
                    {
                        if (HelperFunctions.DataRetrivalFunctions.GetCivPlanetList(civ).Exists(p => p.Name == pData.Name))
                        {
                            proposedCapital = pData;
                            if (highPlanetValue < pData.BasePlanetValue)
                            {            
                                proposedCapital = pData;
                                highPlanetValue = pData.BasePlanetValue;                             
                            }
                        }
                    }
                    if (proposedCapital.Rank != PlanetData.ePlanetRank.ImperialCapital && proposedCapital.Rank != PlanetData.ePlanetRank.ProvinceCapital)
                    {
                        proposedCapital.Rank = PlanetData.ePlanetRank.SystemCapital;
                    }
                    sData.SystemCapitalID = proposedCapital.ID; // set as system capital by default
                }
            }
        }

        void GenerateProvinces()
        {
            foreach (Civilization civ in gameDataRef.CivList)
            {
                if (civ.CivMaxProvinceSize > 1)
                {
                    List<StarData> sDataList = new List<StarData>();
                    sDataList = HelperFunctions.DataRetrivalFunctions.GetCivSystemList(civ);

                    // get a size for each province and then create it with provinces in range
                    int provinceSize = UnityEngine.Random.Range(2, civ.CivMaxProvinceSize); // vary each province size
                    int maxDist = civ.AdminRating * 90; // max dist between systems
                    int count = 0;
                    bool provinceValid = false;

                    foreach (StarData sData in sDataList)
                    {
                        Province newProvince = new Province();

                        // generate province name
                        if (DataManager.systemNameList.Count > 0)
                        {
                            var nameIndex = UnityEngine.Random.Range(0, DataManager.systemNameList.Count);
                            newProvince.Name = DataManager.systemNameList[nameIndex];
                            DataManager.systemNameList.RemoveAt(nameIndex);
                            DataManager.systemNameList.TrimExcess();
                        }
                        else
                            newProvince.Name = "Generic Province";

                        newProvince.ID = "PRO" + UnityEngine.Random.Range(0, 10000);
                        newProvince.OwningCivID = civ.ID;
                        int x = 0;                     
                        bool allSystemsChecked = false;
                        provinceValid = false;

                        do 
                        {                                                 
                            for (int y = count; y < sDataList.Count; y++)
                            {
                                float dist = HelperFunctions.Formulas.MeasureDistanceBetweenSystems(sData,sDataList[y]);
                                if (dist > 0 && dist < maxDist)
                                {
                                    if (sData.AssignedProvinceID == "")
                                    {
                                        sData.AssignedProvinceID = newProvince.ID;
                                        x += 1;
                                        provinceValid = true;
                                        sData.IsProvinceHub = true; // is the central system, will have the province capital installed
                                    }
                                    if (sDataList[y].AssignedProvinceID == "")
                                    {
                                        sDataList[y].AssignedProvinceID = newProvince.ID;
                                        x += 1;
                                        if (x == 1)
                                            sDataList[y].IsProvinceHub = true;
                                        provinceValid = true;
                                    }
                                }                               
                            }

                            if (sData.AssignedProvinceID == "") // if the system is not assigned to a province, create a one-shot province
                            {
                                Province nProv = new Province();

                                // generate province name
                                if (DataManager.systemNameList.Count > 0)
                                {
                                    var nameIndex = UnityEngine.Random.Range(0, DataManager.systemNameList.Count);
                                    nProv.Name = DataManager.systemNameList[nameIndex];
                                    DataManager.systemNameList.RemoveAt(nameIndex);
                                    DataManager.systemNameList.TrimExcess();
                                }
                                else
                                    nProv.Name = "Generic Province";

                                nProv.ID = "PRO" + UnityEngine.Random.Range(0, 10000);
                                nProv.OwningCivID = civ.ID;
                                sData.AssignedProvinceID = nProv.ID;
                                sData.IsProvinceHub = true;
                                gData.AddProvinceToList(nProv);
                            }
                            
                            if (provinceValid)
                                gData.AddProvinceToList(newProvince);

                            allSystemsChecked = true; // all systems have been checked; go to next star
                        }
                        while (x < provinceSize && !allSystemsChecked);
                        count += 1;
                    }
                    // now that provinces are created, designate the province capital
                    foreach (StarData sData in sDataList)
                    {
                        if (sData.IsProvinceHub)
                        {
                            int highPlanetValue = 0;
                            PlanetData proposedCapital = new PlanetData();
                            foreach (PlanetData pData in sData.PlanetList)
                            {
                                if (HelperFunctions.DataRetrivalFunctions.GetCivPlanetList(civ).Exists(p => p.Name == pData.Name))
                                {
                                    if (highPlanetValue < pData.BasePlanetValue)
                                    {
                                        if (pData.Rank != PlanetData.ePlanetRank.ImperialCapital) // don't overwrite the imperial capital! If no other capital, the imperial capital serves as province capital also
                                        {
                                            proposedCapital = pData;
                                            highPlanetValue = pData.BasePlanetValue;
                                        }
                                    }
                                }
                            }
                            proposedCapital.Rank = PlanetData.ePlanetRank.ProvinceCapital;
                            HelperFunctions.DataRetrivalFunctions.GetProvince(sData.AssignedProvinceID).CapitalPlanetID = proposedCapital.ID; // set the capital world ID
                        }
                    }
                }               
            }
        }
    }
}
