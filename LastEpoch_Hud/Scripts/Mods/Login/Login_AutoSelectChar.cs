using HarmonyLib;
using Il2Cpp;
using Il2CppCysharp.Threading.Tasks;
using Il2CppLE.Data;
using Il2CppOperationResult;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace LastEpoch_Hud.Scripts.Mods.Login
{
    /// <summary>
    /// Mod to automatically select a character on the character selection screen based on the name specified in the settings.
    /// </summary>
    [RegisterTypeInIl2Cpp]
    public class Login_AutoSelectChar : LastEpochMod
    {
        #region Constants
        private const string CHAR_SELECT_SCENE_NAME = "CharacterSelectScene";
        #endregion

        #region Properties
        public static new bool CanRun
        {
            get
            {
                return IsCharacterSelectionScene && !Refs_Manager.game_uibase.IsNullOrDestroyed() && Refs_Manager.game_uibase.characterSelectOpen && !Save_Manager.instance.IsNullOrDestroyed() &&
                    !Save_Manager.instance.data.IsNullOrDestroyed() && !Refs_Manager.character_select.IsNullOrDestroyed() && Save_Manager.instance.data.Login.Enable_AutoSelectChar;
            }
        }
        public static Login_AutoSelectChar Instance { get; private set; }
        private static bool IsCharacterSelectionScene { get { return SceneManager.GetActiveScene().name == CHAR_SELECT_SCENE_NAME; } }
        #endregion

        #region .ctor
        public Login_AutoSelectChar(System.IntPtr ptr) : base(ptr) { }
        #endregion

        #region Overrides        
        private void Awake()
        {
            Instance = this;
        }
        private void Update()
        {
            if (CanRun)
            {
                if (!string.IsNullOrWhiteSpace(Save_Manager.instance.data.Login.AutoSelectCharName))
                {
                    if (Save_Manager.instance.data.Login.LegacyCharacter && !Refs_Manager.character_select.CharacterCreationRealm_Legacy.active)
                    {
                        Refs_Manager.character_select.CharacterCreationRealm_Legacy.active = true;
                    }

                    if (Refs_Manager.character_select.AvailableCharacterTiles.Count > 0)
                    {
                        string charName = Save_Manager.instance.data.Login.AutoSelectCharName;
                        CharacterTile tile = getCharacterTile(charName);
                        if (!tile.IsNullOrDestroyed() && tile.isActiveAndEnabled)
                        {
                            Refs_Manager.character_select.OnCharacterTileDoubleClicked(tile);
                            Main.logger_instance?.Msg($"Login_AutoSelectChar.Update() - Selected {tile.characterCycle} {(CharacterClassID)tile.characterData.CharacterClass} character {tile.characterData.CharacterName}");
                        }
                        else { Main.logger_instance?.Msg($"Login_AutoSelectChar.Update() - Could not find tile for character {charName}"); }
                    }
                    else { Main.logger_instance?.Msg("Login_AutoSelectChar.Update() - No characters available"); Save_Manager.instance.data.Login.Enable_AutoSelectChar = false; }
                }
                else { Main.logger_instance?.Msg("Login_AutoSelectChar.Update() - Character name is empty"); Save_Manager.instance.data.Login.Enable_AutoSelectChar = false; }
            }
        }
        #endregion

        #region Functions
        /// <summary>
        /// Gets the <see cref="CharacterTile"/> associated with the specified character's name.
        /// </summary>
        /// <param name="name">The name of the character to get the <see cref="CharacterTile"/> for.</param>
        /// <returns>Returns a <see cref="CharacterTile"/> if one is found for the specified character name; otherwise, <see langword="null"/>.</returns>
        private static CharacterTile getCharacterTile(string name)
        {
            if (!string.IsNullOrEmpty(name) && !CharacterSelect.instance.AvailableCharacterTiles.IsNullOrDestroyed() && CharacterSelect.instance.AvailableCharacterTiles.Count > 0)
            {
                foreach (CharacterTile tile in CharacterSelect.instance.AvailableCharacterTiles)
                {
                    if (!tile.characterData.IsNullOrDestroyed() && tile.characterData.CharacterName == name)
                    {
                        return tile;
                    }
                }
            }

            return null;
        }
        #endregion
    }
}
