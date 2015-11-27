using UnityEngine;
using System.Collections;

namespace Constants
{
    public class Constants
    {
        // intel level consts
        public const int LowIntelLevelMax = 3;
        public const int MediumIntelLevelMax = 6;
        public const int HighIntelMax = 10;

        // resource needs constants (to start)
        public const float FarmingBaseAlphaUsage = .15f;
        public const float FarmingBaseHeavyUsage = .1f;
        public const float FarmingBaseEnergyUsage = .05f;
        public const float FarmingBaseRareUsage = 0f;

        public const float ScienceBaseAlphaUsage = .4f;
        public const float ScienceBaseHeavyUsage = .15f;
        public const float ScienceBaseEnergyUsage = .9f;
        public const float ScienceBaseRareUsage = 0.12f;

        public const float ManufacturingBaseAlphaUsage = .9f;
        public const float ManufacturingBaseHeavyUsage = .7f;
        public const float ManufacturingBaseEnergyUsage = .5f;
        public const float ManufacturingBaseRareUsage = 0;

        public const float HighTechBaseAlphaUsage = .5f;
        public const float HighTechBaseHeavyUsage = .25f;
        public const float HighTechBaseEnergyUsage = .15f;
        public const float HighTechBaseRareUsage = 0.35f;

        public const float MiningBaseAlphaUsage = .2f;
        public const float MiningBaseHeavyUsage = .12f;
        public const float MiningBaseEnergyUsage = .3f;
        public const float MiningBaseRareUsage = 0;

        public const float FoodBaseUsagePerPop = .07f;
        public const float EnergyBaseUsagePerPop = .045f;

        // build amount constants
        public const float AlphaMaterialsPerFarmLevel = 30f;
        public const float HeavyMaterialsPerFarmLevel = 5f;
        public const float RareMaterialsPerFarmLevel = 0f;
        public const float AlphaMaterialsPerFactoryLevel = 80f;
        public const float HeavyMaterialsPerFactoryLevel = 30f;
        public const float RareMaterialsPerFactoryLevel = 10f;
        public const float AlphaMaterialsPerMineLevel = 50f;
        public const float HeavyMaterialsPerMineLevel = 10f;
        public const float RareMaterialsPerMineLevel = 3f;
        public const float AlphaMaterialsPerHighTechLevel = 70f;
        public const float HeavyMaterialsPerHighTechLevel = 40f;
        public const float RareMaterialsPerHighTechLevel = 15f;
        public const float AlphaMaterialsPerGovernmentLevel = 100f;
        public const float HeavyMaterialsPerGovernmentLevel = 15f;
        public const float RareMaterialsPerGovernmentLevel = 0f;
        public const float AlphaMaterialsPerScienceLevel = 80f;
        public const float HeavyMaterialsPerScienceLevel = 50f;
        public const float RareMaterialsPerScienceLevel = 50f;
        public const float AlphaMaterialsPerInfraLevel = 100f;
        public const float HeavyMaterialsPerInfraLevel = 15f;
        public const float RareMaterialsPerInfraLevel = 0f;

        // eco constants
        public const float ResourceBaseCost = .08f;
        public const float BaseEconFactor = .055f;
        public const float BaseTradeFactor = .0013f;
        public const float DistanceModifier = 3.0f;
        public const float MaxGPPAllocatedForPlanet = .40f; // percent of GPP that needs to stay on the planet for planet development, taxes, etc
        public const float MaxGPPAllocatedForImports = .50f; // percent of GPP that can be used to buy imports
        public const float MaxGPPAllocatedForTrade = .30f; // percent of GPP that can be used to export goods

        // trade constants (this is what the total available resources are divided by to send; lower # = more sent
        public const float SupplyPlanetLow = 4f; // 25%
        public const float SupplyPlanetModerate = 2f; // 50%
        public const float SupplyPlanetHigh = 1.5f; // 66%

        // starbase throughput levels
        public const int Level1StarbaseCapacity = 15;
        public const int Level2StarbaseCapacity = 25;
        public const int Level3StarbaseCapacity = 40;
        public const int Level4StarbaseCapacity = 80;
        public const int Level5StarbaseCapacity = 160;

        // development level consts
        public const float FarmingDevelopmentModifier = .2f;
        public const float ScienceDevelopmentModifier = 3.2f;
        public const float ManufacturingDevelopmentModifier = 1.3f;
        public const float HighTechDevelopmentModifier = 2.2f;
        public const float MiningDevelopmentModifier = .6f;
        public const float GovernmentDevelopmentModifier = 2.1f;

        // const for base job type desirability
        public const float FarmJobDesirability = .7f;
        public const float MineJobDesirability = .85f;
        public const float BuilderJobDesirability = 1.05f;
        public const float EngineerJobDesirability = 1.25f;
        public const float ScientistJobDesirability = 1.5f;

        // house generation percentages (must equal 100)
        public const int CommonHouseChance = 55;
        public const int MinorHouseChance = 25;
        public const int GreatHouseChance = 20;

        // leader chance that they are in a certain house for different jobs
        public const int ChanceProvinceGovernorGreat = 75;
        public const int ChanceProvinceGovernorMinor = 20;

        public const int ChanceSystemGovernorGreat = 65;
        public const int ChanceSystemGovernorMinor = 20;

        public const int ChanceViceroyGreat = 50;
        public const int ChanceViceroyMinor = 25;

        // pop migration chances
        public const int MigrationTarget = 230;
        public const int StellarMigrationThreshold = 500;

        // character creation constants
        public const int MaxCharTraits = 4; // maximum amount of traits that a character can have

    }
}
