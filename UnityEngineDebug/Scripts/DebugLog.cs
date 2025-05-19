using HarmonyLib;

namespace UnityEngineDebug.Scripts
{
    public class DebugLog
    {
        [HarmonyPatch(typeof(UnityEngine.Debug), "Log", new System.Type[] { typeof(Il2CppSystem.Object) })]
        public class Debug_Log_Il2CppSystemObject
        {
            [HarmonyPostfix]
            static void Postfix(Il2CppSystem.Object __0)
            {
                if (Main.Enable_DebugLog)
                {
                    string message = __0?.ToString() ?? "null";
                    Main.logger_instance?.Msg(message);
                }
            }
        }

        [HarmonyPatch(typeof(UnityEngine.Debug), "Log", new System.Type[] { typeof(Il2CppSystem.Object), typeof(UnityEngine.Object) })]
        public class Debug_Log_Il2CppSystemObject_UnityEngineObject
        {
            [HarmonyPostfix]
            static void Postfix(Il2CppSystem.Object __0, UnityEngine.Object __1)
            {
                if (Main.Enable_DebugLog)
                {
                    string message = __0?.ToString() ?? "null";
                    string context = __1?.ToString() ?? "null";
                    Main.logger_instance?.Msg(context + " ->" + message);
                }
            }
        }
    }
}
