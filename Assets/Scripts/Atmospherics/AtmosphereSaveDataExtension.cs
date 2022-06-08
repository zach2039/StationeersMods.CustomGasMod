using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Atmospherics;
using UnityEngine;

namespace zach2039.CustomGas.Assets.Scripts.Atmospherics
{
    public static class AtmosphereSaveDataExtension
    {
        private static readonly ConditionalWeakTable<AtmosphereSaveData, AtmosphereSaveDataAdditionalData> data = new ConditionalWeakTable<AtmosphereSaveData, AtmosphereSaveDataAdditionalData>();

        public static AtmosphereSaveDataAdditionalData GetAdditionalData(this AtmosphereSaveData atmosphereSaveData)
        {
            return data.GetOrCreateValue(atmosphereSaveData);
        }

        public static void AddData(this AtmosphereSaveData atmosphereSaveData, AtmosphereSaveDataAdditionalData value)
        {
            try
            {
                data.Add(atmosphereSaveData, value);
            }
            catch (Exception e) 
            {
                Debug.LogError(e);
            }
        }
    }
}
