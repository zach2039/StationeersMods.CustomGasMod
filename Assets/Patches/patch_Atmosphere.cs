using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonoMod;

namespace Assets.Scripts.Atmospherics
{
    class patch_Atmosphere : Atmosphere
    {
		[MonoModReplace]
		public new float ParticalPressureO2
		{
			get
			{
				if (this.IsCachable && !Atmosphere.CanWriteAccess)
				{
					return this._particalPressureO2Cached;
				}
				return this.GetAdditionalData().ModGasMixture.GetGasByName("oxygen").Quantity * 8.3144f * this.GetAdditionalData().ModGasMixture.Temperature / this.Volume;
			}
		}
	}
}

