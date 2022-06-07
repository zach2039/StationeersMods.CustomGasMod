using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Atmospherics;

namespace zach2039.CustomGasMod.Assets.Scripts.Atmospherics
{
    public class ModGasTemplate
    {
        private ModGasTemplate() {}

        public int ID { get; set; } = -1;

        public string Name { get; set; } = "Unknown";

        public string GasSymbol { get; set; } = "?";

        public Atmosphere.MatterState MatterState { get; set; } = Atmosphere.MatterState.Gas;

        public float HeatCapacityRatio { get; set; } = 1.666666f;

        public float ThermalEfficiency { get; set; } = 0.03f;

        public float SpecificHeat { get; set; } = 12f;

        public float Enthalpy { get; set; } = 0f;

        public float LowerFlammableLimit { get; set; } = 0f;

        public float UpperFlammableLimit { get; set; } = 0f;

        public float LimitingOxygenConcentration { get; set; } = 0f;

        public bool IsToxic { get; set; } = false;

        public bool IsPollutant { get; set; } = false;

        public bool IsFuel { get; set; } = false;

        public float TradeValue { get; set; } = 0.1f;

        public static readonly ModGasTemplate EmptyModGasTemplate = new ModGasTemplate();

        public class Builder
        {
            private ModGasTemplate modGasTemplate = new ModGasTemplate();

            public void ID(int iD)
            {
                modGasTemplate.ID = iD;
            }

            public void Name(string name)
            {
                modGasTemplate.Name = name;
            }

            public void GasSymbol(string gasSymbol)
            {
                modGasTemplate.GasSymbol = gasSymbol;
            }

            public void MatterState(Atmosphere.MatterState matterState)
            {
                modGasTemplate.MatterState = matterState;
            }

            public void SpecificHeat(float specificHeat)
            {
                modGasTemplate.SpecificHeat = specificHeat;
            }

            public void HeatCapacityRatio(float heatCapacityRatio)
            {
                modGasTemplate.HeatCapacityRatio = heatCapacityRatio;
            }

            public void ThermalEfficiency(float thermalEfficiency)
            {
                modGasTemplate.ThermalEfficiency = thermalEfficiency;
            }

            public void LowerFlammableLimit(float lowerFlammableLimit)
            {
                modGasTemplate.LowerFlammableLimit = lowerFlammableLimit;
            }

            public void UpperFlammableLimit(float upperFlammableLimit)
            {
                modGasTemplate.UpperFlammableLimit = upperFlammableLimit;
            }

            public void Enthalpy(float enthalpy)
            {
                modGasTemplate.Enthalpy = enthalpy;
            }

            public ModGasTemplate Build()
            {
                return modGasTemplate;
            }
        }
    }
}
