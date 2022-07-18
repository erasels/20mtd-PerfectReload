using BepInEx;
using BepInEx.Configuration;
using flanne;
using flanne.Core;
using flanne.Player;
using HarmonyLib;
using PerfectReload.listeners;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace PerfectReload {
    public class PRConstants {
        internal static BepInEx.Logging.ManualLogSource Logger;
        internal static Plugin plugin;
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin {
        public static ConfigEntry<string> altReloadKey;

        private void Awake() {
            PRConstants.Logger = Logger;
            PRConstants.plugin = this;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            altReloadKey = Config.Bind("Keybind", "altReloadKey", "f", "Define the additional key that you can use for reloading. Only for keyboard.");

            new PRMechanic();
            Harmony.CreateAndPatchAll(typeof(PRMechanic), null);
        }
    }
}
