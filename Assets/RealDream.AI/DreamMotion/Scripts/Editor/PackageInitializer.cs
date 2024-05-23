using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace RealDream.AI
{
    public class PackageInitializer : MonoBehaviour
    {
        [InitializeOnLoadMethod]
        [MenuItem("Tools/RealDream/Init RealDream")]
        private static void Init()
        {
            RegisterPacks();
            DefineSymbols();
        }

        private static void RegisterPacks()
        {
            var tag = "https://github.com/realdream-ai/motion.common.git";
            var packPath = "Packages/manifest.json";
            var packs = System.IO.File.ReadAllText(packPath);
            if (!packs.Contains(tag))
            {
                UnityEngine.Debug.Log("RealDream RegisterPacks");
                AddRequest request = Client.Add(tag);
                while (!request.IsCompleted)
                {
                }
            }
        }

        private static void DefineSymbols()
        {
            string symbol = "REALDREAM_MOTION";
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            if (!defineSymbols.Contains(symbol))
            {
                defineSymbols += ";" + symbol;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defineSymbols);
                UnityEngine.Debug.Log("RealDream DefineSymbols");
            }
        }
    }
}