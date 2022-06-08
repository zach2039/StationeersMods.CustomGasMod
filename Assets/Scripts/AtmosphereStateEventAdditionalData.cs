using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace zach2039.CustomGas.Assets.Scripts
{
    public class AtmosphereStateEventAdditionalData
    {
        public Dictionary<int, float> ModMinNumMoles;

        public Dictionary<int, float> ModMaxNumMoles;

        public AtmosphereStateEventAdditionalData()
        {
            ModMinNumMoles = new Dictionary<int, float>();
            ModMaxNumMoles = new Dictionary<int, float>();
        }
    }
}
