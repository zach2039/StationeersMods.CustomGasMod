using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace zach2039
{
    /// <summary>
    /// Replaces blueprint material with one using the wireframe shader
    /// </summary>
    public class CustomWireframe : Assets.Scripts.UI.Wireframe
    {
        public void Awake()
        {
            Renderer meshRenderer = GetComponent<Renderer>();
            meshRenderer.material.shader = Shader.Find("Unlit/AlphaSelfIllum");
            meshRenderer.material.color = Color.green;
        }
    }
}
