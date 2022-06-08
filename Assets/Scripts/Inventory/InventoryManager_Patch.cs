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

namespace Assets.Scripts.Inventory
{
	[HarmonyPatch(typeof(InventoryManager))]
	public class InventoryManager_Patch
	{
		[HarmonyPatch("DebugContentsMethod")]
		[HarmonyPrefix]
		public static bool DebugContentsMethod_Patch(InventoryManager __instance, ref StringBuilder ____gasContents)
		{
			if (!__instance.DebugContents)
			{
				return false; // skip original method
			}
			____gasContents.Clear();
			if (InventoryManager.Parent.BreathingAtmosphere != null)
			{
				ModGasMixture gasMixture = InventoryManager.Parent.BreathingAtmosphere.GetAdditionalData().ModGasMixture;
				foreach (ModMole modMole in gasMixture.ContainedGases.Values)
				{
					____gasContents.Append(ModAtmosphericsManager.DisplayGas(gasMixture, modMole, "F2"));
				}
			}
			else
			{
				____gasContents.Append("None");
			}
			__instance.DebugContents.text = ____gasContents.ToString();
			return false; // skip original method
		}
	}
}