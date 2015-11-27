using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ShowGameCreationStats : MonoBehaviour
{

    GalaxyData gData;
    GlobalGameData gameData;
    Text planetsGenerated;
    Text housesGenerated;
    Text charactersGenerated;

    // Use this for initialization
    void Start()
    {
        gData = GameObject.Find("GameManager").GetComponent<GalaxyData>();
        gameData = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
        planetsGenerated = GameObject.Find("Planets Generated").GetComponent<Text>();
        housesGenerated = GameObject.Find("Houses Generated").GetComponent<Text>();
        charactersGenerated = GameObject.Find("Characters Generated").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        planetsGenerated.text = "STARS GENERATED: " + gData.GalaxyStarList.Count.ToString("N0"); // test to see if it draws planet counts
        housesGenerated.text = "HOUSES GENERATED: " + gameData.HouseList.Count.ToString("N0"); // test to see if it draws planet counts
        charactersGenerated.text = "CHARACTERS GENERATED: " + gameData.CharacterList.Count.ToString("N0"); // test to see if it draws planet counts
    }
}
