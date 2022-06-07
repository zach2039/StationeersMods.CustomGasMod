using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Objects;
using HarmonyLib;
using UnityEngine;
using zach2039.CustomGasMod.Assets.Scripts.Atmospherics;
using zach2039.CustomGasMod.Assets.Scripts.Objects.Items;

namespace Assets.Scripts.Atmospherics
{
	[HarmonyPatch(typeof(InternalAtmosphereConditioner))]
	public class InternalAtmosphereConditioner_Patch
	{
		[HarmonyPatch("HandleFilters")]
		[HarmonyPrefix]
		public static bool HandleFilters_Patch(InternalAtmosphereConditioner __instance)
		{
			List<Slot> filterSlots = __instance.Parent.GetFilterSlots() as List<Slot>;
			for (int i = 0; i < filterSlots.Count; i++)
			{
				ModGasFilter gasFilter = filterSlots[i].Occupant as ModGasFilter;
				if (gasFilter)
				{
					ModGasMixture internalGasMixture = __instance.InternalAtmosphere.GetAdditionalData().ModGasMixture;
					ModGasMixture wasteGasMixture = __instance.WasteTank.InternalAtmosphere.GetAdditionalData().ModGasMixture;
					gasFilter.FilterGas(ref internalGasMixture, ref wasteGasMixture);
				}
			}
			return false; // skip original method
		}
	}
}