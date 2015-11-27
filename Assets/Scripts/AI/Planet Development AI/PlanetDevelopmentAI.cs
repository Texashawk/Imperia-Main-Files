using UnityEngine;
using System.Collections;
using StellarObjects;
using PlanetObjects;
using CivObjects;
using CharacterObjects;
using Constants;
using System.Collections.Generic;

public static class PlanetDevelopmentAI
{
	
    public static void AdjustViceroyBuildPlan(PlanetData pData, bool forceBuildPlanChance)
    {
        GlobalGameData gDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
        Character vic = pData.Viceroy;
        bool targetRegionSelected = false;

        if (vic != null)
        {
            // determine focus and if change needed
            bool changeFocus = DoesBuildFocusChange(pData, vic, gDataRef);
            if (changeFocus || forceBuildPlanChance)
            {
                float farmPriority = 1f;
                float highTechPriority = 1f;
                float minePriority = 1f;
                float infrastructurePriority = 1f;
                float labPriority = 1f;
                float adminPriority = 1f;
                float factoryPriority = 1f;
                float militaryGroundPriority = 0f;
                targetRegionSelected = false;
                float militarySpacePriority = 0f;

                // determine the current need for builds
                if (pData.FoodDifference < 0)
                {
                    farmPriority += Mathf.Abs(pData.FoodDifference * 4);
                }

                if (pData.FoodDifference > pData.TotalFoodConsumed)
                {
                    farmPriority -= pData.FoodDifference / pData.TotalFoodConsumed;
                }
                
                if (pData.EnergyDifference < 0)
                {
                    highTechPriority += Mathf.Abs(pData.EnergyDifference * 2f);
                }

                if (pData.EnergyDifference > pData.TotalEnergyConsumed)
                {
                    highTechPriority -= pData.EnergyDifference / pData.TotalEnergyConsumed;
                }

                // determine the amount of overcrowding in the planet regions
                float infraNeed = 0f;
                foreach (Region rData in pData.RegionList)
                {
                    
                    if (rData.PopsInTile.Count > rData.MaxSafePopulationLevel)
                    {
                        float crowding = (float)(rData.PopsInTile.Count / (float)rData.MaxSafePopulationLevel) - 1f;
                        if (crowding < 0f)
                            crowding = 0f;
                        infraNeed += crowding;
                    }
                }

                if (pData.ManufacturingLevel < 10)
                {
                    factoryPriority += (10 - pData.ManufacturingLevel) / 2f; 
                }

                if (pData.AlphaPreProductionDifference < 0)
                {
                    minePriority += Mathf.Abs(pData.AlphaPreProductionDifference);
                }

                if (pData.HeavyPreProductionDifference < 0)
                {
                    minePriority += Mathf.Abs(pData.HeavyPreProductionDifference / 2f);
                }

                if (pData.RarePreProductionDifference < 0)
                {
                    minePriority += Mathf.Abs(pData.RarePreProductionDifference / 3f);
                }

                adminPriority += ((float)pData.Rank * 1.5f);

                farmPriority += vic.Empathy / 20f; // characters with high Empathy will focus on food and power
                highTechPriority += vic.Empathy / 20f;

                // now determine based on tendencies of viceroy
                farmPriority += (float)(vic.PopsTendency / 100f);
                highTechPriority += (float)(vic.PopsTendency / 100f);
                infrastructurePriority += (float)(vic.PopsTendency / 100f);
                adminPriority += (float)(vic.AdminTendency / 100f);
                minePriority += (float)(vic.WealthTendency / 100f);
                factoryPriority += (float)(((vic.WealthTendency + vic.PopsTendency) / 2f) / 100f);
                labPriority += (float)(vic.ScienceTendency / 100f);

                
                // use traits to also determine specific biases
                if (vic.Traits.Exists(p => p.Name.ToUpper() == "TECHNOPHILE"))
                {
                    labPriority += UnityEngine.Random.Range(0, 2f);
                }

                if (vic.Traits.Exists(p => p.Name.ToUpper() == "INDUSTRIALIST"))
                {
                    factoryPriority += UnityEngine.Random.Range(0, 2f);
                }

                if (vic.Traits.Exists(p => p.Name.ToUpper() == "TECHNOPHOBE"))
                {
                    labPriority = 0;
                }

                if (vic.Traits.Exists(p => p.Name.ToUpper() == "BUREAUCRAT"))
                {
                    adminPriority += UnityEngine.Random.Range(0, 2f);
                }

                // normalize results below 0
                if (farmPriority < 0f)
                    farmPriority = 0f;
                if (highTechPriority < 0f)
                    highTechPriority = 0f;
                if (adminPriority < 0f)
                    adminPriority = 0f;
                if (minePriority < 0f)
                    minePriority = 0f;
                if (factoryPriority < 0f)
                    factoryPriority = 0f;
                if (labPriority < 0f)
                    labPriority = 0f;
                if (infrastructurePriority < 0f)
                    infrastructurePriority = 0f;

                // now normalize the priorities to percentage of 100
                float sumPriorities = farmPriority + highTechPriority + minePriority + infrastructurePriority + labPriority + adminPriority + factoryPriority + militaryGroundPriority + militarySpacePriority + pData.BuildPlan.TotalEdictAllocation;
                pData.BuildPlan.FarmsAllocation = farmPriority / sumPriorities;
                pData.BuildPlan.HighTechAllocation = highTechPriority / sumPriorities;
                pData.BuildPlan.MineAllocation = minePriority / sumPriorities;
                pData.BuildPlan.LabsAllocation = labPriority / sumPriorities;
                pData.BuildPlan.AdminAllocation = adminPriority / sumPriorities;
                pData.BuildPlan.InfraAllocation = infrastructurePriority / sumPriorities;
                pData.BuildPlan.FactoryAllocation = factoryPriority / sumPriorities;
                pData.BuildPlan.GroundMilitaryAllocation = militaryGroundPriority / sumPriorities;
                pData.BuildPlan.StarshipAllocation = militarySpacePriority / sumPriorities;
                pData.BuildPlan.Status = BuildPlan.eBuildPlanStatus.Pending;

                if (pData.BuildPlan.InfraAllocation > 0f && !targetRegionSelected)
                {
                    Region mostOvercrowdedRegion = null;
                    float highRegionCrowding = 0f;

                    List<Region> regionsEligible = new List<Region>();
                    foreach (Region rData in pData.RegionList)
                    {  
                        {
                            if (rData.IsHabitable)
                                regionsEligible.Add(rData);
                        }
                    }

                    foreach (Region rID in regionsEligible)
                    {
                        Region testRegion = rID;
                        float currentRegionCrowding = (float)testRegion.PopsInTile.Count / (float)testRegion.MaxSafePopulationLevel;
                        if (currentRegionCrowding > highRegionCrowding)
                        {
                            highRegionCrowding = currentRegionCrowding;
                            mostOvercrowdedRegion = testRegion;
                        }
                    }

                    targetRegionSelected = true;
                    pData.BuildPlan.TargetRegion = mostOvercrowdedRegion;
                   
                }
            }

            pData.BuildPlan.OverdriveMultiplier = 1; // set overdrive at 1 until I write the AI for it
            
        }

    }

    private static bool DoesBuildFocusChange(PlanetData pData, Character vic, GlobalGameData gDataRef)
    {
        float timeSinceChange = vic.TimeSinceBuildFocusChange;
        float changeChance = 0;

        if (vic.Traits.Exists(p => p.Name == "Erratic"))
        {
            changeChance += 20f;
        }

        changeChance -= (vic.GoalStabilityTendency / 10f);
        changeChance += timeSinceChange * 10f;

        // if there are critical shortfalls on planets, vics are much less likely to change their focus
        if (vic.BuildFocus == Character.eBuildFocus.Farms)
        {
            if (pData.FoodDifference > 0)
            {
                changeChance -= pData.FoodDifference;
            }
        }

        if (vic.BuildFocus == Character.eBuildFocus.Hightech)
        {
            if (pData.EnergyDifference > 0)
            {
                changeChance -= pData.EnergyDifference;
            }
        }

        if (changeChance > 80)
        {
            return true; // yes, the focus changes
        }

        return false;
    }
}
