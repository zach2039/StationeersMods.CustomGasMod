using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace zach2039.CustomGasMod
{
    public static class ModChemistry
    {
        public static List<ModGasTemplate> GasTemplates = new List<ModGasTemplate>();

        public static string GetSymbol(int gasId)
        {
            ModGasTemplate gasTemplate = FindGasTemplateById(gasId);
            return gasTemplate.GasSymbol;
        }

        public static ModGasTemplate FindGasTemplateByName(string gasName)
        {
            foreach (ModGasTemplate modGasTemplate in ModChemistry.GasTemplates)
            {
                if (modGasTemplate.Name.ToLower() == gasName.ToLower())
                {
                    return modGasTemplate;
                }
            }
            throw new ArgumentException("Could not find gas template of name: " + gasName + ".");
        }

        public static ModGasTemplate FindGasTemplateById(int gasId)
        {
            foreach (ModGasTemplate modGasTemplate in ModChemistry.GasTemplates)
            {
                if (modGasTemplate.ID == gasId)
                {
                    return modGasTemplate;
                }
            }
            throw new ArgumentException("Could not find gas template of id: " + gasId + ".");
        }
    }
}
