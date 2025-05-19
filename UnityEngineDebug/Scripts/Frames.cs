using HarmonyLib;
using MelonLoader;
using System;
using System.Linq;
using UnityEngine;

namespace UnityEngineDebug.Scripts
{
    [RegisterTypeInIl2Cpp]
    public class Frames : MonoBehaviour
    {
        public Frames(System.IntPtr ptr) : base(ptr) { }
        public static Frames? instance { get; private set; }

        public static Il2CppSystem.Collections.Generic.List<Il2CppSystem.Diagnostics.StackFrame> frames = new Il2CppSystem.Collections.Generic.List<Il2CppSystem.Diagnostics.StackFrame>();
        public static string[] unwanted_class = { "", "UnityEngine.Logger", "UnityEngine.Debug", "LE.Telemetry.ClientLogHandler" };
        

        void Awake()
        {
            Main.logger_instance?.Msg("Trace Awake");
            instance = this;
            Application.stackTraceLogType = StackTraceLogType.Full;
        }
        void Update()
        {

        }

        [HarmonyPatch(typeof(Application), "SetStackTraceLogType")]
        public class Application_SetStackTraceLogType
        {
            [HarmonyPostfix]
            static void Postfix(LogType __0, StackTraceLogType __1)
            {
                Main.logger_instance?.Msg("Application.SetStackTraceLogType() LogType : " + __0.ToString() + ", StackTraceType = " + __1.ToString());
            }
        }
        
        [HarmonyPatch(typeof(Il2CppSystem.Diagnostics.StackTrace), "GetFrame", new System.Type[] { typeof(int) })]
        public class Il2CppSystemDiagnosticsStackTrace_GetFrame_int
        {
            [HarmonyPostfix]
            static void Postfix(Il2CppSystem.Diagnostics.StackTrace __instance, Il2CppSystem.Diagnostics.StackFrame __result, int __0)
            {
                if (Main.Enable_Trace)
                {
                    Il2CppSystem.Reflection.MethodBase method_base = __result.methodBase;
                    if (!method_base.IsNullOrDestroyed())
                    {
                        Il2CppSystem.Reflection.RuntimeMethodInfo? method_info = method_base.TryCast<Il2CppSystem.Reflection.RuntimeMethodInfo>();
                        string? return_type = method_info?.ReturnType?.FullName ?? "null";
                        string? classe = method_info?.DeclaringType?.FullName ?? "null";
                        string? method_name = method_info?.Name ?? "null";
                        if (!unwanted_class.Contains(classe))
                        {
                            string result_str = "";
                            if (return_type != "null") { result_str += return_type + " "; }
                            if (classe != "null")
                            {
                                result_str += Functions.str_nullable(classe) + ".";
                                //if (classe.Contains("`1")) { result_str += classe.Split('`')[0] + "?."; }
                                //else { result_str += str_nullable(classe) + "."; }                                    
                            }
                            if (method_name != "null") { result_str += method_name + "();"; }

                            if (result_str != "") { Main.logger_instance?.Msg("Frame : " + result_str); }
                            else { Main.logger_instance?.Error("Error GetFrame"); }
                        }
                    }
                    if (!frames.Contains(__result)) { frames.Add(__result); }
                }
            }
        }
    }
}
