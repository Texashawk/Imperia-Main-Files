using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using CharacterObjects;
using Tooltips;

public class RelationsScrollView : MonoBehaviour
{

    public GameObject Button_Template;
    private List<Character> RelationsList = new List<Character>();
    private GlobalGameData gameDataRef;
    private List<GameObject> buttonList = new List<GameObject>();
    public bool listIsInitialized = false;

    void Awake()
    {
        gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
    }
    
    public void InitializeList(Character cData)
    {
        if (!listIsInitialized)
        {
            ClearList(); // clear out the list
            string houseID = cData.HouseID;
            RelationsList = gameDataRef.CharacterList.FindAll(p => p.CivID == cData.CivID); // find all characters in the Empire first
            foreach (Character cha in RelationsList)
            {
                bool relationshipIsKnown = false;
                // check for at least high level of intel on both characters to know their relationship
                if (cData.IntelLevel > Constants.Constants.LowIntelLevelMax && cha.IntelLevel > Constants.Constants.LowIntelLevelMax)
                {
                    relationshipIsKnown = true;
                }

                if (cha.ID != cData.ID && relationshipIsKnown && cha.Role != Character.eRole.Pool) // don't populate yourself, and don't include pooled characters to reduce clutter
                {
                    GameObject go = Instantiate(Button_Template) as GameObject;
                    go.SetActive(true);
                    RelationsButton TB = go.GetComponent<RelationsButton>();
                    TB.SetRelationsValue(cData.NewRelationships[cha.ID]);
                    TB.SetID(cha.ID);
                    TB.SetName(cha.Name);
                    TB.SetPicture(cha.PictureID);
                    go.GetComponent<CharacterTooltip>().InitializeTooltipData(cha, (-TB.GetComponent<RectTransform>().sizeDelta.x / 4f)); // initialize character tooltip
                    go.transform.SetParent(Button_Template.transform.parent);
                    go.transform.localScale = new Vector3(1, 1, 1); // to offset canvas scaling
                    go.transform.localPosition = new Vector3(go.transform.localPosition.x, go.transform.localPosition.y, 0);
                    buttonList.Add(go);
                }
            }

            listIsInitialized = true;
            RelationsList.Clear();
        }
    }

    public void ClearList()
    {
        listIsInitialized = false;
        foreach (GameObject go in buttonList)
        {
            Destroy(go);
        }
    }

    public void ButtonClicked(string str)
    {
        Debug.Log(str + " button clicked.");
        //if (buttonList.Exists(p => p.name == str)) // delete current tooltip associated with the button
        //{
        //    buttonList.Find(p => p.name == str).GetComponent<CharacterTooltip>().exitRequested = true;
        //}
        GetComponent<CharacterScreenActivation>().InitializeData(HelperFunctions.DataRetrivalFunctions.GetCharacter(str));
        GetComponent<CharacterScreenActivation>().ActivateWindow();
        listIsInitialized = false;
        if (gameDataRef.activeTooltip != null) // if there is a tooltip active from another window, destroy it immediately so that it doesn't 'hang' between reloads
        {
            GameObject.DestroyImmediate(gameDataRef.activeTooltip);
        }
       
        
    }
}
