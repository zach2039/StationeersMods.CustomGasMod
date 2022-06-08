using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.GridSystem;
using HarmonyLib;
using UnityEngine;
using zach2039.CustomGas.Assets.Scripts.Atmospherics;

namespace zach2039.CustomGas.Assets.Scripts
{
    [HarmonyPatch(typeof(AtmosphereStateEvent))]
    public class AtmosphereStateEvent_Patch
    {
        [HarmonyPatch("IsValidMoleCounts", new Type[] { typeof(Atmosphere), typeof(Room) })]
        [HarmonyPrefix]
        public static bool IsValidMoleCounts(ref bool __result, AtmosphereStateEvent __instance, Atmosphere atmos, Room room)
        {
            ModMole[] array = ModGasMixture.ReadOnlyMoles(atmos.GetAdditionalData().ModGasMixture);
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Quantity > __instance.GetAdditionalData().ModMaxNumMoles[i] || array[i].Quantity < __instance.GetAdditionalData().ModMinNumMoles[i])
                {
                    __instance.roomValidAtmosCount[room] = 0;
                    __result = false;
                    return false; // skip original method
                }
            }
            __result = true;
            return false; // skip original method
        }
    }
}
