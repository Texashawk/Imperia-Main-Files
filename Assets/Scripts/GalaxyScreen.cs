using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using CameraScripts;
using StellarObjects;
using System.Collections.Generic;
using System.Collections;
using System;
using HelperFunctions;
using CivObjects;

namespace Screens.Galaxy
{
    public class GalaxyScreen : MonoBehaviour
    {
        enum eGalaxyNameFilter : int
        {
            NONE,
            BIO,
            OWNERSHIP,
            UNREST
        }

        public Font labelFont; // font for labels on screen
        public GameObject systemGrid;
        public GameObject systemTrail;
        private ModalPanel modalPanel; // for the modal panel

        private UnityAction myYesAction;
        private UnityAction myNoAction;
        private UnityAction myCancelAction;

        // font properties
        public GameObject starNameObject;

        // planet prefab variables
        public GameObject[] barrenPlanets;
        public GameObject[] asteroidBelts;
        public GameObject[] greenhousePlanets;
        public GameObject[] desertPlanets;
        public GameObject[] terranPlanets;
        public GameObject[] icePlanets;
        public GameObject[] dwarfJovianPlanets;
        public GameObject[] jovianPlanets;
        public GameObject[] lavaPlanets;
        public GameObject[] irradiatedPlanets;
        public GameObject[] waterPlanets;
        public GameObject[] superEarthPlanets;
        public GameObject[] brownDwarfPlanets;
        public GameObject[] iceBelts;
        public GameObject[] dustRings;
        public GameObject[] rings;
        public GameObject[] starmapNebulas;
        public GameObject[] systemNebulas;
        public Material provinceLineTexture;

        // galaxy label status
        private eGalaxyNameFilter galaxyNameFilter = eGalaxyNameFilter.OWNERSHIP; // defaults to ownership

        private Image empireStatusBar;
        private Image button;

        private Text starDataTextLine;
        private Text secondaryDataTextLine;
        private Text secondaryZoomValueLine;
        private Text gameDate;
        private Text empireTreasuryRevenues;
        private Text empireTreasuryExpenses;
        private Text emperorPoSup;
        private Text emperorAP;
        private Text emperorBenevolentInfluence;
        private Text emperorPragmaticInfluence;
        private Text emperorTyrannicalInfluence;
        private GalaxyData galaxyDataRef;
        private GalaxyCameraScript gCameraRef;
        private GameObject eventScrollView;
        //public bool StarSelected = false;
        private bool planetSelected = false;
        private float fov = 0.0f;
        private GUIStyle gStyle;
        private float zoomValue = 0.0f;
        private float maxZoomLevel = 0.0f;
        private GlobalGameData gameDataRef;
        private GameObject starmapSprite;
        private GameObject backingSphere;

        private Text galaxyMapModeInfo;
        private Text galaxyMapSubModeInfo;
        private Camera mainCamera;
        private Canvas galaxyUICanvas;
        private Canvas galaxyPlanetInfoCanvas;

        private GameObject[] planetsToDraw = new GameObject[6];
        [HideInInspector] public List<GameObject> listSystemObjectsCreated = new List<GameObject>();
        [HideInInspector] public List<GameObject> listSystemPlanetsCreated = new List<GameObject>();
        [HideInInspector] public List<GameObject> listGalaxyUIObjectsCreated = new List<GameObject>(); // list of all UI elements created (lines, range circles, etc)
        private List<StellarObjectDataBlock> listTextObjectsCreated = new List<StellarObjectDataBlock>(); // list of text labels (so can move dynamically onGUI)
        public bool planetsDrawn = false; // system planets are drawn?
        private bool systemNameDrawn = false; // system names are drawn?
        private bool systemGridCreated = false; // is system grid created?
        private bool provinceLinesDrawn = false; // are province lines drawn?
        private bool systemTrailCreated = false; // is background trail created?
        private bool rangeCirclesGenerated = false; // have range circles been drawn?
        private bool statsUpdated = false;
        private bool mapInOverviewMode = false; // is the map in province or galaxy mode?
        public bool EventPanelActive = true; // set visibilty
        private GameObject eventPanelButton;
        private GameObject smallSystemGrid;
        private GameObject sysTrail; // the local copy of the system trail
        private GameObject systemUICanvas;
        private GameObject selectedUnitInfoCanvas;
        private GameObject systemHeaderImage; // system / planet header
        private Text systemDisplaySystemName;
        private Text versionNumber;
        private Text systemDisplaySecondaryDataLine;
        private Text systemDisplayTertiaryDataLine;
        private Text systemIntelLevel;
        private GalaxyCameraScript.cameraZoomLevel zoomLevel = GalaxyCameraScript.cameraZoomLevel.Galaxy;

        void Awake()
        {

            // modal stuff
            modalPanel = ModalPanel.Instance();  // sets up a static instance of the window
            myYesAction = new UnityAction(TestYesFunction); // assign functions to actions
            myNoAction = new UnityAction(TestNoFunction);
            myCancelAction = new UnityAction(TestCancelFunction);

            starmapSprite = GameObject.Find("Y-Axis Grid");
            systemHeaderImage = GameObject.Find("System Header Image");
            starDataTextLine = GameObject.Find("StarDataTextInfo").GetComponent<Text>();
            secondaryDataTextLine = GameObject.Find("CameraZoomValue").GetComponent<Text>();
            secondaryZoomValueLine = GameObject.Find("CameraZoomInfo").GetComponent<Text>();
            gameDate = GameObject.Find("GameDate").GetComponent<Text>();
            backingSphere = GameObject.Find("Backing Sphere");
            empireTreasuryRevenues = GameObject.Find("EmpireTreasuryRevenues").GetComponent<Text>();
            empireTreasuryExpenses = GameObject.Find("EmpireTreasuryExpenses").GetComponent<Text>();
            emperorPoSup = GameObject.Find("Popular Support").GetComponent<Text>();
            emperorAP = GameObject.Find("Action Points").GetComponent<Text>();
            emperorBenevolentInfluence = GameObject.Find("Benevolent Influence").GetComponent<Text>();
            emperorPragmaticInfluence = GameObject.Find("Pragmatic Influence").GetComponent<Text>();
            emperorTyrannicalInfluence = GameObject.Find("Tyrannical Influence").GetComponent<Text>();
            versionNumber = GameObject.Find("Version Info").GetComponent<Text>();
            systemUICanvas = GameObject.Find("System UI Canvas");
            eventScrollView = GameObject.Find("Event ScrollView");
            selectedUnitInfoCanvas = GameObject.Find("Selected Unit Information Canvas");
            systemDisplaySystemName = GameObject.Find("System Name Text").GetComponent<Text>();
            systemDisplaySecondaryDataLine = GameObject.Find("Secondary Header Line").GetComponent<Text>();
            systemDisplayTertiaryDataLine = GameObject.Find("Tertiary Header Line").GetComponent<Text>();
            galaxyMapModeInfo = GameObject.Find("MapModeInfo").GetComponent<Text>();
            galaxyMapSubModeInfo = GameObject.Find("MapSubModeInfo").GetComponent<Text>();
            empireStatusBar = GameObject.Find("Empire Status Bar").GetComponent<Image>();
            button = GameObject.Find("War Button").GetComponent<Image>();
            galaxyUICanvas = GameObject.Find("Galaxy UI Canvas").GetComponent<Canvas>();
            galaxyPlanetInfoCanvas = GameObject.Find("Galaxy Planet Info Canvas").GetComponent<Canvas>();
            eventPanelButton = GameObject.Find("Event Panel Button");

            // data objects
            gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>(); // get global game data (date, location, version, etc)
            gCameraRef = GameObject.Find("Main Camera").GetComponent<GalaxyCameraScript>(); // get global camera script

            if (!starDataTextLine)
            {
                Debug.LogError("This script requires a text object!");
                enabled = false;
                return;
            }
        }

        public void VerifyExitModalWindow()
        {
            modalPanel.Choice("Wait. You're going to leave?\nAre you sure? We have so many miles to go...", myYesAction, myNoAction, myCancelAction); // calls the modal panel
        }

        void TestYesFunction()
        {
            Debug.Log("You clicked yes!");
            Application.Quit(); // Get Out
        }

        void TestNoFunction()
        {
            Debug.Log("You clicked no.");
        }

        void TestCancelFunction()
        {
            Debug.Log("You clicked cancel.");
        }

        void Start()
        {
            galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
            gStyle = new GUIStyle();
            maxZoomLevel = 3500f;
            backingSphere.SetActive(false);
            starmapSprite.transform.localScale = new Vector3((gameDataRef.GalaxySizeWidth / 6.5f), (gameDataRef.GalaxySizeWidth / 6.5f), 1); // scale the galaxy based on the size of the galaxy
            // set canvas as inactive depending on mode (will move to helper UI state script eventually)
            systemUICanvas.SetActive(false);
            selectedUnitInfoCanvas.SetActive(false);
        }

        void OnGUI()
        {
            
            GUI.depth = -2; // set behind all other objects?
            GUI.skin.font = labelFont; // set the font for all labels in this scene
            // display system data if there is a selected star and zoomed in

            //if (!statsUpdated)
            //{
                DisplayEmpirePrimaryStats(); // always for now
            //}
            DisplayVersionInfo();
            DisplayMapMode();

            if ((zoomLevel == GalaxyCameraScript.cameraZoomLevel.Galaxy || zoomLevel == GalaxyCameraScript.cameraZoomLevel.Province))
            {
                DrawGalaxyMapUI();
                // hide all other stars
                if (mainCamera.GetComponent<GalaxyCameraScript>().provinceZoomActive)
                {
                    HideStellarObjectsNotInSelectedProvince();
                    HideCivilizationRangeCircles();
                    DisplayProvinceData();
                }
                else
                {
                    ShowStellarObjects();
                    ShowCivilizationRangeCircles();
                    ShowProvinceLines();
                    ShowStellarDataBlocks();
                    selectedUnitInfoCanvas.SetActive(false);
                    
                }
            }
            else
            {
                HideGalaxyMapUI();
            }

            if ((zoomLevel == GalaxyCameraScript.cameraZoomLevel.System) && (GetSelectedStar() != null))
            {
                planetSelected = false;
                DisplaySystemData();
                HideStellarDataBlocks();
                HideProvinceLines();
                if (GetSelectedStar().GetComponent<LineRenderer>() != null)
                {
                    GetSelectedStar().GetComponent<LineRenderer>().enabled = false;
                }
            }
            else if ((zoomLevel == GalaxyCameraScript.cameraZoomLevel.Planet) && (GetSelectedPlanet() != null))
            {
                DisplayPlanetData();
            }
            else
            {            
                if (galaxyNameFilter == eGalaxyNameFilter.OWNERSHIP) // hide range circles if not in mode (should probably move to a separate function)
                    ShowCivilizationRangeCircles();
                else
                    HideCivilizationRangeCircles();

            }

            
  
            if (!provinceLinesDrawn)
            {
                DrawProvinceLines();
            }
        }

        void HideProvinceLines()
        {
            if (provinceLinesDrawn)
            {
                foreach (GameObject star in galaxyDataRef.GalaxyStarList)
                {
                    if (star.GetComponent<LineRenderer>() != null)
                    {
                        star.GetComponent<LineRenderer>().enabled = false;
                    }
                }
            }
        }

        public void ToggleEventPanelDisplay()
        {
            if (EventPanelActive)
            {
                EventPanelActive = false;
                return;
            }
            else
            {
                EventPanelActive = true;
                return;
            }
        }

        void ShowProvinceLines()
        {
            if (provinceLinesDrawn)
            {
                foreach (GameObject star in galaxyDataRef.GalaxyStarList)
                {
                    if (star.GetComponent<LineRenderer>() != null)
                    {
                        if (star.GetComponent<Star>().starData.Province != null)
                        {
                            Color lineColor = star.GetComponent<Star>().starData.Province.OwningCiv.Color;
                            lineColor = new Color(lineColor.r, lineColor.g, lineColor.b, GetAlphaProvinceFadeValue());
                            star.GetComponent<LineRenderer>().enabled = true;
                            star.GetComponent<LineRenderer>().SetColors(new Color(lineColor.r, lineColor.g, lineColor.b, .05f), lineColor);
                        }                      
                    }
                }
            }
        }

        void DrawProvinceLines()
        {
            foreach (Province pData in galaxyDataRef.ProvinceList)
            {
                if (pData.OwningCivID == gameDataRef.CivList[0].ID)
                {
                    Vector3 pHubLocation = new Vector3(0, 0, 0);
                    foreach (StarData sData in pData.SystemList)
                    {                      
                        if (sData.IsProvinceHub)
                        {
                            pHubLocation = new Vector3(sData.WorldLocation.x,sData.WorldLocation.y,-30);
                            break;
                        }                           
                    }

                    foreach (StarData sData in pData.SystemList)
                    {
                        if (!sData.IsProvinceHub)
                        {                        
                            GameObject.Find(sData.Name).AddComponent<LineRenderer>();
                            LineRenderer lr = GameObject.Find(sData.Name).GetComponent<LineRenderer>();
                            lr.SetPosition(0, new Vector3(sData.WorldLocation.x,sData.WorldLocation.y,-30));
                            lr.SetPosition(1, pHubLocation);
                            lr.material = provinceLineTexture;
                            GameObject.Find(sData.Name).GetComponent<LineRenderer>().SetWidth(5f, 5f);
                            lr.SetColors(gameDataRef.CivList[0].Color, gameDataRef.CivList[0].Color);
                        }
                    }
                }
            }
            provinceLinesDrawn = true;
        }

        void LateUpdate()
        {
            DisplayCameraZoomLevel();
            FadeOutStarMapOnZoom();

            if (systemNameDrawn && zoomLevel < GalaxyCameraScript.cameraZoomLevel.System)
            {
                if (!mainCamera.GetComponent<GalaxyCameraScript>().provinceZoomActive)
                {
                    ShowStellarDataBlocks();
                }
                UpdateStellarDataBlocks();
            }

            if (!rangeCirclesGenerated)
                GenerateCivilizationRangeCircles();
        }

        // Update is called once per frame
        void Update()
        {
            // get updated zoom levels
            zoomValue = mainCamera.GetComponent<GalaxyCameraScript>().zoom;
            zoomLevel = mainCamera.GetComponent<GalaxyCameraScript>().zoomLevel;

            // determine if in overview mode
            if (zoomLevel == GalaxyCameraScript.cameraZoomLevel.Province || zoomLevel == GalaxyCameraScript.cameraZoomLevel.Galaxy)
            {
                mapInOverviewMode = true;
            }
            else
            {
                mapInOverviewMode = false;
            }

            // draw civ range circles if debug mode changes
            if (gameDataRef.RequestGraphicRefresh)
            {
                foreach (GameObject uiObject in listGalaxyUIObjectsCreated)
                {
                    GameObject.Destroy(uiObject);
                }
                listGalaxyUIObjectsCreated.Clear(); // clear out any range circles, lines, system pips, etc
                GenerateCivilizationRangeCircles();
                DrawGalaxyMapUI(); // refresh UI
                statsUpdated = false; // reset empire data bar stats
                gameDataRef.RequestGraphicRefresh = false;
            }

            //test for a modal window
            if (Input.GetKey(KeyCode.Escape))
            {
                VerifyExitModalWindow(); // call the exit modal window
            }

            if (Input.GetKeyUp(KeyCode.M))  // cycle though the map modes
            {
                if (galaxyNameFilter == eGalaxyNameFilter.NONE)
                    galaxyNameFilter = eGalaxyNameFilter.BIO;
                else if (galaxyNameFilter == eGalaxyNameFilter.BIO)
                    galaxyNameFilter = eGalaxyNameFilter.UNREST;
                else if (galaxyNameFilter == eGalaxyNameFilter.UNREST)
                    galaxyNameFilter = eGalaxyNameFilter.OWNERSHIP;
                else
                    galaxyNameFilter = eGalaxyNameFilter.NONE;
            }

            
            fov = mainCamera.GetComponent<Camera>().fieldOfView;

            if (zoomLevel < GalaxyCameraScript.cameraZoomLevel.System && gameDataRef.uiSubMode == GlobalGameData.eSubMode.None && !gameDataRef.modalIsActive) // must be in no submode (astrographic)
                CheckForObjectSelection();
            if (zoomLevel == GalaxyCameraScript.cameraZoomLevel.System && gameDataRef.uiSubMode == GlobalGameData.eSubMode.None && !gameDataRef.modalIsActive)
            {
                GetSelectedStar().GetComponent<SpriteRenderer>().enabled = true; // show the stars
                GetSelectedStar().GetComponent<Light>().enabled = true; // show the lights
                ClearSelectedPlanet(); // reset all selections
                ShowAllPlanets();
                CheckForPlanetSelection();
            }

            foreach (GameObject star in galaxyDataRef.GalaxyStarList)
            {
                float scaleFactor = star.GetComponent<Star>().starData.Size / 50f;
                float screenFactor = Screen.width / 1920f;

                // normalize the scale
                if (scaleFactor < 1)
                    scaleFactor = 1;

                if (star.tag == "Selected") // zoomed star is bigger
                {
                    star.transform.localScale = new Vector2((30f  * scaleFactor) * screenFactor, ((30f * scaleFactor) * screenFactor));

                    // scale the binary/trinary children properly
                    foreach (Transform child in star.transform)
                    {
                        scaleFactor = child.GetComponent<Star>().starData.Size / 25;
                        // normalize the scale
                        if (scaleFactor < .75f)
                            scaleFactor = .75f;
                        child.localScale = new Vector2(.5f * scaleFactor, .5f * scaleFactor);
                        child.GetComponent<Light>().range = 1.05f * child.GetComponent<SpriteRenderer>().bounds.size.x;
                    }
                }
                else
                {
                    star.transform.localScale = new Vector2(6f * scaleFactor, 6f * scaleFactor);

                    // scale the binary/trinary children properly
                    foreach (Transform child in star.transform)
                    {
                        scaleFactor = child.GetComponent<Star>().starData.Size / 25;

                        // normalize the scale
                        if (scaleFactor < .75f)
                            scaleFactor = .75f;
                        child.localScale = new Vector2(.5f * scaleFactor, .5f * scaleFactor);
                        child.GetComponent<Light>().range = 1.5f * child.GetComponent<SpriteRenderer>().bounds.size.x;
                    }
                    star.transform.eulerAngles = new Vector3(0, 0, 0);
                    star.GetComponent<Light>().range = 1.2f * star.GetComponent<SpriteRenderer>().bounds.size.x;
                }
            }

            // display star data if there is a selected star, but no planet
            if (GetSelectedStar() != null && GetSelectedPlanet() == null)
            {
               
            }
            else
                starDataTextLine.enabled = false;

            // if camera zooms out too far, camera is considered in galaxy mode (use this in base camera script)
            if (zoomLevel != GalaxyCameraScript.cameraZoomLevel.System && gameDataRef.StarSelected == true && mainCamera.GetComponent<GalaxyCameraScript>().systemZoomActive == false && mainCamera.GetComponent<GalaxyCameraScript>().provinceZoomActive == false)
            {
                gameDataRef.StarSelected = false;
                RemoveSystemPlanets();
                GetSelectedStar().transform.position = GetSelectedStar().GetComponent<Star>().starData.WorldLocation; // set star back to its correct position
                ClearSelectedStar();
                ShowStellarObjects();
                ShowCivilizationRangeCircles();
                backingSphere.SetActive(false); // turn off the sphere
                GameObject.DestroyObject(smallSystemGrid);
                GameObject.DestroyObject(sysTrail);
                systemGridCreated = false;
                //provinceLinesDrawn = false;
                systemTrailCreated = false;
                systemUICanvas.SetActive(false); // don't show panel
                selectedUnitInfoCanvas.SetActive(false);
            }

            if (zoomLevel == GalaxyCameraScript.cameraZoomLevel.Province && !mainCamera.GetComponent<GalaxyCameraScript>().provinceZoomActive)
            {
                ShowProvinceLines();
            }
            else
            {
                //if (!mainCamera.GetComponent<GalaxyCameraScript>().provinceZoomActive)
                //    HideProvinceLines();
            }

            if (!systemNameDrawn)
            {
                GenerateStellarDataBlocks();
                GenerateProvinceNames();
            }
        }

        void DrawGalaxyMapUI()
        {
            if (gameDataRef.RequestGraphicRefresh)
            {
                eventScrollView.GetComponent<EventScrollView>().InitializeList(); // clear the list when made active
            }

            if (mapInOverviewMode)
            {
                eventPanelButton.SetActive(true);
                

                // turn off/on the event window but always turn off if there are no events to show
                if (gameDataRef.CivList[0].LastTurnEvents.Count > 0)
                    if (EventPanelActive)
                    {
                        eventScrollView.SetActive(true);
                    }
                    else
                    {
                        eventScrollView.SetActive(false);
                        EventPanelActive = false;
                    }
                else
                {
                    eventScrollView.SetActive(false);
                    EventPanelActive = false;
                }
                    
            }
            else
            {
                eventScrollView.GetComponent<EventScrollView>().InitializeList(); // clear the list when made active
                eventScrollView.SetActive(false);
                eventPanelButton.SetActive(false);
            }         
        }

        void HideGalaxyMapUI()
        {
            eventScrollView.SetActive(false);
            eventPanelButton.SetActive(false);
        }

        void CheckForObjectSelection()
        {
            if (Input.GetMouseButtonDown(0) && gameDataRef.StarSelected != true)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.GetRayIntersection(ray, 6000);

                if ((hit.collider != null))
                {
                    if (hit.transform.tag == "Star")
                    {
                        ClearSelectedStar(); // reset all selections
                        hit.transform.GetComponent<Star>().tag = "Selected"; // select the star

                        //invoke zoom sequence
                        Camera.main.GetComponent<GalaxyCameraScript>().starTarget = hit.transform;
                        Camera.main.GetComponent<GalaxyCameraScript>().systemZoomActive = true;
                        Camera.main.GetComponent<GalaxyCameraScript>().planetZoomActive = false;
                        Camera.main.GetComponent<GalaxyCameraScript>().provinceZoomActive = false;
                        gameDataRef.StarSelected = true; // probably need to move to a global UI manager
                    }
                    //else if (hit.transform.tag == "Province")
                    //{
                    //     create call to hide all stars that are not part of the province - add later
                    //    Camera.main.GetComponent<GalaxyCameraScript>().provinceTarget = HelperFunctions.DataRetrivalFunctions.GetProvince(hit.transform.name);
                    //    Camera.main.GetComponent<GalaxyCameraScript>().systemZoomActive = false;
                    //    Camera.main.GetComponent<GalaxyCameraScript>().planetZoomActive = false;
                    //    Camera.main.GetComponent<GalaxyCameraScript>().provinceZoomActive = true;
                       
                    //}
                }
            }
        }

        void GenerateProvinceNames()
        {
            foreach (Province pData in galaxyDataRef.ProvinceList)
            {
                Vector3 textLocation = new Vector3(pData.ProvinceCenter.x, pData.ProvinceCenter.y, 0); // where the text box is located
                GameObject provinceName = Instantiate(starNameObject, pData.ProvinceCenter, Quaternion.identity) as GameObject;
                StellarObjectDataBlock starDataBlock = new StellarObjectDataBlock();  // create a new star data block
                provinceName.transform.SetParent(galaxyPlanetInfoCanvas.transform.Find("Galaxy Data Panel"), true); // attach the blocks to the panel


                provinceName.transform.localPosition = textLocation; //new Vector3(textLocation.x - (Screen.width / 2), textLocation.y - (Screen.height / 2), 0); // reset after making a parent to canvas relative coordinates (pivot in center)
                provinceName.transform.localScale = new Vector3(1, 1, 1); // do not scale
                provinceName.tag = "Province"; // tag for selection
                provinceName.name = pData.ID;

                starDataBlock.objectRotation = 0f;
                starDataBlock.ownerName = pData.Name.ToUpper() + " PROVINCE";
                starDataBlock.provinceBounds = pData.ProvinceBounds; // to set the size of the label
                starDataBlock.provinceObjectLocation = textLocation; // provinceName.transform.localPosition;
                starDataBlock.ownerColor = HelperFunctions.DataRetrivalFunctions.FindProvinceOwnerColor(pData);
                starDataBlock.blockType = StellarObjectDataBlock.eBlockType.Province;
                starDataBlock.textObject = provinceName;
                listTextObjectsCreated.Add(starDataBlock);

            }
        }

        void CheckForPlanetSelection()
        {
            if (Input.GetMouseButtonDown(0) && planetSelected != true)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.GetRayIntersection(ray, maxZoomLevel + 20);

                if (hit.collider != null && hit.transform.tag == "Planet")
                {
                    hit.transform.GetComponent<Planet>().tag = "Selected"; // select the star

                    //invoke zoom sequence
                    Camera.main.GetComponent<GalaxyCameraScript>().planetTarget = hit.transform;
                    Camera.main.GetComponent<GalaxyCameraScript>().planetZoomActive = true;
                    Camera.main.GetComponent<GalaxyCameraScript>().provinceZoomActive = false;
                    GetSelectedStar().GetComponent<SpriteRenderer>().enabled = false; // hide the stars
                    GetSelectedStar().GetComponent<Light>().enabled = false; // hide the light halo
                    planetSelected = true;
                }
            }
        }

        void RemoveSystemPlanets()
        {
            planetsDrawn = false;
            foreach (GameObject p in listSystemPlanetsCreated)
            {
                Destroy(p);
            }
            listSystemPlanetsCreated.Clear(); // clear the planets
            Array.Clear(planetsToDraw, 0, planetsToDraw.Length);
        }

        void DisplayVersionInfo()
        {
            versionNumber.text = "build " + gameDataRef.gameVersion;
        }

        void DisplayStarDataLine()
        {
            StarData hitData = null;
            string compData = "Single";

            hitData = GetSelectedStar().GetComponent<Star>().starData;
            compData = hitData.starMultipleType.ToString();
            starDataTextLine.text = "Size: " + hitData.Size.ToString("N0") + "(" + hitData.timesClicked.ToString("N0") + ")" + "   Coordinates: " + hitData.WorldLocation.ToString("N0")
                    + "   Stellar Type: " + hitData.SpectralClass.ToString() + hitData.SecondarySpectralClass.ToString("N0") + "   Stellar Age: " + hitData.Age.ToString("N0") +
                    "   Metallicity: " + hitData.Metallicity.ToString("N0") + "   Stellar Material: " + hitData.pMaterial.ToString("N1") + "   Companion Status: " + compData;
        }

        void FadeOutStarMapOnZoom()
        {
            float fadeIntensity = 7.2f;

            if (fov < 100)
            {
                float alphaValue = ((255 - (100 - fov) * fadeIntensity) / 255) * 1f;
                if (alphaValue > .90f) // normalize alpha
                    alphaValue = .90f;
                if (alphaValue < 0)
                    alphaValue = 0;

                starmapSprite.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alphaValue);
            }
        }

        void DisplayMapMode() // will need to flesh out
        {
            // only show info if not zoomed into system level
            if (zoomLevel < GalaxyCameraScript.cameraZoomLevel.System && !gCameraRef.provinceZoomActive)
            {
                galaxyMapModeInfo.enabled = true;
                galaxyMapSubModeInfo.enabled = true;
                galaxyMapModeInfo.text = "ASTROGRAPHIC MODE";
            }
            else
            {
                galaxyMapModeInfo.enabled = false;
                galaxyMapSubModeInfo.enabled = false;
            }

            switch (galaxyNameFilter)
            {
                case eGalaxyNameFilter.BIO:
                    galaxyMapSubModeInfo.text = "BIO SUBMODE";
                    break;
                case eGalaxyNameFilter.NONE:
                    galaxyMapSubModeInfo.text = "NO SUBMODE";
                    break;
                case eGalaxyNameFilter.OWNERSHIP:
                    galaxyMapSubModeInfo.text = "SOVEREIGNITY SUBMODE";
                    break;
                case eGalaxyNameFilter.UNREST:
                    galaxyMapSubModeInfo.text = "UNREST SUBMODE";
                    break;
                default:
                    galaxyMapSubModeInfo.text = "NO SUBMODE";
                    break;
            }
        }

        void HideCivilizationRangeCircles()
        {
            if (listGalaxyUIObjectsCreated.Count > 0)
            {
                foreach (GameObject rCircle in listGalaxyUIObjectsCreated)
                {
                    rCircle.SetActive(false);
                }
            }
        }

        void ShowCivilizationRangeCircles()
        {
            if (listGalaxyUIObjectsCreated.Count > 0)
            {
                foreach (GameObject rCircle in listGalaxyUIObjectsCreated)
                {
                    rCircle.SetActive(true);
                    if (zoomLevel == GalaxyCameraScript.cameraZoomLevel.Province)
                    {
                        Color circColor = rCircle.GetComponent<MeshRenderer>().material.color;
                        rCircle.GetComponent<MeshRenderer>().material.color = new Color(circColor.r, circColor.g, circColor.b, .04f - (GetAlphaProvinceFadeValue() / 3f));
                    }
                }
            }
        }

        void GenerateCivilizationRangeCircles()
        {
            
            foreach (Civilization civ in gameDataRef.CivList)
            {
                if (HelperFunctions.DataRetrivalFunctions.GetCivHomeSystem(civ).IntelValue > 6 || gameDataRef.DebugMode)
                {
                    Vector3 civCapitalLocation = HelperFunctions.DataRetrivalFunctions.GetCivHomeSystem(civ).WorldLocation; // pull the star location of the capital system
                    float civRange = 0;

                    // determine scale of circle
                    civRange = civ.Range;
                    civRange *= 2; // double the range since scaling is for a whole side

                    GameObject rangeCircle = Instantiate(Resources.Load("Galaxy View Assets/Empire Range Circle",typeof(GameObject)), civCapitalLocation, Quaternion.Euler(new Vector3(90,0,0))) as GameObject;
                    rangeCircle.transform.localScale = new Vector3(civRange, 1, civRange); // expand the circle
                    listGalaxyUIObjectsCreated.Add(rangeCircle);
                    rangeCircle.GetComponent<MeshRenderer>().material.color = new Color(civ.Color.r, civ.Color.g, civ.Color.b, .06f); // set range circle to show civ color
                }
            }

            rangeCirclesGenerated = true;
        }

        void DisplayCameraZoomLevel()
        {
            // if camera zooms out too far, camera is considered in galaxy mode (use this in base camera script)
            string debugMode = "";
            if (gameDataRef.DebugMode)
                debugMode = "(DEBUG)";
            secondaryDataTextLine.text = "ZOOM: " + fov.ToString("N1");
            secondaryZoomValueLine.text = mainCamera.GetComponent<GalaxyCameraScript>().zoomLevel.ToString().ToUpper() + " " + debugMode;
        }

        void DisplayEmpirePrimaryStats() // this will need to maybe move to a separate view
        {
            gameDate.text = gameDataRef.GameDate.ToString("F1"); // display date
            empireTreasuryRevenues.text = HelperFunctions.StringConversions.ConvertFloatDollarToText(gameDataRef.CivList[0].Revenues);//.ToString("N2") + " M";
            empireTreasuryExpenses.text = HelperFunctions.StringConversions.ConvertFloatDollarToText(gameDataRef.CivList[0].Expenses); //.ToString("N2") + " M";
            emperorAP.text = HelperFunctions.DataRetrivalFunctions.GetCivLeader("CIV0").ActionPoints.ToString("N0"); // get human leader
            emperorPoSup.text = (HelperFunctions.DataRetrivalFunctions.GetCivLeader("CIV0").EmperorPoSup * 100).ToString("N1") + "%";
            emperorBenevolentInfluence.text = HelperFunctions.DataRetrivalFunctions.GetCivLeader("CIV0").BenevolentInfluence.ToString("N1");
            emperorPragmaticInfluence.text = HelperFunctions.DataRetrivalFunctions.GetCivLeader("CIV0").PragmaticInfluence.ToString("N1");
            emperorTyrannicalInfluence.text = HelperFunctions.DataRetrivalFunctions.GetCivLeader("CIV0").TyrannicalInfluence.ToString("N1");
            statsUpdated = true;
        }

        void HideStellarDataBlocks()
        {
            foreach (StellarObjectDataBlock star in listTextObjectsCreated)
            {
                star.textObject.SetActive(false);
            }
        }

        void ShowStellarDataBlocks()
        {
            foreach (StellarObjectDataBlock star in listTextObjectsCreated)
            {
                star.textObject.SetActive(true);
            }
        }


        void ClearSelectedStar()
        {
            //reset the selected star by changing all tags to 'Star'
            foreach (GameObject star in galaxyDataRef.GalaxyStarList)
            {
                if (star.tag != "Companion Star")
                    star.tag = "Star";
            }
        }

        public GameObject GetSelectedStar()
        {
            foreach (GameObject star in galaxyDataRef.GalaxyStarList)
            {
                if (star.tag == "Selected")
                {
                    return star; // return the star
                }
            }
            return null; // no star, error
        }

        void UpdateStellarDataBlocks()
        {
            Vector3 textLocation;

            foreach (StellarObjectDataBlock text in listTextObjectsCreated)
            {

                if (text.textObject.activeSelf)
                {
                    Color textColor = new Color();

                    if (galaxyNameFilter == eGalaxyNameFilter.BIO) // check for bio status
                    {
                        if (text.blockType == StellarObjectDataBlock.eBlockType.Star)
                        {
                            StarData starData = text.stellarObject.GetComponent<Star>().starData;
                            textColor = CheckBioStatus(starData);
                            text.textObject.GetComponent<Text>().text = starData.Name.ToUpper();
                        }
                        else
                        {
                            text.textObject.SetActive(false);
                        }
                        textColor = Color.white;
                    }
                    else if (galaxyNameFilter == eGalaxyNameFilter.NONE)
                    {
                        if (text.blockType == StellarObjectDataBlock.eBlockType.Star)
                        {
                            StarData starData = text.stellarObject.GetComponent<Star>().starData;

                            text.textObject.GetComponent<Text>().text = starData.Name.ToUpper(); //+ civNames;
                        }
                        textColor = Color.gray;
                    }
                    else if (galaxyNameFilter == eGalaxyNameFilter.OWNERSHIP)
                    {
                        if (text.blockType == StellarObjectDataBlock.eBlockType.Star)
                        {
                            StarData starData = text.stellarObject.GetComponent<Star>().starData;
                            if (!gameDataRef.DebugMode)
                            {
                                text.textObject.GetComponent<Text>().text = starData.Name.ToUpper();
                                if (starData.IntelValue > Constants.Constants.MediumIntelLevelMax)
                                    textColor = text.ownerColor;
                                else
                                    textColor = Color.grey;
                            }
                            else
                            {
                                text.textObject.GetComponent<Text>().text = starData.Name.ToUpper() + "(" + starData.WorldLocation.x.ToString("N0") + "," + starData.WorldLocation.y.ToString("N0") + ")" + text.ownerName + text.ownerTolerance;// stellarObject.civNames;
                                textColor = text.ownerColor;
                            }
                        }

                        // show province-level data, fade in and out
                        else if (text.blockType == StellarObjectDataBlock.eBlockType.Lower)
                        {
                            float alphaLevel = 0.0f;
                            StarData starData = text.stellarObject.GetComponent<Star>().starData;
                            Color scanColor = new Color();

                            if (zoomLevel == GalaxyCameraScript.cameraZoomLevel.Galaxy)
                            {
                                if (starData.IntelLevel == eStellarIntelLevel.None)
                                    scanColor = Color.grey;
                                // get color of zoom level
                                else if (starData.IntelLevel == eStellarIntelLevel.Low)
                                    scanColor = Color.red;
                                else if (starData.IntelLevel == eStellarIntelLevel.Medium)
                                    scanColor = Color.yellow;
                                else
                                    scanColor = Color.green;
                                alphaLevel = GetAlphaGalaxyFadeValue();
                                text.textObject.SetActive(true);
                                text.textObject.GetComponent<Text>().fontSize = 10;
                                text.textObject.GetComponent<Text>().text = starData.IntelLevel.ToString().ToUpper();

                                textColor = new Color(scanColor.r, scanColor.g, scanColor.b, alphaLevel);
                            }
                            if (zoomLevel == GalaxyCameraScript.cameraZoomLevel.Province)
                            {


                                alphaLevel = GetAlphaProvinceFadeValue();
                                text.textObject.SetActive(true);
                                text.textObject.GetComponent<Text>().fontSize = 10;
                                if (starData.AssignedProvinceID != "")
                                {
                                    if (gameDataRef.DebugMode || starData.IntelValue == 10)
                                    {
                                        text.textObject.GetComponent<Text>().text = HelperFunctions.DataRetrivalFunctions.GetProvince(starData.AssignedProvinceID).Name.ToUpper() + " PROVINCE";
                                        if (starData.IsProvinceHub)
                                            text.textObject.GetComponent<Text>().text += " CAPITAL";
                                        textColor = new Color(text.ownerColor.r, text.ownerColor.g, text.ownerColor.b, alphaLevel);
                                    }
                                    else
                                    {
                                        text.textObject.GetComponent<Text>().text = "UNKNOWN";
                                        textColor = new Color(Color.grey.r, Color.grey.g, Color.grey.b, alphaLevel);
                                    }
                                }
                                else
                                {
                                    if (gameDataRef.DebugMode || starData.IntelValue == 10)
                                        text.textObject.GetComponent<Text>().text = "NONE";
                                    else
                                        text.textObject.GetComponent<Text>().text = "UNKNOWN";
                                    textColor = new Color(Color.grey.r, Color.grey.g, Color.grey.b, alphaLevel);
                                }
                            }
                            else
                            {
                                //text.textObject.SetActive(false);
                            }
                        }
                        else if (text.blockType == StellarObjectDataBlock.eBlockType.Province)
                        {
                            float alphaLevel = 0.0f;
                            if (zoomLevel == GalaxyCameraScript.cameraZoomLevel.Province)
                            {
                                alphaLevel = GetAlphaProvinceFadeValue();
                                text.textObject.SetActive(true);

                                // set the size of the province label
                                if (text.provinceBounds.width + text.provinceBounds.height > 5000)
                                {
                                    text.textObject.GetComponent<Text>().fontSize = 100;
                                }
                                else if (text.provinceBounds.width + text.provinceBounds.height <= 1000)
                                {
                                    text.textObject.GetComponent<Text>().fontSize = 20;
                                }
                                else
                                    text.textObject.GetComponent<Text>().fontSize = (int)((text.provinceBounds.width + text.provinceBounds.height) / 50);

                                // set size of bounds for province object
                                text.textObject.GetComponent<RectTransform>().sizeDelta = new Vector2(text.textObject.GetComponent<Text>().fontSize * text.ownerName.Length, text.textObject.GetComponent<Text>().fontSize);

                                if (gameDataRef.DebugMode)
                                {
                                    text.textObject.GetComponent<Text>().text = text.ownerName.ToUpper();
                                    textColor = new Color(text.ownerColor.r, text.ownerColor.g, text.ownerColor.b, alphaLevel);
                                }
                                else
                                {
                                    text.textObject.GetComponent<Text>().text = text.ownerName.ToUpper();
                                    textColor = new Color(text.ownerColor.r, text.ownerColor.g, text.ownerColor.b, alphaLevel);
                                }

                            }
                        }
                    }
                    else
                        textColor = Color.white;

                    // adjust text size of nebulas when zoomed out a lot
                    if (text.blockType == StellarObjectDataBlock.eBlockType.Nebula)
                    {
                        if (fov > 80)
                            text.textObject.GetComponent<Text>().fontSize = 15;
                        else if (fov > 60)
                            text.textObject.GetComponent<Text>().fontSize = 20;
                        else
                            text.textObject.GetComponent<Text>().fontSize = 30;
                    }

                    text.textObject.GetComponent<Text>().color = textColor;
                    Vector3 nameVector;
                    if (text.blockType != StellarObjectDataBlock.eBlockType.Province)
                    {
                        nameVector = Camera.main.WorldToScreenPoint(text.stellarObject.transform.position); // gets the screen point of the star's transform position
                    }
                    else
                    {
                        nameVector = Camera.main.WorldToScreenPoint(text.provinceObjectLocation); // gets the screen point of the star's transform position
                    }
                    if (text.blockType != StellarObjectDataBlock.eBlockType.Lower)
                        textLocation = new Vector3(nameVector.x, nameVector.y + 20 + ((120 - zoomValue) / 4.3f), 0); // where the text box is located
                    else if (text.blockType != StellarObjectDataBlock.eBlockType.Province)
                    {
                        textLocation = nameVector;
                    }
                    else
                        textLocation = new Vector3(nameVector.x, nameVector.y - 5 - ((120 - zoomValue) / 4.4f), 0); // where the text box is located
                    text.textObject.transform.localPosition = new Vector3(textLocation.x - (Screen.width / 2), textLocation.y - (Screen.height / 2), 0); // reset after making a parent to canvas relative coordinates (pivot in center)

                }
            }
        }

        private float GetAlphaProvinceFadeValue()
        {
            float alphaLevel = 0.0f;

            alphaLevel = 290 - (fov * 2.7f);
            alphaLevel = alphaLevel / 255;

            if (alphaLevel < 0)
                alphaLevel = 0;

            if (alphaLevel > .8)
                alphaLevel = 1;

            if (mainCamera.GetComponent<GalaxyCameraScript>().provinceZoomActive)
                alphaLevel = 1;

            return alphaLevel;
        }

        private float GetAlphaGalaxyFadeValue()
        {
            float alphaLevel = 255f;

            alphaLevel = -800 + (fov * 7f); // was 8f
            alphaLevel = alphaLevel / 255;
            if (alphaLevel < 0)
                alphaLevel = 0;
            if (alphaLevel > .9)
                alphaLevel = 1;

            return alphaLevel;
        }

        private Color CheckBioStatus(StarData starData)
        {
            bool hasGoodPlanets = false;
            bool hasOKPlanets = false;
            bool hasPoorPlanets = false;
            Color textColor = new Color();

            foreach (PlanetData planet in starData.PlanetList)  // test to check for changing label colors dynamically
            {
                if (starData.PlanetList.Count > 0)
                {
                    if (planet.Bio > 70)
                        hasGoodPlanets = true;
                    else if (planet.Bio > 40)
                        hasOKPlanets = true;
                    else
                        hasPoorPlanets = true;
                }
                else
                {
                    textColor = Color.grey;
                    return textColor;
                }
            }

            // set the color depending on planet bio
            if (hasGoodPlanets)
                textColor = Color.green;
            else if (hasOKPlanets)
                textColor = Color.yellow;
            else if (hasPoorPlanets)
                textColor = Color.red;
           
            return textColor;
        }

        void ClearSelectedPlanet()
        {
            //reset the selected star by changing all tags to 'Star'
            foreach (GameObject planet in listSystemPlanetsCreated)
            {
                planet.tag = "Planet";
            }
        }

        public GameObject GetSelectedPlanet() // find the selected planet in the system (will have to change eventually!)
        {
            foreach (GameObject planet in listSystemPlanetsCreated)
            {
                if (planet.tag == "Selected")
                {
                    return planet; // return the planet
                }
            }
            return null; // no star, error
        }

        void GenerateStellarDataBlocks()
        {
            Vector3 textLocation;
            Vector3 textLocation2;
           
            foreach (GameObject star in galaxyDataRef.GalaxyStarList)
            {
                if (star != null)
                {
                    if (star.GetComponent<Star>().tag != "Companion Star") // if not a companion star
                    {
                        string civNames = "";
                        string civTolerance = "";
                        bool civOwnerFound = false;
                        Color civColor = Color.grey;

                        var nameVector = Camera.main.WorldToScreenPoint(star.transform.position); // gets the screen point of the star's transform position
                        textLocation = new Vector3(nameVector.x, nameVector.y, 0); // where the text box is located
                        textLocation2 = new Vector3(nameVector.x, nameVector.y, 0); // where the lower text box is located

                        // create the text objects
                        GameObject starName = Instantiate(starNameObject, textLocation, Quaternion.identity) as GameObject;                      
                        StellarObjectDataBlock starDataBlock = new StellarObjectDataBlock();  // create a new star data block
                        GameObject provinceName = Instantiate(starNameObject, textLocation2, Quaternion.identity) as GameObject;
                        StellarObjectDataBlock lowerProvinceDataBlock = new StellarObjectDataBlock(); // create the lower data block for province info

                        starName.transform.SetParent(galaxyPlanetInfoCanvas.transform.Find("Galaxy Data Panel"), true); // attach the blocks to the panel
                        provinceName.transform.SetParent(galaxyPlanetInfoCanvas.transform.Find("Galaxy Data Panel"), true);  // attach the blocks to the panel

                        // add the owning civs if present                      
                        foreach (Civilization civ in gameDataRef.CivList)
                        {
                            List<StarData> civSystems = new List<StarData>();
                            civSystems = HelperFunctions.DataRetrivalFunctions.GetCivSystemList(civ);

                            if (!civOwnerFound)
                            {
                                foreach (StarData sys in civSystems)
                                {
                                    if (sys.ID == star.GetComponent<Star>().starData.ID)
                                    {
                                        bool systemIsCapital = false;
                                        foreach (PlanetData pData in sys.PlanetList)
                                        {
                                            if (pData.ID == civ.CapitalPlanetID)
                                            {
                                                systemIsCapital = true;
                                                break;
                                            }
                                        }
                                        if (systemIsCapital)
                                            civNames += " (" + civ.Name.ToUpper() + " HOME SYSTEM," + civ.Size.ToString() + ")";
                                        else
                                            civNames += " (" + civ.Name.ToUpper() + "," + civ.Size.ToString() + ")";

                                        civNames += " (" + civ.Type.ToString() + ")";
                                        if (sys.AssignedProvinceID != "")
                                            civNames += "(PROV NAME: " + HelperFunctions.DataRetrivalFunctions.GetProvince(sys.AssignedProvinceID).Name.ToUpper() + ")";
                                        civColor = civ.Color;
                                        civTolerance = "(TOL: " + civ.PlanetMinTolerance.ToString("N0") + ")";
                                        civOwnerFound = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (!gameDataRef.DebugMode)
                            starName.GetComponent<Text>().text = star.GetComponent<Star>().starData.Name.ToUpper() + "[" + star.GetComponent<Star>().starData.IntelValue.ToString("N0") + "]"; //+ civNames;
                        else
                            starName.GetComponent<Text>().text = star.GetComponent<Star>().starData.Name.ToUpper() + civNames;
                        starName.GetComponent<Text>().color = civColor;
                        starName.transform.localPosition = new Vector3(textLocation.x - (Screen.width / 2),textLocation.y - (Screen.height / 2),0); // reset after making a parent to canvas relative coordinates (pivot in center)
                        starName.transform.localScale = new Vector3(1, 1, 1); // do not scale
                        starName.name = star.GetComponent<Star>().starData.ID;

                        provinceName.transform.localPosition = new Vector3(textLocation.x - (Screen.width / 2), textLocation.y - (Screen.height / 2), 0); // reset after making a parent to canvas relative coordinates (pivot in center)
                        provinceName.transform.localScale = new Vector3(1, 1, 1); // do not scale
                        provinceName.name = star.GetComponent<Star>().starData.ID;

                        // assign to the star data block
                        starDataBlock.objectRotation = 0f;
                        starDataBlock.ownerName = civNames;
                        starDataBlock.blockType = StellarObjectDataBlock.eBlockType.Star;
                        starDataBlock.stellarObject = star;
                        starDataBlock.ownerColor = civColor;
                        
                        starDataBlock.ownerTolerance = civTolerance;
                        starDataBlock.textObject = starName;
                        listTextObjectsCreated.Add(starDataBlock);

                        // assign to the secondary data block
                        lowerProvinceDataBlock.objectRotation = 0f;
                        lowerProvinceDataBlock.ownerName = civNames;
                        lowerProvinceDataBlock.blockType = StellarObjectDataBlock.eBlockType.Lower;
                        lowerProvinceDataBlock.stellarObject = star;
                        lowerProvinceDataBlock.ownerColor = civColor;
                        lowerProvinceDataBlock.textObject = provinceName;
                        listTextObjectsCreated.Add(lowerProvinceDataBlock);

                    }
                }
            }

            foreach (GameObject nebula in galaxyDataRef.stellarPhenonomaList)
            {
                if (nebula != null)
                {
                        var nameVector = Camera.main.WorldToScreenPoint(nebula.transform.position); // gets the screen point of the star's transform position
                        Vector2 vectorTwo = GUIUtility.ScreenToGUIPoint(new Vector2(nameVector.x, nameVector.y)); // gets center of star on screen position
                        textLocation = new Vector3(nameVector.x, nameVector.y, 0); // where the text box is located

                        // create the text object
                        GameObject stellarObjectName = Instantiate(starNameObject, textLocation, Quaternion.identity) as GameObject;
                        StellarObjectDataBlock stellarObjectDataBlock = new StellarObjectDataBlock();  // create a new star data block
                        
                        stellarObjectName.transform.SetParent(galaxyPlanetInfoCanvas.transform, true);
                        stellarObjectName.GetComponent<Text>().text = nebula.GetComponent<Nebula>().nebulaData.Name.ToUpper();
                        stellarObjectName.transform.localPosition = new Vector3(textLocation.x - (Screen.width / 2), textLocation.y - (Screen.height / 2), 1); // reset after making a parent to canvas relative coordinates (pivot in center)
                        stellarObjectName.transform.localScale = new Vector3(1, 1, 1); // do not scale
                        stellarObjectName.GetComponent<LetterSpacing>().spacing = 40; // space out the letters
                        stellarObjectName.GetComponent<Text>().fontSize = 20;

                        // assign to the star data block
                        stellarObjectDataBlock.objectRotation = nebula.GetComponent<Nebula>().nebulaData.TextRotation;
                        stellarObjectDataBlock.blockType = StellarObjectDataBlock.eBlockType.Nebula;
                        stellarObjectDataBlock.stellarObject = nebula;
                        stellarObjectDataBlock.textObject = stellarObjectName;
                        listTextObjectsCreated.Add(stellarObjectDataBlock);

                        stellarObjectName.transform.Rotate(new Vector3(0, 0, stellarObjectDataBlock.objectRotation));
                }
            }
            systemNameDrawn = true;        
        }

        void HideStellarObjectsNotInSelectedProvince()
        {
            Province selectedProvince = null;

            if (Camera.main.GetComponent<GalaxyCameraScript>().provinceTarget != null)
                selectedProvince = Camera.main.GetComponent<GalaxyCameraScript>().provinceTarget;
            else
                return;

            foreach (GameObject star in galaxyDataRef.GalaxyStarList)
            {
                if (star != null)
                {
                    if (star.tag == "Star")
                    {
                        if (!selectedProvince.SystemList.Exists(p => p.ID == star.GetComponent<Star>().starData.ID))
                        {
                            star.GetComponent<SpriteRenderer>().enabled = false;
                            star.GetComponent<Light>().enabled = false;
                            if (star.GetComponent<LineRenderer>() != null)
                            {
                                star.GetComponent<LineRenderer>().enabled = false;
                            }


                            foreach (Transform child in star.transform)
                            {
                                child.GetComponent<SpriteRenderer>().enabled = false;
                                child.GetComponent<Light>().enabled = false;
                            }
                        }
                    }
                }
            }

            // hide data blocks
            foreach (StellarObjectDataBlock star in listTextObjectsCreated)
            {
                if (!selectedProvince.SystemList.Exists(p => p.ID == star.textObject.name))
                    star.textObject.SetActive(false);
            }
        }

        void HideStellarObjects()
        {
            foreach (GameObject star in galaxyDataRef.GalaxyStarList)
            {
                if (star != null)
                {
                    if (star.tag == "Star")
                    {
                        star.GetComponent<SpriteRenderer>().enabled = false;
                        star.GetComponent<Light>().enabled = false;
                        

                        foreach (Transform child in star.transform)
                        {
                            child.GetComponent<SpriteRenderer>().enabled = false;
                            child.GetComponent<Light>().enabled = false;
                        }
                    }
                }
            }

            foreach (GameObject nebula in galaxyDataRef.stellarPhenonomaList)
            {
                if (nebula != null)
                {
                    if (nebula.tag == "StellarObject")
                    {
                        nebula.GetComponent<SpriteRenderer>().enabled = false;
                        
                    }
                }
            }
        }

        void HideUnselectedPlanets()
        {
            foreach (GameObject planet in listSystemPlanetsCreated)
            {
                if (planet != null)
                {
                    if (planet.tag == "Planet")
                    {
                        planet.GetComponent<SpriteRenderer>().enabled = false;
                        foreach (Transform child in planet.transform)
                            child.GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }
        }

        void ShowStellarObjects()
        {
            foreach (GameObject star in galaxyDataRef.GalaxyStarList)
            {
                if (star != null)
                {
                    if (star.tag == "Star")
                    {
                        star.GetComponent<SpriteRenderer>().enabled = true;
                        star.GetComponent<Light>().enabled = true;
                        if (provinceLinesDrawn)
                        {
                            if (star.GetComponent<LineRenderer>() != null)
                            {
                                star.GetComponent<LineRenderer>().enabled = true;
                            }
                        }
                        foreach (Transform child in star.transform)
                        {
                            child.GetComponent<SpriteRenderer>().enabled = true;
                            child.GetComponent<Light>().enabled = true;
                        }
                    }
                }
            }

            foreach (GameObject nebula in galaxyDataRef.stellarPhenonomaList)
            {
                if (nebula != null)
                {
                    if (nebula.tag == "StellarObject")
                    {
                        nebula.GetComponent<SpriteRenderer>().enabled = true;                       
                    }
                }
            }
        }

        void ShowAllPlanets()
        {
            foreach (GameObject planet in listSystemPlanetsCreated)
            {
                if (planet != null)
                {
                    if (planet.tag == "Planet")
                    {
                        planet.GetComponent<SpriteRenderer>().enabled = true;
                        foreach (Transform child in planet.transform)
                            child.GetComponent<SpriteRenderer>().enabled = true;
                    }
                }
            }
        }

        void DisplayProvinceData()
        {
            HideStellarObjectsNotInSelectedProvince();
            selectedUnitInfoCanvas.SetActive(true); // show information panel
            systemDisplaySystemName.text = GetSelectedProvince().Name.ToUpper() + " PROVINCE";
            systemDisplaySecondaryDataLine.text = GetSelectedProvince().OwningCiv.Name.ToUpper() + " SOVEREIGNITY";
            systemDisplayTertiaryDataLine.text = GetSelectedProvince().PlanetList.Count.ToString("N0") + " TOTAL PLANETS";
        }

        private Province GetSelectedProvince()
        {
            if (gCameraRef.provinceTarget != null)
            {
                return gCameraRef.provinceTarget;
            }
            else
                return null;
        }

        void DisplayPlanetData() // This and the display system data functions will need to move to their respective view classes for continuity purposes.
        {
            PlanetData planetDat = GetSelectedPlanet().GetComponent<Planet>().planetData; // reference
            StarData starDat = GetSelectedStar().GetComponent<Star>().starData; // reference
            string owner = "NO";
            HideUnselectedPlanets();
            smallSystemGrid.SetActive(false); // hides the grid
            sysTrail.SetActive(false); // hides the trail

            // will need to move to a new class eventually to make it cleaner
            systemDisplaySystemName.text = planetDat.Name.ToUpper();  //show text
            systemDisplayTertiaryDataLine.color = Color.green;
            foreach (Civilization civ in gameDataRef.CivList)
            {
                 if (civ.PlanetIDList.Exists(p => p == planetDat.ID))
                 {
                     owner = civ.Name.ToUpper();
                     systemDisplayTertiaryDataLine.color = civ.Color;                    
                 }
            }

            systemDisplaySecondaryDataLine.text = "CLASS " + HelperFunctions.StringConversions.ConvertToRomanNumeral((int)(planetDat.Size / 10)) + " " + HelperFunctions.StringConversions.ConvertPlanetEnum(planetDat.Type).ToUpper();
            systemDisplaySecondaryDataLine.text += " | " + HelperFunctions.DataRetrivalFunctions.GetSystem(planetDat.SystemID).Name.ToUpper() + " SYSTEM";
            if (starDat.AssignedProvinceID != "")
                systemDisplaySecondaryDataLine.text += " | " + HelperFunctions.DataRetrivalFunctions.GetProvince(HelperFunctions.DataRetrivalFunctions.GetSystem(planetDat.SystemID).AssignedProvinceID).Name.ToUpper() + " PROVINCE";
            systemDisplayTertiaryDataLine.text = owner + " SOVEREIGNITY";
            systemDisplayTertiaryDataLine.text += " | " + HelperFunctions.StringConversions.ConvertPlanetRankEnum(planetDat.Rank).ToUpper();
        }

        void DisplaySystemData()
        {

            StarData starDat = GetSelectedStar().GetComponent<Star>().starData; // reference
            backingSphere.SetActive(true); // turn on the sphere

            if (fov < 80)  // hide stars during zoom in
            {
                GetSelectedStar().GetComponent<Light>().enabled = false; // cut the light
                gStyle.normal.textColor = new Color(1, 1, 1, ((30 - fov) * 10) / 255); // fade in the text color

                if (fov < 60)
                {
                    HideStellarObjects(); // hide stars around the star when in close
                    HideCivilizationRangeCircles(); // hide range circles
                }

                // draw the labels
                var nameVector = Camera.main.WorldToScreenPoint(GetSelectedStar().transform.position);
                Vector2 vectorTwo = GUIUtility.ScreenToGUIPoint(new Vector2(nameVector.x, nameVector.y));

                // create system grid if not yet created
                if (!systemGridCreated)
                {
                    Vector3 position = new Vector3(GetSelectedStar().transform.position.x - 50, GetSelectedStar().transform.position.y, GetSelectedStar().transform.position.z);

                    smallSystemGrid = Instantiate(systemGrid, position, Quaternion.Euler(279, 180, 180)) as GameObject; // was 90,0,0, with top-down turn
                    smallSystemGrid.transform.localScale = new Vector3(smallSystemGrid.transform.localScale.x * ((float)Screen.width / 1920f), smallSystemGrid.transform.localScale.y * ((float)Screen.width / 1920f),
                        smallSystemGrid.transform.localScale.z * ((float)Screen.width / 1920f));
                    systemGridCreated = true;
                }
                else
                {
                    smallSystemGrid.SetActive(true);
                }

                // create system trail if not yet created
                if (!systemTrailCreated)
                {
                    Vector3 starPosition = GetSelectedStar().GetComponent<Star>().transform.position;
                    Vector3 trailLocation = new Vector3(starPosition.x - 35, starPosition.y, 0);
                    sysTrail = Instantiate(systemTrail, trailLocation, Quaternion.identity) as GameObject;
                    systemTrailCreated = true;
                }
                else
                {
                    sysTrail.SetActive(true); // shows the trail
                }

                // draw planets in system view if not yet created
                if (!planetsDrawn && (starDat.IntelLevel > eStellarIntelLevel.Low || gameDataRef.DebugMode))
                {
                    DrawPlanets(starDat);
                }

                smallSystemGrid.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, ((30 - fov) * 8) / 255);  // fade in the grid
                systemUICanvas.SetActive(true); // show the display
                selectedUnitInfoCanvas.SetActive(true); // show information panel
                systemDisplaySystemName.color = new Color(1, 1, 1, ((45 - fov) * 7) / 255);  // fade in the grid
                systemDisplaySystemName.text = starDat.Name.ToUpper();  //show text
                if (Screen.width < 1420)
                    systemHeaderImage.transform.localScale = new Vector3(.9f, .9f);

                string starType = "";
                if (starDat.starMultipleType == StarData.eStarMultiple.Binary)
                    starType = "BINARY ";
                if (starDat.starMultipleType == StarData.eStarMultiple.Trinary)
                    starType = "TRINARY ";

                systemDisplaySecondaryDataLine.text = "CLASS " + starDat.SpectralClass.ToString().ToUpper() + starDat.SecondarySpectralClass.ToString("N0") + " " + starType + "STAR";
                
                string owner = "NO";
                Color secondaryDisplayColor = Color.green;
                Color tertiaryDisplayColor = Color.green;

                foreach (Civilization civ in gameDataRef.CivList)
                {
                    List<StarData> civSystemList = HelperFunctions.DataRetrivalFunctions.GetCivSystemList(civ);

                    if (civSystemList.Exists(p => p.ID == starDat.ID))
                    {
                        owner = civ.Name.ToUpper();
                        tertiaryDisplayColor = civ.Color;
                        if (starDat.AssignedProvinceID != "")
                            systemDisplaySecondaryDataLine.text += " | " + HelperFunctions.DataRetrivalFunctions.GetProvince(starDat.AssignedProvinceID).Name.ToUpper() + " PROVINCE";
                    }
                }
                if (starDat.IntelValue > 6 || gameDataRef.DebugMode)
                {
                    systemDisplayTertiaryDataLine.enabled = true;
                    systemDisplayTertiaryDataLine.text = owner + " SOVEREIGNITY";                  
                    systemDisplayTertiaryDataLine.color = new Color(tertiaryDisplayColor.r, tertiaryDisplayColor.g, tertiaryDisplayColor.b, ((45 - fov) * 7) / 255);
                }
                else
                {
                    systemDisplayTertiaryDataLine.enabled = false;
                }
                systemDisplaySecondaryDataLine.color = new Color(secondaryDisplayColor.r, secondaryDisplayColor.g, secondaryDisplayColor.b, ((45 - fov) * 7) / 255);
            }
        }

        void DrawPlanets(StarData sData)
        {
            GameObject selectedStar = GetSelectedStar();
            Vector3 starPosition = GetSelectedStar().GetComponent<Star>().transform.position;

            for (int x = 0; x < 6; x++)
            {
                if (sData.PlanetSpots[x] != null)
                {
                    // add planet to draw, if a sprite exists
                    if (sData.PlanetSpots[x].PlanetSpriteNumber != -1)
                        switch (sData.PlanetSpots[x].Type)
                        {
                            case PlanetData.ePlanetType.AsteroidBelt:
                                {
                                    planetsToDraw[x] = asteroidBelts[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.Barren:
                                {
                                    planetsToDraw[x] = barrenPlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.Greenhouse:
                                {
                                    planetsToDraw[x] = greenhousePlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.Desert:
                                {
                                    planetsToDraw[x] = desertPlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.Terran:
                                {
                                    planetsToDraw[x] = terranPlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.Ice:
                                {
                                    planetsToDraw[x] = icePlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.IceGiant:
                                {
                                    planetsToDraw[x] = dwarfJovianPlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.GasGiant:
                                {
                                    planetsToDraw[x] = jovianPlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.Lava:
                                {
                                    planetsToDraw[x] = lavaPlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.Irradiated:
                                {
                                    planetsToDraw[x] = irradiatedPlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.SuperEarth:
                                {
                                    planetsToDraw[x] = superEarthPlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.Ocean:
                                {
                                    planetsToDraw[x] = waterPlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.BrownDwarf:
                                {
                                    planetsToDraw[x] = brownDwarfPlanets[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.IceBelt:
                                {
                                    planetsToDraw[x] = iceBelts[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            case PlanetData.ePlanetType.DustRing:
                                {
                                    planetsToDraw[x] = dustRings[sData.PlanetSpots[x].PlanetSpriteNumber];
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                }
            }

            for (int x = 0; x < 6; x++)
            {
                if (planetsToDraw[x] != null)
                {
                    // planet draw routine
                    PlanetData pData = new PlanetData();
                    float startingPlanetMargin = 110;
                    float screenScaleFactor = (float)Screen.width / 1920f;
                    float screenWidthFactor = 0f;

                    if (Screen.width >= 1600)
                    {
                        screenWidthFactor = 15f;
                    }
                    else if (Screen.width >= 1400)
                    {
                        screenWidthFactor = 14f;
                    }
                    else if (Screen.width >= 1280)
                        screenWidthFactor = 14f;

                    else
                        screenWidthFactor = 12.3f;

                    pData = sData.PlanetSpots[x];

                    Vector3 position = new Vector3(starPosition.x + ((selectedStar.transform.localScale.x / 2) + (startingPlanetMargin * screenScaleFactor)) + ((x + 1) * ((float)Screen.width / screenWidthFactor)), starPosition.y, 10); // derive the planet position from the star
                    GameObject planet = Instantiate(planetsToDraw[x], position, Quaternion.Euler(0, 180, 0)) as GameObject; // draw the planet
                    planet.AddComponent<Planet>(); // add the planet container
                    planet.GetComponent<Planet>().planetData = pData; // and attach the data
                    planet.name = pData.Name;
                    planet.GetComponent<CircleCollider2D>().radius = 2.0f;
                    int size = 0;

                    // normalize the size of the object
                    if (pData.Type == PlanetData.ePlanetType.IceBelt || pData.Type == PlanetData.ePlanetType.AsteroidBelt || pData.Type == PlanetData.ePlanetType.IceBelt)
                        size = 50;
                    else
                        size = pData.Size;                        
                    
                    // size of the planet (need to increase)                                     
                    pData.PlanetSystemScaleSize = size *.25f; // set the planet's size as it should be in the system view (will probably set with the gameobject)

                    planet.transform.localScale = new Vector2(pData.PlanetSystemScaleSize, pData.PlanetSystemScaleSize); // scale the sprite depending on size
                    planet.transform.Rotate(180, 180, 0);
                    listSystemPlanetsCreated.Add(planet); // add the planet to the drawn list (for destruction later as a group)

                    //draw rings if they exist
                    if (pData.Rings)
                    {
                        GameObject ring = Instantiate(rings[pData.PlanetRingSpriteNumber], position, Quaternion.Euler(0, 0, 0)) as GameObject; // create the ring if it exists
                        ring.transform.SetParent(planet.transform, false); // set as child of planet
                        ring.transform.Rotate(new Vector3(0, 0, 0 + pData.PlanetRingTilt));
                        ring.transform.localScale = new Vector3(1, 1, 1);
                        ring.transform.localPosition = new Vector3(0, 0, 0);
                        ring.GetComponent<SpriteRenderer>().sortingOrder = 1;
                    }
                }
            }
            planetsDrawn = true; // set flag
        }
    }

    public class StellarObjectDataBlock
    {
        public enum eBlockType : int
        {
            Star,
            Nebula,
            Province,
            Lower
        }

        [HideInInspector] public eBlockType blockType = eBlockType.Star;
        [HideInInspector] public GameObject stellarObject; // object that the block is tied to
        [HideInInspector] public GameObject textObject; // text object that the block is named
        [HideInInspector] public Vector2 provinceObjectLocation; // where the province information is located
        [HideInInspector] public Rect provinceBounds; // where the province information is located
        [HideInInspector] public GameObject planetCountObject; // future UI object for additional information
        [HideInInspector] public Color ownerColor; // civ ownership color
        [HideInInspector] public string ownerName; // civ name
        [HideInInspector] public string ownerTolerance; // civ planet tolerance
        [HideInInspector] public float objectRotation; // rotation of the block (for constellations and nebulas)

    }
}