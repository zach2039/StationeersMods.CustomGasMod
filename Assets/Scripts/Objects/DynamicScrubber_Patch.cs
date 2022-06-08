using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Util;
using Assets.Scripts.UI;
using HarmonyLib;
using Assets.Scripts.Networking;
using UI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.UI;
using zach2039.CustomGas.Assets.Scripts.Atmospherics;

namespace Assets.Scripts.Objects
{
	[HarmonyPatch(typeof(DynamicScrubber))]
	public class DynamicScrubber_Patch
	{
		[HarmonyPatch("GetPassiveTooltip", new Type[] { typeof(Collider) })]
		[HarmonyPrefix]
		public static bool GetPassiveTooltip_Patch(DynamicScrubber __instance, ref PassiveTooltip __result, Collider hitCollider, ref StringBuilder ____tooltipBuilder)
		{
			ModGasMixture gasMixture = __instance.InternalAtmosphere.GetAdditionalData().ModGasMixture;
			____tooltipBuilder.Clear();
			if (!(hitCollider == __instance.InfoPanel))
			{
				__result = (__instance as PortableAtmospherics).GetPassiveTooltip(hitCollider);
				return false; // skip original method
			}
			PassiveTooltip result = new PassiveTooltip(true);
			result.Title = (__instance as PortableAtmospherics).DisplayName;
			result.Description = string.Format("Pressure {0}", __instance.InternalAtmosphere.PressureGassesAndLiquidsInPa.ToStringPrefix("Pa", "yellow"));
			if (!__instance.OnOff || !__instance.Powered)
			{
				__result = result;
				return false; // skip original method
			}
			if (__instance.IsOpen)
			{
				____tooltipBuilder.Append("<color=red>Vent is open, internal atmosphere will be released</color>\n");
			}
			if (!__instance.HasFilters)
			{
				____tooltipBuilder.Append("<color=red>No Filters loaded, will take all atmosphere</color>\n");
			}
			if (__instance.ExceededPressure)
			{
				____tooltipBuilder.Append("<color=red>Maximum internal pressure exceeded.</color> Connect to pipe network to release pressure.\n");
			}
			____tooltipBuilder.Append(string.Format("Pressure: <color=green>{0:F0}</color> kPa\nTemperature: <color=green>{1:F0}</color> K", __instance.InternalAtmosphere.PressureGassesAndLiquids, __instance.InternalAtmosphere.Temperature));
			foreach (ModMole modMole in gasMixture.ContainedGases.Values)
            {
				____tooltipBuilder.Append(ModAtmosphericsManager.DisplayGas(gasMixture, modMole, "F0"));
			}
			
			result.Description = ____tooltipBuilder.ToString();
			__result = result;
			return false; // skip original method
		}
	}
}