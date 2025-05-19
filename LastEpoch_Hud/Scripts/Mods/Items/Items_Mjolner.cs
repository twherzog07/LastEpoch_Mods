//https://discord.com/channels/1366160878579351756/1372660677491036272

using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastEpoch_Hud.Scripts.Mods.Items
{
    [RegisterTypeInIl2Cpp]
    public class Items_Mjolner : MonoBehaviour
    {
        public static Items_Mjolner? instance { get; private set; }
        public Items_Mjolner(System.IntPtr ptr) : base(ptr) { }

        bool InGame = false;

        void Awake()
        {
            instance = this;
            SceneManager.add_sceneLoaded(new System.Action<Scene, LoadSceneMode>(OnSceneLoaded));
        }
        void Update()
        {
            if (Unique.Icon.IsNullOrDestroyed()) { Unique.Get_Unique_Icon(); }
            if ((Locales.current != Locales.Selected.Unknow) && (!Unique.AddedToUniqueList)) { Unique.AddToUniqueList(); }
            if ((Locales.current != Locales.Selected.Unknow) && (Unique.AddedToUniqueList) && (!Unique.AddedToDictionary)) { Unique.AddToDictionary(); }
            if (!Events.OnHitEvent_Initialized) { Events.Init_OnHitEvent(); }
        }
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (Scenes.IsGameScene())
            {
                if (!InGame) { Events.OnHitEvent_Initialized = false; }
                InGame = true;
            }
            else if (InGame) { InGame = false; }
        }
        
        public class Basic
        {
            public static readonly byte base_type = 7;  //Mace
            public static readonly int base_id = 10;    //Rune hammer
        }
        public class Unique
        {
            public static bool AddedToUniqueList = false;
            public static bool AddedToDictionary = false;
            public static bool Loading_icon = false;
            public static Sprite Icon = null;
            public static readonly ushort unique_id = 421;

            public static UniqueList.Entry Item()
            {
                UniqueList.Entry item = new UniqueList.Entry
                {
                    name = Get_Unique_Name(),
                    displayName = Get_Unique_Name(),
                    uniqueID = unique_id,
                    isSetItem = false,
                    setID = 0,
                    overrideLevelRequirement = false,
                    levelRequirement = 78,
                    legendaryType = LegendaryType(),
                    overrideEffectiveLevelForLegendaryPotential = true,
                    effectiveLevelForLegendaryPotential = 60,
                    canDropRandomly = Save_Manager.instance.data.Items.Mjolner.UniqueDrop,
                    rerollChance = 1,
                    itemModelType = UniqueList.ItemModelType.Unique,
                    subTypeForIM = 0,
                    baseType = Basic.base_type,
                    subTypes = SubType(),
                    mods = Mods(),
                    tooltipDescriptions = TooltipDescription(),
                    loreText = Get_Unique_Lore(), //lore,
                    tooltipEntries = TooltipEntries(),
                    oldSubTypeID = 0,
                    oldUniqueID = 0
                };

                return item;
            }            
            public static void AddToUniqueList()
            {
                if ((!AddedToUniqueList) && (!Refs_Manager.unique_list.IsNullOrDestroyed()))
                {
                    try
                    {
                        UniqueList.getUnique(0); //force initialize uniquelist
                        Refs_Manager.unique_list.uniques.Add(Item());
                        AddedToUniqueList = true;
                    }
                    catch { Main.logger_instance.Error("Mjolner Unique List Error"); }
                }
            }
            public static void AddToDictionary()
            {
                if ((AddedToUniqueList) && (!AddedToDictionary) && (!Refs_Manager.unique_list.IsNullOrDestroyed()))
                {
                    try
                    {
                        UniqueList.Entry? item = null;
                        if (Refs_Manager.unique_list.uniques.Count > 1)
                        {
                            foreach (UniqueList.Entry unique in Refs_Manager.unique_list.uniques)
                            {
                                if ((unique.uniqueID == unique_id) && (unique.name == Get_Unique_Name()))
                                {
                                    item = unique;
                                    break;
                                }
                            }
                        }
                        if (!item.IsNullOrDestroyed())
                        {
                            Refs_Manager.unique_list.entryDictionary.Add(unique_id, item);
                            AddedToDictionary = true;
                        }
                    }
                    catch { Main.logger_instance.Error("Mjolner Unique Dictionary Error"); }
                }
            }
            public static void Get_Unique_Icon()
            {
                if ((!Loading_icon) && (!Hud_Manager.asset_bundle.IsNullOrDestroyed()))
                {
                    Loading_icon = true;                    
                    foreach (string name in Hud_Manager.asset_bundle.GetAllAssetNames())
                    {
                        if (name.Contains("mjolner.png"))
                        {
                            Texture2D texture = Hud_Manager.asset_bundle.LoadAsset(name).TryCast<Texture2D>();
                            Unique.Icon = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                            break;
                        }
                    }
                    Loading_icon = false;
                }
            }
            public static string Get_Unique_Name()
            {
                string result = "";
                switch (Locales.current)
                {
                    case Locales.Selected.English: { result = MjLocales.UniqueName.en; break; }
                    case Locales.Selected.French: { result = MjLocales.UniqueName.fr; break; }
                    case Locales.Selected.German: { result = MjLocales.UniqueName.de; break; }
                    case Locales.Selected.Russian: { result = MjLocales.UniqueName.ru; break; }
                    case Locales.Selected.Portuguese: { result = MjLocales.UniqueName.pt; break; }

                    case Locales.Selected.Korean: { result = MjLocales.UniqueName.en; break; }
                    case Locales.Selected.Polish: { result = MjLocales.UniqueName.en; break; }
                    case Locales.Selected.Chinese: { result = MjLocales.UniqueName.en; break; }
                    case Locales.Selected.Spanish: { result = MjLocales.UniqueName.en; break; }
                }

                return result;
            }
            public static string Get_Unique_Description()
            {
                string result = "";
                switch (Locales.current)
                {
                    case Locales.Selected.English: { result = MjLocales.UniqueDescription.en; break; }
                    case Locales.Selected.French: { result = MjLocales.UniqueDescription.fr; break; }

                    case Locales.Selected.Korean: { result = MjLocales.UniqueDescription.en; break; }
                    case Locales.Selected.German: { result = MjLocales.UniqueDescription.en; break; }
                    case Locales.Selected.Russian: { result = MjLocales.UniqueDescription.en; break; }
                    case Locales.Selected.Polish: { result = MjLocales.UniqueDescription.en; break; }
                    case Locales.Selected.Portuguese: { result = MjLocales.UniqueDescription.en; break; }
                    case Locales.Selected.Chinese: { result = MjLocales.UniqueDescription.en; break; }
                    case Locales.Selected.Spanish: { result = MjLocales.UniqueDescription.en; break; }
                }

                return result;
            }
            public static string Get_Unique_Lore()
            {
                string result = "";
                switch (Locales.current)
                {
                    case Locales.Selected.English: { result = MjLocales.Lore.en; break; }
                    case Locales.Selected.French: { result = MjLocales.Lore.fr; break; }
                    case Locales.Selected.German: { result = MjLocales.Lore.de; break; }

                    case Locales.Selected.Korean: { result = MjLocales.Lore.en; break; }
                    case Locales.Selected.Russian: { result = MjLocales.Lore.en; break; }
                    case Locales.Selected.Polish: { result = MjLocales.Lore.en; break; }
                    case Locales.Selected.Portuguese: { result = MjLocales.Lore.en; break; }
                    case Locales.Selected.Chinese: { result = MjLocales.Lore.en; break; }
                    case Locales.Selected.Spanish: { result = MjLocales.Lore.en; break; }
                }

                return result;
            }
            
            private static Il2CppSystem.Collections.Generic.List<byte> SubType()
            {
                Il2CppSystem.Collections.Generic.List<byte> result = new Il2CppSystem.Collections.Generic.List<byte>();
                byte r = (byte)Basic.base_id;
                result.Add(r);

                return result;
            }
            private static Il2CppSystem.Collections.Generic.List<UniqueItemMod> Mods()
            {
                Il2CppSystem.Collections.Generic.List<UniqueItemMod> result = new Il2CppSystem.Collections.Generic.List<UniqueItemMod>();
                result.Add(new UniqueItemMod
                {
                    canRoll = true,
                    property = SP.Damage,
                    tags = AT.Lightning,
                    type = BaseStats.ModType.INCREASED,
                    maxValue = 1.0f,
                    value = 0.8f
                });
                result.Add(new UniqueItemMod
                {
                    canRoll = true,
                    property = SP.Damage,
                    tags = AT.Physical,
                    type = BaseStats.ModType.INCREASED,
                    maxValue = 1.2f,
                    value = 0.8f
                });

                return result;
            }
            private static Il2CppSystem.Collections.Generic.List<UniqueModDisplayListEntry> TooltipEntries()
            {
                Il2CppSystem.Collections.Generic.List<UniqueModDisplayListEntry> result = new Il2CppSystem.Collections.Generic.List<UniqueModDisplayListEntry>();
                result.Add(new UniqueModDisplayListEntry(0));
                result.Add(new UniqueModDisplayListEntry(1));
                result.Add(new UniqueModDisplayListEntry(128));

                return result;
            }
            private static Il2CppSystem.Collections.Generic.List<ItemTooltipDescription> TooltipDescription()
            {
                Il2CppSystem.Collections.Generic.List<ItemTooltipDescription> result = new Il2CppSystem.Collections.Generic.List<ItemTooltipDescription>();
                result.Add(new ItemTooltipDescription { description = Get_Unique_Description() });

                return result;
            }
            private static UniqueList.LegendaryType LegendaryType()
            {
                UniqueList.LegendaryType legendaryType = UniqueList.LegendaryType.LegendaryPotential;
                if (Save_Manager.instance.data.Items.Mjolner.WeaverWill) { legendaryType = UniqueList.LegendaryType.WeaversWill; }

                return legendaryType;
            }
            
            //Fix for V1.2 (icon in inventory)
            [HarmonyPatch(typeof(InventoryItemUI), "SetImageSpritesAndColours")]
            public class InventoryItemUI_SetImageSpritesAndColours
            {
                [HarmonyPostfix]
                static void Postfix(ref Il2Cpp.InventoryItemUI __instance)
                {
                    if ((__instance.EntryRef.data.getAsUnpacked().FullName == Get_Unique_Name()) && (!Icon.IsNullOrDestroyed()))
                    {
                        __instance.contentImage.sprite = Icon;
                    }
                }
            }

            [HarmonyPatch(typeof(UITooltipItem), "GetItemSprite")]
            public class UITooltipItem_GetItemSprite
            {
                [HarmonyPostfix]
                static void Postfix(ref UnityEngine.Sprite __result, ItemData __0)
                {
                    if ((__0.getAsUnpacked().FullName == Get_Unique_Name()) && (!Icon.IsNullOrDestroyed()))
                    {
                        __result = Icon;
                    }
                }
            }
        }
        
        public class MjLocales
        {
            private static string unique_name_key = "Unique_Name_" + Unique.unique_id;
            private static string unique_description_key = "Unique_Tooltip_0_" + Unique.unique_id;
            private static string unique_lore_key = "Unique_Lore_" + Unique.unique_id;

            public class SubType
            {
                public static string en = "Rune Hammer";
                public static string fr = "Marteau Runique";
                public static string de = "Runenhammer";
                public static string ru = "Рунический молот";
                public static string pt = "Martelo Rúnico";
                //Add all languages here
            }
            public class UniqueName
            {
                public static string en = "Mjölner";
                public static string fr = "Mjölner";
                public static string de = "Mjölner";
                public static string ru = "Мьёльнир";
                public static string pt = "Mjölner";
                //Add all languages here
            }
            public class UniqueDescription
            {
                public static string en = Save_Manager.instance.data.Items.Mjolner.MinTriggerChance + " to " + Save_Manager.instance.data.Items.Mjolner.MaxTriggerChance + "% chance to Trigger a Lightning Spell on Hit with an Attack";
                public static string pt = Save_Manager.instance.data.Items.Mjolner.MinTriggerChance + " a " + Save_Manager.instance.data.Items.Mjolner.MaxTriggerChance + "% de chance para Ativar uma Magia de Raio ao Acertar um Ataque";
                public static string de = Save_Manager.instance.data.Items.Mjolner.MinTriggerChance + " bis " + Save_Manager.instance.data.Items.Mjolner.MaxTriggerChance + "% Chance bei Treffer einen Blitzzauber auszulösen yeah";
                public static string fr = Save_Manager.instance.data.Items.Mjolner.MinTriggerChance + " à " + Save_Manager.instance.data.Items.Mjolner.MaxTriggerChance + "% de chances de Déclenche un Sort de foudre au Toucher";
                //Add all languages here
            }
            public class Lore
            {
                public static readonly string en = "Look the storm in the eye and you will have its respect.";
                public static readonly string fr = "Entrez dans l'œil de la tempête et vous gagnerez son respect.";
                public static readonly string de = "Blickt dem Sturm ins Auge,\r\nund sein Respekt ist Euch gewiss.";
                public static readonly string pt = "Encare o olho da tempestade, e ela te respeitará.";
                //Add all languages here
            }

            [HarmonyPatch(typeof(Localization), "TryGetText")]
            public class Localization_TryGetText
            {
                [HarmonyPrefix]
                static bool Prefix(ref bool __result, string __0) //, Il2CppSystem.String __1)
                {
                    bool result = true;
                    if (/*(__0 == basic_subtype_name_key) ||*/ (__0 == unique_name_key) ||
                        (__0 == unique_description_key) || (__0 == unique_lore_key))
                    {
                        __result = true;
                        result = false;
                    }

                    return result;
                }
            }

            [HarmonyPatch(typeof(Localization), "GetText")]
            public class Localization_GetText
            {
                [HarmonyPrefix]
                static bool Prefix(ref string __result, string __0)
                {
                    bool result = true;
                    /*if (__0 == basic_subtype_name_key)
                    {
                        __result = Basic.Get_Subtype_Name();
                        result = false;
                    }
                    else */
                    if (__0 == unique_name_key)
                    {
                        __result = Unique.Get_Unique_Name();
                        result = false;
                    }
                    else if (__0 == unique_description_key)
                    {
                        string description = Unique.Get_Unique_Description();
                        if (description != "")
                        {
                            __result = description;
                            result = false;
                        }
                    }
                    else if (__0 == unique_lore_key)
                    {
                        string lore = Unique.Get_Unique_Lore();
                        if (lore != "")
                        {
                            __result = lore;
                            result = false;
                        }
                    }

                    return result;
                }
            }
        }
        public class Events
        {
            public static bool OnHitEvent_Initialized = false;
            public static void Init_OnHitEvent()
            {
                if (!Refs_Manager.player_actor.IsNullOrDestroyed())
                {
                    if (!Refs_Manager.player_actor.gameObject.IsNullOrDestroyed())
                    {
                        AbilityEventListener listener = Refs_Manager.player_actor.gameObject.GetComponent<AbilityEventListener>();
                        if (!listener.IsNullOrDestroyed())
                        {
                            listener.add_onHitEvent(OnHitAction);
                            OnHitEvent_Initialized = true;
                        }
                    }
                }
            }
            
            private static readonly System.Action<Ability, Actor> OnHitAction = new System.Action<Ability, Actor>(OnHit);
            private static void OnHit(Ability ability, Actor hitActor)
            {
                if (!Refs_Manager.player_actor.IsNullOrDestroyed())
                {
                    if (Refs_Manager.player_actor.itemContainersManager.hasUniqueEquipped(Unique.unique_id))
                    {
                        //ItemData item = Refs_Manager.player_actor.itemContainersManager.equipment.weapon.getItem();                                
                        float dice_roll = Random.Range(Save_Manager.instance.data.Items.Mjolner.MinTriggerChance, Save_Manager.instance.data.Items.Mjolner.MaxTriggerChance);
                        if (Save_Manager.instance.data.Items.Mjolner.ProcAnyLightningSpell)
                        {
                            if (!Refs_Manager.player_treedata.IsNullOrDestroyed())
                            {
                                foreach (LocalTreeData.SkillTreeData skill_tree_data in Refs_Manager.player_treedata.specialisedSkillTrees)
                                {
                                    if (!skill_tree_data.ability.IsNullOrDestroyed())
                                    {
                                        if (skill_tree_data.ability.tags.HasFlag(AT.Lightning))
                                        {
                                            skill_tree_data.ability.castAtTargetFromConstructorAfterDelay(Refs_Manager.player_actor.abilityObjectConstructor, Vector3.zero, hitActor.position(), 0, UseType.Indirect);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Do stuff with specific config'd spells...
                        }
                    }
                }
            }
        }
    }
}