using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using StellarObjects;
using PlanetObjects;
using HelperFunctions;

public class RegionCensusTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private bool tooltipDisplayed = false; // initially set false
    public GameObject TooltipItem; // the rect transform
    private Canvas planetUICanvas;
   
    private GalaxyData gDataRef;
    private Image regionImage;
    private Region rData;
    private GameObject toolTipObject; // the actual tooltip object
    private Text tileIDText;
    private Text currentPopText;
    private Text maxPopText;
    private Text regionType;
    private Camera uiCamera;

    // development level text
    private Text agLevelText;
    private Text manLevelText;
    private Text sciLevelText;
    private Text hiTechLevelText;
    private Text miningLevelText;

    // input vars
    float mouseWheelValue = 0f;

    void Awake()
    {
        gDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>(); // set the reference
        planetUICanvas = GameObject.Find("Planet UI Canvas").GetComponent<Canvas>();
        uiCamera = GameObject.Find("UI Camera").GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Right Mouse Button") || (Input.GetAxis("Mouse ScrollWheel") != mouseWheelValue))
        {
            if (toolTipObject != null)
            {
                tooltipDisplayed = false;
                transform.localScale = new Vector3(1, 1, 1);
                GameObject.Destroy(toolTipObject);
            }
        }

        Ray ray = uiCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.name == this.name)
            {
                tooltipDisplayed = true;  // set as active the tooltip
                //transform.localScale = new Vector3(1.5f, 1.5f, 1.5f); // make the square a little bigger when moused over
                if (toolTipObject == null)
                    DrawTooltip();
            }
            else
            {
                if (toolTipObject != null)
                {
                    tooltipDisplayed = false;
                    //transform.localScale = new Vector3(1, 1, 1);
                    GameObject.Destroy(toolTipObject);
                }
            }
        }
        else
        {
            if (toolTipObject != null)
            {
                tooltipDisplayed = false;
                //transform.localScale = new Vector3(1, 1, 1);
                GameObject.Destroy(toolTipObject);
            }
        }
    }

    void LateUpdate()
    {
        mouseWheelValue = Input.GetAxis("Mouse ScrollWheel");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipDisplayed = true;  // set as active the tooltip
        DrawTooltip();
    }

    private void DrawTooltip()
    {
        toolTipObject = Instantiate(TooltipItem, Camera.main.WorldToScreenPoint(transform.position), Quaternion.identity) as GameObject;
        rData = HelperFunctions.DataRetrivalFunctions.GetRegion(transform.name);
        //toolTipObject.transform.Find("Tile Name").GetComponent<Text>().text = rData.ID; // assign the name to the text object child
        string sign = "";
        if ((rData.GraduatedLastTurn - rData.DiedLastTurn) > 0)
        {
            sign = "+";
        }
        else
            sign = "";
        
        toolTipObject.transform.Find("Base Info Panel/Current Pop Data").GetComponent<Text>().text = rData.PopsInTile.Count.ToString("N0") + " MILLION (" + sign + (rData.GraduatedLastTurn - rData.DiedLastTurn).ToString("N0") + ")" ; // assign the name to the text object child
        toolTipObject.transform.Find("Base Info Panel/Current Max Pop Data").GetComponent<Text>().text = rData.MaxSafePopulationLevel.ToString("N0") + " MILLION"; // assign the name to the text object child
        toolTipObject.transform.Find("Base Info Panel/Immigrating Data").GetComponent<Text>().text = rData.ImmigratedLastTurn.ToString("N0") + " MILLION";
        toolTipObject.transform.Find("Base Info Panel/Emigrating Data").GetComponent<Text>().text = rData.EmigratedLastTurn.ToString("N0") + " MILLION";
        toolTipObject.transform.Find("Region Type").GetComponent<Text>().text = rData.RegionDevelopmentLevel.ToString().ToUpper() + "(" + rData.HabitatationInfrastructureLevel.ToString("N0") + ")";

        // production levels
        toolTipObject.transform.Find("Region Production Panel/Basic Materials Production").GetComponent<Text>().text = rData.AlphaMaterialsPerPop.ToString("N0") + " mT"; // assign the name to the text object child
        toolTipObject.transform.Find("Region Production Panel/Energy Production").GetComponent<Text>().text = rData.EnergyPerPop.ToString("N0") + " tJ"; // assign the name to the text object
        toolTipObject.transform.Find("Region Production Panel/Food Production").GetComponent<Text>().text = rData.FoodPerPop.ToString("N0") + " kT"; // assign the name to the text object
        toolTipObject.transform.Find("Region Production Panel/Heavy Materials Production").GetComponent<Text>().text = rData.HeavyPerPop.ToString("N0") + " mT"; // assign the name to the text object
        toolTipObject.transform.Find("Region Production Panel/Rare Materials Production").GetComponent<Text>().text = rData.RarePerPop.ToString("N0") + " mT"; // assign the name to the text object

        // development levels
        toolTipObject.transform.Find("Development Panel/Agriculture Development Level").GetComponent<Text>().text = rData.FarmingPopRating.ToString("N0"); // assign the name to the text object child
        toolTipObject.transform.Find("Development Panel/Manufacturing Development Level").GetComponent<Text>().text = rData.ManufacturingPopRating.ToString("N0"); // assign the name to the text object child
        toolTipObject.transform.Find("Development Panel/Science Development Level").GetComponent<Text>().text = rData.SciencePopRating.ToString("N0"); // assign the name to the text object child
        toolTipObject.transform.Find("Development Panel/Mining Development Level").GetComponent<Text>().text = rData.MiningPopRating.ToString("N0"); // assign the name to the text object child
        toolTipObject.transform.Find("Development Panel/High Tech Development Level").GetComponent<Text>().text = rData.HighTechPopRating.ToString("N0"); // assign the name to the text object child

        // demographic levels
        toolTipObject.transform.Find("Demographics Panel/Worker Pops").GetComponent<Text>().text = rData.WorkerPops.ToString("N0"); // assign the name to the text object child
        toolTipObject.transform.Find("Demographics Panel/Retired Pops").GetComponent<Text>().text = rData.RetiredPops.ToString("N0"); // assign the name to the text object child

        // base levels
        toolTipObject.transform.Find("Base Stats Panel/Bio Rating").GetComponent<Text>().text = rData.BioRating.ToString("N0"); // assign the name to the text object child
        toolTipObject.transform.Find("Base Stats Panel/Basic Material Rating").GetComponent<Text>().text = rData.AlphaRating.ToString("N0"); // assign the name to the text object child
        toolTipObject.transform.Find("Base Stats Panel/Rare Material Rating").GetComponent<Text>().text = rData.RareRating.ToString("N0"); // assign the name to the text object child
        toolTipObject.transform.Find("Base Stats Panel/Heavy Material Rating").GetComponent<Text>().text = rData.HeavyRating.ToString("N0"); // assign the name to the text object child
        toolTipObject.transform.Find("Base Stats Panel/Energy Rating").GetComponent<Text>().text = rData.EnergyRating.ToString("N0"); // assign the name to the text object child

        // number of pops in each region       
        toolTipObject.transform.Find("Demographics Panel/Miner Pops").GetComponent<Text>().text = rData.TotalMiners.ToString("N0") + "/" + rData.EmployedMiners.ToString("N0") + "/" + rData.UnemployedMiners.ToString("N0"); // assign the name to the text object child
        toolTipObject.transform.Find("Demographics Panel/Farmer Pops").GetComponent<Text>().text = rData.TotalFarmers.ToString("N0") + "/" + rData.EmployedFarmers.ToString("N0") + "/"  + rData.UnemployedFarmers.ToString("N0"); ; // assign the name to the text object child
        toolTipObject.transform.Find("Demographics Panel/Fluxmen Pops").GetComponent<Text>().text = rData.TotalFluxmen.ToString("N0") + "/" + rData.EmployedFluxmen.ToString("N0") + "/" + rData.UnemployedFluxmen.ToString("N0");
        toolTipObject.transform.Find("Demographics Panel/Merchant Pops").GetComponent<Text>().text = rData.TotalMerchants.ToString("N0"); // assign the name to the text object child
        toolTipObject.transform.Find("Demographics Panel/Engineer Pops").GetComponent<Text>().text = rData.TotalEngineers.ToString("N0") + "/" + rData.EmployedEngineers.ToString("N0") + "/" + rData.UnemployedEngineers.ToString("N0"); // assign the name to the text object child
        toolTipObject.transform.Find("Demographics Panel/Administrator Pops").GetComponent<Text>().text = rData.TotalAdministrators.ToString("N0") + "/" + rData.EmployedAdminstrators.ToString("N0") + "/" + rData.UnemployedAdminstrators.ToString("N0"); // assign the name to the text object child
        toolTipObject.transform.Find("Demographics Panel/Scientist Pops").GetComponent<Text>().text = rData.TotalScientists.ToString("N0"); // assign the name to the text object child
        toolTipObject.transform.Find("Demographics Panel/Starving Pops").GetComponent<Text>().text = rData.StarvingPops.ToString("N0"); // assign the name to the text object child
        toolTipObject.transform.Find("Demographics Panel/Blackout Pops").GetComponent<Text>().text = rData.BlackedOutPops.ToString("N0"); // assign the name to the text object child

        // region
        toolTipObject.transform.SetParent(planetUICanvas.transform);
        toolTipObject.transform.localScale = new Vector3(1, 1, 1);
        toolTipObject.transform.Rotate(new Vector3(0, 20, 0));
        toolTipObject.transform.localPosition = new Vector3(transform.parent.GetComponent<RectTransform>().rect.width + 20, -80, 0);
        // do some stuff
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipDisplayed = false;
        transform.localScale = new Vector3(1, 1, 1);
        GameObject.Destroy(toolTipObject);
    }

}
