using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using HelperFunctions;
using StellarObjects;
using CharacterObjects;
using CivObjects;

namespace GameEvents
{
    public class GameEvent
    {
        public enum eEventType : int
        {
            PlanetGainedPopulation,
            PlanetLostPopulation,
            PlanetPoSupIncreased,
            PlanetPoSupDecreased,
            StarvationOnPlanet,
            BlackoutsOnPlanet,
            NewFarmBuilt,
            NewMineBuilt,
            NewHighTechBuilt,
            NewFactoryBuilt
        }

        public enum eEventLevel : int
        {
            Informational,
            Moderate,
            Serious,
            Critical,
            Positive
        }

        public enum eEventScope : int
        {
            Region,
            Planet,
            System,
            Province,
            Civ,
            Global,
            Character
        }

        public string LocationName { get; set; }
        public string CivID { get; set; }
        public eEventScope Scope { get; set; }
        public eEventType Type { get; set; }
        public eEventLevel Level { get; set; }
        public int Value1 { get; set; }
        public string Label1 { get; set; }
        public float Date { get; set; }
        public string SystemLocationID { get; set; }
        public string PlanetLocationID { get; set; }
        public string Description { get; set; }
        public bool EventIsNew { get; set; }

        public static void CreateNewStellarEvent(string civID, string locationName, string systemLocationID, string planetLocationID, eEventType eType, eEventLevel eLevel, eEventScope eScope, int Value1, string Label1)
        {
            GlobalGameData gDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();
            string origDesc = DataManager.Descriptions[eType];
            string newDesc = origDesc;
            GameEvent newEvent = new GameEvent();

            newEvent.Type = eType;
            newEvent.Level = eLevel;
            newEvent.Scope = eScope;
            newEvent.CivID = civID;
            newEvent.SystemLocationID = systemLocationID;
            newEvent.PlanetLocationID = planetLocationID;
            newEvent.EventIsNew = true;
            newEvent.Date = gDataRef.GameDate; // set the date
            
            newEvent.Description = DataManager.Descriptions[eType];
            if (origDesc.Contains("[VALUE]"))
            {
                string valueReplace;
                valueReplace = origDesc.Replace("[VALUE]", Mathf.Abs(Value1).ToString("N0"));
                newDesc = valueReplace;
                newEvent.Description = newDesc;
            }

            if (origDesc.Contains("[PLANET]"))
            {
                string planetReplace;
                planetReplace = newDesc.Replace("[PLANET]", locationName);
                newDesc = planetReplace;
                newEvent.Description = newDesc;
            }
           
            HelperFunctions.DataRetrivalFunctions.GetCivilization(civID).LastTurnEvents.Add(newEvent); // add the event to the correct civ list
        }
    }

  static public class PlanetEventCreator
  {

      public static void GeneratePlanetEvents(PlanetData pData, Civilization civ)
      {
          GlobalGameData gDataRef = GameObject.Find("GameManager").GetComponent<GlobalGameData>();

          // population events
          if (pData.PopulationChangeLastTurn > 0)
          {
              GameEvents.GameEvent.CreateNewStellarEvent(civ.ID, pData.Name, pData.SystemID, pData.ID, GameEvents.GameEvent.eEventType.PlanetGainedPopulation, GameEvents.GameEvent.eEventLevel.Positive, GameEvents.GameEvent.eEventScope.Planet, pData.PopulationChangeLastTurn, null);
          }

          if (pData.PopulationChangeLastTurn < 0)
          {
              GameEvents.GameEvent.CreateNewStellarEvent(civ.ID, pData.Name, pData.SystemID, pData.ID, GameEvents.GameEvent.eEventType.PlanetLostPopulation, GameEvents.GameEvent.eEventLevel.Moderate, GameEvents.GameEvent.eEventScope.Planet, pData.PopulationChangeLastTurn, null);
          }

          // pop distress events
          if (pData.StarvationOnPlanet)
          {
              GameEvents.GameEvent.CreateNewStellarEvent(civ.ID, pData.Name, pData.SystemID, pData.ID, GameEvents.GameEvent.eEventType.StarvationOnPlanet, GameEvents.GameEvent.eEventLevel.Critical, GameEvents.GameEvent.eEventScope.Planet, pData.PopsStarvingOnPlanet, null);
          }

          if (pData.BlackoutsOnPlanet)
          {
              GameEvents.GameEvent.CreateNewStellarEvent(civ.ID, pData.Name, pData.SystemID, pData.ID, GameEvents.GameEvent.eEventType.BlackoutsOnPlanet, GameEvents.GameEvent.eEventLevel.Serious, GameEvents.GameEvent.eEventScope.Planet, pData.PopsBlackedOutOnPlanet, null);
          }

          if (pData.FarmsBuiltLastTurn > 0)
          {
              GameEvents.GameEvent.CreateNewStellarEvent(civ.ID, pData.Name, pData.SystemID, pData.ID, GameEvents.GameEvent.eEventType.NewFarmBuilt, GameEvents.GameEvent.eEventLevel.Positive, GameEvents.GameEvent.eEventScope.Planet, pData.FarmsBuiltLastTurn, null);
          }

          if (pData.HighTechBuiltLastTurn > 0)
          {
              GameEvents.GameEvent.CreateNewStellarEvent(civ.ID, pData.Name, pData.SystemID, pData.ID, GameEvents.GameEvent.eEventType.NewHighTechBuilt, GameEvents.GameEvent.eEventLevel.Positive, GameEvents.GameEvent.eEventScope.Planet, pData.HighTechBuiltLastTurn, null);
          }

          if (pData.FactoriesBuiltLastTurn > 0)
          {
              GameEvents.GameEvent.CreateNewStellarEvent(civ.ID, pData.Name, pData.SystemID, pData.ID, GameEvents.GameEvent.eEventType.NewFactoryBuilt, GameEvents.GameEvent.eEventLevel.Positive, GameEvents.GameEvent.eEventScope.Planet, pData.FactoriesBuiltLastTurn, null);
          }

          if (pData.MinesBuiltLastTurn > 0)
          {
              GameEvents.GameEvent.CreateNewStellarEvent(civ.ID, pData.Name, pData.SystemID, pData.ID, GameEvents.GameEvent.eEventType.NewMineBuilt, GameEvents.GameEvent.eEventLevel.Positive, GameEvents.GameEvent.eEventScope.Planet, pData.MinesBuiltLastTurn, null);
          }
      }

  }

}
