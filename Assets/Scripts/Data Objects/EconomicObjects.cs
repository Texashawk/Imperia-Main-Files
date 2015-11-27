using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using StellarObjects;
using HelperFunctions;
using CivObjects;

namespace EconomicObjects
{
    public class TradeAgreement
    {
        public enum eAgreementStatus : int
        {
            Active,
            Inactive_Support,
            Inactive_Resources,
            Inactive_Pirates,
            Supply_Trade,
            Cancelled
        }
        public eAgreementStatus Status { get; set; }
        public int LengthOfTime { get; set; }
        public float Distance
        {
            get
            {
                float distance = 0f;
                distance = HelperFunctions.Formulas.MeasureDistanceBetweenSystems(ImportPlanet.System, ExportPlanet.System);
                if (distance < 10)
                    distance = 10;
                return distance;
            }
        }

        public string ImportPlanetID { get; set; }
        public string ExportPlanetID { get; set; }
        public float CostModifier { get; set; }

        // get planets
        public PlanetData ImportPlanet
        {
            get
            {
                return HelperFunctions.DataRetrivalFunctions.GetPlanet(ImportPlanetID);
            }
        }

        public PlanetData ExportPlanet
        {
            get
            {
                return HelperFunctions.DataRetrivalFunctions.GetPlanet(ExportPlanetID);
            }
        }

        public float Cost
        {
            get
            {
                Civilization civ = HelperFunctions.DataRetrivalFunctions.GetCivilization(ExportPlanet.Owner.ID);
              
                float baseResourceCost = 0f;
                float totalCost = 0f;
                float _costModifier = 1.0f; // chance in cost based on support level of governors

                baseResourceCost = (FoodSent * civ.CurrentFoodPrice);
                baseResourceCost += (EnergySent * civ.CurrentEnergyPrice);
                baseResourceCost += (AlphaSent * civ.CurrentAlphaPrice);
                baseResourceCost += (HeavySent * civ.CurrentHeavyPrice);
                baseResourceCost += (RareSent * civ.CurrentRarePrice);

                // determine cost modifier
                if (ImportPlanet.ProvGovSupport == eSupportLevel.Partial)
                    _costModifier += 1f;
                if (ImportPlanet.SysGovSupport == eSupportLevel.Partial)
                    _costModifier += .5f;
                if (ImportPlanet.SysGovSupport == eSupportLevel.Full && ImportPlanet.System.Governor.HouseID == ExportPlanet.Viceroy.HouseID)
                    _costModifier = _costModifier / 2;
                if (ImportPlanet.ProvGovSupport == eSupportLevel.Full && ImportPlanet.System.Province.Governor.HouseID == ExportPlanet.Viceroy.HouseID)
                    _costModifier = _costModifier / 2;

                CostModifier = _costModifier;

                if (Status == eAgreementStatus.Supply_Trade)
                {
                    CostModifier = .25f; // huge discount for supply trades
                }

                totalCost = ((baseResourceCost * (Distance / Constants.Constants.DistanceModifier)) * CostModifier) * Constants.Constants.BaseTradeFactor;
                return totalCost;
            }
        }

        public float EnergySent { get; set; }
        public float FoodSent { get; set; }
        public float AlphaSent { get; set; }
        public float HeavySent { get; set; }
        public float RareSent { get; set; }
        public float TotalSent
        {
            get
            {
                float totalSent = 0;
                totalSent = EnergySent + FoodSent + AlphaSent + HeavySent + RareSent;
                return totalSent;
            }
        }
    }
}
