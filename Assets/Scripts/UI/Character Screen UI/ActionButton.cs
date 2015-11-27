using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class ActionButton : MonoBehaviour
{
    private string buttonID;
    private string characterID;
    private GraphicAssets gAssets;
    public Text ButtonText;
    public Image ButtonImage;
    public ActionScrollView ScrollView;

    public void Awake()
    {
        gAssets = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
    }

    public void SetName(string name)
    {      
        ButtonText.text = name;
    }

    public void SetID(string ID)
    {
        buttonID = ID;        
    }

    public void SetPicture(string picID)
    {
        ButtonImage.sprite = gAssets.CharacterList.Find(p => p.name == picID);
    }

    public void SetCharacter(string cID)
    {
        characterID = cID;
    }

    public void Button_Click()
    {
        ScrollView.ButtonClicked(buttonID, HelperFunctions.DataRetrivalFunctions.GetCharacter(characterID));  
    }
}
