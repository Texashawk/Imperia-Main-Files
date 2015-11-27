using UnityEngine;
using System.Collections.Generic;
using StellarObjects;

public class DisplayGalaxyStellarObjects : MonoBehaviour {

    private GalaxyData gData;
    public List<GameObject> starList = new List<GameObject>(); // add the star prefabs here
    public List<GameObject> stellarList = new List<GameObject>(); // add the star prefabs here

	// Use this for initialization
	void Start () 
    {
        gData = GameObject.Find("GameManager").GetComponent<GalaxyData>();
        Debug.Log("Drawing stars...");
        DrawStars(); // draw the star map
        DrawNebulas();
	}

    void DrawStars()
    {
        var i = 0;
        var x = 0;
        foreach (StarData star in gData.GalaxyStarDataList)
        {
            i += 1;
            x = (int)star.SpectralClass - 1; // convert spectral class to lists

            GameObject writtenStar;
            writtenStar = Instantiate(starList[x], star.WorldLocation, Quaternion.identity) as GameObject;

            // draw secondary star if duplex star
            if (star.starMultipleType == StarData.eStarMultiple.Binary)
            {
                var y = 0;
                Vector3 secondLoc = new Vector3(star.WorldLocation.x - 20, star.WorldLocation.y - 20, star.WorldLocation.z);
                y = (int)star.compSpectralClass - 1; // convert spectral class to list
                GameObject secondaryStar = Instantiate(starList[y], secondLoc, Quaternion.identity) as GameObject;
                secondaryStar.AddComponent<Star>();
                secondaryStar.GetComponent<Star>().starData.compSecondarySpectralClass = star.SecondarySpectralClass;
                secondaryStar.GetComponent<Star>().starData.compSpectralClass = star.compSpectralClass;
                secondaryStar.GetComponent<Star>().starData.Size = 5; // placeholder
                secondaryStar.name = star.Name + " B";
                secondaryStar.GetComponent<CircleCollider2D>().enabled = false; // do not enable the second collider since it is just visual!
                secondaryStar.tag = "Companion Star";
                secondaryStar.transform.SetParent(writtenStar.transform); // set as parent of star
                secondaryStar.transform.localPosition = new Vector3(5, -5, 0); // offset by parent
                secondaryStar.transform.localScale = new Vector2(.5f, .5f);
                gData.AddStarObjectToList(secondaryStar);
            }

            writtenStar.AddComponent<Star>();
            writtenStar.GetComponent<Star>().starData = star; // assign the star data
            writtenStar.name = writtenStar.GetComponent<Star>().starData.Name;
            writtenStar.GetComponent<Star>().starData.SetWorldLocation(star.WorldLocation);
            gData.AddStarObjectToList(writtenStar);
        }

    }

    void DrawNebulas()
    {
        var i = 0;
        var x = 0;
        float scale = 0f;

        foreach (NebulaData nebula in gData.stellarPhenonomaData)
        {
            i += 1;
            x = (int)nebula.NebulaSpriteNumber; // convert spectral class to lists

            GameObject writtenNebula; //Instantiate(starList[x], star.worldLocation, Quaternion.identity) as GameObject;
            writtenNebula = Instantiate(stellarList[x], nebula.WorldLocation, Quaternion.identity) as GameObject;
            writtenNebula.AddComponent<Nebula>();
            writtenNebula.GetComponent<Nebula>().nebulaData = nebula; // assign the star data
            writtenNebula.name = writtenNebula.GetComponent<Nebula>().nebulaData.Name;
            scale = writtenNebula.GetComponent<Nebula>().nebulaData.NebulaSize;
            writtenNebula.transform.localScale = new Vector3(writtenNebula.transform.localScale.x * scale, writtenNebula.transform.localScale.y * scale, writtenNebula.transform.localScale.z);
            gData.AddStellarPhenonomaObjectToList(writtenNebula);
            //writtenStar = star;
            Debug.Log("The " + i.ToString("N0") + " nebula is " + writtenNebula.GetComponent<Nebula>().nebulaData.Name + ". Location is: " + writtenNebula.GetComponent<Nebula>().nebulaData.WorldLocation);
        }

    }
}

