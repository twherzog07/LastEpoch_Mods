using HarmonyLib;

namespace UnityEngineDebug.Scripts
{
    public class DebugLogWarning
    {
        [HarmonyPatch(typeof(UnityEngine.Debug), "LogWarning", new System.Type[] { typeof(Il2CppSystem.Object) })]
        public class Debug_LogWarning_Il2CppSystemObject
        {
            [HarmonyPostfix]
            static void Postfix(Il2CppSystem.Object __0) //__0 = message
            {
                if (Main.Enable_DebugLogWarning)
                {
                    string message = __0?.ToString() ?? "null";
                    Main.logger_instance?.Warning(message);
                }
            }
        }

        [HarmonyPatch(typeof(UnityEngine.Debug), "LogWarning", new System.Type[] { typeof(Il2CppSystem.Object), typeof(UnityEngine.Object) })]
        public class Debug_LogWarning_Il2CppSystemObject_UnityEngineObject
        {
            [HarmonyPostfix]
            static void Postfix(Il2CppSystem.Object __0, UnityEngine.Object __1) //__1 = context
            {
                if (Main.Enable_DebugLogWarning)
                {
                    string message = __0?.ToString() ?? "null";
                    string context = __1?.ToString() ?? "null";
                    Main.logger_instance?.Warning(context + " ->" + message);
                }
            }
        }
    }
}
