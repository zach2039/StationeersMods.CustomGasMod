using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Atmospherics;
using HarmonyLib;

namespace zach2039.CustomGasMod.Scripts.Atmospherics
{
    [HarmonyPatch(typeof(Chemistry))]
    public static class Chemistry_Patch
    {
    }
}
