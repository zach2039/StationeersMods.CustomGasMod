using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.Atmospherics;

namespace zach2039.CustomGasMod.Assets.Scripts.Atmospherics
{
    public class ModAtmosphere : Atmosphere
    {
		public static bool FilterGas(int filterType, ref ModGasMixture fromMix, ref ModGasMixture tooMix)
		{
			ModMole mole = fromMix.RemoveAll(filterType);
			bool result = mole.Quantity > 0f;
			tooMix.GetGasById(filterType).Add(mole);
			return result;
		}

		public static float FilterGas(int filterType, ref ModGasMixture fromMix, ref ModGasMixture tooMix, Atmosphere atmosphere, float minRatio)
		{
			ModMole mole = fromMix.RemoveAll(filterType);
			float quantity = mole.Quantity;
			tooMix.GetGasById(filterType).Add(mole);
			if (atmosphere.GetAdditionalData().ModGasMixture.GetGasById(filterType).Quantity / atmosphere.TotalMoles < minRatio)
			{
				tooMix.GetGasById(filterType).Add(atmosphere.GetAdditionalData().ModGasMixture.RemoveAll(filterType));
			}
			return quantity;
		}

		public static bool IsCloseToGlobal(float minPressure, ModGasMixture newGasMix)
		{
			ModGasMixture globalGasMixture = AtmosphericsController.GlobalAtmosphere.GetAdditionalData().ModGasMixture;
			float num = newGasMix.TotalMolesGasses * 8.3144f * newGasMix.Temperature / AtmosphericsController.GlobalAtmosphere.Volume;
			float gasDiff = 0f;
			foreach (int gasId in newGasMix.ContainedGases.Keys)
			{
				num += Math.Abs(globalGasMixture.GetGasById(gasId).Quantity - newGasMix.GetGasById(gasId).Quantity);
			}
			return (Mathf.Abs(AtmosphericsController.GlobalAtmosphere.PressureGasses - num) <= minPressure) && (gasDiff <= minPressure);
		}
	}
}