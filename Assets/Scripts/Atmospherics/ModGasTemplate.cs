using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Atmospherics;

namespace zach2039.CustomGas.Assets.Scripts.Atmospherics
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

        public bool CanStun { get; set; } = false;

        public float TradeValue { get; set; } = 0.1f;

        public static readonly ModGasTemplate EmptyModGasTemplate = new ModGasTemplate();

        public class Builder
        {
            private ModGasTemplate modGasTemplate = new ModGasTemplate();

            public Builder ID(int iD)
            {
                modGasTemplate.ID = iD;
                return this;
            }

            public Builder Name(string name)
            {
                modGasTemplate.Name = name;
                return this;
            }

            public Builder GasSymbol(string gasSymbol)
            {
                modGasTemplate.GasSymbol = gasSymbol;
                return this;
            }

            public Builder MatterState(Atmosphere.MatterState matterState)
            {
                modGasTemplate.MatterState = matterState;
                return this;
            }

            public Builder SpecificHeat(float specificHeat)
            {
                modGasTemplate.SpecificHeat = specificHeat;
                return this;
            }

            public Builder HeatCapacityRatio(float heatCapacityRatio)
            {
                modGasTemplate.HeatCapacityRatio = heatCapacityRatio;
                return this;
            }

            public Builder ThermalEfficiency(float thermalEfficiency)
            {
                modGasTemplate.ThermalEfficiency = thermalEfficiency;
                return this;
            }

            public Builder LowerFlammableLimit(float lowerFlammableLimit)
            {
                modGasTemplate.LowerFlammableLimit = lowerFlammableLimit;
                return this;
            }

            public Builder UpperFlammableLimit(float upperFlammableLimit)
            {
                modGasTemplate.UpperFlammableLimit = upperFlammableLimit;
                return this;
            }

            public Builder LimitingOxygenConcentration(float limitingOxygenConcentration)
            {
                modGasTemplate.LimitingOxygenConcentration = limitingOxygenConcentration;
                return this;
            }

            public Builder Enthalpy(float enthalpy)
            {
                modGasTemplate.Enthalpy = enthalpy;
                return this;
            }

            public Builder IsToxic()
            {
                modGasTemplate.IsToxic = true;
                return this;
            }

            public Builder IsPollutant()
            {
                modGasTemplate.IsPollutant = true;
                return this;
            }

            public Builder IsFuel()
            {
                modGasTemplate.IsFuel = true;
                return this;
            }

            public Builder CanStun()
            {
                modGasTemplate.CanStun = true;
                return this;
            }

            public ModGasTemplate Build()
            {
                return modGasTemplate;
            }
        }
    }
}
