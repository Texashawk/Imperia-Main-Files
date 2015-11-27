using UnityEngine;
using System.Collections;
using GalaxyCreator;
using StellarObjects;

namespace CameraScripts
{
public class GalaxyCameraScript : MonoBehaviour {

    public enum cameraZoomLevel : int
    {
        Galaxy,
        Province,
        System,
        Planet
    };

	private float mouseWheelValue = 0.0f;
	private bool moveMapUp = false;
	private bool moveMapDown = false;
	private bool moveMapRight = false;
	private bool moveMapLeft = false;
    [HideInInspector]public bool ScrollWheelIsValid { get; set; } // can the scrollwheel be used to manipulate the camera?
	private float zoomSpeed = 9.5f;
    private float zoomSensitivity = 50f;
    private Camera mainC; // camera reference
    private GlobalGameData gDataRef;

    [HideInInspector]public cameraZoomLevel zoomLevel;
	private const int scrollSpeedVariable = 1200;
	[HideInInspector]public bool systemZoomActive = false;
    [HideInInspector]public bool planetZoomActive = false;
    [HideInInspector]public bool provinceZoomActive = false;
    public bool systemZoomComplete = false;
    private bool planetZoomComplete = false;
    private bool provinceZoomComplete = false;
    private bool planetToSystemZoom = false;
	public Transform starTarget = null;
    public Transform planetTarget = null;
    public Province provinceTarget = null;
    private Vector3 cameraSystemPosition; // stores the camera position when in system view
    private Vector3 cameraPlanetPosition; // stores the camera position when in planet view
    private Vector3 cameraProvincePosition; // stores the camera position when in province view
    private float scaleRatio;

    public const int galaxyMinZoomLevel = 120;
    public int galaxyHeight;
    public int galaxyWidth;
    public const int provinceMinZoomLevel = 40;
    public const int systemMinZoomLevel = 12;
    public const int planetMinZoomLevel = 3;
    public const int maxZoomLevel = 135;
    public const int minZoomLevel = 30;

    public float zoom;


	// Use this for initialization
	void Start () {
        mainC = GameObject.Find("Main Camera").GetComponent<Camera>();
        gDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
        mainC.fieldOfView = maxZoomLevel;
        zoom = mainC.fieldOfView;
        galaxyHeight = gDataRef.GalaxySizeHeight;
        galaxyWidth = gDataRef.GalaxySizeWidth;
        ScrollWheelIsValid = true;
	}
	
	// Update is called once per frame
    void Update()
    {
        scaleRatio = (Screen.height / 1920f) * (Screen.width / 1080f);
       
        if (gDataRef.uiSubMode == GlobalGameData.eSubMode.None && !gDataRef.modalIsActive)  // only work in no submode
        {  
            if (provinceZoomActive && !systemZoomActive && provinceTarget != null)        
                ZoomToProvince(provinceTarget);
            
            if (systemZoomActive && !planetZoomActive && starTarget != null)
                ZoomToSystem(starTarget);

            if (systemZoomActive && planetZoomActive && planetTarget != null)
                ZoomToPlanet(planetTarget);

            if (provinceZoomActive)
            {
                if ((Input.GetAxis("Mouse ScrollWheel") > mouseWheelValue) || Input.GetButtonDown("Right Mouse Button"))
                {
                    transform.position = new Vector3(provinceTarget.ProvinceCenter.x, provinceTarget.ProvinceCenter.y, transform.position.z);
                    provinceZoomActive = false;
                    provinceZoomComplete = false;
                    zoom = 120;
                }
            }

            if (systemZoomActive)
                if ((Input.GetAxis("Mouse ScrollWheel") > mouseWheelValue) || Input.GetButtonDown("Right Mouse Button"))
                {
                    transform.position = new Vector3(transform.position.x - 38 - (Screen.width / 10), transform.position.y, transform.position.z);
                    systemZoomActive = false;
                    provinceZoomActive = false;
                    provinceZoomComplete = false;
                    systemZoomComplete = false;
                    zoom = 80;
                }

            if (planetZoomActive)
                if ((Input.GetAxis("Mouse ScrollWheel") > mouseWheelValue) || Input.GetButtonDown("Right Mouse Button"))
                {
                    planetTarget.transform.localScale = new Vector3(planetTarget.GetComponent<Planet>().planetData.PlanetSystemScaleSize,planetTarget.GetComponent<Planet>().planetData.PlanetSystemScaleSize,1); // reset the scale
                    planetTarget = null;
                    planetZoomActive = false;
                    planetZoomComplete = false;
                    provinceZoomActive = false;
                    provinceZoomComplete = false;
                    systemZoomActive = true;
                    zoom = systemMinZoomLevel;
                }

            // run movement/zoom functions
            CheckForMapPan();
            StartCoroutine(PanMap());
            CheckForMapZoom();
            DetermineZoomLevel(); // update zoom level of camera        
        }
    }

    void CheckForMapZoom()
    {       
        if (!systemZoomActive && !planetZoomActive && !provinceZoomActive)
        {
            // keyboard uses the zoom
            if (Input.GetKeyDown(KeyCode.KeypadPlus) || (Input.GetKeyDown(KeyCode.R)))
                zoom -= 10;

            if (Input.GetKeyDown(KeyCode.KeypadMinus) || (Input.GetKeyDown(KeyCode.F)))
                zoom += 10;

            if (ScrollWheelIsValid)
            {
                zoom -= mouseWheelValue * zoomSensitivity;
                zoom = Mathf.Clamp(zoom, minZoomLevel, maxZoomLevel);
            }
        }

        // set normal angle of camera
        if (zoom <= systemMinZoomLevel && systemZoomActive)
            transform.eulerAngles = new Vector3(180, 0, 0); // was 30,0,0
        else
        {   //int cameraAngle = 15;
            //transform.eulerAngles = new Vector3(180 - cameraAngle + (((180 + (cameraAngle * 30)) / mainC.fieldOfView)), 0, 0); // snap the camera back to 0 (top-down) when not in system zoom mode
            transform.eulerAngles = new Vector3(180, 0, 0); // was 30,0,0
        }
    }

    void CheckForMapPan()
    {
        if (zoomLevel < cameraZoomLevel.System && !provinceZoomActive) // don't pan the map if in system or planet view or if in province mode
        {
            // check for map movement
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                moveMapUp = true;
            else
                moveMapUp = false;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                moveMapLeft = true;
            else
                moveMapLeft = false;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                moveMapRight = true;
            else
                moveMapRight = false;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                moveMapDown = true;
            else
                moveMapDown = false;
        }
    }

    IEnumerator PanMap()
    {
        // scroll map (need to add bounds checks)  
        
        float yLocation = transform.position.y;
        float xLocation = transform.position.x;

        float moveUpVector = transform.position.y - scrollSpeedVariable;
        float moveDownVector = transform.position.y + scrollSpeedVariable;
        float moveRightVector = transform.position.x + scrollSpeedVariable;
        float moveLeftVector = transform.position.x - scrollSpeedVariable;

        if (zoomLevel <= cameraZoomLevel.Province) // don't pan map if in system or planet mode
        {
            if (moveMapUp && (yLocation <= galaxyHeight) && (yLocation >= -galaxyHeight))
            {
                transform.position = new Vector3(transform.position.x, Mathf.Lerp(yLocation, moveUpVector, Time.deltaTime * 3), transform.position.z);
            }

            if (moveMapDown && (yLocation <= galaxyHeight) && (yLocation >= -galaxyHeight))
            {
                transform.position = new Vector3(transform.position.x, Mathf.Lerp(yLocation, moveDownVector, Time.deltaTime * 3), transform.position.z);
            }

            if (moveMapRight && (xLocation <= galaxyWidth) && (xLocation >= -galaxyWidth))
            {
                transform.position = new Vector3(Mathf.Lerp(xLocation, moveRightVector, Time.deltaTime * 3), transform.position.y, transform.position.z);
            }

            if (moveMapLeft && (xLocation <= galaxyWidth) && (xLocation >= -galaxyWidth))
            {
                transform.position = new Vector3(Mathf.Lerp(xLocation, moveLeftVector, Time.deltaTime * 3), transform.position.y, transform.position.z);
            }

            // normalize to prevent scrolling outside of map
            if (transform.position.x > galaxyWidth)
                transform.position = new Vector3(galaxyWidth, transform.position.y, transform.position.z);
            if (transform.position.x < -galaxyWidth)
                transform.position = new Vector3(-galaxyWidth, transform.position.y, transform.position.z);

            if (transform.position.y > galaxyHeight)
                transform.position = new Vector3(transform.position.x, galaxyHeight, transform.position.z);
            if (transform.position.y < -galaxyHeight)
                transform.position = new Vector3(transform.position.x, -galaxyHeight, transform.position.z);

            yield return null;
        }
    }

    void LateUpdate()
    {
        mouseWheelValue = Input.GetAxis("Mouse ScrollWheel");
        mainC.fieldOfView = Mathf.Lerp(mainC.fieldOfView, zoom, Time.deltaTime * zoomSpeed);
    }

    void ZoomToProvince(Province target)
    {
        float provWidth = target.ProvinceBounds.width;
        float provHeight = target.ProvinceBounds.height;
        float constraintBound = 0f;

        // determine which side is larger; that will set the max zoom so that the whole province is seen
        if (provWidth > provHeight)
            constraintBound = provWidth;
        else
            constraintBound = provHeight;

        // zoom to the province
        if (!provinceZoomComplete)
        {
            Vector3 tgtPosition = target.ProvinceCenter;
            zoom = 30 + constraintBound / 120; // test, will need to account for non-linear zoom
            StartCoroutine(ProvinceZoom(tgtPosition));
        }

        provinceZoomComplete = true;

    }

	void ZoomToSystem(Transform target)
	{
        Vector3 tgtPosition;

        int leftCameraMove = 0;

        if (Screen.width >= 1600)
            leftCameraMove = 420;
        else if (Screen.width >= 1280)
            leftCameraMove = 460;
        else
            leftCameraMove = 490;

        if (!systemZoomComplete)
        {
            if (!planetToSystemZoom)
            {
                tgtPosition = new Vector3(target.position.x + (leftCameraMove * (Screen.width / 1920f)) + (target.localScale.x / 2), target.position.y + 75 * (Screen.height / 1080f), transform.position.z);
                zoom = systemMinZoomLevel; // set system view zoom level
                StartCoroutine(SystemZoom(tgtPosition));
            }
            
            else
            {
                tgtPosition = new Vector3(target.position.x + (leftCameraMove * (Screen.width / 1920f)) + (target.localScale.x / 2), target.position.y + 75 * (Screen.height / 1080f), transform.position.z);
                transform.position = new Vector3 (cameraPlanetPosition.x,cameraPlanetPosition.y,cameraPlanetPosition.z);
                zoom = systemMinZoomLevel; // set system view zoom level
                StartCoroutine(SystemZoom(tgtPosition));
            }

            target = null; // reset the target  
            systemZoomComplete = true;
            planetToSystemZoom = false;       
        }   
	}

    void ZoomToPlanet(Transform planet) // need to tweak where the planet shows up on the screen (aiming for lower-left)
    {       
        if (!planetZoomComplete)
        {                
            Vector3 tgtPosition = new Vector3(planet.position.x, planet.position.y - 7f, transform.position.z);
            planet.localScale = new Vector3(11 * scaleRatio, 11 * scaleRatio, 1); // then normalize the size of the planet to show close-up
            zoom = planetMinZoomLevel; // set planet view zoom level
            StartCoroutine(PlanetZoom(tgtPosition));
            cameraPlanetPosition = transform.position;
            planetZoomComplete = true; // set views as true
            planetToSystemZoom = true;
        }
    }

    IEnumerator PlanetZoom(Vector3 tgtPosition)
    {
        while (((int)transform.position.x != (int)tgtPosition.x | (int)transform.position.y != (int)tgtPosition.y) && (planetZoomActive))
        {
            transform.position = new Vector3(Mathf.Lerp(transform.position.x, tgtPosition.x, Time.deltaTime * (zoomSpeed + 5)), Mathf.Lerp(transform.position.y, tgtPosition.y, Time.deltaTime * (zoomSpeed + 5)), tgtPosition.z); // interpolate the movement
            cameraPlanetPosition = transform.position;
            planetToSystemZoom = true;
            yield return null;          
        }
    }
 
    IEnumerator SystemZoom(Vector3 tgtPosition)
    {
        
        while(((int)transform.position.x != (int)tgtPosition.x | (int)transform.position.y != (int)tgtPosition.y) && (systemZoomActive) && (!planetZoomActive) && (!provinceZoomActive))
        {      
            transform.position = new Vector3(Mathf.Lerp(transform.position.x, tgtPosition.x, Time.deltaTime * (zoomSpeed + 3)), Mathf.Lerp(transform.position.y, tgtPosition.y, Time.deltaTime * (zoomSpeed + 3)), transform.position.z); // interpolate the movement
            yield return null;        
        }

        cameraSystemPosition = transform.position; // assign the camera system position
        
    }

    IEnumerator ProvinceZoom(Vector3 tgtPosition)
    {

        while (((int)transform.position.x != (int)tgtPosition.x | (int)transform.position.y != (int)tgtPosition.y) && (!systemZoomActive) && (!planetZoomActive) && (provinceZoomActive))
        {
            transform.position = new Vector3(Mathf.Lerp(transform.position.x, tgtPosition.x, Time.deltaTime * (zoomSpeed + 2)), Mathf.Lerp(transform.position.y, tgtPosition.y, Time.deltaTime * (zoomSpeed + 2)), transform.position.z); // interpolate the movement
            yield return null;
        }

        //cameraSystemPosition = transform.position; // assign the camera system position

    }

    void DetermineZoomLevel()
    {
        if (zoom > galaxyMinZoomLevel)
        {
            zoomLevel = cameraZoomLevel.Galaxy;
            systemZoomActive = false;
            planetZoomActive = false;
        }
        else if ((zoom <= galaxyMinZoomLevel) && (zoom > systemMinZoomLevel))
        {
            zoomLevel = cameraZoomLevel.Province;
            systemZoomActive = false;
            planetZoomActive = false;
        }
        else if ((zoom <= provinceMinZoomLevel) && (zoom > planetMinZoomLevel))
        {
            zoomLevel = cameraZoomLevel.System;
            planetZoomActive = false;
            provinceZoomActive = false;
        }
        else
            zoomLevel = cameraZoomLevel.Planet;
    }
}
}
