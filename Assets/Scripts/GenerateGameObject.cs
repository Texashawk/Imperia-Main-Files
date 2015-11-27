using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System;
using StellarObjects; // using classes generated for stellar objects
using PlanetObjects; // for tiles
using CharacterObjects; // for characters
using CivObjects; // for civs
using HelperFunctions; // helper functions

public class GenerateGameObject
{
    // random name lists
    
    private static List<Color> civColorList = new List<Color>(); // stores colors for selecting civ colors (no duplicates)

    // accessor variables
    private static GalaxyData galaxyDataRef;
    private static GlobalGameData gameDataRef;
    private static GraphicAssets graphicsDataRef;


    // static data tables for generation
    private static List<PlanetGenerationData> planetGenerationDataList = new List<PlanetGenerationData>();
    
    public static List<RegionTypeData> regionTypeDataList = new List<RegionTypeData>();
    //public static List<PlanetTraits> planetTraitDataList = new List<PlanetTraits>();
   
    // consts for unit generation
    private const int SpritesPerPlanetType = 43; // planet sprites per type
    private const int SpritesPerNebulaType = 5; // as on the tin
    private const int SpritesPerRingType = 5; // for planet rings
    private const int SpritesPerBeltType = 5; // sprites for ice belts, dust rungs, etc
    private const int MaxPlanetsPerSystem = 5; // the max planets per system

    // consts for pop generation
    private const int ChanceForPopOnStandardColony = 50;
    private const int ChanceForPopOnSystemCapital = 60;
    private const int ChanceForPopOnProvinceCapital = 70;
    private const int ChanceForPopOnCivCapital = 80;
    private const int ChanceForPrimaryCiv = 85; // chance that the pop generated is from the owning civ
    private const int ChanceForClosestCiv = 7;
    private const int ChanceForSecondClosestCiv = 5;
    private const int ChanceForOtherCiv = 3;
    private const int skillNeedMultiple = 6; // balanced - do not touch!

    // generation consts
    const int ChancePopIsYoungAdult = 15;
    const int ChancePopIsWorker = 60;
    const int ChancePopIsRetired = 85;
    const float YoungAdultSkillModifier = .6f;
    const float WorkerSkillModifier = 1.1f;
    const float RetiredSkillModifier = .8f;

    // chance per planet trait
    private const int TraitChanceWith0Traits = 80;
    private const int TraitChanceWith1Traits = 40;
    private const int TraitChanceWith2Traits = 10;

    public const int MultiSystemEmpireRange = 600; // ranges for AI system claims initially
    public const int MultiRegionEmpireRange = 1500;

    private static void PopulateCivColorList()
    {
        civColorList.Add(Color.blue);
        civColorList.Add(Color.cyan);
        civColorList.Add(Color.green);
        civColorList.Add(Color.magenta);
        civColorList.Add(Color.red);
        civColorList.Add(Color.white);
        civColorList.Add(Color.yellow);
        civColorList.Add(new Color(.4f, .2f, .6f)); // royal purple
        civColorList.Add(new Color(1f, .4f, .0f)); // orange
    }

    public static Civilization CreateNewCivilization()
    {
        List<String> civNameList = DataManager.civNameList;
        // populate the color list if needed
        if (civColorList.Count == 0)
            PopulateCivColorList();

        Civilization newCiv = new Civilization();
        int dieRoll = 0;
        galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>(); // access galaxy data
        gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>(); // access galaxy data

        // Step 1: Generate type of civ
        dieRoll = UnityEngine.Random.Range(1, 7);
        newCiv.Type = (Civilization.eCivType)dieRoll;  // cast to enum

        // Step 2: Generate ID of civ
        newCiv.ID = "CIV" + gameDataRef.CivList.Count.ToString("N0");

        // Step 2: Generate name of civ
        string primaryName = "";
        string surName = "";

        if (civNameList.Count > 0)
        {
            var nameIndex = UnityEngine.Random.Range(0, civNameList.Count);
            primaryName = civNameList[nameIndex];
            civNameList.RemoveAt(nameIndex);
            civNameList.TrimExcess();
        }
        else
            primaryName = "GenericName";

        var surNameIndex = UnityEngine.Random.Range(0, DataManager.civSurNameList.Count);
        surName = DataManager.civSurNameList[surNameIndex];

        newCiv.Name = primaryName + " " + surName;

        // Step 3: Generate other base stats (treasury, size, etc)      
      
        // size/range
        newCiv.Range = 40; // to start with
        int size = UnityEngine.Random.Range(0, 100);

        // adjust size/other ratings for civ type
        switch(newCiv.Type)
        {
            case Civilization.eCivType.Confederation :
                {
                    size += 55;
                    break;
                }
            case Civilization.eCivType.MinorEmpire:
                {
                    size += 40;
                    break;
                }
            case Civilization.eCivType.Satrapy:
                {
                    size += 20;
                    break;
                }
            case Civilization.eCivType.BrokenCivilization:
                {
                    size -= 30;
                    break;
                }
            case Civilization.eCivType.Pirates:
                {
                    size -= 15;
                    break;
                }
        }

        // add a empire type mod here
        if (size < 40)
            newCiv.Size = Civilization.eCivSize.SinglePlanet;
        else if (size < 70)
            newCiv.Size = Civilization.eCivSize.Local;
        else if (size < 90)
        {
            newCiv.Size = Civilization.eCivSize.Minor;
            newCiv.Range = UnityEngine.Random.Range(MultiSystemEmpireRange - 200, MultiSystemEmpireRange + 200);
        }
        else
        {
            newCiv.Size = Civilization.eCivSize.Major;
            newCiv.Range = UnityEngine.Random.Range(MultiRegionEmpireRange - 400, MultiRegionEmpireRange + 400);
        }

        // skill ratings
        newCiv.FarmingBaseRating = UnityEngine.Random.Range(70, 100) - (int)newCiv.Type * 10;
        newCiv.MiningBaseRating = UnityEngine.Random.Range(50, 90) - (int)newCiv.Type * 10;
        newCiv.ScienceBaseRating = UnityEngine.Random.Range(0, 50) + (int)newCiv.Type * 10;
        newCiv.HighTechBaseRating = UnityEngine.Random.Range(0, 40) + (int)newCiv.Type * 10;
        newCiv.ManufacturingBaseRating = UnityEngine.Random.Range(5, 50) + (int)newCiv.Type * 10;

        // tolerance
        newCiv.PlanetMinTolerance = UnityEngine.Random.Range(40, 60); // sets the base minimum habitable world a civilization is willing to tolerate

        // province size
        if (newCiv.Size == Civilization.eCivSize.Major)
        {
            newCiv.CivMaxProvinceSize = UnityEngine.Random.Range(1, 6); // sets a province size between 1 and 5 systems
            newCiv.AdminRating = UnityEngine.Random.Range(5, 21); // generates the civ's base admin rating (how efficient they are in adminstering provinces
        }
        else
        {
            newCiv.CivMaxProvinceSize = 0; // no provinces can be created; the civ is essentially one province
            newCiv.AdminRating = 1;
        }

        // admin rating

        // Step 4: Determine planet of origin
        retryPlanet: // beginning of retry loop
        PlanetData civPlanet = new PlanetData();

        dieRoll = UnityEngine.Random.Range(0, galaxyDataRef.GalaxyPlanetDataList.Count); // find the planets in the quadrant
        civPlanet = galaxyDataRef.GalaxyPlanetDataList[dieRoll];
        
        if (galaxyDataRef.GalaxyPlanetDataList[dieRoll].AdjustedBio < newCiv.PlanetMinTolerance) // if the bio is too low, throw it out
            goto retryPlanet;

        if (gameDataRef.CivList.Count > 0)
        {
            foreach (Civilization civ in gameDataRef.CivList)
            {
                List<StarData> populatedHomeSystems = new List<StarData>();
                populatedHomeSystems = HelperFunctions.DataRetrivalFunctions.GetCivSystemList(civ);

                if (civPlanet.ID == civ.CapitalPlanetID)  // check for capital world
                    goto retryPlanet;
                if (civ.PlanetIDList.Exists(p => p == civPlanet.ID)) // then all other worlds
                    goto retryPlanet;
                if (newCiv.ID != civ.ID)
                    if (populatedHomeSystems.Exists(p => p.ID == civPlanet.SystemID)) // check for systems that other civs have claimed
                        goto retryPlanet;             
            }

            // assign a name for the civ's planet
            if (DataManager.planetNameList.Count > 0)
            {
                var nameIndex = UnityEngine.Random.Range(0, DataManager.planetNameList.Count);
                civPlanet.Name = DataManager.planetNameList[nameIndex];
                DataManager.planetNameList.RemoveAt(nameIndex);
                DataManager.planetNameList.TrimExcess();
            }
            else
                civPlanet.Name = "GenericName";

            // set as capital and send to assign pops, developments, etc.
            newCiv.CapitalPlanetID = civPlanet.ID;          
            ClaimCivPlanet(civPlanet, newCiv);
            civPlanet.Rank = PlanetData.ePlanetRank.ImperialCapital;
            newCiv.PlanetIDList.Add(civPlanet.ID); // add the planet
        }

        // Step 5: Determine additional planets
        if (newCiv.Size != Civilization.eCivSize.SinglePlanet)
            ClaimPlanetsForCiv(newCiv);

        // Step 5: Generate color of civ
        retryColor: // beginning of retry loop
        Color civColor = new Color();

        civColor = new Color(UnityEngine.Random.Range(.01f, .99f), UnityEngine.Random.Range(.01f, .99f), UnityEngine.Random.Range(.01f, .99f)); // select a random color
        if (gameDataRef.CivList.Count > 0)
        {
            foreach (Civilization civ in gameDataRef.CivList)
            {
                if (civColor == civ.Color)
                    goto retryColor;
            }
            newCiv.Color = civColor;
        }

        return newCiv;
    }

    public static void ClaimPlanetsForCiv(Civilization newCiv) // this 'flags' planets to seed with pops in the next step
    {
        gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
        galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();

        float civMaxDistance = newCiv.Range;
        PlanetData homePlanet = galaxyDataRef.GalaxyPlanetDataList.Find(p => p.ID == newCiv.CapitalPlanetID);
        StarData homeStar = galaxyDataRef.GalaxyStarDataList.Find(p => p.ID == homePlanet.SystemID);

        List<StarData> eligibleSystems = new List<StarData>();
        foreach (StarData star in galaxyDataRef.GalaxyStarDataList)
        {
            float distance = HelperFunctions.Formulas.MeasureDistanceBetweenSystems(star, homeStar);
            if ( distance <= civMaxDistance) // must be within range and also include the home system
                eligibleSystems.Add(star); // add systems that are close to the home star
        }

        // 
        // now check planets in those systems to see if they can be owned
        foreach (StarData potStar in eligibleSystems)
        {
            foreach(PlanetData pData in potStar.PlanetList)
            {
                if (pData.AdjustedBio >= newCiv.PlanetMinTolerance)
                {
                    bool planetEligible = true;
                    foreach (Civilization civ in gameDataRef.CivList) // check each civ to make sure they don't own the planet
                    {
                        if (civ.PlanetIDList.Exists (p => p == pData.ID))
                            planetEligible = false;
                        if (pData.ID == homePlanet.ID)
                            planetEligible = false;
                        if (pData.AdjustedMaxHabitableTiles == 0) // if after traits there are no tiles to put people on, it's not a good planet!
                            planetEligible = false;
                    }
                    if (planetEligible) // if they don't, then claim!
                        ClaimCivPlanet(pData,newCiv);
                }
            }
        }
    }

    public static void ClaimCivPlanet(PlanetData pData, Civilization newCiv)
    {
        // assign a name for the civ's planet
        if (DataManager.planetNameList.Count > 0)
        {
            var nameIndex = UnityEngine.Random.Range(0, DataManager.planetNameList.Count);
            pData.Name = DataManager.planetNameList[nameIndex];
            DataManager.planetNameList.RemoveAt(nameIndex);
            DataManager.planetNameList.TrimExcess();
        }
        else
            pData.Name = "GenericName";

        pData.Rank = PlanetData.ePlanetRank.EstablishedColony;
        newCiv.PlanetIDList.Add(pData.ID); // add the planet
    }

    public static void AddStockpilesToPlanet(PlanetData pData)
    {
        if (pData.FoodDifference > 0)
        {
            pData.FoodStored = UnityEngine.Random.Range(20, 100) * pData.FoodDifference; // number of months of storage based on food flow
        }
        else
            pData.FoodStored = UnityEngine.Random.Range(0, 5000);

        if (pData.AlphaPreProductionDifference > 0)
        {
            pData.AlphaStored = UnityEngine.Random.Range(20, 100) * pData.AlphaPreProductionDifference; // number of months of storage based on food flow
        }
        else
            pData.AlphaStored = UnityEngine.Random.Range(0, 3000);

        if (pData.HeavyPreProductionDifference > 0)
        {
            pData.HeavyStored = UnityEngine.Random.Range(20, 100) * pData.HeavyPreProductionDifference; // number of months of storage based on food flow
        }
        else
            pData.HeavyStored = UnityEngine.Random.Range(0, 500);

        if (pData.RarePreProductionDifference > 0)
        {
            pData.RareStored = UnityEngine.Random.Range(20, 100) * pData.RarePreProductionDifference; // number of months of storage based on food flow
        }
        else
            pData.RareStored = UnityEngine.Random.Range(0, 300);

        if (pData.EnergyDifference > 0)
        {
            pData.EnergyStored = UnityEngine.Random.Range(20, 100) * pData.EnergyDifference; // number of months of storage based on food flow
        }
        else
            pData.EnergyStored = UnityEngine.Random.Range(0, 5000);

    }

    public static void AddTradeInfrastructureToPlanet(PlanetData pData)
    {
        // determine starbase level
        if (pData.Rank == PlanetData.ePlanetRank.ImperialCapital)
            pData.StarbaseLevel = UnityEngine.Random.Range(3, 5);
        else if (pData.Rank == PlanetData.ePlanetRank.ProvinceCapital)
            pData.StarbaseLevel = UnityEngine.Random.Range(1, 4);
        else if (pData.Rank == PlanetData.ePlanetRank.SystemCapital)
            pData.StarbaseLevel = UnityEngine.Random.Range(0, 3);
        else if (pData.Rank == PlanetData.ePlanetRank.EstablishedColony)
            pData.StarbaseLevel = UnityEngine.Random.Range(0, 2);
        else
            pData.StarbaseLevel = 0;

        // determine whether a trade hub
        if (pData.StarbaseLevel > 0 && pData.Rank != PlanetData.ePlanetRank.EstablishedColony)
        {
            int baseChance = pData.StarbaseLevel * 20;
            if (UnityEngine.Random.Range(0, 100) >= baseChance)
                pData.IsTradeHub = true;
        }
    }

    public static void AddDevelopmentToPlanet(PlanetData pData)
    {
        // populate the planet by adding industry
        foreach (Region rData in pData.RegionList)
        {
            int farmChance = 0;
            int mineChance = 0;
            int adminChance = 0;
            int scienceChance = 0;
            int factoryChance = 0;
            int highTechChance = 0;

            int minDevelopmentLevel = rData.MaxDevelopmentLevel - 10;
            if (minDevelopmentLevel <= 0)
            {
                minDevelopmentLevel = 0;
            }

            int maxDevelopmentAmount = UnityEngine.Random.Range(minDevelopmentLevel, rData.MaxDevelopmentLevel);
            for (int x = 0; x < maxDevelopmentAmount; x++)
            {
                farmChance = 0;
                mineChance = 0;
                adminChance = 0;
                scienceChance = 0;
                factoryChance = 0;
                highTechChance = 0;

                int govAdjust = 20;
                // basic adjustment for capitals to have more government developments
                if (HelperFunctions.DataRetrivalFunctions.GetPlanet(rData.PlanetLocationID).Rank == PlanetData.ePlanetRank.SystemCapital)
                    govAdjust = 30;
                if (HelperFunctions.DataRetrivalFunctions.GetPlanet(rData.PlanetLocationID).Rank == PlanetData.ePlanetRank.ProvinceCapital)
                    govAdjust = 45;
                if (HelperFunctions.DataRetrivalFunctions.GetPlanet(rData.PlanetLocationID).Rank == PlanetData.ePlanetRank.ImperialCapital)
                    govAdjust = 60;

                // adjust by region characteristics, as well as what is already there
                if (rData.FarmingLevel == 0)
                {
                    farmChance += 40; // try to get at least one farm
                }

                if (rData.HighTechLevel == 0)
                {
                    highTechChance += 30; // try to get at least one energy station
                }
                // normalize to have a roughly equal amount of mines and factories
                if (rData.MiningLevel < rData.ManufacturingLevel)
                {
                    mineChance += 5 * (rData.ManufacturingLevel - rData.MiningLevel);
                }

                if (rData.MiningLevel > rData.ManufacturingLevel)
                {
                    factoryChance += 5 * (rData.MiningLevel - rData.ManufacturingLevel);
                }

                farmChance += 23 + (rData.BioRating / 3) + UnityEngine.Random.Range(0, 10) + rData.MiningLevel + rData.ManufacturingLevel + rData.GovernmentLevel + rData.HighTechLevel - rData.FarmingLevel;
                mineChance += 15 + ((rData.AlphaRating + rData.HeavyRating) / 4) + (rData.RareRating / 4) + rData.FarmingLevel + rData.ManufacturingLevel - rData.MiningLevel + UnityEngine.Random.Range(0, 10);
                adminChance += govAdjust + UnityEngine.Random.Range(5, 30) + rData.FarmingLevel + rData.MiningLevel + rData.ManufacturingLevel + rData.HighTechLevel;
                scienceChance += 38 + rData.FarmingLevel + rData.MiningLevel + rData.ManufacturingLevel + UnityEngine.Random.Range(0, 10) + rData.FarmingLevel;
                factoryChance += 15 + ((rData.AlphaRating + rData.HeavyRating) / 4) + rData.MiningLevel + rData.FarmingLevel + UnityEngine.Random.Range(0, 20);
                highTechChance += 30 + (rData.EnergyRating / 3) + rData.FarmingLevel + rData.MiningLevel + rData.ManufacturingLevel + rData.GovernmentLevel + UnityEngine.Random.Range(5, 10);

                // generate a sorted list
                Dictionary<string,int> developmentDictionary = new Dictionary<string,int>();
                developmentDictionary.Add("Farm",farmChance);
                developmentDictionary.Add("Mine",mineChance);
                developmentDictionary.Add("Admin",adminChance);
                developmentDictionary.Add("Science",scienceChance);
                developmentDictionary.Add("Factory",factoryChance);
                developmentDictionary.Add("HighTech",highTechChance);

                List<KeyValuePair<string, int>> SortedDevelopments = developmentDictionary.ToList();
                SortedDevelopments.Sort((firstPair, nextPair) =>
                    {
                        return firstPair.Value.CompareTo(nextPair.Value);
                    }
                );

                if (SortedDevelopments.Last().Key == "Farm")
                    rData.FarmingLevel += 1;
                else if (SortedDevelopments.Last().Key == "Mine")
                    rData.MiningLevel += 1;
                else if (SortedDevelopments.Last().Key == "Admin")
                    rData.GovernmentLevel += 1;
                else if (SortedDevelopments.Last().Key == "Science")
                    rData.ScienceLevel += 1;
                else if (SortedDevelopments.Last().Key == "Factory")
                    rData.ManufacturingLevel += 1;
                else if (SortedDevelopments.Last().Key == "HighTech")
                    rData.HighTechLevel += 1;
            }
        }
    }

    public static void AddPopsToPlanet(PlanetData pData, Civilization civ)
    {
        // add pops to each region on a planet
        foreach (Region rData in pData.RegionList)
        {
            for (int x = 0; x < rData.MaxSafePopulationLevel; x++ )
            {
                float generationChance = 0;
                int generationTarget = 0;

                // determine base chance for pop generation
                generationChance = UnityEngine.Random.Range(0, 135);
                
                if (pData.Rank == PlanetData.ePlanetRank.EstablishedColony)
                    generationTarget = ChanceForPopOnStandardColony;
                if (pData.Rank == PlanetData.ePlanetRank.SystemCapital)
                    generationTarget = ChanceForPopOnSystemCapital;
                if (pData.Rank == PlanetData.ePlanetRank.ProvinceCapital)
                    generationTarget = ChanceForPopOnProvinceCapital;
                if (pData.Rank == PlanetData.ePlanetRank.ImperialCapital)
                    generationTarget = ChanceForPopOnCivCapital;

                generationChance -= (float)(rData.HabitatationInfrastructureLevel / 2f);

                // adjust for existing infrastructure
                generationChance += rData.MaxSafePopulationLevel - (rData.MaxSafePopulationLevel - rData.PopsInTile.Count);
                if (rData.PopsInTile.Count == 0 && rData.TotalDevelopmentLevel > 0)
                {
                    generationChance = 0; // always add at least one pop!
                }

                if (rData.TotalDevelopmentLevel == 0)
                {
                    break;
                }
                   
                if (generationChance <= generationTarget)
                {
                    GenerateNewPop(civ, rData, false); // add a new pop to the region
                }
            }             
        }
    }

    public static Character GenerateNewCharacter(Character.eRole cRole, string empireID)
    {
        Character newChar = new Character();

        // Step 0: Initialize the character
        newChar.CivID = empireID;
        newChar.Role = cRole;

        // Step 1: Generate basic type (sex, age, health, ID)
        int sex = UnityEngine.Random.Range(0, 2);
        if (sex == 0)
        {
            newChar.Gender = Character.eSex.Female;
        }
        else
        {
            newChar.Gender = Character.eSex.Male;
        }

        int lifeForm = UnityEngine.Random.Range(0, 20);
        if (lifeForm < 12)
        {
            newChar.Lifeform = Character.eLifeformType.Human;
        }
        else if (lifeForm < 14)
        {
            newChar.Lifeform = Character.eLifeformType.Human_Immobile;
        }
        else if (lifeForm < 16)
        {
            newChar.Lifeform = Character.eLifeformType.Hybrid;
        }
        else if (lifeForm < 18)
        {
            newChar.Lifeform = Character.eLifeformType.Machine;
        }
        else if (lifeForm < 20)
        {
            newChar.Lifeform = Character.eLifeformType.AI;
        }
        else
            newChar.Lifeform = Character.eLifeformType.Resuscitated;

        // 1A: determine age based on lifeform type
        int charTypeMax = 0;
        if (newChar.Lifeform == Character.eLifeformType.Human || newChar.Lifeform == Character.eLifeformType.Human_Immobile)
            charTypeMax = 90;
        else if (newChar.Lifeform == Character.eLifeformType.Resuscitated)
            charTypeMax = 200;
        else if (newChar.Lifeform == Character.eLifeformType.Hybrid)
            charTypeMax = 140;
        else if (newChar.Lifeform == Character.eLifeformType.Machine || newChar.Lifeform == Character.eLifeformType.AI)
            charTypeMax = 500;
        newChar.Age = UnityEngine.Random.Range(18, charTypeMax); // generate ages between 18 and 80

        

        // 1B: set health type depending on type of lifeform
        if (newChar.Lifeform == Character.eLifeformType.Human || newChar.Lifeform == Character.eLifeformType.Human_Immobile || newChar.Lifeform == Character.eLifeformType.Resuscitated)
        {
            int healthRating = newChar.Age + UnityEngine.Random.Range(-50, 50);

            if (healthRating <= 30)
                newChar.Health = Character.eHealth.Perfect;
            else if (healthRating <= 50)
                newChar.Health = Character.eHealth.Fine;
            else if (healthRating <= 70)
                newChar.Health = Character.eHealth.Healthy;
            else if (healthRating <= 90)
                newChar.Health = Character.eHealth.Impaired;
            else if (healthRating <= 120)
                newChar.Health = Character.eHealth.Unhealthy;
            else
                newChar.Health = Character.eHealth.Bedridden;
        }
        else
        {
            int healthRating = UnityEngine.Random.Range(0, 100);
            if (healthRating <= 60)
                newChar.Health = Character.eHealth.Perfect;
            else if (healthRating <= 70)
                newChar.Health = Character.eHealth.Functional;
            else if (healthRating <= 80)
                newChar.Health = Character.eHealth.Malfunctioning;
            else if (healthRating <= 95)
                newChar.Health = Character.eHealth.Seriously_Damaged;
            else
                newChar.Health = Character.eHealth.Critically_Damaged;
        }

        newChar.ID = "CHA" + UnityEngine.Random.Range(0, 1000000);

        // Step 2: Create name
        if (newChar.Gender == Character.eSex.Female)
        {
            var nameIndex = UnityEngine.Random.Range(0, DataManager.characterFemaleFirstNameList.Count);
            newChar.Name = DataManager.characterFemaleFirstNameList[nameIndex];
        }
        else if (newChar.Gender == Character.eSex.Male)
        {
            var nameIndex = UnityEngine.Random.Range(0, DataManager.characterMaleFirstNameList.Count);
            newChar.Name = DataManager.characterMaleFirstNameList[nameIndex];
        }
        else
        {
            newChar.Name = "GenericName";
        }

        if (newChar.Lifeform == Character.eLifeformType.Machine) // add 'r' to the beginning for robot
        {
            newChar.Name = "R. " + newChar.Name;
        }

        // Step 3: Generate base stats
        newChar.Intelligence = UnityEngine.Random.Range(20, 90); // min and max intelligence, add bonus for AIs
        if (newChar.Lifeform == Character.eLifeformType.AI || newChar.Lifeform == Character.eLifeformType.Machine)
            newChar.Intelligence += UnityEngine.Random.Range(15, 60);
        newChar.Honor = UnityEngine.Random.Range(5, 95); // younger characters tend to be more loyal base
        newChar.Passion = UnityEngine.Random.Range(5, 95);
        if (newChar.Lifeform == Character.eLifeformType.AI || newChar.Lifeform == Character.eLifeformType.Machine)
            newChar.Passion = 0; // machines are emotionless
        newChar.Drive = UnityEngine.Random.Range(5, 95);
        if (newChar.Lifeform == Character.eLifeformType.AI || newChar.Lifeform == Character.eLifeformType.Machine)
            newChar.Drive = 95; // machines never stop
        newChar.Charm = UnityEngine.Random.Range(-95, 95);
        newChar.Empathy = UnityEngine.Random.Range(-95, 95);
        newChar.Caution = UnityEngine.Random.Range(-95, 95); // low end - risk taker, high end - security
        newChar.Piety = UnityEngine.Random.Range(5, 95);
        newChar.Morality = UnityEngine.Random.Range(-95, 95);
        newChar.BaseInfluence = UnityEngine.Random.Range(0, 15); // base influence before calculation
        newChar.Admin = UnityEngine.Random.Range(1,5); // AP points
        if (newChar.CivID == "CIV0")
            newChar.IntelLevel = UnityEngine.Random.Range(0, 5) + (newChar.Age / 25); // older characters are more well-known
        else
            newChar.IntelLevel = 0; // base no knowledge of other civ's characters

        // Step 3a: Generate AI tendencies
        newChar.AdminTendency = UnityEngine.Random.Range(-90, 90);
        newChar.BudgetTendency = UnityEngine.Random.Range(-90, 90);
        newChar.ChangeTendency = UnityEngine.Random.Range(-90, 90);
        newChar.CourageTendency = UnityEngine.Random.Range(-90, 90);
        newChar.DiplomacyTendency = UnityEngine.Random.Range(-90, 90);
        newChar.GluttonyTendency = UnityEngine.Random.Range(-90, 90);
        newChar.GoalFocusTendency = UnityEngine.Random.Range(-90, 90);
        newChar.GoalStabilityTendency = UnityEngine.Random.Range(-90, 90);
        newChar.LearningTendency = UnityEngine.Random.Range(-90, 90);
        newChar.PopsTendency = UnityEngine.Random.Range(-90, 90);
        newChar.ReserveTendency = UnityEngine.Random.Range(-90, 90);
        newChar.ScienceTendency = UnityEngine.Random.Range(-90, 90);
        newChar.TaxTendency = UnityEngine.Random.Range(-90, 90);
        newChar.TraderTendency = UnityEngine.Random.Range(-90, 90);
        newChar.TravelTendency = UnityEngine.Random.Range(-90, 90);

        // Step 3b: Generate traits
        int traitMax = gameDataRef.CharacterTraitList.Count - 1;
        int traitMin = 0;
        int totalTraits = UnityEngine.Random.Range(1, Constants.Constants.MaxCharTraits);
        int traitCount = 0;
        while (traitCount < totalTraits)
        {
            CharacterTrait tempTrait = new CharacterTrait();
            int traitChoice = UnityEngine.Random.Range(traitMin,traitMax); // choose a trait from the data list
            tempTrait = gameDataRef.CharacterTraitList[traitChoice]; 

            if (!newChar.Traits.Exists(p=> p.ID == tempTrait.ID)) // check trait against being already present and that it does not have an opposite trait ID already present
            {
                if (!newChar.Traits.Exists(p=> p.OppositeTraitID == tempTrait.ID))
                {
                    newChar.Traits.Add(tempTrait);
                    traitCount += 1; // only add to count if a valid trait is picked
                }
            }
        }

        // Step 3a: Adjust tendencies based on traits
        foreach (CharacterTrait trait in newChar.Traits)
        {
            if (trait.AdminTendency != 0)
            {
                newChar.AdminTendency = trait.AdminTendency;
            }

            if (trait.BudgetTendency != 0)
            {
                newChar.BudgetTendency = trait.BudgetTendency;
            }

            if (trait.ChangeTendency != 0)
            {
                newChar.ChangeTendency = trait.BudgetTendency;
            }

            if (trait.CourageTendency != 0)
            {
                newChar.CourageTendency = trait.CourageTendency;
            }

            if (trait.DiplomacyTendency != 0)
            {
                newChar.DiplomacyTendency = trait.DiplomacyTendency;
            }

            if (trait.GluttonyTendency != 0)
            {
                newChar.GluttonyTendency = trait.GluttonyTendency;
            }

            if (trait.GoalFocusTendency != 0)
            {
                newChar.GoalFocusTendency = trait.GoalFocusTendency;
            }

            if (trait.GoalStabilityTendency != 0)
            {
                newChar.GoalStabilityTendency = trait.GoalStabilityTendency;
            }

            if (trait.LearningTendency != 0)
            {
                newChar.LearningTendency = trait.LearningTendency;
            }

            if (trait.PopsTendency != 0)
            {
                newChar.PopsTendency = trait.PopsTendency;
            }

            if (trait.ReserveTendency != 0)
            {
                newChar.ReserveTendency = trait.ReserveTendency;
            }

            if (trait.ScienceTendency != 0)
            {
                newChar.ScienceTendency = trait.ScienceTendency;
            }

            if (trait.TaxTendency != 0)
            {
                newChar.TaxTendency = trait.TaxTendency;
            }

            if (trait.TraderTendency != 0)
            {
                newChar.TraderTendency = trait.TraderTendency;
            }

            if (trait.TravelerTendency != 0)
            {
                newChar.TravelTendency = trait.TravelerTendency;
            }

            if (trait.AdminTendency != 0)
            {
                newChar.AdminTendency = trait.AdminTendency;
            }

            if (trait.TrustTendency != 0)
            {
                newChar.TrustTendency = trait.TrustTendency;
            }

            if (trait.WealthTendency != 0)
            {
                newChar.WealthTendency = trait.WealthTendency;
            }


        }

        // Step 4: Generate picture ID
        AssignCharacterPictureID(newChar);

        // Step 5: Generate base history
        string creationVerb = "";
        if (newChar.Lifeform == Character.eLifeformType.Human || newChar.Lifeform == Character.eLifeformType.Human_Immobile || newChar.Lifeform == Character.eLifeformType.Hybrid)
            creationVerb = "born";
        else if (newChar.Lifeform == Character.eLifeformType.Resuscitated)
            creationVerb = "reborn";
        else
            creationVerb = "built and programmed";
        newChar.History += "In " + (gameDataRef.GameDate - newChar.Age).ToString("G0") + ", the esteemed " + newChar.Name + " was " + creationVerb + ". "; 

        return newChar;
        
    }

    public static void AssignCharacterPictureID(Character cData)
    {
        graphicsDataRef = GameObject.Find("GameManager").GetComponent<GraphicAssets>();
        string age = "";
        string faction = "";
        string strSex = "";
        int index = 0;

        if (cData.Age < 10)
            age = "C";
        else if (cData.Age < 40)
            age = "Y";
        else if (cData.Age < 60)
            age = "M";
        else
            age = "O";

        if (cData.Lifeform == Character.eLifeformType.AI)
            age = "I";

        faction = "Imp"; // default for now until factions are added

        if (cData.Gender == Character.eSex.Female)
            strSex = "F";
        else if (cData.Gender == Character.eSex.Male)
            strSex = "M";
        else
            strSex = "O";

        // get how many portraits are in each folder subtype
        int femaleChildCount = graphicsDataRef.CharacterList.FindAll(p => p.name.Substring(3, 2) == "FC").Count;
        int femaleYoungCount = graphicsDataRef.CharacterList.FindAll(p => p.name.Substring(3, 2) == "FY").Count;
        int femaleMiddleAgeCount = graphicsDataRef.CharacterList.FindAll(p => p.name.Substring(3, 2) == "FM").Count;
        int femaleOldCount = graphicsDataRef.CharacterList.FindAll(p => p.name.Substring(3, 2) == "FO").Count;

        int maleChildCount = graphicsDataRef.CharacterList.FindAll(p => p.name.Substring(3, 2) == "MC").Count;
        int maleYoungCount = graphicsDataRef.CharacterList.FindAll(p => p.name.Substring(3, 2) == "MY").Count;
        int maleMiddleAgeCount = graphicsDataRef.CharacterList.FindAll(p => p.name.Substring(3, 2) == "MM").Count;
        int maleOldCount = graphicsDataRef.CharacterList.FindAll(p => p.name.Substring(3, 2) == "MO").Count;

        int maleOtherCount = graphicsDataRef.CharacterList.FindAll(p => p.name.Substring(3, 2) == "MI").Count;
        int femaleOtherCount = graphicsDataRef.CharacterList.FindAll(p => p.name.Substring(3, 2) == "FI").Count;
       
        if (strSex == "F")
        {
            if (age == "C")
                index = UnityEngine.Random.Range(1, femaleChildCount + 1);
            if (age == "Y")
                index = UnityEngine.Random.Range(1, femaleYoungCount + 1);
            if (age == "M")
                index = UnityEngine.Random.Range(1, femaleMiddleAgeCount + 1);
            if (age == "O")
                index = UnityEngine.Random.Range(1, femaleOldCount + 1);
            if (age == "I")
                index = UnityEngine.Random.Range(1, femaleOtherCount + 1);
        }
        else if (strSex == "M")
        {
            if (age == "C")
                index = UnityEngine.Random.Range(1, maleChildCount + 1);
            if (age == "Y")
                index = UnityEngine.Random.Range(1, maleYoungCount + 1);
            if (age == "M")
                index = UnityEngine.Random.Range(1, maleMiddleAgeCount + 1);
            if (age == "O")
                index = UnityEngine.Random.Range(1, maleOldCount + 1);
            if (age == "I")
                index = UnityEngine.Random.Range(1, maleOtherCount + 1);
        }
        
        // ai pics, robot, etc
        else if (strSex == "O")
        {
            //    if (age == "I")
            //        //index = UnityEngine.Random.Range(1, otherCount + 1);
            //}
        }
        cData.PictureID = faction + strSex + age + index.ToString("N0");
    }
    public static void GenerateNewPop(Civilization civ, Region rData, Boolean isGraduating)
    {
        float PopSkillModifier = 0.0f;
        float HouseFarmingModifier = 1f;
        float HouseMiningModifier = 1f;
        float HouseEngineeringModifier = 1f;
        float HouseScienceModifier = 1f;
        float HouseMerchantModifier = 1f;
        float HouseFluxModifier = 1f;
        float HouseAdminModifier = 1f;

        Pops newPop = new Pops();

        // Step 1: Determine civ of pop and set location
        newPop.EmpireID = civ.ID;
        newPop.RegionLocationID = rData.ID;

        // Step 1a: Determine type/age of pop
        int popTypeChance = 0;
        popTypeChance = UnityEngine.Random.Range(0, 100);
        if (isGraduating) // there must always be fewer child pops then worker pops (for Peter Pan issue)
        {          
            newPop.Age = 18;
            PopSkillModifier = YoungAdultSkillModifier;
        }

        if (popTypeChance < ChancePopIsYoungAdult && !isGraduating)
        {
            newPop.Type = Pops.ePopType.Worker;
            newPop.Age = UnityEngine.Random.Range(18,25);
            PopSkillModifier = YoungAdultSkillModifier * ((float)newPop.Age/ 18f);
        }
                          
        if (popTypeChance < ChancePopIsWorker && !isGraduating)
        {
            newPop.Type = Pops.ePopType.Worker;
            newPop.Age = UnityEngine.Random.Range(25, 65);
            PopSkillModifier = WorkerSkillModifier;
        }

        if (popTypeChance >= ChancePopIsRetired && !isGraduating)
        {
            newPop.Type = Pops.ePopType.Retired;
            newPop.Age = UnityEngine.Random.Range(66, 85);
            PopSkillModifier = RetiredSkillModifier;
        }
        //}
        //else
        //{
        //    newPop.Type = Pops.ePopType.Worker;
        //    newPop.Age = UnityEngine.Random.Range(18, 65);
        //    PopSkillModifier = WorkerSkillModifier;
        //}

        // Step 1A: Get House Skill Modifier;
        if (rData.PlanetLocated.Viceroy.AssignedHouse != null)
        {
            //HouseFarmingModifier += ((rData.PlanetLocated.Viceroy.AssignedHouse.FarmingTradition - 50) * .01f * (rData.PlanetLocated.Viceroy.TimeInPosition / 100f));
            //HouseMiningModifier += ((rData.PlanetLocated.Viceroy.AssignedHouse.MiningTradition - 50) * .01f * (rData.PlanetLocated.Viceroy.TimeInPosition / 100f));
            //HouseEngineeringModifier += ((rData.PlanetLocated.Viceroy.AssignedHouse.ManufacturingTradition - 50) * .01f * (rData.PlanetLocated.Viceroy.TimeInPosition / 100f));
            //HouseFluxModifier += (((rData.PlanetLocated.Viceroy.AssignedHouse.ScienceTradition + rData.PlanetLocated.Viceroy.AssignedHouse.ManufacturingTradition) - 100) * .01f * (rData.PlanetLocated.Viceroy.TimeInPosition / 100f));
            //HouseScienceModifier += ((rData.PlanetLocated.Viceroy.AssignedHouse.ScienceTradition - 50) * .01f * (rData.PlanetLocated.Viceroy.TimeInPosition / 100f));
            //HouseAdminModifier += ((rData.PlanetLocated.Viceroy.AssignedHouse.GovernmentTradition - 50) * .01f * (rData.PlanetLocated.Viceroy.TimeInPosition / 100f));
            //HouseMerchantModifier += ((rData.PlanetLocated.Viceroy.AssignedHouse.TradeTradition - 50) * .01f * (rData.PlanetLocated.Viceroy.TimeInPosition / 100f));
        }

        // Step 2: Generate basic stats (eventually blend for civ speciality/tech level)
        int farmSkill = 0;
        int scienceSkill = 0;
        int miningSkill = 0;
        int highTechSkill = 0;
        int manufacturingSkill = 0;
        int merchantSkill = 0;
        int fluxSkill = 0;
        int adminSkill = 0;

        // determine the need in each region for each type of pop
        int farmDeficit = rData.FarmingLevel - rData.TotalFarmers;
        int minerDeficit = rData.MiningLevel - rData.TotalMiners;
        int fluxmenDeficit = rData.HighTechLevel - rData.TotalFluxmen;
        int adminDeficit = rData.GovernmentLevel - rData.TotalAdministrators;
        int engineerDeficit = rData.ManufacturingLevel - rData.TotalEngineers;
        int scientistDeficit = rData.ScienceLevel - rData.TotalScientists;
        int merchantDeficit = (rData.PopsInTile.Count / 4) - rData.TotalMerchants;

        // create the base skills using the house modifiers
        farmSkill = (int)(UnityEngine.Random.Range(45, 55) * PopSkillModifier * HouseFarmingModifier);
        scienceSkill = (int)(UnityEngine.Random.Range(45, 55) * PopSkillModifier * HouseScienceModifier);
        miningSkill = (int)(UnityEngine.Random.Range(45, 55) * PopSkillModifier * HouseMiningModifier);
        highTechSkill = (int)(UnityEngine.Random.Range(45, 55) * PopSkillModifier * HouseEngineeringModifier);
        manufacturingSkill = (int)(UnityEngine.Random.Range(45, 55) * PopSkillModifier * HouseEngineeringModifier);
        fluxSkill = (int)(UnityEngine.Random.Range(45, 55) * PopSkillModifier * HouseFluxModifier);
        merchantSkill = (int)(UnityEngine.Random.Range(45, 55) * PopSkillModifier * HouseMerchantModifier);
        adminSkill = (int)(UnityEngine.Random.Range(45, 55) * PopSkillModifier * HouseAdminModifier);

        // adjust for more 'needed/common' pops based on type of job and current need
        farmSkill += farmDeficit * skillNeedMultiple;
        scienceSkill += scientistDeficit * skillNeedMultiple;
        miningSkill += minerDeficit * skillNeedMultiple;
        highTechSkill += ((fluxmenDeficit + engineerDeficit) / 2) * skillNeedMultiple;
        manufacturingSkill += engineerDeficit * skillNeedMultiple;
        fluxSkill += fluxmenDeficit * skillNeedMultiple;
        merchantSkill +=  merchantDeficit * skillNeedMultiple;
        adminSkill += adminDeficit * skillNeedMultiple;
        
        // assign the final skill totals
        newPop.FarmingSkill = farmSkill;
        newPop.ScienceSkill = scienceSkill;
        newPop.MiningSkill = miningSkill;
        newPop.HighTechSkill = highTechSkill;
        newPop.ManufacturingSkill = manufacturingSkill;
        newPop.FluxSkill = fluxSkill;
        newPop.MerchantSkill = merchantSkill;
        newPop.AdminSkill = adminSkill;

        // put the pop into the workforce
        newPop.Employment = Pops.ePopEmployment.Unemployed;

        // Step 3: Generate popular support and unrest levels
        newPop.PopSupport = .5f; // 50% to start
        newPop.UnrestLevel = UnityEngine.Random.Range(0f, .1f);
        newPop.PlanetHappinessLevel = UnityEngine.Random.Range(30, 90) - (100-rData.PlanetLocated.AdjustedBio);

        // Step 4: Add the pop
        rData.PopsInTile.Add(newPop);
    }

    public static void PopulatePlanetGenerationTables()
    {
        try
        {
            string line = null;
            string path = Application.dataPath + "/Resources/planetGenerationData.txt";
            bool fileEmpty = false;

            PlanetGenerationData pGenData = new PlanetGenerationData();
           
            int spotIdx = 0;
            int typeIdx = 0;
            
            System.IO.TextReader readFile = new StreamReader(path);

            while (!fileEmpty) // until the line hits a null object
            {
                line = readFile.ReadLine();

                if (line != null)
                {
                    try
                    {
                        if (line != "//") // don't carriage return, keep reading!
                        {
                            int[] minChance = new int[10];
                            int[] planetType = new int[10];
                            int colonFound;  // moves carriage return past the colon
                            string editedLine; // the part of the data that is used after the colon

                            if (line.StartsWith("CLASS")) // get the class from the text
                            {
                                colonFound = line.IndexOf(":");
                                editedLine = line.Substring(colonFound + 1);
                                int starClass = int.Parse(editedLine);
                                pGenData.starType = (StarData.eSpectralClass)starClass;
                            }

                            if (line.StartsWith("SPOT")) // this is the line that gives the spot # and base generation chances
                            {
                                colonFound = line.IndexOf(":");
                                editedLine = line.Substring(colonFound + 1);
                                string[] planetBaseChanceString = editedLine.Split(new Char[] { ',' });
                                int planetSpot = int.Parse(planetBaseChanceString[0]);
                                int basePercent = int.Parse(planetBaseChanceString[1]);
                                float multiplier = float.Parse(planetBaseChanceString[2]);
                                int compMult = int.Parse(planetBaseChanceString[3]);
                                pGenData.planetGenerationTable.Add(new SpotPlanetGenerationTable()); // need to add a new blank table per spot
                                pGenData.planetGenerationTable[spotIdx].planetSpot = planetSpot;
                                pGenData.planetGenerationTable[spotIdx].baseChance = basePercent;
                                pGenData.planetGenerationTable[spotIdx].materialMultiplier = multiplier;
                                pGenData.planetGenerationTable[spotIdx].companionModifier = compMult;
                            }

                            if (line.StartsWith(":")) // this is the line that gives chance and type of planet
                            {
                                colonFound = line.IndexOf(":");
                                editedLine = line.Substring(colonFound + 1);
                                string[] planetTypePercent = editedLine.Split(new Char[] { ',' });
                                minChance[typeIdx] = int.Parse(planetTypePercent[0]);
                                planetType[typeIdx] = int.Parse(planetTypePercent[1]);
                                pGenData.planetGenerationTable[spotIdx].chanceData.Add(minChance[typeIdx]);
                                pGenData.planetGenerationTable[spotIdx].typeData.Add(planetType[typeIdx]);
                                typeIdx += 1;
                            }

                            if (line.StartsWith("/"))
                            {
                                spotIdx += 1; // space means move to the next spot definition
                                typeIdx = 0; // reset the type index
                            }

                        }

                        else
                        {
                            planetGenerationDataList.Add(pGenData); // add the current data table to the pgData list
                            pGenData = new PlanetGenerationData(); // clear out the old object
                            spotIdx = 0;
                            typeIdx = 0;
                        }
                    }
                    catch(IOException ex)
                    {
                        Debug.LogError("Issue with formatting in file " + path + " Error:" + ex.ToString());
                    }
                }

                else
                    fileEmpty = true;
            }

            readFile.Close();
            readFile = null;
            Debug.Log("Planet Generation Tables successfully read!");
        }
        catch (IOException ex)
        {
            Debug.LogError("Could not read Planet Generation Table file; error:" + ex.ToString());
        }
    }

    

    public static void PopulateRegionTypeTables()
    {
        try
        {
            string line = null;
            string path = Application.dataPath;
            bool fileEmpty = false;

            System.IO.TextReader readFile = new StreamReader(path + "/Resources/regionTypeData.txt");

            while (!fileEmpty) // until the line hits a null object
            {
                line = readFile.ReadLine();

                if (line != null)
                {
                    if (!line.StartsWith("TYPE"))
                    {
                        RegionTypeData pRegionType = new RegionTypeData();                        
                        string[] pRegionTypeString = line.Split(new Char[] { ',' });

                        //pull out each modifier
                        pRegionType.Type = (RegionTypeData.eRegionType)int.Parse(pRegionTypeString[0]);
                        pRegionType.BioMod = float.Parse(pRegionTypeString[1]);
                        pRegionType.FarmMod = float.Parse(pRegionTypeString[2]);
                        pRegionType.ManMod = float.Parse(pRegionTypeString[3]);
                        pRegionType.ResMod = float.Parse(pRegionTypeString[4]);
                        pRegionType.HTMod = float.Parse(pRegionTypeString[5]);
                        pRegionType.AttMod = float.Parse(pRegionTypeString[6]);
                        pRegionType.DefMod = float.Parse(pRegionTypeString[7]);
                        pRegionType.MineralMod = float.Parse(pRegionTypeString[8]);                      

                        // add to the attribute data list                        
                        regionTypeDataList.Add(pRegionType);
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                    fileEmpty = true;
            }

            readFile.Close();
            readFile = null;
            Debug.Log("Region Data successfully read!");
        }
        catch (IOException ex)
        {
            Debug.LogError("Could not read Region Data file; error:" + ex.ToString());
        }
    }

    public static StarData CreateNewStar()  // create a new star system and return it to the GalaxyCreator
    {
        StarData stData = new StarData();
        GenerateStar(stData);
        return stData;
    }

    public static StarData CreateSystemPlanets(StarData curSys)
    {
        PlanetGenerationData pgList = new PlanetGenerationData();
        SpotPlanetGenerationTable pgTable = new SpotPlanetGenerationTable();
        bool planetFound = false;

        try
        {
            pgList = planetGenerationDataList.Find(p => p.starType == curSys.SpectralClass);
        }
        catch (Exception ex)
        {
            Debug.LogError("Could not lookup " + curSys.SpectralClass.ToString() + " table for system " + curSys.Name + "! Error:" + ex.ToString());
            return null; // try again with the next star
        }

        for (int x = 0; x < MaxPlanetsPerSystem; x++) // 6 spots to generate
        {
            pgTable = pgList.planetGenerationTable[x];
            planetFound = false; // reset planet flag
            int generationChance = 0;
            int planetTypeChance = 0;

            // Step 1: check to see if there is a chance of a planet
            generationChance = UnityEngine.Random.Range(0, 100);
            if (generationChance <= pgTable.baseChance + (curSys.pMaterial * pgTable.materialMultiplier) - ((int)curSys.starMultipleType * pgTable.companionModifier))

            // Step 2: look up the planet generation tables for this type of star and determine which planet type is generated
            {
                planetTypeChance = UnityEngine.Random.Range(0, 100);

                for (int y = 0; y < pgTable.chanceData.Count; y++) // look up planet data on each table 
                {
                    if (!planetFound)
                    {
                        int curChance = pgTable.chanceData[y];
                        if (planetTypeChance <= curChance)
                        {
                            PlanetData curPlanetData = new PlanetData();
                            curPlanetData = GeneratePlanet((PlanetData.ePlanetType)pgTable.typeData[y],curSys,x+1);
                            curPlanetData.SystemID = curSys.ID;
                            galaxyDataRef.AddPlanetDataToList(curPlanetData);
                            //curSys.PlanetList.Add(curPlanetData); // a planet is born! Generate it now
                            curSys.PlanetSpots[x] = curPlanetData; // stick it in the correct 'spot' (probably will rewrite)
                            planetFound = true;
                        }
                    }
                }
            }
        }
        
        return curSys;
    }

    private static PlanetData GeneratePlanet(PlanetData.ePlanetType planetType, StarData curSys, int systemSpot)
    {
        PlanetData cPlan = new PlanetData();
        PlanetAttributeTable pTable = new PlanetAttributeTable();
        PlanetAttributeData pData = new PlanetAttributeData();
       
        // assign the attribute table for the planet type
        try
        {
            pData = DataManager.planetAttributeDataList.Find(p => p.planetType == planetType);
            pTable = pData.planetTraitsTable;
        }
        catch (Exception ex)
        {
            Debug.LogError("Could not lookup " + planetType.ToString() + " table for planet type " + planetType.ToString() + "! Error:" + ex.ToString());
            return null; // try again with the next planet
        }
        // Step 0: Log spot of planet in system
        cPlan.PlanetSpot = systemSpot;

        // Step 1: Determine type of planet
        cPlan.Type = (StellarObjects.PlanetData.ePlanetType)planetType; // assign the planet type

        // Step 2: Pick name for planet and assign ID(will change to generic star name)
        systemSpot = curSys.PlanetList.Count + 1;

        if (cPlan.Type == PlanetData.ePlanetType.AsteroidBelt)        
            cPlan.Name = curSys.Name + " AB-" + HelperFunctions.StringConversions.ConvertToRomanNumeral(systemSpot);
        
        else if (cPlan.Type == PlanetData.ePlanetType.IceBelt)     
            cPlan.Name = curSys.Name + " IB-" + HelperFunctions.StringConversions.ConvertToRomanNumeral(systemSpot);
        
        else if (cPlan.Type == PlanetData.ePlanetType.DustRing)      
            cPlan.Name = curSys.Name + " DR-" + HelperFunctions.StringConversions.ConvertToRomanNumeral(systemSpot);

        else
            cPlan.Name = curSys.Name + " " + HelperFunctions.StringConversions.ConvertToRomanNumeral(systemSpot); // convert to planet and spot
        
        cPlan.ID = curSys.Name.Substring(0,2) + UnityEngine.Random.Range(0,9999); // set ID

        // Step 3: Determine size/axial tilt of planet
        try
        {
            int sizeVar = UnityEngine.Random.Range(0, pTable.sizeVar);
            int sizeResult = pTable.size + sizeVar;
            cPlan.Size = sizeResult; // sizeMod;
        }
        catch(Exception ex)
        {
            Debug.LogError("Could not calcuate. Error: " + ex.ToString());
        }

        cPlan.AxialTilt = UnityEngine.Random.Range(0f, 70f); // tilt between 0-70
      
        // Step 4: Determine habitibility of planet
        try
        {
           int habVar = UnityEngine.Random.Range(0, pTable.habVar);
           float habResult = pTable.habitability + habVar;
           float spotVar = 0f;
           switch (cPlan.PlanetSpot)
           {
               case 1:
                   if ((int)curSys.SpectralClass < 5)
                   {
                       spotVar = .6f;
                       break;
                   }
                   else
                   {
                       spotVar = .9f;
                       break;
                   }
                   
               case 2:
                   spotVar = .9f;
                   break;
               case 3:
                   spotVar = 1f;
                   break;
               case 4:
                   spotVar = 1f;
                   break;
               case 5:
                   spotVar = .9f;
                   break;
               case 6:
                   spotVar = .7f;
                   break;
               default:
                   spotVar = 1f;
                   break;
           }
            cPlan.Bio = (int)(habResult * spotVar);
        }
        catch (Exception ex)
        {
            Debug.LogError("Could not calcuate. Error: " + ex.ToString());
        }

        // Step 5: Determine industrial modifier of planet
        cPlan.IndustrialMultiplier = (pTable.indMult / 100);

        // Step 6: Determine if planet has any rings
        if (UnityEngine.Random.Range(0, 100) <= pTable.ringChance)
            cPlan.Rings = true;
        else
            cPlan.Rings = false;

        // Step 7: Determine how many moons the planet has
        if (UnityEngine.Random.Range(0, 100) <= pTable.moonChance)
            cPlan.Moons = UnityEngine.Random.Range(1,pTable.maxMoons); // change later to expand on moons
        
        // Step 7b: Adjust habitability based on moons       
        if (cPlan.Moons > 0)
            if (((int)cPlan.Type == 7) || ((int)cPlan.Type == 8))
            {
                cPlan.Bio += (cPlan.Moons * 2);
            }
        if (((int)cPlan.Type == 10) || ((int)cPlan.Type == 3) || ((int)cPlan.Type == 2) || ((int)cPlan.Type == 13))
            {
                cPlan.Bio += (cPlan.Moons * 5);
            }
       
        // Step 8: Determine rating of alpha materials
        try
        {
            int alphaVar = UnityEngine.Random.Range(0, pTable.alpVar);
            cPlan.BasicMaterials = pTable.alpMaterial + alphaVar;
        }
        catch (Exception ex)
        {
            Debug.LogError("Could not calcuate. Error: " + ex.ToString());
        }

        // Step 9: Determine rating of heavy materials
        try
        {
            int heavyVar = UnityEngine.Random.Range(0, pTable.heavVar);
            cPlan.HeavyMaterials = (int)(pTable.heavyMaterial * (1 + (curSys.pMaterial / 20)));
            cPlan.HeavyMaterials = cPlan.HeavyMaterials * (1 + ((5 - systemSpot) / 10));
            cPlan.HeavyMaterials += heavyVar;

        }
        catch (Exception ex)
        {
            Debug.LogError("Could not calcuate. Error: " + ex.ToString());
        }

        // Step 10: Determine rating of rare materials
        try
        {
            int rareMod = UnityEngine.Random.Range(0, pTable.rareVar);
            cPlan.RareMaterials = (int)(pTable.rareMaterial * (1 + (curSys.pMaterial / 12)));
            cPlan.RareMaterials = cPlan.RareMaterials * (1 + ((5 - systemSpot) / 8));
            cPlan.RareMaterials += rareMod;
        }
        catch (Exception ex)
        {
            Debug.LogError("Could not calcuate. Error: " + ex.ToString());
        }

        // Step 11: Determine energy rating
        try
        {
            cPlan.Energy = (int)(pTable.energy * (1 + (curSys.pMaterial / 12)));
            cPlan.Energy = cPlan.Energy * (1 + ((5 - systemSpot) / 10));
            cPlan.Energy += UnityEngine.Random.Range(0, 30);
        }
        catch (Exception ex)
        {
            Debug.LogError("Could not calcuate. Error: " + ex.ToString());
        }

        // Step 12: Generate sprite number (to show type of planet)
        if ((cPlan.Type != PlanetData.ePlanetType.DustRing) && (cPlan.Type != PlanetData.ePlanetType.IceBelt) && (cPlan.Type != PlanetData.ePlanetType.AsteroidBelt))
            cPlan.PlanetSpriteNumber = UnityEngine.Random.Range(0, SpritesPerPlanetType - 1);
        else
            cPlan.PlanetSpriteNumber = UnityEngine.Random.Range(0, SpritesPerBeltType - 1);

        // Step 13: Generate ring sprite number if applicable
        cPlan.PlanetRingSpriteNumber = UnityEngine.Random.Range(0, SpritesPerRingType - 1);
        cPlan.PlanetRingTilt = UnityEngine.Random.Range(-10, 10);

        // Step 13: Determine planetary traits
        DeterminePlanetTraits(cPlan);

        // Step 14: Determine total and habitable tiles based on bio level
        cPlan.MaxTiles = (cPlan.Size / 3);
        for (int x = 0; x < cPlan.MaxTiles; x++)
        {
            if (UnityEngine.Random.Range(0, 100) < cPlan.AdjustedBio)
            {
                cPlan.MaxHabitableTiles += 1;
            }
        }

        // Step 15: Create and assign tiles to the planet
        GeneratePlanetRegions(cPlan);
        
        // Step 16: Send the planet data back!
        return cPlan;

    }

    public static void GeneratePlanetDescription(PlanetData cPlan) // make function public to allow regeneration of planet description
    {
        string possessivePronoun = "";
        if (gameDataRef.CivList[0].PlanetIDList.Exists(p => p == cPlan.ID))
        {
            possessivePronoun = "our";
        }
        else
        {
            possessivePronoun = "the";
        }
        cPlan.Description = ""; // clear the description
        cPlan.Description = possessivePronoun + " planet " + cPlan.Name + " is a " + HelperFunctions.StringConversions.ConvertPlanetEnum(cPlan.Type).ToLower() + ", Your Majesty.";
        if (cPlan.Rank == PlanetData.ePlanetRank.ImperialCapital)
        {
            cPlan.Description += " It is " + possessivePronoun + " capital world.";
        }
        else if (cPlan.Rank == PlanetData.ePlanetRank.ProvinceCapital)
        {
            cPlan.Description += " It is " + possessivePronoun + " capital world of the " + HelperFunctions.DataRetrivalFunctions.GetProvince(HelperFunctions.DataRetrivalFunctions.GetSystem(cPlan.SystemID).AssignedProvinceID).Name + " province.";
        }
        else if (cPlan.Rank == PlanetData.ePlanetRank.SystemCapital)
        {
            cPlan.Description += " It is " + possessivePronoun + " capital world of the " + HelperFunctions.DataRetrivalFunctions.GetSystem(cPlan.SystemID).Name + " system.";
        }
        if (cPlan.ScanLevel < .5f)
        {
            cPlan.Description += " Until we scan the planet more throughly, we can not understand more about its history and features.";
        }
        else
        {
            cPlan.Description += " We know enough about " + possessivePronoun + " planet to understand more about its history and features.";
        }
    }

    private static void DeterminePlanetTraits(PlanetData cPlan)
    {
        int baseChance = 0;
        bool traitFound = false;
        bool checkValid = true; // was a trait found? If so, keep going
        int diceRoll = 0;

        while (checkValid)
        {
            // determine chance for a planet trait
            if (cPlan.PlanetTraits.Count == 0)
                baseChance = TraitChanceWith0Traits;
            if (cPlan.PlanetTraits.Count == 1)
                baseChance = TraitChanceWith1Traits;
            if (cPlan.PlanetTraits.Count == 2)
                baseChance = TraitChanceWith2Traits;
            if (cPlan.PlanetTraits.Count > 2)
            {
                Debug.Log("Exiting trait generation for planet " + cPlan.Name);
                break;
            }

            diceRoll = UnityEngine.Random.Range(0, 101);

            if (diceRoll < baseChance)
            {
                traitFound = true;
            }
            else
            {
                checkValid = false;
                Debug.Log("Exiting trait generation for planet " + cPlan.Name);
                break;
            }
                
            if (traitFound)
            {
                bool validTraitFound = false;
                bool validTypeFound = false;
                int tryCount = 0; // how many attempts have been made to find a trait

                while (!validTraitFound && tryCount < 5)
                {
                    newTest:
                    tryCount += 1;
                    
                    PlanetTraits testTrait = DataManager.planetTraitDataList[UnityEngine.Random.Range(0, DataManager.planetTraitDataList.Count)]; // grab a trait
                    if (testTrait != null)
                    {
                        if (testTrait.PlanetType[0] != 0) // a zero in slot 1 means any type
                        {
                            for (int x = 0; x < 5; x++ )
                            {
                                if (testTrait.PlanetType[x] == cPlan.Type)
                                {
                                    validTypeFound = true;
                                    break;
                                }
                                else
                                    validTypeFound = false;
                            }                             
                        }
                        else
                            validTypeFound = true;
                        
                        if (validTypeFound)
                        {
                            if (testTrait.MoonsNecessary > cPlan.Moons) // check moon requirement
                                goto newTest;

                            if (cPlan.PlanetTraits.Count > 0) // only check if there is at least one trait, otherwise always valid
                            {
                                foreach (PlanetTraits trait in cPlan.PlanetTraits)
                                {
                                    if (trait.Category == testTrait.Category)
                                        goto newTest;
                                }
                            }

                            if (testTrait.BeltEligible == 0) // check for exclusion traits with belts
                            {
                                if (cPlan.Type == PlanetData.ePlanetType.AsteroidBelt || cPlan.Type == PlanetData.ePlanetType.DustRing || cPlan.Type == PlanetData.ePlanetType.IceBelt)
                                    goto newTest;
                            }

                            validTraitFound = true;
                        }

                        if (validTraitFound)
                        {
                            cPlan.PlanetTraits.Add(testTrait);
                            break;
                        }
                        
                    }
                    else
                    {
                        Debug.Log("Error generating planet trait: Read trait was null.");
                        break; //error
                    }
                }
            }
        }
    }

    public static void GenerateNebula()
    {
        NebulaData nData = new NebulaData();
        galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
        gameDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();

        // Step 1: Generate name
        if (DataManager.planetNameList.Count > 0)
        {
            var nameIndex = UnityEngine.Random.Range(0, DataManager.planetNameList.Count);
            nData.Name = DataManager.planetNameList[nameIndex] + " Nebula";
            DataManager.planetNameList.RemoveAt(nameIndex);
            DataManager.planetNameList.TrimExcess();
        }
        else
            nData.Name = "GenericName";

        // Step 2: Set location
        nData.WorldLocation = new Vector3(UnityEngine.Random.Range(-gameDataRef.GalaxySizeWidth,gameDataRef.GalaxySizeWidth), UnityEngine.Random.Range(-gameDataRef.GalaxySizeHeight, gameDataRef.GalaxySizeHeight), UnityEngine.Random.Range(-25,-75)); // set slightly back of X axis for parallax effect

        // Step 3: Assign sprite #
        nData.NebulaSpriteNumber = UnityEngine.Random.Range(0, SpritesPerNebulaType - 1);

        // Step 4: Generate size
        nData.NebulaSize = UnityEngine.Random.Range(.5f, 1.5f);

        // Step 5: Assign rotation
        switch (nData.NebulaSpriteNumber)
        {
            case 0:
            { 
                nData.TextRotation = -55f;
                break;
            }
            case 1:
            {
                nData.TextRotation = -55f;
                break;
            }
            case 2:
            { 
                nData.TextRotation = 45f;
                break;
            }
            case 3:
            {
                nData.TextRotation = -55f;
                break;
            }
            case 4:
            { 
                nData.TextRotation = -60f;
                break;
            }
            default:
            {
                nData.TextRotation = 0f;
                break;
            }
        }

        // Step 6: Assign the data
        galaxyDataRef.AddStellarPhenonomaDataToList(nData);
        
    }

    public static void GeneratePlanetRegions(PlanetData cPlan)
    {
        galaxyDataRef = GameObject.Find("GameManager").GetComponent<GalaxyData>();
        
        List<int> tileNumberList = new List<int>(); // for the uninhabitable tiles
        List<Region> tempTileList = new List<Region>();

        cPlan.RegionList.Clear(); // remove any old tiles

        for (int x = 0; x < cPlan.MaxTiles; x++)
        {
            Region newTile = new Region(); // create temp tile
            newTile.IsHabitable = true;

            // determine region type
            int[] tRegions = DataManager.planetAttributeDataList.Find(p => p.planetType == cPlan.Type).planetTraitsTable.validRegions;
            int typeChoice = tRegions[UnityEngine.Random.Range(0, tRegions.GetLength(0))];
            newTile.RegionType = regionTypeDataList[typeChoice];

            // finish the tile based on region type
            newTile.ID = cPlan.ID + "TILE" + x.ToString("N0");
            newTile.BioRating = (int)(UnityEngine.Random.Range((int)(cPlan.AdjustedBio / 1.5f),(int)(cPlan.AdjustedBio * 1.5f)) * newTile.RegionType.BioMod);
            newTile.EnergyRating = (int)(UnityEngine.Random.Range((int)(cPlan.Energy / 1.5f), (int)(cPlan.Energy * 1.5f)) * newTile.RegionType.MineralMod);
            newTile.HeavyRating = (int)(UnityEngine.Random.Range((int)(cPlan.HeavyMaterials / 1.5f), (int)(cPlan.HeavyMaterials * 1.5f)) * newTile.RegionType.MineralMod);
            newTile.AlphaRating = (int)(UnityEngine.Random.Range((int)(cPlan.BasicMaterials / 1.5f), (int)(cPlan.BasicMaterials * 1.5f)) * newTile.RegionType.MineralMod);
            newTile.RareRating = (int)(UnityEngine.Random.Range((int)(cPlan.RareMaterials / 1.5f), (int)(cPlan.RareMaterials * 1.5f)) * newTile.RegionType.MineralMod);
            newTile.MaxDevelopmentLevel = (int)(newTile.BioRating / 4); // 25% of bio rating for tile
            newTile.CurLevel = 0; // new tile
            newTile.PlanetLocationID = cPlan.ID; // assign the planet's ID to the region to allocate during loading

            // now add to the temp list
            tempTileList.Add(newTile);
        }

        foreach (Region reg in tempTileList)
        {
            if (reg.BioRating == 0)
            {
                reg.IsHabitable = false;
                reg.MaxDevelopmentLevel = 0;
            }
        }

        // finally add the tiles to the respective lists
        for (int x = 0; x < tempTileList.Count; x++)
        {
            cPlan.RegionList.Add(tempTileList[x]);
            galaxyDataRef.AddTileToList(tempTileList[x]);
        }

    }

    public static void GenerateRegionInfrastructure(PlanetData cPlan)
    {

        List<Region> newRegionList = new List<Region>();
        newRegionList = cPlan.RegionList;

        foreach (Region newRegion in newRegionList)
        {
           
            int infraLevel = UnityEngine.Random.Range(1, 75);

            // adjust modifier by planet rank
            if (cPlan.Rank == PlanetData.ePlanetRank.EstablishedColony)
                infraLevel = (int)(infraLevel * .5f);
            else if (cPlan.Rank == PlanetData.ePlanetRank.SystemCapital)
                infraLevel = (int)(infraLevel * .7f);
            else if (cPlan.Rank == PlanetData.ePlanetRank.ProvinceCapital)
                infraLevel = (int)(infraLevel * .9f);
            else if (cPlan.Rank == PlanetData.ePlanetRank.ImperialCapital)
                infraLevel = (int)(infraLevel * 1.2f);

            // adjust development type by region bio modifier
            infraLevel = (int)(infraLevel * newRegion.RegionType.BioMod);

            // adjust by development already there
            infraLevel += newRegion.TotalDevelopmentLevel / 2;

            if (infraLevel > 110)
                infraLevel = 110;

            if (infraLevel < 0)
                infraLevel = 0;
            newRegion.HabitatationInfrastructureLevel = infraLevel;
            
        }
    }
    private static void GenerateStar(StarData stData)
    {
        // Step 1: Pick name for star
        if (DataManager.systemNameList.Count > 0)
        {
            var nameIndex = UnityEngine.Random.Range(0, DataManager.systemNameList.Count);
            stData.Name = DataManager.systemNameList[nameIndex];
            DataManager.systemNameList.RemoveAt(nameIndex);
            DataManager.systemNameList.TrimExcess();
        }
        else
        { 
            stData.Name = "GenericName";
        }

            // Step 2/3: Determine spectral class/adjust

            // generate age
            int specAge = 0;
            specAge = UnityEngine.Random.Range(1, 11);
            stData.Age = specAge; // assign age

            // assign classes
            DetermineSpectralClasses(stData, "S");

            // Step 3b: Adjust size for secondary spectral class
            stData.Size -= stData.SecondarySpectralClass;

            // Step 4: Now check for the possibility of multiple star systems (binary, trinary systems)

            int binChance = UnityEngine.Random.Range(1, 11); // check 1D10 for -nary system

            if (binChance <= 4)
            {
                if (binChance > 2 && binChance < 5)
                {
                    stData.starMultipleType = StarData.eStarMultiple.Binary; // binary system
                    DetermineSpectralClasses(stData, "B");
                }
                else
                {
                    stData.starMultipleType = StarData.eStarMultiple.Trinary; // trinary system
                    DetermineSpectralClasses(stData, "B"); // determine binary class
                    DetermineSpectralClasses(stData, "T"); // and then trinary
                }
            }

            // Step 5: Generate specials for each star (not all stars may have a special!) - TBD

            // Step 6: Determine metallicity
            stData.Metallicity = UnityEngine.Random.Range(1, 11);

            // Step 7: Generate stellar planetary material (automatically calculated in the class; derived when needed

            // Step 8: Generate star ID
            stData.ID = "STAR" + stData.Name.Substring(0, 2).ToUpper() + UnityEngine.Random.Range(0, 10000); // 2 parts: first 4 letters of name and 5 digit # creates unique ID

            // Step 9: Reset Intel level to 0 (determine from empire settings)
            stData.IntelValue = 0;    
    }

    private static void DetermineSpectralClasses(StarData stData, string stType)
    {

        int specClass = 0;        
        int secSpecClass = 0;

        // rolls for spectral classes
        specClass = UnityEngine.Random.Range(1, 101);
        secSpecClass = UnityEngine.Random.Range(1, 11);

        if (specClass < 3)
        {
            if (stType == "S")
            {
                stData.SpectralClass = StarData.eSpectralClass.O_B;
                stData.Size = 90;
            }
            else if (stType == "B")
            {
                stData.compSpectralClass = StarData.eSpectralClass.O_B;
                stData.Size = 90;
            }
            else
            {
                stData.terniaryCompSpectralClass = StarData.eSpectralClass.O_B;
                stData.Size = 90;
            }
        }

        else if (specClass < 8)
        {
            if (stType == "S")
            {
                stData.SpectralClass = StarData.eSpectralClass.A;
                stData.Size = 70;
            }
            else if (stType == "B")
            {
                stData.compSpectralClass = StarData.eSpectralClass.A;
                stData.Size = 70;
            }
            else
            {
                stData.terniaryCompSpectralClass = StarData.eSpectralClass.A;
                stData.Size = 70;
            }
        }
        else if (specClass < 21)
        {
            stData.Size = 60;
            if (stType == "S")           
                stData.SpectralClass = StarData.eSpectralClass.F;
      
            else if (stType == "B")           
                stData.compSpectralClass = StarData.eSpectralClass.F;
                
            else
                stData.terniaryCompSpectralClass = StarData.eSpectralClass.F;
        }
        else if (specClass < 38)
        {
            stData.Size = 50;
            if (stType == "S")         
                stData.SpectralClass = StarData.eSpectralClass.G;
                
            else if (stType == "B")
                stData.compSpectralClass = StarData.eSpectralClass.G;
            else
                stData.terniaryCompSpectralClass = StarData.eSpectralClass.G;
        }
        else if (specClass < 59)
        {
            stData.Size = 40;
            if (stType == "S")
            {
                stData.SpectralClass = StarData.eSpectralClass.K;
                stData.Size = 40;
            }
            else if (stType == "B")
                stData.compSpectralClass = StarData.eSpectralClass.K;
            else
                stData.terniaryCompSpectralClass = StarData.eSpectralClass.K;
        }
        else if (specClass < 77)
        {
            stData.Size = 20;
            if (stType == "S")
            {
                stData.SpectralClass = StarData.eSpectralClass.M;
                stData.Size = 20;
            }
            else if (stType == "B")
                stData.compSpectralClass = StarData.eSpectralClass.M;
            else
                stData.terniaryCompSpectralClass = StarData.eSpectralClass.M;
        }         
        else if (specClass < 84)
        {
            stData.Size = 17;
            if (stType == "S")
            {
                stData.SpectralClass = StarData.eSpectralClass.L;
                stData.Size = 17;
            }
            else if (stType == "B")
                stData.compSpectralClass = StarData.eSpectralClass.L;
            else
                stData.terniaryCompSpectralClass = StarData.eSpectralClass.L;
        }
        else if (specClass < 90)
        {
            stData.Size = 15;
            if (stType == "S")
            {
                stData.SpectralClass = StarData.eSpectralClass.T;
                
            }
            else if (stType == "B")
                stData.compSpectralClass = StarData.eSpectralClass.T;
            else
                stData.terniaryCompSpectralClass = StarData.eSpectralClass.T;
        }
        else if (specClass < 93)
        {
            stData.Size = 20;
            if (stType == "S")
            {
                stData.SpectralClass = StarData.eSpectralClass.D;
                
            }
            else if (stType == "B")
                stData.compSpectralClass = StarData.eSpectralClass.D;
            else
                stData.terniaryCompSpectralClass = StarData.eSpectralClass.D;
        }
        else if (specClass < 97)
        {
            stData.Size = 50;
            if (stType == "S")
            {
                stData.SpectralClass = StarData.eSpectralClass.BH;               
            }
            else if (stType == "B")
                stData.compSpectralClass = StarData.eSpectralClass.BH;
            else
                stData.terniaryCompSpectralClass = StarData.eSpectralClass.BH;
        }
        else
        {
            stData.SpectralClass = StarData.eSpectralClass.NoStar;
        }

        stData.SecondarySpectralClass = secSpecClass; // assign secondary class
        

        // Step 3: Now compare age and adjust the spectral class if needed due to age

        //giant stars
        if (stType == "S")
        {
            if (stData.Age == 10 && stData.SpectralClass < (StarData.eSpectralClass)3)
            {
                stData.SpectralClass = StarData.eSpectralClass.BH; //black hole
                stData.Size = 50;
            }

            if (stData.Age == 9 && stData.SpectralClass < (StarData.eSpectralClass)3)
            {
                stData.SpectralClass = StarData.eSpectralClass.Neutron; //neutron star
                stData.Size = 15;
            }

            if (stData.Age == 8 && stData.SpectralClass < (StarData.eSpectralClass)3)
            {
                stData.SpectralClass = StarData.eSpectralClass.RG; //sub-giant
                stData.Size = 120;
            }

            //main sequence stars
            if (stData.Age == 10 && (int)stData.SpectralClass < 6 && (int)stData.SpectralClass > 1)
            {
                stData.SpectralClass = StarData.eSpectralClass.D; //degenerate/white dwarf
                stData.Size = 20;
            }

            if (stData.Age == 9 && (int)stData.SpectralClass < 6 && (int)stData.SpectralClass > 2)
            {
                stData.SpectralClass = StarData.eSpectralClass.Neutron; //neutron star
                stData.Size = 20;
            }

            if (stData.Age == 8 && (int)stData.SpectralClass < 6 && (int)stData.SpectralClass > 2)
            {
                stData.SpectralClass = StarData.eSpectralClass.SG; //red giant
                stData.Size = 120;
            }
        }
        else // binary star adjustments
        {
            StarData.eSpectralClass tempClass = StarData.eSpectralClass.NoStar;
            bool changeOfClass = false;

            if ((int)stData.SpectralClass == 11)
            {
                tempClass = StarData.eSpectralClass.WR;
                changeOfClass = true;
            }
            if ((int)stData.SpectralClass > 1 && (int)stData.SpectralClass < 6)
            {
                int compChangeChange = UnityEngine.Random.Range(0, 101); // 33% chance of a white dwarf binary companion if SC is low
                if (compChangeChange < 34)
                {
                    tempClass = StarData.eSpectralClass.D;
                    changeOfClass = true;
                }
            }
            if ((int)stData.SpectralClass > 10 && (int)stData.SpectralClass <15)
            {
                int compChangeChange = UnityEngine.Random.Range(0, 101); // 80% chance of a white dwarf binary companion if spectral class is high
                if (compChangeChange < 81)
                {
                    tempClass = StarData.eSpectralClass.D;
                    changeOfClass = true;
                }
            }

            if (changeOfClass)
            {
                if (stType == "B")
                    stData.compSpectralClass = tempClass;
                else
                    stData.terniaryCompSpectralClass = tempClass;
            }
        }      
    }
}
