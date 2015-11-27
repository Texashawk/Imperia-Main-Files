using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using CharacterObjects;


public class CharacterScreenActivation : MonoBehaviour, IPointerClickHandler
{
    GlobalGameData gDataRef;
    CharacterScreen cScreen;
    Character cData;
   

    void Awake()
    {    
        gDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
        cScreen = GameObject.Find("Character Window Canvas").GetComponent<CharacterScreen>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ActivateWindow();
    }

    public void ActivateWindow()
    {
        gDataRef.CharacterWindowActive = true;
        gDataRef.SelectedCharacter = cData;
        gDataRef.modalIsActive = true;
        cScreen.CharacterDataLoaded = false;
        cScreen.InitializeData(cData);
    }

    public void InitializeData(Character characterData)
    {
        cData = characterData;
    }
   
}
