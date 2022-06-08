using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Util;
using Assets.Scripts.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace zach2039.CustomGas.Assets.Scripts.Atmospherics
{
    public class ModGasMixture : IRocketReaderWriter, ICloneable
    {
        public ModGasMixture()
        {
            this.IsValid = true;
            this._isCachable = false;
            this._temperatureCached = 0;
            this.Position = Vector3.zero;
            InitModMoles();
        }

        private void InitModMoles()
        {
            foreach (ModGasTemplate modGasTemplate in ModChemistry.GasTemplates)
            {
                this.ContainedGases.Add(modGasTemplate.ID, new ModMole(modGasTemplate, 0f, 0f));
            }
        }

        public object Clone()
        {
            ModGasMixture copyGasMix = new ModGasMixture();
            copyGasMix.IsValid = this.IsValid;
            copyGasMix._isCachable = this._isCachable;
            copyGasMix._temperatureCached = this._temperatureCached;
            copyGasMix.Position = this.Position;
            copyGasMix.ContainedGases = new Dictionary<int, ModMole>(this.ContainedGases);
            return copyGasMix;
        }

        public void Read(RocketBinaryReader reader)
        {
            this.Position = reader.ReadVector3();
            int numContainedGases = reader.ReadInt32();
            for (int i = 0; i < numContainedGases; i++)
            {
                int modMoleId = reader.ReadInt32();
                ModMole modMole = ModMole.Create(modMoleId);
                modMole.Read(reader);
                this.ContainedGases.Add(modMoleId, modMole);
            }
        }

        public void Write(RocketBinaryWriter writer)
        {
            writer.WriteVector3(this.Position);
            writer.WriteInt32(this.ContainedGases.Count);
            foreach (ModMole modMole in this.ContainedGases.Values)
            {
                writer.WriteInt32(modMole.ID);
                modMole.Write(writer);
            }
        }

        public void Cleanup()
        {
            foreach (ModMole modMole in this.ContainedGases.Values)
            {
                modMole.Cleanup();
            }
        }

        public void Reset()
        {
            this.ContainedGases.Clear();
        }

        public static ModMole[] ReadOnlyMoles(ModGasMixture gasMixture)
        {
            ModMole[] modMoles = new ModMole[gasMixture.ContainedGases.Count];
            gasMixture.ContainedGases.Values.CopyTo(modMoles, 0);
            return modMoles;
        }

        public void SetReadOnly(bool isReadOnly)
        {
            foreach (ModMole modMole in this.ContainedGases.Values)
            {
                modMole.ReadOnly = isReadOnly;
            }
        }

        public ModMole GetGasByName(string gasName)
        {
            foreach (ModMole modMole in this.ContainedGases.Values)
            {
                if (modMole.Name.ToLower() == gasName.ToLower())
                {
                    return modMole;
                } 
            }
            Debug.LogError("Could not find gas of name: " + gasName + ".");
            return ModMole.Invalid;
        }

        public ModMole GetGasById(int gasId)
        {
            ModMole modMole;
            this.ContainedGases.TryGetValue(gasId, out modMole);
            return modMole;
        }

        public float TotalMoles(Atmosphere.MatterState matterState)
        {
            float totalMoles = 0f;
            foreach (ModMole modMole in this.ContainedGases.Values)
            {
                if (modMole.MatterState == matterState || matterState == Atmosphere.MatterState.All)
                {
                    totalMoles += modMole.Quantity;
                }
            }
            return totalMoles;
        }

        public void UpdateCache()
        {
            this._temperatureCached = this.Temperature;
            foreach (ModMole modMole in this.ContainedGases.Values)
            {
                modMole.UpdateCache();
            }
        }

        public void Add(ModGasMixture newGasMix, Atmosphere.MatterState matterState = Atmosphere.MatterState.All)
        {
            if (!newGasMix.IsValid || newGasMix.TotalMoles(matterState) <= 0f || float.IsNaN(newGasMix.TotalMoles(matterState)))
            {
                return;
            }
            switch (matterState)
            {
                case Atmosphere.MatterState.Liquid:
                    this.AddLiquids(newGasMix);
                    return;
                case Atmosphere.MatterState.Gas:
                    this.AddGasses(newGasMix);
                    return;
                case Atmosphere.MatterState.All:
                    this.AddGasses(newGasMix);
                    this.AddLiquids(newGasMix);
                    return;
                default:
                    return;
            }
        }

        public void Set(ModGasMixture gasMixture, Atmosphere.MatterState matterState = Atmosphere.MatterState.All)
        {
            if (!gasMixture.IsValid)
            {
                return;
            }
            switch (matterState)
            {
                case Atmosphere.MatterState.Liquid:
                    this.SetLiquids(gasMixture);
                    return;
                case Atmosphere.MatterState.Gas:
                    this.SetGasses(gasMixture);
                    return;
                case Atmosphere.MatterState.All:
                    this.SetGasses(gasMixture);
                    this.SetLiquids(gasMixture);
                    return;
                default:
                    return;
            }
        }

        public void Scale(float ratio, Atmosphere.MatterState matterState = Atmosphere.MatterState.All)
        {
            switch (matterState)
            {
                case Atmosphere.MatterState.Liquid:
                    this.ScaleLiquids(ratio);
                    return;
                case Atmosphere.MatterState.Gas:
                    this.ScaleGasses(ratio);
                    return;
                case Atmosphere.MatterState.All:
                    this.ScaleGasses(ratio);
                    this.ScaleLiquids(ratio);
                    return;
                default:
                    return;
            }
        }

        public float GetGasTypeRatio(int gasId)
        {
            if (this.TotalMolesGassesAndLiquids == 0f)
            {
                return 0f;
            }
            ModMole modMole;
            this.ContainedGases.TryGetValue(gasId, out modMole);
            return modMole.Quantity / this.TotalMolesGassesAndLiquids;
            
        }

        public void Divide(float divideBy, Atmosphere.MatterState matterState = Atmosphere.MatterState.All)
        {
            switch (matterState)
            {
                case Atmosphere.MatterState.Liquid:
                    this.DivideLiquids(divideBy);
                    return;
                case Atmosphere.MatterState.Gas:
                    this.DivideGasses(divideBy);
                    return;
                case Atmosphere.MatterState.All:
                    this.DivideGasses(divideBy);
                    this.DivideLiquids(divideBy);
                    return;
                default:
                    return;
            }
        }

        public ModGasMixture Remove(ModGasMixture modGasMixture)
        {
            if (!modGasMixture.IsValid)
            {
                return ModGasMixture.Invalid;
            }
            ModGasMixture result = new ModGasMixture();
            return result;
        }

        public ModGasMixture Remove(float totalMolesRemoved, Atmosphere.MatterState stateToRemove)
        {
            float totalMolesOfState = this.TotalMoles(stateToRemove);
            float molesToRemove = Mathf.Min(totalMolesOfState, totalMolesRemoved);
            if (molesToRemove <= 0f)
            {
                return ModGasMixture.Invalid;
            }
            ModGasMixture result = new ModGasMixture();
            float moleRemovalPercent = molesToRemove / totalMolesOfState;
            foreach (ModMole modMole in this.ContainedGases.Values)
            {
                if (modMole.MatterState == stateToRemove || stateToRemove == Atmosphere.MatterState.All)
                {
                    result.GetGasById(modMole.ID).Add(modMole.Remove(modMole.Quantity * moleRemovalPercent));
                }
            }
            return result;
        }

        public ModMole Remove(int gasId, float quantity)
        {
            ModMole modMole;
            this.ContainedGases.TryGetValue(gasId, out modMole);
            return modMole.Remove(quantity);
        }

        public ModMole RemoveAll(int gasId)
        {
            ModMole modMole;
            this.ContainedGases.TryGetValue(gasId, out modMole);
            return this.Remove(gasId, modMole.Quantity);
        }

        public void LerpGasses(ModGasMixture gasMixture, float volumeRatio)
        {
            foreach (ModMole modMole in this.ContainedGases.Values)
            {
                int gasId = modMole.ID;
                ModMole otherModMole;
                gasMixture.ContainedGases.TryGetValue(gasId, out otherModMole);
                modMole.Lerp(otherModMole, volumeRatio);
            }
        }

        public void AddEnergy(float energy)
        {
            if (this.TotalMolesGassesAndLiquids <= Chemistry.MinimumValidTotalMoles || energy.IsDenormalOrZero() || float.IsNaN(energy))
            {
                return;
            }
            this.TotalEnergy += energy;
        }

        public float RemoveEnergy(float energy)
        {
            if (this.TotalMolesGassesAndLiquids <= Chemistry.MinimumValidTotalMoles || energy <= 0f || float.IsNaN(energy))
            {
                return 0f;
            }
            float energyLossConstrained = Mathf.Min(this.TotalEnergy, energy);
            this.TotalEnergy -= energyLossConstrained;
            return energyLossConstrained;
        }

        public float TransferEnergyTo(ref ModGasMixture targetGasMix, float signedEnergyToTransfer)
        {
            if (!targetGasMix.IsValid)
            {
                return 0f;
            }
            float totalHeatCapacity = targetGasMix.HeatCapacity + this.HeatCapacity;
            float totalEnergy = targetGasMix.TotalEnergy + this.TotalEnergy;
            float targetMixEnergyPortion = totalEnergy * (targetGasMix.HeatCapacity / totalHeatCapacity);
            float totalMinusTargetEnergyPortion = targetGasMix.TotalEnergy - targetMixEnergyPortion;
            float thisMixEnergyPortion = totalEnergy * (this.HeatCapacity / totalHeatCapacity);
            float totalMinusThisMixEnergyPortion = this.TotalEnergy - thisMixEnergyPortion;
            if (Mathf.Abs(signedEnergyToTransfer) > Mathf.Abs(totalMinusTargetEnergyPortion) || Mathf.Abs(signedEnergyToTransfer) > Mathf.Abs(totalMinusThisMixEnergyPortion))
            {
                signedEnergyToTransfer = (float)Math.Sign(signedEnergyToTransfer) * Mathf.Min(Math.Abs(totalMinusTargetEnergyPortion), Math.Abs(totalMinusThisMixEnergyPortion));
            }
            if (signedEnergyToTransfer < 0f)
            {
                return TransferEnergyTo(targetGasMix, this, -signedEnergyToTransfer);
            }
            float energyTransfered = this.RemoveEnergy(signedEnergyToTransfer);
            targetGasMix.AddEnergy(energyTransfered);
            return energyTransfered;
        }

        // FIXME: cant do a ref this in above, so bootleg it
        public static float TransferEnergyTo(ModGasMixture baseGasMix, ModGasMixture targetGasMix, float signedEnergyToTransfer)
        {
            if (!targetGasMix.IsValid)
            {
                return 0f;
            }
            float totalHeatCapacity = targetGasMix.HeatCapacity + baseGasMix.HeatCapacity;
            float totalEnergy = targetGasMix.TotalEnergy + baseGasMix.TotalEnergy;
            float targetMixEnergyPortion = totalEnergy * (targetGasMix.HeatCapacity / totalHeatCapacity);
            float totalMinusTargetEnergyPortion = targetGasMix.TotalEnergy - targetMixEnergyPortion;
            float thisMixEnergyPortion = totalEnergy * (baseGasMix.HeatCapacity / totalHeatCapacity);
            float totalMinusThisMixEnergyPortion = baseGasMix.TotalEnergy - thisMixEnergyPortion;
            if (Mathf.Abs(signedEnergyToTransfer) > Mathf.Abs(totalMinusTargetEnergyPortion) || Mathf.Abs(signedEnergyToTransfer) > Mathf.Abs(totalMinusThisMixEnergyPortion))
            {
                signedEnergyToTransfer = (float)Math.Sign(signedEnergyToTransfer) * Mathf.Min(Math.Abs(totalMinusTargetEnergyPortion), Math.Abs(totalMinusThisMixEnergyPortion));
            }
            if (signedEnergyToTransfer < 0f)
            {
                return targetGasMix.TransferEnergyTo(ref baseGasMix, -signedEnergyToTransfer);
            }
            float energyTransfered = baseGasMix.RemoveEnergy(signedEnergyToTransfer);
            targetGasMix.AddEnergy(energyTransfered);
            return energyTransfered;
        }

        private void AddGasses(ModGasMixture newGasMix)
        {
            if (!newGasMix.IsValid || newGasMix.TotalMolesLiquids <= 0f || float.IsNaN(newGasMix.TotalMolesLiquids))
            {
                return;
            }
            foreach (ModMole modMole in this.ContainedGases.Values)
            {
                if (modMole.MatterState == Atmosphere.MatterState.Gas)
                {
                    int gasId = modMole.ID;
                    ModMole otherModMole;
                    newGasMix.ContainedGases.TryGetValue(gasId, out otherModMole);
                    modMole.Set(otherModMole);
                }
            }
        }

        private void AddLiquids(ModGasMixture newGasMix)
        {
            if (!newGasMix.IsValid || newGasMix.TotalMolesLiquids <= 0f || float.IsNaN(newGasMix.TotalMolesLiquids))
            {
                return;
            }
            foreach (ModMole modMole in this.ContainedGases.Values)
            {
                if (modMole.MatterState == Atmosphere.MatterState.Liquid)
                {
                    int gasId = modMole.ID;
                    ModMole otherModMole;
                    newGasMix.ContainedGases.TryGetValue(gasId, out otherModMole);
                    modMole.Set(otherModMole);
                }
            }
        }

        private void SetGasses(ModGasMixture gasMixture)
        {
            foreach (ModMole modMole in this.ContainedGases.Values)
            {
                if (modMole.MatterState == Atmosphere.MatterState.Gas)
                {
                    int gasId = modMole.ID;
                    ModMole otherModMole;
                    gasMixture.ContainedGases.TryGetValue(gasId, out otherModMole);
                    modMole.Set(otherModMole);
                }
            }
        }

        private void SetLiquids(ModGasMixture gasMixture)
        {
            foreach (ModMole modMole in this.ContainedGases.Values)
            {
                if (modMole.MatterState == Atmosphere.MatterState.Liquid)
                {
                    int gasId = modMole.ID;
                    ModMole otherModMole;
                    gasMixture.ContainedGases.TryGetValue(gasId, out otherModMole);
                    modMole.Set(otherModMole);
                }
            }
        }

        private void ScaleGasses(float ratio)
        {
            foreach (ModMole modMole in this.ContainedGases.Values)
            {
                if (modMole.MatterState == Atmosphere.MatterState.Gas)
                {
                    modMole.Scale(ratio);
                }
            }
        }

        private void ScaleLiquids(float ratio)
        {
            foreach (ModMole modMole in this.ContainedGases.Values)
            {
                if (modMole.MatterState == Atmosphere.MatterState.Liquid)
                {
                    modMole.Scale(ratio);
                }
            }
        }

        private void DivideGasses(float divideBy)
        {
            foreach (ModMole modMole in this.ContainedGases.Values)
            {
                if (modMole.MatterState == Atmosphere.MatterState.Gas)
                {
                    modMole.Split(divideBy);
                }
            }
        }

        private void DivideLiquids(float divideBy)
        {
            foreach (ModMole modMole in this.ContainedGases.Values)
            {
                if (modMole.MatterState == Atmosphere.MatterState.Liquid)
                {
                    modMole.Split(divideBy);
                }
            }
        }

        public bool IsCachable
        {
            get
            {
                return this._isCachable;
            }
            set
            {
                this._isCachable = value;
                foreach (ModMole modMole in this.ContainedGases.Values)
                {
                    modMole.IsCachable = value;
                }
            }
        }

        public float ThermalEfficiency
        {
            get
            {
                float totalMolesGassesAndLiquids = this.TotalMolesGassesAndLiquids;
                float totalThermalEfficiency = 0f;
                foreach (ModMole modMole in this.ContainedGases.Values)
                {
                    totalThermalEfficiency += modMole.ThermalEfficiency * (modMole.Quantity / totalMolesGassesAndLiquids);
                }
                return totalThermalEfficiency;
            }
        }

        public float HeatCapacity
        {
            get
            {
                float totalHeatCapacity = 0f;
                foreach (ModMole modMole in this.ContainedGases.Values)
                {
                    totalHeatCapacity += modMole.HeatCapacity;
                }
                return totalHeatCapacity;
            }
        }

        public float TotalToxins
        {
            get
            {
                float totalToxinQuantity = 0f;
                foreach (ModMole modMole in this.ContainedGases.Values)
                {
                    if (modMole.IsToxic)
                    {
                        totalToxinQuantity += modMole.Quantity;
                    }
                }
                return totalToxinQuantity;
            }
        }

        public float TotalPollutants
        {
            get
            {
                float totalPollutantQuantity = 0f;
                foreach (ModMole modMole in this.ContainedGases.Values)
                {
                    if (modMole.IsPollutant)
                    {
                        totalPollutantQuantity += modMole.Quantity;
                    }
                }
                return totalPollutantQuantity;
            }
        }

        public float TotalFuel
        {
            get
            {
                float totalFuelQuantity = 0f;
                foreach (ModMole modMole in this.ContainedGases.Values)
                {
                    if (modMole.IsFuel)
                    {
                        totalFuelQuantity += modMole.Quantity;
                    }
                }
                return totalFuelQuantity;
            }
        }

        public float FuelHeatOfCombustion
        {
            get
            {
                float totalFuelQuantity = 0f;
                float totalEnthalpy = 0f;
                foreach (ModMole modMole in this.ContainedGases.Values)
                {
                    if (modMole.IsFuel)
                    {
                        totalFuelQuantity += modMole.Quantity;
                        totalEnthalpy += modMole.Enthalpy;
                    }
                }
                return totalFuelQuantity * totalEnthalpy;
            }
        }

        public float TotalEnthalpy
        {
            get
            {
                float totalEnthalpy = 0f;
                foreach (ModMole modMole in this.ContainedGases.Values)
                {
                    totalEnthalpy += modMole.Enthalpy;
                }
                return totalEnthalpy;
            }
        }

        public float TotalEnergy
        {
            get
            {
                float totalEnergy = 0f;
                foreach (ModMole modMole in this.ContainedGases.Values)
                {
                    totalEnergy += modMole.Energy;
                }
                return totalEnergy;
            }
            set
            {
                if (this.HeatCapacity.IsDenormalToNegative() || float.IsNaN(this.HeatCapacity))
                {
                    return;
                }
                if (value.IsDenormalOrNegative())
                {
                    value = 0f;
                }
                foreach (ModMole modMole in this.ContainedGases.Values)
                {
                    modMole.Energy = value * (modMole.HeatCapacity / this.HeatCapacity);
                }
            }
        }

        public float TotalMolesGassesAndLiquids
        {
            get
            {
                return this.TotalMoles(Atmosphere.MatterState.All);
            }
        }

        public float TotalMolesLiquids
        {
            get
            {
                return this.TotalMoles(Atmosphere.MatterState.Liquid);
            }
        }

        public float TotalMolesGasses
        {
            get
            {
                return this.TotalMoles(Atmosphere.MatterState.Gas);
            }
        }

        public float TotalMolesGassesAndLiquidsTradeCost
        {
            get
            {
                float tradeVal = 0f;
                foreach (ModMole modMole in this.ContainedGases.Values)
                { 
                    tradeVal += (modMole.Quantity * modMole.TradeValue);
                }
                return tradeVal;
            }
        }

        public float Temperature
        {
            get
            {
                if (this.IsCachable && !Atmosphere.CanWriteAccess)
                {
                    return this._temperatureCached;
                }
                if (this.HeatCapacity.IsDenormalToNegative())
                {
                    return 0f;
                }
                return this.TotalEnergy / this.HeatCapacity;
            }
        }

        public float HeatCapacityRatio
        {
            get
            {
                float totalMolesGassesAndLiquids = this.TotalMolesGassesAndLiquids;
                float ratio = 0f;
                foreach (ModMole modMole in this.ContainedGases.Values)
                {
                    ratio += (modMole.HeatCapacityRatio * (modMole.Quantity / totalMolesGassesAndLiquids));
                }
                return ratio;
            }
        }

        public Dictionary<int, ModMole> ContainedGases { get; set; } = new Dictionary<int, ModMole>();

        public bool IsValid { get; set; }

        public Vector3 Position { get; set; }

        public static readonly ModGasMixture Invalid;

        private bool _isCachable;

        private float _temperatureCached;
    }
}
