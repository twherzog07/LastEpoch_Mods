using HarmonyLib;

namespace UnityEngineDebug.Scripts
{
    public class DebugLogAssertion
    {
        [HarmonyPatch(typeof(UnityEngine.Debug), "LogAssertion", new System.Type[] { typeof(Il2CppSystem.Object) })]
        public class Debug_LogAssertion_Il2CppSystemException
        {
            [HarmonyPostfix]
            static void Postfix(Il2CppSystem.Object __0)
            {
                if (Main.Enable_DebugLogAssertion)
                {
                    string message = __0?.ToString() ?? "null";
                    Main.logger_instance?.Warning("Assertion : " + message);
                }
            }
        }
    }
}
