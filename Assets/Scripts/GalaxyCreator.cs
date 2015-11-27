using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// object namespaces
using StellarObjects;
using PlanetObjects;
using CivObjects;

// classes
using CameraScripts;
using Assets.Scripts.States;

namespace GalaxyCreator
{
    public class GalaxyCreator : MonoBehaviour
    {      
        private GalaxyData gData;
        private GlobalGameData gameDataRef;       
        private int galaxySizeWidth;
        private int galaxySizeHeight;
        private const int SpaceBetweenStars = 80; // the minimum space between star objects

        void Awake()
        {
            //gData = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            //gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
        }

        void Start()
        {
            // data structure references
            gData = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
            //DataManager.PopulateObjectNameLists(); // populate the name lists once!
            galaxySizeWidth = gameDataRef.GalaxySizeWidth; // these will be selected in new game screen
            galaxySizeHeight = gameDataRef.GalaxySizeHeight;
            GenerateNebulas();
            GenerateStars();
            GeneratePlanets();
        }

        void GenerateStars()
        {
            GenerateHumanHomeStar(); // generate the human home system

            int starCount = gameDataRef.TotalSystems;
            for (int i = 0; i < starCount; i++)
            {
                Vector3 starLoc = DetermineStarLocation();
               
                StarData newStar = new StarData();

                newStar = GenerateGameObject.CreateNewStar(); // this creates the star and adds the data/accessor components
                // if the star type is 'no star', break this loop, and if it is too high (no specials or wolf rayet yet) no star either (yet)               
                if ((int)newStar.SpectralClass >= 12)
                    newStar.SpectralClass = StarData.eSpectralClass.NoStar;

                if (newStar.SpectralClass == 0)
                {
                    continue;
                }
                newStar.SetWorldLocation(starLoc);
                gData.AddStarDataToList(newStar);
            }

            

            gData.galaxyIsGenerated = true; // add more checks here
        }

        Vector3 DetermineStarLocation()
        {
            bool locIsValid = true; // flag to show whether location is valid
            int placementTries = 0; // after 5 tries, place and move on
            Vector3 proposedLocation = new Vector3();

            if (gData.GalaxyStarDataList.Count == 0) // if nothing in the list, obviously any location will work! 
                proposedLocation = GenerateLocation();           
                
            else
            {
            restart:
                placementTries += 1;
                proposedLocation = GenerateLocation(); // generate a new tentative location

                foreach (StarData starLoc in gData.GalaxyStarDataList)
                {                  
                    locIsValid = CheckLocation(starLoc.WorldLocation, proposedLocation);

                    if (!locIsValid && placementTries < 20) // if an overlap is found      
                        goto restart; // restarts the loop from the beginning once a valid location is generated
                    else if (placementTries == 20)
                    {
                        Debug.Log("Error; could not find suitable location for star within parameters. Placing at last generated location.");
                        return proposedLocation;
                    }
                }
            }
           
            return proposedLocation;
        }

        Vector3 GenerateLocation()
        {
            Vector3 pLoc;
            pLoc.x = UnityEngine.Random.Range(-galaxySizeWidth, galaxySizeWidth);
            pLoc.y = UnityEngine.Random.Range(-galaxySizeHeight, galaxySizeHeight);
            pLoc.z = 0;

            return pLoc;
        }

        bool CheckLocation(Vector2 sLoc, Vector2 pLoc)
        {
            if (pLoc.x < sLoc.x + SpaceBetweenStars && pLoc.x > sLoc.x - SpaceBetweenStars) // x too close? return false
                return false;
            if (pLoc.y < sLoc.y + SpaceBetweenStars && pLoc.y > sLoc.y - SpaceBetweenStars) // y too close? return true
                return false;

            return true; // true if all checks pass
        }
            

        void GenerateHumanHomeStar()
        {
            // create New Terra
            Vector3 starLoc = new Vector3(0, 0, 0);
            StarData newStar = new StarData();

            newStar.SpectralClass = StarData.eSpectralClass.G;
            newStar.Age = 7;
            newStar.starMultipleType = StarData.eStarMultiple.Single;
            newStar.Size = 8;
            newStar.Metallicity = 8.0;
            newStar.Name = "Neo-Sirius";
            newStar.ID = "STANEOS001";

            newStar.SetWorldLocation(starLoc);
            gData.AddStarDataToList(newStar);
        }

        void GenerateHouses()
        {
            // load the house data
            //DataManager.ReadHouseXMLFiles();
        }

        void GeneratePlanets()
        {
            // populate the planet tables
            GenerateGameObject.PopulatePlanetGenerationTables();
            GenerateGameObject.PopulateRegionTypeTables();

            // populate the planet trait and region data lists
            gData.PopulatePlanetTraitList(DataManager.planetTraitDataList);  // move the static version to a public version? may not need this
            gData.PopulateRegionDataList(GenerateGameObject.regionTypeDataList);

            // run the planet generation routines for each star
            foreach (StarData star in gData.GalaxyStarDataList)
            {
                GenerateGameObject.CreateSystemPlanets(star);
                //foreach (PlanetData planet in star.PlanetList)
                //    gData.AddPlanetDataToList(planet);
            }
        }

        void GenerateNebulas()
        {
            int nebulaCount = Random.Range(0, 3);
            for (int x = 0; x < nebulaCount; x++)
            {
                GenerateGameObject.GenerateNebula();
            }
        }

        void AddPlanetDataToGalaxyManager()
        {
            foreach (StarData star in gData.GalaxyStarDataList)
            {
                foreach (PlanetData planet in star.PlanetList)
                    gData.AddPlanetDataToList(planet);
            }
        }
    }
}
