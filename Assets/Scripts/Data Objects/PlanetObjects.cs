using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using StellarObjects;
using HelperFunctions;
using System.IO;
using Constants;

namespace PlanetObjects
{
    public class Region
    {
        public enum eRegionDevelopmentLevel : int
        {
            Uninhabited,
            Islands,
            Floating_Stations,
            Tiny_Outposts,
            Small_Settlements,
            Small_Villages,
            Small_Towns,
            Large_Enclaves,
            Scattered_Cities,
            Moderately_Urban,
            Large_Cities,
            Super_Cities,
            Megalopolis          
        }
        // assigned vars
        public string ID { get; set; }
        public bool IsHabitable = false;
        public RegionTypeData RegionType = new RegionTypeData();
        public eRegionDevelopmentLevel RegionDevelopmentLevel
        {
            get
            {
                // assign urban level
                if (RegionType.Type == RegionTypeData.eRegionType.Ocean)
                {
                    return eRegionDevelopmentLevel.Islands;
                }

                if (RegionType.Type == RegionTypeData.eRegionType.Helium_Island)
                {
                    return eRegionDevelopmentLevel.Floating_Stations;
                }

                if (HabitatationInfrastructureLevel == 0)
                    return eRegionDevelopmentLevel.Uninhabited;
                if (HabitatationInfrastructureLevel < 5)
                    return Region.eRegionDevelopmentLevel.Tiny_Outposts;
                else if (HabitatationInfrastructureLevel < 15)
                    return Region.eRegionDevelopmentLevel.Small_Settlements;
                else if (HabitatationInfrastructureLevel < 25)
                    return Region.eRegionDevelopmentLevel.Small_Villages;
                else if (HabitatationInfrastructureLevel < 35)
                    return Region.eRegionDevelopmentLevel.Small_Towns;
                else if (HabitatationInfrastructureLevel < 43)
                    return Region.eRegionDevelopmentLevel.Large_Enclaves;
                else if (HabitatationInfrastructureLevel < 50)
                    return Region.eRegionDevelopmentLevel.Scattered_Cities;
                else if (HabitatationInfrastructureLevel < 60)
                    return Region.eRegionDevelopmentLevel.Moderately_Urban;
                else if (HabitatationInfrastructureLevel < 70)
                    return Region.eRegionDevelopmentLevel.Large_Cities;
                else if (HabitatationInfrastructureLevel < 85)
                    return Region.eRegionDevelopmentLevel.Super_Cities;
                else
                    return Region.eRegionDevelopmentLevel.Megalopolis;
            }
        }
        public int HabitatationInfrastructureLevel { get; set; }
        public string PlanetLocationID { get; set; }

        // variance between turns
        public int DiedLastTurn { get; set; }
        public int GraduatedLastTurn { get; set; }

        // migration between turns
        public int ImmigratedLastTurn { get; set; }
        public int EmigratedLastTurn { get; set; }

        // production stats
        public float TotalAlphaBPsAllocatedInfra { get; set;}
        public float TotalHeavyBPsAllocatedInfra { get; set; }
        public float TotalRareBPsAllocatedInfra { get; set; }

        public bool IsTargetedRegionForBuild
        {
            get
            {
                if (PlanetLocated.BuildPlan.TargetRegion == this)
                    return true;
                else
                    return false;
            }
        }

        public PlanetData PlanetLocated
        {
            get
            {
                return HelperFunctions.DataRetrivalFunctions.GetPlanet(PlanetLocationID);
            }
        }

        public string OwnerCivID
        {
            get
            {
                string cID = "";
                int civNumber = 0;
                int maxPower = 0;
                int[] powerGrid = new int[HelperFunctions.DataRetrivalFunctions.GetGameDataObject().CivList.Count]; // set up a 2 dim array with the total civs on one side and the pop power on the other

                foreach (Pops pop in PopsInTile)
                {
                    civNumber = int.Parse(pop.EmpireID.Substring(3));
                    powerGrid[civNumber] += 1;
                }

                for (int i = 0; i < powerGrid.Length; i++ )
                {
                    if (powerGrid[i] > maxPower)
                    {
                        civNumber = i;
                        maxPower = powerGrid[i];
                    }
                }

                cID = HelperFunctions.DataRetrivalFunctions.GetGameDataObject().CivList[civNumber].ID;
                return cID;
            }
        }// owning civilization; generate dynamically depending on pop power in a given region
        
        // public properties that require validation
        private int maxLevel = 0;
        public int MaxDevelopmentLevel  // max development level for region
        {
            get
            {
                return maxLevel;
            }

            set
            {
                maxLevel = Mathf.Clamp(value, 0, 100);
            }
        }
        private float maxSafePopLevel = 0;
        public int MaxSafePopulationLevel  // max development level for region
        {
            get              
            {
                float regionDevelopmentModifier = 1.0f;

                switch (RegionDevelopmentLevel)
                {
                    case eRegionDevelopmentLevel.Uninhabited:
                        {
                            regionDevelopmentModifier = 0f;
                            break;
                        }
                    case eRegionDevelopmentLevel.Islands:
                        {
                            regionDevelopmentModifier = .3f;
                            break;
                        }
                    case eRegionDevelopmentLevel.Floating_Stations:
                    {
                        regionDevelopmentModifier = .35f;
                        break;
                    }
                    case eRegionDevelopmentLevel.Small_Settlements:
                        {
                            regionDevelopmentModifier = .5f;
                            break;
                        }
                    case eRegionDevelopmentLevel.Small_Villages:
                        {
                            regionDevelopmentModifier = .65f;
                            break;
                        }
                    case eRegionDevelopmentLevel.Small_Towns:
                        {
                            regionDevelopmentModifier = .8f;
                            break;
                        }
                    case eRegionDevelopmentLevel.Scattered_Cities:
                        {
                            regionDevelopmentModifier = 1.0f;
                            break;
                        }
                    case eRegionDevelopmentLevel.Large_Cities:
                        {
                            regionDevelopmentModifier = 1.5f;
                            break;
                        }
                    case eRegionDevelopmentLevel.Megalopolis:
                        {
                            regionDevelopmentModifier = 2.0f;
                            break;
                        }
                    default:
                        {
                            regionDevelopmentModifier = 1f;
                            break;
                        }
                }
                maxSafePopLevel = (float)HabitatationInfrastructureLevel * .6f * regionDevelopmentModifier;
                return (int)maxSafePopLevel;
            }
        }
        private int curLevel = 0;
        public int CurLevel // current development level for region
        {
            get
            {
                return curLevel;
            }
            set
            {
                curLevel = Mathf.Clamp(value, 0, 100);
            }
        }
        private float DevelopmentLevel
        {
            get
            {
                float farmingDevelopmentLevel = FarmingLevel * Constants.Constants.FarmingDevelopmentModifier;
                float miningDevelopmentLevel = MiningLevel * Constants.Constants.MiningDevelopmentModifier;
                float highTechDevelopmentLevel = HighTechLevel * Constants.Constants.HighTechDevelopmentModifier;
                float scienceDevelopmentLevel = ScienceLevel * Constants.Constants.ScienceDevelopmentModifier;
                float manufacturingDevelopmentLevel = ManufacturingLevel * Constants.Constants.ManufacturingDevelopmentModifier;
                float governmentDevelopmentLevel = GovernmentLevel * Constants.Constants.GovernmentDevelopmentModifier;

                return (farmingDevelopmentLevel + miningDevelopmentLevel + highTechDevelopmentLevel + scienceDevelopmentLevel + manufacturingDevelopmentLevel + governmentDevelopmentLevel);
            }
        }

        

        private int energyRating = 0;
        public int EnergyRating
        {
            get
            { 
                return energyRating;
            }
            set
            {
                energyRating = Mathf.Clamp(value, 0, 100);
            }
        }

        private int alphaRating = 0;
        public int AlphaRating
        {
            get
            {
                return alphaRating;
            }
            set
            {
                alphaRating = Mathf.Clamp(value, 0, 100);
            }
        }

        private int heavyRating = 0;
        public int HeavyRating
        {
            get
            { 
                return heavyRating;
            }
            set
            {
                heavyRating = Mathf.Clamp(value, 0, 100);
            }
        }

        private int rareRating = 0;
        public int RareRating
        {
            get
            {
                return rareRating;
            }
            set
            {
                rareRating = Mathf.Clamp(value, 0, 100);
            }
        }

        private int bioRating = 0;
        public int BioRating
        {
            get
            {
                return bioRating;
            }

            set
            {
                
                bioRating = Mathf.Clamp(value, 0, 100);
                
            }
        }

        public int FarmingLevel { get; set; }
        public float FarmsStaffed { get; set; }
        public int ScienceLevel { get; set; }
        public float LabsStaffed { get; set; }
        public int ManufacturingLevel { get; set; }
        public float FactoriesStaffed { get; set; }
        public int HighTechLevel { get; set; }
        public float HighTechFacilitiesStaffed { get; set; }
        public int MiningLevel { get; set; }
        public float MinesStaffed { get; set; }
        public int GovernmentLevel { get; set; }
        public float GovernmentFacilitiesStaffed { get; set; }

        public int PartEmployedFarmers
        {
            get
            {
                int partEmployedFarmers = 0;
                foreach (Pops farmer in PopsInTile)
                {
                    if (farmer.PopClass == Pops.ePopClass.Farmer)
                    {
                        if (farmer.Employment == Pops.ePopEmployment.PartiallyEmployed)
                            partEmployedFarmers += 1;
                    }
                }
                return partEmployedFarmers;
            }
        }

        public int UnemployedFarmers
        {
            get
            {
                int unemployedFarmers = 0;
                foreach (Pops farmer in PopsInTile)
                {
                    if (farmer.PopClass == Pops.ePopClass.Farmer)
                    {
                        if (farmer.Employment == Pops.ePopEmployment.Unemployed)
                            unemployedFarmers += 1;
                    }
                }
                return unemployedFarmers;
            }
        }

        public int EmployedFarmers
        {
            get
            {
                int employedFarmers = 0;
                foreach (Pops farmer in PopsInTile)
                {
                    if (farmer.PopClass == Pops.ePopClass.Farmer)
                    {
                        if (farmer.Employment == Pops.ePopEmployment.FullyEmployed)
                            employedFarmers += 1;
                    }
                }
                return employedFarmers;
            }
        }

        public int TotalFarmers
        {
            get
            {
                int totalFarmers = 0;
                foreach (Pops pop in PopsInTile)
                {
                    if (pop.PopClass == Pops.ePopClass.Farmer)
                    {
                        totalFarmers += 1;
                    }
                }

                return totalFarmers;
            }
        }

        public int PartEmployedMiners
        {
            get
            {
                int partEmployedMiners = 0;
                foreach (Pops miner in PopsInTile)
                {
                    if (miner.PopClass == Pops.ePopClass.Miner)
                    {
                        if (miner.Employment == Pops.ePopEmployment.PartiallyEmployed)
                            partEmployedMiners += 1;
                    }
                }
                return partEmployedMiners;
            }
        }

        public int UnemployedMiners
        {
            get
            {
                int unemployedMiners = 0;
                foreach (Pops miner in PopsInTile)
                {
                    if (miner.PopClass == Pops.ePopClass.Miner)
                    {
                        if (miner.Employment == Pops.ePopEmployment.Unemployed)
                            unemployedMiners += 1;
                    }
                }
                return unemployedMiners;
            }
        }

        public int EmployedMiners
        {
            get
            {
                int employedMiners = 0;
                foreach (Pops miner in PopsInTile)
                {
                    if (miner.PopClass == Pops.ePopClass.Miner)
                    {
                        if (miner.Employment == Pops.ePopEmployment.FullyEmployed)
                            employedMiners += 1;
                    }
                }
                return employedMiners;
            }
        }

        public int TotalMiners
        {
            get
            {
                int totalMiners = 0;
                foreach (Pops pop in PopsInTile)
                {
                    if (pop.PopClass == Pops.ePopClass.Miner)
                    {
                        totalMiners += 1;
                    }
                }

                return totalMiners;
            }
        }

        public int PartEmployedFluxmen
        {
            get
            {
                int partEmployed = 0;
                foreach (Pops fluxmen in PopsInTile)
                {
                    if (fluxmen.PopClass == Pops.ePopClass.Fluxmen)
                    {
                        if (fluxmen.Employment == Pops.ePopEmployment.PartiallyEmployed)
                            partEmployed += 1;
                    }
                }
                return partEmployed;
            }
        }

        public int UnemployedFluxmen
        {
            get
            {
                int unemployed = 0;
                foreach (Pops fluxmen in PopsInTile)
                {
                    if (fluxmen.PopClass == Pops.ePopClass.Fluxmen)
                    {
                        if (fluxmen.Employment == Pops.ePopEmployment.Unemployed)
                            unemployed += 1;
                    }
                }
                return unemployed;
            }
        }

        public int EmployedFluxmen
        {
            get
            {
                int employed = 0;
                foreach (Pops fluxmen in PopsInTile)
                {
                    if (fluxmen.PopClass == Pops.ePopClass.Fluxmen)
                    {
                        if (fluxmen.Employment == Pops.ePopEmployment.FullyEmployed)
                            employed += 1;
                    }
                }
                return employed;
            }
        }

        public int TotalFluxmen
        {
            get
            {
                int totalFluxmen = 0;
                foreach (Pops pop in PopsInTile)
                {
                    if (pop.PopClass == Pops.ePopClass.Fluxmen)
                    {
                        totalFluxmen += 1;
                    }
                }

                return totalFluxmen;
            }
        }

        public int PartEmployedEngineers
        {
            get
            {
                int partEmployed = 0;
                foreach (Pops engineers in PopsInTile)
                {
                    if (engineers.PopClass == Pops.ePopClass.Engineer)
                    {
                        if (engineers.Employment == Pops.ePopEmployment.PartiallyEmployed)
                            partEmployed += 1;
                    }
                }
                return partEmployed;
            }
        }

        public int UnemployedEngineers
        {
            get
            {
                int unemployed = 0;
                foreach (Pops engineers in PopsInTile)
                {
                    if (engineers.PopClass == Pops.ePopClass.Engineer)
                    {
                        if (engineers.Employment == Pops.ePopEmployment.Unemployed)
                            unemployed += 1;
                    }
                }
                return unemployed;
            }
        }

        public int EmployedEngineers
        {
            get
            {
                int employed = 0;
                foreach (Pops engineers in PopsInTile)
                {
                    if (engineers.PopClass == Pops.ePopClass.Engineer)
                    {
                        if (engineers.Employment == Pops.ePopEmployment.FullyEmployed)
                            employed += 1;
                    }
                }
                return employed;
            }
        }

        public int TotalEngineers
        {
            get
            {
                int totalEngineers = 0;
                foreach (Pops pop in PopsInTile)
                {
                    if (pop.PopClass == Pops.ePopClass.Engineer)
                    {
                        totalEngineers += 1;
                    }
                }

                return totalEngineers;
            }
        }

        public int PartEmployedAdminstrators
        {
            get
            {
                int partEmployed = 0;
                foreach (Pops admins in PopsInTile)
                {
                    if (admins.PopClass == Pops.ePopClass.Administrators)
                    {
                        if (admins.Employment == Pops.ePopEmployment.PartiallyEmployed)
                            partEmployed += 1;
                    }
                }
                return partEmployed;
            }
        }

        public int UnemployedAdminstrators
        {
            get
            {
                int unemployed = 0;
                foreach (Pops admins in PopsInTile)
                {
                    if (admins.PopClass == Pops.ePopClass.Administrators)
                    {
                        if (admins.Employment == Pops.ePopEmployment.Unemployed)
                            unemployed += 1;
                    }
                }
                return unemployed;
            }
        }

        public int EmployedAdminstrators
        {
            get
            {
                int employed = 0;
                foreach (Pops admins in PopsInTile)
                {
                    if (admins.PopClass == Pops.ePopClass.Administrators)
                    {
                        if (admins.Employment == Pops.ePopEmployment.FullyEmployed)
                            employed += 1;
                    }
                }
                return employed;
            }
        }

        public int TotalAdministrators
        {
            get
            {
                int totalAdministrators = 0;
                foreach (Pops pop in PopsInTile)
                {
                    if (pop.PopClass == Pops.ePopClass.Administrators)
                    {
                        totalAdministrators += 1;
                    }
                }

                return totalAdministrators;
            }
        }

        public int TotalMerchants
        {
            get
            {
                int totalMerchants = 0;
                foreach (Pops pop in PopsInTile)
                {
                    if (pop.PopClass == Pops.ePopClass.Merchants)
                    {
                        totalMerchants += 1;
                    }
                }

                return totalMerchants;
            }
        }

        public int TotalScientists
        {
            get
            {
                int totalScientists = 0;
                foreach (Pops pop in PopsInTile)
                {
                    if (pop.PopClass == Pops.ePopClass.Scientist)
                    {
                        totalScientists += 1;
                    }
                }

                return totalScientists;
            }
        }

        public int TotalStarvingPops
        {
            get
            {
                int starvingCount = 0;
                foreach (Pops pop in PopsInTile)
                {
                    if (pop.IsStarving)
                    {
                        starvingCount += 1;
                    }
                }
                return starvingCount;
            }
        }

        public int TotalBlackedOutPops
        {
            get
            {
                int blackoutCount = 0;
                foreach (Pops pop in PopsInTile)
                {
                    if (pop.IsBlackedOut)
                    {
                        blackoutCount += 1;
                    }
                }
                return blackoutCount;
            }
        }

        private int totalDevelopmentLevel = 0;
        public int TotalDevelopmentLevel  // max level for region
        {
            get
            {
                int totalLevel = 0;
                totalLevel = FarmingLevel + ScienceLevel + ManufacturingLevel + HighTechLevel + MiningLevel + GovernmentLevel;
                totalDevelopmentLevel = totalLevel;
                return totalDevelopmentLevel;
            }
        }

        // searchable lists
        public List<string> PopIDsInTile = new List<string>(); // ID accessors for pops assigned to tile (for save games)
        public List<Pops> PopsInTile = new List<Pops>();
        
        #region Tile derived variables
        // derived vars
        private float fluxmenPopRating = 0;
        public float FluxmenPopRating
        {
            get
            {
                int fluxmenPopRating = 0;
                int total = 0;
                int eligPops = 0;

                if (PopsInTile.Count > 0)
                {
                    foreach (Pops pop in PopsInTile)
                    {
                        if (pop.PopClass == Pops.ePopClass.Fluxmen)
                        {
                            total += pop.FluxSkill;
                            eligPops += 1;
                        }                     
                    }
                    if (eligPops > 0)
                    {
                        fluxmenPopRating = total / eligPops;
                    }
                    else
                        fluxmenPopRating = 0;                
                }
                return fluxmenPopRating;
            }
        }

        private float farmingPopRating = 0;
        public float FarmingPopRating
        {
            get
            {
                int farmingPopRating = 0;
                int total = 0;
                int eligPops = 0;

                if (PopsInTile.Count > 0)
                {
                    foreach (Pops pop in PopsInTile)
                    {
                        if (pop.PopClass == Pops.ePopClass.Farmer)
                        {
                            total += pop.FarmingSkill;
                            eligPops += 1;
                        }
                        
                    }
                    if (eligPops > 0)
                    {
                        farmingPopRating = total / eligPops;
                    }
                    else
                        farmingPopRating = 0;
                    
                }
                return farmingPopRating;
            }
        }

        public float MiningPopRating
        {
            get
            {
                int miningPopRating = 0;
                int total = 0;
                int eligPops = 0;
                if (PopsInTile.Count > 0)
                {
                    foreach (Pops pop in PopsInTile)
                    {
                        if (pop.PopClass == Pops.ePopClass.Miner)
                        {
                            total += pop.MiningSkill;
                            eligPops += 1;
                        }
                    }
                    if (eligPops > 0)
                    {
                        miningPopRating = total / eligPops;
                    }
                    else
                        miningPopRating = 0;
                }
                return miningPopRating;
            }
        }

        private float sciencePopRating;
        public float SciencePopRating
        {
            get
            {
                int total = 0;
                if (PopsInTile.Count > 0)
                {
                    foreach (Pops pop in PopsInTile)
                    {
                        total += pop.ScienceSkill;
                    }
                    sciencePopRating = total / PopsInTile.Count;
                }
                return sciencePopRating;
            }
            
        }

        private float highTechPopRating;
        public float HighTechPopRating
        {
            get
            {
                int total = 0;
                if (PopsInTile.Count > 0)
                {
                    foreach (Pops pop in PopsInTile)
                    {
                        total += pop.HighTechSkill;
                    }
                    highTechPopRating = total / PopsInTile.Count;
                }
                return highTechPopRating;
            }
        }

        private float manufacturingPopRating;
        public float ManufacturingPopRating
        {
            get
            {
                int manufacturingPopRating = 0;
                int total = 0;
                int eligPops = 0;
                if (PopsInTile.Count > 0)
                {
                    foreach (Pops pop in PopsInTile)
                    {
                        if (pop.PopClass == Pops.ePopClass.Engineer)
                        {
                            total += pop.ManufacturingSkill;
                            eligPops += 1;
                        }
                    }
                    if (eligPops > 0)
                    {
                        manufacturingPopRating = total / eligPops;
                    }
                    else
                        manufacturingPopRating = 0;
                }
                return manufacturingPopRating;
            }
        }

        private float unemploymentLevel = 0f;
        public float UnemploymentLevel
        {
            get
            {
                if (PopsInTile.Count > 0)
                {
                    float workCount = 0;
                    float popCount = 0;
                    foreach (Pops pop in PopsInTile)
                    {
                        if (pop.Type == Pops.ePopType.Worker && pop.Employment == Pops.ePopEmployment.FullyEmployed)
                            workCount += 1;
                        else if (pop.Type == Pops.ePopType.Worker && pop.Employment == Pops.ePopEmployment.PartiallyEmployed)
                            workCount += 1f;
                        if (pop.Type == Pops.ePopType.Worker)
                        {
                            popCount += 1;
                        }
                    }
                    if (popCount > 0)
                        unemploymentLevel = 1 - (workCount / popCount);
                    else
                        unemploymentLevel = 0f;

                    if (unemploymentLevel > 1f)
                    {
                        unemploymentLevel = 1f;
                    }

                    if (unemploymentLevel < 0f)
                    {
                        unemploymentLevel = 0f;
                    }
                }
                return unemploymentLevel;
            }
        }
        
        //private float energyPerPop;
        public float EnergyPerPop
        {
            get
            {
                return EnergyRating / (1200 / FluxmenPopRating);
            }
        }

        //private float energyPerTile;
        public float EnergyPerTile
        {
            get
            {
                return ((HighTechFacilitiesStaffed * RegionType.HTMod) * EnergyPerPop);
            }
        }

        public float EnergyUsedPerTile
        {
            get
            {
                float energyUsed = 0f;

                energyUsed += Constants.Constants.EnergyBaseUsagePerPop * PopsInTile.Count; // total up energy use of everyone in the region
                energyUsed += Constants.Constants.FarmingBaseEnergyUsage * FarmsStaffed; // total up energy use of all farms
                energyUsed += Constants.Constants.HighTechBaseEnergyUsage * HighTechFacilitiesStaffed;
                energyUsed += Constants.Constants.MiningBaseEnergyUsage * MinesStaffed; // total up energy use of all farms
                energyUsed += Constants.Constants.ScienceBaseEnergyUsage * LabsStaffed;
                energyUsed += Constants.Constants.ManufacturingBaseEnergyUsage * FactoriesStaffed;

                return energyUsed;
            }
        }

        public float AlphaUsedPerTile
        {
            get
            {
                float alphaUsed = 0f;

                alphaUsed += Constants.Constants.FarmingBaseAlphaUsage * FarmsStaffed; // total up energy use of all farms
                alphaUsed += Constants.Constants.HighTechBaseAlphaUsage * HighTechFacilitiesStaffed;
                alphaUsed += Constants.Constants.MiningBaseAlphaUsage * MinesStaffed; // total up energy use of all farms
                alphaUsed += Constants.Constants.ScienceBaseAlphaUsage * LabsStaffed;
                alphaUsed += Constants.Constants.ManufacturingBaseAlphaUsage * FactoriesStaffed;

                return alphaUsed;
            }
        }

        public float HeavyUsedPerTile
        {
            get
            {
                float heavyUsed = 0f;

                heavyUsed += Constants.Constants.FarmingBaseHeavyUsage * FarmsStaffed; // total up energy use of all farms
                heavyUsed += Constants.Constants.HighTechBaseHeavyUsage * HighTechFacilitiesStaffed;
                heavyUsed += Constants.Constants.MiningBaseHeavyUsage * MinesStaffed; // total up energy use of all farms
                heavyUsed += Constants.Constants.ScienceBaseHeavyUsage * LabsStaffed;
                heavyUsed += Constants.Constants.ManufacturingBaseHeavyUsage * FactoriesStaffed;

                return heavyUsed;
            }
        }

        public float RareUsedPerTile
        {
            get
            {
                float rareUsed = 0f;

                rareUsed += Constants.Constants.FarmingBaseRareUsage * FarmsStaffed; // total up energy use of all farms
                rareUsed += Constants.Constants.HighTechBaseRareUsage * HighTechFacilitiesStaffed;
                rareUsed += Constants.Constants.MiningBaseRareUsage * MinesStaffed; // total up energy use of all farms
                rareUsed += Constants.Constants.ScienceBaseRareUsage * LabsStaffed;
                rareUsed += Constants.Constants.ManufacturingBaseRareUsage * FactoriesStaffed;

                return rareUsed;
            }
        }

        public float FoodUsedPerTile
        {
            get
            {
                float foodUsed = 0f;

                foodUsed += Constants.Constants.FoodBaseUsagePerPop * PopsInTile.Count; // total up energy use of everyone in the region

                return foodUsed;
            }
        }

        private float ProductionMod
        {
            get
            {
                float prodMod = 0;
                bool productionModFound = false;

                if (HelperFunctions.DataRetrivalFunctions.GetPlanet(PlanetLocationID).PlanetTraits.Count > 0)
                {
                    foreach(PlanetTraits trait in HelperFunctions.DataRetrivalFunctions.GetPlanet(PlanetLocationID).PlanetTraits)
                    {
                        if (trait.ProdMod > 0)
                        {
                            productionModFound = true;
                            prodMod += trait.ProdMod;
                        }
                    }
                }

                if (!productionModFound)
                    return prodMod = 1;
                else
                    return (prodMod/100) + 1;
            }
        }
     
        public float AlphaMaterialsPerPop
        {
            get
            {              
                return (AlphaRating / (1500 / MiningPopRating)) * ProductionMod;
            }
        }

       
        public float AlphaMaterialsPerTile
        {
            get
            {
                return  (MinesStaffed * RegionType.MineralMod) * AlphaMaterialsPerPop;
            }
        }

        public float FoodPerPop
        {
            get
            {
                return BioRating / (3000 / FarmingPopRating);
            }
        }

        public float FoodPerTile
        {
            get
            {
                return ((FarmsStaffed * RegionType.FarmMod) * FoodPerPop);
            }
        }

        public float HeavyPerPop
        {
            get
            {
                return (HeavyRating / (2000 / MiningPopRating)) * ProductionMod;
            }

        }

        public float HeavyPerTile
        {
            get
            {
                return (((FactoriesStaffed * RegionType.ManMod) * (MinesStaffed * RegionType.MineralMod)) * HeavyPerPop);
            }
        }

        public float RarePerPop
        {
            get
            {
                return (RareRating / (400 / HighTechPopRating)) / (600 / MiningPopRating) * ProductionMod;
            }

        }

        public float RarePerTile
        {
            get
            {
                return (((HighTechFacilitiesStaffed * RegionType.HTMod) * (MinesStaffed * RegionType.MineralMod)) * RarePerPop);
            }
        }

        public int ChildPops
        {
            get
            {
                int childPops = 0;
                //int total = 0;

                //if (PopsInTile.Count > 0)
                //{
                //    foreach (Pops pop in PopsInTile)
                //    {
                //        if (pop.Type == Pops.ePopType.Children)
                //            total += 1;
                //    }
                //    childPops = total;
                //}
                return childPops;  
            }
        }

        public int WorkerPops
        {
            get
            {         
                int total = 0;
                if (PopsInTile.Count > 0)
                {
                    foreach (Pops pop in PopsInTile)
                    {
                        if (pop.Type == Pops.ePopType.Worker)
                            total += 1;
                    }
                }
                return total;
            }
        }

        public int RetiredPops
        {
            get
            {
                int total = 0;
                if (PopsInTile.Count > 0)
                {
                    foreach (Pops pop in PopsInTile)
                    {
                        if (pop.Type == Pops.ePopType.Retired)
                            total += 1;
                    }                   
                }
                return total;
            }
        }

        public int StarvingPops
        {
            get
            {
                int total = 0;
                if (PopsInTile.Count > 0)
                {
                    foreach (Pops pop in PopsInTile)
                    {
                        if (pop.IsStarving)
                            total += 1;
                    }
                    
                }
                return total;              
            }
        }

        public int BlackedOutPops
        {
            get
            {
                int total = 0;
                if (PopsInTile.Count > 0)
                {
                    foreach (Pops pop in PopsInTile)
                    {
                        if (pop.IsBlackedOut)
                            total += 1;
                    }

                }
                return total;
            }
        }

        public int PopulationChange
        {
            get
            {
                return GraduatedLastTurn + ImmigratedLastTurn - (DiedLastTurn + EmigratedLastTurn);
            }
        }

        public void UpdateBirthAndDeath()
        {
            // reset movement values
            DiedLastTurn = 0;
            GraduatedLastTurn = 0;
            ImmigratedLastTurn = 0;
            EmigratedLastTurn = 0;

            GlobalGameData gData = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
            int month = gData.GameMonth;

            // check for deaths
            foreach (Pops pop in PopsInTile)
            {
                if (month == 0)
                {
                    pop.Age += 1; // age pops at the start of every year
                }

                int deathChance = UnityEngine.Random.Range(-10000, 5000);
                if (pop.IsStarving)
                {
                    deathChance += 500;
                }
                if (pop.IsBlackedOut)
                {
                    if (pop.Type == Pops.ePopType.Retired)
                    {
                        deathChance += 200;
                    }
                    deathChance += 150;
                }

                if (pop.Age < 30)
                {
                    deathChance += (pop.Age);
                }
                else if (pop.Age < 60)
                {
                    deathChance += (pop.Age * 3);
                }
                else if (pop.Age < 70)
                {
                    deathChance += (pop.Age * 5);
                }
                else
                    deathChance += (pop.Age * 12);

                if (deathChance > 5250)
                {
                    pop.Type = Pops.ePopType.Dead;
                    DiedLastTurn += 1;
                }
            }

            // check for births
            //if (PopsInTile.Count < MaxSafePopulationLevel)
            //{
                int birthChance = 5000;
                int birthCount = 0;
                while (birthChance > 4850)
                {
                    birthChance = UnityEngine.Random.Range(-5000, 5000);
                    birthChance += BioRating * 10;
                    birthChance += (int)UnemploymentLevel * 10;
                    birthChance -= birthCount * 100;

                    if (birthChance > 4850)
                    {      
                        GenerateGameObject.GenerateNewPop(HelperFunctions.DataRetrivalFunctions.GetCivilization(OwnerCivID), this, true); // generate a new young adult
                        birthCount += 1;
                        GraduatedLastTurn += 1;                    
                    }
                }               
           // }

            // bring out your dead
            foreach (Pops pop in PopsInTile.ToArray())
            {
                if (pop.Type == Pops.ePopType.Dead)
                {
                    PopsInTile.Remove(pop);
                    PopsInTile.TrimExcess();
                }
            }

        }

        public void UpdatePopularSupport()
        {
            foreach (Pops pop in PopsInTile)
            {
                // check employment status
                if (pop.Employment == Pops.ePopEmployment.FullyEmployed)
                {
                    pop.PopSupport += UnityEngine.Random.Range(.005f, .015f);
                    pop.UnrestLevel -= UnityEngine.Random.Range(.1f, .4f);
                }
                if (pop.Employment == Pops.ePopEmployment.PartiallyEmployed)
                {
                    pop.PopSupport += UnityEngine.Random.Range(-.01f, .005f);
                    pop.UnrestLevel -= UnityEngine.Random.Range(-.8f, .2f);
                }
                if (pop.Employment == Pops.ePopEmployment.Unemployed)
                {
                    pop.PopSupport += UnityEngine.Random.Range(-.03f, -.015f);
                    pop.UnrestLevel -= UnityEngine.Random.Range(-1.2f, -.3f);
                }

                // status conditions
                if (pop.IsStarving)
                {
                    pop.PopSupport += UnityEngine.Random.Range(-.2f, -.05f);
                    pop.UnrestLevel -= UnityEngine.Random.Range(-1.5f, -.8f);
                }
                if (pop.IsBlackedOut)
                {
                    pop.PopSupport += UnityEngine.Random.Range(-.15f, -.05f);
                    pop.UnrestLevel -= UnityEngine.Random.Range(-.9f, -.2f);
                }
            }
        }

        // functions to determine unemployment/employment in a region
        public void UpdateEmployment()
        {

            foreach(Pops pop in PopsInTile)
            {
                
                if (pop.Type == Pops.ePopType.Worker && pop.Employment == Pops.ePopEmployment.Unemployed)
                {
                    //pop.Employment = Pops.ePopEmployment.Unemployed; // reset employment status
                    switch (pop.PopClass)
                    {                      
                        case Pops.ePopClass.Scientist:
                            if (LabsStaffed < ScienceLevel)
                            {
                                pop.Employment = Pops.ePopEmployment.FullyEmployed;
                            }
                            //else if (LabsStaffed < ScienceLevel)
                            //{                     
                            //    pop.Employment = Pops.ePopEmployment.PartiallyEmployed;
                            //}
                            //else
                            {
                                pop.Employment = Pops.ePopEmployment.Unemployed;
                            }
                            break;

                        case Pops.ePopClass.Farmer:
                            if (FarmsStaffed < FarmingLevel)
                            {                               
                                pop.Employment = Pops.ePopEmployment.FullyEmployed;
                            }
                            //else if (FarmsStaffed < FarmingLevel)
                            //{
                            //    pop.Employment = Pops.ePopEmployment.PartiallyEmployed;
                            //}
                            else
                            {
                                pop.Employment = Pops.ePopEmployment.Unemployed;
                            }
                            break;

                        case Pops.ePopClass.Miner:
                            if (MinesStaffed < MiningLevel)
                            {
                                
                                pop.Employment = Pops.ePopEmployment.FullyEmployed;
                            }
                            //else if (MinesStaffed < MiningLevel)
                            //{                              
                            //    pop.Employment = Pops.ePopEmployment.PartiallyEmployed;
                            //}
                            else
                            {
                                pop.Employment = Pops.ePopEmployment.Unemployed;
                            }
                            break;

                        case Pops.ePopClass.Engineer:
                            if (FactoriesStaffed < ManufacturingLevel)
                            {                             
                                pop.Employment = Pops.ePopEmployment.FullyEmployed;
                            }
                            //else if (FactoriesStaffed < ManufacturingLevel)
                            //{
                            //    pop.Employment = Pops.ePopEmployment.PartiallyEmployed;
                            //}
                            else
                            {
                                pop.Employment = Pops.ePopEmployment.Unemployed;
                            }
                            break;

                        case Pops.ePopClass.Fluxmen:
                            if (HighTechFacilitiesStaffed < HighTechLevel)
                            {
                                pop.Employment = Pops.ePopEmployment.FullyEmployed;
                            }
                            //else if (HighTechFacilitiesStaffed < HighTechLevel)
                            //{
                            //    pop.Employment = Pops.ePopEmployment.PartiallyEmployed;
                            //}
                            else
                            {
                                pop.Employment = Pops.ePopEmployment.Unemployed;
                            }
                            break;

                        case Pops.ePopClass.Merchants:
                            pop.Employment = Pops.ePopEmployment.FullyEmployed; // will write more code later when determine how merchants are employed
                            break;

                        case Pops.ePopClass.Administrators:
                            if (GovernmentFacilitiesStaffed < GovernmentLevel)
                            {
                                //GovernmentFacilitiesStaffed += 1; // staff a lab
                                pop.Employment = Pops.ePopEmployment.FullyEmployed;
                            }
                            //else if (GovernmentFacilitiesStaffed < GovernmentLevel)
                            //{
                            //    //GovernmentFacilitiesStaffed += .5f; // staff a lab
                            //    pop.Employment = Pops.ePopEmployment.PartiallyEmployed;
                            //}
                            else
                            {
                                pop.Employment = Pops.ePopEmployment.Unemployed;
                            }
                            break;
                        case Pops.ePopClass.None:
                            break;
                        default:
                            break;
                    }
                    UpdateRegionStaffing();
                }
            }
        }

        private void UpdateRegionStaffing()
        {
            // reset all staffing
            LabsStaffed = 0;
            FarmsStaffed = 0;
            MinesStaffed = 0;
            FactoriesStaffed = 0;
            GovernmentFacilitiesStaffed = 0;
            HighTechFacilitiesStaffed = 0;

            foreach (Pops pop in PopsInTile)
            {
                switch (pop.PopClass)
                {
                    case Pops.ePopClass.Scientist:
                        if (pop.Employment == Pops.ePopEmployment.FullyEmployed)
                            LabsStaffed += 1;
                        if (pop.Employment == Pops.ePopEmployment.PartiallyEmployed)
                            LabsStaffed += .5f;
                        break;
                    case Pops.ePopClass.Farmer:
                        if (pop.Employment == Pops.ePopEmployment.FullyEmployed)
                            FarmsStaffed += 1;
                        if (pop.Employment == Pops.ePopEmployment.PartiallyEmployed)
                            FarmsStaffed += .5f;
                        break;
                    case Pops.ePopClass.Miner:
                        if (pop.Employment == Pops.ePopEmployment.FullyEmployed)
                            MinesStaffed += 1;
                        if (pop.Employment == Pops.ePopEmployment.PartiallyEmployed)
                            MinesStaffed += .5f;
                        break;
                    case Pops.ePopClass.Engineer:
                        if (pop.Employment == Pops.ePopEmployment.FullyEmployed)
                            FactoriesStaffed += 1;
                        if (pop.Employment == Pops.ePopEmployment.PartiallyEmployed)
                            FactoriesStaffed += .5f;
                        break;
                    case Pops.ePopClass.Fluxmen:
                        if (pop.Employment == Pops.ePopEmployment.FullyEmployed)
                            HighTechFacilitiesStaffed += 1;
                        if (pop.Employment == Pops.ePopEmployment.PartiallyEmployed)
                            HighTechFacilitiesStaffed += .5f;
                        break;
                    case Pops.ePopClass.Merchants:
                        break;
                    case Pops.ePopClass.Administrators:
                        if (pop.Employment == Pops.ePopEmployment.FullyEmployed)
                            GovernmentFacilitiesStaffed += 1;
                        if (pop.Employment == Pops.ePopEmployment.PartiallyEmployed)
                            GovernmentFacilitiesStaffed += .5f;
                        break;
                    case Pops.ePopClass.None:
                        break;
                    default:
                        break;
                }
            }
        }


        #endregion
    }

    public class Pops
    {
        public enum ePopType : int
        {
            Worker,
            Retired,
            Rioting,
            InTransit,
            Dead
        }

        public enum ePopEmployment : int
        {
            FullyEmployed,
            PartiallyEmployed,
            Unemployed,
        }

        public enum ePopClass : int
        {
            Scientist,
            Farmer,
            Miner,
            Engineer,
            Fluxmen,
            Merchants,
            Administrators,
            None
        }

        public ePopClass PopClass
        {
            get
            {
                int[,] Value_Skill = new int[8,2]{{FarmingSkill,1},{ScienceSkill,0},{ManufacturingSkill,3},{MiningSkill,2},{HighTechSkill,4}, {MerchantSkill,5},{FluxSkill,4}, {AdminSkill, 6}};
                ePopClass highestValue = ePopClass.Administrators; // initialize
                int highestSkillValue = 0;

                if (Type != ePopType.Retired) // don't count retired folks
                {
                    for (int i = 0; i < 8; i++) // iterate through the skill arrays and return the skill type that corresponds to the highest valued skill
                    {
                        if (Value_Skill[i, 0] > highestSkillValue)
                        {
                            highestSkillValue = Value_Skill[i, 0];
                            highestValue = (ePopClass)(Value_Skill[i, 1]);
                        }
                    }
                }
                else
                {
                    highestValue = ePopClass.None;
                }

                return highestValue;
            }
        }

        public string ID { get; set; }
        public int Age { get; set; }
        public bool IsStarving { get; set; }
        public bool IsMigratingOffPlanet { get; set; }
        public bool IsBlackedOut { get; set; }
        public string RegionLocationID { get; set; } // the region location (by extension will have planet)
        public string EmpireID { get; set; } // the empire/faction/civ the pops belong to
        public ePopType Type { get; set; }
        public ePopEmployment Employment { get; set; }

        public PlanetData PlanetLocated
        {
            get
            {
                Region regionLocated = HelperFunctions.DataRetrivalFunctions.GetRegion(RegionLocationID);
                return regionLocated.PlanetLocated;
            }

        }

        public Region RegionLocated
        {
            get
            {
                Region regionLocated = HelperFunctions.DataRetrivalFunctions.GetRegion(RegionLocationID);
                return regionLocated;              
            }

        }
        private int _pFarmingSkill;
        public int FarmingSkill
        {
            get
            {
                return _pFarmingSkill;
            }

            set
            {
                _pFarmingSkill = Mathf.Clamp(value, 10, 100);
            }
        }
        private int _pScienceSkill;
        public int ScienceSkill
        {
            get
            {
                return _pScienceSkill;
            }

            set
            {
                _pScienceSkill = Mathf.Clamp(value, 10, 100);
            }
        }
        private int _pManuSkill;
        public int ManufacturingSkill
        {
            get
            {
                return _pManuSkill;
            }

            set
            {
                _pManuSkill = Mathf.Clamp(value, 10, 100);
            }
        }
        private int _pHTechSkill;
        public int HighTechSkill
        {
            get
            {
                return _pHTechSkill;
            }

            set
            {
                _pHTechSkill = Mathf.Clamp(value, 10, 100);
            }
        }
        private int _pMiningSkill;
        public int MiningSkill
        {
            get
            {
                return _pMiningSkill;
            }

            set
            {
                _pMiningSkill = Mathf.Clamp(value, 10, 100);
            }
        }
        private int _pMerSkill;
        public int MerchantSkill
        {
            get
            {
                return _pMerSkill;
            }

            set
            {
                _pMerSkill = Mathf.Clamp(value, 10, 100);
            }
        }
        private int _pFluxSkill;
        public int FluxSkill
        {
            get
            {
                return _pFluxSkill;
            }

            set
            {
                _pFluxSkill = Mathf.Clamp(value, 10, 100);
            }
        }
        private int _pAdminSkill;
        public int AdminSkill
        {
            get
            {
                return _pAdminSkill;
            }

            set
            {
                _pAdminSkill = Mathf.Clamp(value, 10, 100);
            }
        }

        private float _pPopSupport;
        public float PopSupport
        {
            get
            {
                return _pPopSupport;
            }

            set
            {
                _pPopSupport = Mathf.Clamp(value, 0f, 100f);
            }
        }

        private float _pUnrestLevel;
        public float UnrestLevel
        {
            get
            {
                return _pUnrestLevel;
            }

            set
            {
                _pUnrestLevel = Mathf.Clamp(value, 0f, 100f);
            }
        }
        public float PlanetHappinessLevel { get; set; }
    }

    public class RegionTypeData
    {
        public enum eRegionType : int
        {
            Plains,
            Mountains,
            Lava,
            Volcanic,
            Ocean,
            Forest,
            Grassland,
            Jungle,
            Barren,
            Uninhabitable,
            Frozen,
            Desert,
            Helium_Island,
            Dead
        }

        public eRegionType Type { get; set; }
        public float BioMod { get; set; }
        public float FarmMod { get; set; }
        public float ManMod { get; set; }
        public float ResMod { get; set; }
        public float AttMod { get; set; }
        public float DefMod { get; set; }
        public float HTMod { get; set; }
        public float MineralMod { get; set; }
    }

    public class PlanetTraits
    {
        public enum eTraitCategory : int
        {
            NoContradiction,
            PureAstronomical,
            Gravity,
            Geological,
            Magnetic,
            Atmosphere,
            Soil,
            Water,
            Weather,
            Lifeforms
        }

        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public eTraitCategory Category { get; set; }
        public PlanetData.ePlanetType[] PlanetType = new PlanetData.ePlanetType[5]; // 5 different planet types
        public int MoonsNecessary { get; set; }
        public int MinSize { get; set; }
        public int HabMod { get; set; }
        public int ProdMod { get; set; }
        public int EnergyMod { get; set; }
        public int ResearchMod { get; set; }
        public float ChanceDestroyed { get; set; }
        public float ChanceTerraformed { get; set; }
        public float ChanceStellarTerraformed { get; set; }
        public int BeltEligible { get; set; }
        public string TextWhenDestroyed { get; set; }
        public int HabitableTilesMod { get; set; }
        public int AttackMod { get; set; }
        public int DefenceMod { get; set; }
        public string EventCode { get; set; }
        public float EventChance { get; set; }
        public string SecondaryEventCode { get; set; }
        public float SecondaryEventChance { get; set; }
    }

    // the active build plan for each planet
    public class BuildPlan
    {
        public enum eBuildPlanStatus : int
        {
            Active,
            Pending,
            Inactive
        }

        public eBuildPlanStatus Status;
        public float FarmsAllocation { get; set; }
        public float HighTechAllocation { get; set; }
        public float FactoryAllocation { get; set; }
        public float MineAllocation { get; set; }
        public float AdminAllocation { get; set; }
        public float LabsAllocation { get; set; }
        public float GroundMilitaryAllocation { get; set; }
        public float StarshipAllocation { get; set; }
        public float InfraAllocation { get; set; }
        public float TotalEdictAllocation { get; set; }
        public Region TargetRegion { get; set; } // sets a targeted region of the planet to develop
        public int OverdriveMultiplier { get; set; }
    }
}
