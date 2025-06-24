using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using UnityFrooxEngineRunner;

namespace LogiWheel;

public class LogiWheel : ResoniteMod
{
    public override string Name => "LogiWheel";
    public override string Author => "LeCloutPanda";
    public override string Version => "1.0.0";
    public override string Link => "https://github.com/LeCloutPanda/LogiWheel";

    public static ModConfiguration config;
    [AutoRegisterConfigKey] private static ModConfigurationKey<bool> ENABLED = new ModConfigurationKey<bool>("enabledToggle", "", () => true);
    [AutoRegisterConfigKey] private static ModConfigurationKey<bool> RESFRESH = new ModConfigurationKey<bool>("refresh", "", () => true);

    private static LogitechGSDK.LogiControllerPropertiesData controllerData = new LogitechGSDK.LogiControllerPropertiesData();

    public override void OnEngineInit()
    {
        config = GetConfiguration();
        config.Save(true);

        Harmony harmony = new Harmony("dev.lecloutpanda.logiwheel");
        harmony.PatchAll();

        config.OnThisConfigurationChanged += (key) =>
        {
            if (config.GetValue(RESFRESH) == true)
            {
                LogitechGSDK.LogiSteeringShutdown();
                bool initSuccess = LogitechGSDK.LogiSteeringInitialize(true);
                if (!initSuccess)
                {
                    Error("Failed initializing LogitechGSDK");
                    return;
                }
                else Msg("Initializing LogitechGSDK");
                LogitechGSDK.LogiGetCurrentControllerProperties(0, ref controllerData);
                config.Set(RESFRESH, false);
                config.Save();
            }
        };

        Engine.Current.OnReady += () =>
        {
            LogitechGSDK.LogiSteeringShutdown();
            bool initSuccess = LogitechGSDK.LogiSteeringInitialize(true);
            if (!initSuccess)
            {
                Error("Failed initializing LogitechGSDK");
                return;
            }
            else Msg("Initializing LogitechGSDK");

            LogitechGSDK.LogiGetCurrentControllerProperties(0, ref controllerData);
        };
    }

    [HarmonyPatch(typeof(FrooxEngineRunner), "OnAppWantsToQuit")]
    class ShutdownPatch {
        [HarmonyPrefix]
        private static void Prefix()
        {
            LogitechGSDK.LogiSteeringShutdown();
            Msg("Shutting Down LogitechGSDK");
        }
    }

    [HarmonyPatch(typeof(UpdateManager), nameof(UpdateManager.RunUpdates))]
    class WorldUpdatePatch
    {
        public static void Postfix(UpdateManager __instance)
        {
            if (__instance.World.IsUserspace()) return;
            if (!config.GetValue(ENABLED)) return;
            if (__instance.CurrentlyUpdatingUser != __instance.World.LocalUser) return;

            try
            {
                Slot userRoot = __instance.World.LocalUser.Root.Slot;
                Slot logiWheelSlot = userRoot.FindChildOrAdd("LogiWheel - Values", false);
                if (logiWheelSlot != null)
                {

                    bool hasUpdated = LogitechGSDK.LogiUpdate();
                    bool isConnected = LogitechGSDK.LogiIsConnected(0);

                    // Connected State
                    DynamicVariableWriteResult isConnectedResult = logiWheelSlot.WriteDynamicVariable("User/LogiWheel.isConnected", isConnected);
                    if (isConnectedResult == DynamicVariableWriteResult.NotFound) DynamicVariableHelper.CreateVariable(logiWheelSlot, "User/LogiWheel.isConnected", isConnected);

                    if (hasUpdated && isConnected)
                    {
                        LogitechGSDK.DIJOYSTATE2ENGINES state = LogitechGSDK.LogiGetStateCSharp(0);

                        float xAxis = MathX.Clamp((float)state.lX / 32767f, -1f, 1f);
                        DynamicVariableWriteResult xAxisResult = logiWheelSlot.WriteDynamicVariable("User/LogiWheel.xAxis", xAxis);
                        if (xAxisResult == DynamicVariableWriteResult.NotFound) DynamicVariableHelper.CreateVariable(logiWheelSlot, "User/LogiWheel.xAxis", xAxis);

                        float yAxis = MathX.Clamp((float)-state.lY / 32767f, 0, 1f);
                        DynamicVariableWriteResult yAxisResult = logiWheelSlot.WriteDynamicVariable("User/LogiWheel.acceleration", yAxis);
                        if (yAxisResult == DynamicVariableWriteResult.NotFound) DynamicVariableHelper.CreateVariable(logiWheelSlot, "User/LogiWheel.acceleration", yAxis);

                        float zRotation = MathX.Clamp((float)-state.lRz / 32767f, 0, 1f);
                        DynamicVariableWriteResult zRotationResult = logiWheelSlot.WriteDynamicVariable("User/LogiWheel.brake", zRotation);
                        if (zRotationResult == DynamicVariableWriteResult.NotFound) DynamicVariableHelper.CreateVariable(logiWheelSlot, "User/LogiWheel.brake", zRotation);

                        float slider0 = MathX.Clamp((float)-state.rglSlider[0] / 32767f, 0, 1f);
                        DynamicVariableWriteResult slider0Result = logiWheelSlot.WriteDynamicVariable("User/LogiWheel.clutch", slider0);
                        if (slider0Result == DynamicVariableWriteResult.NotFound) DynamicVariableHelper.CreateVariable(logiWheelSlot, "User/LogiWheel.clutch", slider0);

                        //int range = controllerData.wheelRange;  //MathX.Clamp((float)controllerData.wheelRange / 32767f, 0, 1f);
                        //DynamicVariableWriteResult rangeResult = logiWheelSlot.WriteDynamicVariable("User/LogiWheel.operatingRange", range);
                        //if (rangeResult == DynamicVariableWriteResult.NotFound) DynamicVariableHelper.CreateVariable(logiWheelSlot, "User/LogiWheel.operatingRange", range);
                    }
                }
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }
    }

}
