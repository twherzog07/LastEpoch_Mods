using MelonLoader;
using UnityEngine;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: MelonInfo(typeof(UnityEngineDebug.Main), UnityEngineDebug.Main.mod_name, UnityEngineDebug.Main.mod_version, "Ash")]
[assembly: MelonGame(UnityEngineDebug.Main.company_name, UnityEngineDebug.Main.game_name)]
[assembly: VerifyLoaderVersion(0, 6, 0, true)]
[assembly: AssemblyTitle(UnityEngineDebug.Main.mod_name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct(UnityEngineDebug.Main.mod_name)]
[assembly: AssemblyCopyright("Copyright ©  2025")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("0.0.0.1")]
[assembly: AssemblyFileVersion("0.0.0.1")]

[assembly: AssemblyInformationalVersion("0.0.0.1")]

namespace UnityEngineDebug
{
    public class Main : MelonMod
    {
        public const string company_name = "Eleventh Hour Games";
        public const string game_name = "Last Epoch";
        public const string mod_name = "UnityEngineDebug";
        public const string mod_version = "0.0.1";
        public static MelonLogger.Instance? logger_instance = null;
        public static bool initialized = false;

        //------------------------- Config -----------------------//
        public static bool Enable_Trace = true;
        public static bool Enable_DebugLog = true;
        public static bool Enable_DebugLogWarning = true;
        public static bool Enable_DebugLogError = true;
        public static bool Enable_DebugLogException = true;
        public static bool Enable_DebugLogAssertion = true;
        //--------------------------------------------------------//

        public override void OnInitializeMelon()
        {
            logger_instance = LoggerInstance;
            Main.logger_instance.Msg("OnInitializeMelon");            
            initialized = false;
        }
        public override void OnLateUpdate()
        {
            if (!initialized)
            {
                logger_instance?.Msg("Initialize");
                GameObject obj = Object.Instantiate(new GameObject { name = "Unity_Engine_Debug" }, Vector3.zero, Quaternion.identity);
                Object.DontDestroyOnLoad(obj);
                obj.AddComponent<Scripts.Frames>();
                obj.active = true;

                initialized = true;
            }
        }
    }
    public static class Functions
    {
        public static bool IsNullOrDestroyed(this object? obj)
        {
            try
            {
                if (obj == null) { return true; }
                else if (obj is UnityEngine.Object unityObj && !unityObj) { return true; }
                return false;
            }
            catch { return true; }
        }
        public static string str_nullable(string str)
        {
            string result = str;
            if (str.Contains("`1")) { result = str.Split('`')[0] + "?"; }

            return result;
        }
    }
}
