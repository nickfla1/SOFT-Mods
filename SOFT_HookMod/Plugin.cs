using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SOFT_HookMod
{
    [BepInPlugin(About.PLUGIN_GUID, About.PLUGIN_NAME, About.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        public static ManualLogSource Logger;

        internal static readonly string Folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public Plugin()
        {
            Logger = Log;
        }

        public override void Load()
        {
            try
            {
                RegisterIL2CPP();
                Log.LogInfo("Register IL2CPP type 'HookedBehaviour'");
            }
            catch
            {
                Log.LogError("Failed to register IL2CPP type 'HookedBehaviour'");
            }

            try
            {
                Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
                Log.LogInfo("Succesfully registered patches");
            }
            catch
            {
                Log.LogError("Failed to register patches");
            }

            Log.LogInfo($"Plugin {About.PLUGIN_GUID} loaded");
        }

        public static void RegisterIL2CPP()
        {
            ClassInjector.RegisterTypeInIl2Cpp<HookedBehaviour>();

            GameObject hookedGameObject = new GameObject("HookedGameObject");
            hookedGameObject.AddComponent<HookedBehaviour>();
            hookedGameObject.hideFlags = HideFlags.HideAndDontSave;

            UnityEngine.Object.DontDestroyOnLoad(hookedGameObject);
        }
    }
}
