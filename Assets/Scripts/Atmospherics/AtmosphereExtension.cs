using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Atmospherics;
using UnityEngine;

namespace zach2039.CustomGas.Assets.Scripts.Atmospherics
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

        public static void Add(this Atmosphere atmosphere, ModGasMixture gasMixtureToAdd)
        {
            ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(atmosphere).ModGasMixture;
            gasMixture.Add(gasMixtureToAdd, Atmosphere.MatterState.All);
        }

        public static ModGasMixture Remove(this Atmosphere atmosphere, float transferMoles, Atmosphere.MatterState matterStateToRemove)
        {
            ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(atmosphere).ModGasMixture;
            return gasMixture.Remove(transferMoles, matterStateToRemove);
        }

        public static ModGasMixture Remove(this Atmosphere atmosphere, float transferMoles)
        {
            ModGasMixture gasMixture = AtmosphereExtension.GetAdditionalData(atmosphere).ModGasMixture;
            return gasMixture.Remove(transferMoles, Atmosphere.MatterState.All);
        }
    }
}
