using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using PlanetObjects;
using StellarObjects;
using CharacterObjects;
using CivObjects;
using HelperFunctions;
using EconomicObjects;
using GameEvents;
using Constants;

public class TurnEngine : MonoBehaviour {

    private GlobalGameData gDataRef; // global game data
    private GalaxyData galDataRef; // galaxy data

	// Use this for initialization
	void Start () 
    {
        galDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
        gDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();

        // run 4 times before first turn to maximize 
        for (int x = 0; x < 4; x++)
        {
            foreach (Civilization civ in gDataRef.CivList)
            {
                UpdatePlanets(civ);
                CheckForMigration(civ); // check for intraplanet migration
                MigratePopsBetweenPlanets(civ); // and if there are any pops who want to leave, check for where   
            }
        }
        UpdateTrades();
        UpdateEmperor();
        gDataRef.UpdateGameDate();
        gDataRef.RequestGraphicRefresh = true;
	}

    public void ExecuteNewTurn() // this is the master turn execution function
    {   
        UpdateTrades();
        UpdateAllCivs();
        UpdateEmperor();
        gDataRef.UpdateGameDate(); // advance the year
        gDataRef.RequestGraphicRefresh = true;
    }

    private void UpdateAllCivs()
    {
        foreach (Civilization civ in gDataRef.CivList)
        {
            if (gDataRef.GameMonth == 0)
            {
                ResetCivBudgets(civ);
            }
            UpdatePopularSupport(civ); // advance popular support
            UpdatePlanets(civ); // advance economy, move pops to same planet, update unrest/popular support levels, etc
            CheckForMigration(civ); // check for intraplanet migration                     
            MigratePopsBetweenPlanets(civ); // and if there are any pops who want to leave, check for where          
            UpdateEvents(civ);           
        }
    }

    private void UpdateEmperor()
    {
        int AP = 0;
        AP = gDataRef.CivList[0].Leader.BaseActionPoints;
        AP = UnityEngine.Random.Range(2, AP + 2) + gDataRef.CivList[0].Leader.ActionPoints;
        if (AP > 10)
            AP = 10;
        gDataRef.CivList[0].Leader.ActionPoints = AP;
    }

    private void ResetCivBudgets(Civilization civ)
    {
        civ.Revenues = 0;
        civ.Expenses = 0; // reset the budgets at the start of the year
    }

    private void UpdateEvents(Civilization civ)
    {

        foreach (PlanetData pData in civ.PlanetList)
        {
            GameEvents.PlanetEventCreator.GeneratePlanetEvents(pData, civ); // generate the events for each planet
        }

        foreach (GameEvents.GameEvent gEvent in civ.LastTurnEvents.ToArray())
        {
            if (!gEvent.EventIsNew) // delete the event if it's already been seen
            {
                civ.LastTurnEvents.Remove(gEvent);
                civ.LastTurnEvents.TrimExcess();
            }
            if (gEvent.EventIsNew)
            {
                gEvent.EventIsNew = false;
            }
        }
    }

    private void UpdateTrades()
    {
        UpdateTradeAgreements();
        UpdateResourceStockBalances();
    }

    private void UpdatePlanets(Civilization civ) // this is where all planet-update functions go to save time
    {
        foreach (PlanetData pData in civ.PlanetList)
        {
            pData.UpdateBirthAndDeath(); // update popular support and unrest levels
            pData.MigratePopsBetweenRegions(); // first check to move pops between regions to regions that might better support their talents
            pData.UpdateEmployment(); // once pops have moved, check employment status
            pData.UpdateShortfallConditions(); // update shortfall results (food, power, etc)
                      
            if (gDataRef.GameMonth == 1) // on the first month of every year, do this
            {
                pData.SendTaxUpward(); // determine taxes and send up the chute
                PlanetDevelopmentAI.AdjustViceroyBuildPlan(pData, true); // look at adjusting the build plan for each planet, force every year or when game starts
            }
            else
            {
                PlanetDevelopmentAI.AdjustViceroyBuildPlan(pData, false); // look at adjusting the build plan for each planet
            }

            pData.ExecuteProductionPlan(); // update production of new infrastructure on the planet
            //GameEvents.PlanetEventCreator.GeneratePlanetEvents(pData, civ); // generate the events for each planet
        }
    }

    private void UpdatePopularSupport(Civilization civ)
    {
        foreach (PlanetData pData in civ.PlanetList)
        {
            pData.UpdatePopularSupport(); // update popular support and unrest levels
        }
    }

    private void CheckForMigration(Civilization civ)
    {
        foreach (PlanetData pData in civ.PlanetList)
        {
            pData.UpdateMigrationStatus();
        }
    }

    private void MigratePopsBetweenPlanets(Civilization civ)
    {
        foreach (PlanetData pData in civ.PlanetList)
        {
            foreach (Region rData in pData.RegionList)
            {
                foreach (Pops pop in rData.PopsInTile.ToArray())
                {
                    if (pop.IsMigratingOffPlanet)
                    {
                        DetermineMigrationLocation(pop, civ);
                    }
                }
            }
        }
    }

    private void DetermineMigrationLocation(Pops pop, Civilization civ)
    {
        float topPlanetValue = 0f;
        PlanetData topPlanet = null;

        foreach (PlanetData pData in civ.PlanetList) // determine the top value of each planet and choose the best one
        {
            if (pData != pop.PlanetLocated)
            {
                float currentPlanetValue = MigrationValueOfPlanet(pData, pop);
                if (currentPlanetValue > topPlanetValue)
                {
                    topPlanetValue = currentPlanetValue;
                    topPlanet = pData;
                }
            }
        }

        if (topPlanetValue > Constants.Constants.StellarMigrationThreshold + UnityEngine.Random.Range(0,200)) // if the best value is above a certain threshold
        {
            List<Region> eligibleRegionList = new List<Region>();
            foreach (Region rData in topPlanet.RegionList.ToArray())
            {            
                if (rData.IsHabitable)
                {
                    eligibleRegionList.Add(rData);
                }
            }

            if (eligibleRegionList.Count > 0)
            {
                int regionChoice = UnityEngine.Random.Range(0,eligibleRegionList.Count); // find an eligible region

                // move the pop from one region on a planet to another planet and a suitable region
                string oldPlanetName = pop.PlanetLocated.Name;
                pop.IsMigratingOffPlanet = false; // reset the flag
                pop.RegionLocated.PopsInTile.Remove(pop);
                pop.RegionLocated.EmigratedLastTurn += 1;
                pop.RegionLocated.PopsInTile.TrimExcess();
                pop.RegionLocationID = eligibleRegionList[regionChoice].ID;
                pop.RegionLocated.PopsInTile.Add(pop);
                pop.RegionLocated.ImmigratedLastTurn += 1;
                pop.PlanetHappinessLevel = 50; // reset planet happiness since they just moved
                Debug.Log("In " + gDataRef.GameDate.ToString("N1") +", a " + pop.PopClass.ToString() + " migrated from " + oldPlanetName + " to " + topPlanet.Name + ".");
                topPlanet.MigratePopsBetweenRegions(); // rerun migration to move the pop to a more suitable region
            }
            else
            {
                pop.UnrestLevel += .05f; // can't leave
                pop.PopSupport -= .05f; // and is pissed
            }                   
        }

        else
        {
            pop.UnrestLevel += .05f; // can't leave
            pop.PopSupport -= .05f; // and is pissed
        }
    }

    private float MigrationValueOfPlanet(PlanetData pData, Pops pop)
    {

        float planetValue = 0f;

        float FarmerJobsOnPlanet = 0f;
        float MinerJobsOnPlanet = 0f;
        float EngineerJobsOnPlanet = 0f;
        float FluxmenJobsOnPlanet = 0f;
        float AdministratorJobsOnPlanet = 0f;
        float ScientistJobsOnPlanet = 0f;

        float releventJobsOnPlanet = 0f;

        foreach (Region rData in pData.RegionList)
        {
            FarmerJobsOnPlanet += rData.FarmingLevel - rData.FarmsStaffed;
            MinerJobsOnPlanet += rData.MiningLevel - rData.MinesStaffed;
            EngineerJobsOnPlanet += rData.ManufacturingLevel - rData.FactoriesStaffed;
            FluxmenJobsOnPlanet += rData.HighTechLevel - rData.HighTechFacilitiesStaffed;
            AdministratorJobsOnPlanet += rData.GovernmentLevel - rData.GovernmentFacilitiesStaffed;
            ScientistJobsOnPlanet += rData.ScienceLevel - rData.LabsStaffed;
        }

        switch (pop.PopClass)
	    {
		case Pops.ePopClass.Scientist:
            releventJobsOnPlanet = ScientistJobsOnPlanet;
            break;
        case Pops.ePopClass.Farmer:
            releventJobsOnPlanet = FarmerJobsOnPlanet;
            break;
        case Pops.ePopClass.Miner:
            releventJobsOnPlanet = MinerJobsOnPlanet;
            break;
        case Pops.ePopClass.Engineer:
            releventJobsOnPlanet = EngineerJobsOnPlanet;
            break;
        case Pops.ePopClass.Fluxmen:
            releventJobsOnPlanet = FluxmenJobsOnPlanet;
            break;
        case Pops.ePopClass.Merchants:
            break;
        case Pops.ePopClass.Administrators:
            releventJobsOnPlanet = AdministratorJobsOnPlanet;
            break;
        case Pops.ePopClass.None:
            break;
        default:
            break;
	    }

        planetValue += (2500 - HelperFunctions.Formulas.MeasureDistanceBetweenSystems(pData.System, pop.PlanetLocated.System)) + pData.AdjustedBio + pData.BasePlanetValue + (releventJobsOnPlanet * 50);
        return planetValue;
    }

    private void CheckPlanetForSupplyDesignation(PlanetData pData)
    {

        if (pData.Rank == PlanetData.ePlanetRank.EstablishedColony || pData.Rank == PlanetData.ePlanetRank.NewColony)
        {
            if (!pData.IsSupplyPlanet && !pData.IsTradeHub) // only valid if not a trade hub and not a capital planet
            {
                List<PlanetData> supplyHubList = new List<PlanetData>(); // get list of supply hubs in province
                foreach (PlanetData planetData in pData.System.Province.PlanetList)
                {
                    if (planetData.IsTradeHub)
                    {
                        supplyHubList.Add(planetData);
                    }
                }

                UpdatePlanetTradeInfo(pData); // check for resources sent already
                // check each resource and if there is a shortfall and the planet has a surplus, chance that designation occurs
                foreach (PlanetData planetD in supplyHubList)
                {
                    float supplyPlanetChance = 0;
                    if (planetD.FoodDifference < 0 && pData.FoodExportAvailable > 0)
                    {
                        supplyPlanetChance += Mathf.Abs(planetD.FoodDifference) + pData.FoodExportAvailable;
                    }
                    if (planetD.EnergyDifference < 0 && pData.EnergyExportAvailable > 0)
                    {
                        supplyPlanetChance += Mathf.Abs(planetD.EnergyDifference) + pData.EnergyExportAvailable;
                    }
                    if (planetD.AlphaPreProductionDifference < 0 && pData.AlphaExportAvailable > 0)
                    {
                        supplyPlanetChance += Mathf.Abs(planetD.AlphaPreProductionDifference) + pData.AlphaExportAvailable;
                    }
                    if (planetD.RarePreProductionDifference < 0 && pData.RareExportAvailable > 0)
                    {
                        supplyPlanetChance += Mathf.Abs(planetD.RarePreProductionDifference) + pData.RareExportAvailable;
                    }
                    if (planetD.HeavyPreProductionDifference < 0 && pData.HeavyExportAvailable > 0)
                    {
                        supplyPlanetChance += Mathf.Abs(planetD.HeavyPreProductionDifference) + pData.HeavyExportAvailable;
                    }

                    if (supplyPlanetChance >= UnityEngine.Random.Range(0,200))
                    {
                        pData.IsSupplyPlanet = true;
                        pData.SupplyToPlanetID = planetD.ID;
                        break; // exit, the supply planet is found
                    }
                }

                if (pData.IsSupplyPlanet)
                {
                    CreateSupplyTrade(pData);
                }
            }
        }
    }

    private void CreateSupplyTrade(PlanetData pData)
    {
        TradeAgreement newTrade = new TradeAgreement();
        newTrade.ExportPlanetID = pData.ID;
        newTrade.Status = TradeAgreement.eAgreementStatus.Supply_Trade;
        newTrade.ImportPlanetID = pData.SupplyToPlanetID;

        if (pData.FoodExportAvailable > 0)
        {
            newTrade.FoodSent = pData.FoodExportAvailable / Constants.Constants.SupplyPlanetLow;
        }

        if (pData.EnergyExportAvailable > 0)
        {
            newTrade.EnergySent = pData.EnergyExportAvailable / Constants.Constants.SupplyPlanetLow;
        }

        if (pData.AlphaExportAvailable > 0)
        {
            newTrade.AlphaSent = pData.AlphaExportAvailable / Constants.Constants.SupplyPlanetLow;
        }

        if (pData.HeavyExportAvailable > 0)
        {
            newTrade.HeavySent = pData.HeavyExportAvailable / Constants.Constants.SupplyPlanetLow;
        }

        if (pData.RareExportAvailable > 0)
        {
            newTrade.RareSent = pData.RareExportAvailable / Constants.Constants.SupplyPlanetLow;
        }

        Debug.Log("Supply trade created from " + newTrade.ExportPlanet.Name + " to " + newTrade.ImportPlanet.Name + " for " + newTrade.TotalSent.ToString("N2") + " units, costing " + newTrade.Cost.ToString("N2") + ".");
        gDataRef.ActiveTradeAgreements.Add(newTrade);
    }

    private void UpdateResourceStockBalances()
    {
        foreach (PlanetData pData in galDataRef.GalaxyPlanetDataList)
        {
            pData.FoodStored += pData.FoodDifference;
            pData.EnergyStored += pData.EnergyDifference;
            pData.AlphaStored += pData.AlphaTotalDifference;
            pData.HeavyStored += pData.HeavyPreProductionDifference;
            pData.RareStored += pData.RarePreProductionDifference;
        }
    }

    private void UpdateTradeAgreements()
    {
        Civilization civ = gDataRef.CivList[0]; // human civ
       
        foreach (PlanetData pData in civ.PlanetList)
        {
            // step 1: check for shortfalls of each type
            if (pData.FoodDifference < 0)
            {
                LookForTradePartner(civ, "food", pData.ID); // step 2: see if a trade agreement can be created
            }

            if (pData.EnergyDifference < 0)
            {
                LookForTradePartner(civ, "energy", pData.ID); // step 2: see if a trade agreement can be created
            }

            if (pData.AlphaPreProductionDifference < 0)
            {
                LookForTradePartner(civ, "alpha", pData.ID); // step 2: see if a trade agreement can be created
            }

            if (pData.HeavyPreProductionDifference < 0)
            {
                LookForTradePartner(civ, "heavy", pData.ID); // step 2: see if a trade agreement can be created
            }

            if (pData.RarePreProductionDifference < 0)
            {
                LookForTradePartner(civ, "rare", pData.ID); // step 2: see if a trade agreement can be created
            }

            // step 2 - check to see if this planet will be a supply planet
            CheckPlanetForSupplyDesignation(pData);
            UpdatePlanetTradeInfo(pData);
        }
    }

    private void UpdatePlanetTradeInfo(PlanetData pData)
    {
        // update trade stats
        List<TradeAgreement> expList = new List<TradeAgreement>();
        List<TradeAgreement> impList = new List<TradeAgreement>();

        float totalImports = 0f;
        float totalExports = 0f;

        if (gDataRef.ActiveTradeAgreements.Exists(p => p.ExportPlanetID == pData.ID))
        {
            expList = gDataRef.ActiveTradeAgreements.FindAll(p => p.ExportPlanetID == pData.ID);
        }

        if (gDataRef.ActiveTradeAgreements.Exists(p => p.ImportPlanetID == pData.ID))
        {
            impList = gDataRef.ActiveTradeAgreements.FindAll(p => p.ImportPlanetID == pData.ID);
        }

        foreach (TradeAgreement t in expList)
        {
            totalExports += t.Cost;
        }
        pData.ExportRevenue = totalExports;

        foreach (TradeAgreement t in impList)
        {
            totalImports += t.Cost;
        }
        pData.ImportCosts = totalImports;

        float totalFood = 0f;
        foreach (TradeAgreement t in impList)
        {
            if (t.FoodSent > 0)
                totalFood += t.FoodSent;
        }
        pData.FoodImported = totalFood;

        totalFood = 0f;
        foreach (TradeAgreement t in expList)
        {
            if (t.FoodSent > 0)
                totalFood += t.FoodSent;
        }
        pData.FoodExported = totalFood;

        float totalEnergy = 0f;
        foreach (TradeAgreement t in impList)
        {
            if (t.EnergySent > 0)
                totalEnergy += t.EnergySent;
        }
        pData.EnergyImported = totalEnergy;

        totalEnergy = 0f;
        foreach (TradeAgreement t in expList)
        {
            if (t.EnergySent > 0)
                totalEnergy += t.EnergySent;
        }
        pData.EnergyExported = totalEnergy;  
    }
	
    private void LookForTradePartner(Civilization civ, string resource, string planetID)
    {    
        PlanetData needyPlanet = HelperFunctions.DataRetrivalFunctions.GetPlanet(planetID);
        SortedList sortedPlanetDistList = new SortedList();

        // create the sorted list
        foreach (PlanetData pData in needyPlanet.System.Province.PlanetList)
        {
            if (pData.ID != planetID && pData.IsInhabited)
            {
                float dist = HelperFunctions.Formulas.MeasureDistanceBetweenSystems(needyPlanet.System,pData.System);
                while (sortedPlanetDistList.ContainsKey(dist))
                {
                    dist += .001f;
                }
                sortedPlanetDistList.Add(dist,pData);
            }
        }

        // now check for general ability to create agreement (distance, starbase, support level of viceroy)
        foreach (DictionaryEntry de in sortedPlanetDistList)
        {
            PlanetData exportPlanet = (PlanetData)de.Value; // cast?
            
            if (exportPlanet.IsTradeAgreementValid(needyPlanet, resource)) // check for food agreement (is there enough food to send?)
            {
                TradeAgreement newTrade = new TradeAgreement();
                newTrade.ExportPlanetID = exportPlanet.ID;
                newTrade.Status = TradeAgreement.eAgreementStatus.Active;
                newTrade.ImportPlanetID = needyPlanet.ID;
                switch (resource)
                {
                    case "food" :
                        {
                            if (exportPlanet.FoodExportAvailable >= Math.Abs(needyPlanet.FoodDifference))
                                newTrade.FoodSent = Math.Abs(needyPlanet.FoodDifference);
                            else if (exportPlanet.FoodExportAvailable > 0)
                                newTrade.FoodSent = exportPlanet.FoodExportAvailable;
                            else
                                newTrade.FoodSent = 0; // no food avail, error check

                            if (newTrade.FoodSent >= exportPlanet.StarbaseCapacityRemaining)
                            {
                                newTrade.FoodSent = exportPlanet.StarbaseCapacityRemaining;
                            }
                            break;
                        }
                    case "energy":
                        {
                            if (exportPlanet.EnergyExportAvailable >= Math.Abs(needyPlanet.EnergyDifference))
                                newTrade.EnergySent = Math.Abs(needyPlanet.EnergyDifference);
                            else if (exportPlanet.EnergyExportAvailable > 0)
                                newTrade.EnergySent = exportPlanet.EnergyExportAvailable;
                            else
                                newTrade.EnergySent = 0;

                            if (newTrade.EnergySent >= exportPlanet.StarbaseCapacityRemaining)
                            {
                                newTrade.EnergySent = exportPlanet.StarbaseCapacityRemaining;
                            }
                            break;
                        }
                    case "alpha":
                        {
                            if (exportPlanet.AlphaExportAvailable >= Math.Abs(needyPlanet.AlphaPreProductionDifference))
                                newTrade.AlphaSent = Math.Abs(needyPlanet.AlphaPreProductionDifference);
                            else if (exportPlanet.AlphaExportAvailable > 0)
                                newTrade.AlphaSent = exportPlanet.AlphaExportAvailable;
                            else
                                newTrade.AlphaSent = 0;

                            if (newTrade.AlphaSent >= exportPlanet.StarbaseCapacityRemaining)
                            {
                                newTrade.AlphaSent = exportPlanet.StarbaseCapacityRemaining;
                            }
                            break;
                        }
                    case "heavy":
                        {
                            if (exportPlanet.HeavyExportAvailable >= Math.Abs(needyPlanet.HeavyPreProductionDifference))
                                newTrade.HeavySent = Math.Abs(needyPlanet.HeavyPreProductionDifference);
                            else if (exportPlanet.HeavyExportAvailable > 0)
                                newTrade.HeavySent = exportPlanet.HeavyExportAvailable;
                            else
                                newTrade.HeavySent = 0;

                            if (newTrade.HeavySent >= exportPlanet.StarbaseCapacityRemaining)
                            {
                                newTrade.HeavySent = exportPlanet.StarbaseCapacityRemaining;
                            }
                            break;
                        }
                    case "rare":
                        {
                            if (exportPlanet.RareExportAvailable >= Math.Abs(needyPlanet.RarePreProductionDifference))
                                newTrade.RareSent = Math.Abs(needyPlanet.RarePreProductionDifference);
                            else if (exportPlanet.RareExportAvailable > 0)
                                newTrade.RareSent = exportPlanet.RareExportAvailable;
                            else
                                newTrade.RareSent = 0;

                            //starbase capacity check
                            if (newTrade.RareSent >= exportPlanet.StarbaseCapacityRemaining)
                            {
                                newTrade.RareSent = exportPlanet.StarbaseCapacityRemaining;
                            }
                            break;
                        } 
                    default:
                        break;
                }

                if (newTrade.TotalSent >= 0.01) // minimum threshold amount for trades, also check for capacity of the trade
                {
                    Debug.Log("Trade of " + newTrade.TotalSent.ToString("N2") + " " + resource + " to " + newTrade.ImportPlanet.Name + " from " + newTrade.ExportPlanet.Name + " over " + newTrade.Distance.ToString("N1") + " LY for $" + newTrade.Cost.ToString("N2") + " with a " + newTrade.CostModifier.ToString("N1") + " modifier.");
                    gDataRef.ActiveTradeAgreements.Add(newTrade); // add the new agreement
                }

                // update the trade info for each planet
                UpdatePlanetTradeInfo(needyPlanet);
                UpdatePlanetTradeInfo(exportPlanet);            
            }
            
        }
        
    }
	
}
