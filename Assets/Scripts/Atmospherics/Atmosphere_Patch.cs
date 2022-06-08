using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Networks;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Util;
using HarmonyLib;
using Assets.Scripts.Networking;
using UnityEngine;
using UnityEngine.Networking;
using zach2039.CustomGas.Assets.Scripts;
using zach2039.CustomGas.Assets.Scripts.Atmospherics;

namespace Assets.Scripts.Atmospherics
{
	[HarmonyPatch(typeof(Atmosphere))]
	public class Atmosphere_Patch
	{
		[HarmonyPatch("Atmosphere", MethodType.Constructor)]
		[HarmonyPostfix]
		public static void Atmosphere_Constructor0_Patch(Atmosphere __instance)
		{
			AtmosphereExtension.GetAdditionalData(__instance);
			Debug.Log("[" + CustomGas.MODID + "] Attached additional data to Atmophere instance + " + __instance + ".");
		}

		[HarmonyPatch("Atmosphere", MethodType.Constructor)]
		[HarmonyPatch(new Type[] { typeof(PipeNetwork), typeof(long) })]
		[HarmonyPostfix]
		public static void Atmosphere_Constructor1_Patch(Atmosphere __instance)
		{
			AtmosphereExtension.GetAdditionalData(__instance);
			Debug.Log("[" + CustomGas.MODID + "] Attached additional data to Atmophere instance + " + __instance + ".");
		}

		[HarmonyPatch("Atmosphere", MethodType.Constructor)]
		[HarmonyPatch(new Type[] { typeof(Thing), typeof(float), typeof(long) })]
		[HarmonyPostfix]
		public static void Atmosphere_Constructor2_Patch(Atmosphere __instance)
		{
			AtmosphereExtension.GetAdditionalData(__instance);
			Debug.Log("[" + CustomGas.MODID + "] Attached additional data to Atmophere instance + " + __instance + ".");
		}

		[HarmonyPatch("Atmosphere", MethodType.Constructor)]
		[HarmonyPatch(new Type[] { typeof(Grid3), typeof(GridController), typeof(long) })]
		[HarmonyPostfix]
		public static void Atmosphere_Constructor3_Patch(Atmosphere __instance)
		{
			AtmosphereExtension.GetAdditionalData(__instance);
			Debug.Log("[" + CustomGas.MODID + "] Attached additional data to Atmophere instance + " + __instance + ".");
		}

		[HarmonyPatch("Combust", new Type[] { typeof(Atmosphere.MatterState) })]
		[HarmonyPrefix]
		public static bool Combust_Patch(Atmosphere __instance, Atmosphere.MatterState productType, ref bool ____inflamed)
		{
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
			float totalFuel = gasMixture.TotalFuel;
			float quantity = gasMixture.GetGasByName("oxygen").Quantity;
			float num = Mathf.Min(totalFuel, quantity * 2f);
			float num2 = num / 2f;
			float num3 = 0.95f;
			float num4 = num * num3;
			float removedMoles = num2 * num3;
			ModMole mole = gasMixture.GetGasByName("hydrogen").Remove(num4); // FIXME: Need to remove fuel gases instead of just hydrogen.
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
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
			__result = Mathf.Clamp01(gasMixture.GetGasByName("oxygen").Quantity / gasMixture.TotalFuel * 2f);
			return false; // skip original method
		}

		[HarmonyPatch("FireLevel")]
		[HarmonyPrefix]
		public static bool FireLevel_Patch(Atmosphere __instance, ref float __result)
		{
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
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
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
		
			Debug.Log("[" + CustomGas.MODID + "] Atmophere instance " + __instance + ".");
			Debug.Log("[" + CustomGas.MODID + "] Gas mixture instance " + gasMixture + ".");
			Debug.Log("[" + CustomGas.MODID + "] Gas " + gasMixture.GetGasByName("water") + ".");
			gasMixture.GetGasByName("water").Clear();
			gasMixture.GetGasByName("water").ReadOnly = true;
		}

		[HarmonyPatch("IsCloseToGlobal", new Type[] { typeof(float) })]
		[HarmonyPrefix]
		public static bool IsCloseToGlobal_Patch(Atmosphere __instance, ref bool __result, float minPressure)
		{
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
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
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
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
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
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

		[HarmonyPatch("React")]
		[HarmonyPrefix]
		public static bool React_Patch(Atmosphere __instance, ref bool ____inflamed, ref bool ____sparked)
		{
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
			float temperature = gasMixture.Temperature;
			bool flag = __instance.Ignited || temperature > Atmosphere.FuelAutoignitionTemperature;
			__instance.BurnedPropaneRatio = 0f;
			__instance.CleanBurnRate = 0f;
			bool canExtinguish = true;
			foreach (ModMole modMole in gasMixture.ContainedGases.Values)
            {
				if (modMole.IsFuel)
                {
					float num = modMole.LowerFlammableLimit;
					float num2 = modMole.UpperFlammableLimit;
					float num3 = modMole.LimitingOxygenConcentration;
					if (!flag)
					{
						num += Atmosphere.LimitOffsetNotOnFire;
						num2 -= Atmosphere.LimitOffsetNotOnFire;
						num3 += Atmosphere.LimitOffsetNotOnFire;
					}
					____sparked = false;
					float num4 = modMole.Quantity / gasMixture.TotalMolesGassesAndLiquids;
					float num5 = gasMixture.GetGasByName("oxygen").Quantity / gasMixture.TotalMolesGassesAndLiquids;
					if (!(temperature < Atmosphere.PropaneMinimumBurnTemperature || !flag || num4 < num || num4 > num2 || num5 < num3 || __instance.PressureGassesAndLiquids < Atmosphere.MinimumCombustionPressure))
					{
						// some type of fuel is burning, so cannot stop burning
						canExtinguish = false;
					}
				}
            }
			if (canExtinguish)
            {
				____inflamed = false;
				return false; // skip original method
			}
			__instance.Combust(Atmosphere.MatterState.Gas);
			return false; // skip original method
		}

		[HarmonyPatch("Read", new Type[] { typeof(RocketBinaryReader) })]
		[HarmonyPrefix]
		public static bool Read_Patch(Atmosphere __instance, RocketBinaryReader reader, ref float ____temperatureCachedClient)
		{
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
			if (__instance.IsNetworkUpdateRequired(8U))
			{
				__instance.Direction = reader.ReadVector3Half();
			}
			if (__instance.IsNetworkUpdateRequired(16U))
			{
				byte b = reader.ReadByte();
				__instance.CleanBurnRate = (float)b / 255f;
				byte b2 = reader.ReadByte();
				__instance.BurnedPropaneRatio = (float)b2 / 255f;
				__instance.Inflamed = reader.ReadBoolean();
			}
			if (__instance.IsNetworkUpdateRequired(32U))
			{
				__instance.Volume = (float)reader.ReadUInt16();
			}
			if (__instance.IsNetworkUpdateRequired(64U))
			{
				gasMixture.Read(reader);
			}
			if (__instance.IsNetworkUpdateRequired(4U))
			{
				____temperatureCachedClient = reader.ReadSingle();
				gasMixture.TotalEnergy = ____temperatureCachedClient * gasMixture.HeatCapacity;
			}
			__instance.SetFlame(__instance.BurnedPropaneRatio, __instance.CleanBurnRate);
			__instance.LastNetworkUpdateTime = DateTime.Now;
			if (__instance.Mode == Atmosphere.AtmosphereMode.Thing && __instance.Thing != null)
			{
				__instance.Thing.OnAtmosphereClient();
			}
			return false; // skip original method
		}

		[HarmonyPatch("Write", new Type[] { typeof(RocketBinaryWriter), typeof(byte) })]
		[HarmonyPrefix]
		public static bool Write_Patch(Atmosphere __instance, ref float ____temperatureCachedClient, RocketBinaryWriter writer, byte quantitiesDirtiedFlag = 0)
		{
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
			writer.WriteInt64(__instance.ReferenceId);
			writer.WriteByte((byte)__instance.NetworkUpdateFlags);
			writer.WriteByte((byte)__instance.Mode);
			if (__instance.IsNetworkUpdateRequired(1U))
			{
				Atmosphere.AtmosphereMode mode = __instance.Mode;
				if (mode != Atmosphere.AtmosphereMode.Network)
				{
					if (mode != Atmosphere.AtmosphereMode.Thing)
					{
						writer.WriteInt64(0L);
					}
					else
					{
						writer.WriteInt64(__instance.Thing.ReferenceId);
					}
				}
				else
				{
					writer.WriteInt64(__instance.PipeNetwork.ReferenceId);
				}
			}
			if (__instance.IsNetworkUpdateRequired(2U))
			{
				writer.WriteGrid3(__instance.Grid);
			}
			if (__instance.IsNetworkUpdateRequired(8U))
			{
				writer.WriteVector3Half(__instance.Direction);
			}
			if (__instance.IsNetworkUpdateRequired(16U))
			{
				writer.WriteByte((byte)Mathf.RoundToInt(__instance.CleanBurnRate * 255f));
				writer.WriteByte((byte)Mathf.RoundToInt(__instance.BurnedPropaneRatio * 255f));
				writer.WriteBoolean(__instance.Inflamed);
			}
			if (__instance.IsNetworkUpdateRequired(32U))
			{
				writer.WriteUInt16((ushort)__instance.Volume);
			}
			if (__instance.IsNetworkUpdateRequired(64U))
			{
				gasMixture.Write(writer);
			}
			if (__instance.IsNetworkUpdateRequired(4U))
			{
				writer.WriteSingle(__instance.Temperature);
			}
			__instance.LastNetworkUpdateTime = DateTime.Now;
			return false; // skip original method
		}

		[HarmonyPatch("ParticalPressureO2", MethodType.Getter)]
		public bool ParticalPressureO2_Patch(Atmosphere __instance, ref float __result, ref float ____particalPressureO2Cached)
		{
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
			if (__instance.IsCachable && !Atmosphere.CanWriteAccess)
            {
				__result = ____particalPressureO2Cached;
				return false; // skip original method
			}
			__result = gasMixture.GetGasByName("oxygen").Quantity * 8.3144f * gasMixture.Temperature / __instance.Volume;
			return false; // skip original method
		}

		[HarmonyPatch("ParticalPressureNO2", MethodType.Getter)]
		public bool ParticalPressureNO2_Patch(Atmosphere __instance, ref float __result, ref float ____particalPressureNO2Cached)
		{
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
			if (__instance.IsCachable && !Atmosphere.CanWriteAccess)
			{
				__result = ____particalPressureNO2Cached;
				return false; // skip original method
			}
			__result = gasMixture.GetGasByName("nitrousoxide").Quantity * 8.3144f * gasMixture.Temperature / __instance.Volume;
			return false; // skip original method
		}

		[HarmonyPatch("ParticalPressureVolatiles", MethodType.Getter)]
		public bool ParticalPressureVolatiles_Patch(Atmosphere __instance, ref float __result, ref float ____particalPressureVolatilesCached)
		{
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
			if (__instance.IsCachable && !Atmosphere.CanWriteAccess)
			{
				__result = ____particalPressureVolatilesCached;
				return false; // skip original method
			}
			float fuelAmount = 0f;
			foreach (ModMole modMole in gasMixture.ContainedGases.Values)
            {
				if (modMole.IsFuel)
                {
					fuelAmount += modMole.Quantity;
				}
            }
			__result = fuelAmount * 8.3144f * gasMixture.Temperature / __instance.Volume;
			return false; // skip original method
		}

		[HarmonyPatch("ParticalPressurePollutants", MethodType.Getter)]
		public bool ParticalPressurePollutants_Patch(Atmosphere __instance, ref float __result, ref float ____particalPressurePollutantsCached)
		{
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
			if (__instance.IsCachable && !Atmosphere.CanWriteAccess)
			{
				__result = ____particalPressurePollutantsCached;
				return false; // skip original method
			}
			float pollutantAmount = 0f;
			foreach (ModMole modMole in gasMixture.ContainedGases.Values)
			{
				if (modMole.IsPollutant)
				{
					pollutantAmount += modMole.Quantity;
				}
			}
			__result = pollutantAmount * 8.3144f * gasMixture.Temperature / __instance.Volume;
			return false; // skip original method
		}

		[HarmonyPatch("ParticalPressureToxins", MethodType.Getter)]
		public bool ParticalPressureToxins_Patch(Atmosphere __instance, ref float __result, ref float ____particalPressureToxinsCached)
		{
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
			if (__instance.IsCachable && !Atmosphere.CanWriteAccess)
			{
				__result = ____particalPressureToxinsCached;
				return false; // skip original method
			}
			float toxinAmount = 0f;
			foreach (ModMole modMole in gasMixture.ContainedGases.Values)
			{
				if (modMole.IsToxic)
				{
					toxinAmount += modMole.Quantity;
				}
			}
			__result = toxinAmount * 8.3144f * gasMixture.Temperature / __instance.Volume;
			return false; // skip original method
		}

		[HarmonyPatch("PressureGasses", MethodType.Getter)]
		public bool PressureGasses_Patch(Atmosphere __instance, ref float __result, ref float ____pressureGassesCached)
		{
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
			if (__instance.IsCachable && !Atmosphere.CanWriteAccess)
			{
				__result = ____pressureGassesCached;
				return false; // skip original method
			}
			__result = gasMixture.TotalMolesGasses * 8.3144f * gasMixture.Temperature / __instance.Volume;
			return false; // skip original method
		}

		[HarmonyPatch("PressureGassesAndLiquids", MethodType.Getter)]
		public bool PressureGassesAndLiquids_Patch(Atmosphere __instance, ref float __result, ref float ____pressureGassesAndLiquidsCached)
		{
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
			if (__instance.IsCachable && !Atmosphere.CanWriteAccess)
			{
				__result = ____pressureGassesAndLiquidsCached;
				return false; // skip original method
			}
			__result = gasMixture.TotalMolesGassesAndLiquids * 8.3144f * gasMixture.Temperature / __instance.Volume;
			return false; // skip original method
		}

		[HarmonyPatch("PressureLiquids", MethodType.Getter)]
		public bool PressureLiquids_Patch(Atmosphere __instance, ref float __result, ref float ____pressureLiquidsCached)
		{
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
			if (__instance.IsCachable && !Atmosphere.CanWriteAccess)
			{
				__result = ____pressureLiquidsCached;
				return false; // skip original method
			}
			__result = gasMixture.TotalMolesLiquids * 8.3144f * gasMixture.Temperature / __instance.Volume;
			return false; // skip original method
		}

		[HarmonyPatch("TotalMoles", MethodType.Getter)]
		public bool TotalMoles_Patch(Atmosphere __instance, ref float __result, ref float ____totalMolesCached)
		{
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
			if (__instance.IsCachable && !Atmosphere.CanWriteAccess)
			{
				__result = ____totalMolesCached;
				return false; // skip original method
			}
			__result = gasMixture.TotalMolesGassesAndLiquids;
			return false; // skip original method
		}

		[HarmonyPatch("WaterHeight", MethodType.Getter)]
		public bool WaterHeight_Patch(Atmosphere __instance, ref float __result)
		{
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
			__result = gasMixture.GetGasByName("water").Quantity * Chemistry.WaterMolarVolume / __instance.Volume;
			return false; // skip original method
		}

		[HarmonyPatch("WaterRatio", MethodType.Getter)]
		public bool WaterRatio_Patch(Atmosphere __instance, ref float __result)
		{
			ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(__instance).ModGasMixture;
			__result = gasMixture.GetGasByName("water").Quantity / gasMixture.TotalMolesGassesAndLiquids;
			return false; // skip original method
		}
	}
}

