using UnityEngine;
using System.Collections;
using CharacterObjects;
using StellarObjects;
using CivObjects;
using ConversationAI;

namespace Actions
{
    public class CharacterAction
    {
        public enum eType : int
        {
            Political,
            Military,
            Economic,
            Personal,
            Psychic,
            IntelOps
        }

        public string Name { get; set; }
        public string ID { get; set; }
        public eType Category { get; set; }
        public string Description { get; set; }
        public bool ViceroyValid { get; set; }
        public bool SysGovValid { get; set; }
        public bool ProvGovValid { get; set; }
        public bool PrimeValid { get; set; } // need to add all the other Prime positions
        public bool AllValid { get; set; }
        public bool InquisitorValid { get; set; }
        public bool EmperorNearValid { get; set; }
        public bool EmperorAction { get; set; }

        public bool IsActionValid(Character cData, Civilization civ)
        {
            if (AllValid)
            {
                return true;
            }

            if (cData.Role == Character.eRole.Emperor && !EmperorAction) // if not an Emperor character action, kick
                return false;

            if (cData.Role == Character.eRole.Viceroy && !ViceroyValid)
            {
                return false;
            }

            if (cData.Role == Character.eRole.SystemGovernor && !SysGovValid)
            {
                return false;
            }

            if (cData.Role == Character.eRole.ProvinceGovernor && !ProvGovValid)
            {
                return false;
            }

            if (cData.Role == Character.eRole.DomesticPrime && !PrimeValid)
            {
                return false;
            }

            if (cData.Role == Character.eRole.Inquisitor && !InquisitorValid)
            {
                return false;
            }

            if (EmperorNearValid)
            {
                if (civ.Leader != null)
                {
                    if (cData.PlanetLocationID != civ.Leader.PlanetLocationID)
                    {
                        return false;
                    }

                }
                else
                {
                    return false; // no leader, so false anyway
                }
            }
            return true; // passed all tests so it's true


        }

    }

    // action functions
    public static class ActionFunctions
    {
        public static string GivePraisingSpeech(Character cData)
        {
            GlobalGameData gDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>(); 
            Character eData = gDataRef.CivList[0].Leader; // you
            CharacterAction aData = gDataRef.CharacterActionList.Find(p => p.ID == "A1");
            bool speechSuccessful = false;
            float responseIndex = 0f;
            int speechEffectiveness = 0;
            string speechSuccess = ""; // debug code

            // make check based on charm + intelligence
            if (eData.Charm >= -30)
            {
                speechEffectiveness = UnityEngine.Random.Range(30, eData.Charm) + UnityEngine.Random.Range(0, eData.Intelligence);
                if (speechEffectiveness > 80)
                {
                    speechSuccessful = true;
                    speechSuccess = "successful, value of " + speechEffectiveness.ToString("N0"); // debug code
                }
                else
                {
                    speechSuccessful = false;
                    speechSuccess = "unsuccessful, value of " + speechEffectiveness.ToString("N0"); // debug code
                }
            }
            else
            {
                speechSuccessful = false;
                speechSuccess = "unsuccessful, minimum check not passed to try"; // debug code
            }

            // now determine effect of character
            responseIndex = speechEffectiveness;
            if (speechSuccessful)
            {
                if (cData.NewRelationships[eData.ID].Trust > 50)
                {
                    cData.NewRelationships[eData.ID].Trust += UnityEngine.Random.Range(0, (speechEffectiveness / 5));
                }
                else
                {
                    cData.NewRelationships[eData.ID].Trust += UnityEngine.Random.Range(0, (speechEffectiveness / 8)); // less effective when more hated
                }

                // now determine effect of characters around them, checking each character individually
                foreach (string cID in cData.NewRelationships.Keys)
                {
                    if (cData.NewRelationships.ContainsKey(cID))
                    {
                        if (cData.NewRelationships[cID].RelationshipState == Relationship.eRelationshipState.Friends || cData.NewRelationships[cID].RelationshipState == Relationship.eRelationshipState.Allies)
                        {
                            cData.NewRelationships[cID].Trust += UnityEngine.Random.Range(0, (speechEffectiveness / 8));
                            HelperFunctions.DataRetrivalFunctions.GetCharacter(cID).NewRelationships[eData.ID].Trust += UnityEngine.Random.Range(0, (speechEffectiveness / 10)); // improve trust slightly with the emperor
                        }

                        if (cData.NewRelationships[cID].RelationshipState == Relationship.eRelationshipState.Rivals || cData.NewRelationships[cID].RelationshipState == Relationship.eRelationshipState.Vendetta)
                        {
                            cData.NewRelationships[cID].Trust -= UnityEngine.Random.Range(0, (speechEffectiveness / 8));
                            HelperFunctions.DataRetrivalFunctions.GetCharacter(cID).NewRelationships[eData.ID].Trust -= UnityEngine.Random.Range(0, (speechEffectiveness / 10)); // distrusts slightly with the emperor
                        }

                        if (cData.NewRelationships[cID].RelationshipState == Relationship.eRelationshipState.Vengeance)
                        {
                            cData.NewRelationships[cID].Trust -= UnityEngine.Random.Range(0, (speechEffectiveness / 6));
                            HelperFunctions.DataRetrivalFunctions.GetCharacter(cID).NewRelationships[eData.ID].Trust -= UnityEngine.Random.Range(0, (speechEffectiveness / 6)); // distrusts a lot with the emperor
                        }
                    }
                }
            }

            // now send to speech engine to create response and return response
            UnityEngine.Debug.Log("Give Praising Speech executed. Speech was " + speechSuccess); // debug code
            string response = ConversationEngine.GenerateResponse(cData, aData, responseIndex, false);

            return response;
        }

        public static string IssuePublicReprimand(Character cData)
        {
            return null;
        }
    }


}
