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
	[HarmonyPatch(typeof(H2CombustorMachine))]
	public class H2CombustorMachine_Patch
	{
		[HarmonyPatch(typeof(H2CombustorMachine), "OnAtmosphericTick")]
		[HarmonyPrefix]
		public static bool OnAtmosphericTick_Patch(H2CombustorMachine __instance)
		{
			if (!__instance.OnOff || !__instance.Powered || !__instance.IsOperable)
			{
				if (__instance.Activate == 1)
				{
					OnServer.Interact((__instance as Thing).InteractActivate, 0, false);
				}
				return false; // skip original method
			}
			float transferMoles = __instance.PressurePerTick * __instance.InputNetwork.Atmosphere.Volume / (__instance.InputNetwork.Atmosphere.Temperature * 8.3144f);
			ModGasMixture gasMixture = AtmosphereExtension.Remove(__instance.InputNetwork.Atmosphere, transferMoles);
			if (gasMixture.IsValid)
			{
				AtmosphereExtension.Add(__instance.InternalAtmosphere, gasMixture);
			}
			else if (__instance.Activate == 1)
			{
				OnServer.Interact((__instance as Thing).InteractActivate, 0, false);
			}
			__instance.InternalAtmosphere.Combust(Atmosphere.MatterState.Liquid);
			DeviceAtmospherics.MoveToEqualizeBidirectional(__instance.InternalAtmosphere, __instance.OutputNetwork.Atmosphere, __instance.PressurePerTick, Atmosphere.MatterState.Liquid);
			if (__instance.OutputNetwork2 != null)
			{
				(__instance as DeviceAtmospherics).MoveVolume(__instance.InternalAtmosphere, __instance.OutputNetwork2.Atmosphere, Atmosphere.MatterState.Gas);
			}
			if (__instance.Activate == 0)
			{
				OnServer.Interact((__instance as Thing).InteractActivate, 1, false);
			}
			return false; // skip original method
		}
	}
}