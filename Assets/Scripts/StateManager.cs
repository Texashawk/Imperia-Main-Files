using UnityEngine;
using Assets.Scripts.States;
using System.Collections;
using Assets.Scripts.Interfaces;

public class StateManager : MonoBehaviour
   {
    private IStateBase activeState;
    [HideInInspector] public GalaxyData galaxyDataRef; //reference (link) to all galaxy data
    private static StateManager instanceRef;
    public GlobalGameData gameDataRef;

    void Awake()
    {
        if (instanceRef == null)
        {
            instanceRef = this;
            DontDestroyOnLoad(gameObject); //the statemanager is attached to this game object, preventing it from being destroyed
        }
        else
        {
            DestroyImmediate(gameObject);  //and when the beginning scene is loaded, this will destroy the extra game manager created
        }
    }
       
    void Start()
    {
        // initialize state machine and managers
        activeState = new BeginState(this);
        galaxyDataRef = GetComponent<GalaxyData>();  // gets the galaxy data script containing the data structure
        gameDataRef = GetComponent<GlobalGameData>();  // gets global game data (screens, etc) that are used universally

        Debug.Log("This object is of type: " + activeState);
    }

    // Update is called once per frame
    void Update()
    {
        if (activeState != null)
        {
            activeState.StateUpdate();
        }
    }

    void OnGUI() //calls the drawing code for a specific mode
    {
        if (activeState != null)
            activeState.ShowIt();
    }

    public void SwitchState(IStateBase newState)
    {
        activeState = newState;
        //StartCoroutine(ChangeLevel());
    }

    IEnumerator ChangeLevel()
    {     
        float fadeTime = GameObject.Find("GameManager").GetComponent<Fading>().BeginFade(1);
        yield return new WaitForSeconds(fadeTime);
        Application.LoadLevel(Application.loadedLevel + 1);
        fadeTime = GameObject.Find("GameManager").GetComponent<Fading>().BeginFade(1);
        yield return new WaitForSeconds(fadeTime);
    }
 }
