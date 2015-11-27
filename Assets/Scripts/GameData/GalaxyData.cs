using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using StellarObjects;
using PlanetObjects;

public class GalaxyData : MonoBehaviour //contains all of the game's stellar data
{
    // stellar level objects
    public List<GameObject> GalaxyStarList; // master star list (may remove)
    public List<StarData> GalaxyStarDataList; // master star data list

    // organization level objects
    public List<Province> ProvinceList = new List<Province>(); // master province list

    // planet level objects
    public List<GameObject> GalaxyPlanetList; // master planet list (may remove)
    public List<PlanetData> GalaxyPlanetDataList = new List<PlanetData>(); // master planet data list
    public List<Region> GalaxyRegionDataList = new List<Region>(); // master region list
    
    public List<NebulaData> stellarPhenonomaData = new List<NebulaData>();
    public List<GameObject> stellarPhenonomaList = new List<GameObject>();
    public List<PlanetTraits> PlanetTraitDataList = new List<PlanetTraits>(); // planet trait database
    public List<RegionTypeData> RegionDataList = new List<RegionTypeData>();
    public bool galaxyIsGenerated = false; // flag to show completion of galaxy

    public void AddStarObjectToList(GameObject star)
    {
        GalaxyStarList.Add(star);
    }

    public void AddProvinceToList(Province prov)
    {
        ProvinceList.Add(prov);
    }

    public void PopulatePlanetTraitList(List<PlanetTraits> pDataList)
    {
        PlanetTraitDataList = pDataList;
    }

    public void PopulateRegionDataList(List<RegionTypeData> rDataList)
    {
        RegionDataList = rDataList;
    }

    public void AddStarDataToList(StarData sData)
    {
        GalaxyStarDataList.Add(sData);
    }

    public void AddPlanetDataToList(PlanetData pData)
    {
        GalaxyPlanetDataList.Add(pData);
    }

    public void AddPlanetObjectToList(GameObject planet)
    {
        GalaxyPlanetList.Add(planet);
    }

    public void AddTileToList(Region tile)
    {
        GalaxyRegionDataList.Add(tile);
    }

    public void AddStellarPhenonomaObjectToList(GameObject nebula)
    {
        stellarPhenonomaList.Add(nebula);
    }

    public void AddStellarPhenonomaDataToList(NebulaData nebula)
    {
        stellarPhenonomaData.Add(nebula);
    }

    void Start()
    {
        GalaxyStarList = new List<GameObject>();
        GalaxyStarDataList = new List<StarData>();
        GalaxyPlanetDataList = new List<PlanetData>();
    }

}
