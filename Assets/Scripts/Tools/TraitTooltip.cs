using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using StellarObjects;
using CameraScripts;

public class TraitTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private bool tooltipDisplayed = false;
    public RectTransform TooltipItem;
    private Vector3 tooltipOffset;
    private GalaxyData gDataRef;
    private GalaxyCameraScript galCameraRef;
    private Vector3 toolTipOriginalLocation;

    void Awake()
    {
        gDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>(); // set the reference
        galCameraRef = GameObject.Find("Main Camera").GetComponent<GalaxyCameraScript>();
        toolTipOriginalLocation = TooltipItem.transform.localPosition;
    }

	// Use this for initialization
	void Start () {
	    
        // Deactivate the tooltip so that is only active when you want it
        TooltipItem.gameObject.SetActive(tooltipDisplayed);
	}

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipDisplayed = false; // set as active the tooltip
       
        if ((transform.name == "Planet Trait 1") && (transform.GetComponent<Text>().text != "NO UNIQUE TRAITS"))
        {
            tooltipDisplayed = true;
            //DrawTooltip();
            
        }
        if ((transform.name == "Planet Trait 2") && (transform.GetComponent<Text>().text != ""))
        {
            tooltipDisplayed = true;
            //DrawTooltip();
        }

        TooltipItem.gameObject.SetActive(tooltipDisplayed); // activate tooltip
        TooltipItem.SetAsLastSibling();
    }

    void Update()
    {
        if (tooltipDisplayed)
        {
            DrawTooltip();
        }
    }

    private void DrawTooltip()
    {
        int bioMod = 0;
        int prodMod = 0;
        int habTilesMod = 0;
        int researchMod = 0;
        int energyMod = 0;
        int attackMod = 0;
        string name = "";
        string desc = "";
        // draw according to screen
        if (galCameraRef.zoomLevel == GalaxyCameraScript.cameraZoomLevel.Planet)
        {
            TooltipItem.transform.localPosition = new Vector3(toolTipOriginalLocation.x + TooltipItem.GetComponent<RectTransform>().rect.width + 270, toolTipOriginalLocation.y - 30, toolTipOriginalLocation.z);
        }
        
        // get values from trait table
        if ((transform.GetComponent<Text>().text != "") && (transform.GetComponent<Text>().text != "NO UNIQUE TRAITS")) // is transform valid?
        {
            name = gDataRef.PlanetTraitDataList.Find(p => p.Name == transform.GetComponent<Text>().text.ToLower()).Name;
            desc = "'" + gDataRef.PlanetTraitDataList.Find(p => p.Name == transform.GetComponent<Text>().text.ToLower()).Description + ".'";
            bioMod = gDataRef.PlanetTraitDataList.Find(p => p.Name == transform.GetComponent<Text>().text.ToLower()).HabMod;
            prodMod = gDataRef.PlanetTraitDataList.Find(p => p.Name == transform.GetComponent<Text>().text.ToLower()).ProdMod;
            habTilesMod = gDataRef.PlanetTraitDataList.Find(p => p.Name == transform.GetComponent<Text>().text.ToLower()).HabitableTilesMod;
            researchMod = gDataRef.PlanetTraitDataList.Find(p => p.Name == transform.GetComponent<Text>().text.ToLower()).ResearchMod;
            energyMod = gDataRef.PlanetTraitDataList.Find(p => p.Name == transform.GetComponent<Text>().text.ToLower()).EnergyMod;
            attackMod = gDataRef.PlanetTraitDataList.Find(p => p.Name == transform.GetComponent<Text>().text.ToLower()).AttackMod;

            TooltipItem.transform.Find("Trait Name").GetComponent<Text>().text = name.ToUpper();
            TooltipItem.transform.Find("Trait Description").GetComponent<Text>().text = desc.ToUpper();
            TooltipItem.transform.Find("Bio Mod").GetComponent<Text>().text = bioMod.ToString("N0");
            TooltipItem.transform.Find("Prod Mod").GetComponent<Text>().text = prodMod.ToString("N0");
            TooltipItem.transform.Find("Hab Tiles Mod").GetComponent<Text>().text = habTilesMod.ToString("N0");
            TooltipItem.transform.Find("Energy Mod").GetComponent<Text>().text = energyMod.ToString("N0");
            TooltipItem.transform.Find("Research Mod").GetComponent<Text>().text = researchMod.ToString("N0");
            TooltipItem.transform.Find("Attack Mod").GetComponent<Text>().text = attackMod.ToString("N0");

            // change colors of values
            TooltipItem.transform.Find("Bio Mod").GetComponent<Text>().color = Color.white;
            TooltipItem.transform.Find("Prod Mod").GetComponent<Text>().color = Color.white;
            TooltipItem.transform.Find("Hab Tiles Mod").GetComponent<Text>().color = Color.white;
            TooltipItem.transform.Find("Research Mod").GetComponent<Text>().color = Color.white;
            TooltipItem.transform.Find("Energy Mod").GetComponent<Text>().color = Color.white;
            TooltipItem.transform.Find("Attack Mod").GetComponent<Text>().color = Color.white;

            if (bioMod > 0)
                TooltipItem.transform.Find("Bio Mod").GetComponent<Text>().color = Color.green;
            else if (bioMod < 0)
                TooltipItem.transform.Find("Bio Mod").GetComponent<Text>().color = Color.red;
            if (prodMod > 0)
                TooltipItem.transform.Find("Prod Mod").GetComponent<Text>().color = Color.green;
            else if (prodMod < 0)
                TooltipItem.transform.Find("Prod Mod").GetComponent<Text>().color = Color.red;
            if (habTilesMod > 0)
                TooltipItem.transform.Find("Hab Tiles Mod").GetComponent<Text>().color = Color.green;
            else if (habTilesMod < 0)
                TooltipItem.transform.Find("Hab Tiles Mod").GetComponent<Text>().color = Color.red;
            if (researchMod > 0)
                TooltipItem.transform.Find("Research Mod").GetComponent<Text>().color = Color.green;
            else if (researchMod < 0)
                TooltipItem.transform.Find("Research Mod").GetComponent<Text>().color = Color.red;
            if (energyMod > 0)
                TooltipItem.transform.Find("Energy Mod").GetComponent<Text>().color = Color.green;
            else if (energyMod < 0)
                TooltipItem.transform.Find("Energy Mod").GetComponent<Text>().color = Color.red;
            if (attackMod > 0)
                TooltipItem.transform.Find("Attack Mod").GetComponent<Text>().color = Color.green;
            else if (attackMod < 0)
                TooltipItem.transform.Find("Attack Mod").GetComponent<Text>().color = Color.red;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipDisplayed = false;
        TooltipItem.gameObject.SetActive(tooltipDisplayed);
    }
}
