using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instanceRef;

    // Use this for initialization
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

    // Update is called once per frame
    void Update()
    {

    }
}
