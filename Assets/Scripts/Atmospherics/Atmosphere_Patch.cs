using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Util;
using HarmonyLib;
using UnityEngine;
using zach2039.CustomGasMod.Assets.Scripts.Atmospherics;

namespace Assets.Scripts.Atmospherics
{
	[HarmonyPatch(typeof(Atmosphere))]
	public class Atmosphere_Patch
	{
		[HarmonyPatch("Combust", new Type[] { typeof(Atmosphere.MatterState) })]
		[HarmonyPrefix]
		public static bool Combust_Patch(Atmosphere __instance, Atmosphere.MatterState productType, ref bool ____inflamed)
		{
			ModGasMixture gasMixture = __instance.GetAdditionalData().ModGasMixture;
			float totalFuel = gasMixture.TotalFuel;
			float quantity = gasMixture.GetGasByName("oxygen").Quantity;
			float num = Mathf.Min(totalFuel, quantity * 2f);
			float num2 = num / 2f;
			float num3 = 0.95f;
			float num4 = num * num3;
			float removedMoles = num2 * num3;
			ModMole mole = gasMixture.GetGasByName("volatiles").Remove(num4); // FIXME: Need to remove fuel gases instead of just volatiles.
			mole.Add(gasMixture.GetGasByName("oxygen").Remove(removedMoles));
			float num5 = (productType == Atmosphere.MatterState.All) ? 0.5f : 1f;
			if (productType == Atmosphere.MatterState.Liquid || productType == Atmosphere.MatterState.All)
			{
				ModMole waterMole = gasMixture.GetGasByName("water");
				waterMole.Add(new ModMole(waterMole.ID, num5 * mole.Quantity / 3f, 0f)
				{
					Energy = num5 * mole.Energy
				});
			}
			else if (productType == Atmosphere.MatterState.Gas || productType == Atmosphere.MatterState.All)
			{
				ModMole pollutantMole = gasMixture.GetGasByName("pollutant");
				ModMole mole2 = ModMole.Create(pollutantMole.ID, num5 * mole.Quantity, 0f);
				mole2.Energy = num5 * mole.Energy;
				ModMole carbondioxideMole = gasMixture.GetGasByName("carbondioxide");
				ModMole mole3 = ModMole.Create(carbondioxideMole.ID, num5 * 3f * num4, 0f);
				carbondioxideMole.Add(mole3);
				pollutantMole.Add(mole2);
			}
			__instance.CombustionEnergy = gasMixture.TotalEnthalpy * num4;
			gasMixture.AddEnergy(__instance.CombustionEnergy);
			__instance.BurnedPropaneRatio = Mathf.Clamp(num4 / 10f, 0f, 1f);
			__instance.CleanBurnRate = __instance.CombustableMix();
			____inflamed = true;
			return false; // skip original method
		}

		[HarmonyPatch("CombustableMix")]
		[HarmonyPrefix]
		public static bool CombustableMix_Patch(Atmosphere __instance, ref float __result)
		{
			ModGasMixture gasMixture = __instance.GetAdditionalData().ModGasMixture;
			__result = Mathf.Clamp01(gasMixture.GetGasByName("oxygen").Quantity / gasMixture.TotalFuel * 2f);
			return false; // skip original method
		}

		[HarmonyPatch("FireLevel")]
		[HarmonyPrefix]
		public static bool FireLevel_Patch(Atmosphere __instance, ref float __result)
		{
			ModGasMixture gasMixture = __instance.GetAdditionalData().ModGasMixture;
			float totalFuel = gasMixture.TotalFuel;
			float quantity = gasMixture.GetGasByName("oxygen").Quantity;
			if (totalFuel < 0f || quantity < 0f)
			{
				__result = 0f;
				return false; // skip original method
			}
			float num = 0.95f;
			__result = Atmosphere.FireLevelMultiplier * __instance.CombustableMix() * num;
			return false; // skip original method
		}

		[HarmonyPatch("Init", new Type[] { typeof(Grid3), typeof(GridController), typeof(long) })]
		[HarmonyPostfix]
		public static void Init_Patch(Atmosphere __instance)
		{
			ModGasMixture gasMixture = __instance.GetAdditionalData().ModGasMixture;
			gasMixture.GetGasByName("water").Clear();
			gasMixture.GetGasByName("water").ReadOnly = true;
		}

		[HarmonyPatch("IsCloseToGlobal", new Type[] { typeof(float) })]
		[HarmonyPrefix]
		public static bool IsCloseToGlobal_Patch(Atmosphere __instance, ref bool __result, float minPressure)
		{
			ModGasMixture gasMixture = __instance.GetAdditionalData().ModGasMixture;
			ModGasMixture globalGasMixture = AtmosphericsController.GlobalAtmosphere.GetAdditionalData().ModGasMixture;
			if (AtmosphericsController.GlobalAtmosphere == null)
			{
				__result = false;
				return false; // skip original method
			}
			if (Mathf.Abs(AtmosphericsController.GlobalAtmosphere.PressureGasses - __instance.PressureGasses) > minPressure)
			{
				__result = false;
				return false; // skip original method
			}
			float num = 0f;
			float num2 = AtmosphericsController.GlobalAtmosphere.Volume / __instance.Volume;
			foreach (int gasId in gasMixture.ContainedGases.Keys)
            {
				num += Math.Abs(globalGasMixture.GetGasById(gasId).Quantity - gasMixture.GetGasById(gasId).Quantity * num2);

			}
			__result = num <= minPressure;
			return false; // skip original method
		}

		[HarmonyPatch("ShouldCreateNewWorldAtmos", new Type[] { typeof(float) })]
		[HarmonyPrefix]
		public static bool ShouldCreateNewWorldAtmos_Patch(ref bool __result, float energyToTransfer)
		{
			ModGasMixture newGasMix = AtmosphericsController.GlobalAtmosphere.GetAdditionalData().ModGasMixture.Clone() as ModGasMixture;
			newGasMix.AddEnergy(energyToTransfer);
			__result = !ModAtmosphere.IsCloseToGlobal(Chemistry.MinimumWorldValidTotalMoles, newGasMix);
			return false; // skip original method
		}

		[HarmonyPatch("Load", new Type[] { typeof(AtmosphereSaveData) })]
		[HarmonyPrefix]
		public static bool Load_Patch(Atmosphere __instance, AtmosphereSaveData saveData, ref Grid3 ____grid, ref Vector3 ____worldPosition)
		{
			ModGasMixture gasMixture = __instance.GetAdditionalData().ModGasMixture;
			____worldPosition = saveData.Position;
			if (__instance.ParentGridController != null)
			{
				____grid = __instance.ParentGridController.WorldToLocalGrid(____worldPosition, 2f, 0f);
			}
			else
			{
				____grid = ____worldPosition.ToGrid(2f, 0f);
			}
			gasMixture.Reset();
			__instance.Volume = saveData.Volume;
			__instance.Direction = saveData.Direction;
			foreach (int gasId in saveData.GetAdditionalData().GasSaveData.Keys)
            {
				float amt = 0f;
				saveData.GetAdditionalData().GasSaveData.TryGetValue(gasId, out amt);
				gasMixture.GetGasById(gasId).Add(amt);
            }
			gasMixture.TotalEnergy = saveData.Energy;
			__instance.CleanBurnRate = saveData.CleanBurnRate;
			if (saveData.Energy <= 0f && gasMixture.TotalMolesGassesAndLiquids > 0f)
			{
				gasMixture.TotalEnergy = Chemistry.Temperature.TwentyDegrees * gasMixture.HeatCapacity;
			}
			__instance.UpdateCache();
			return false; // skip original method
		}

		[HarmonyPatch("UpdateCache")]
		[HarmonyPrefix]
		public static bool UpdateCache_Patch(Atmosphere __instance, ref Grid3 ____grid, ref Vector3 ____worldPosition,
			ref float ____particalPressureO2Cached,
			ref float ____particalPressureN2OCached,
			ref float ____particalPressureVolatilesCached,
			ref float ____particalPressurePollutantsCached,
			ref float ____particalPressureToxinsCached,
			ref float ____pressureGassesCached,
			ref float ____pressureGassesAndLiquidsCached,
			ref float ____pressureLiquidCached,
			ref float ____totalMolesCached)
		{
			ModGasMixture gasMixture = __instance.GetAdditionalData().ModGasMixture;
			____particalPressureO2Cached = __instance.ParticalPressureO2;
			____particalPressureN2OCached = __instance.ParticalPressureN2O;
			____particalPressureVolatilesCached = __instance.ParticalPressureVolatiles;
			____particalPressurePollutantsCached = __instance.ParticalPressurePollutants;
			____particalPressureToxinsCached = __instance.ParticalPressureToxins;
			____pressureGassesCached = __instance.PressureGasses;
			____pressureGassesAndLiquidsCached = __instance.PressureGassesAndLiquids;
			____pressureLiquidCached = __instance.PressureLiquids;
			____totalMolesCached = __instance.TotalMoles;
			gasMixture.UpdateCache();
			return false; // skip original method
		}
	}
}

