using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class ModalPanel : MonoBehaviour {

    public Text questionString;
    public Image iconImage;
    public Button yesButton;
    public Button noButton;
    public Button cancelButton;
    public GameObject modalPanelObject;
    private static GlobalGameData gGameData;

    private static ModalPanel modalPanel;

    public static ModalPanel Instance()
    {
        if (!modalPanel)
        {
            gGameData = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
            //modalPanel = new ModalPanel();
            modalPanel = FindObjectOfType(typeof(ModalPanel)) as ModalPanel;
            if (!modalPanel)
                Debug.LogError("There needs to be one active ModalPanel script on a GameObject in your scene.");
        }     
        return modalPanel;
    }

    // Yes/No/Cancel: A string, a yes event, a no event and a cancel event
    public void Choice (string question, UnityAction yesEvent, UnityAction noEvent, UnityAction cancelEvent)
    {
        modalPanelObject.SetActive(true); // open the panel
        gGameData.modalIsActive = true;

        // set up the event calls for each button
        yesButton.onClick.RemoveAllListeners(); // remove all listeners from the event window
        yesButton.onClick.AddListener(yesEvent);
        yesButton.onClick.AddListener(ClosePanel);

        noButton.onClick.RemoveAllListeners(); // remove all listeners from the event window
        noButton.onClick.AddListener(noEvent);
        noButton.onClick.AddListener(ClosePanel);

        cancelButton.onClick.RemoveAllListeners(); // remove all listeners from the event window
        cancelButton.onClick.AddListener(cancelEvent);
        cancelButton.onClick.AddListener(ClosePanel);

        this.questionString.text = question; // set the text string to the question

        this.iconImage.gameObject.SetActive(true);

        // turn on the buttons
        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);
        cancelButton.gameObject.SetActive(true);
    }

    void ClosePanel()
    {
        gGameData.modalIsActive = false;
        modalPanelObject.SetActive(false); // sets the panel to false
    }
}
