using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using CharacterObjects;
using HelperFunctions;

namespace Tooltips
{
    public class CharacterTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject tooltipPrefab; // the prefab to use
        private GameObject tooltipObject; // the actual tooltip instance
        private bool displayToolTip = false;
        private bool toolTipCreated = false;
        public bool exitRequested = false;
        private GraphicAssets graphicDataRef;
        private GlobalGameData gDataRef;
        private Canvas uiCanvas;
        private Character cData; // the character information that is passed
        private float alphaValue = 0f;
        private Color fadeColor;
        private float offsetAmount = 0.0f; // sets the distance away from the center of the tooltipped object

        void Awake()
        {
            uiCanvas = GameObject.Find("Character Window Canvas").GetComponent<Canvas>();
            graphicDataRef = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
            gDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
        }

        void OnGUI()
        {
            if (Input.GetButtonDown("Right Mouse Button")) // right mouse button kills the tooltip
            {
                alphaValue = 0f;
                exitRequested = true;
            }

            if (exitRequested)
            {
                displayToolTip = false;
                if (alphaValue <= 0f)
                {
                    Destroy(tooltipObject);
                }
                if (tooltipObject != null)
                {
                    FadeToolTip(false);
                }
            }

            if (displayToolTip)
            {
                DrawToolTip();
                if (tooltipObject != null)
                {
                    FadeToolTip(true);
                }
            }           
        }

        public void OnPointerEnter(PointerEventData eventData)
        {          
            if (tooltipObject != null)
            {
                DestroyImmediate(tooltipObject);
            }

            if (cData != null)
            {
                alphaValue = 0f;
                toolTipCreated = false;
                displayToolTip = true;
                exitRequested = false;
                gDataRef.CharacterTooltipIDActive = cData.ID; // set active tooltip ID
            }
            else
                return;
        }

        public void InitializeTooltipData(Character characterData, float offset)
        {
            cData = characterData;
            offsetAmount = offset;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            displayToolTip = false;
            exitRequested = true;           
        }

        private void FadeToolTip(bool fadeIn)
        {
            fadeColor = new Color(1, 1, 1, alphaValue / 255f);
            if (fadeIn)
                StartCoroutine(FadeInAlpha(255f));
            else
                StartCoroutine(FadeOutAlpha(0f));

            tooltipObject.GetComponent<Image>().color = fadeColor;

            // white colored data
            tooltipObject.transform.Find("Character Name").GetComponent<Text>().color = fadeColor;
            tooltipObject.transform.Find("Character Image").GetComponent<Image>().color = fadeColor;
            tooltipObject.transform.Find("Character Age").GetComponent<Text>().color = fadeColor;
            tooltipObject.transform.Find("Character Rank").GetComponent<Text>().color = fadeColor;
            tooltipObject.transform.Find("Character Health").GetComponent<Text>().color = fadeColor;
            tooltipObject.transform.Find("Character Trait 1").GetComponent<Text>().color = fadeColor;
            tooltipObject.transform.Find("Character Trait 2").GetComponent<Text>().color = fadeColor;
            tooltipObject.transform.Find("Character Trait 3").GetComponent<Text>().color = fadeColor;
            tooltipObject.transform.Find("Character Trait 4").GetComponent<Text>().color = fadeColor;
            tooltipObject.transform.Find("Character Lifeform").GetComponent<Text>().color = fadeColor;

            // labels
            tooltipObject.transform.Find("Character Age Label").GetComponent<Text>().color = fadeColor;
            tooltipObject.transform.Find("Character Intelligence Label").GetComponent<Text>().color = fadeColor;
            tooltipObject.transform.Find("Character Loyalty Label").GetComponent<Text>().color = fadeColor;
            tooltipObject.transform.Find("Character Will Label").GetComponent<Text>().color = fadeColor;
            tooltipObject.transform.Find("Character Charisma Label").GetComponent<Text>().color = fadeColor;
            tooltipObject.transform.Find("Character Ambition Label").GetComponent<Text>().color = fadeColor;
            tooltipObject.transform.Find("Character Influence Label").GetComponent<Text>().color = fadeColor;
            tooltipObject.transform.Find("Character Trait Label").GetComponent<Text>().color = fadeColor;
           
            //tooltipObject.transform.Find("Character House Specialty Label").GetComponent<Text>().color = fadeColor;
            

            Color intelRatingColor = tooltipObject.transform.Find("Character Intel Level").GetComponent<Text>().color;
            Color intelColor;
            Color loyaltyColor;
            Color charismaColor;
            Color willColor;
            Color ambitionRatingColor;
            Color influenceColor;
            Color specialtyColor;

            // dynamic colored data
            if (cData.IntelLevel > 1)
            {
                intelColor = HelperFunctions.StringConversions.GetTextValueColor(cData.Intelligence);
                loyaltyColor = HelperFunctions.StringConversions.GetTextValueColor(cData.Honor);
                charismaColor = HelperFunctions.StringConversions.GetTextValueColor(cData.Charm);
                willColor = HelperFunctions.StringConversions.GetTextValueColor(cData.Passion);              
                ambitionRatingColor = HelperFunctions.StringConversions.GetTextValueColor(cData.Drive);
                influenceColor = HelperFunctions.StringConversions.GetTextValueColor(cData.Influence);
                specialtyColor = Color.white;
            }
            else
            {
                intelColor = Color.white;
                loyaltyColor = Color.white;
                charismaColor = Color.white;
                willColor = Color.white;
                ambitionRatingColor = Color.white;
                influenceColor = Color.white;
                specialtyColor = Color.white;

            }
            
            tooltipObject.transform.Find("Character Intelligence").GetComponent<Text>().color = new Color(intelColor.r,intelColor.g,intelColor.b,fadeColor.a);       
            tooltipObject.transform.Find("Character Loyalty").GetComponent<Text>().color = new Color(loyaltyColor.r, loyaltyColor.g, loyaltyColor.b, fadeColor.a);          
            tooltipObject.transform.Find("Character Charisma").GetComponent<Text>().color = new Color(charismaColor.r, charismaColor.g, charismaColor.b, fadeColor.a);          
            tooltipObject.transform.Find("Character Will").GetComponent<Text>().color = new Color(willColor.r, willColor.g, willColor.b, fadeColor.a);         
            tooltipObject.transform.Find("Character Intel Level").GetComponent<Text>().color = new Color(intelRatingColor.r, intelRatingColor.g, intelRatingColor.b, fadeColor.a);        
            tooltipObject.transform.Find("Character Ambition").GetComponent<Text>().color = new Color(ambitionRatingColor.r, ambitionRatingColor.g, ambitionRatingColor.b, fadeColor.a);
            tooltipObject.transform.Find("Character Influence").GetComponent<Text>().color = new Color(influenceColor.r,  influenceColor.g, influenceColor.b, fadeColor.a);
            //tooltipObject.transform.Find("Character House Specialty").GetComponent<Text>().color = new Color(specialtyColor.r, specialtyColor.g, specialtyColor.b, fadeColor.a);
        }

        private void DrawToolTip()
        {
            string intelLevelText = "NONE";

            if (!toolTipCreated)
            {
                Vector3 tooltipLocation = new Vector3(transform.position.x + offsetAmount, transform.position.y, transform.position.z - 5);
                
                tooltipObject = Instantiate(tooltipPrefab, tooltipLocation, Quaternion.identity) as GameObject;             
                tooltipObject.transform.SetParent(uiCanvas.transform, true); // assign to canvas
                tooltipObject.transform.localScale = new Vector3(.8f, .8f, .8f);
                tooltipObject.name = cData.ID;

                // convert intel level

                if (cData.IntelLevel == 0)
                {
                    intelLevelText = "NONE";
                }
                else if (cData.IntelLevel > 0 && cData.IntelLevel < Constants.Constants.LowIntelLevelMax)
                {
                    intelLevelText = "LOW";
                }
                else if (cData.IntelLevel < Constants.Constants.MediumIntelLevelMax)
                {
                    intelLevelText = "MEDIUM";
                }
                else if (cData.IntelLevel < Constants.Constants.HighIntelMax)
                {
                    intelLevelText = "HIGH";
                }
                else
                    intelLevelText = "MAX";

                string charName = cData.Name.ToUpper();
                if (cData.HouseID != null)
                {
                    charName += " OF " + HelperFunctions.DataRetrivalFunctions.GetHouse(cData.HouseID).Name.ToUpper();
                    //tooltipObject.transform.Find("Character House Specialty").GetComponent<Text>().text = cData.AssignedHouse.Specialty.ToString().ToUpper();
                }
                else
                {
                    //tooltipObject.transform.Find("Character House Specialty").GetComponent<Text>().text = "N/A";
                }
                int traitSize = cData.Traits.Count;

                // clear out old data
                tooltipObject.transform.Find("Character Trait 1").GetComponent<Text>().text = "";
                tooltipObject.transform.Find("Character Trait 2").GetComponent<Text>().text = "";
                tooltipObject.transform.Find("Character Trait 3").GetComponent<Text>().text = "";
                tooltipObject.transform.Find("Character Trait 4").GetComponent<Text>().text = "";

                if (traitSize > 0)
                {
                    tooltipObject.transform.Find("Character Trait 1").GetComponent<Text>().text = cData.Traits[0].Name.ToUpper();
                }

                if (traitSize > 1)
                {
                    tooltipObject.transform.Find("Character Trait 2").GetComponent<Text>().text = cData.Traits[1].Name.ToUpper();
                }

                if (traitSize > 2)
                {
                    tooltipObject.transform.Find("Character Trait 3").GetComponent<Text>().text = cData.Traits[2].Name.ToUpper();
                }

                if (traitSize > 3)
                {
                    tooltipObject.transform.Find("Character Trait 4").GetComponent<Text>().text = cData.Traits[3].Name.ToUpper();
                }

                tooltipObject.transform.Find("Character Lifeform").GetComponent<Text>().text = cData.Lifeform.ToString().ToUpper();
                tooltipObject.transform.Find("Character Name").GetComponent<Text>().text = charName;
                tooltipObject.transform.Find("Character Age").GetComponent<Text>().text = cData.Age.ToString("N0");
                tooltipObject.transform.Find("Character Rank").GetComponent<Text>().text = HelperFunctions.StringConversions.ConvertRoleEnum(cData.Role).ToUpper();
                tooltipObject.transform.Find("Character Intel Level").GetComponent<Text>().text = intelLevelText;
                tooltipObject.transform.Find("Character Image").GetComponent<Image>().sprite = graphicDataRef.CharacterList.Find(p => p.name == cData.PictureID);

                // get intel level
                if (cData.IntelLevel > 0)
                {
                    tooltipObject.transform.Find("Character Health").GetComponent<Text>().text = cData.Health.ToString().ToUpper();
                }
                if (cData.IntelLevel > 0)
                {
                    tooltipObject.transform.Find("Character Intelligence").GetComponent<Text>().text = HelperFunctions.StringConversions.ConvertCharacterValueToDescription(cData.Intelligence, cData.IntelLevel);                
                    tooltipObject.transform.Find("Character Loyalty").GetComponent<Text>().text = HelperFunctions.StringConversions.ConvertCharacterValueToDescription(cData.Honor, cData.IntelLevel);                
                    tooltipObject.transform.Find("Character Charisma").GetComponent<Text>().text = HelperFunctions.StringConversions.ConvertCharacterValueToDescription(cData.Charm, cData.IntelLevel);                 
                    tooltipObject.transform.Find("Character Will").GetComponent<Text>().text = HelperFunctions.StringConversions.ConvertCharacterValueToDescription(cData.Passion, cData.IntelLevel);
                    tooltipObject.transform.Find("Character Ambition").GetComponent<Text>().text = HelperFunctions.StringConversions.ConvertCharacterValueToDescription(cData.Drive, cData.IntelLevel);
                    tooltipObject.transform.Find("Character Influence").GetComponent<Text>().text = HelperFunctions.StringConversions.ConvertCharacterValueToDescription(cData.Influence, cData.IntelLevel);            
                }
                gDataRef.activeTooltip = tooltipObject; // assign the active tooltip
                toolTipCreated = true;
            }
        }

        IEnumerator FadeInAlpha(float targetAlpha)
        {
            while (alphaValue < targetAlpha)
            {
                alphaValue += 5f;
                yield return null;
            }
        }

        IEnumerator FadeOutAlpha(float targetAlpha)
        {
            while (alphaValue > targetAlpha)
            {
                alphaValue -= 5f;
                if (alphaValue < 0f)
                    alphaValue = 0f;
                yield return null;
            }
        }

    }
}
