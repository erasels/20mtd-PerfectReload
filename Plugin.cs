﻿using BepInEx;
using flanne;
using flanne.Core;
using flanne.Player;
using HarmonyLib;
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
        private static GameController gameController = null;
        private static UnityEngine.Color orgCol = UnityEngine.Color.white;
        private static Dictionary<UnityEngine.GameObject, UnityEngine.SpriteRenderer> saveBar = new Dictionary<UnityEngine.GameObject, UnityEngine.SpriteRenderer>();
        private static bool failedReload = false;
        private static bool jankLoaded = false;

        public static float SweetSpot = 0.65f;
        public static int ChainSuccess;

        public UnityEvent OnPerfectReload;

        private void Awake() {
            PRConstants.Logger = Logger;
            PRConstants.plugin = this;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Harmony.CreateAndPatchAll(typeof(Plugin), null);
            OnPerfectReload = new UnityEvent();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameController), "Start")]
        private static void GameControllerStartPostPatch(GameController __instance) {
            gameController = __instance;
            ChainSuccess = 0;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerController), "Start")]
        private static void PlayerControllerStartPostPatch(PlayerController __instance) {
            __instance.gameObject.AddComponent<PRBonus>();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerController), "Update")]
        private static void PlayerControllerUpdatePostPatch(PlayerController __instance) {
            if (gameController.CurrentState is CombatState) {
                var keyboard = Keyboard.current;
                if (keyboard == null)
                    return; // No keyboard connected.

                if (__instance.CurrentState is ReloadState) {
                    if (!saveBar.ContainsKey(__instance.reloadBar.gameObject)) {
                        saveBar.Clear();
                        saveBar.Add(__instance.reloadBar.gameObject, __instance.reloadBar.gameObject.GetComponentInChildren<UnityEngine.SpriteRenderer>());
                    }

                    bool inRange = __instance.reloadBar.value >= (SweetSpot - 0.1f) && __instance.reloadBar.value <= (SweetSpot + 0.1f);
                    bool inSweetSpot = __instance.reloadBar.value >= (SweetSpot - (0.025f * Math.Min(ChainSuccess, 4))) &&
                        __instance.reloadBar.value <= (SweetSpot + (0.025f * Math.Min(ChainSuccess, 4)));

                    //Color the reloadbar slider, skip rest of method when failed
                    if (!failedReload) {
                        if (inSweetSpot) {
                            saveBar[__instance.reloadBar.gameObject].color = UnityEngine.Color.blue;
                        } else if (inRange) {
                            saveBar[__instance.reloadBar.gameObject].color = UnityEngine.Color.green;
                        } else {
                            saveBar[__instance.reloadBar.gameObject].color = orgCol;
                        }
                    } else {
                        saveBar[__instance.reloadBar.gameObject].color = UnityEngine.Color.red;
                        ChainSuccess = 0;
                        return;
                    }

                    if (keyboard.rKey.wasPressedThisFrame) {
                        PRConstants.Logger.LogDebug($"Pressed at {__instance.reloadBar.value}");
                        if (inRange) {
                            doJankReload(__instance);
                            if (inSweetSpot) {
                                PRConstants.Logger.LogDebug("Sweet spot hit!");
                                PRConstants.plugin.OnPerfectReload.Invoke();
                                createTextPopup(__instance.reloadBar.transform.position, $"Sweet! (x{ChainSuccess})", Color.cyan);
                            } else {
                                createTextPopup(__instance.reloadBar.transform.position, "x" + ChainSuccess, Color.green);
                            }
                        } else {
                            ((PlayerState)__instance.CurrentState).owner.reloadStartSFX.Play(null);
                            failedReload = true;
                            createTextPopup(__instance.reloadBar.transform.position, "Failed!", Color.red);
                        }
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ReloadState), "Exit")]
        private static void CatchReloadExit(ReloadState __instance) {
            //Catches state change (StateMachine.Transition), if jankLoaded is false, then that means the reload was finished normally or interrupted by shooting
            //Shouldn't be an issue with ammo resotring effects because all of this happens in order
            if (!jankLoaded && __instance.ammo.fullOnAmmo)
                ChainSuccess = 0;
            failedReload = false;
        }

        private static void doJankReload(PlayerController pc) {
            ChainSuccess++;
            PRConstants.Logger.LogDebug($"Success, chain counter at: {ChainSuccess}");
            pc.CurrentState.StopCoroutine(((ReloadState)pc.CurrentState).reloadCoroutine);
            ((ReloadState)pc.CurrentState).reloadCoroutine = null;

            pc.ammo.Reload();
            ((PlayerState)pc.CurrentState).owner.reloadEndSFX.Play(null);

            jankLoaded = true;
            if (pc.playerInput.actions["Fire"].ReadValue<float>() != 0f) {
                ((PlayerState)pc.CurrentState).owner.ChangeState<ShootingState>();
            } else {
                ((PlayerState)pc.CurrentState).owner.ChangeState<IdleState>();
            }
            jankLoaded = false;

            pc.reloadBar.transform.parent.gameObject.SetActive(false);
        }

        private static void createTextPopup(Vector3 pos, String text, Color col) {
            GameObject pooledObject = ObjectPooler.SharedInstance.GetPooledObject("DamagePopup");
            pooledObject.transform.position = pos;
            pooledObject.SetActive(true);
            pooledObject.GetComponent<TextMeshPro>().color = col;
            pooledObject.GetComponent<TextMeshPro>().text = text;
        }
    }
}