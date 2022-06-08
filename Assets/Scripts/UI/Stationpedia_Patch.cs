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

namespace Assets.Scripts.UI
{
	[HarmonyPatch(typeof(Stationpedia))]
	public class Stationpedia_Patch
	{
		// TODO: Patch PopulateGases()

		// TODO: Patch PopulateLists()
	}
}
