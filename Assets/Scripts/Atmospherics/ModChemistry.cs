using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Atmospherics;
using UnityEngine;

namespace zach2039.CustomGas.Assets.Scripts.Atmospherics
{
    public static class ModChemistry
    {
        public static List<ModGasTemplate> GasTemplates = new List<ModGasTemplate>()
        {
            new ModGasTemplate.Builder()
                .ID(1)
                .Name("Oxygen")
                .GasSymbol("O2")
                .MatterState(Atmosphere.MatterState.Gas)
                .SpecificHeat(ModChemistry.SpecificHeatOxygen)
                .HeatCapacityRatio(ModChemistry.DiatomicDegreesOfFreedom)
                .ThermalEfficiency(0.12f)
                .Build(),
            new ModGasTemplate.Builder()
                .ID(32)
                .Name("Water")
                .GasSymbol("HO2")
                .MatterState(Atmosphere.MatterState.Liquid)
                .SpecificHeat(ModChemistry.SpecificHeatWater)
                .HeatCapacityRatio(ModChemistry.TriatomicDegreesOfFreedom)
                .ThermalEfficiency(0.05f)
                .Build()
        };

        public static string GetSymbol(int gasId)
        {
            ModGasTemplate gasTemplate = FindGasTemplateById(gasId);
            return gasTemplate.GasSymbol;
        }

        public static ModGasTemplate FindGasTemplateByName(string gasName)
        {
            foreach (ModGasTemplate modGasTemplate in ModChemistry.GasTemplates)
            {
                if (modGasTemplate.Name.ToLower() == gasName.ToLower())
                {
                    return modGasTemplate;
                }
            }
            throw new ArgumentException("Could not find gas template of name: " + gasName + ".");
        }

        public static ModGasTemplate FindGasTemplateById(int gasId)
        {
            foreach (ModGasTemplate modGasTemplate in ModChemistry.GasTemplates)
            {
                if (modGasTemplate.ID == gasId)
                {
                    return modGasTemplate;
                }
            }
            throw new ArgumentException("Could not find gas template of id: " + gasId + ".");
        }

        public static readonly float MonatomicDegreesOfFreedom = 1.666666f;

        public static readonly float DiatomicDegreesOfFreedom = 1.4f;

        public static readonly float TriatomicDegreesOfFreedom = 1.333333f;

        public static readonly float PolyatomicDegreesOfFreedom = 1.26f;

        public static readonly float SpecificHeatOxygen = 21.1f;

        public static readonly float SpecificHeatNitrogen = 20.6f;

        public static readonly float SpecificHeatCarbonDioxide = 28.2f;

        public static readonly float SpecificHeatVolatiles = 20.4f;

        public static readonly float SpecificHeatPollutant = 24.8f;

        public static readonly float SpecificHeatWater = 72f;

        public static readonly float SpecificHeatNitrousOxide = 23f;

        public static readonly float EnthalpyVolatiles = 286000f;
    }
}
