using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Util;
using Assets.Scripts.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace zach2039.CustomGas.Assets.Scripts.Atmospherics
{
    public class ModAtmosphericsManager : AtmosphericsManager
    {
		[NotNull]
		public static string DisplayGas(ModGasMixture gasMixture, ModMole mole, string formatMethod = "F0")
		{
			if (mole.Quantity <= 0f)
			{
				return "";
			}
			float num = mole.Quantity / gasMixture.TotalMolesGassesAndLiquids * 100f;
			return string.Concat(new string[]
			{
				"\n",
				mole.DisplayName,
				" ",
				num.ToString(formatMethod),
				"% (",
				mole.Quantity.ToStringPrefix("mol", "yellow"),
				")"
			});
		}
	}
}
