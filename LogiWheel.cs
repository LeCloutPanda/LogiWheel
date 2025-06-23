using System;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;

namespace LogiWheel;

public class LogiWheel : ResoniteMod
{
    public override string Name => "LogiWheel";
    public override string Author => "LeCloutPanda";
    public override string Version => "1.0.0";
    public override string Link => "https://github.com/LeCloutPanda/LogiWheel";

    public static ModConfiguration config;
    [AutoRegisterConfigKey] private static ModConfigurationKey<bool> ENABLED = new ModConfigurationKey<bool>("enabledToggle", "", () => true);

    public override void OnEngineInit()
    {
        config = GetConfiguration();
        config.Save(true);

        Engine.Current.OnReady += () =>
        {
            bool initSuccess = LogitechGSDK.LogiSteeringInitialize(false);
            if (!initSuccess) Msg("Initializing LogitechGSDK");
            else Msg("Initializing LogitechGSDK");
        };

        Engine.Current.OnShutdown += () =>
        {
            LogitechGSDK.LogiSteeringShutdown();
            Msg("Shutting Down LogitechGSDK");
        };

        Harmony harmony = new Harmony("dev.lecloutpanda.logiwheel");
        harmony.PatchAll();
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

                    //DynamicVariableWriteResult hasUpdatedResult = logiWheelSlot.WriteDynamicVariable("User/LogiWheel.hasUpdated", hasUpdated);
                    //if (hasUpdatedResult == DynamicVariableWriteResult.NotFound) DynamicVariableHelper.CreateVariable(logiWheelSlot, "User/LogiWheel.hasUpdated", hasUpdated);

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
   
                        //LogitechGSDK.LogiGetCurrentControllerProperties(0, ref data);
                        //float range = MathX.Clamp((float)data.wheelRange / 32767f, 0, 1f);
                        //DynamicVariableWriteResult rangeResult = logiWheelSlot.WriteDynamicVariable("User/LogiWheel.operatingRange", (float)data.wheelRange);
                        //if (rangeResult == DynamicVariableWriteResult.NotFound) DynamicVariableHelper.CreateVariable(logiWheelSlot, "User/LogiWheel.operatingRange", (float)data.wheelRange, false);
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
