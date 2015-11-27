using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using StellarObjects;
using PlanetObjects;
using CharacterObjects;
using HelperFunctions;
using Constants;
using GameEvents;

namespace CivObjects
{
    public class Civilization
    {
        public enum eCivType : int
        {
            PlayerEmpire,
            Confederation,
            Satrapy,
            MinorEmpire,
            RenegadeFaction,
            BrokenCivilization,
            Pirates,
            XylHeralds
        }

        public enum eCivSize : int
        {
            SinglePlanet,
            Local,
            Minor,
            Major
        }

        public bool HumanCiv = false; // is this the human player civilization?
        public string Name { get; set; }
        public Color Color { get; set; }
        public float Treasury { get; set; }
        public eCivType Type { get; set; }
        public string ID { get; set; }
        public string CapitalPlanetID { get; set; }
        public int AdminRating { get; set; }
        public eCivSize Size { get; set; }
        public float Revenues { get; set; }
        public float Expenses { get; set; }
        public float Range { get; set; }
        public Character Leader
        {
            get
            {
                return HelperFunctions.DataRetrivalFunctions.GetCharacter(LeaderID);
            }
        }
        public string LeaderID { get; set; }
        public List<GameEvents.GameEvent> LastTurnEvents = new List<GameEvents.GameEvent>(); // events from the last turn go here
        public int AstronomyRating { get; set; } // used to determine what information about a star can be gleaned from the civ's technology level
        public int PlanetMinTolerance { get; set; }
        public int CivMaxProvinceSize { get; set; } // used to set the maximum size of provinces

        // ratings for generating pop skill ratings from this civilization
        public int MiningBaseRating { get; set; }
        public int FarmingBaseRating { get; set; }
        public int ManufacturingBaseRating { get; set; }
        public int ScienceBaseRating { get; set; }
        public int HighTechBaseRating { get; set; }


        // empire prices for resources
        public float TotalResourcesGenerated
        {
            get
            {
                float totalResources = 0f;

                foreach (PlanetData pData in PlanetList)
                {
                    totalResources += pData.TotalFoodGenerated;
                    totalResources += pData.TotalEnergyGenerated;
                    totalResources += pData.TotalAlphaMaterialsGenerated;
                    totalResources += pData.TotalHeavyMaterialsGenerated;
                    totalResources += pData.TotalRareMaterialsGenerated;
                }

                return totalResources;
            }
        }

        public float CurrentFoodPrice
        {
            get
            {
                float totalFood = 1;             
                float foodPrice = 0;

                foreach (PlanetData pData in PlanetList)
                {
                    if (pData.FoodDifference > 0)
                        totalFood += pData.FoodDifference;
                }

                foodPrice = (TotalResourcesGenerated / totalFood) * Constants.Constants.ResourceBaseCost;
                return foodPrice;
            }
        }
        public float CurrentEnergyPrice
        {
            get
            {
                float totalEnergy = 1;
                float energyPrice = 0;

                foreach (PlanetData pData in PlanetList)
                {
                    if (pData.EnergyDifference > 0)
                        totalEnergy += pData.EnergyDifference;
                }

                energyPrice = (TotalResourcesGenerated / totalEnergy) * Constants.Constants.ResourceBaseCost;
                return energyPrice;
            }
        }

        public float CurrentAlphaPrice
        {
            get
            {
                float totalAlpha = 1;
                float alphaPrice = 0;

                foreach (PlanetData pData in PlanetList)
                {
                    if (pData.AlphaPreProductionDifference > 0)
                        totalAlpha += pData.AlphaPreProductionDifference;
                }

                alphaPrice = (TotalResourcesGenerated / totalAlpha) * Constants.Constants.ResourceBaseCost;
                return alphaPrice;
            }
        }

        public float CurrentHeavyPrice
        {
            get
            {
                float totalHeavy = 1;
                float heavyPrice = 0;

                foreach (PlanetData pData in PlanetList)
                {
                    if (pData.HeavyPreProductionDifference > 0)
                        totalHeavy += pData.HeavyPreProductionDifference;
                }

                heavyPrice = (TotalResourcesGenerated / totalHeavy) * Constants.Constants.ResourceBaseCost;
                return heavyPrice;
            }
        }
        public float CurrentRarePrice
        {
            get
            {
                float totalRare = 1;
                float rarePrice = 0;

                foreach (PlanetData pData in PlanetList)
                {
                    if (pData.RarePreProductionDifference > 0)
                        totalRare += pData.RarePreProductionDifference;
                }

                rarePrice = (TotalResourcesGenerated / totalRare) * Constants.Constants.ResourceBaseCost;
                return rarePrice;
            }
        }

        public List<string> PlanetIDList = new List<string>(); // which planets does the civilization own?
        public List<PlanetData> PlanetList
        {
            get
            {
                List<PlanetData> pList = new List<PlanetData>();
                foreach (string pID in PlanetIDList)
                {
                    if (HelperFunctions.DataRetrivalFunctions.GetPlanet(pID) != null)
                    {
                        pList.Add(HelperFunctions.DataRetrivalFunctions.GetPlanet(pID));
                    }           
                }

                return pList;
            }
        }
        public List<Pops> PopList // dynamic depending on pops in regions (not all pops in all regions are loyal!)
        {
            get
            {
                List<Pops> pList = new List<Pops>();
                foreach (Region rData in TileList)
                {
                    foreach (Pops pop in rData.PopsInTile)
                    {
                        if (pop.EmpireID == ID)
                            pList.Add(pop);
                    }
                }
                return pList;
            }
        }
       
        public List<Region> TileList // which tiles do the civilizations live on?
        {
            get
            {
                List<Region> rList = new List<Region>();
                foreach (PlanetData pData in HelperFunctions.DataRetrivalFunctions.GetCivPlanetList(this))
                {
                    foreach (Region rData in pData.RegionList)
                    {
                        if (rData.OwnerCivID == ID)
                            rList.Add(rData);
                    }
                }
                return rList;
            }
        }
        // Use this for initialization (equiv to New)
        public Civilization()
        {
            
        }

    }
}
