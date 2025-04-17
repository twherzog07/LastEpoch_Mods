using HarmonyLib;
using Il2Cpp;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.Mods.Cosmetics
{
    public class Cosmetics_Panel
    {
        public static bool initialized = false;
        public static bool initializing = false;

        public static CosmeticPanelUI instance = null;
        public static bool isOpen = false;
        public static GameObject player_title = null;
        //public static GameObject player_title_dropdown = null;
        public static GameObject ep = null;
        public static GameObject get_points = null;
        public static GameObject open_shop = null;
        public static GameObject skill_cosmetics = null;
        public static GameObject equippables = null;
        public static GameObject eq_helmet = null;
        public static GameObject eq_chest = null;
        public static GameObject eq_mainhand = null;
        public static GameObject eq_backslot = null;
        public static GameObject eq_gloves = null;
        public static GameObject eq_boots = null;
        public static GameObject eq_offhand = null;
        public static GameObject eq_pet1 = null;
        public static GameObject eq_pet2 = null;
        public static GameObject eq_portal = null;
        public static GameObject eq_profil_image = null;
        public static GameObject eq_profil_frame = null;
        
        [HarmonyPatch(typeof(InventoryPanelUI), "OnEnable")]
        public class InventoryPanelUI_OnEnable
        {
            [HarmonyPostfix]
            static void Postfix(ref InventoryPanelUI __instance)
            {
                if ((!initialized) && (!initializing))
                {
                    Main.logger_instance.Msg("Intialize CosmeticPanelUI");
                    initializing = true;
                    instance = __instance.cosmeticPanel;
                    if (!instance.IsNullOrDestroyed())
                    {
                        //Refs
                        if (player_title.IsNullOrDestroyed()) { player_title = Functions.GetChild(instance.gameObject, "Player Titles"); }
                        //if ((player_title_dropdown.IsNullOrDestroyed()) && (!player_title.IsNullOrDestroyed())) { player_title_dropdown = Functions.GetChild(instance.gameObject, "Title Dropdown"); }
                        if (ep.IsNullOrDestroyed()) { ep = Functions.GetChild(instance.gameObject, "EP"); }
                        if (get_points.IsNullOrDestroyed()) { get_points = Functions.GetChild(instance.gameObject, "GetPoints"); }
                        if (open_shop.IsNullOrDestroyed()) { open_shop = Functions.GetChild(instance.gameObject, "OpenShop"); }
                        if (skill_cosmetics.IsNullOrDestroyed()) { skill_cosmetics = Functions.GetChild(instance.gameObject, "Skill Cosmetics"); }
                        if (equippables.IsNullOrDestroyed()) { equippables = Functions.GetChild(instance.gameObject, "Equippables"); }
                        if (!equippables.IsNullOrDestroyed())
                        {
                            if (eq_helmet.IsNullOrDestroyed()) { eq_helmet = Functions.GetChild(equippables, "Helmet"); }
                            if (eq_chest.IsNullOrDestroyed()) { eq_chest = Functions.GetChild(equippables, "Chest"); }
                            if (eq_mainhand.IsNullOrDestroyed()) { eq_mainhand = Functions.GetChild(equippables, "MainHand"); }
                            if (eq_backslot.IsNullOrDestroyed()) { eq_backslot = Functions.GetChild(equippables, "BackSlot"); }
                            if (eq_gloves.IsNullOrDestroyed()) { eq_gloves = Functions.GetChild(equippables, "Gloves"); }
                            if (eq_boots.IsNullOrDestroyed()) { eq_boots = Functions.GetChild(equippables, "Boots"); }
                            if (eq_offhand.IsNullOrDestroyed()) { eq_offhand = Functions.GetChild(equippables, "Offhand"); }
                            if (eq_pet1.IsNullOrDestroyed()) { eq_pet1 = Functions.GetChild(equippables, "Pet1"); }
                            if (eq_pet2.IsNullOrDestroyed()) { eq_pet2 = Functions.GetChild(equippables, "Pet2"); }
                            if (eq_portal.IsNullOrDestroyed()) { eq_portal = Functions.GetChild(equippables, "Portal"); }
                            if (eq_profil_image.IsNullOrDestroyed()) { eq_profil_image = Functions.GetChild(equippables, "ProfileImage"); }
                            if (eq_profil_frame.IsNullOrDestroyed()) { eq_profil_frame = Functions.GetChild(equippables, "ProfileFrame"); }
                        }

                        //Hide
                        if (!ep.IsNullOrDestroyed()) { ep.active = false; } //Hide EP
                        if (!get_points.IsNullOrDestroyed()) { get_points.active = false; } //Hide Get Points Btn
                        if (!open_shop.IsNullOrDestroyed()) { open_shop.active = false; } //Hide Open Shop Btn                    
                        if (!eq_backslot.IsNullOrDestroyed()) { eq_backslot.active = false; } //Hide BackSlot
                        if (!eq_pet1.IsNullOrDestroyed()) { eq_pet1.active = false; } //Hide Pet1
                        if (!eq_pet2.IsNullOrDestroyed()) { eq_pet2.active = false; } //Hide Pet2
                        if (!eq_portal.IsNullOrDestroyed()) { eq_portal.active = false; } //Hide Portal                    
                        if (!eq_profil_image.IsNullOrDestroyed()) { eq_profil_image.active = false; } //Hide Profil Image
                        if (!eq_profil_frame.IsNullOrDestroyed()) { eq_profil_frame.active = false; } //Hide Profil Frame                    
                        if (!skill_cosmetics.IsNullOrDestroyed()) { skill_cosmetics.active = false; } //Hide Skills

                        //Set Titles
                        /*if(!player_title_dropdown.IsNullOrDestroyed())
                        {
                            Il2CppTMPro.TMP_Dropdown dropdown = player_title_dropdown.GetComponent<Il2CppTMPro.TMP_Dropdown>();
                            if (dropdown.options.Count < 2)
                            {
                                foreach (string s in Cosmetics_Titles.Titles)
                                {
                                    Il2CppTMPro.TMP_Dropdown.OptionData optionData = new Il2CppTMPro.TMP_Dropdown.OptionData();
                                    optionData.text = "s";
                                    dropdown.options.Add(optionData);
                                }
                            }
                        }*/

                        //Set Slots btns
                        if (!eq_helmet.IsNullOrDestroyed()) { Actions.Set(eq_helmet, Actions.eq_helmet_click); }
                        if (!eq_chest.IsNullOrDestroyed()) { Actions.Set(eq_chest, Actions.eq_chest_click); }
                        if (!eq_mainhand.IsNullOrDestroyed()) { Actions.Set(eq_mainhand, Actions.eq_mainhand_click); }
                        if (!eq_gloves.IsNullOrDestroyed()) { Actions.Set(eq_gloves, Actions.eq_gloves_click); }
                        if (!eq_boots.IsNullOrDestroyed()) { Actions.Set(eq_boots, Actions.eq_boots_click); }
                        if (!eq_offhand.IsNullOrDestroyed()) { Actions.Set(eq_offhand, Actions.eq_offhand_click); }
                        initialized = true;
                    }
                    initializing = false;
                }
            }
        }

        public class Slots
        {
            public static void AddSkin(int slot, Sprite s)
            {
                if (slot == 0) { SetImage(eq_helmet, s); } //Helm
                else if (slot == 1) { SetImage(eq_chest, s); } //Chest
                else if (slot == 3) { SetImage(eq_boots, s); } //Boots
                else if (slot == 4) { SetImage(eq_gloves, s); } //Gloves
                else if (slot == 50) { SetImage(eq_mainhand, s); } //MainHand
                else if (slot == 99) { SetImage(eq_offhand, s); } //OffHand
            }
            public static void RemoveSkin(int slot)
            {
                AddSkin(slot, null);
            }
            public static void SetImage(GameObject g, Sprite s)
            {
                CosmeticItemSlot slot = g.GetComponent<CosmeticItemSlot>();
                if (!slot.IsNullOrDestroyed())
                {
                    if (s == null) { slot.paperDollImage.gameObject.active = true; }
                    else { slot.paperDollImage.gameObject.active = false; }
                    Vector3 position = slot.paperDollImage.gameObject.transform.position;
                    Vector3 size_delta = slot.paperDollImage.gameObject.GetComponent<RectTransform>().sizeDelta;

                    GameObject new_g = Functions.GetChild(g, "skin");
                    if (new_g.IsNullOrDestroyed())
                    {
                        GameObject skin_obj = new GameObject("skin");
                        skin_obj.AddComponent<Image>();
                        skin_obj.transform.SetParent(g.transform);
                        skin_obj.transform.position = position;
                        skin_obj.GetComponent<RectTransform>().sizeDelta = size_delta;
                        new_g = skin_obj;
                    }
                    if (!new_g.IsNullOrDestroyed())
                    {
                        new_g.GetComponent<Image>().sprite = s;
                        if (s == null) { new_g.active = false; }
                        else { new_g.active = true; }
                    }
                }
            }
        }
        public class Actions
        {
            public static void Set(GameObject g, System.Action a)
            {
                Button b = g.GetComponent<Button>();
                if (!b.IsNullOrDestroyed())
                {
                    b.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                    b.onClick.AddListener(a);
                }
            }

            /*
        public static void SetupShopBtn()
        {
            if (!Refs_Manager.InventoryPanelUI.IsNullOrDestroyed())
            {
                CosmeticPanelUI cosmetic_panel_ui = Refs_Manager.InventoryPanelUI.cosmeticPanel.GetComponent<CosmeticPanelUI>();
                cosmetic_panel_ui.openShopButton.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                cosmetic_panel_ui.openShopButton.onClick.AddListener(OpenMtxAction);
            }
        }
        private static readonly System.Action OpenMtxAction = new System.Action(Cosmetics_Mtx.OpenMtx);
        */
            public static readonly System.Action eq_helmet_click = new System.Action(OnClick_Helmet);
            public static void OnClick_Helmet()
            {
                Cosmetics_Flyout.Close();
                Cosmetics_Flyout.selected_slot = 0;
                Cosmetics_Flyout.Open();
            }
            public static readonly System.Action eq_chest_click = new System.Action(OnClick_Chest);
            public static void OnClick_Chest()
            {
                Cosmetics_Flyout.Close();
                Cosmetics_Flyout.selected_slot = 1;
                Cosmetics_Flyout.Open();
            }
            public static readonly System.Action eq_boots_click = new System.Action(OnClick_Boots);
            public static void OnClick_Boots()
            {
                Cosmetics_Flyout.Close();
                Cosmetics_Flyout.selected_slot = 3;
                Cosmetics_Flyout.Open();
            }
            public static readonly System.Action eq_gloves_click = new System.Action(OnClick_Gloves);
            public static void OnClick_Gloves()
            {
                Cosmetics_Flyout.Close();
                Cosmetics_Flyout.selected_slot = 4;
                Cosmetics_Flyout.Open();
            }
            public static readonly System.Action eq_mainhand_click = new System.Action(OnClick_MainHand);
            public static void OnClick_MainHand()
            {
                Cosmetics_Flyout.Close();
                Cosmetics_Flyout.selected_slot = 50;
                Cosmetics_Flyout.Open();
            }
            public static readonly System.Action eq_offhand_click = new System.Action(OnClick_OffHand);
            public static void OnClick_OffHand()
            {
                Cosmetics_Flyout.Close();
                Cosmetics_Flyout.selected_slot = 99;
                Cosmetics_Flyout.Open();
            }
        }
    }
}
