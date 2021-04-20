﻿using System.Collections;
using UnityEngine;
using UnityEditor;
using FreeRoaming.WildPokemonArea;

namespace Editor
{
    [CustomEditor(typeof(WildGrassGenerator))]
    [CanEditMultipleObjects]
    public class WildGrassGeneratorEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {

            DrawDefaultInspector();

            if (GUILayout.Button("Generate"))
            {

                ((WildGrassGenerator)target).Generate();

            }

        }

    }
}