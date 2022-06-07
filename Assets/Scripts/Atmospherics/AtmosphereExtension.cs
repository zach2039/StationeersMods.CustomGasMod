using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Atmospherics;
using UnityEngine;

namespace zach2039.CustomGasMod
{
    public static class AtmosphereExtension
    {
        private static readonly ConditionalWeakTable<Atmosphere, AtmosphereAdditionalData> data = new ConditionalWeakTable<Atmosphere, AtmosphereAdditionalData>();

        public static AtmosphereAdditionalData GetAdditionalData(this Atmosphere atmosphere)
        {
            return data.GetOrCreateValue(atmosphere);
        }

        public static void AddData(this Atmosphere atmosphere, AtmosphereAdditionalData value)
        {
            try
            {
                data.Add(atmosphere, value);
            }
            catch (Exception e) 
            {
                Debug.LogError(e);
            }
        }
    }
}
