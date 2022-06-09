using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Util;
using Assets.Scripts.Objects;
using Assets.Scripts.UI;
using HarmonyLib;
using Assets.Scripts.Networking;
using UI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.UI;
using zach2039.CustomGas.Assets.Scripts;
using zach2039.CustomGas.Assets.Scripts.Atmospherics;

namespace Assets.Scripts.Objects
{
	[HarmonyPatch(typeof(DynamicComposter))]
	public class DynamicComposter_Patch
	{
		[HarmonyPatch("DoProcessing")]
		[HarmonyPrefix]
		public static bool DoProcessing_Patch(DynamicComposter __instance, ref float ____powerUsedDuringTick, ref float ____powerGenerated, ref int ___TotalFertilizerProcessedSeconds, ref int ___LastFertilizerProcessedSeconds)
		{
			int totalFertilizerProcessedSeconds = ___TotalFertilizerProcessedSeconds;
			float num = (float)(totalFertilizerProcessedSeconds - ___LastFertilizerProcessedSeconds) / (float)DynamicComposter.ProcessTimeSeconds;
			___LastFertilizerProcessedSeconds = totalFertilizerProcessedSeconds;
			if (__instance.GasCanister != null)
            {
				ModGasMixture cansiterGasMixture = AtmosphereExtension.GetAdditionalData(__instance.GasCanister.InternalAtmosphere).ModGasMixture;
				if (cansiterGasMixture.GetGasByName("water").Quantity > 0f)
				{
					ModGasMixture newGasMixture = new ModGasMixture();
					newGasMixture.GetGasByName("water").Quantity = num;
					ModAtmosphericEventInstance.CreateRemove(__instance.GasCanister.InternalAtmosphere, newGasMixture);
					____powerUsedDuringTick += DynamicComposter.MolesDrainedPerProcessedItem * num * __instance.PowerUseMult;
				}
			}
			Atmosphere atmosphereWorld = AtmosphericsController.World.GetAtmosphereWorld(__instance.CenterPosition);
			ModGasMixture gasMixture = ModGasMixture.Create();
			foreach (ModSpawnGas spawnGas in DynamicComposterExtension.GetAdditionalData(__instance).ExspelledGas)
			{
				gasMixture.Add(new ModMole(spawnGas.GasID, spawnGas.Quantity * num, 0f));
			}
			ModAtmosphericEventInstance.CreateAdd(atmosphereWorld, gasMixture);
			if (atmosphereWorld != null && atmosphereWorld.IsAboveArmstrong())
			{
				ModAtmosphericEventInstance.CreateAddEnergy(atmosphereWorld, DynamicComposter.HeatTransferJoulesPerProcessedItem * num, false);
				____powerUsedDuringTick += DynamicComposter.HeatTransferJoulesPerProcessedItem * num;
			}
			if (__instance.Connected)
			{
				____powerGenerated = ____powerUsedDuringTick;
			}
			return false; // skip original method
		}
	}
}