using UnityEngine;
using System.Collections.Generic;
using PlanetObjects;
using CivObjects;
using CharacterObjects;
using EconomicObjects;
using Tooltips;
using Actions;

public class GlobalGameData : MonoBehaviour
{

    // ui mode enums
    public enum eSubMode : int
    {
        None,
        Intel,
        War,
        Finance,
        Diplomacy,
        Emperor,
        Science,
        NextTurn
    };

    public eSubMode uiSubMode = eSubMode.None;
    public bool CharacterWindowActive = false;
    public string CharacterTooltipIDActive;
    public GameObject activeTooltip = null;
    public bool StarSelected = false;
    public bool modalIsActive = false; // checks for modal window active
    public Character SelectedCharacter = null; // selected character ID

    public bool DebugMode = false; // allows to see all values during testing (remove for play builds)
    public bool RequestGraphicRefresh = false; // this determines whether a redraw is done

    // game screen backgrounds
    public Texture2D mainGameScreen;
    public Texture2D createWorldGameScreen;
    public Texture2D createEmperorScreen;

    // universal game data
    public float gameVersionNumber = .001f;
    public string gameVersionSuffix = "a";
    public string gameVersion = "";
    public float GameDate { get; set; }
    public int GameMonth { get; set; }

    // gamewide object lists
    public List<Civilization> CivList = new List<Civilization>();
    public List<Pops> PopList = new List<Pops>(); // univeral population list in game
    public List<TradeAgreement> ActiveTradeAgreements = new List<TradeAgreement>();
    public List<Character> CharacterList = new List<Character>();
    public List<CharacterAction> CharacterActionList = new List<CharacterAction>();
    public List<CharacterTrait> CharacterTraitList = new List<CharacterTrait>();
    public List<House> HouseList = new List<House>(); // all houses generated in the game

    // galaxy setup variables
    public int GalaxySizeHeight { get; set; }
    public int GalaxySizeWidth { get; set; }
    public int MinSystems { get; set; }
    public int MaxSystems { get; set; }
    public int TotalSystems { get; set; }

    // empire setup variables
    public string PlayerEmpireName { get; set; }

    // game setup variables
    public int NumberOfAICivs { get; set; }
    public bool CivsGenerated = false;

    // Use this for initialization
    void Awake()
    {
        CivList = new List<Civilization>(); // initialize lists
        GameDate = 3050.0f;
        GameMonth = 1;
        GalaxySizeHeight = 6500;
        GalaxySizeWidth = 6500;
        MinSystems = 90;
        MaxSystems = 115;
        TotalSystems = 50;
        NumberOfAICivs = 8; // static number to test UnityEngine.Random.Range(4, 7); // test
        PlayerEmpireName = "New Human Empire";
        gameVersion = gameVersionNumber.ToString("N4") + gameVersionSuffix;
        DataManager.InitializeData(); // intialize data all in one place (test)
    }

    public void UpdateGameDate()
    {
        GameDate += .1f;
        GameMonth += 1;
        if (GameMonth == 9)
        {
            GameMonth = 0;
        }
    }

    public void AddNewCivilization(Civilization civ)
    {
        CivList.Add(civ);
    }

} 
