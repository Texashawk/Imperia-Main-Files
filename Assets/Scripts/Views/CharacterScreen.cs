using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CharacterObjects;
using StellarObjects;
using PlanetObjects;
using System.Collections;
using HelperFunctions;
using Tooltips;
using ConversationAI;

public class CharacterScreen : MonoBehaviour, IPointerClickHandler
{
    GlobalGameData gDataRef;
    GraphicAssets graphicsDataRef;
    Character cData;
    GameObject characterWindow;
    GameObject blockingPanel;
    Image characterImage;
    Image charRankImage;
    Image rank1Image;
    Image rank2Image;
    Text characterName;
    public Text CommText;
    Text history;
    Text age;
    Text health;
    Text intelligence;
    Text wealth;
    Text drive;
    Text charm;
    Text intel;
    Text poSup;
    Text honor;
    Text piety;
    Text empathy;
    Text passion;
    Text influence;
    Text trait1;
    Text trait2;
    Text trait3;
    Text trait4;
    ActionScrollView aView;
    RelationsScrollView rView;
    Text characterRank;
    bool exitRequested = false;
    public bool CommTextGenerated = false;
    public bool CharacterDataLoaded = false;

    void Awake()
    {
        gDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
        graphicsDataRef = GameObject.Find("GameManager").GetComponent<GraphicAssets>(); // get graphics
        characterWindow = GameObject.Find("Character Window Panel");
        blockingPanel = GameObject.Find("Blocking Panel");
        age = characterWindow.transform.Find("Age Value").GetComponent<Text>();
        health = characterWindow.transform.Find("Health Value").GetComponent<Text>();
        wealth = characterWindow.transform.Find("Wealth Value").GetComponent<Text>();
        intelligence = characterWindow.transform.Find("Intelligence Value").GetComponent<Text>();
        influence = characterWindow.transform.Find("Influence Value").GetComponent<Text>();
        honor = characterWindow.transform.Find("Loyalty Value").GetComponent<Text>();
        intel = characterWindow.transform.Find("Intel Value").GetComponent<Text>();
        charm = characterWindow.transform.Find("Charisma Value").GetComponent<Text>();
        drive = characterWindow.transform.Find("Drive Value").GetComponent<Text>();
        piety = characterWindow.transform.Find("Piety Value").GetComponent<Text>();
        empathy = characterWindow.transform.Find("Empathy Value").GetComponent<Text>();
        passion = characterWindow.transform.Find("Will Value").GetComponent<Text>();
        history = characterWindow.transform.Find("History Text").GetComponent<Text>();
        CommText = characterWindow.transform.Find("Comm Text").GetComponent<Text>();
        trait1 = characterWindow.transform.Find("Trait 1").GetComponent<Text>();
        trait2 = characterWindow.transform.Find("Trait 2").GetComponent<Text>();
        trait3 = characterWindow.transform.Find("Trait 3").GetComponent<Text>();
        trait4 = characterWindow.transform.Find("Trait 4").GetComponent<Text>();
        characterName = characterWindow.transform.Find("Character Name").GetComponent<Text>();
        characterRank = characterWindow.transform.Find("Character Rank").GetComponent<Text>();
        characterImage = characterWindow.transform.Find("Character Image").GetComponent<Image>();
        aView = characterWindow.transform.Find("ScrollView").GetComponent<ActionScrollView>();
        rView = characterWindow.transform.Find("Relations Scrollview").GetComponent<RelationsScrollView>();
        charRankImage = characterWindow.transform.Find("Char Rank Image").GetComponent<Image>();
        rank1Image = characterWindow.transform.Find("Rank 1 Image").GetComponent<Image>();
        rank2Image = characterWindow.transform.Find("Rank 2 Image").GetComponent<Image>();
        characterWindow.SetActive(false); // sets the window as active initially
        blockingPanel.SetActive(false); // this panel blocks mouse clicks from going 'through' the screen since it is supposed to be a modal screen
    }

    void OnGUI()
    {
        if (gDataRef.CharacterWindowActive)
        {
            if (!exitRequested)
            {
                characterWindow.SetActive(true);
                blockingPanel.SetActive(true);
                PopulateCharacterWindow();
            }
            else
            {
                characterWindow.SetActive(false);
                blockingPanel.SetActive(false);
                aView.listIsInitialized = false;
                aView.ClearList();
                rView.ClearList();
                CharacterDataLoaded = false;
                gDataRef.CharacterWindowActive = false;
                gDataRef.modalIsActive = false;
                gDataRef.SelectedCharacter = null; 
                exitRequested = false;
                CommTextGenerated = false; // resets conversation engine generation request
            }
        }
    }

    public void InitializeData(Character characterData)
    {
        cData = characterData;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerId == -2) // for right mouse click
        {
            exitRequested = true;
        }
    }

    void PopulateCharacterWindow()
    {
        if (!CharacterDataLoaded) // load character data;
        {
            cData = gDataRef.SelectedCharacter;
            CharacterDataLoaded = true;
            aView.listIsInitialized = false;
            rView.ClearList();
            CommTextGenerated = false;
        }

        characterName.text = cData.Name.ToUpper() + " OF " + cData.AssignedHouse.Name.ToUpper();

        // get role string
        DrawRoleString();

        // draw character stats
        DrawStats();

        // draw traits
        DrawTraits();

        // draw chain of command pics
        DrawChainOfCommand();

        // generate initial conversation
        GenerateInitialConversation();

        // populate the lists (test for now, just populates the fellow house members)
        aView.InitializeList(cData);
        rView.InitializeList(cData);      
    }

    void DrawStats()
    {
        
        age.text = cData.Age.ToString("N0");       
        intel.text = cData.IntelLevel.ToString("N0");

        if (cData.IntelLevel > 0)
        {
            history.text = cData.History;
            history.color = Color.green;
            wealth.text = HelperFunctions.StringConversions.ConvertFloatDollarToText(cData.Wealth);
            health.text = cData.Health.ToString().ToUpper();
            drive.text = HelperFunctions.StringConversions.ConvertCharacterValueToDescription(cData.Passion, cData.IntelLevel);
            intelligence.text = HelperFunctions.StringConversions.ConvertCharacterValueToDescription(cData.Intelligence, cData.IntelLevel);
            influence.text = HelperFunctions.StringConversions.ConvertCharacterValueToDescription(cData.Influence, cData.IntelLevel);
            charm.text = HelperFunctions.StringConversions.ConvertCharacterValueToDescription(cData.Charm, cData.IntelLevel);
            honor.text = HelperFunctions.StringConversions.ConvertCharacterValueToDescription(cData.Honor, cData.IntelLevel);
            passion.text = HelperFunctions.StringConversions.ConvertCharacterValueToDescription(cData.Passion, cData.IntelLevel);
            empathy.text = HelperFunctions.StringConversions.ConvertCharacterValueToDescription(cData.Empathy, cData.IntelLevel);
            piety.text = HelperFunctions.StringConversions.ConvertCharacterValueToDescription(cData.Piety, cData.IntelLevel);
        }
    }

    void DrawTraits()
    {
        // clear out all text bars
        trait1.text = "\n";
        trait2.text = "\n";
        trait3.text = "\n";
        trait4.text = "\n";

        if (cData.Traits.Count > 0)
            trait1.text = cData.Traits[0].Name.ToUpper();
        if (cData.Traits.Count > 1)
            trait2.text = cData.Traits[1].Name.ToUpper();
        if (cData.Traits.Count > 2)
            trait3.text = cData.Traits[2].Name.ToUpper();
        if (cData.Traits.Count > 3)
            trait4.text = cData.Traits[3].Name.ToUpper();
    }

    void GenerateInitialConversation()
    {
        if (!CommTextGenerated)
        {
            CommText.text = ConversationAI.ConversationEngine.GenerateInitialDialogue(cData); // super basic call
            CommTextGenerated = true;
        }
    }

    void DrawRoleString()
    {
        string assignment = "";
        if (cData.Role == Character.eRole.Viceroy)
        {
            assignment = cData.PlanetAssigned.Name + ", " + cData.PlanetAssigned.System.Name + " SYSTEM" + ", " + cData.PlanetAssigned.System.Province.Name + " PROVINCE";
        }
        else if (cData.Role == Character.eRole.SystemGovernor)
        {
            assignment = cData.SystemAssigned.Name + ", " + cData.SystemAssigned.Province.Name + " PROVINCE";
        }
        else if (cData.Role == Character.eRole.ProvinceGovernor)
        {
            assignment = cData.ProvinceAssigned.Name  + " PROVINCE";
        }
        else
        {
            assignment = HelperFunctions.DataRetrivalFunctions.GetCivilization(cData.CivID).Name;
        }
        characterRank.text = HelperFunctions.StringConversions.ConvertRoleEnum(cData.Role).ToUpper() + " OF " + assignment.ToUpper();
    }

    void DrawChainOfCommand()
    {
        characterImage.sprite = graphicsDataRef.CharacterList.Find(p => p.name == cData.PictureID);
        if (cData.Role == Character.eRole.Viceroy)
        {
            charRankImage.sprite = graphicsDataRef.CharacterList.Find(p => p.name == cData.PictureID);
            charRankImage.GetComponent<CharacterTooltip>().InitializeTooltipData(cData, -21f); // set up the tooltip
            
            if (cData.PlanetAssigned.System.Governor != null)
            {
                rank1Image.enabled = true;
                rank1Image.GetComponent<CharacterScreenActivation>().InitializeData(cData.PlanetAssigned.System.Governor);
                rank1Image.sprite = graphicsDataRef.CharacterList.Find(p => p.name == cData.PlanetAssigned.System.Governor.PictureID);
                rank1Image.GetComponent<CharacterTooltip>().InitializeTooltipData(cData.PlanetAssigned.System.Governor, -21f); // set up the tooltip
            }
            else
            {
                rank1Image.enabled = false;
            }

            if (cData.PlanetAssigned.System.Province.Governor != null)
            {
                rank2Image.enabled = true;
                rank2Image.GetComponent<CharacterScreenActivation>().InitializeData(cData.PlanetAssigned.System.Province.Governor);
                rank2Image.sprite = graphicsDataRef.CharacterList.Find(p => p.name == cData.PlanetAssigned.System.Province.Governor.PictureID);
                rank2Image.GetComponent<CharacterTooltip>().InitializeTooltipData(cData.PlanetAssigned.System.Province.Governor, -21f); // set up the tooltip
            }
            else
            {
                rank2Image.enabled = false;
            }
        }

        if (cData.Role == Character.eRole.SystemGovernor)
        {
            charRankImage.sprite = graphicsDataRef.CharacterList.Find(p => p.name == cData.PictureID);
            charRankImage.GetComponent<CharacterTooltip>().InitializeTooltipData(cData, -21f); // set up the tooltip
            if (cData.SystemAssigned.Province.Governor != null)
            {
                rank1Image.enabled = true;
                rank1Image.GetComponent<CharacterScreenActivation>().InitializeData(cData.SystemAssigned.Province.Governor);
                rank1Image.sprite = graphicsDataRef.CharacterList.Find(p => p.name == cData.SystemAssigned.Province.Governor.PictureID);
                rank1Image.GetComponent<CharacterTooltip>().InitializeTooltipData(cData.SystemAssigned.Province.Governor, -21f); // set up the tooltip
            }
            else
            {
                rank1Image.enabled = false;
            }

            if (gDataRef.CharacterList.Exists(p => p.Role == Character.eRole.DomesticPrime))
            {
                string picID = gDataRef.CharacterList.Find(p => p.Role == Character.eRole.DomesticPrime).PictureID;
                rank2Image.enabled = true;
                rank2Image.GetComponent<CharacterScreenActivation>().InitializeData(gDataRef.CharacterList.Find(p => p.Role == Character.eRole.DomesticPrime));
                rank2Image.sprite = graphicsDataRef.CharacterList.Find(p => p.name == picID);
                rank2Image.GetComponent<CharacterTooltip>().InitializeTooltipData(gDataRef.CharacterList.Find(p => p.Role == Character.eRole.DomesticPrime), -21f); // set up the tooltip
            }
            else
            {
                rank2Image.enabled = false;
            }
        }
    }

   
}
