using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using StellarObjects;
using PlanetObjects;
using HelperFunctions;

public class RegionTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{

	private bool tooltipDisplayed = false; // initially set false
    public GameObject TooltipItem; // the rect transform
    private Canvas planetUICanvas;
    public Sprite forestPic;
    public Sprite mountainPic;
    public Sprite junglePic;
    public Sprite plainsPic;
    public Sprite barrenPic;
    public Sprite lavaPic;
    public Sprite oceanPic;
    public Sprite desertPic;
    public Sprite volcanicPic;
    public Sprite deadPic;
    public Sprite icePic;
    public Sprite impassiblePic;
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
        //tooltipDisplayed = true;  // set as active the tooltip
        //transform.localScale = new Vector3(1.5f, 1.5f, 1.5f); // make the square a little bigger when moused over
        //DrawTooltip();
    }

    private void DrawTooltip()
    {
        toolTipObject = Instantiate(TooltipItem, Camera.main.WorldToScreenPoint(transform.position), Quaternion.identity) as GameObject;
        rData = HelperFunctions.DataRetrivalFunctions.GetRegion(transform.name);
        //toolTipObject.transform.Find("Tile Name").GetComponent<Text>().text = rData.ID; // assign the name to the text object child
        Color popColor = Color.white;

        // test to change pic
        Sprite regionPic;
        switch(rData.RegionType.Type)
        {
            case RegionTypeData.eRegionType.Barren:
                regionPic = barrenPic;
                break;
            case RegionTypeData.eRegionType.Dead:
                regionPic = barrenPic;
                break;
            case RegionTypeData.eRegionType.Forest:
                regionPic = forestPic;
                break;
            case RegionTypeData.eRegionType.Frozen:
                regionPic = icePic;
                break;
            case RegionTypeData.eRegionType.Grassland:
                regionPic = plainsPic;
                break;
            case RegionTypeData.eRegionType.Plains:
                regionPic = plainsPic;
                break;
            case RegionTypeData.eRegionType.Uninhabitable:
                regionPic = impassiblePic;
                break;
            case RegionTypeData.eRegionType.Jungle:
                regionPic = junglePic;
                break;
            case RegionTypeData.eRegionType.Lava:
                regionPic = lavaPic;
                break;
            case RegionTypeData.eRegionType.Volcanic:
                regionPic = volcanicPic;
                break;
            case RegionTypeData.eRegionType.Mountains:
                regionPic = mountainPic;
                break;
            case RegionTypeData.eRegionType.Ocean:
                regionPic = oceanPic;
                break;
            case RegionTypeData.eRegionType.Desert:
                regionPic = desertPic;
                break;
            default:
                regionPic = barrenPic;
                break;
        }
       
        toolTipObject.transform.Find("Region Image").GetComponent<Image>().sprite = regionPic;
        if (rData.PopsInTile.Count > rData.MaxSafePopulationLevel * 1.1f)
        {
            popColor = Color.yellow;
        }
        if (rData.PopsInTile.Count > rData.MaxSafePopulationLevel * 1.25f)
        {
            popColor = Color.red;
        }
        toolTipObject.transform.Find("Base Info Panel/Current Pop Data").GetComponent<Text>().text = rData.PopsInTile.Count.ToString("N0") + " MILLION"; // assign the name to the text object child
        toolTipObject.transform.Find("Base Info Panel/Current Pop Data").GetComponent<Text>().color = popColor;
        toolTipObject.transform.Find("Base Info Panel/Current Max Pop Data").GetComponent<Text>().text = rData.MaxSafePopulationLevel.ToString("N0") + " MILLION"; // assign the name to the text object child
        toolTipObject.transform.Find("Base Info Panel/Employment Ratio").GetComponent<Text>().text = rData.UnemploymentLevel.ToString("P1"); // assign the name to the text object child
        toolTipObject.transform.Find("Region Type").GetComponent<Text>().text = rData.RegionType.Type.ToString().ToUpper();

        // production levels
        toolTipObject.transform.Find("Region Production Panel/Basic Materials Production").GetComponent<Text>().text = rData.AlphaMaterialsPerTile.ToString("N0") + " mT"; // assign the name to the text object child
        toolTipObject.transform.Find("Region Production Panel/Energy Production").GetComponent<Text>().text = rData.EnergyPerTile.ToString("N0") + " tJ"; // assign the name to the text object
        toolTipObject.transform.Find("Region Production Panel/Food Production").GetComponent<Text>().text = rData.FoodPerTile.ToString("N0") + " kT"; // assign the name to the text object
        toolTipObject.transform.Find("Region Production Panel/Heavy Materials Production").GetComponent<Text>().text = rData.HeavyPerTile.ToString("N0") + " mT"; // assign the name to the text object
        toolTipObject.transform.Find("Region Production Panel/Rare Materials Production").GetComponent<Text>().text = rData.RarePerTile.ToString("N0") + " mT"; // assign the name to the text object

        // development levels
        toolTipObject.transform.Find("Development Panel/Agriculture Development Level").GetComponent<Text>().text = rData.FarmingLevel.ToString("N0") + "/" + rData.FarmsStaffed.ToString("N1"); // assign the name to the text object child
        toolTipObject.transform.Find("Development Panel/Manufacturing Development Level").GetComponent<Text>().text = rData.ManufacturingLevel.ToString("N0") + "/" + rData.FactoriesStaffed.ToString("N1"); // assign the name to the text object child
        toolTipObject.transform.Find("Development Panel/Science Development Level").GetComponent<Text>().text = rData.ScienceLevel.ToString("N0") + "/" + rData.LabsStaffed.ToString("N1"); // assign the name to the text object child
        toolTipObject.transform.Find("Development Panel/Mining Development Level").GetComponent<Text>().text = rData.MiningLevel.ToString("N0") + "/" + rData.MinesStaffed.ToString("N1"); ; // assign the name to the text object child
        toolTipObject.transform.Find("Development Panel/High Tech Development Level").GetComponent<Text>().text = rData.HighTechLevel.ToString("N0") + "/" + rData.HighTechFacilitiesStaffed.ToString("N1"); ; // assign the name to the text object child
        toolTipObject.transform.Find("Development Panel/Government Development Level").GetComponent<Text>().text = rData.GovernmentLevel.ToString("N0") + "/" + rData.GovernmentFacilitiesStaffed.ToString("N1"); ; // assign the name to the text object child

        // base levels
        toolTipObject.transform.Find("Base Stats Panel/Bio Rating").GetComponent<Text>().text = rData.BioRating.ToString("N0"); // assign the name to the text object child
        toolTipObject.transform.Find("Base Stats Panel/Basic Material Rating").GetComponent<Text>().text = rData.AlphaRating.ToString("N0"); // assign the name to the text object child
        toolTipObject.transform.Find("Base Stats Panel/Rare Material Rating").GetComponent<Text>().text = rData.RareRating.ToString("N0"); // assign the name to the text object child
        toolTipObject.transform.Find("Base Stats Panel/Heavy Material Rating").GetComponent<Text>().text = rData.HeavyRating.ToString("N0"); // assign the name to the text object child
        toolTipObject.transform.Find("Base Stats Panel/Energy Rating").GetComponent<Text>().text = rData.EnergyRating.ToString("N0"); // assign the name to the text object child

        // region
        toolTipObject.transform.SetParent(planetUICanvas.transform);
        toolTipObject.transform.localScale = new Vector3(1, 1, 1);
        toolTipObject.transform.Rotate(new Vector3(0, -20, 0));
        toolTipObject.transform.localPosition = new Vector3(-transform.parent.GetComponent<RectTransform>().rect.width - 20, -80, 0);
        // do some stuff
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipDisplayed = false;
        transform.localScale = new Vector3(1, 1, 1);
        GameObject.Destroy(toolTipObject);
    }

}
