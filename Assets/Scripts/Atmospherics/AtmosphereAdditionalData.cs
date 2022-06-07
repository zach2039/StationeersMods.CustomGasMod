using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace zach2039.CustomGasMod.Assets.Scripts.Atmospherics
{
    public class AtmosphereAdditionalData
    {
        public ModGasMixture ModGasMixture;

        public AtmosphereAdditionalData()
        {
            ModGasMixture = new ModGasMixture();
        }
    }
}
