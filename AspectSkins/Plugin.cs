using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;

namespace AspectSkins {
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    
    public class AspectSkins : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "BALLS";
        public const string PluginName = "AspectSkins";
        public const string PluginVersion = "1.0.0";

        public static BepInEx.Logging.ManualLogSource ModLogger;

        public void Awake() {
            // set logger
            ModLogger = Logger;

            EliteSkins.Setup();
        }
    }
}