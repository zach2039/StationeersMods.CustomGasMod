using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.GridSystem;
using HarmonyLib;
using UnityEngine;

namespace zach2039.CustomGasMod
{
    [HarmonyPatch(typeof(Atmosphere))]
    public class Atmosphere_Patch
    {
        [HarmonyPatch("Combust", new Type[] { typeof(Atmosphere.MatterState) })]
        [HarmonyPrefix]
        public static bool Combust(Atmosphere __instance, Atmosphere.MatterState productType, ref bool ____inflamed)
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
    }
}

