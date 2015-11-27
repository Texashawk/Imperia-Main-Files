using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using StellarObjects;
using PlanetObjects;
using CharacterObjects;
using CivObjects;

namespace HelperFunctions
{
    public static class StringConversions
    {

        public static string ConvertToRomanNumeral(int Number)
        {
            switch (Number)
            {
                case 1:
                    return "I";
                case 2:
                    return "II";
                case 3:
                    return "III";
                case 4:
                    return "IV";
                case 5:
                    return "V";
                case 6:
                    return "VI";
                case 7:
                    return "VII";
                case 8:
                    return "VIII";
                case 9:
                    return "IX";
                case 10:
                    return "X";
                default:
                    return "0";
            }
        }

        public static string ConvertValueToDescription(int number)
        {
            if (number == 0)
                return "impossible";
            else if (number < 15)
                return "abysmal";
            else if (number < 30)
                return "very low";
            else if (number < 40)
                return "low";
            else if (number < 55)
                return "medium";
            else if (number < 70)
                return "high";
            else if (number < 85)
                return "very high";
            else if (number < 101)
                return "fantastic";
            else
                return "extraordinary";
        }

        public static string ConvertCharacterValueToDescription(int number, int intelLevel)
        {
            if (intelLevel < Constants.Constants.LowIntelLevelMax)
            {
                if (number < 50)
                    return "low";
                else 
                    return "high";
            }
            else if (intelLevel < Constants.Constants.MediumIntelLevelMax)
            {
                if (number < 40)
                    return "low";
                else if (number < 70)
                    return "average";
                else
                    return "high";
            }
            else if (intelLevel < Constants.Constants.HighIntelMax)
            {
                if (number < 20)
                    return "poor";
                else if (number < 40)
                    return "low";
                else if (number < 60)
                    return "average";
                else if (number < 80)
                    return "high";
                else
                    return "very high";
            }
            else
            {
                if (number == 0)
                    return "none";
                else if (number < 15)
                    return "abysmal";
                else if (number < 30)
                    return "very low";
                else if (number < 40)
                    return "low";
                else if (number < 55)
                    return "medium";
                else if (number < 70)
                    return "high";
                else if (number < 85)
                    return "very high";
                else
                    return "exceptional";
            }
        }

        public static string ConvertPlanetEnum(PlanetData.ePlanetType type)
        {
            switch (type)
            {
                case PlanetData.ePlanetType.AsteroidBelt:
                    return "Asteroid Belt";
                case PlanetData.ePlanetType.Barren:
                    return "Barren Planet";
                case PlanetData.ePlanetType.BrownDwarf:
                    return "Brown Dwarf";
                case PlanetData.ePlanetType.City:
                    return "City Planet";
                case PlanetData.ePlanetType.Desert:
                    return "Desert Planet";
                case PlanetData.ePlanetType.DustRing:
                    return "Dust Ring";
                case PlanetData.ePlanetType.GasGiant:
                    return "Gas Giant";
                case PlanetData.ePlanetType.Greenhouse:
                    return "Greenhouse Planet";
                case PlanetData.ePlanetType.Ice:
                    return "Ice Planet";
                case PlanetData.ePlanetType.IceBelt:
                    return "Ice Belt";
                case PlanetData.ePlanetType.IceGiant:
                    return "Ice Giant";
                case PlanetData.ePlanetType.Irradiated:
                    return "Irradiated Planet";
                case PlanetData.ePlanetType.Lava:
                    return "Lava Planet";
                case PlanetData.ePlanetType.Ocean:
                    return "Ocean Planet";
                case PlanetData.ePlanetType.Organic:
                    return "Organic Planet";
                case PlanetData.ePlanetType.SuperEarth:
                    return "Super Earth Planet";
                case PlanetData.ePlanetType.Terran:
                    return "Terran Planet";
                default:
                    return "Unknown Planet";
            }
        }

        public static string ConvertRoleEnum(Character.eRole role)
        {
            switch (role)
            {
                case Character.eRole.DomesticPrime:
                    return "Domestic Prime";
                case Character.eRole.ProvinceGovernor:
                    return "Provincial Governor";
                case Character.eRole.SystemGovernor:
                    return "System Governor";
                case Character.eRole.FinancePrime:
                    return "Financial Prime";
                case Character.eRole.Viceroy:
                    return "Planetary Viceroy";
                default:
                    return "Unknown Role";
            }
        }

        public static string ConvertIntDollarToText(int num)
        {
            string numbStr = "";

            if ((float)(num / 1000f) <= 1)
                numbStr = "$" + num.ToString("N0") + "B";
            else if ((float)(num / 1000000f) <= 1)
                numbStr = "$" + ((float)(num / 1000f)).ToString("N2") + "T";
            else if ((float)(num / 1000000000f) <= 1)
                numbStr = "$" + ((float)(num / 1000000f)).ToString("N2") + "QD";

            return numbStr;
        }

        public static string ConvertFloatDollarToText(float num)
        {
            string numbStr = "";

            if ((float)(num / 1000f) <= 1)
                numbStr = "$" + num.ToString("N0") + "B";
            else if ((float)(num / 1000000f) <= 1)
                numbStr = "$" + ((float)(num / 1000f)).ToString("N2") + "T";
            else if ((float)(num / 1000000000f) <= 1)
                numbStr = "$" + ((float)(num / 1000000f)).ToString("N2") + "QD";

            return numbStr;
        }

        public static string ConvertIntToText(int num)
        {
            string numbStr = "";

            if ((float)(num / 1000f) <= 1)
                numbStr = num.ToString("N0") + " M";
            else if ((float)(num / 1000000f) <= 1)
                numbStr = ((float)(num / 1000f)).ToString("N2") + " B";
            else if ((float)(num / 1000000000f) <= 1)
                numbStr = ((float)(num / 1000000f)).ToString("N2") + " T";

            return numbStr;
        }

        public static string ConvertPlanetRankEnum(PlanetData.ePlanetRank rank)
        {
            switch (rank)
            {
                case PlanetData.ePlanetRank.EstablishedColony:
                    return "Established Colony";
                case PlanetData.ePlanetRank.ImperialCapital:
                    return "Civilization Capital";
                case PlanetData.ePlanetRank.NewColony:
                    return "New Colony";
                case PlanetData.ePlanetRank.ProvinceCapital:
                    return "Provincial Capital";
                case PlanetData.ePlanetRank.SystemCapital:
                    return "System Capital";
                case PlanetData.ePlanetRank.Uninhabited:
                    return "Uninhabited";
                default:
                    return "Unknown Rank";
            }
        }

        public static Color GetTextValueColor(int number)
        {
            if (number < 15)
                return new Color(.48f,.1f,.1f);
            else if (number < 30)
                return Color.red;
            else if (number < 40)
                return new Color(1,.6f,0);
            else if (number < 55)
                return Color.yellow;
            else if (number < 70)
                return Color.cyan;
            else if (number < 85)
                return Color.green;
            else
                return Color.white;
        }       
    }

    public static class Formulas
    {
        public static float MeasureDistanceBetweenSystems(StarData star1, StarData star2)
        {
            float distance = 0f;
            float a = 0f;
            float b = 0f;
            float h = 0f;

            a = Mathf.Abs(star1.WorldLocation.x - star2.WorldLocation.x); // x distance
            b = Mathf.Abs(star1.WorldLocation.y - star2.WorldLocation.y); // y distance
            h = Mathf.Pow(a, 2f) + Mathf.Pow(b, 2f);
            distance = Mathf.Sqrt(h);

            return distance;

        }
    }

    public static class DataRetrivalFunctions
    {
        public static Color FindPlanetOwnerColor(PlanetData pData)
        {
            GlobalGameData gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
            Color pColor = Color.white; // base unowned color

            foreach (Civilization civ in gameDataRef.CivList)
            {
                for (int x = 0; x < civ.PlanetIDList.Count; x++)
                {
                    if (civ.PlanetIDList[x] == pData.ID) // will need to change
                    {
                        pColor = civ.Color;
                        return pColor;
                    }
                }
            }

            return pColor;
        }

        public static Color FindProvinceOwnerColor(Province pData)
        {
            GlobalGameData gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
            GalaxyData galDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            Color pColor = Color.white; // base unowned color

            foreach (Civilization civ in gameDataRef.CivList)
            {           
                if (pData.OwningCivID == civ.ID)
                {
                    pColor = civ.Color;
                    return pColor;
                }               
            }

            return pColor;
        }

        public static Character GetCharacter(string cID)
        {
            GlobalGameData gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
            Character cData = new Character();

            cData = gameDataRef.CharacterList.Find(p => p.ID == cID);
            return cData;
        }

        public static string GetPrime(Character.eRole charRole)
        {
            GlobalGameData gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();

            foreach (Character charData in gameDataRef.CharacterList)
            {
                if (charData.Role == charRole)
                {
                    return charData.ID;
                }
            }

            return "none"; 
        }

        public static House GetHouse(string hID)
        {
            //DataManager dManager = GameObject.Find("GameManager").GetComponent<DataManager>();
            GlobalGameData gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();

            return gameDataRef.HouseList.Find(p => p.ID == hID);
        }

        public static string GetProvinceGovernorID(string sID)
        {
            GlobalGameData gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();

            foreach (Character charData in gameDataRef.CharacterList)
            {
                if (charData.Role == Character.eRole.ProvinceGovernor && charData.ProvinceAssignedID == sID)
                {
                    return charData.ID;
                }
            }

            return "none"; 
        }

        public static Character GetCivLeader(string cID)
        {
            GlobalGameData gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();

            foreach (Character charData in gameDataRef.CharacterList)
            {
                if (charData.Role == Character.eRole.Emperor && charData.CivID == cID)
                {
                    return charData;
                }
            }

            return null;
        }

        public static List<Pops> GetCivPopList(string cID)
        {          
            GlobalGameData gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
            Civilization civ = gameDataRef.CivList.Find(p => p.ID == cID);
            return civ.PopList;           
        }

        public static string GetSystemGovernorID(string sID)
        {
            GlobalGameData gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
         
            foreach (Character charData in gameDataRef.CharacterList)
            {
                if (charData.Role == Character.eRole.SystemGovernor && charData.SystemAssignedID == sID)
                {         
                    return charData.ID;
                }
            }

            return "none"; // returns the first character if can't find the governor, thus error         
        }

        public static string GetPlanetViceroyID(string pID)
        {
            GlobalGameData gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();

            foreach (Character charData in gameDataRef.CharacterList)
            {
                if (charData.Role == Character.eRole.Viceroy && charData.PlanetAssignedID == pID)
                {
                    return charData.ID;
                }
            }

            return "none"; ; // returns the first character if can't find the governor, thus error         
        }

        public static String FindOwnerName(PlanetData pData)
        {
            string oName; // base unowned name
            GlobalGameData gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();

            foreach (Civilization civ in gameDataRef.CivList)
            {
                for (int x = 0; x < civ.PlanetIDList.Count; x++)
                {
                    if (civ.PlanetIDList[x] == pData.ID) // will need to change
                    {
                        oName = civ.Name;
                        return oName.ToUpper();
                    }
                }
            }

            return "UNKNOWN DESIGNATION";
        }

        public static List<StarData> GetCivSystemList(Civilization civ)
        {
            GlobalGameData gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
            GalaxyData galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            List<StarData> civStarList = new List<StarData>();

            foreach (PlanetData planet in galaxyDataRef.GalaxyPlanetDataList)
            {
                if (civ.PlanetIDList.Exists(p => p == planet.ID))
                {
                    StarData star = galaxyDataRef.GalaxyStarDataList.Find(p => p.ID == planet.SystemID);

                    if (!civStarList.Contains(star))
                        civStarList.Add(star);
                }
            }

            return civStarList;
        }

        public static GlobalGameData GetGameDataObject()
        {
            GlobalGameData gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
            return gameDataRef;
        }

        public static Civilization GetCivilization(string ID)
        {
            GlobalGameData gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
            if (gameDataRef.CivList.Exists(p => p.ID == ID))
                return gameDataRef.CivList.Find(p => p.ID == ID);
            else
                return null;
        }

        public static List<PlanetData> GetCivPlanetList(Civilization civ)
        {
            GlobalGameData gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
            GalaxyData galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            List<PlanetData> civPlanetList = new List<PlanetData>();

            foreach (PlanetData planet in galaxyDataRef.GalaxyPlanetDataList)
            {
                if (civ.PlanetIDList.Exists(p => p == planet.ID))
                {                 
                    civPlanetList.Add(planet);
                }
            }

            return civPlanetList;
        }

        public static List<Province> GetCivProvinceList(Civilization civ)
        {
            GlobalGameData gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
            GalaxyData galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            List<Province> civProvinceList = new List<Province>();

            foreach (Province prov in galaxyDataRef.ProvinceList)
            {
                if (prov.OwningCivID == civ.ID)
                {
                    civProvinceList.Add(prov);
                }
            }

            return civProvinceList;
        }

        public static StarData GetCivHomeSystem(Civilization civ)
        {
            GlobalGameData gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
            GalaxyData galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            PlanetData pData = new PlanetData();
            StarData sData = new StarData();

            pData = galaxyDataRef.GalaxyPlanetDataList.Find(p => p.ID == civ.CapitalPlanetID);
            sData = galaxyDataRef.GalaxyStarDataList.Find(p => p.ID == pData.SystemID);

            return sData;
        }

        public static List<StarData> GetCivSystems(Civilization civ)
        {
            GalaxyData galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            PlanetData pData = new PlanetData();
            List<PlanetData> pDataList = new List<PlanetData>();
            List<StarData> sDataList = new List<StarData>();

            foreach (string ID in civ.PlanetIDList)
            {
                pData = galaxyDataRef.GalaxyPlanetDataList.Find(p => p.ID == ID);
                if (!sDataList.Exists(p => p.ID == pData.SystemID))
                    sDataList.Add(galaxyDataRef.GalaxyStarDataList.Find(p => p.ID == pData.SystemID));
            }
            
            return sDataList;
        }

        public static List<StarData> GetProvinceSystems(string ID)
        {
            GalaxyData galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            StarData sData = new StarData();
            List<StarData> sDataList = new List<StarData>();

            sDataList = galaxyDataRef.GalaxyStarDataList.FindAll(p => p.AssignedProvinceID == ID);
            return sDataList;
        }

        public static PlanetData GetPlanet(string ID)
        {
            GalaxyData galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            PlanetData pData = new PlanetData();

            pData = galaxyDataRef.GalaxyPlanetDataList.Find(p => p.ID == ID);
            return pData;
        }

        public static StarData GetSystem(string ID)
        {
            GalaxyData galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            StarData sData = new StarData();

            sData = galaxyDataRef.GalaxyStarDataList.Find(p => p.ID == ID);
            return sData;
        }

        public static Province GetProvince(string ID)
        {
            GalaxyData galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            Province prov = new Province();

            prov = galaxyDataRef.ProvinceList.Find(p => p.ID == ID);
            return prov;
        }

        public static Region GetRegion(string ID)
        {
            GalaxyData galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
            Region rData = new Region();

            rData = galaxyDataRef.GalaxyRegionDataList.Find(p => p.ID == ID);
            return rData;
        }
    }

    
}

