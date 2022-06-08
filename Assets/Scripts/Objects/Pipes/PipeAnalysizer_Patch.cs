using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Util;
using Assets.Scripts.UI;
using Assets.Scripts.Networks;
using HarmonyLib;
using Assets.Scripts.Networking;
using UI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.UI;
using zach2039.CustomGas.Assets.Scripts.Atmospherics;

namespace Assets.Scripts.Objects.Pipes
{
	[HarmonyPatch(typeof(PipeAnalysizer))]
	public class PipeAnalysizer_Patch
	{
		private static bool hasReadableAtmosphere(PipeAnalysizer instance)
		{
			if (instance.SmallCell != null && instance.SmallCell.Pipe != null)
			{
				PipeNetwork pipeNetwork = instance.SmallCell.Pipe.PipeNetwork;
				return ((pipeNetwork != null) ? pipeNetwork.Atmosphere : null) != null;
			}
			return false;
		}

		[HarmonyPatch("GetPassiveTooltip", new Type[] { typeof(Collider) })]
		[HarmonyPrefix]
		public static bool GetPassiveTooltip_Patch(PipeAnalysizer __instance, ref PassiveTooltip __result, Collider hitCollider)
		{
			Tooltip.ToolTipStringBuilder.Clear();
			PassiveTooltip result = new PassiveTooltip(true);
			if (hitCollider == null || hitCollider.transform != __instance.ThingTransform)
			{
				__result = (__instance as Device).GetPassiveTooltip(hitCollider);
				return false; // skip original method
			}
			if (!__instance.OnOff || !__instance.Powered || __instance.Error == 1)
			{
				__result = (__instance as Device).GetPassiveTooltip(hitCollider);
				return false; // skip original method
			}
			if (!PipeAnalysizer_Patch.hasReadableAtmosphere(__instance))
			{
				__result = (__instance as Device).GetPassiveTooltip(hitCollider);
				return false; // skip original method
			}
			PipeNetwork pipeNetwork = __instance.SmallCell.Pipe.PipeNetwork;
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(pipeNetwork.Atmosphere).ModGasMixture;
			Tooltip.ToolTipStringBuilder.Append(string.Format("Pressure {0}\nTemperature {1}\n", pipeNetwork.Atmosphere.PressureGassesAndLiquidsInPa.ToStringPrefix("Pa", "yellow"), pipeNetwork.Atmosphere.Temperature.ToStringPrefix("K", "yellow")));
			foreach (ModMole modMole in gasMixture.ContainedGases.Values)
			{
				Tooltip.ToolTipStringBuilder.Append(ModAtmosphericsManager.DisplayGas(gasMixture, modMole, "F0"));
			}
			result.Title = (__instance as Thing).DisplayName;
			result.Description = Tooltip.ToolTipStringBuilder.ToString();
			__result = result;
			return false; // skip original method
		}
	}
}