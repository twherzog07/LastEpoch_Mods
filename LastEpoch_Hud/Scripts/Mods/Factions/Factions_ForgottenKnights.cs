using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Data;
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
    /// Mod for the Forgotten Knights faction.
    /// </summary>
    [RegisterTypeInIl2Cpp]
    public class Factions_ForgottenKnights : Factions_Faction
    {
        #region Vars
        #region Constants
        private const FactionID factionId = FactionID.ForgottenKnights;
        private const float default_float_val = -1;
        private const int default_int_val = -1;
        private const float min_float_value = 0;
        private const float max_float_value = 100;
        private const int min_int_value = 0;
        private const int max_int_value = 255;
        #endregion

        private static float orig_AdditionalChanceForHarbingersToDropGlyphOfEnvy = default_float_val;
        private static float orig_AdditionalChanceForSwappableNemesisUnique = default_float_val;
        private static int orig_BonusCorruptionPerGazeOfOrobyss = default_int_val;
        private static int orig_BonusGazeOfOrobyssOnHarbingerKill = default_int_val;
        private static int orig_BonusMaxForgingPotentialWhenEmpoweringNemesis = default_int_val;
        private static int orig_defeatedAterroths = default_int_val;
        private static float orig_DoubleStabilityGainChance = default_float_val;
        private static float orig_IncreasedChanceToDropHarbingersNeedles = default_float_val;
        private static float orig_IncreasedChanceToDropHarbingerSpecificLoot = default_float_val;
        private static System.ConsoleColor logColor;
        #endregion

        #region Properties
        public static Factions_ForgottenKnights Instance { get; private set; }
        #endregion

        #region .ctor
        public Factions_ForgottenKnights() { }
        public Factions_ForgottenKnights(IntPtr ptr) : base(ptr) { }
        #endregion        

        #region Overrides
        private new void Awake()
        {
            SetFaction(factionId);
            if (!Save_Manager.instance.IsNullOrDestroyed()) { Rank = Save_Manager.instance.data.Factions.ForgottenKnights.Rank; }
            base.Awake();
            Instance = this;
            logColor = getLogColorFromFactionColor(FactionID);
        }
        private new void Update()
        {
            base.Update();

            if (CanRun)
            {
                // Additional Chance For Harbingers To Drop Glyph Of Envy
                if (Save_Manager.instance.data.Factions.ForgottenKnights.Enable_SetAddtlGlyphOfEnvyDropRate)
                {
                    SetAdditionalChanceForHarbingersToDropGlyphOfEnvy(Save_Manager.instance.data.Factions.ForgottenKnights.Set_AddtlGlyphOfEnvyDropRate);
                }
                else if (orig_AdditionalChanceForHarbingersToDropGlyphOfEnvy != default_float_val)
                {
                    ResetAdditionalChanceForHarbingersToDropGlyphOfEnvy();
                }
                // Additional Chance For Nemesis Swap
                if (Save_Manager.instance.data.Factions.ForgottenKnights.Enable_SetAddtlChanceForNemesisSwap)
                {
                    SetAdditionalChanceForNemesisSwap(Save_Manager.instance.data.Factions.ForgottenKnights.Set_AddtlChanceForNemesisSwap);
                }
                else if (orig_AdditionalChanceForSwappableNemesisUnique != default_float_val)
                {
                    ResetAdditionalChanceForNemesisSwap();
                }
                // Bonus Corruption Per Gaze Of Orobyss
                if (Save_Manager.instance.data.Factions.ForgottenKnights.Enable_SetBonusCorruptionPerGaze)
                {
                    SetBonusCorruptionPerGazeOfOrobyss(Save_Manager.instance.data.Factions.ForgottenKnights.Set_BonusCorruptionPerGaze);
                }
                else if (orig_BonusCorruptionPerGazeOfOrobyss != default_int_val)
                {
                    ResetBonusCorruptionPerGazeOfOrobyss();
                }
                // Bonus Gaze Of Orobyss On Harbinger Kill
                if (Save_Manager.instance.data.Factions.ForgottenKnights.Enable_SetBonusGazeOnHarbingerKill)
                {
                    SetBonusGazeOfOrobyssOnHarbingerKill(Save_Manager.instance.data.Factions.ForgottenKnights.Set_BonusGazeOnHarbingerKill);
                }
                else if (orig_BonusGazeOfOrobyssOnHarbingerKill != default_int_val)
                {
                    ResetBonusGazeOfOrobyssOnHarbingerKill();
                }
                // Defeated Aterroths
                if (Save_Manager.instance.data.Factions.ForgottenKnights.Enable_SetAberrothDefeats)
                {
                    SetDefeatedAterroths(Save_Manager.instance.data.Factions.ForgottenKnights.Set_AberrothDefeats);
                }
                else if (orig_defeatedAterroths != default_int_val)
                {
                    ResetDefeatedAterroths();
                }
                // Double Stability Gain Chance
                if (Save_Manager.instance.data.Factions.ForgottenKnights.Enable_SetDoubleStabilityGainChance)
                {
                    SetDoubleStabilityGainChance(Save_Manager.instance.data.Factions.ForgottenKnights.Set_DoubleStabilityGainChance);
                }
                else if (orig_DoubleStabilityGainChance != default_float_val)
                {
                    ResetDoubleStabilityGainChance();
                }
                // Increased Chance To Drop Harbingers Needles
                if (Save_Manager.instance.data.Factions.ForgottenKnights.Enable_SetIncreasedChanceToDropHarbingersNeedles)
                {
                    SetIncreasedChanceToDropHarbingersNeedles(Save_Manager.instance.data.Factions.ForgottenKnights.Set_IncreasedChanceToDropHarbingersNeedles);
                }
                else if (orig_IncreasedChanceToDropHarbingersNeedles != default_float_val)
                {
                    ResetIncreasedChanceToDropHarbingersNeedles();
                }
                // Increased Chance To Drop Harbinger Specific Loot
                if (Save_Manager.instance.data.Factions.ForgottenKnights.Enable_SetIncreasedChanceToDropHarbingerSpecificLoot)
                {
                    SetIncreasedChanceToDropHarbingerSpecificLoot(Save_Manager.instance.data.Factions.ForgottenKnights.Set_IncreasedChanceToDropHarbingerSpecificLoot);
                }
                else if (orig_IncreasedChanceToDropHarbingerSpecificLoot != default_float_val)
                {
                    ResetIncreasedChanceToDropHarbingerSpecificLoot();
                }
            }
        }
        #endregion

        #region Functions
        private static bool checkIntVal(int val) { return val >= min_int_value && val <= max_int_value; }
        private static bool checkFloatVal(float val) { return val >= min_float_value && val <= max_float_value; }
        private static ForgottenKnights getForgottenKnightsFaction()
        {
            Faction faction = getFaction(factionId);
            ForgottenKnights fkFaction = faction as ForgottenKnights;
            if (fkFaction.IsNullOrDestroyed())
            {
                Main.logger_instance?.Error($"Factions_ForgottenKnights.SetAdditionalChanceForNemesisSwap() - Failed to find {FactionID.ForgottenKnights} faction.");
            }

            return fkFaction;
        }
        #region Reset
        /// <summary>
        /// Resets <see cref="ForgottenKnights.AdditionalChanceForHarbingersToDropGlyphOfEnvy"/> to its original value.
        /// </summary>
        public static void ResetAdditionalChanceForHarbingersToDropGlyphOfEnvy()
        {
            ForgottenKnights fkFaction = getForgottenKnightsFaction();
            if (!fkFaction.IsNullOrDestroyed() && checkFloatVal(orig_AdditionalChanceForHarbingersToDropGlyphOfEnvy) && fkFaction.AdditionalChanceForHarbingersToDropGlyphOfEnvy != orig_AdditionalChanceForHarbingersToDropGlyphOfEnvy)
            {
                fkFaction.AdditionalChanceForHarbingersToDropGlyphOfEnvy = orig_AdditionalChanceForHarbingersToDropGlyphOfEnvy;
                orig_AdditionalChanceForHarbingersToDropGlyphOfEnvy = default_float_val;
                Main.logger_instance?.Msg(logColor, $"Factions_ForgottenKnights.ResetAdditionalChanceForHarbingersToDropGlyphOfEnvy() - Reset AdditionalChanceForHarbingersToDropGlyphOfEnvy = {fkFaction.AdditionalChanceForHarbingersToDropGlyphOfEnvy}");
            }
        }
        /// <summary>
        /// Resets <see cref="ForgottenKnights.AdditionalChanceForSwappableNemesisUnique"/> to its original value.
        /// </summary>
        public static void ResetAdditionalChanceForNemesisSwap()
        {
            ForgottenKnights fkFaction = getForgottenKnightsFaction();
            if (!fkFaction.IsNullOrDestroyed() && checkFloatVal(orig_AdditionalChanceForSwappableNemesisUnique) && fkFaction.AdditionalChanceForSwappableNemesisUnique != orig_AdditionalChanceForSwappableNemesisUnique)
            {
                fkFaction.AdditionalChanceForSwappableNemesisUnique = orig_AdditionalChanceForSwappableNemesisUnique;
                orig_AdditionalChanceForSwappableNemesisUnique = default_float_val;
                Main.logger_instance?.Msg(logColor, $"Factions_ForgottenKnights.ResetAdditionalChanceForNemesisSwap() - Reset AdditionalChanceForSwappableNemesisUnique = {fkFaction.AdditionalChanceForSwappableNemesisUnique}");
            }
        }
        /// <summary>
        /// Resets <see cref="ForgottenKnights.BonusCorruptionPerGazeOfOrobyss"/> to its original value.
        /// </summary>
        public static void ResetBonusCorruptionPerGazeOfOrobyss()
        {
            ForgottenKnights fkFaction = getForgottenKnightsFaction();
            if (!fkFaction.IsNullOrDestroyed() && checkIntVal(orig_BonusCorruptionPerGazeOfOrobyss) && fkFaction.BonusCorruptionPerGazeOfOrobyss != orig_BonusCorruptionPerGazeOfOrobyss)
            {
                fkFaction.BonusCorruptionPerGazeOfOrobyss = orig_BonusCorruptionPerGazeOfOrobyss;
                orig_BonusCorruptionPerGazeOfOrobyss = default_int_val;
                Main.logger_instance?.Msg(logColor, $"Factions_ForgottenKnights.ResetBonusCorruptionPerGazeOfOrobyss() - Reset BonusCorruptionPerGazeOfOrobyss = {fkFaction.BonusCorruptionPerGazeOfOrobyss}");
            }
        }
        /// <summary>
        /// Resets <see cref="ForgottenKnights.BonusGazeOfOrobyssOnHarbingerKill"/> to its original value.
        /// </summary>
        public static void ResetBonusGazeOfOrobyssOnHarbingerKill()
        {
            ForgottenKnights fkFaction = getForgottenKnightsFaction();
            if (!fkFaction.IsNullOrDestroyed() && checkIntVal(orig_BonusGazeOfOrobyssOnHarbingerKill) && fkFaction.BonusGazeOfOrobyssOnHarbingerKill != orig_BonusGazeOfOrobyssOnHarbingerKill)
            {
                fkFaction.BonusGazeOfOrobyssOnHarbingerKill = orig_BonusGazeOfOrobyssOnHarbingerKill;
                orig_BonusGazeOfOrobyssOnHarbingerKill = default_int_val;
                Main.logger_instance?.Msg(logColor, $"Factions_ForgottenKnights.ResetBonusGazeOfOrobyssOnHarbingerKill() - Reset BonusGazeOfOrobyssOnHarbingerKill = {fkFaction.BonusGazeOfOrobyssOnHarbingerKill}");
            }
        }
        /// <summary>
        /// Resets <see cref="ForgottenKnights.BonusMaxForgingPotentialWhenEmpoweringNemesis"/> to its original value.
        /// </summary>
        public static void ResetBonusMaxForgingPotentialWhenEmpoweringNemesis()
        {
            ForgottenKnights fkFaction = getForgottenKnightsFaction();
            if (!fkFaction.IsNullOrDestroyed() && checkIntVal(orig_BonusMaxForgingPotentialWhenEmpoweringNemesis) && fkFaction.BonusMaxForgingPotentialWhenEmpoweringNemesis != orig_BonusMaxForgingPotentialWhenEmpoweringNemesis)
            {
                fkFaction.BonusMaxForgingPotentialWhenEmpoweringNemesis = orig_BonusMaxForgingPotentialWhenEmpoweringNemesis;
                orig_BonusMaxForgingPotentialWhenEmpoweringNemesis = default_int_val;
                Main.logger_instance?.Msg(logColor, $"Factions_ForgottenKnights.ResetBonusMaxForgingPotentialWhenEmpoweringNemesis() - Reset BonusMaxForgingPotentialWhenEmpoweringNemesis = {fkFaction.BonusMaxForgingPotentialWhenEmpoweringNemesis}");
            }
        }
        /// <summary>
        /// Resets <see cref="HarbingersProgress.defeatedAterroths"/> to its original value.
        /// </summary>
        public static void ResetDefeatedAterroths()
        {
            HarbingersProgress harbProg = new HarbingersProgress();
            if (checkIntVal(orig_defeatedAterroths) && harbProg.defeatedAterroths != orig_defeatedAterroths)
            {
                harbProg.defeatedAterroths = orig_defeatedAterroths;
                orig_defeatedAterroths = default_int_val;
                Main.logger_instance?.Msg(logColor, $"Factions_ForgottenKnights.ResetDefeatedAterroths() - Reset defeatedAterroths = {harbProg.defeatedAterroths}");
            }
        }
        /// <summary>
        /// Resets <see cref="ForgottenKnights.DoubleStabilityGainChance"/> to its original value.
        /// </summary>
        public static void ResetDoubleStabilityGainChance()
        {
            ForgottenKnights fkFaction = getForgottenKnightsFaction();
            if (!fkFaction.IsNullOrDestroyed() && checkFloatVal(orig_DoubleStabilityGainChance) && fkFaction.DoubleStabilityGainChance != orig_DoubleStabilityGainChance)
            {
                fkFaction.DoubleStabilityGainChance = orig_DoubleStabilityGainChance;
                orig_DoubleStabilityGainChance = default_float_val;
                Main.logger_instance?.Msg(logColor, $"Factions_ForgottenKnights.ResetDoubleStabilityGainChance() - Reset DoubleStabilityGainChance = {fkFaction.DoubleStabilityGainChance}");
            }
        }
        /// <summary>
        /// Resets <see cref="ForgottenKnights.IncreasedChanceToDropHarbingersNeedles"/> to its original value.
        /// </summary>
        public static void ResetIncreasedChanceToDropHarbingersNeedles()
        {
            ForgottenKnights fkFaction = getForgottenKnightsFaction();
            if (!fkFaction.IsNullOrDestroyed() && checkFloatVal(orig_IncreasedChanceToDropHarbingersNeedles) && fkFaction.IncreasedChanceToDropHarbingersNeedles != orig_IncreasedChanceToDropHarbingersNeedles)
            {
                fkFaction.IncreasedChanceToDropHarbingersNeedles = orig_IncreasedChanceToDropHarbingersNeedles;
                orig_IncreasedChanceToDropHarbingersNeedles = default_float_val;
                Main.logger_instance?.Msg(logColor, $"Factions_ForgottenKnights.ResetIncreasedChanceToDropHarbingersNeedles() - Reset IncreasedChanceToDropHarbingersNeedles = {fkFaction.IncreasedChanceToDropHarbingersNeedles}");
            }
        }
        /// <summary>
        /// Resets <see cref="ForgottenKnights.IncreasedChanceToDropHarbingerSpecificLoot"/> to its original value.
        /// </summary>
        public static void ResetIncreasedChanceToDropHarbingerSpecificLoot()
        {
            ForgottenKnights fkFaction = getForgottenKnightsFaction();
            if (!fkFaction.IsNullOrDestroyed() && checkFloatVal(orig_IncreasedChanceToDropHarbingerSpecificLoot) && fkFaction.IncreasedChanceToDropHarbingerSpecificLoot != orig_IncreasedChanceToDropHarbingerSpecificLoot)
            {
                fkFaction.IncreasedChanceToDropHarbingerSpecificLoot = orig_IncreasedChanceToDropHarbingerSpecificLoot;
                orig_IncreasedChanceToDropHarbingerSpecificLoot = default_float_val;
                Main.logger_instance?.Msg(logColor, $"Factions_ForgottenKnights.ResetIncreasedChanceToDropHarbingerSpecificLoot() - Reset IncreasedChanceToDropHarbingerSpecificLoot = {fkFaction.IncreasedChanceToDropHarbingerSpecificLoot}");
            }
        }
        #endregion
        #region Set
        /// <summary>
        /// Sets <see cref="ForgottenKnights.AdditionalChanceForHarbingersToDropGlyphOfEnvy"/> to the specified value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public static void SetAdditionalChanceForHarbingersToDropGlyphOfEnvy(float value)
        {
            ForgottenKnights fkFaction = getForgottenKnightsFaction();
            if (!fkFaction.IsNullOrDestroyed() && checkFloatVal(value) && fkFaction.AdditionalChanceForHarbingersToDropGlyphOfEnvy != value)
            {
                if (orig_AdditionalChanceForHarbingersToDropGlyphOfEnvy == default_float_val)
                {
                    orig_AdditionalChanceForHarbingersToDropGlyphOfEnvy = fkFaction.AdditionalChanceForHarbingersToDropGlyphOfEnvy;
                }
                fkFaction.AdditionalChanceForHarbingersToDropGlyphOfEnvy = value;
                Main.logger_instance?.Msg(logColor, $"Factions_ForgottenKnights.SetAdditionalChanceForHarbingersToDropGlyphOfEnvy() - Set AdditionalChanceForHarbingersToDropGlyphOfEnvy = {fkFaction.AdditionalChanceForHarbingersToDropGlyphOfEnvy}");
            }
        }
        /// <summary>
        /// Sets <see cref="ForgottenKnights.AdditionalChanceForSwappableNemesisUnique"/> to the specified value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public static void SetAdditionalChanceForNemesisSwap(float value)
        {
            ForgottenKnights fkFaction = getForgottenKnightsFaction();
            if (!fkFaction.IsNullOrDestroyed() && checkFloatVal(value) && fkFaction.AdditionalChanceForSwappableNemesisUnique != value)
            {
                if (orig_AdditionalChanceForSwappableNemesisUnique == default_float_val)
                {
                    orig_AdditionalChanceForSwappableNemesisUnique = fkFaction.AdditionalChanceForSwappableNemesisUnique;
                }
                fkFaction.AdditionalChanceForSwappableNemesisUnique = value;
                Main.logger_instance?.Msg(logColor, $"Factions_ForgottenKnights.SetAdditionalChanceForNemesisSwap() - Set AdditionalChanceForSwappableNemesisUnique = {fkFaction.AdditionalChanceForSwappableNemesisUnique}");
            }
        }
        /// <summary>
        /// Sets <see cref="ForgottenKnights.BonusCorruptionPerGazeOfOrobyss"/> to the specified value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public static void SetBonusCorruptionPerGazeOfOrobyss(int value)
        {
            ForgottenKnights fkFaction = getForgottenKnightsFaction();
            if (!fkFaction.IsNullOrDestroyed() && checkIntVal(value) && fkFaction.BonusCorruptionPerGazeOfOrobyss != value)
            {
                if (orig_AdditionalChanceForSwappableNemesisUnique == default_float_val)
                {
                    orig_AdditionalChanceForSwappableNemesisUnique = fkFaction.BonusCorruptionPerGazeOfOrobyss;
                }
                fkFaction.BonusCorruptionPerGazeOfOrobyss = value;
                Main.logger_instance?.Msg(logColor, $"Factions_ForgottenKnights.SetBonusCorruptionPerGazeOfOrobyss() - Set BonusCorruptionPerGazeOfOrobyss = {fkFaction.BonusCorruptionPerGazeOfOrobyss}");
            }
        }
        /// <summary>
        /// Sets <see cref="ForgottenKnights.BonusGazeOfOrobyssOnHarbingerKill"/> to the specified value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public static void SetBonusGazeOfOrobyssOnHarbingerKill(int value)
        {
            ForgottenKnights fkFaction = getForgottenKnightsFaction();
            if (!fkFaction.IsNullOrDestroyed() && checkIntVal(value) && fkFaction.BonusGazeOfOrobyssOnHarbingerKill != value)
            {
                if (orig_BonusGazeOfOrobyssOnHarbingerKill == default_int_val)
                {
                    orig_BonusGazeOfOrobyssOnHarbingerKill = fkFaction.BonusGazeOfOrobyssOnHarbingerKill;
                }
                fkFaction.BonusGazeOfOrobyssOnHarbingerKill = value;
                Main.logger_instance?.Msg(logColor, $"Factions_ForgottenKnights.SetBonusGazeOfOrobyssOnHarbingerKill() - Set BonusGazeOfOrobyssOnHarbingerKill = {fkFaction.BonusGazeOfOrobyssOnHarbingerKill}");
            }
        }
        /// <summary>
        /// Sets <see cref="ForgottenKnights.BonusMaxForgingPotentialWhenEmpoweringNemesis"/> to the specified value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public static void SetBonusMaxForgingPotentialWhenEmpoweringNemesis(int value)
        {
            ForgottenKnights fkFaction = getForgottenKnightsFaction();
            if (!fkFaction.IsNullOrDestroyed() && checkIntVal(value) && fkFaction.BonusMaxForgingPotentialWhenEmpoweringNemesis != value)
            {
                if (orig_BonusMaxForgingPotentialWhenEmpoweringNemesis == default_int_val)
                {
                    orig_BonusMaxForgingPotentialWhenEmpoweringNemesis = fkFaction.BonusMaxForgingPotentialWhenEmpoweringNemesis;
                }
                fkFaction.BonusMaxForgingPotentialWhenEmpoweringNemesis = value;
                Main.logger_instance?.Msg(logColor, $"Factions_ForgottenKnights.SetBonusMaxForgingPotentialWhenEmpoweringNemesis() - Set BonusMaxForgingPotentialWhenEmpoweringNemesis = {fkFaction.BonusMaxForgingPotentialWhenEmpoweringNemesis}");
            }
        }
        /// <summary>
        /// Sets <see cref="HarbingersProgress.defeatedAterroths"/> to the specified value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public static void SetDefeatedAterroths(int value)
        {
            HarbingersProgress harbProg = new HarbingersProgress();
            if (checkIntVal(value) && value != harbProg.defeatedAterroths)
            {
                if (orig_defeatedAterroths == default_int_val)
                {
                    orig_defeatedAterroths = harbProg.defeatedAterroths;
                }
                harbProg.defeatedAterroths = value;
                Main.logger_instance?.Msg(logColor, $"Factions_ForgottenKnights.SetDefeatedAterroths() - Set defeatedAterroths = {harbProg.defeatedAterroths}");
            }
        }
        /// <summary>
        /// Sets <see cref="ForgottenKnights.DoubleStabilityGainChance"/> to the specified value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public static void SetDoubleStabilityGainChance(float value)
        {
            ForgottenKnights fkFaction = getForgottenKnightsFaction();
            if (!fkFaction.IsNullOrDestroyed() && checkFloatVal(value) && fkFaction.DoubleStabilityGainChance != value)
            {
                if (orig_DoubleStabilityGainChance == default_float_val)
                {
                    orig_DoubleStabilityGainChance = fkFaction.DoubleStabilityGainChance;
                }
                fkFaction.DoubleStabilityGainChance = value;
                Main.logger_instance?.Msg(logColor, $"Factions_ForgottenKnights.SetDoubleStabilityGainChance() - Set DoubleStabilityGainChance = {fkFaction.BonusGazeOfOrobyssOnHarbingerKill}");
            }
        }
        /// <summary>
        /// Sets <see cref="ForgottenKnights.IncreasedChanceToDropHarbingersNeedles"/> to the specified value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public static void SetIncreasedChanceToDropHarbingersNeedles(float value)
        {
            ForgottenKnights fkFaction = getForgottenKnightsFaction();
            if (!fkFaction.IsNullOrDestroyed() && checkFloatVal(value) && fkFaction.IncreasedChanceToDropHarbingersNeedles != value)
            {
                if (orig_IncreasedChanceToDropHarbingersNeedles == default_float_val)
                {
                    orig_IncreasedChanceToDropHarbingersNeedles = fkFaction.IncreasedChanceToDropHarbingersNeedles;
                }
                fkFaction.IncreasedChanceToDropHarbingersNeedles = value;
                Main.logger_instance?.Msg(logColor, $"Factions_ForgottenKnights.SetIncreasedChanceToDropHarbingersNeedles() - Set IncreasedChanceToDropHarbingersNeedles = {fkFaction.IncreasedChanceToDropHarbingersNeedles}");
            }
        }
        /// <summary>
        /// Sets <see cref="ForgottenKnights.IncreasedChanceToDropHarbingerSpecificLoot"/> to the specified value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public static void SetIncreasedChanceToDropHarbingerSpecificLoot(float value)
        {
            ForgottenKnights fkFaction = getForgottenKnightsFaction();
            if (!fkFaction.IsNullOrDestroyed() && checkFloatVal(value) && fkFaction.IncreasedChanceToDropHarbingerSpecificLoot != value)
            {
                if (orig_IncreasedChanceToDropHarbingerSpecificLoot == default_float_val)
                {
                    orig_IncreasedChanceToDropHarbingerSpecificLoot = fkFaction.IncreasedChanceToDropHarbingerSpecificLoot;
                }
                fkFaction.IncreasedChanceToDropHarbingerSpecificLoot = value;
                Main.logger_instance?.Msg(logColor, $"Factions_ForgottenKnights.SetIncreasedChanceToDropHarbingerSpecificLoot() - Set IncreasedChanceToDropHarbingerSpecificLoot = {fkFaction.IncreasedChanceToDropHarbingerSpecificLoot}");
            }
        }
        #endregion
        #endregion
    }
}
