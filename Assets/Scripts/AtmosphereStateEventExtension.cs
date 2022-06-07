using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

namespace zach2039.CustomGasMod.Assets.Scripts
{
    public static class AtmosphereStateEventExtension
    {
        private static readonly ConditionalWeakTable<AtmosphereStateEvent, AtmosphereStateEventAdditionalData> data = new ConditionalWeakTable<AtmosphereStateEvent, AtmosphereStateEventAdditionalData>();

        public static AtmosphereStateEventAdditionalData GetAdditionalData(this AtmosphereStateEvent atmosphereStateEvent)
        {
            return data.GetOrCreateValue(atmosphereStateEvent);
        }

        public static void AddData(this AtmosphereStateEvent atmosphereStateEvent, AtmosphereStateEventAdditionalData value)
        {
            try
            {
                data.Add(atmosphereStateEvent, value);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
