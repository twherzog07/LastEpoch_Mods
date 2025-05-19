using HarmonyLib;
using Il2CppLE.Data;
using Il2CppLE.Factions;
using Il2CppSystem.Linq;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastEpoch_Hud.Scripts.Mods.Factions
{
    /// <summary>
    /// Base class for all faction mods.
    /// </summary>
    public abstract class Factions_Faction : LastEpochMod
    {
        #region Vars
        #region Constants
        private const int default_val = -1;
        #endregion

        //protected System.ConsoleColor logColor;
        public Save_Manager.Data.Rank Rank = new Save_Manager.Data.Rank();
        protected static Dictionary<FactionID, int> orig_rank = new Dictionary<FactionID, int>
            {
            { FactionID.CircleOfFortune, default_val },
            { FactionID.MerchantsGuild, default_val },
            { FactionID.ForgottenKnights, default_val },
            { FactionID.TheWeaver, default_val }
        };
        protected static Dictionary<FactionID, int> orig_rep = new Dictionary<FactionID, int>
        {
            { FactionID.CircleOfFortune, default_val },
            { FactionID.MerchantsGuild, default_val },
            { FactionID.ForgottenKnights, default_val },
            { FactionID.TheWeaver, default_val }
        };
        private static System.ConsoleColor logColor;
        #endregion

        #region Properties
        public Faction Faction { get; protected set; }
        public FactionID FactionID { get; protected set; }
        protected virtual Dictionary<int, int> RankReputation { get; set; }
        #endregion

        #region .ctor
        public Factions_Faction() : base() { }
        public Factions_Faction(IntPtr ptr) : base(ptr) { }
        #endregion

        #region Overrides
        protected void Awake()
        {
            getRequiredRanksRep();
            logColor = getLogColorFromFactionColor(FactionID);
        }
        protected void Update()
        {
            if (!Refs_Manager.player_data_tracker.IsNullOrDestroyed() && Faction.IsNullOrDestroyed()) { Faction = getFaction(FactionID); }
            if (CanRun && FactionID >= FactionID.CircleOfFortune && FactionID <= FactionID.TheWeaver)
            {
                if (Rank.Enable_SetRank)
                {
                    SetRank(Rank.SetRank);
                }
                else if (orig_rank[FactionID] != default_val)
                {
                    ResetRank();
                }
            }
        }
        #endregion

        #region Functions
        /// <summary>
        /// Gets the <see cref="Il2CppLE.Factions.Faction"/> for the <see cref="Refs_Manager.player_data_tracker">current player data tracker</see> for the specified <see cref="Il2CppLE.Factions.FactionID"/>.
        /// </summary>
        /// <param name="factionID">The <see cref="Il2CppLE.Factions.FactionID"/> for the faction to get.</param>
        /// <returns>Returns the <see cref="Il2CppLE.Factions.Faction"/> for the <see cref="Refs_Manager.player_data_tracker">current player data tracker</see> 
        /// for the specified <see cref="Il2CppLE.Factions.FactionID"/>, if it exists; otherwise, <see langword="null"/>.</returns>
        protected static Faction getFaction(FactionID factionID)
        {
            if (!Refs_Manager.player_data_tracker.IsNullOrDestroyed())
            {
                Il2CppSystem.Collections.Generic.List<Faction> factions = Refs_Manager.player_data_tracker.getFactionInfoProvider().GetFactions().ToList();
                foreach (Faction faction in factions)
                {
                    if (faction.ID == factionID)
                    {
                        return faction;
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Gets the <see cref="FactionCharacterData"/> from the <see cref="Refs_Manager.player_data">current player data</see> for the specified <see cref="Il2CppLE.Factions.FactionID"/>.
        /// </summary>
        /// <param name="faction">The <see cref="Il2CppLE.Factions.FactionID"/> for the faction to get.</param>
        /// <param name="attemptJoin">Optional setting to attempt to join the faction if the character is not currently a member.</param>
        /// <returns>Returns the <see cref="FactionCharacterData"/> from the <see cref="Refs_Manager.player_data">current player data</see> 
        /// for the specified <see cref="Il2CppLE.Factions.FactionID"/>, if it exists; otherwise, <see langword="null"/>.</returns>
        public static FactionCharacterData getFactionCharData(FactionID faction, bool attemptJoin = false)
        {
            if (!Refs_Manager.player_data.IsNullOrDestroyed())
            {
                foreach (FactionCharacterData factionData in Refs_Manager.player_data.Factions.Values)
                {
                    if (factionData.ID == (int)faction)
                    {
                        return factionData;
                    }
                }
                if (attemptJoin)
                {
                    // Attempt to join the faction since we aren't a member
                    JoinFaction(faction);
                    // Try again
                    foreach (FactionCharacterData factionData in Refs_Manager.player_data.Factions.Values)
                    {
                        if (factionData.ID == (int)faction)
                        {
                            return factionData;
                        }
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Gets the <see cref="FactionData"/> for the specified <see cref="Il2CppLE.Factions.FactionID"/>.
        /// </summary>
        /// <param name="faction">The <see cref="Il2CppLE.Factions.FactionID"/> for the faction to get.</param>
        /// <returns>Returns the <see cref="FactionData"/> for the specified <see cref="Il2CppLE.Factions.FactionID"/>.</returns>
        protected static FactionData getFactionData(FactionID faction) { return FactionsList.GetFactionDataByID(faction); }
        protected static System.ConsoleColor getLogColorFromFactionColor(FactionID faction)
        {
            FactionData factionData = getFactionData(faction);
            if (!factionData.IsNullOrDestroyed()) { UnityEngine.Color unityColor = factionData.GetColor; return ConsoleColorFromColor(System.Drawing.Color.FromArgb((int)unityColor.a, (int)unityColor.r, (int)unityColor.g, (int)unityColor.b)); }

            return System.ConsoleColor.Gray;
        }
        /// <summary>
        /// Gets the required reputation for each rank in the faction, and stores it in <see cref="RankReputation"/>.
        /// </summary>
        protected void getRequiredRanksRep()
        {
            FactionData factionData = getFactionData(FactionID);
            if (!factionData.IsNullOrDestroyed())
            {
                RankReputation = new Dictionary<int, int>();
                for (int i = 0; i <= factionData.MaxRank; i++)
                {
                    int requiredRep = i > 0 ? factionData.TotalReputationForNextRank(i) : 0;
                    RankReputation.Add(i, requiredRep);
                }
            }
        }
        /// <summary>
        /// Attempts to join the specified faction.
        /// </summary>
        /// <param name="faction">The <see cref="Il2CppLE.Factions.FactionID"/> for the faction to join.</param>
        public static void JoinFaction(FactionID faction)
        {
            Faction f = getFaction(faction);
            if (!f.IsNullOrDestroyed()) { f.Join(false); }
        }
        public void Join()
        {
            JoinFaction(FactionID);
        }
        /// <summary>
        /// Resets faction rank back to the original value.
        /// </summary>
        public void ResetRank()
        {
            FactionCharacterData faction = getFactionCharData(FactionID);
            if (!faction.IsNullOrDestroyed())
            {
                if (orig_rep[FactionID] != default_val && faction.Reputation != orig_rep[FactionID])
                {
                    faction.Reputation = orig_rep[FactionID];
                    orig_rep[FactionID] = default_val;
                    Main.logger_instance?.Msg(logColor, $"Factions_Faction.ResetRank() - Set {FactionID} Reputation Back To Original Reputation {faction.Reputation}");
                }
                if (orig_rank[FactionID] != default_val && orig_rank[FactionID] != faction.Rank)
                {
                    faction.Rank = orig_rank[FactionID];
                    orig_rank[FactionID] = default_val;
                    Main.logger_instance?.Msg(logColor, $"Factions_Faction.ResetRank() - Set {FactionID} Rank Back To Original Rank {faction.Rank}");
                }
            }
            else
            {
                string charName = !Refs_Manager.player_data.IsNullOrDestroyed() ? Refs_Manager.player_data.CharacterName : "(Empty)";
                Main.logger_instance?.Error($"Factions_Faction.ResetRank() - Faction {FactionID} not found for character {charName}");
            }
        }
        protected void SetFaction(FactionID faction)
        {
            FactionID = faction;
        }
        /// <summary>
        /// Sets faction rank to the specified value.
        /// </summary>
        /// <param name="rank">The rank to set the faction to.</param>
        public void SetRank(int rank)
        {
            FactionCharacterData factionCharData = getFactionCharData(FactionID);
            int rankRep = RankReputation[rank];
            if (!factionCharData.IsNullOrDestroyed() && (factionCharData.Rank != rank || factionCharData.Reputation != rankRep))
            {
                if (orig_rep[FactionID] == default_val)
                {
                    orig_rep[FactionID] = factionCharData.Reputation;
                }
                factionCharData.Reputation = rankRep;
                Main.logger_instance?.Msg(logColor, $"Factions_Faction.SetRank() - Set {FactionID} Reputation = {factionCharData.Reputation}");
                if (orig_rank[FactionID] == default_val)
                {
                    orig_rank[FactionID] = factionCharData.Rank;
                }
                factionCharData.Rank = rank;
                Main.logger_instance?.Msg(logColor, $"Factions_Faction.SetRank() - Set {FactionID} Rank = {factionCharData.Rank}");
            }
            else
            {
                string charName = !Refs_Manager.player_data.IsNullOrDestroyed() ? Refs_Manager.player_data.CharacterName : "(Empty)";
                Main.logger_instance?.Error($"Factions_Faction.SetRank() - Faction {FactionID} not found for character {charName}");
            }
        }
        #endregion
    }
}
