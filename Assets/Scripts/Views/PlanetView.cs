using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using StellarObjects;
using PlanetObjects;
using CameraScripts;
using Screens.Galaxy;
using HelperFunctions;
using CharacterObjects;
using Tooltips;

public class PlanetView : MonoBehaviour {

    private GalaxyCameraScript gScriptRef;
    private GlobalGameData gameDataRef;
    private GraphicAssets graphicsDataRef;
    private GalaxyScreen gScreen; 
    public GameObject habitableTile;
    public GameObject uninhabitableTile;
    public GameObject resourceFlowPanel;
    public GameObject productionPlanPanel;
    public GameObject stockPanel;
    public GameObject economicPanel;
    public GameObject planetSummaryPanel; // place to stick the summary panel
    private GameObject tileMapPanel;
    private Light tileMapLight;

    private PlanetData pData;
    private GameObject planetButtonBarBackground;
    private GameObject stellarographyPanel;
    private GameObject edictPanel;
    private GameObject wireFrameOverlay;
    private Text intelLevelText;
    private Text intelLevelValue;
    private Text planetValueText;
    private Text planetValue;
    private Text starbaseText;
    private Text starbaseValue;
    private Text tradeHubIndicator;
    private Text throughputRemaining;
    private Image starbaseDataPanel;
   
    private GameObject industryDisplayButton;
    private GameObject economicDisplayButton;
    private List<GameObject> tileList = new List<GameObject>();
    private List<GameObject> planetObjectsDrawnList = new List<GameObject>();
    private Vector3 originalPlanetRingPosition;
    private Vector3 originalSecondaryPlanetRingPosition;
    private float alphaValue = 0f;
    private float planetAlphaValue = 255f;
    private float wireAlphaValue = 0f;
    private bool planetDataLoaded = false;  //ensures that only one check is made every time script is run
    private bool tileMapGenerated = false;
    private Quaternion tilePanelOriginalRotation;
    private bool planetDataBoxGenerated = false;
    private bool industryDisplayMode = false;
    private bool economicDisplayMode = false;
    private bool subPanelExists = false;
    private bool planetTransitionComplete = false;
    private bool planetInfoInitialized = false;
    private Canvas planetCanvas;
    private float screenWidthRatio;
    private float screenHeightRatio;
    private GameObject selectedPlanet;
    private Color planetColor;
    private Vector3 planetCurrentPosition;
    private Vector3 planetCurrentScale;
    private Vector3 planetTargetScale;
    private Vector3 wireFrameCurrentScale;
    private Vector3 wireFrameTargetScale;
    private Vector3 wireframeCurrentPosition;
    private Vector3 planetTargetPosition;
    private Vector3 wireframeTargetPosition;

    // ui bools
    private bool regionDisplayMode = false; // off to start
    private bool leadershipPanelUpdated = false; // to update leadership panel
    private bool regionDisplayTiltActive = false; // to show tilt status

    // tile panel movement vars
    private float tilePanelCurrentAngle = 0f;

    void Awake()
    {    
        tileMapPanel = GameObject.Find("Tile Map Panel");
        stellarographyPanel = GameObject.Find("Stellarography Panel");
        industryDisplayButton = GameObject.Find("Industry Display Button");
        economicDisplayButton = GameObject.Find("Economic Display Button");
        graphicsDataRef = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
        intelLevelText = GameObject.Find("Intel Level Text Label").GetComponent<Text>();
        intelLevelValue = GameObject.Find("Intel Level").GetComponent<Text>();
        planetValueText = GameObject.Find("Planet Value Label").GetComponent<Text>();
        planetValue = GameObject.Find("Planet Value").GetComponent<Text>();
        starbaseText = GameObject.Find("Starbase Level Label").GetComponent<Text>();
        starbaseValue = GameObject.Find("Starbase Level").GetComponent<Text>();
        starbaseDataPanel = GameObject.Find("Starbase Data Panel").GetComponent<Image>();
        tradeHubIndicator = GameObject.Find("Trade Hub Indicator").GetComponent<Text>();
        throughputRemaining = GameObject.Find("Capacity Remaining").GetComponent<Text>();
        tileMapLight = GameObject.Find("Region UI Light").GetComponent<Light>();
        edictPanel = GameObject.Find("Edict Panel");
        wireFrameOverlay = GameObject.Find("Wireframe Planet Overlay");
        planetButtonBarBackground = GameObject.Find("Planet Button Bar Background");
        gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
        planetCanvas = GameObject.Find("Planet UI Canvas").GetComponent<Canvas>();
        //viceroyImageTop = GameObject.Find("Character Image").GetComponent<Image>();
    }

	// Use this for initialization
	void Start () 
    {
        gScriptRef = GameObject.Find("Main Camera").GetComponent<GalaxyCameraScript>(); // tie the game camera script to the data
        gScreen = GameObject.Find("GameEngine").GetComponent<GalaxyScreen>();     
        wireFrameOverlay.SetActive(false);
        planetButtonBarBackground.SetActive(false);     
        tileMapPanel.SetActive(false);
        stellarographyPanel.SetActive(false);
        edictPanel.SetActive(false);
        tileMapLight.enabled = false;
        tilePanelOriginalRotation = tileMapPanel.transform.rotation;
	}

    void OnGUI()
    {
        // set ratios against native resolution (1920 X 1080)
        screenWidthRatio = ((float)Screen.width / 1920f);
        screenHeightRatio = ((float)Screen.height / 1080f);

        if (gScriptRef.zoomLevel == GalaxyCameraScript.cameraZoomLevel.Planet)
        {
            if (!planetDataLoaded) // load selected planet data into pData ref if needed or changed
            {
                pData = gScreen.GetSelectedPlanet().GetComponent<Planet>().planetData;
                selectedPlanet = gScreen.GetSelectedPlanet();
                planetCurrentPosition = selectedPlanet.transform.position;
               
                wireFrameOverlay.transform.SetParent(selectedPlanet.transform); // attach the wireframe to the planet as a child
                wireFrameOverlay.transform.localScale = new Vector3(1, 1, 1); // selectedPlanet.transform.localScale;
                wireFrameOverlay.transform.localPosition = new Vector3(0, 0, 0); // center the overlay relative to the planet sprite
                planetCurrentScale = selectedPlanet.transform.localScale;
                planetTargetScale = new Vector3(planetCurrentScale.x * .5f, planetCurrentScale.y * .5f, planetCurrentScale.z * .5f);       
                planetTargetPosition = new Vector3(planetCurrentPosition.x - (18 * screenWidthRatio), planetCurrentPosition.y - 23, planetCurrentPosition.z);
                wireframeTargetPosition = new Vector3(wireframeCurrentPosition.x - 20.2f, wireframeCurrentPosition.y + 20.3f, wireframeCurrentPosition.z);
                planetColor = gScreen.GetSelectedPlanet().GetComponent<Planet>().GetComponent<SpriteRenderer>().color;
                planetDataLoaded = true;

                // show intel info
                intelLevelText.text = "PLANET INTEL LEVEL";
                intelLevelValue.enabled = true;
                intelLevelValue.text = pData.IntelLevel.ToString("N0");
                planetValue.enabled = true;
                planetValueText.enabled = true;
                intelLevelValue.GetComponent<Text>().enabled = true;
                planetInfoInitialized = true;

                if ((pData.IntelLevel < 10) || (!pData.IsInhabited))
                {
                    HideAllButtons();
                }
                else
                {
                    ShowAllButtons();
                }
                ShowPlanetView();
            }
           
            if (!tileMapGenerated)
                if (pData.Size > 0)
                {
                    GenerateTileMap();
                    
                }
            
            FadePlanetUIObjects();
            UpdateUIPanels();
        }
        else
        {
            ResetDrawStates();
        }      
    }

    void ShowAllButtons()
    {
        industryDisplayButton.SetActive(true); // else enable them
        economicDisplayButton.SetActive(true);
    }

    void HideAllButtons()
    {
        industryDisplayButton.SetActive(false); // disable the mode buttons if uninhabited or not enough Intel
        economicDisplayButton.SetActive(false);
    }

    void UpdateUIPanels()
    {
        
        if (HelperFunctions.DataRetrivalFunctions.GetSystem(pData.SystemID).IntelValue < 10 && !gameDataRef.DebugMode)
        {
            regionDisplayMode = false; // reset background;
        }
        if (!regionDisplayMode)
        {
            tileMapLight.enabled = false;
            tileMapPanel.SetActive(false);
            tileMapPanel.transform.rotation = tilePanelOriginalRotation;
        }
        else
        {
            tileMapLight.enabled = true;
            tileMapPanel.SetActive(true);
                   
            // flatten the panel if desired
            if (Input.GetKeyUp(KeyCode.F))
            {
                ToggleRegionDisplayTiltMode();
            }
        }

        if (industryDisplayMode)
        {
            regionDisplayMode = false;
            economicDisplayMode = false;
            if (!subPanelExists)
            {
                DrawIndustryDisplay();
            }
        }
        else if (economicDisplayMode)
        {
            regionDisplayMode = false;
            industryDisplayMode = false;
            if (!subPanelExists)
            {
                DrawEconomicDisplay();
            }
           
        }
        else
        {
            ResetPlanetOverlay();
            EnableStarbaseData();
        }

        if (!planetInfoInitialized)
        {
            
        }
     
        if (pData.SurfaceScanLevel > .75f)
        {
            planetValue.text = HelperFunctions.StringConversions.ConvertValueToDescription(pData.BasePlanetValue).ToUpper();
            planetValue.color = HelperFunctions.StringConversions.GetTextValueColor(pData.BasePlanetValue);
        }
        else
        {
            planetValue.text = "UNKNOWN";
            planetValue.color = Color.red;
        }
    }

    void EnableStarbaseData()
    {
        if (pData.System.IntelValue == 10 && pData.IsInhabited)
        {
            starbaseDataPanel.enabled = true;
            starbaseText.enabled = true;
            starbaseValue.enabled = true;
            starbaseValue.text = pData.StarbaseLevel.ToString("N0");

            if (pData.StarbaseLevel > 0)
            {
                starbaseValue.color = Color.green;
            }
            else
            {
                starbaseValue.color = Color.red;
            }

            if (pData.IsSupplyPlanet)
            {
                tradeHubIndicator.enabled = true;
                tradeHubIndicator.text = "SUPPLYING " + HelperFunctions.DataRetrivalFunctions.GetPlanet(pData.SupplyToPlanetID).Name.ToUpper();
                tradeHubIndicator.color = Color.yellow;
            }
           
            if (pData.IsTradeHub)
            {
                tradeHubIndicator.enabled = true;
                tradeHubIndicator.text = "TRADE HUB";
                tradeHubIndicator.color = Color.green;
                throughputRemaining.enabled = true;
                throughputRemaining.text = pData.StarbaseCapacityRemaining.ToString("N1") + " CAPACITY REMAINING";
            }
        }      
    }

    void DisableStarbaseData()
    {
        starbaseDataPanel.enabled = false;
        starbaseText.enabled = false;
        starbaseValue.enabled = false;   
        tradeHubIndicator.enabled = false;
        throughputRemaining.enabled = false;    
    }

    
    void CreatePlanetOverlay()
    {
        wireFrameOverlay.SetActive(true);

        // set fade in here
        Color planetColor = new Color(1, 1, 1, planetAlphaValue / 255f);
        Color wireframeColor = new Color(1, 1, 1, wireAlphaValue / 255f);
        float planetFadeValue = 0f; // originally set at 110f
        float wireFrameFadeValue = 255f;

        StartCoroutine(FadeOutPlanet(planetFadeValue));
        gScreen.GetSelectedPlanet().GetComponent<Planet>().GetComponent<SpriteRenderer>().color = planetColor;

        StartCoroutine(FadeInWireframe(wireFrameFadeValue));
        wireFrameOverlay.GetComponent<SpriteRenderer>().color = wireframeColor;

    }

    void DrawIndustryDisplay()
    {
        if (!planetTransitionComplete)
        {
            CreatePlanetOverlay();
            DisableStarbaseData(); // hides starbase data panel

            // TEST TEST TEST to move planet to the upper-left
            float speed = 60.0f;
            float step = speed * Time.deltaTime;
            selectedPlanet.transform.position = Vector3.MoveTowards(selectedPlanet.transform.position, planetTargetPosition, step);

            // TEST TEST TEST to shrink the planet/wireframe
            StartCoroutine(ShrinkPlanet(planetTargetScale));
        }

         //then create panel if not already created
        if (!subPanelExists && planetTransitionComplete)
        {
            DisableStarbaseData();
            CreateResourceFlowPanel();
            CreateStockPanel();
            CreateProductionPlanPanel();
            subPanelExists = true;
        }
    }

    void DrawEconomicDisplay()
    {
        if (!planetTransitionComplete)
        {
            CreatePlanetOverlay();
            DisableStarbaseData(); // hides stabase data panel

            // TEST TEST TEST to move planet to the upper-left
            float speed = 60.0f;
            float step = speed * Time.deltaTime;
            selectedPlanet.transform.position = Vector3.MoveTowards(selectedPlanet.transform.position, planetTargetPosition, step);

            // TEST TEST TEST to shrink the planet/wireframe
            StartCoroutine(ShrinkPlanet(planetTargetScale));
        }

        // then create panel if not already created
        if (!subPanelExists && planetTransitionComplete)
        {
            CreateEconomicPanel();
            subPanelExists = true;
        }
    }

    void CreateEconomicPanel()
    {
        GameObject pPanel = Instantiate(economicPanel, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        pPanel.transform.SetParent(planetCanvas.transform, false);
        pPanel.GetComponent<RectTransform>().pivot = new Vector2(.33f, 1f); // set pivot to the left
        pPanel.transform.localScale = new Vector3(1.1f * screenWidthRatio, 1.1f * screenWidthRatio, 1.1f);
        pPanel.transform.localPosition = new Vector2(70, edictPanel.GetComponent<RectTransform>().localPosition.y);

        pPanel.name = "Panel"; // give name to panel to find later for destruction
        planetObjectsDrawnList.Add(pPanel); // add to view object list

        pPanel.transform.Find("GPP Total").GetComponent<Text>().text = HelperFunctions.StringConversions.ConvertFloatDollarToText(pData.GrossPlanetaryProduct);
        pPanel.transform.Find("Base GPP Total").GetComponent<Text>().text = HelperFunctions.StringConversions.ConvertFloatDollarToText(pData.BaseGrossPlanetaryProduct);
        pPanel.transform.Find("Export Total").GetComponent<Text>().text = HelperFunctions.StringConversions.ConvertFloatDollarToText(pData.ExportRevenue);
        pPanel.transform.Find("Imports Total").GetComponent<Text>().text = HelperFunctions.StringConversions.ConvertFloatDollarToText(pData.ImportCosts);
        pPanel.transform.Find("Commerce Total").GetComponent<Text>().text = HelperFunctions.StringConversions.ConvertFloatDollarToText(pData.RetailRevenue);
        pPanel.transform.Find("Imports Max Total").GetComponent<Text>().text = pData.PercentGPPForImports.ToString("P1") + "(" + HelperFunctions.StringConversions.ConvertFloatDollarToText(pData.BaseGrossPlanetaryProduct * pData.PercentGPPForImports) + ")";
        pPanel.transform.Find("Exports Max Total").GetComponent<Text>().text = pData.PercentGPPForTrade.ToString("P1") + "(" + HelperFunctions.StringConversions.ConvertFloatDollarToText(pData.BaseGrossPlanetaryProduct * pData.PercentGPPForTrade) + ")";
        pPanel.transform.Find("Commerce Max Total").GetComponent<Text>().text = pData.FoodRetailPercentHold.ToString("P1");
    }

    void CreateStockPanel()
    {
        GameObject pPanel = Instantiate(stockPanel, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        pPanel.transform.SetParent(planetCanvas.transform, false);
        pPanel.GetComponent<RectTransform>().pivot = new Vector2(1f, 1f); // set pivot to the left
        pPanel.transform.localScale = new Vector3(1.1f * screenWidthRatio, 1.1f * screenWidthRatio, 1.1f);
        pPanel.transform.localPosition = new Vector2(edictPanel.GetComponent<RectTransform>().localPosition.x - (edictPanel.GetComponent<RectTransform>().rect.width) - 20, edictPanel.GetComponent<RectTransform>().localPosition.y - (pPanel.GetComponent<RectTransform>().rect.height) - 11);

        pPanel.name = "Panel"; // give name to panel to find later for destruction
        planetObjectsDrawnList.Add(pPanel); // add to view object list

        // populate text fields
        pPanel.transform.Find("Energy Stock Level").GetComponent<Text>().text = pData.EnergyStored.ToString("N1");
        if (pData.EnergyStored == 0)
        {
            pPanel.transform.Find("Energy Stock Level").GetComponent<Text>().color = Color.red;
        }

        pPanel.transform.Find("Food Stock Level").GetComponent<Text>().text = pData.FoodStored.ToString("N1");
        if (pData.FoodStored == 0)
        {
            pPanel.transform.Find("Food Stock Level").GetComponent<Text>().color = Color.red;
        }

        pPanel.transform.Find("Alpha Stock Level").GetComponent<Text>().text = pData.AlphaStored.ToString("N1");
        if (pData.AlphaStored == 0)
        {
            pPanel.transform.Find("Alpha Stock Level").GetComponent<Text>().color = Color.red;
        }

        pPanel.transform.Find("Heavy Stock Level").GetComponent<Text>().text = pData.HeavyStored.ToString("N1");
        if (pData.HeavyStored == 0)
        {
            pPanel.transform.Find("Heavy Stock Level").GetComponent<Text>().color = Color.red;
        }

        pPanel.transform.Find("Rare Stock Level").GetComponent<Text>().text = pData.RareStored.ToString("N1");
        if (pData.RareStored == 0)
        {
            pPanel.transform.Find("Rare Stock Level").GetComponent<Text>().color = Color.red;
        }

        // stock prices
        pPanel.transform.Find("Food Price").GetComponent<Text>().text = pData.Owner.CurrentFoodPrice.ToString("N1");
        pPanel.transform.Find("Energy Price").GetComponent<Text>().text = pData.Owner.CurrentEnergyPrice.ToString("N1");
        pPanel.transform.Find("Alpha Price").GetComponent<Text>().text = pData.Owner.CurrentAlphaPrice.ToString("N1");
        pPanel.transform.Find("Heavy Price").GetComponent<Text>().text = pData.Owner.CurrentHeavyPrice.ToString("N1");
        pPanel.transform.Find("Rare Price").GetComponent<Text>().text = pData.Owner.CurrentRarePrice.ToString("N1");

    }

    void CreateProductionPlanPanel()
    {
        GameObject pPanel = Instantiate(productionPlanPanel, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        pPanel.transform.SetParent(planetCanvas.transform, false);
        pPanel.GetComponent<RectTransform>().pivot = new Vector2(1f, 1f); // set pivot to the upper left
        pPanel.transform.localScale = new Vector3(1f * screenWidthRatio, 1f * screenWidthRatio, 1f);
        pPanel.transform.localPosition = new Vector2(edictPanel.GetComponent<RectTransform>().localPosition.x - (edictPanel.GetComponent<RectTransform>().rect.width) - 20, edictPanel.GetComponent<RectTransform>().localPosition.y - (pPanel.GetComponent<RectTransform>().rect.height) - 150);

        pPanel.name = "Panel"; // give name to panel to find later for destruction
        planetObjectsDrawnList.Add(pPanel); // add to view object list
        pPanel.transform.Find("Farm Build Allocation").GetComponent<Text>().text = pData.BuildPlan.FarmsAllocation.ToString("P0");
        pPanel.transform.Find("High Tech Build Allocation").GetComponent<Text>().text = pData.BuildPlan.HighTechAllocation.ToString("P0");
        pPanel.transform.Find("Factory Build Allocation").GetComponent<Text>().text = pData.BuildPlan.FactoryAllocation.ToString("P0");
        pPanel.transform.Find("Admin Build Allocation").GetComponent<Text>().text = pData.BuildPlan.AdminAllocation.ToString("P0");
        pPanel.transform.Find("Mines Build Allocation").GetComponent<Text>().text = pData.BuildPlan.MineAllocation.ToString("P0");
        pPanel.transform.Find("Labs Build Allocation").GetComponent<Text>().text = pData.BuildPlan.LabsAllocation.ToString("P0");
        pPanel.transform.Find("Infra Build Allocation").GetComponent<Text>().text = pData.BuildPlan.InfraAllocation.ToString("P0");
        pPanel.transform.Find("Farm Builds Per Month").GetComponent<Text>().text = pData.PercentToNewFarmLevel.ToString("P0");
        pPanel.transform.Find("High Tech Builds Per Month").GetComponent<Text>().text = pData.PercentToNewHighTechLevel.ToString("P0");
        pPanel.transform.Find("Factory Builds Per Month").GetComponent<Text>().text = pData.PercentToNewFactoryLevel.ToString("P0");
        pPanel.transform.Find("Mine Builds Per Month").GetComponent<Text>().text = pData.PercentToNewMineLevel.ToString("P0");
        pPanel.transform.Find("Infra Builds Per Month").GetComponent<Text>().text = pData.PercentToNewMineLevel.ToString("P0");

        // reserve for farm build levels/month
    }

    void CreateResourceFlowPanel()
    {
        GameObject pPanel = Instantiate(resourceFlowPanel, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        pPanel.transform.SetParent(planetCanvas.transform, false);
        pPanel.GetComponent<RectTransform>().pivot = new Vector2(1f, 1f); // set pivot to the left
        pPanel.transform.localScale = new Vector3(1.2f * screenWidthRatio, 1.2f * screenWidthRatio, 1.1f);
        pPanel.transform.localPosition = new Vector2(edictPanel.GetComponent<RectTransform>().localPosition.x - (edictPanel.GetComponent<RectTransform>().rect.width) - 20, edictPanel.GetComponent<RectTransform>().localPosition.y);
        
        pPanel.name = "Panel"; // give name to panel to find later for destruction
        planetObjectsDrawnList.Add(pPanel); // add to view object list
        pPanel.transform.Find("Energy Production").GetComponent<Text>().text = pData.TotalEnergyGenerated.ToString("N1");
        pPanel.transform.Find("Food Production").GetComponent<Text>().text = pData.TotalFoodGenerated.ToString("N1");
        pPanel.transform.Find("Alpha Production").GetComponent<Text>().text = pData.TotalAlphaMaterialsGenerated.ToString("N1");
        pPanel.transform.Find("Heavy Production").GetComponent<Text>().text = pData.TotalHeavyMaterialsGenerated.ToString("N1");
        pPanel.transform.Find("Rare Production").GetComponent<Text>().text = pData.TotalRareMaterialsGenerated.ToString("N1");
        pPanel.transform.Find("Energy Usage").GetComponent<Text>().text = pData.TotalEnergyConsumed.ToString("N1");
        pPanel.transform.Find("Food Usage").GetComponent<Text>().text = pData.TotalFoodConsumed.ToString("N1");
        pPanel.transform.Find("Alpha Usage").GetComponent<Text>().text = pData.TotalAlphaMaterialsConsumed.ToString("N1");
        pPanel.transform.Find("Heavy Usage").GetComponent<Text>().text = pData.TotalHeavyMaterialsConsumed.ToString("N1");
        pPanel.transform.Find("Rare Usage").GetComponent<Text>().text = pData.TotalRareMaterialsConsumed.ToString("N1");
        pPanel.transform.Find("Energy Trade").GetComponent<Text>().text = pData.EnergyTrade.ToString("N1");
        pPanel.transform.Find("Alpha BPs Generated").GetComponent<Text>().text = pData.AlphaBPsGeneratedMonthly.ToString("N1");
        pPanel.transform.Find("Heavy BPs Generated").GetComponent<Text>().text = pData.HeavyBPsGeneratedMonthly.ToString("N1");
        pPanel.transform.Find("Rare BPs Generated").GetComponent<Text>().text = pData.RareBPsGeneratedMonthly.ToString("N1");

        if (pData.EnergyTrade < 0)
            pPanel.transform.Find("Energy Trade").GetComponent<Text>().color = Color.red;
        pPanel.transform.Find("Food Trade").GetComponent<Text>().text = pData.FoodTrade.ToString("N1");
        if (pData.FoodTrade < 0)
            pPanel.transform.Find("Food Trade").GetComponent<Text>().color = Color.red;
        pPanel.transform.Find("Alpha Trade").GetComponent<Text>().text = pData.AlphaTrade.ToString("N1");
        if (pData.AlphaTrade < 0)
            pPanel.transform.Find("Alpha Trade").GetComponent<Text>().color = Color.red;
        pPanel.transform.Find("Heavy Trade").GetComponent<Text>().text = pData.HeavyTrade.ToString("N1");
        if (pData.HeavyTrade < 0)
            pPanel.transform.Find("Heavy Trade").GetComponent<Text>().color = Color.red;
        pPanel.transform.Find("Rare Trade").GetComponent<Text>().text = pData.RareTrade.ToString("N1");
        if (pData.RareTrade < 0)
            pPanel.transform.Find("Rare Trade").GetComponent<Text>().color = Color.red;
       
        pPanel.transform.Find("Food Difference").GetComponent<Text>().text = pData.FoodDifference.ToString("N1");
        if (pData.FoodDifference < 0)
            pPanel.transform.Find("Food Difference").GetComponent<Text>().color = Color.red;
        pPanel.transform.Find("Energy Difference").GetComponent<Text>().text = pData.EnergyDifference.ToString("N1");
        if (pData.EnergyDifference < 0)
            pPanel.transform.Find("Energy Difference").GetComponent<Text>().color = Color.red;
        pPanel.transform.Find("Alpha Difference").GetComponent<Text>().text = pData.AlphaPreProductionDifference.ToString("N1") + "(" + pData.AlphaTotalDifference.ToString("N1") + ")";
        if (pData.AlphaPreProductionDifference < 0)
            pPanel.transform.Find("Alpha Difference").GetComponent<Text>().color = Color.red;
        pPanel.transform.Find("Heavy Difference").GetComponent<Text>().text = pData.HeavyPreProductionDifference.ToString("N1") + "(" + pData.HeavyTotalDifference.ToString("N1") + ")";
        if (pData.HeavyPreProductionDifference < 0)
            pPanel.transform.Find("Heavy Difference").GetComponent<Text>().color = Color.red;
        pPanel.transform.Find("Rare Difference").GetComponent<Text>().text = pData.RarePreProductionDifference.ToString("N1") + "(" + pData.RareTotalDifference.ToString("N1") + ")";
        if (pData.RarePreProductionDifference < 0)
            pPanel.transform.Find("Rare Difference").GetComponent<Text>().color = Color.red;
    }

    void ResetPlanetOverlay()
    {
        wireFrameOverlay.SetActive(true);

        // set fade in here
        Color planetColor = new Color(1, 1, 1, planetAlphaValue / 255f);
        Color wireframeColor = new Color(1, 1, 1, wireAlphaValue / 255f);
        float planetFadeValue = 255f;
        float wireFrameFadeValue = 0f;

        StartCoroutine(FadeInPlanet(planetFadeValue));
        gScreen.GetSelectedPlanet().GetComponent<Planet>().GetComponent<SpriteRenderer>().color = planetColor;

        StartCoroutine(FadeOutWireframe(wireFrameFadeValue));
        wireFrameOverlay.GetComponent<SpriteRenderer>().color = wireframeColor;

        // TEST TEST TEST to move planet to the upper-left
        float speed = 60.0f;
        float step = speed * Time.deltaTime;
        selectedPlanet.transform.position = Vector3.MoveTowards(selectedPlanet.transform.position, planetCurrentPosition, step);

        // TEST TEST TEST to move the planet/wireframe back
        StartCoroutine(RestorePlanet(planetCurrentScale));

        // destroy any panel
        if (planetObjectsDrawnList.Exists(p => p.name == "Panel"))
        {
            subPanelExists = false;        
            GameObject.Destroy(planetObjectsDrawnList.Find(p => p.name == "Panel")); // destroy the panel
            planetObjectsDrawnList.Remove(planetObjectsDrawnList.Find(p => p.name == "Panel")); // then remove it
        }    
    }
   
    void ResetDrawStates()
    {      
        planetButtonBarBackground.SetActive(false); // turn off background       
        alphaValue = 0f;
        planetAlphaValue = 255f;
        wireAlphaValue = 0f;
        if (selectedPlanet != null)
        {
            selectedPlanet.GetComponent<Planet>().GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f); // reset the planet sprite on the system display
            selectedPlanet.transform.localScale = new Vector3(selectedPlanet.GetComponent<Planet>().planetData.PlanetSystemScaleSize, selectedPlanet.GetComponent<Planet>().planetData.PlanetSystemScaleSize, 1);
            selectedPlanet.transform.localPosition = planetCurrentPosition; // reset the planet's position
        }
        
        planetDataLoaded = false; // reset data load toggle
        planetDataBoxGenerated = false;
        leadershipPanelUpdated = false;

        foreach (GameObject tile in tileList)
            GameObject.DestroyObject(tile);

        tileList.Clear();
        planetValue.enabled = false;
        regionDisplayTiltActive = false;
        planetValueText.enabled = false;
        starbaseDataPanel.enabled = false;
        starbaseText.enabled = false;
        starbaseValue.enabled = false;
        tradeHubIndicator.enabled = false;
        throughputRemaining.enabled = false;
        tileMapGenerated = false;
        industryDisplayMode = false;
        economicDisplayMode = false;
        planetInfoInitialized = false;
        subPanelExists = false;
        planetTransitionComplete = false;
        tileMapPanel.SetActive(false);
        stellarographyPanel.SetActive(false);
        edictPanel.SetActive(false);
        wireFrameOverlay.transform.parent = null; // remove the wireframe from the planet object
        wireFrameOverlay.SetActive(false);

        // delete all other objects in scene
        foreach (GameObject pObject in planetObjectsDrawnList)
        {
            Destroy(pObject);
        }
        planetObjectsDrawnList.Clear();
        selectedPlanet = null; //clear the selected planet
                   
    }

    void ShowEdictPanel()
    {
        Vector3 boxLocation;
        
        boxLocation = new Vector3((Screen.width / 2) - 10, 300 * screenHeightRatio, 0); // where the edict box is located, test       
        edictPanel.SetActive(true);
        edictPanel.GetComponent<RectTransform>().pivot = new Vector2(1, 1); // set pivot to upper-right
        edictPanel.transform.localPosition = boxLocation;
        edictPanel.transform.localScale = new Vector2(1 * screenWidthRatio, 1 * screenWidthRatio);

        if (!leadershipPanelUpdated)
        {
            UpdateLeadershipChain();
            leadershipPanelUpdated = true;
        }
    }

    void ShowPlanetView()
    {
        if (HelperFunctions.DataRetrivalFunctions.GetSystem(pData.SystemID).IntelValue >= 5 || gameDataRef.DebugMode)
        {            
            planetButtonBarBackground.SetActive(true); // turn on background;
            stellarographyPanel.SetActive(true);
            UpdateStellarData(); // updates any changes to stellar data panels
            
            if (pData.IsInhabited)
            {
                ShowEdictPanel();
            }
          
            if (!planetDataBoxGenerated) // show planet data if generated
            {
                ShowPlanetDataBox();
            }
            //if (pData.IsInhabited)
            //{
            //    viceroyImageTop.enabled = true;
            //    string vID = HelperFunctions.DataRetrivalFunctions.GetPlanetViceroyID(pData.ID);
            //    if (vID != "none")
            //    {
            //        Character vData = HelperFunctions.DataRetrivalFunctions.GetCharacter(vID);
            //        if (graphicsDataRef.CharacterList.Find(p => p.name == vData.PictureID))
            //        {
            //            viceroyImageTop.sprite = graphicsDataRef.CharacterList.Find(p => p.name == vData.PictureID);
            //        }
            //        else
            //            viceroyImageTop.enabled = false;
            //    }             
            //}
            //else
            //    viceroyImageTop.enabled = false;
        }
        
        // UpdateUIPanels();
        Color planetBarBackgroundColor = planetButtonBarBackground.GetComponent<Image>().color;
        StartCoroutine(FadeInAlpha(255f));
        planetBarBackgroundColor = new Color(1, 1, 1, alphaValue / 255f);
        if (tileMapGenerated)
        {
            StartCoroutine(FadeInAlpha(255f));
            tileMapPanel.GetComponent<Image>().color = new Color(1, 1, 1, alphaValue / 255f);
        }       
    }

    void UpdateLeadershipChain()
    {
        StarData sData = HelperFunctions.DataRetrivalFunctions.GetSystem(pData.SystemID); // get system ID
    
        // define the 4 groups of data
        Transform domesticPrimeInfo = edictPanel.transform.Find("Domestic Prime Image");
        Transform provinceGovernorInfo = edictPanel.transform.Find("Province Governor Image");
        Transform systemGovernorInfo = edictPanel.transform.Find("System Governor Image");
        Transform viceroyInfo = edictPanel.transform.Find("Viceroy Image");

        if (pData.IsInhabited)
        {
            if (!gameDataRef.CivList[0].PlanetIDList.Exists(p => p == pData.ID)) // if the planet is not owned by your civ, disable the panels except for viceroy
            {
                domesticPrimeInfo.gameObject.SetActive(false);
                provinceGovernorInfo.gameObject.SetActive(false);
                systemGovernorInfo.gameObject.SetActive(false);
                if (HelperFunctions.DataRetrivalFunctions.GetPlanetViceroyID(pData.ID) != "none")
                {
                    viceroyInfo.gameObject.SetActive(true);
                    string vID = HelperFunctions.DataRetrivalFunctions.GetPlanetViceroyID(pData.ID);
                    Character vGov = HelperFunctions.DataRetrivalFunctions.GetCharacter(vID);
                    viceroyInfo.GetComponent<CharacterTooltip>().InitializeTooltipData(vGov, -21f); // set up the tooltip
                    viceroyInfo.GetComponent<Image>().sprite = graphicsDataRef.CharacterList.Find(p => p.name == vGov.PictureID);
                    string charName = vGov.Name.ToUpper();
                    if (vGov.HouseID != null)
                    {
                        charName += " OF " + HelperFunctions.DataRetrivalFunctions.GetHouse(vGov.HouseID).Name.ToUpper();
                    }
                    viceroyInfo.Find("Viceroy Name").GetComponent<Text>().text = charName;
                    if (HelperFunctions.DataRetrivalFunctions.GetPlanet(vGov.PlanetLocationID) != null)
                    {
                        viceroyInfo.Find("Viceroy Location").GetComponent<Text>().text = "LOCATED ON " + HelperFunctions.DataRetrivalFunctions.GetPlanet(vGov.PlanetLocationID).Name.ToUpper();
                    }
                }
                else
                {
                    viceroyInfo.gameObject.SetActive(false);
                }
                return;
            }
        }

        // set domestic prime image
        if (HelperFunctions.DataRetrivalFunctions.GetPrime(Character.eRole.DomesticPrime) != "none")
        {
            domesticPrimeInfo.gameObject.SetActive(true);
            string pID = HelperFunctions.DataRetrivalFunctions.GetPrime(Character.eRole.DomesticPrime);
            Character pGov = HelperFunctions.DataRetrivalFunctions.GetCharacter(pID);
            domesticPrimeInfo.GetComponent<CharacterTooltip>().InitializeTooltipData(pGov, -25f);
            domesticPrimeInfo.GetComponent<CharacterScreenActivation>().InitializeData(pGov);
            domesticPrimeInfo.GetComponent<Image>().sprite = graphicsDataRef.CharacterList.Find(p => p.name == pGov.PictureID);
            domesticPrimeInfo.Find("Domestic Prime Name").GetComponent<Text>().text = pGov.Name.ToUpper() + " OF " + HelperFunctions.DataRetrivalFunctions.GetHouse(pGov.HouseID).Name.ToUpper();

            if (pGov.AssignedHouse.Rank == House.eHouseRank.Great)
            {
                domesticPrimeInfo.Find("Domestic Prime Name").GetComponent<Text>().color = Color.yellow; // if great house, change color
            }
            else if (pGov.AssignedHouse.Rank == House.eHouseRank.Minor)
            {
                domesticPrimeInfo.Find("Domestic Prime Name").GetComponent<Text>().color = Color.green; // if great house, change color
            }
            else
            {
                domesticPrimeInfo.Find("Domestic Prime Name").GetComponent<Text>().color = Color.white; // if great house, change color
            }

            if (HelperFunctions.DataRetrivalFunctions.GetPlanet(pGov.PlanetLocationID) != null)
            {
                domesticPrimeInfo.Find("Domestic Prime Location").GetComponent<Text>().text = "LOCATED ON " + HelperFunctions.DataRetrivalFunctions.GetPlanet(pGov.PlanetLocationID).Name.ToUpper();
            }
        }
        else
        {
            domesticPrimeInfo.gameObject.SetActive(false);
        }
        
        // set province governor image  
        if (HelperFunctions.DataRetrivalFunctions.GetProvinceGovernorID(sData.AssignedProvinceID) != "none")
        {
            provinceGovernorInfo.gameObject.SetActive(true);         
            string pID = HelperFunctions.DataRetrivalFunctions.GetProvinceGovernorID(sData.AssignedProvinceID);
            Character pGov = HelperFunctions.DataRetrivalFunctions.GetCharacter(pID);
            provinceGovernorInfo.GetComponent<CharacterTooltip>().InitializeTooltipData(pGov, -23f);
            provinceGovernorInfo.GetComponent<CharacterScreenActivation>().InitializeData(pGov);
            provinceGovernorInfo.GetComponent<Image>().sprite = graphicsDataRef.CharacterList.Find(p => p.name == pGov.PictureID);
            provinceGovernorInfo.Find("Province Governor Name").GetComponent<Text>().text = pGov.Name.ToUpper() + " OF " + HelperFunctions.DataRetrivalFunctions.GetHouse(pGov.HouseID).Name.ToUpper();
            if (pGov.AssignedHouse.Rank == House.eHouseRank.Great)
            {
                provinceGovernorInfo.Find("Province Governor Name").GetComponent<Text>().color = Color.yellow; // if great house, change color
            }
            else if (pGov.AssignedHouse.Rank == House.eHouseRank.Minor)
            {
                provinceGovernorInfo.Find("Province Governor Name").GetComponent<Text>().color = Color.green; // if great house, change color
            }
            else
            {
                provinceGovernorInfo.Find("Province Governor Name").GetComponent<Text>().color = Color.white; // if great house, change color
            }
            if (HelperFunctions.DataRetrivalFunctions.GetPlanet(pGov.PlanetLocationID) != null)
            {
                provinceGovernorInfo.Find("Province Governor Location").GetComponent<Text>().text = "LOCATED ON " + HelperFunctions.DataRetrivalFunctions.GetPlanet(pGov.PlanetLocationID).Name.ToUpper();
            }

            // set color of label for support
            Color supportColor;
            if (pData.ProvGovSupport == eSupportLevel.Full)
            {
                supportColor = Color.green;
            }
            else if (pData.ProvGovSupport == eSupportLevel.Partial)
            {
                supportColor = Color.yellow;
            }
            else
            {
                supportColor = Color.red;
            }
            provinceGovernorInfo.Find("Province Governor Label").GetComponent<Text>().color = supportColor;
        }
        else
        {
            provinceGovernorInfo.gameObject.SetActive(false);
        }

        // set system governor image
        if (HelperFunctions.DataRetrivalFunctions.GetSystemGovernorID(sData.ID) != "none")
        {
            systemGovernorInfo.gameObject.SetActive(true);
            string sID = HelperFunctions.DataRetrivalFunctions.GetSystemGovernorID(sData.ID);
            Character sGov = HelperFunctions.DataRetrivalFunctions.GetCharacter(sID);
            systemGovernorInfo.GetComponent<CharacterTooltip>().InitializeTooltipData(sGov, -22f);
            systemGovernorInfo.GetComponent<CharacterScreenActivation>().InitializeData(sGov);
            systemGovernorInfo.GetComponent<Image>().sprite = graphicsDataRef.CharacterList.Find(p => p.name == sGov.PictureID);
            systemGovernorInfo.Find("System Governor Name").GetComponent<Text>().text = sGov.Name.ToUpper() + " OF " + HelperFunctions.DataRetrivalFunctions.GetHouse(sGov.HouseID).Name.ToUpper();
            if (sGov.AssignedHouse.Rank == House.eHouseRank.Great)
            {
                systemGovernorInfo.Find("System Governor Name").GetComponent<Text>().color = Color.yellow; // if great house, change color
            }
            else if (sGov.AssignedHouse.Rank == House.eHouseRank.Minor)
            {
                systemGovernorInfo.Find("System Governor Name").GetComponent<Text>().color = Color.green; // if great house, change color
            }
            else
            {
                systemGovernorInfo.Find("System Governor Name").GetComponent<Text>().color = Color.white; // if great house, change color
            }
            if (HelperFunctions.DataRetrivalFunctions.GetPlanet(sGov.PlanetLocationID) != null)
            {
                systemGovernorInfo.Find("System Governor Location").GetComponent<Text>().text = "LOCATED ON " + HelperFunctions.DataRetrivalFunctions.GetPlanet(sGov.PlanetLocationID).Name.ToUpper();
            }

            // set color of label for support
            Color supportColor;
            if (pData.SysGovSupport == eSupportLevel.Full)
            {
                supportColor = Color.green;
            }
            else if (pData.SysGovSupport == eSupportLevel.Partial)
            {
                supportColor = Color.yellow;
            }
            else
            {
                supportColor = Color.red;
            }
            systemGovernorInfo.Find("System Governor Label").GetComponent<Text>().color = supportColor;
        }
        else
        {
            systemGovernorInfo.gameObject.SetActive(false);
        }

        // set viceroy image
        if (HelperFunctions.DataRetrivalFunctions.GetPlanetViceroyID(pData.ID) != "none")
        {
            viceroyInfo.gameObject.SetActive(true);
            string vID = HelperFunctions.DataRetrivalFunctions.GetPlanetViceroyID(pData.ID);
            Character vGov = HelperFunctions.DataRetrivalFunctions.GetCharacter(vID);
            viceroyInfo.GetComponent<CharacterTooltip>().InitializeTooltipData(vGov, -21f); // set up the tooltip
            viceroyInfo.GetComponent<CharacterScreenActivation>().InitializeData(vGov); // set up data for character screen
            viceroyInfo.GetComponent<Image>().sprite = graphicsDataRef.CharacterList.Find(p => p.name == vGov.PictureID);
            viceroyInfo.Find("Viceroy Name").GetComponent<Text>().text = vGov.Name.ToUpper() + " OF " + HelperFunctions.DataRetrivalFunctions.GetHouse(vGov.HouseID).Name.ToUpper();
            if (vGov.AssignedHouse.Rank == House.eHouseRank.Great)
            {
                viceroyInfo.Find("Viceroy Name").GetComponent<Text>().color = Color.yellow; // if great house, change color
            }
            else if (vGov.AssignedHouse.Rank == House.eHouseRank.Minor)
            {
                viceroyInfo.Find("Viceroy Name").GetComponent<Text>().color = Color.green; // if great house, change color
            }
            else
            {
                viceroyInfo.Find("Viceroy Name").GetComponent<Text>().color = Color.white; // if great house, change color
            }
            if (HelperFunctions.DataRetrivalFunctions.GetPlanet(vGov.PlanetLocationID) != null)
            {
                viceroyInfo.Find("Viceroy Location").GetComponent<Text>().text = "LOCATED ON " + HelperFunctions.DataRetrivalFunctions.GetPlanet(vGov.PlanetLocationID).Name.ToUpper();
            }
        }
        else
        {
            viceroyInfo.gameObject.SetActive(false);
        }
    }

    void ShowPlanetDataBox()
    {
        Vector3 boxLocation;
        PlanetData planetData = pData; // ref for planet's data
                    
        // set the planet data box position relative to the planet's world location
        boxLocation = new Vector3(0, 300 * screenHeightRatio, 0); // where the text box is located, test
        GameObject pPanel = Instantiate(planetSummaryPanel, boxLocation, Quaternion.identity) as GameObject; // draw the panel
        pPanel.transform.SetParent(planetCanvas.transform);
        boxLocation = new Vector3(boxLocation.x - (Screen.width / 2) + (25 * screenWidthRatio), boxLocation.y, -10);
        pPanel.GetComponent<RectTransform>().pivot = new Vector2(0, 1); // sets the pivot point      
        pPanel.transform.localPosition = boxLocation; //  new Vector3(-470 + (x * 325), -160, 0); //x = 435; y = 245                    

        pPanel.GetComponent<PlanetDataBox>().PopulateDataBox(pData.ID);
        pPanel.transform.Find("Viceroy Image").gameObject.SetActive(false); // disable the viceroy pic on planet screen
        pPanel.transform.localScale = new Vector2(1.1f * screenWidthRatio, 1.16f * screenWidthRatio);    
        planetObjectsDrawnList.Add(pPanel);
        stellarographyPanel.transform.localPosition = boxLocation;
        stellarographyPanel.transform.localScale = new Vector2(1 * screenWidthRatio, 1 * screenWidthRatio);
        planetDataBoxGenerated = true;            
    }

    void UpdateStellarData()
    {
        GenerateGameObject.GeneratePlanetDescription(pData);
        stellarographyPanel.transform.Find("Planetary History Text").GetComponent<Text>().text = pData.Description.ToUpper();
        stellarographyPanel.transform.Find("Diameter Value").GetComponent<Text>().text = pData.Diameter.ToString("N0") + " km";
        stellarographyPanel.transform.Find("Axial Tilt Value").GetComponent<Text>().text = pData.AxialTilt.ToString("N1") + " deg";
        stellarographyPanel.transform.Find("Atmosphere Scan Level").GetComponent<Text>().text = pData.AtmosphereScanLevel.ToString("P0") + " SCANNED";
        stellarographyPanel.transform.Find("Surface Scan Level").GetComponent<Text>().text = pData.SurfaceScanLevel.ToString("P0") + " SCANNED";
        stellarographyPanel.transform.Find("Interior Scan Level").GetComponent<Text>().text = pData.InteriorScanLevel.ToString("P0") + " SCANNED";
    }

    void GenerateTileMap()
    {
        tileMapPanel.SetActive(true); // active tile panel
        tileMapPanel.transform.rotation = tilePanelOriginalRotation; // reset the rotation to the original

        // draw tiles
        int maxRowCount = 0;
        int maxColCount = 0;
        int curRow = 0;
        int curCol = 0;
        Vector2 topLeftOfTileMap;
        int tileSizeInUnits = 0; // size of tile prefab
        float squareRoot = Mathf.Sqrt(pData.MaxTiles); // get the square root
        int squareRound = Mathf.RoundToInt(squareRoot); // then round it

        maxColCount = squareRound; // set # of rows
        maxRowCount = squareRound;

        if (squareRound <= squareRoot) // if rounded down, add 1 to the squareround to always make rows equal
            maxRowCount += 1;

        maxColCount -= 1;
           
        for (int x = 0; x < pData.RegionList.Count; x++)
        {
            Region tempTile = new Region();
            GameObject tileImage;

            tempTile = pData.RegionList[x];

            if (tempTile.IsHabitable)
            {
                tileImage = Instantiate(habitableTile, new Vector3(0, 0, 0), Quaternion.identity) as GameObject; // create a new tile from prefab
            }
            else
                tileImage = Instantiate(uninhabitableTile, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;

            tileImage.transform.SetParent(tileMapPanel.transform, true); // assign to tile map
            tileList.Add(tileImage); // add to tile objects to be destroyed when view ends
            tileImage.name = tempTile.ID;
            tileImage.transform.localScale = new Vector3(42f, 42f, 0f); // make same size          

            // convert to screen coordinates
            tileSizeInUnits = (int)tileImage.GetComponent<RectTransform>().rect.width;

            topLeftOfTileMap = new Vector3(-(maxColCount * tileSizeInUnits) / 2, ((maxRowCount * tileSizeInUnits) - tileSizeInUnits) / 2, 0);
            tileImage.transform.localPosition = new Vector3(topLeftOfTileMap.x + (curCol * tileSizeInUnits), topLeftOfTileMap.y - (curRow * tileSizeInUnits), 0f);

            // update tile row and column coordinates
            if (curCol < maxColCount)
                curCol += 1;
            else
            {
                curCol = 0;
                curRow += 1;
            }

            // show development level current/max (eventually this will be a mode and moved to an outside function to query each tile in the tile list to change the value)

            // highlight if a targeted region in a build plan, otherwise show general habitability with graduated color
            if (pData.RegionList[x].IsHabitable)
            {
                float red = ((100 - (float)pData.RegionList[x].BioRating) * 2.55f) / 255f;
                float green = ((float)pData.RegionList[x].BioRating * 2.55f) / 255f;
                Color habColor = new Color(red, green, 0f);
                tileImage.GetComponent<MeshRenderer>().material.color = habColor;
            }
            else
                tileImage.GetComponent<MeshRenderer>().material.color = Color.grey;

            if (pData.RegionList[x].IsTargetedRegionForBuild)
                tileImage.GetComponent<MeshRenderer>().material.color = Color.yellow;
        }
        
        tileMapGenerated = true;        
    }

    public void ToggleRegionDisplayMode()
    {
        if (regionDisplayMode)
            regionDisplayMode = false;
        else
        {
            regionDisplayMode = true;
            StartCoroutine(TiltRegionPanel(.35f));

            int x = 0;
            foreach (GameObject tile in tileList)
            {
                float height = (float)pData.RegionList[x].HabitatationInfrastructureLevel + 1f;
                StartCoroutine(RaiseBars(height, tile));
                x += 1;
            }
           
        }
    }

    public void ToggleIndustryDisplayMode()
    {
        if (industryDisplayMode)
            industryDisplayMode = false;
        else
        {
            industryDisplayMode = true;
            economicDisplayMode = false;
        }
    }

    public void ToggleRegionDisplayTiltMode()
    {
        if (regionDisplayTiltActive)
            regionDisplayTiltActive = false;
        else
            regionDisplayTiltActive = true;

        if (!regionDisplayTiltActive)
        {
            StartCoroutine(TiltRegionPanel(.35f));
        }
        else
        {
            StartCoroutine(FlattenRegionPanel());
        }
    }

    public void ToggleEconomicDisplayMode()
    {
        if (economicDisplayMode)
            economicDisplayMode = false;
        else
        {
            industryDisplayMode = false;
            economicDisplayMode = true;
        }
    }

    void FadePlanetUIObjects()
    {    
        Color fadeColor = new Color(1, 1, 1, alphaValue / 255f);
        StartCoroutine(FadeInAlpha(150f));

        for (int y = 0; y < tileList.Count; y++)
        {
            GameObject pPanel = tileList[y];
        }

        for (int y = 0; y < planetObjectsDrawnList.Count; y++ )
        {
            planetObjectsDrawnList[y].GetComponent<Image>().color = fadeColor;
        }

        if (stellarographyPanel.activeSelf)
        {
            Color stellarPanelColor = stellarographyPanel.GetComponent<Image>().color;
            StartCoroutine(FadeInAlpha(255f));
            stellarographyPanel.GetComponent<Image>().color = fadeColor;
        }

        if (edictPanel.activeSelf)
        {
            Color edictPanelColor = edictPanel.GetComponent<Image>().color;
            StartCoroutine(FadeInAlpha(255f));
            edictPanel.GetComponent<Image>().color = fadeColor;
        }

    }

    IEnumerator TiltRegionPanel(float targetAngleTilt)
    {

        while (tileMapPanel.transform.rotation.x < targetAngleTilt)
        {          
            tileMapPanel.transform.Rotate(2.2f, 0f, 0f); // give it the tilted look
            yield return null;
            regionDisplayTiltActive = true;
        }
    }

    IEnumerator FlattenRegionPanel()
    {

        while (tileMapPanel.transform.rotation.x > 0f)
        {
            tileMapPanel.transform.Rotate(-2.2f, 0f, 0f); // give it the tilted look
            yield return null;
            regionDisplayTiltActive = false;
        }
    }

    IEnumerator FadeInAlpha(float targetAlpha)
    {
        while (alphaValue < targetAlpha)
        {
            alphaValue += .15f;
            yield return null;
        }
    }

    IEnumerator FadeOutPlanet(float targetAlpha)
    {
        while (planetAlphaValue > targetAlpha)
        {
            planetAlphaValue -= .5f;
            yield return null;
        }
    }

    IEnumerator FadeInPlanet(float targetAlpha)
    {
        while (planetAlphaValue < targetAlpha)
        {
            planetAlphaValue += .5f;
            yield return null;
        }
    }

    IEnumerator RaiseBars(float targetHeight, GameObject tile)
    {
        while (tile.transform.localScale.z < targetHeight)
        {
            tile.transform.localScale = new Vector3(45f,45f,tile.transform.localScale.z + 1.3f);
            tile.transform.localPosition = new Vector3(tile.transform.localPosition.x,tile.transform.localPosition.y, 0f - (tile.transform.localScale.z / 2f));
            yield return null;
        }
    }

    IEnumerator ShrinkPlanet(Vector3 targetSize)
    {
        Vector3 curSize = selectedPlanet.transform.localScale;
        while (curSize.x > targetSize.x)
        {
            if (selectedPlanet != null)
            {
                selectedPlanet.transform.localScale = new Vector3(curSize.x - .2f, curSize.y - .2f);
            }
            yield return null;
        }

    }

    IEnumerator RestorePlanet(Vector3 originalSize)
    {
        Vector3 curSize = selectedPlanet.transform.localScale;
        while (curSize.x < originalSize.x)
        {
            if (selectedPlanet != null)
            {
                selectedPlanet.transform.localScale = new Vector3(curSize.x + .2f, curSize.y + .2f);
            }
            yield return null;
        }
    }

    IEnumerator FadeInWireframe(float targetAlpha)
    {
        while (wireAlphaValue < targetAlpha)
        {
            wireAlphaValue += .5f;
            yield return null;
        }
        planetTransitionComplete = true;
    }

    IEnumerator FadeOutWireframe(float targetAlpha)
    {
        while (wireAlphaValue > targetAlpha)
        {
            wireAlphaValue -= .5f;
            yield return null;
        }
        planetTransitionComplete = false;
    }
}
