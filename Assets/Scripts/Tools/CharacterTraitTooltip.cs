using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CharacterObjects;
using System.Collections;
using CameraScripts;

public class CharacterTraitTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltipPrefab; // the prefab to use
    private GameObject tooltipObject; // the actual tooltip instance
    private bool displayToolTip = false;
    private bool toolTipCreated = false;
    public bool exitRequested = false;
    private GraphicAssets graphicDataRef;
    private GlobalGameData gDataRef;
    private Canvas uiCanvas;
    private float alphaValue = 0f;
    private Color fadeColor;
    private float offsetAmount = 2.0f; // sets the distance away from the center of the tooltipped object

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

        string checkNull = "";
        checkNull = transform.GetComponent<Text>().text;
        if (checkNull != "\n") // is transform valid?
        {
            alphaValue = 0f;           
            displayToolTip = true;
            exitRequested = false;
        }
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        toolTipCreated = false;
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
        tooltipObject.transform.Find("Name").GetComponent<Text>().color = fadeColor;
        tooltipObject.transform.Find("Description").GetComponent<Text>().color = fadeColor;   
    }

    private void DrawToolTip()
    {
        
        if (!toolTipCreated)
        {
            string name = "";
            string desc = "";

            Vector3 tooltipLocation = new Vector3(transform.position.x + offsetAmount, transform.position.y - 9, transform.position.z);
                
            tooltipObject = Instantiate(tooltipPrefab, tooltipLocation, Quaternion.identity) as GameObject;             
            tooltipObject.transform.SetParent(uiCanvas.transform, true); // assign to canvas
            tooltipObject.transform.localScale = new Vector3(.8f, .8f, .8f);

            if (transform.GetComponent<Text>().text != "\n") // is transform valid?
            {
                name = gDataRef.CharacterTraitList.Find(p => p.Name.ToUpper() == transform.GetComponent<Text>().text.ToUpper()).Name.ToUpper();
                desc = "'" + gDataRef.CharacterTraitList.Find(p => p.Name.ToUpper() == name).Description.ToUpper();
            
                tooltipObject.transform.Find("Name").GetComponent<Text>().text = name.ToUpper();
                tooltipObject.transform.Find("Description").GetComponent<Text>().text = desc.ToUpper();     
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
