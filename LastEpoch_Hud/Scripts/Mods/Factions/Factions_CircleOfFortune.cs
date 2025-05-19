using HarmonyLib;
using Il2CppLE.Factions;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastEpoch_Hud.Scripts.Mods.Factions
{
    /// <summary>
    /// Mod for the Circle of Fortune faction.
    /// </summary>
    [RegisterTypeInIl2Cpp]
    public class Factions_CircleOfFortune : Factions_Faction
    {
        #region Vars
        #region Constants
        private const FactionID factionId = FactionID.CircleOfFortune;
        #endregion

        private static System.ConsoleColor logColor;
        #endregion

        #region Properties
        public static Factions_CircleOfFortune Instance { get; private set; }
        #endregion

        #region .ctor
        public Factions_CircleOfFortune() { }
        public Factions_CircleOfFortune(IntPtr ptr) : base(ptr) { }
        #endregion

        #region Overrides
        private new void Awake()
        {
            SetFaction(factionId);
            if (!Save_Manager.instance.IsNullOrDestroyed()) { Rank = Save_Manager.instance.data.Factions.CircleOfFortune.Rank; }
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
        /// <summary>
        /// Adds the ability to purchase prophecies without spending favor.
        /// </summary>
        [HarmonyPatch(typeof(CircleOfFortune), "PurchaseProphecy", new System.Type[] { typeof(ProphecyRegion), typeof(byte) })]
        public class CircleOfFortune_PurchaseProphecy
        {
            static int prePurchaseFavor;

            [HarmonyPrefix]
            static void Prefix(CircleOfFortune __instance, ProphecyRegion __0, byte __1)
            {
                if (LastEpochMod.CanRun && Save_Manager.instance.data.Factions.CircleOfFortune.Enable_FreeProphecies)
                {
                    prePurchaseFavor = __instance.Favor;
                    Main.logger_instance?.Msg(logColor, "CircleOfFortune.PurchaseProphecy() - Stored Favor = " + prePurchaseFavor);
                }
            }

            [HarmonyPostfix]
            static void Postfix(ref CircleOfFortune __instance, ProphecyRegion __0, byte __1)
            {
                if (LastEpochMod.CanRun && Save_Manager.instance.data.Factions.CircleOfFortune.Enable_FreeProphecies)
                {
                    __instance.Favor = prePurchaseFavor;
                    Main.logger_instance?.Msg(logColor, "CircleOfFortune.PurchaseProphecy() - Set Favor = " + prePurchaseFavor);
                }
            }
        }
        #endregion
    }
}
