using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Util;
using Assets.Scripts.Networking;
using UnityEngine;

namespace zach2039.CustomGas.Assets.Scripts.Atmospherics
{
    public class ModMole : UnityEngine.Networking.IRocketReaderWriter, ICloneable
    {
        private ModMole() {}

        public ModMole(int gasId, float quantity = 0f, float energy = 0f)
        {
            Init(ModChemistry.FindGasTemplateById(gasId), quantity, energy);
        }

        public ModMole(ModGasTemplate gasTemplate, float quantity = 0f, float energy = 0f)
        {
            Init(gasTemplate, quantity, energy);
        }

        private void Init(ModGasTemplate gasTemplate, float quantity, float energy)
        {
            this.IsValid = true;
            this.ReadOnly = false;
            this.IsCachable = false;
            this.ID = gasTemplate.ID;
            this._energy = energy;
            this._energyCached = energy;
            this._quantity = quantity;
            this._quantityCached = quantity;
            this.SpecificHeat = gasTemplate.SpecificHeat;
            this.Enthalpy = gasTemplate.Enthalpy;
            this.LowerFlammableLimit = gasTemplate.LowerFlammableLimit;
            this.UpperFlammableLimit = gasTemplate.UpperFlammableLimit;
            this.LimitingOxygenConcentration = gasTemplate.LimitingOxygenConcentration;
            this.QuantityDirty = true;
            this.EnergyDirty = true;
            this._lastQuantityDirtied = 0f;
            this._lastEnergyDirtied = 0f;
        }

        public object Clone()
        {
            ModMole copyModMole = new ModMole();
            copyModMole.IsValid = this.IsValid;
            copyModMole.ReadOnly = this.ReadOnly;
            copyModMole.IsCachable = this.IsCachable;
            copyModMole.ID = this.ID;
            copyModMole._energy = this._energy;
            copyModMole._energyCached = this._energyCached;
            copyModMole._quantity = this._quantity;
            copyModMole._quantityCached = this._quantityCached;
            copyModMole.SpecificHeat = this.SpecificHeat;
            copyModMole.Enthalpy = this.Enthalpy;
            copyModMole.LowerFlammableLimit = this.LowerFlammableLimit;
            copyModMole.UpperFlammableLimit = this.UpperFlammableLimit;
            copyModMole.LimitingOxygenConcentration = this.LimitingOxygenConcentration;
            copyModMole.QuantityDirty = this.QuantityDirty;
            copyModMole.EnergyDirty = this.EnergyDirty;
            copyModMole._lastQuantityDirtied = this._lastQuantityDirtied;
            copyModMole._lastEnergyDirtied = this._lastEnergyDirtied;
            return copyModMole;
        }

        public void Read(RocketBinaryReader reader)
        {
            this.ReadOnly = reader.ReadBoolean();
            this._energyCached = reader.ReadSingle();
            this._quantityCached = reader.ReadSingle();
            this._energy = reader.ReadSingle();
            this._quantity = reader.ReadSingle();
        }

        public void Write(RocketBinaryWriter writer)
        {
            writer.WriteBoolean(this.ReadOnly);
            writer.WriteSingle(this._energyCached);
            writer.WriteSingle(this._quantityCached);
            writer.WriteSingle(this._energy);
            writer.WriteSingle(this._quantity);
        }

        public static ModMole Create(int gasId)
        {
            return new ModMole(ModChemistry.FindGasTemplateById(gasId), 0f, 0f);
        }

        public static ModMole Create(int gasId, float quantity, float energy)
        {
            return new ModMole(ModChemistry.FindGasTemplateById(gasId), quantity, energy);
        }

        public static ModMole Generate(string mole)
        {
            ModGasTemplate modGasTemplate = ModChemistry.FindGasTemplateByName(mole);
            return new ModMole(modGasTemplate, 0f, 0f);
        }

        public void Clear()
        {
            this.Quantity = 0f;
            this.Energy = 0f;
            this.QuantityDirty = true;
            this.EnergyDirty = true;
            this.UpdateCache();
        }

        public void Cleanup()
        {
            if (this.Quantity < Chemistry.MinimumQuantityMoles)
            {
                this.Clear();
            }
        }

        public void UpdateCache()
        {
            this._energyCached = this._energy;
            if (NetworkManager.IsServer && RocketMath.ApproximatelyByRatio(this._energyCached, this._lastEnergyDirtied, 0.02f))
            {
                this.EnergyDirty = true;
                this._lastEnergyDirtied = this._energyCached;
            }
            this._quantityCached = this._quantity;
            if (NetworkManager.IsServer && !RocketMath.ApproximatelyByRatio(this._quantityCached, this._lastQuantityDirtied, 0.02f))
            {
                this.QuantityDirty = true;
                this._lastQuantityDirtied = this._quantityCached;
            }
        }

        public void Undirty()
        {
            this.QuantityDirty = false;
            this.EnergyDirty = false;
        }

        public void Add(float quantity, float energy)
        {
            if (!this.IsValid)
            {
                return;
            }
            if (quantity.IsDenormalOrNegative() || energy.IsDenormalOrNegative())
            {
                return;
            }
            if (float.IsNaN(quantity) || float.IsNaN(energy))
            {
                Debug.LogError("Atmosphere: Energy being set to NaN/Denormal");
                return;
            }
            if (3.4028235E+38f - this.Quantity < quantity || 3.4028235E+38f - this.Energy < energy)
            {
                Debug.LogError("Atmosphere: Avoid float overflow on Add");
                return;
            }
            this.Quantity += quantity;
            this.Energy += energy;
        }

        public void Add(float quantity)
        {
            float energy = quantity * this.SpecificHeat * Chemistry.Temperature.TwentyDegrees;
            this.Add(quantity, energy);
        }

        public void Add(ModMole mole)
        {
            if (!mole.IsValid)
            {
                return;
            }
            this.Add(mole.Quantity, mole.Energy);
        }

        public ModMole Remove(float removedMoles)
        {
            removedMoles = Mathf.Min(this.Quantity, removedMoles);
            if (float.IsNaN(this.Quantity) || float.IsNaN(removedMoles))
            {
                Debug.LogWarning("Atmosphere: Skipping remove moles due to NaN/Denormal");
                return ModMole.Invalid;
            }
            if (this.Quantity.IsDenormalToNegative())
            {
                return ModMole.Invalid;
            }
            ModMole result = Create(this.ID);
            float num = removedMoles / this.Quantity * this.Energy;
            this.Quantity -= removedMoles;
            this.Energy -= num;
            result.Add(removedMoles, num);
            return result;
        }

        public void Set(ModMole newMole)
        {
            if (!newMole.IsValid)
            {
                return;
            }
            this.Set(newMole.Quantity, newMole.Energy);
        }

        public void Set(float quantity, float energy)
        {
            if (quantity.IsDenormal() || quantity < 0f || energy.IsDenormal() || energy < 0f)
            {
                return;
            }
            this.Quantity = quantity;
            this.Energy = energy;
        }

        public void Set(float quantity)
        {
            this.Set(quantity, 0f);
        }

        public void Scale(float scaleFactor)
        {
            if (scaleFactor.IsDenormal())
            {
                scaleFactor = 0f;
            }
            if (float.IsNaN(scaleFactor))
            {
                return;
            }
            this.Quantity *= scaleFactor;
            this.Energy *= scaleFactor;
        }

        public void Split(float divideBy)
        {
            if (divideBy.IsDenormalOrZero() || float.IsNaN(divideBy))
            {
                return;
            }
            this.Quantity /= divideBy;
            this.Energy /= divideBy;
        }

        public void Lerp(ModMole target, float volumeRatio)
        {
            this.Quantity = Mathf.Lerp(this.Quantity, target.Quantity * volumeRatio, AtmosphericsManager.Instance.LerpRate);
            this.Energy = Mathf.Lerp(this.Energy, target.Energy * volumeRatio, AtmosphericsManager.Instance.LerpRate);
        }

        public string DisplayName
        {
            get
            {
                //return Assets.Scripts.Localization.GetName(this.Name);
                return this.Name;
            }
        }

        public string MoleDescription
        {
            get
            {
                return Localization.GetInterface(Animator.StringToHash("Mole" + this.Name + "Description"), false);
            }
        }

        public float Energy
        {
            get
            {
                if (this.IsCachable && (!Atmosphere.CanWriteAccess || NetworkManager.IsClient))
                {
                    return this._energyCached;
                }
                return this._energy;
            }
            set
            {
                if (float.IsNaN(value))
                {
                    Debug.LogError("Atmosphere: Energy being set to NaN");
                    return;
                }
                if (!value.IsDenormal() && !this.ReadOnly)
                {
                    this._energy = value;
                }
            }
        }

        public float Temperature
		{
			get
			{
				if (!this.IsValid)
				{
					return 0f;
				}
				if (!this.HeatCapacity.IsDenormalOrZero())
				{
					return this.Energy / this.HeatCapacity;
				}
				Debug.LogWarning("Atmosphere: HeatCapacity is Denormal or Zero on get temperature");
				return 0f;
			}
		}

        public float Quantity
        {
            get
            {
                if (this.IsCachable && (!Atmosphere.CanWriteAccess || NetworkManager.IsClient))
                {
                    return this._quantityCached;
                }
                return this._quantity;
            }
            set
            {
                if (!this.ReadOnly)
                {
                    this._quantity = value;
                }
            }
        }

        public float HeatCapacity
        {
            get
            {
                return this.SpecificHeat * this.Quantity;
            }
        }

        public int ID { get; private set; }

        public string Name { get; private set; }

        public string GasSymbol { get; private set; }

        public float HeatCapacityRatio { get; private set; }

        public float ThermalEfficiency { get; private set; }

        public float SpecificHeat { get; private set; }

        public float Enthalpy { get; private set; }

        public float LowerFlammableLimit { get; private set; }

        public float UpperFlammableLimit { get; private set; }

        public float LimitingOxygenConcentration { get; private set; }

        public bool IsToxic { get; set; } = false;

        public bool IsPollutant { get; set; } = false;

        public bool IsFuel { get; set; } = false;

        public float TradeValue { get; set; } = 0.1f;

        public Atmosphere.MatterState MatterState { get; private set; }

        public bool IsValid { get; private set; }

        public static ModMole Invalid { get; }

        public bool IsCachable { set; get; }

        public bool ReadOnly { set; get; }

        public bool QuantityDirty { get; private set; }

        public bool EnergyDirty { get; private set; }

        private float _energyCached;

        private float _quantityCached;

        private float _energy;

        private float _quantity;

        private float _lastEnergyDirtied;

        private float _lastQuantityDirtied;
    }
}
