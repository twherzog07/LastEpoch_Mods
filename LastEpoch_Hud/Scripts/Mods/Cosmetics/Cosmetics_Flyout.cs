using HarmonyLib;
using Il2Cpp;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.Mods.Cosmetics
{
    public class Cosmetics_Flyout
    {
        public static GameObject flyout_content = null;
        public static bool isOpen = false;
        public static int selected_slot = -1;
        public static int selected_type = -1;
        public static int selected_subtype = -1;
        public static int selected_rarity = -1;
        public static int selected_unique = -1;
        private static readonly string skins_name = "skin_";
        private static readonly int[] mainhand_type = { 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 23, 24 };
        private static readonly int[] offhand_type = { 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 22, 23, 24 };

        public static void Open()
        {
            //Main.logger_instance.Msg("Cosmetics_Flyout.Open()");
            if (!Cosmetics_Panel.instance.IsNullOrDestroyed())
            {
                if (!Cosmetics_Panel.instance.flyoutWindow.IsNullOrDestroyed())
                {
                    GameObject go_title = Functions.GetChild(Cosmetics_Panel.instance.flyoutWindow, "Title");
                    if (!go_title.IsNullOrDestroyed())
                    {
                        string title = "";
                        if (selected_slot == 0) { title = "Helmet"; }
                        else if (selected_slot == 1) { title = "Chest"; }
                        else if (selected_slot == 3) { title = "Boots"; }
                        else if (selected_slot == 4) { title = "Gloves"; }
                        else if (selected_slot == 50) { title = "MainHand"; }
                        else if (selected_slot == 99) { title = "OffHand"; }
                        title += " APPEARANCE";
                        go_title.GetComponent<Il2CppTMPro.TextMeshProUGUI>().text = title;
                    }
                    Cosmetics_Panel.instance.flyoutWindow.active = true;
                }
            }
        }
        public static void Close()
        {
            //Main.logger_instance.Msg("Cosmetics_Flyout.Close()");
            if (!Cosmetics_Panel.instance.IsNullOrDestroyed())
            {
                if (!Cosmetics_Panel.instance.flyoutWindow.IsNullOrDestroyed())
                {
                    Cosmetics_Panel.instance.flyoutWindow.active = false;
                }              
            }
        }
        public static void ResetContent()
        {
            //Main.logger_instance.Msg("Cosmetics_Flyout.ResetContent()");
            if (!flyout_content.IsNullOrDestroyed())
            {
                foreach (GameObject g in Functions.GetAllChild(flyout_content))
                {
                    if (g.name.Contains(skins_name)) { Object.Destroy(g); }
                }
            }
        }
        public static void Clear_btn()
        {
            Cosmetics_Panel.Slots.RemoveSkin(selected_slot);
            Cosmetics_Visual.RemoveSkin();
            Close();
        }
        private static readonly System.Action ClearAction = new System.Action(Clear_btn);

        [HarmonyPatch(typeof(CosmeticsFlyoutSelectionContentNavigable), "OnEnable")]
        public class CosmeticsFlyoutSelectionContentNavigable_OnEnable
        {
            [HarmonyPostfix]
            static void Postfix(ref CosmeticsFlyoutSelectionContentNavigable __instance)
            {
                //Main.logger_instance.Msg("CosmeticsFlyoutSelectionContentNavigable.OnEnable()");
                isOpen = true;
                if ((!__instance.scrollContent.IsNullOrDestroyed()) && (flyout_content.IsNullOrDestroyed()))
                {
                    //Main.logger_instance.Msg("Set cosmetic Flyout Ref");
                    flyout_content = __instance.scrollContent.gameObject;
                }
                if (!flyout_content.IsNullOrDestroyed())
                {
                    //Clear btn
                    //Main.logger_instance.Msg("Set cleay button event");
                    GameObject cleat_btn_gameobject = Functions.GetChild(flyout_content, "Remove Button");
                    if (!cleat_btn_gameobject.IsNullOrDestroyed())
                    {
                        Button b = cleat_btn_gameobject.GetComponent<Button>();
                        if (!b.IsNullOrDestroyed())
                        {
                            b.onClick = new Button.ButtonClickedEvent();
                            b.onClick.AddListener(ClearAction);
                        }
                    }
                    //Add skins
                    //Main.logger_instance.Msg("Add Basic Skins");
                    foreach (ItemList.BaseEquipmentItem item in ItemList.instance.EquippableItems)
                    {
                        if ((item.baseTypeID == selected_slot) ||
                            ((selected_slot == 50) && (mainhand_type.Contains(item.baseTypeID))) ||
                            ((selected_slot == 99) && (offhand_type.Contains(item.baseTypeID))))
                        {
                            foreach (ItemList.EquipmentItem sub_item in item.subItems)
                            {
                                if ((sub_item.levelRequirement <= Refs_Manager.exp_tracker.CurrentLevel) &&
                                    (Functions.CheckClass(Refs_Manager.player_data.CharacterClass, sub_item.classRequirement)))
                                {
                                    GameObject g = new GameObject(skins_name + item.baseTypeID + "_" + sub_item.subTypeID + "_0_0");
                                    g.AddComponent<Button>();
                                    g.AddComponent<Image>();
                                    g.GetComponent<Image>().sprite = Functions.GetItemIcon(new ItemDataUnpacked
                                    {
                                        LvlReq = 0,
                                        classReq = ItemList.ClassRequirement.Any,
                                        itemType = (byte)item.baseTypeID,
                                        subType = (ushort)sub_item.subTypeID,
                                        rarity = 0,
                                        sockets = 0,
                                        uniqueID = 0
                                    });
                                    g.transform.SetParent(flyout_content.transform);
                                }
                            }
                        }
                    }
                    //Main.logger_instance.Msg("Add Unique Skins");
                    foreach (UniqueList.Entry unique in UniqueList.instance.uniques)
                    {
                        ItemList.EquipmentItem base_item = ItemList.instance.EquippableItems[unique.baseType].subItems[unique.subTypes[0]];
                        if ((base_item.levelRequirement <= Refs_Manager.exp_tracker.CurrentLevel) &&
                            (Functions.CheckClass(Refs_Manager.player_data.CharacterClass, base_item.classRequirement)))
                        {
                            if ((unique.baseType == selected_slot) ||
                                ((selected_slot == 50) && (mainhand_type.Contains(unique.baseType))) ||
                                ((selected_slot == 99) && (offhand_type.Contains(unique.baseType))))
                            {
                                int item_rarity = 7;
                                if (unique.isSetItem) { item_rarity = 8; }
                                GameObject g = new GameObject(skins_name + unique.baseType + "_" + unique.subTypes[0] + "_" + item_rarity + "_" + unique.uniqueID);
                                g.AddComponent<Button>();
                                g.AddComponent<Image>();                                
                                g.GetComponent<Image>().sprite = Functions.GetItemIcon(new ItemDataUnpacked
                                {
                                    LvlReq = 0,
                                    classReq = ItemList.ClassRequirement.Any,
                                    itemType = unique.baseType,
                                    subType = unique.subTypes[0],
                                    rarity = (byte)item_rarity,
                                    sockets = 0,
                                    uniqueID = unique.uniqueID
                                });
                                g.transform.SetParent(flyout_content.transform);
                            }
                        }
                    }
                }
                else { Main.logger_instance.Error("Flyout content is null"); }
            }
        }

        [HarmonyPatch(typeof(CosmeticsFlyoutSelectionContentNavigable), "OnDisable")]
        public class CosmeticsFlyoutSelectionContentNavigable_OnDisable
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                //Main.logger_instance.Msg("CosmeticsFlyoutSelectionContentNavigable.OnDisable()");
                isOpen = false;
                selected_slot = -1;
                selected_type = -1;
                selected_rarity = -1;
                selected_unique = -1;
                ResetContent();
                /*if (!flyout_content.IsNullOrDestroyed())
                {
                    foreach (GameObject g in Functions.GetAllChild(flyout_content))
                    {
                        if (g.name.Contains(skins_name)) { Object.Destroy(g); }
                    }
                }*/
            }
        }
        
        [HarmonyPatch(typeof(Button), "Press")]
        public class Button_Press
        {
            [HarmonyPostfix]
            static void Postfix(ref Button __instance)
            {
                if (isOpen)
                {
                    if (__instance.name.Contains(skins_name))
                    {
                        try
                        {
                            selected_type = System.Convert.ToInt32(__instance.name.Split('_')[1]);
                            selected_subtype = System.Convert.ToInt32(__instance.name.Split('_')[2]);
                            selected_rarity = System.Convert.ToInt32(__instance.name.Split('_')[3]);
                            selected_unique = System.Convert.ToInt32(__instance.name.Split('_')[4]);
                                                       
                            Cosmetics_Panel.Slots.AddSkin(selected_slot, Functions.GetItemIcon(new ItemDataUnpacked
                            {
                                LvlReq = 0,
                                classReq = ItemList.ClassRequirement.Any,
                                itemType = (byte)selected_type,
                                subType = (byte)selected_subtype,
                                rarity = (byte)selected_rarity,
                                sockets = 0,
                                uniqueID = (byte)selected_unique,
                            }));
                            Cosmetics_Visual.EquipSkin();
                            Close();
                        }
                        catch { }
                    }
                }
            }
        }
    }
}
