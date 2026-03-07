#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// https://discussions.unity.com/t/materials-shader-recompiled-event/796279/2
namespace Arielado.EditorTools {
    public class AssetChangeDispatcher : AssetPostprocessor {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths) {
                ShaderChangedDispatcher.OnPostprocessImportedAssets(importedAssets);
        }
    }

    public static class ShaderChangedDispatcher {
        public static event Action<Shader> OnShaderChanged;
        public static event Action<ComputeShader> OnComputeShaderChanged;

        public static void OnPostprocessImportedAssets(string[] importedAssets) {
            CallOnAssetChanged(importedAssets, ".shader", OnShaderChanged);
            CallOnAssetChanged(importedAssets, ".compute", OnComputeShaderChanged);
        }

        private static void CallOnAssetChanged<T>(IEnumerable<string> importedAssets, string extension, Action<T> callEvent) where T : UnityEngine.Object {
            if (callEvent == null) return;
         
            var assetPaths = importedAssets.Where(path => Path.GetExtension(path) == extension);
            var assets = assetPaths.Select(AssetDatabase.LoadAssetAtPath<T>);
         
            foreach (var asset in assets)
                callEvent(asset);
        }
    }
}

#endif 
