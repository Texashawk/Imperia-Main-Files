using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using StellarObjects;
using PlanetObjects;
using CharacterObjects;
using CivObjects;
using HelperFunctions;

public class Province
{
    public string Name { get; set; }
    public string ID { get; set; }
    public string OwningCivID { get; set; }
    public string CapitalPlanetID { get; set; }
    public Rect ProvinceBounds { get; set; }

    public Civilization OwningCiv
    {
        get
        {
            return HelperFunctions.DataRetrivalFunctions.GetCivilization(OwningCivID);
        }
    }

    public Character Governor
    {
        get
        {
            GlobalGameData gData = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
            if (gData.CharacterList.Exists(p => p.ProvinceAssignedID == ID))
            {
                return gData.CharacterList.Find(p => p.ProvinceAssignedID == ID);
            }
            else
                return null;
        }
    }

    public List<StarData> SystemList // dynamic system list that make up provinces
    {
        get
        {
            List<StarData> pList = new List<StarData>();
            GalaxyData gData = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            
            pList = gData.GalaxyStarDataList.FindAll(p => p.AssignedProvinceID == ID);
              
            return pList;
        }
    }

    public List<PlanetData> PlanetList // dynamic planet list that make up provinces
    {
        get
        {
            List<PlanetData> pList = new List<PlanetData>();
            GalaxyData gData = GameObject.Find("GameManager").GetComponent<GalaxyData>();

            pList = gData.GalaxyPlanetDataList.FindAll(p => p.System.AssignedProvinceID == ID);

            return pList;
        }
    }
    public float BaseProvinceValue
    {
        get
        {
            float pValue = 0f;
            foreach (StarData sData in SystemList)
            {
                pValue += sData.BaseSystemValue;
            }

            return pValue;
        }
    }
    public Vector2 ProvinceCenter
    {
        get
        {
            float top = 0;
            float bottom = 0;
            float left = 0;
            float right = 0;
            Rect centRect;

            // initialize the first values
            top = SystemList[0].WorldLocation.y;
            bottom = SystemList[0].WorldLocation.y;
            left = SystemList[0].WorldLocation.x;
            right = SystemList[0].WorldLocation.x;

            // then loop through the rest
            foreach (StarData sData in SystemList)
            {
                if (sData.WorldLocation.x < left)
                {
                    left = sData.WorldLocation.x;
                }
                if (sData.WorldLocation.x > right)
                {
                    right = sData.WorldLocation.x;
                }
                if (sData.WorldLocation.y < top)
                {
                    top = sData.WorldLocation.y;
                }
                if (sData.WorldLocation.y > bottom)
                {
                    bottom = sData.WorldLocation.y;
                }
            }
            if (SystemList.Count > 1)
            {
                centRect = new Rect(left, top, Mathf.Abs(right - left), Mathf.Abs(top - bottom));
                ProvinceBounds = centRect; // assign the rectangle to the bounds assignor
                return centRect.center;
            }
            else
            {
                ProvinceBounds = new Rect(100, 100, 100, 100); // generic size for one system provinces (for zoom)
                return new Vector2(SystemList[0].WorldLocation.x, SystemList[0].WorldLocation.y - 50); // offset the position a bit higher so that it doesn't obscure the system name     
            }
        }
    }

    public List<string> SystemIDs = new List<string>();
    
}
