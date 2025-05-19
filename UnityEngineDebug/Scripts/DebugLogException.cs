using HarmonyLib;

namespace UnityEngineDebug.Scripts
{
    public class DebugLogException
    {
        [HarmonyPatch(typeof(UnityEngine.Debug), "LogException", new System.Type[] { typeof(Il2CppSystem.Exception) })]
        public class Debug_LogException_Il2CppSystemException
        {
            [HarmonyPostfix]
            static void Postfix(Il2CppSystem.Exception __0)
            {
                if (Main.Enable_DebugLogException)
                {
                    string exception = __0?.ToString() ?? "null";
                    Main.logger_instance?.BigError(exception);
                }
            }
        }

        [HarmonyPatch(typeof(UnityEngine.Debug), "LogException", new System.Type[] { typeof(Il2CppSystem.Exception), typeof(UnityEngine.Object) })]
        public class Debug_LogException_Il2CppSystemException_UnityEngineObject
        {
            [HarmonyPostfix]
            static void Postfix(Il2CppSystem.Exception __0, UnityEngine.Object __1)
            {
                if (Main.Enable_DebugLogException)
                {
                    string exception = __0?.ToString() ?? "null";
                    string context = __1?.ToString() ?? "null";
                    Main.logger_instance?.BigError(context + " ->" + exception);
                }
            }
        }
    }
}
