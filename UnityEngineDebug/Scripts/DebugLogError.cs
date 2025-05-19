using HarmonyLib;

namespace UnityEngineDebug.Scripts
{
    public class DebugLogError
    {
        [HarmonyPatch(typeof(UnityEngine.Debug), "LogError", new System.Type[] { typeof(Il2CppSystem.Object) })]
        public class Debug_LogError_Il2CppSystemObject
        {
            [HarmonyPostfix]
            static void Postfix(Il2CppSystem.Object __0) //__0 = message
            {
                if (Main.Enable_DebugLogError)
                {
                    string message = __0?.ToString() ?? "null";
                    Main.logger_instance?.Error(message);
                }
            }
        }

        [HarmonyPatch(typeof(UnityEngine.Debug), "LogError", new System.Type[] { typeof(Il2CppSystem.Object), typeof(UnityEngine.Object) })]
        public class Debug_LogError_Il2CppSystemObject_UnityEngineObject
        {
            [HarmonyPostfix]
            static void Postfix(Il2CppSystem.Object __0, UnityEngine.Object __1) //__1 = context
            {
                if (Main.Enable_DebugLogError)
                {
                    string message = __0?.ToString() ?? "null";
                    string context = __1?.ToString() ?? "null";
                    Main.logger_instance?.Error(context + " ->" + message);
                }
            }
        }
    }
}
