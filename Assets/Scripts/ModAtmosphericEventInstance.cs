using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Util;
using Assets.Scripts.Networking;
using Assets.Scripts.GridSystem;
using UnityEngine;
using UnityEngine.Networking;
using zach2039.CustomGas.Assets.Scripts.Atmospherics;

namespace zach2039.CustomGas.Assets.Scripts
{
	public class ModAtmosphericEventInstance
	{
		public static ModAtmosphericEventInstance CreateAddEnergy(Atmosphere atmosphere, float energy, bool ignite)
		{
			if (!GameManager.RunSimulation)
			{
				return null;
			}
			ModAtmosphericEventInstance atmosphericEventInstance = new ModAtmosphericEventInstance(atmosphere, energy, ignite);
			atmosphere.AtmosphericsController.OnThreadProcess += atmosphericEventInstance.AddEnergy;
			return atmosphericEventInstance;
		}

		public static ModAtmosphericEventInstance CreateRemoveEnergy(Atmosphere atmosphere, float energy)
		{
			if (!GameManager.RunSimulation)
			{
				return null;
			}
			ModAtmosphericEventInstance atmosphericEventInstance = new ModAtmosphericEventInstance(atmosphere, energy, false);
			atmosphere.AtmosphericsController.OnThreadProcess += atmosphericEventInstance.RemoveEnergy;
			return atmosphericEventInstance;
		}

		public static ModAtmosphericEventInstance CreateSet(Atmosphere atmosphere, ModGasMixture gasMixture)
		{
			if (!GameManager.RunSimulation)
			{
				return null;
			}
			ModAtmosphericEventInstance atmosphericEventInstance = new ModAtmosphericEventInstance(atmosphere, gasMixture);
			atmosphere.AtmosphericsController.OnThreadProcess += atmosphericEventInstance.SetGasMixture;
			return atmosphericEventInstance;
		}

		public static ModAtmosphericEventInstance CreateRemove(Atmosphere atmosphere, ModGasMixture gasMixture)
		{
			if (!GameManager.RunSimulation)
			{
				return null;
			}
			ModAtmosphericEventInstance atmosphericEventInstance = new ModAtmosphericEventInstance(atmosphere, gasMixture);
			atmosphere.AtmosphericsController.OnThreadProcess += atmosphericEventInstance.RemoveGasMixture;
			return atmosphericEventInstance;
		}

		public static ModAtmosphericEventInstance CreateAdd(Atmosphere atmosphere, ModGasMixture gasMixture)
		{
			if (!GameManager.RunSimulation)
			{
				return null;
			}
			ModAtmosphericEventInstance atmosphericEventInstance = new ModAtmosphericEventInstance(atmosphere, gasMixture);
			atmosphere.AtmosphericsController.OnThreadProcess += atmosphericEventInstance.AddGasMixture;
			return atmosphericEventInstance;
		}

		private ModAtmosphericEventInstance(Atmosphere atmosphere, ModGasMixture gasMixture)
		{
			this.Atmosphere = atmosphere;
			this.GasMixture = gasMixture;
			atmosphere.IsAwaitingEvent = true;
		}

		private ModAtmosphericEventInstance(Atmosphere atmosphere, float energy, bool ignite = false)
		{
			this.Atmosphere = atmosphere;
			this.Energy = energy;
			this.Ignite = ignite;
			atmosphere.IsAwaitingEvent = true;
		}

		public void AddGasMixture()
		{
			if (this.Atmosphere == null)
			{
				return;
			}
			AtmosphereExtension.Add(this.Atmosphere, this.GasMixture);
			this.Atmosphere.IsAwaitingEvent = false;
		}

		public void RemoveGasMixture()
		{
			if (this.Atmosphere == null)
			{
				return;
			}
			AtmosphereExtension.GetAdditionalData(this.Atmosphere).ModGasMixture.Remove(this.GasMixture);
			this.Atmosphere.IsAwaitingEvent = false;
		}

		public void SetGasMixture()
		{
			if (this.Atmosphere == null)
			{
				return;
			}
			AtmosphereExtension.GetAdditionalData(this.Atmosphere).ModGasMixture.Set(this.GasMixture, Atmosphere.MatterState.All);
			this.Atmosphere.IsAwaitingEvent = false;
		}

		public void AddEnergy()
		{
			if (this.Atmosphere == null)
			{
				return;
			}
			AtmosphereExtension.GetAdditionalData(this.Atmosphere).ModGasMixture.AddEnergy(this.Energy);
			if (this.Ignite)
			{
				this.Atmosphere.Ignited = true;
			}
			this.Atmosphere.IsAwaitingEvent = false;
		}

		public void RemoveEnergy()
		{
			if (this.Atmosphere == null)
			{
				return;
			}
			AtmosphereExtension.GetAdditionalData(this.Atmosphere).ModGasMixture.RemoveEnergy(this.Energy);
			this.Atmosphere.IsAwaitingEvent = false;
		}

		public Grid3 Grid;

		public Atmosphere Atmosphere;

		public ModGasMixture GasMixture;

		public float Energy;

		public bool Ignite;
	}
}
