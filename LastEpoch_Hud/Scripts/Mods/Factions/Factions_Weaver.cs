using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Factions;
using MelonLoader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MelonLoader.MelonLogger;

namespace LastEpoch_Hud.Scripts.Mods.Factions
{
    /// <summary>
    /// Mod for the Weaver faction.
    /// </summary>
    [RegisterTypeInIl2Cpp]
    public class Factions_Weaver : Factions_Faction
    {
        #region Vars
        #region Constants
        private const FactionID factionId = FactionID.TheWeaver;
        private const int min_weaver_points = 0;
        private const int max_weaver_points = 255;
        #endregion

        private static System.ConsoleColor logColor;
        #endregion

        #region Properties
        public static Factions_Weaver Instance { get; private set; }
        #endregion

        #region .ctor
        public Factions_Weaver() : base() { }
        public Factions_Weaver(IntPtr ptr) : base(ptr) { }
        #endregion

        #region Overrides
        private new void Awake()
        {
            SetFaction(factionId);
            if (!Save_Manager.instance.IsNullOrDestroyed()) { Rank = Save_Manager.instance.data.Factions.Weaver.Rank; }
            base.Awake();
            Instance = this;
            logColor = getLogColorFromFactionColor(FactionID);
        }
        private new void Update()
        {
            base.Update();
        }
        #endregion

        #region Classes
        [HarmonyPatch(typeof(LocalTreeData.WeaverTreeData), "getUnspentPoints")]
        public class WeaverTreeData_getUnspentPoints
        {
            public static void Disable(ref LocalTreeData.WeaverTreeData treeData)
            {
                if (!string.IsNullOrWhiteSpace(Save_Manager.instance.data.Factions.Weaver.WeaverTree.Orig_WeaverTreeData))
                {
                    try
                    {
                        // Restore the original Weaver tree
                        LocalTreeData.WeaverTreeData origTree = JsonConvert.DeserializeObject<LocalTreeData.WeaverTreeData>(Save_Manager.instance.data.Factions.Weaver.WeaverTree.Orig_WeaverTreeData);
                        if (!origTree.IsNullOrDestroyed())
                        {
                            treeData = origTree;
                            Main.logger_instance?.Msg(logColor, "LocalTreeData.WeaverTreeData.getUnspentPoints() - Postfix - Restored original Weaver tree");
                        }
                    }
                    catch (Exception ex)
                    {
                        Main.logger_instance?.Error($"LocalTreeData.WeaverTreeData.getUnspentPoints() - Postfix - Failed to restore original Weaver tree from json {Save_Manager.instance.data.Factions.Weaver.WeaverTree.Orig_WeaverTreeData} with exception {ex}");
                    }
                    Save_Manager.instance.data.Factions.Weaver.WeaverTree.Orig_WeaverTreeData = "";
                }
            }

            [HarmonyPostfix]
            static void Postfix(ref LocalTreeData.WeaverTreeData __instance, ref ushort __result)
            {
                if (LastEpochMod.CanRun)
                {
                    if (Save_Manager.instance.data.Factions.Weaver.WeaverTree.Enable_EarnedWeaverTreePoints)
                    {
                        if (Save_Manager.instance.data.Factions.Weaver.WeaverTree.Set_EarnedWeaverTreePoints >= min_weaver_points && Save_Manager.instance.data.Factions.Weaver.WeaverTree.Set_EarnedWeaverTreePoints <= max_weaver_points &&
                            __instance.EarnedWeaverPoints != Save_Manager.instance.data.Factions.Weaver.WeaverTree.Set_EarnedWeaverTreePoints)
                            // Save the current Weaver tree
                            Save_Manager.instance.data.Factions.Weaver.WeaverTree.Orig_WeaverTreeData = JsonConvert.SerializeObject(__instance, Formatting.Indented);

                        if (Save_Manager.instance.data.Factions.Weaver.WeaverTree.Orig_EarnedWeaverTreePoints == -1)
                        {
                            Save_Manager.instance.data.Factions.Weaver.WeaverTree.Orig_EarnedWeaverTreePoints = __instance.EarnedWeaverPoints;
                        }
                        __instance.EarnedWeaverPoints = (ushort)Save_Manager.instance.data.Factions.Weaver.WeaverTree.Set_EarnedWeaverTreePoints;
                        Main.logger_instance?.Msg(logColor, "LocalTreeData.WeaverTreeData.getUnspentPoints() - Postfix - Set Earned Weaver Tree Points = " + __instance.EarnedWeaverPoints);
                    }
                    else
                    {
                        Disable(ref __instance);
                    }
                }
            }
        }
        #endregion
    }
}
