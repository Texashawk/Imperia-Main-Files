using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Tooltips;
using CharacterObjects;

public class RelationsButton : MonoBehaviour
{
    private string buttonID;
    private GraphicAssets gAssets;
    public Text ButtonText;
    public Image CharacterImage;
    public Text RelationsText;
    public Text CharacterName;
    public RelationsScrollView ScrollView;

    public void Awake()
    {
        gAssets = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
    }

    public void SetRelationsValue(Relationship relations)
    {
        float trustValue = relations.Trust;
        string relationshipType = relations.RelationshipState.ToString().ToUpper();
        //ButtonText.text = trustValue.ToString("N0");
        if (trustValue > 70)
            ButtonText.color = Color.green;
        else if (trustValue < 30)
            ButtonText.color = Color.red;
        else
            ButtonText.color = Color.yellow;

        // set color and text of relationship text
        RelationsText.text = relationshipType;
        switch (relations.RelationshipState)
        {
            case Relationship.eRelationshipState.Neutral:      
                RelationsText.color = Color.white;
                break;              
            case Relationship.eRelationshipState.Allies:
                RelationsText.color = Color.green;
                break;
            case Relationship.eRelationshipState.Friends:
                RelationsText.color = Color.green;
                break;
            case Relationship.eRelationshipState.Superior:
                RelationsText.color = new Color(.9f, .67f, .15f); // orange
                break;
            case Relationship.eRelationshipState.Inferior:
                RelationsText.color = Color.yellow;
                break;
            case Relationship.eRelationshipState.Challenger:
                RelationsText.color = Color.magenta;
                break;
            case Relationship.eRelationshipState.Challengee:
                RelationsText.color = Color.yellow;
                break;
            case Relationship.eRelationshipState.Rivals:
                RelationsText.color = Color.red;
                break;
            case Relationship.eRelationshipState.Vengeance:
                RelationsText.color = Color.red;
                break;
            case Relationship.eRelationshipState.VengeanceUpon:
                RelationsText.color = Color.yellow;
                break;
            case Relationship.eRelationshipState.Vendetta:
                RelationsText.color = Color.red;
                break;
            case Relationship.eRelationshipState.HangerOn:
                RelationsText.color = Color.cyan;
                break;
            case Relationship.eRelationshipState.HungUpon:
                RelationsText.color = Color.cyan;
                break;
            default:
                RelationsText.color = Color.white;
                break;
        }
    }

    public void SetID(string ID)
    {
        buttonID = ID;
        name = ID;
    }

    public void SetName(string name)
    {
        CharacterName.text = name.ToUpper();
    }

    public void SetPicture(string picID)
    {
        CharacterImage.sprite = gAssets.CharacterList.Find(p => p.name == picID);
    }
    public void Button_Click()
    {
        ScrollView.ButtonClicked(buttonID);       
    }
}
