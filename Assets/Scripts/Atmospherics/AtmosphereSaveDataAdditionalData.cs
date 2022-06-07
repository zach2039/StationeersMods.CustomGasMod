using System;
using System.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace zach2039.CustomGasMod.Assets.Scripts.Atmospherics
{
    [Serializable]
    public class AtmosphereSaveDataAdditionalData
    {
        public AtmosphereSaveDataAdditionalData()
        {
            GasSaveData = new Dictionary<int, float>();
        }

        [XmlArray(ElementName = "Gases")]
        [XmlArrayItem(ElementName = "Gas")]
        public List<KeyValueElement> SerializableGasSaveData
        {
            get { return GasSaveData.Select(x => new KeyValueElement { Key = x.Key, Value = x.Value }).ToList(); }
            set { GasSaveData = value.ToDictionary(x => x.Key, x => x.Value); }
        }

        [XmlIgnore]
        public Dictionary<int, float> GasSaveData { get; set; }
    }

    [Serializable]
    public class KeyValueElement
    {
        public int Key { get; set; }
        public float Value { get; set; }
    }
}
