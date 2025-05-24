﻿using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Data;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts
{
    [RegisterTypeInIl2Cpp]
    public class Hud_Manager : MonoBehaviour
    {
        public Hud_Manager(System.IntPtr ptr) : base(ptr) { }
        public static Hud_Manager instance;

        public static AssetBundle asset_bundle;
        public static GameObject hud_object = null;
        public bool data_initialized = false;

        private string asset_path = Application.dataPath + "/../Mods/" + Main.mod_name + "/Assets";
        private static Canvas game_canvas = null;
        public static GameObject game_pause_menu = null;
        private static Canvas hud_canvas = null;
        private readonly string asset_bundle_name = "lastepochmods"; //Name of asset file
        private bool hud_initializing = false;
        private bool asset_bundle_initializing = false;
        private bool data_initializing = false;

        private bool updating = false;
        private bool exit = false;
        public static bool enable = false; //Used to wait loading (Fix_PlayerLoopHelper)

        void Awake()
        {
            instance = this;
            enable = true;
        }
        void Update()
        {
            Update_Hud_Scale();
            Update_Refs();
            Update_Locale();

            if (!hud_object.IsNullOrDestroyed())
            {
                if ((!data_initialized) && (!data_initializing)) { Init_UserData(); } //set once
                if ((IsPauseOpen()) && (!updating))
                {
                    updating = true;
                    Update_Hud_Content();
                    hud_object.active = true;
                    Content.Set_Active();
                    if ((!Refs_Manager.epoch_input_manager.IsNullOrDestroyed()) && (!Hud_Base.Btn_Resume.IsNullOrDestroyed()))
                    {
                        if (Input.GetKeyDown(KeyCode.Escape)) { exit = true; }
                        if ((Input.GetKeyUp(KeyCode.Escape)) && (exit)) { Hud_Base.Btn_Resume.onClick.Invoke(); exit = false; }
                        else { Refs_Manager.epoch_input_manager.forceDisableInput = true; }
                    }
                    updating = false;
                }
                else if (!updating)
                {
                    updating = true;
                    hud_object.active = false;
                    if (!Refs_Manager.epoch_input_manager.IsNullOrDestroyed()) { Refs_Manager.epoch_input_manager.forceDisableInput = false; }
                    Content.Character.need_update = true;
                    updating = false;
                }
            }            
        }

        void Init_AssetsBundle()
        {
            asset_bundle_initializing = true;
            if (Main.debug) { Main.logger_instance.Msg("Hud Manager : Initialize assets bundle"); }
            if ((Directory.Exists(asset_path)) && (File.Exists(Path.Combine(asset_path, asset_bundle_name))))
            {
                asset_bundle = AssetBundle.LoadFromFile(Path.Combine(asset_path, asset_bundle_name));
                Object.DontDestroyOnLoad(asset_bundle);
            }
            else { Main.logger_instance.Error("Hud Manager : " + asset_bundle_name + " Not Found in Assets directory"); }
            asset_bundle_initializing = false;
        }
        void Init_Hud()
        {
            hud_initializing = true;
            if (Main.debug) { Main.logger_instance.Msg("Hud Manager : Load hud object in assets"); }
            string asset_name = "";
            foreach (string name in asset_bundle.GetAllAssetNames())
            {
                if ((Functions.Check_Prefab(name)) && (name.Contains("/hud/")) && (name.Contains("hud.prefab")))
                {
                    if (Main.debug) { Main.logger_instance.Msg("Hud Manager : Dhud prefab found"); }
                    asset_name = name;
                    break;
                }
            }
            if (asset_name != "")
            {
                //UnityEngine.AddressableAssets

                if (Main.debug) { Main.logger_instance.Msg("Hud Manager : Register Hud_S"); }
                Il2CppInterop.Runtime.Injection.ClassInjector.RegisterTypeInIl2Cpp<LastEpoch_Hud.Unity.Hud_S>();
                //LastEpoch_Hud.Unity.Hud_S.guid

                if (Main.debug) { Main.logger_instance.Msg("Hud Manager : Load hud prefab obj"); }
                UnityEngine.Object obj = asset_bundle.LoadAsset(asset_name);
                //we need guid in order to load Hus_S

                //GameObject prefab_object = asset_bundle.LoadAsset<GameObject>(asset_name);
                if (Main.debug) { Main.logger_instance.Msg("Hud Manager : Convert to GameObject"); }
                GameObject prefab_object = obj.TryCast<GameObject>();

                //GameObject prefab_object = asset_bundle.LoadAsset(asset_name).TryCast<GameObject>();                
                if (!prefab_object.IsNullOrDestroyed())
                {
                    if (Main.debug) { Main.logger_instance.Msg("Hud Manager : Initialize hud prefab"); }
                    prefab_object.active = false; //Hide
                    //prefab_object.AddComponent<Hud_S>(); //try to load unity script
                    prefab_object.AddComponent<UIMouseListener>(); //Block Mouse
                    prefab_object.AddComponent<WindowFocusManager>();
                    
                    //Instantiate
                    if (Main.debug) { Main.logger_instance.Msg("Hud Manager : Instantiate hud prefab"); }
                    hud_object = Object.Instantiate(prefab_object, Vector3.zero, Quaternion.identity);
                    Object.DontDestroyOnLoad(hud_object);
                    Init_Hud_Refs();
                }
                else { Main.logger_instance.Error("Hud Manager : Hud Prefab not found"); }
            }

            //Shard prefab
            asset_name = "";
            foreach (string name in asset_bundle.GetAllAssetNames())
            {
                if ((Functions.Check_Prefab(name)) && (name.Contains("/hud/")) && (name.Contains("mod_shard.prefab")))
                {
                    asset_name = name;
                    break;
                }
            }
            if (asset_name != "")
            {
                GameObject shard_prefab = asset_bundle.LoadAsset(asset_name).TryCast<GameObject>();
                if (!shard_prefab.IsNullOrDestroyed())
                {
                    if (Main.debug) { Main.logger_instance.Msg("Hud Manager : Instantiate shard prefab"); }
                    Content.OdlForceDrop.shard_prefab = Object.Instantiate(shard_prefab, Vector3.zero, Quaternion.identity);
                    Object.DontDestroyOnLoad(Content.OdlForceDrop.shard_prefab);
                }
                else { Main.logger_instance.Error("Hud Manager : Shard Prefab not found"); }
            }
            else { Main.logger_instance.Error("Hud Manager : Shard Prefab name not found"); }


            hud_initializing = false;
        }
        void Init_Hud_Refs()
        {
            if (Main.debug) { Main.logger_instance.Msg("Hud Manager : Initialize hud refs"); }            
            Hud_Menu.Set_Events();

            Content.content_obj = Functions.GetChild(hud_object, "Content");
            Content.Character.Get_Refs();
            Content.Character.Set_Events();
            Content.Character.Set_Active(false);

            Content.Items.Get_Refs();
            Content.Items.Set_Events();
            Content.Items.Set_Active(false);

            Content.Scenes.Get_Refs();
            Content.Scenes.Set_Events();
            Content.Scenes.Set_Active(false);

            Content.Skills.Get_Refs();
            Content.Skills.Set_Active(false);

            Content.OdlForceDrop.Get_Refs();
            Content.OdlForceDrop.Set_Events();
            Content.OdlForceDrop.Set_Active(false);

            Content.Headhunter.Get_Refs();
            Content.Headhunter.Set_Active(false);
        }
        void Init_UserData()
        {
            data_initializing = true;
            if ((Save_Manager.instance.initialized) && (!Save_Manager.instance.data.IsNullOrDestroyed()))
            {
                if (Main.debug) { Main.logger_instance.Msg("Hud Manager : Initialize user config"); }
                bool character = Content.Character.Init_UserData();
                bool items = Content.Items.Init_UserData();
                bool scenes = Content.Scenes.Init_UserData();
                bool skills = Content.Skills.Init_UserData();
                bool headhunter = Content.Headhunter.Init_Data();
                if ((character) && (items) && (scenes) && (skills)) // && (headhunter))
                {
                    if (Main.debug) { Main.logger_instance.Msg("Hud Manager : Initialized"); }
                    data_initialized = true;
                }
            }
            data_initializing = false;
        }
        void Update_Refs()
        {
            if ((hud_canvas.IsNullOrDestroyed()) && (!hud_object.IsNullOrDestroyed())) { hud_canvas = hud_object.GetComponent<Canvas>(); }            
            if ((asset_bundle.IsNullOrDestroyed()) && (!asset_bundle_initializing)) { Init_AssetsBundle(); }
            if (!Refs_Manager.game_uibase.IsNullOrDestroyed())
            {
                if ((game_canvas.IsNullOrDestroyed()) && (Refs_Manager.game_uibase.canvases.Count > 0)) { game_canvas = Refs_Manager.game_uibase.canvases[0]; }
                if ((game_pause_menu.IsNullOrDestroyed()) || (Hud_Base.Default_PauseMenu_Btns.IsNullOrDestroyed())) { Hud_Base.Get_DefaultPauseMenu(); }
                if ((!Hud_Base.initiliazed_events) && (!game_pause_menu.IsNullOrDestroyed()) && (!Hud_Base.Default_PauseMenu_Btns.IsNullOrDestroyed())) { Hud_Base.Set_Events(); }
                if (Hud_Base.Get_DefaultPauseMenu_Open()) { Hud_Base.Toogle_DefaultPauseMenu(false); }
            }
            if ((!asset_bundle.IsNullOrDestroyed()) && (hud_object.IsNullOrDestroyed()) && (!hud_initializing)) { Init_Hud(); }
        }
        void Update_Hud_Scale()
        {
            if ((!Refs_Manager.game_uibase.IsNullOrDestroyed()) && (!game_canvas.IsNullOrDestroyed()) && (!hud_canvas.IsNullOrDestroyed()))
            {
                if (hud_canvas.scaleFactor != game_canvas.scaleFactor) { hud_canvas.scaleFactor = game_canvas.scaleFactor; }
            }
        }
        void Update_Locale()
        {
            if (Locales.update)
            {
                Locales.update = false;
                if (Locales.debug_text)
                {
                    Locales.debug_json = new System.Collections.Generic.List<string>();
                    Locales.debug_json.Add("{");
                }

                //need to make a function to remove all this trash
                foreach (GameObject level_0_go in Functions.GetAllChild(hud_object))
                {
                    foreach (GameObject level_1_go in Functions.GetAllChild(level_0_go))
                    {
                        ReplaceText(level_1_go);
                        foreach (GameObject level_2_go in Functions.GetAllChild(level_1_go))
                        {
                            ReplaceText(level_2_go);
                            foreach (GameObject level_3_go in Functions.GetAllChild(level_2_go))
                            {
                                ReplaceText(level_3_go);
                                foreach (GameObject level_4_go in Functions.GetAllChild(level_3_go))
                                {
                                    ReplaceText(level_4_go);
                                    foreach (GameObject level_5_go in Functions.GetAllChild(level_4_go))
                                    {
                                        ReplaceText(level_5_go);
                                        foreach (GameObject level_6_go in Functions.GetAllChild(level_5_go))
                                        {
                                            ReplaceText(level_6_go);
                                            foreach (GameObject level_7_go in Functions.GetAllChild(level_6_go))
                                            {
                                                ReplaceText(level_7_go);
                                                foreach (GameObject level_8_go in Functions.GetAllChild(level_7_go))
                                                {
                                                    ReplaceText(level_8_go);

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (Locales.debug_text)
                {
                    Locales.debug_json.Add("}");
                    string json = "";
                    foreach (string s in Locales.debug_json)
                    {
                        json += s;
                    }

                    Main.logger_instance.Msg("Copy to " + Locales.dictionnary_filename + Extensions.json);
                    Main.logger_instance.Msg(json);
                }
            }
        }
        void ReplaceText(GameObject go)
        {
            try
            {
                Text label = go.GetComponent<Text>();
                if (!label.IsNullOrDestroyed())
                {
                    bool ignore = false;
                    foreach (char c in label.text)
                    {
                        if (Locales.igrone_str.Contains(c)) { ignore = true; break; }
                    }
                    if (!ignore)
                    {
                        if (Locales.debug_text)
                        {
                            //string s = "\"" + label.text + "\": \"\", ";
                            string s = "\"" + label.text + "\": \"" + label.text + "\", "; //generate default en.json
                            if (!Locales.debug_json.Contains(s)) { Locales.debug_json.Add(s); }
                        }

                        if (Locales.current_dictionary != null)
                        {
                            if (Locales.current_dictionary.ContainsKey(label.text)) { label.text = Locales.current_dictionary[label.text]; }
                            //else { Main.logger_instance.Error(label.text + ", not found in dictionnary"); }
                        }
                    }
                }
            }
            catch
            {
                //Not a textbox
            }
        }
        void Update_Hud_Content()
        {
            if ((Content.Character.enable) && (Content.Character.need_update)) { Content.Character.Update_PlayerData(); }            
            if ((Content.Character.enable) && (Content.Character.controls_initialized)) { Content.Character.UpdateVisuals(); }
            if ((Content.Items.enable) && (Content.Items.controls_initialized))
            {
                Content.Items.UpdateVisuals();
                if (!Content.Items.ForceDrop.Type_Initialized) { Content.Items.ForceDrop.InitForcedrop(); }
            }            
            if ((Content.Scenes.enable) && (Content.Scenes.controls_initialized)) { Content.Scenes.UpdateVisuals(); }
            if ((Content.Skills.enable) && (Content.Skills.controls_initialized)) { Content.Skills.UpdateVisuals(); }
            if ((Content.OdlForceDrop.enable) && (Content.OdlForceDrop.initialized))
            {
                if (!Content.OdlForceDrop.Type_Initialized) { Content.OdlForceDrop.InitForcedrop(); }
                else
                {
                    Content.OdlForceDrop.implicits.active = Content.OdlForceDrop.implicits_enable;
                    Content.OdlForceDrop.implicits_border.active = Content.OdlForceDrop.implicits_enable;
                    if (!Content.OdlForceDrop.implicits_enable) { Content.OdlForceDrop.implicits_roll = false; }                    
                    Content.OdlForceDrop.implicit_0.active = Content.OdlForceDrop.implicits_roll;
                    Content.OdlForceDrop.implicit_1.active = Content.OdlForceDrop.implicits_roll;
                    Content.OdlForceDrop.implicit_2.active = Content.OdlForceDrop.implicits_roll;

                    Content.OdlForceDrop.forgin_potencial.active = Content.OdlForceDrop.forgin_potencial_enable;
                    Content.OdlForceDrop.forgin_potencial_border.active = Content.OdlForceDrop.forgin_potencial_enable;
                    if (!Content.OdlForceDrop.forgin_potencial_enable) { Content.OdlForceDrop.forgin_potencial_roll = false; }
                    Content.OdlForceDrop.forgin_potencial_value.active = Content.OdlForceDrop.forgin_potencial_roll;

                    Content.OdlForceDrop.seal.active = Content.OdlForceDrop.seal_enable;
                    Content.OdlForceDrop.seal_border.active = Content.OdlForceDrop.seal_enable;
                    if (!Content.OdlForceDrop.seal_enable) { Content.OdlForceDrop.seal_roll = false; }
                    Content.OdlForceDrop.seal_shard.active = Content.OdlForceDrop.seal_roll;
                    Content.OdlForceDrop.seal_tier.active = Content.OdlForceDrop.seal_roll;
                    Content.OdlForceDrop.seal_value.active = Content.OdlForceDrop.seal_roll;
                    if (Content.OdlForceDrop.seal_roll)
                    {
                        if (Content.OdlForceDrop.seal_id == -1) { Content.OdlForceDrop.seal_name = Content.OdlForceDrop.select_affix; }
                        if (!Content.OdlForceDrop.seal_select_text.IsNullOrDestroyed())
                        {
                            Content.OdlForceDrop.seal_select_text.text = Content.OdlForceDrop.seal_name;
                        }
                        else { Main.logger_instance.Error("seal_select_text NULLLLL"); }                            
                    }
                    else { Content.OdlForceDrop.seal_id = -1; }
                    
                    Content.OdlForceDrop.affixs.active = Content.OdlForceDrop.affixs_enable;
                    Content.OdlForceDrop.affixs_border.active = Content.OdlForceDrop.affixs_enable;
                    if (!Content.OdlForceDrop.affixs_enable) { Content.OdlForceDrop.affixs_roll = false; }
                    Content.OdlForceDrop.affixs_numbers.active = Content.OdlForceDrop.affixs_roll;
                    if ((Content.OdlForceDrop.affixs_numbers.active) && (!Content.OdlForceDrop.affixs_numbers_text.IsNullOrDestroyed()) && (!Content.OdlForceDrop.affixs_numbers_slider.IsNullOrDestroyed()))
                    {
                        Content.OdlForceDrop.affixs_numbers_text.text = System.Convert.ToInt32(Content.OdlForceDrop.affixs_numbers_slider.value).ToString();
                    }
                    if (Content.OdlForceDrop.affixs_roll)
                    {
                        if (Content.OdlForceDrop.affixs_numbers_slider.value > 0) { Content.OdlForceDrop.affix_0.active = true; }
                        else { Content.OdlForceDrop.affix_0.active = false; Content.OdlForceDrop.affix_0_id = -1; }
                        if (Content.OdlForceDrop.affixs_numbers_slider.value > 1) { Content.OdlForceDrop.affix_1.active = true; }
                        else { Content.OdlForceDrop.affix_1.active = false; Content.OdlForceDrop.affix_1_id = -1; }
                        if (Content.OdlForceDrop.affixs_numbers_slider.value > 2) { Content.OdlForceDrop.affix_2.active = true; }
                        else { Content.OdlForceDrop.affix_2.active = false; Content.OdlForceDrop.affix_2_id = -1; }
                        if (Content.OdlForceDrop.affixs_numbers_slider.value > 3) { Content.OdlForceDrop.affix_3.active = true; }
                        else { Content.OdlForceDrop.affix_3.active = false; Content.OdlForceDrop.affix_3_id = -1; }
                        if (Content.OdlForceDrop.affixs_numbers_slider.value > 4) { Content.OdlForceDrop.affix_4.active = true; }
                        else { Content.OdlForceDrop.affix_4.active = false; Content.OdlForceDrop.affix_4_id = -1; }
                        if (Content.OdlForceDrop.affixs_numbers_slider.value > 5) { Content.OdlForceDrop.affix_5.active = true; }
                        else { Content.OdlForceDrop.affix_5.active = false; Content.OdlForceDrop.affix_5_id = -1; }

                        if (Content.OdlForceDrop.affix_0_id == -1) { Content.OdlForceDrop.affix_0_name = Content.OdlForceDrop.select_affix; }
                        if (Content.OdlForceDrop.affix_1_id == -1) { Content.OdlForceDrop.affix_1_name = Content.OdlForceDrop.select_affix; }
                        if (Content.OdlForceDrop.affix_2_id == -1) { Content.OdlForceDrop.affix_2_name = Content.OdlForceDrop.select_affix; }
                        if (Content.OdlForceDrop.affix_3_id == -1) { Content.OdlForceDrop.affix_3_name = Content.OdlForceDrop.select_affix; }
                        if (Content.OdlForceDrop.affix_4_id == -1) { Content.OdlForceDrop.affix_4_name = Content.OdlForceDrop.select_affix; }
                        if (Content.OdlForceDrop.affix_5_id == -1) { Content.OdlForceDrop.affix_5_name = Content.OdlForceDrop.select_affix; }

                        if (!Content.OdlForceDrop.affix_0_select_text.IsNullOrDestroyed())
                        {
                            Content.OdlForceDrop.affix_0_select_text.text = Content.OdlForceDrop.affix_0_name;
                        }
                        else { Main.logger_instance.Error("affix_0_select_text NULLLLL"); }
                        if (!Content.OdlForceDrop.affix_1_select_text.IsNullOrDestroyed())
                        {
                            Content.OdlForceDrop.affix_1_select_text.text = Content.OdlForceDrop.affix_1_name;
                        }
                        else { Main.logger_instance.Error("affix_1_select_text NULLLLL"); }
                        if (!Content.OdlForceDrop.affix_2_select_text.IsNullOrDestroyed())
                        {
                            Content.OdlForceDrop.affix_2_select_text.text = Content.OdlForceDrop.affix_2_name;
                        }
                        else { Main.logger_instance.Error("affix_2_select_text NULLLLL"); }
                        if (!Content.OdlForceDrop.affix_3_select_text.IsNullOrDestroyed())
                        {
                            Content.OdlForceDrop.affix_3_select_text.text = Content.OdlForceDrop.affix_3_name;
                        }
                        else { Main.logger_instance.Error("affix_3_select_text NULLLLL"); }
                        if (!Content.OdlForceDrop.affix_4_select_text.IsNullOrDestroyed())
                        {
                            Content.OdlForceDrop.affix_4_select_text.text = Content.OdlForceDrop.affix_4_name;
                        }
                        else { Main.logger_instance.Error("affix_4_select_text NULLLLL"); }
                        if (!Content.OdlForceDrop.affix_5_select_text.IsNullOrDestroyed())
                        {
                            Content.OdlForceDrop.affix_5_select_text.text = Content.OdlForceDrop.affix_5_name;
                        }
                        else { Main.logger_instance.Error("affix_5_select_text NULLLLL"); }
                    }
                    else
                    {
                        Content.OdlForceDrop.affix_0.active = false;
                        Content.OdlForceDrop.affix_0_id = -1;
                        Content.OdlForceDrop.affix_1.active = false;
                        Content.OdlForceDrop.affix_1_id = -1;
                        Content.OdlForceDrop.affix_2.active = false;
                        Content.OdlForceDrop.affix_2_id = -1;
                        Content.OdlForceDrop.affix_3.active = false;
                        Content.OdlForceDrop.affix_3_id = -1;
                        Content.OdlForceDrop.affix_4.active = false;
                        Content.OdlForceDrop.affix_4_id = -1;
                        Content.OdlForceDrop.affix_5.active = false;
                        Content.OdlForceDrop.affix_5_id = -1;
                    }
                    
                    Content.OdlForceDrop.unique_mods.active = Content.OdlForceDrop.unique_mods_enable;
                    Content.OdlForceDrop.unique_mods_border.active = Content.OdlForceDrop.unique_mods_enable;
                    if (!Content.OdlForceDrop.unique_mods_enable) { Content.OdlForceDrop.unique_mods_roll = false; }
                    Content.OdlForceDrop.unique_mod_0.active = Content.OdlForceDrop.unique_mods_roll;
                    Content.OdlForceDrop.unique_mod_1.active = Content.OdlForceDrop.unique_mods_roll;
                    Content.OdlForceDrop.unique_mod_2.active = Content.OdlForceDrop.unique_mods_roll;
                    Content.OdlForceDrop.unique_mod_3.active = Content.OdlForceDrop.unique_mods_roll;
                    Content.OdlForceDrop.unique_mod_4.active = Content.OdlForceDrop.unique_mods_roll;
                    Content.OdlForceDrop.unique_mod_5.active = Content.OdlForceDrop.unique_mods_roll;
                    Content.OdlForceDrop.unique_mod_6.active = Content.OdlForceDrop.unique_mods_roll;
                    Content.OdlForceDrop.unique_mod_7.active = Content.OdlForceDrop.unique_mods_roll;

                    Content.OdlForceDrop.legenday_potencial.active = Content.OdlForceDrop.legenday_potencial_enable;
                    Content.OdlForceDrop.legenday_potencial_border.active = Content.OdlForceDrop.legenday_potencial_enable;
                    if (!Content.OdlForceDrop.legenday_potencial_enable) { Content.OdlForceDrop.legenday_potencial_roll = false; }
                    Content.OdlForceDrop.legenday_potencial_value.active = Content.OdlForceDrop.legenday_potencial_roll;

                    Content.OdlForceDrop.weaver_will.active = Content.OdlForceDrop.weaver_will_enable;
                    Content.OdlForceDrop.weaver_will_border.active = Content.OdlForceDrop.weaver_will_enable;
                    if (!Content.OdlForceDrop.weaver_will_enable) { Content.OdlForceDrop.weaver_will_roll = false; }
                    Content.OdlForceDrop.weaver_will_value.active = Content.OdlForceDrop.weaver_will_roll;

                    Content.OdlForceDrop.quantity.active = Content.OdlForceDrop.quantity_enable;
                    Content.OdlForceDrop.quantity_border.active = Content.OdlForceDrop.quantity_enable;
                    Content.OdlForceDrop.quantity_text.text = "";
                    if ((!Content.OdlForceDrop.quantity_text.IsNullOrDestroyed()) && (!Content.OdlForceDrop.forcedrop_quantity_slider.IsNullOrDestroyed()))
                    {
                        Content.OdlForceDrop.quantity_text.text = System.Convert.ToInt32(Content.OdlForceDrop.forcedrop_quantity_slider.value).ToString();
                    }
                    else { Main.logger_instance.Error("affix_5_select_text NULLLLL"); }
                }
            }
        }
        
        public static bool IsPauseOpen()
        {
            if (!game_pause_menu.IsNullOrDestroyed()) { return game_pause_menu.active; }
            else { return false; }
        }

        public class Hooks
        {
            //GUID
            /*[HarmonyPatch(typeof(UnityEngine.AddressableAssets.AssetReference), "get_AssetGUID")]
            public class UnityEngine_AddressableAssets_AssetReference
            {
                [HarmonyPrefix]
                static void Prefix(UnityEngine.AddressableAssets.AssetReference __instance, string __result)
                {
                    try
                    {
                        Main.logger_instance.Msg("AssetReference.get_AssetGUID(); Prefix : " + __instance?.ToString() ?? "null");
                    }
                    catch { }
                }
                [HarmonyPostfix]
                static void Postfix(UnityEngine.AddressableAssets.AssetReference __instance, string __result)
                {
                    try
                    {
                    Main.logger_instance.Msg("AssetReference.get_AssetGUID(); Postfix : " + __instance?.ToString() ?? "null" + ", Result = " + __result?.ToString() ?? "null");
                    }
                    catch { }
                }
            }*/

            //Select Shards
            [HarmonyPatch(typeof(Button), "Press")]
            public class Button_Press
            {
                [HarmonyPostfix]
                static void Postfix(ref Button __instance)
                {
                    if (Content.OdlForceDrop.enable)
                    {
                        if (__instance.name.Contains(Content.OdlForceDrop.shard_btn_name))
                        {
                            try
                            {
                                int i = System.Convert.ToInt32(__instance.name.Split('_')[1]);
                                //GameObject shard_id_object = Functions.GetChild(shard_btn_object, "shard_id");
                                //Text shard_id = shard_id_object.GetComponent<Text>();

                                GameObject shard_name_object = Functions.GetChild(__instance.gameObject, "shard_name");
                                Text shard_name = shard_name_object.GetComponent<Text>();                                
                                Content.OdlForceDrop.SelectShard(i, shard_name.text);
                            }
                            catch { }
                        }
                    }
                }
            }

            //All Hooks have to be replace by Unity Actions
            [HarmonyPatch(typeof(Toggle), "OnPointerClick")]
            public class Toggle_OnPointerClick
            {
                [HarmonyPostfix]
                static void Postfix(ref Toggle __instance, UnityEngine.EventSystems.PointerEventData __0)
                {
                    if ((!hud_object.IsNullOrDestroyed()) && (!Save_Manager.instance.IsNullOrDestroyed()) && (!Refs_Manager.player_data.IsNullOrDestroyed()))
                    {
                        if ((hud_object.active) && (!Save_Manager.instance.data.IsNullOrDestroyed()))
                        {
                            if (__instance.name.Contains("Toggle_Character_"))
                            {
                                switch (__instance.name)
                                {
                                    case "Toggle_Character_Data_Died": { if (!Refs_Manager.player_data.IsNullOrDestroyed()) { Refs_Manager.player_data.Died = Content.Character.Data.died_toggle.isOn; } break; }
                                    case "Toggle_Character_Data_Hardcore": { if (!Refs_Manager.player_data.IsNullOrDestroyed()) { Refs_Manager.player_data.Hardcore = Content.Character.Data.hardcore_toggle.isOn; } break; }
                                    case "Toggle_Character_Data_Masochist": { if (!Refs_Manager.player_data.IsNullOrDestroyed()) { Refs_Manager.player_data.Masochist = Content.Character.Data.masochist_toggle.isOn; } break; }
                                    case "Toggle_Character_Data_Portal": { if (!Refs_Manager.player_data.IsNullOrDestroyed()) { Refs_Manager.player_data.PortalUnlocked = Content.Character.Data.portal_toggle.isOn; } break; }
                                    case "Toggle_Character_Data_SoloChallenge": { if (!Refs_Manager.player_data.IsNullOrDestroyed()) { Refs_Manager.player_data.SoloChallenge = Content.Character.Data.solo_toggle.isOn; } break; }

                                    case "Toggle_Character_Buffs_Enable": { Save_Manager.instance.data.Character.PermanentBuffs.Enable_Mod = __instance.isOn; break; }
                                    case "Toggle_Character_Buffs_MoveSpeed": { Save_Manager.instance.data.Character.PermanentBuffs.Enable_MoveSpeed_Buff = __instance.isOn; break; }
                                    case "Toggle_Character_Buffs_Damage": { Save_Manager.instance.data.Character.PermanentBuffs.Enable_Damage_Buff = __instance.isOn; break; }
                                    case "Toggle_Character_Buffs_AttackSpeed": { Save_Manager.instance.data.Character.PermanentBuffs.Enable_AttackSpeed_Buff = __instance.isOn; break; }
                                    case "Toggle_Character_Buffs_CastingSpeed": { Save_Manager.instance.data.Character.PermanentBuffs.Enable_CastSpeed_Buff = __instance.isOn; break; }
                                    case "Toggle_Character_Buffs_CriticalChance": { Save_Manager.instance.data.Character.PermanentBuffs.Enable_CriticalChance_Buff = __instance.isOn; break; }
                                    case "Toggle_Character_Buffs_CriticalMultiplier": { Save_Manager.instance.data.Character.PermanentBuffs.Enable_CriticalMultiplier_Buff = __instance.isOn; break; }
                                    case "Toggle_Character_Buffs_HealthRegen": { Save_Manager.instance.data.Character.PermanentBuffs.Enable_HealthRegen_Buff = __instance.isOn; break; }
                                    case "Toggle_Character_Buffs_ManaRegen": { Save_Manager.instance.data.Character.PermanentBuffs.Enable_ManaRegen_Buff = __instance.isOn; break; }
                                    case "Toggle_Character_Buffs_Strenght": { Save_Manager.instance.data.Character.PermanentBuffs.Enable_Str_Buff = __instance.isOn; break; }
                                    case "Toggle_Character_Buffs_Intelligence": { Save_Manager.instance.data.Character.PermanentBuffs.Enable_Int_Buff = __instance.isOn; break; }
                                    case "Toggle_Character_Buffs_Dexterity": { Save_Manager.instance.data.Character.PermanentBuffs.Enable_Dex_Buff = __instance.isOn; break; }
                                    case "Toggle_Character_Buffs_Vitality": { Save_Manager.instance.data.Character.PermanentBuffs.Enable_Vit_Buff = __instance.isOn; break; }
                                    case "Toggle_Character_Buffs_Attunement": { Save_Manager.instance.data.Character.PermanentBuffs.Enable_Att_Buff = __instance.isOn; break; }
                                }
                            }
                            else if (__instance.name.Contains("Toggle_Items_"))
                            {
                                switch (__instance.name)
                                {
                                    case "Toggle_Items_Drop_ForceUnique":
                                        {
                                            if (__instance.isOn)
                                            {
                                                Save_Manager.instance.data.Items.Drop.Enable_ForceSet = false;
                                                Save_Manager.instance.data.Items.Drop.Enable_ForceLegendary = false;
                                                Content.Items.Drop.force_set_toggle.isOn = false;
                                                Content.Items.Drop.force_legendary_toggle.isOn = false;
                                            }
                                            Save_Manager.instance.data.Items.Drop.Enable_ForceUnique = __instance.isOn;
                                            break;
                                        }
                                    case "Toggle_Items_Drop_ForceSet":
                                        {
                                            if (__instance.isOn)
                                            {
                                                Save_Manager.instance.data.Items.Drop.Enable_ForceUnique = false;
                                                Save_Manager.instance.data.Items.Drop.Enable_ForceLegendary = false;
                                                Content.Items.Drop.force_unique_toggle.isOn = false;
                                                Content.Items.Drop.force_legendary_toggle.isOn = false;
                                            }
                                            Save_Manager.instance.data.Items.Drop.Enable_ForceSet = __instance.isOn;
                                            break;
                                        }
                                    case "Toggle_Items_Drop_ForceLegendary":
                                        {
                                            if (__instance.isOn)
                                            {
                                                Save_Manager.instance.data.Items.Drop.Enable_ForceUnique = false;
                                                Save_Manager.instance.data.Items.Drop.Enable_ForceSet = false;
                                                Content.Items.Drop.force_set_toggle.isOn = false;
                                                Content.Items.Drop.force_unique_toggle.isOn = false;
                                            }
                                            Save_Manager.instance.data.Items.Drop.Enable_ForceLegendary = __instance.isOn;
                                            break;
                                        }
                                    case "Toggle_Items_Drop_Implicits": { Save_Manager.instance.data.Items.Drop.Enable_Implicits = __instance.isOn; break; }
                                    case "Toggle_Items_Drop_ForginPotencial": { Save_Manager.instance.data.Items.Drop.Enable_ForginPotencial = __instance.isOn; break; }
                                    case "Toggle_Items_Drop_ForceSeal": { Save_Manager.instance.data.Items.Drop.Enable_ForceSeal = __instance.isOn; break; }
                                    case "Toggle_Items_Drop_SealTier": { Save_Manager.instance.data.Items.Drop.Enable_SealTier = __instance.isOn; break; }
                                    case "Toggle_Items_Drop_SealValue": { Save_Manager.instance.data.Items.Drop.Enable_SealValue = __instance.isOn; break; }
                                    case "Toggle_Items_Drop_NbAffixes": { Save_Manager.instance.data.Items.Drop.Enable_AffixCount = __instance.isOn; break; }                                    
                                    case "Toggle_Items_Drop_AffixesTiers": { Save_Manager.instance.data.Items.Drop.Enable_AffixTiers = __instance.isOn; break; }
                                    case "Toggle_Items_Drop_AffixesValues": { Save_Manager.instance.data.Items.Drop.Enable_AffixValues = __instance.isOn; break; }
                                    case "Toggle_Items_Drop_UniqueMods": { Save_Manager.instance.data.Items.Drop.Enable_UniqueMods = __instance.isOn; break; }
                                    case "Toggle_Items_Drop_LegendaryPotencial": { Save_Manager.instance.data.Items.Drop.Enable_LegendaryPotencial = __instance.isOn; break; }
                                    case "Toggle_Items_Drop_WeaverWill": { Save_Manager.instance.data.Items.Drop.Enable_WeaverWill = __instance.isOn; break; }

                                    case "Toggle_Items_Pickup_AutoPickup_Gold": { Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_Gold = __instance.isOn; break; }
                                    case "Toggle_Items_Pickup_AutoPickup_Keys": { Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_Keys = __instance.isOn; break; }
                                    case "Toggle_Items_Pickup_AutoPickup_Pots": { Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_Potions = __instance.isOn; break; }
                                    case "Toggle_Items_Pickup_AutoPickup_XpTome": { Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_XpTome = __instance.isOn; break; }
                                    case "Toggle_Items_Pickup_AutoPickup_Materials": { Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_Materials = __instance.isOn; break; }
                                    case "Toggle_Items_Pickup_AutoPickup_Filters": { Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_FromFilter = __instance.isOn; break; }
                                    case "Toggle_Items_Pickup_AutoStore_OnDrop": { Save_Manager.instance.data.Items.Pickup.Enable_AutoStore_OnDrop = __instance.isOn; break; }
                                    case "Toggle_Items_Pickup_AutoStore_OnInventoryOpen": { Save_Manager.instance.data.Items.Pickup.Enable_AutoStore_OnInventoryOpen = __instance.isOn; break; }
                                    case "Toggle_Items_Pickup_AutoStore_10sec": { Save_Manager.instance.data.Items.Pickup.Enable_AutoStore_All10Sec = __instance.isOn; break; }
                                    case "Toggle_Items_Pickup_AutoSell_All_Hide": { Save_Manager.instance.data.Items.Pickup.Enable_AutoSell_Hide = __instance.isOn; break; }
                                    
                                    case "Toggle_Items_Pickup_Range_Pickup": { Save_Manager.instance.data.Items.Pickup.Enable_RangePickup = __instance.isOn; break; }
                                    case "Toggle_Items_Pickup_Hide_Notifications": { Save_Manager.instance.data.Items.Pickup.Enable_HideMaterialsNotifications = __instance.isOn; break; }

                                    case "Toggle_Items_Craft_Enable": { Save_Manager.instance.data.Items.CraftingSlot.Enable_Mod = __instance.isOn; break; }
                                    case "Toggle_Items_Craft_ForginPotencial": { Save_Manager.instance.data.Items.CraftingSlot.Enable_ForginPotencial = __instance.isOn; break; }
                                    case "Toggle_Items_Craft_Implicit0": { Save_Manager.instance.data.Items.CraftingSlot.Enable_Implicit_0 = __instance.isOn; break; }
                                    case "Toggle_Items_Craft_Implicit1": { Save_Manager.instance.data.Items.CraftingSlot.Enable_Implicit_1 = __instance.isOn; break; }
                                    case "Toggle_Items_Craft_Implicit2": { Save_Manager.instance.data.Items.CraftingSlot.Enable_Implicit_2 = __instance.isOn; break; }

                                    case "Toggle_Items_Craft_SealTier": { Save_Manager.instance.data.Items.CraftingSlot.Enable_Seal_Tier = __instance.isOn; break; }
                                    case "Toggle_Items_Craft_SealValue": { Save_Manager.instance.data.Items.CraftingSlot.Enable_Seal_Value = __instance.isOn; break; }

                                    case "Toggle_Items_Craft_AffixTier0": { Save_Manager.instance.data.Items.CraftingSlot.Enable_Affix_0_Tier = __instance.isOn; break; }
                                    case "Toggle_Items_Craft_AffixTier1": { Save_Manager.instance.data.Items.CraftingSlot.Enable_Affix_1_Tier = __instance.isOn; break; }
                                    case "Toggle_Items_Craft_AffixTier2": { Save_Manager.instance.data.Items.CraftingSlot.Enable_Affix_2_Tier = __instance.isOn; break; }
                                    case "Toggle_Items_Craft_AffixTier3": { Save_Manager.instance.data.Items.CraftingSlot.Enable_Affix_3_Tier = __instance.isOn; break; }

                                    case "Toggle_Items_Craft_AffixValue0": { Save_Manager.instance.data.Items.CraftingSlot.Enable_Affix_0_Value = __instance.isOn; break; }
                                    case "Toggle_Items_Craft_AffixValue1": { Save_Manager.instance.data.Items.CraftingSlot.Enable_Affix_1_Value = __instance.isOn; break; }
                                    case "Toggle_Items_Craft_AffixValue2": { Save_Manager.instance.data.Items.CraftingSlot.Enable_Affix_2_Value = __instance.isOn; break; }
                                    case "Toggle_Items_Craft_AffixValue3": { Save_Manager.instance.data.Items.CraftingSlot.Enable_Affix_3_Value = __instance.isOn; break; }

                                    case "Toggle_Items_Craft_UniqueMod0": { Save_Manager.instance.data.Items.CraftingSlot.Enable_UniqueMod_0 = __instance.isOn; break; }
                                    case "Toggle_Items_Craft_UniqueMod1": { Save_Manager.instance.data.Items.CraftingSlot.Enable_UniqueMod_1 = __instance.isOn; break; }
                                    case "Toggle_Items_Craft_UniqueMod2": { Save_Manager.instance.data.Items.CraftingSlot.Enable_UniqueMod_2 = __instance.isOn; break; }
                                    case "Toggle_Items_Craft_UniqueMod3": { Save_Manager.instance.data.Items.CraftingSlot.Enable_UniqueMod_3 = __instance.isOn; break; }
                                    case "Toggle_Items_Craft_UniqueMod4": { Save_Manager.instance.data.Items.CraftingSlot.Enable_UniqueMod_4 = __instance.isOn; break; }
                                    case "Toggle_Items_Craft_UniqueMod5": { Save_Manager.instance.data.Items.CraftingSlot.Enable_UniqueMod_5 = __instance.isOn; break; }
                                    case "Toggle_Items_Craft_UniqueMod6": { Save_Manager.instance.data.Items.CraftingSlot.Enable_UniqueMod_6 = __instance.isOn; break; }
                                    case "Toggle_Items_Craft_UniqueMod7": { Save_Manager.instance.data.Items.CraftingSlot.Enable_UniqueMod_7 = __instance.isOn; break; }

                                    case "Toggle_Items_Craft_LegendaryPotencial": { Save_Manager.instance.data.Items.CraftingSlot.Enable_LegendaryPotencial = __instance.isOn; break; }
                                    case "Toggle_Items_Craft_WeaverWill": { Save_Manager.instance.data.Items.CraftingSlot.Enable_WeaverWill = __instance.isOn; break; }
                                }
                            }
                            else if (__instance.name.Contains("Toggle_Scenes_"))
                            {
                                switch (__instance.name)
                                {
                                    case "Toggle_Scenes_Camera_Enable": { Save_Manager.instance.data.Scenes.Camera.Enable_Mod = __instance.isOn; break; }
                                    case "Toggle_Scenes_Camera_ZoomMinimum": { Save_Manager.instance.data.Scenes.Camera.Enable_ZoomMinimum = __instance.isOn; break; }
                                    case "Toggle_Scenes_Camera_ZoomPerScroll": { Save_Manager.instance.data.Scenes.Camera.Enable_ZoomPerScroll = __instance.isOn; break; }
                                    case "Toggle_Scenes_Camera_ZoomSpeed": { Save_Manager.instance.data.Scenes.Camera.Enable_ZoomSpeed = __instance.isOn; break; }
                                    case "Toggle_Scenes_Camera_DefaultRotation": { Save_Manager.instance.data.Scenes.Camera.Enable_DefaultRotation = __instance.isOn; break; }
                                    case "Toggle_Scenes_Camera_OffsetMinimum": { Save_Manager.instance.data.Scenes.Camera.Enable_OffsetMinimum = __instance.isOn; break; }
                                    case "Toggle_Scenes_Camera_OffsetMaximum": { Save_Manager.instance.data.Scenes.Camera.Enable_OffsetMaximum = __instance.isOn; break; }
                                    case "Toggle_Scenes_Camera_AngleMinimum": { Save_Manager.instance.data.Scenes.Camera.Enable_AngleMinimum = __instance.isOn; break; }
                                    case "Toggle_Scenes_Camera_AngleMaximum": { Save_Manager.instance.data.Scenes.Camera.Enable_AngleMaximum = __instance.isOn; break; }
                                    case "Toggle_Scenes_Camera_LoadOnStart": { Save_Manager.instance.data.Scenes.Camera.Enable_LoadOnStart = __instance.isOn; break; }

                                    case "Toggle_Scenes_Dungeons_EnterWithoutKey": { Save_Manager.instance.data.Scenes.Dungeons.Enable_EnterWithoutKey = __instance.isOn; break; }

                                    case "Toggle_Scenes_Minimap_MaxZoomOut": { Save_Manager.instance.data.Scenes.Minimap.Enable_MaxZoomOut = __instance.isOn; break; }
                                    case "Toggle_Scenes_Minimap_RemoveFogOfWar": { Save_Manager.instance.data.Scenes.Minimap.Enable_RemoveFogOfWar = __instance.isOn; break; }

                                    case "Toggle_Scenes_Monoliths_MaxStability": { Save_Manager.instance.data.Scenes.Monoliths.Enable_MaxStability = __instance.isOn; break; }
                                    case "Toggle_Scenes_Monoliths_MobsDensity": { Save_Manager.instance.data.Scenes.Monoliths.Enable_MobsDensity = __instance.isOn; break; }
                                    case "Toggle_Scenes_Monoliths_MobsDefeatOnStart": { Save_Manager.instance.data.Scenes.Monoliths.Enable_MobsDefeatOnStart = __instance.isOn; break; }
                                    case "Toggle_Scenes_Monoliths_BlessingSlots": { Save_Manager.instance.data.Scenes.Monoliths.Enable_BlessingSlots = __instance.isOn; break; }
                                    case "Toggle_Scenes_Monoliths_MaxStabilityOnStart": { Save_Manager.instance.data.Scenes.Monoliths.Enable_MaxStabilityOnStart = __instance.isOn; break; }
                                    case "Toggle_Scenes_Monoliths_MaxStabilityOnStabilityChanged": { Save_Manager.instance.data.Scenes.Monoliths.Enable_MaxStabilityOnStabilityChanged = __instance.isOn; break; }
                                    case "Toggle_Scenes_Monoliths_ObjectiveReveal": { Save_Manager.instance.data.Scenes.Monoliths.Enable_ObjectiveReveal = __instance.isOn; break; }
                                    case "Toggle_Scenes_Monoliths_CompleteObjective": { Save_Manager.instance.data.Scenes.Monoliths.Enable_CompleteObjective = __instance.isOn; break; }
                                    case "Toggle_Scenes_Monoliths_NoLostWhenDie": { Save_Manager.instance.data.Scenes.Monoliths.Enable_NoLostWhenDie = __instance.isOn; break; }
                                }
                            }
                            else
                            {
                                switch (__instance.name)
                                {
                                    //Login
                                    case "Toggle_Login_AutoClickPlayOffline": { Save_Manager.instance.data.Login.Enable_AutoLoginOffline = __instance.isOn; break; }
                                    case "Toggle_Login_AutoSelectChar": { Save_Manager.instance.data.Login.Enable_AutoSelectChar = __instance.isOn; break; }
                                    //Skills
                                    case "Toggle_RemoveManaCost": { Save_Manager.instance.data.Skills.Enable_RemoveManaCost = __instance.isOn; break; }
                                    case "Toggle_RemoveChannelCost": { Save_Manager.instance.data.Skills.Enable_RemoveChannelCost = __instance.isOn; break; }
                                    case "Toggle_ManaRegenWhenChanneling": { Save_Manager.instance.data.Skills.Enable_NoManaRegenWhileChanneling = __instance.isOn; break; }
                                    case "Toggle_DontStopWhenOOM": { Save_Manager.instance.data.Skills.Enable_StopWhenOutOfMana = __instance.isOn; break; }
                                    case "Toggle_NoCooldown": { Save_Manager.instance.data.Skills.Enable_RemoveCooldown = __instance.isOn; break; }
                                    case "Toggle_UnlockAllSkills": { Save_Manager.instance.data.Skills.Enable_AllSkills = __instance.isOn; break; }
                                    case "Toggle_RemoveNodeRequirements": { Save_Manager.instance.data.Skills.Disable_NodeRequirement = __instance.isOn; break; }
                                    case "Toggle_SpecializationSlots": { Save_Manager.instance.data.Skills.Enable_SpecializationSlots = __instance.isOn; break; }
                                    case "Toggle_SkillLevel": { Save_Manager.instance.data.Skills.Enable_SkillLevel = __instance.isOn; break; }
                                    case "Toggle_PassivePoints": { Save_Manager.instance.data.Skills.Enable_PassivePoints = __instance.isOn; break; }
                                    case "Toggle_NoTarget": { Save_Manager.instance.data.Skills.MovementSkills.Enable_NoTarget = __instance.isOn; break; }
                                    case "Toggle_ImmuneDuringMovement": { Save_Manager.instance.data.Skills.MovementSkills.Enable_ImmuneDuringMovement = __instance.isOn; break; }
                                    case "Toggle_DisableSimplePath": { Save_Manager.instance.data.Skills.MovementSkills.Disable_SimplePath = __instance.isOn; break; }
                                    //Companions
                                    case "Toggle_MaximumCompanions": { Save_Manager.instance.data.Skills.Companion.Enable_Limit = __instance.isOn; break; }
                                    case "Toggle_Wolf_SummonToMax": { Save_Manager.instance.data.Skills.Companion.Wolf.Enable_SummonMax = __instance.isOn; break; }
                                    case "Toggle_Wolf_SummonLimit": { Save_Manager.instance.data.Skills.Companion.Wolf.Enable_SummonLimit = __instance.isOn; break; }
                                    case "Toggle_Wolf_StunImmunity": { Save_Manager.instance.data.Skills.Companion.Wolf.Enable_StunImmunity = __instance.isOn; break; }
                                    case "Toggle_Scorpions_SummonLimit": { Save_Manager.instance.data.Skills.Companion.Scorpion.Enable_BabyQuantity = __instance.isOn; break; }
                                    //Minions
                                    case "Toggle_Skeleteon_SummonQuantityFromPassive": { Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_additionalSkeletonsFromPassives = __instance.isOn; break; }
                                    case "Toggle_Skeleteon_SummonQuantityFromSkillTree": { Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_additionalSkeletonsFromSkillTree = __instance.isOn; break; }
                                    case "Toggle_Skeleteon_SummonQuantityPerCast": { Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_additionalSkeletonsPerCast = __instance.isOn; break; }
                                    case "Toggle_Skeleteon_ChanceToResummonOnDeath": { Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_chanceToResummonOnDeath = __instance.isOn; break; }
                                    case "Toggle_Skeleton_ForceArcher": { Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_forceArcher = __instance.isOn; break; }
                                    case "Toggle_Skeleton_ForceBrawler": { Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_forceBrawler = __instance.isOn; break; }
                                    case "Toggle_Skeleton_ForceWarrior": { Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_forceWarrior = __instance.isOn; break; }

                                    case "Toggle_Wraiths_SummonMax": { Save_Manager.instance.data.Skills.Minions.Wraiths.Enable_additionalMaxWraiths = __instance.isOn; break; }
                                    case "Toggle_Wraiths_Delayed": { Save_Manager.instance.data.Skills.Minions.Wraiths.Enable_delayedWraiths = __instance.isOn; break; }
                                    case "Toggle_Wraiths_CastSpeed": { Save_Manager.instance.data.Skills.Minions.Wraiths.Enable_increasedCastSpeed = __instance.isOn; break; }
                                    case "Toggle_Wraiths_DisableLimitTo2": { Save_Manager.instance.data.Skills.Minions.Wraiths.Enable_limitedTo2Wraiths = __instance.isOn; break; }
                                    case "Toggle_Wraiths_DisableDecay": { Save_Manager.instance.data.Skills.Minions.Wraiths.Enable_wraithsDoNotDecay = __instance.isOn; break; }

                                    case "Toggle_Mages_SummonQuantityFromPassive": { Save_Manager.instance.data.Skills.Minions.Mages.Enable_additionalSkeletonsFromPassives = __instance.isOn; break; }
                                    case "Toggle_Mages_SummonQuantityFromSkillTree": { Save_Manager.instance.data.Skills.Minions.Mages.Enable_additionalSkeletonsFromSkillTree = __instance.isOn; break; }
                                    case "Toggle_Mages_SummonQuantityFromItems": { Save_Manager.instance.data.Skills.Minions.Mages.Enable_additionalSkeletonsFromItems = __instance.isOn; break; }
                                    case "Toggle_Mages_SummonPerCast": { Save_Manager.instance.data.Skills.Minions.Mages.Enable_additionalSkeletonsPerCast = __instance.isOn; break; }
                                    case "Toggle_Mages_ChanceForExtraPorjectiles": { Save_Manager.instance.data.Skills.Minions.Mages.Enable_chanceForTwoExtraProjectiles = __instance.isOn; break; }
                                    case "Toggle_Mages_ForceCryomancer": { Save_Manager.instance.data.Skills.Minions.Mages.Enable_forceCryomancer = __instance.isOn; break; }
                                    case "Toggle_Mages_ForceDeathKnight": { Save_Manager.instance.data.Skills.Minions.Mages.Enable_forceDeathKnight = __instance.isOn; break; }
                                    case "Toggle_Mages_ForcePyromancer": { Save_Manager.instance.data.Skills.Minions.Mages.Enable_forcePyromancer = __instance.isOn; break; }

                                    case "Toggle_BoneGolem_GolemPerSkeletons": { Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_addedGolemsPer4Skeletons = __instance.isOn; break; }
                                    case "Toggle_BoneGolem_SelfResurectChance": { Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_selfResurrectChance = __instance.isOn; break; }
                                    case "Toggle_BoneGolem_IncreaseFireAura": { Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_increasedFireAuraArea = __instance.isOn; break; }
                                    case "Toggle_BoneGolem_IncreaseArmorAura": { Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_undeadArmorAura = __instance.isOn; break; }
                                    case "Toggle_BoneGolem_IncreaseMoveSpeedAura": { Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_undeadMovespeedAura = __instance.isOn; break; }
                                    case "Toggle_BoneGolem_IncreaseMoveSpeed": { Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_increasedMoveSpeed = __instance.isOn; break; }
                                    case "Toggle_BoneGolem_Twins": { Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_twins = __instance.isOn; break; }
                                    case "Toggle_BoneGolem_Slam": { Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_hasSlamAttack = __instance.isOn; break; }

                                    case "Toggle_VolatileZombies_ChanceOnMinionDeath": { Save_Manager.instance.data.Skills.Minions.VolatileZombies.Enable_chanceToCastFromMinionDeath = __instance.isOn; break; }
                                    case "Toggle_VolatileZombies_InfernalShadeChance": { Save_Manager.instance.data.Skills.Minions.VolatileZombies.Enable_chanceToCastInfernalShadeOnDeath = __instance.isOn; break; }
                                    case "Toggle_VolatileZombies_MarrowShardsChance": { Save_Manager.instance.data.Skills.Minions.VolatileZombies.Enable_chanceToCastMarrowShardsOnDeath = __instance.isOn; break; }

                                    case "Toggle_DreadShades_Duration": { Save_Manager.instance.data.Skills.Minions.DreadShades.Enable_Duration = __instance.isOn; break; }
                                    case "Toggle_DreadShades_Max": { Save_Manager.instance.data.Skills.Minions.DreadShades.Enable_Max = __instance.isOn; break; }
                                    case "Toggle_DreadShades_Decay": { Save_Manager.instance.data.Skills.Minions.DreadShades.Enable_ReduceDecay = __instance.isOn; break; }
                                    case "Toggle_DreadShades_Radius": { Save_Manager.instance.data.Skills.Minions.DreadShades.Enable_Radius = __instance.isOn; break; }
                                    case "Toggle_DreadShades_DisableLimit": { Save_Manager.instance.data.Skills.Minions.DreadShades.Enable_DisableLimit = __instance.isOn; break; }
                                    case "Toggle_DreadShades_DisableHealthDrain": { Save_Manager.instance.data.Skills.Minions.DreadShades.Enable_DisableHealthDrain = __instance.isOn; break; }
                                }
                            }                            
                        }
                    }                    
                }
            }

            [HarmonyPatch(typeof(Slider), "set_value")]
            public class Slider_set_value
            {
                [HarmonyPostfix]
                static void Postfix(ref Slider __instance, float __0)
                {
                    if ((!hud_object.IsNullOrDestroyed()) && (!Save_Manager.instance.IsNullOrDestroyed()))
                    {
                        if ((hud_object.active) && (!Save_Manager.instance.data.IsNullOrDestroyed()) && (!Refs_Manager.player_data.IsNullOrDestroyed()))
                        {
                            if (__instance.name.Contains("Slider_Character_"))
                            {
                                switch (__instance.name)
                                {
                                    case "Slider_Character_Cheats_AutoPotions":
                                        {
                                            Save_Manager.instance.data.Character.Cheats.autoPot = __0;
                                            //Content.Character.Cheats.autopot_text.text = (int)((Save_Manager.instance.data.Character.Cheats.autoPot / 255) * 100) + " %";
                                            break;
                                        }
                                    case "Slider_Character_Cheats_DensityMultiplier":
                                        {
                                            Save_Manager.instance.data.Character.Cheats.DensityMultiplier = __0;
                                            //Content.Character.Cheats.density_text.text = "x " + (int)(Save_Manager.instance.data.Character.Cheats.DensityMultiplier);
                                            break;
                                        }
                                    case "Slider_Character_Cheats_ExperienceMultiplier":
                                        {
                                            Save_Manager.instance.data.Character.Cheats.ExperienceMultiplier = __0;
                                            //Content.Character.Cheats.experience_text.text = "x " + (int)(Save_Manager.instance.data.Character.Cheats.ExperienceMultiplier);
                                            break;
                                        }
                                    case "Slider_Character_Cheats_AbilityMultiplier":
                                        {
                                            Save_Manager.instance.data.Character.Cheats.AbilityMultiplier = __0;
                                            //Content.Character.Cheats.ability_text.text = "x " + (int)(Save_Manager.instance.data.Character.Cheats.AbilityMultiplier);
                                            break;
                                        }
                                    case "Slider_Character_Cheats_FavorMultiplier":
                                        {
                                            Save_Manager.instance.data.Character.Cheats.FavorMultiplier = __0;
                                            //Content.Character.Cheats.favor_text.text = "x " + (int)(Save_Manager.instance.data.Character.Cheats.FavorMultiplier);
                                            break;
                                        }
                                    case "Slider_Character_Cheats_ItemDropMultiplier":
                                        {
                                            Save_Manager.instance.data.Character.Cheats.ItemDropMultiplier = __0;
                                            //Content.Character.Cheats.itemdropmultiplier_text.text = "x " + (int)(Save_Manager.instance.data.Character.Cheats.ItemDropMultiplier);
                                            break;
                                        }
                                    case "Slider_Character_Cheats_ItemDropChance":
                                        {
                                            Save_Manager.instance.data.Character.Cheats.ItemDropChance = __0;
                                            //Content.Character.Cheats.itemdropchance_text.text = "+ " + (int)((Save_Manager.instance.data.Character.Cheats.ItemDropChance / 255) * 100) + " %";
                                            break;
                                        }
                                    case "Slider_Character_Cheats_GoldDropMultiplier":
                                        {
                                            Save_Manager.instance.data.Character.Cheats.GoldDropMultiplier = __0;
                                            //Content.Character.Cheats.golddropmultiplier_text.text = "x " + (int)(Save_Manager.instance.data.Character.Cheats.GoldDropMultiplier);
                                            break;
                                        }
                                    case "Slider_Character_Cheats_GoldDropChance":
                                        {
                                            Save_Manager.instance.data.Character.Cheats.GoldDropChance = __0;
                                            //Content.Character.Cheats.golddropchance_text.text = "+ " + (int)((Save_Manager.instance.data.Character.Cheats.GoldDropChance / 255) * 100) + " %";
                                            break;
                                        }
                                    //Data
                                    case "Slider_Character_Data_Deaths":
                                        {
                                            if (!Refs_Manager.player_data.IsNullOrDestroyed())
                                            {
                                                Refs_Manager.player_data.Deaths = (int)__0;
                                            }
                                            
                                            //Content.Character.Data.deaths_text.text = ((int)__0).ToString();
                                            break;
                                        }
                                    case "Slider_Character_Data_LanternLuminance":
                                        {
                                            if (!Refs_Manager.player_data.IsNullOrDestroyed())
                                            {
                                                Refs_Manager.player_data.LanternLuminance = (int)__0;
                                            }
                                            //Content.Character.Data.lantern_text.text = ((int)__0).ToString();
                                            break;
                                        }
                                    case "Slider_Character_Data_SoulEmbers":
                                        {
                                            if (!Refs_Manager.player_data.IsNullOrDestroyed())
                                            {
                                                Refs_Manager.player_data.SoulEmbers = (int)__0;
                                            }
                                            //Content.Character.Data.soul_text.text = ((int)__0).ToString();
                                            break;
                                        }
                                    //Buffs
                                    case "Slider_Character_Buffs_MoveSpeed":
                                        {
                                            Save_Manager.instance.data.Character.PermanentBuffs.MoveSpeed_Buff_Value = __0;
                                            //Content.Character.Buffs.movespeed_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.MoveSpeed_Buff_Value / 255) * 100) + " %";
                                            break;
                                        }
                                    case "Slider_Character_Buffs_Damage":
                                        {
                                            Save_Manager.instance.data.Character.PermanentBuffs.Damage_Buff_Value = __0;
                                            //Content.Character.Buffs.damage_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.Damage_Buff_Value / 255) * 100) + " %";
                                            break;
                                        }
                                    case "Slider_Character_Buffs_AttackSpeed":
                                        {
                                            Save_Manager.instance.data.Character.PermanentBuffs.AttackSpeed_Buff_Value = __0;
                                            //Content.Character.Buffs.attackspeed_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.AttackSpeed_Buff_Value / 255) * 100) + " %";
                                            break;
                                        }
                                    case "Slider_Character_Buffs_CastingSpeed":
                                        {
                                            Save_Manager.instance.data.Character.PermanentBuffs.CastSpeed_Buff_Value = __0;
                                            //Content.Character.Buffs.castingspeed_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.CastSpeed_Buff_Value / 255) * 100) + " %";
                                            break;
                                        }
                                    case "Slider_Character_Buffs_CriticalChance":
                                        {
                                            Save_Manager.instance.data.Character.PermanentBuffs.CriticalChance_Buff_Value = __0;
                                            //Content.Character.Buffs.criticalchance_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.CriticalChance_Buff_Value / 255) * 100) + " %";
                                            break;
                                        }
                                    case "Slider_Character_Buffs_CriticalMultiplier":
                                        {
                                            Save_Manager.instance.data.Character.PermanentBuffs.CriticalMultiplier_Buff_Value = __0;
                                            //Content.Character.Buffs.criticalmultiplier_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.CriticalMultiplier_Buff_Value / 255) * 100) + " %";
                                            break;
                                        }
                                    case "Slider_Character_Buffs_HealthRegen":
                                        {
                                            Save_Manager.instance.data.Character.PermanentBuffs.HealthRegen_Buff_Value = __0;
                                            //Content.Character.Buffs.healthregen_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.HealthRegen_Buff_Value / 255) * 100) + " %";
                                            break;
                                        }
                                    case "Slider_Character_Buffs_ManaRegen":
                                        {
                                            Save_Manager.instance.data.Character.PermanentBuffs.ManaRegen_Buff_Value = __0;
                                            //Content.Character.Buffs.manaregen_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.ManaRegen_Buff_Value / 255) * 100) + " %";
                                            break;
                                        }
                                    case "Slider_Character_Buffs_Strenght":
                                        {
                                            Save_Manager.instance.data.Character.PermanentBuffs.Str_Buff_Value = __0;
                                            //Content.Character.Buffs.str_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.Str_Buff_Value / 255) * 100) + " %";
                                            break;
                                        }
                                    case "Slider_Character_Buffs_Intelligence":
                                        {
                                            Save_Manager.instance.data.Character.PermanentBuffs.Int_Buff_Value = __0;
                                            //Content.Character.Buffs.int_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.Int_Buff_Value / 255) * 100) + " %";
                                            break;
                                        }
                                    case "Slider_Character_Buffs_Dexterity":
                                        {
                                            Save_Manager.instance.data.Character.PermanentBuffs.Dex_Buff_Value = __0;
                                            //Content.Character.Buffs.dex_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.Dex_Buff_Value / 255) * 100) + " %";
                                            break;
                                        }
                                    case "Slider_Character_Buffs_Vitality":
                                        {
                                            Save_Manager.instance.data.Character.PermanentBuffs.Vit_Buff_Value = __0;
                                            //Content.Character.Buffs.vit_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.Vit_Buff_Value / 255) * 100) + " %";
                                            break;
                                        }
                                    case "Slider_Character_Buffs_Attunement":
                                        {
                                            Save_Manager.instance.data.Character.PermanentBuffs.Att_Buff_Value = __0;
                                            //Content.Character.Buffs.att_text.text = "+ " + (int)((Save_Manager.instance.data.Character.PermanentBuffs.Att_Buff_Value / 255) * 100) + " %";
                                            break;
                                        }
                                }
                            }
                            else if (__instance.name.Contains("Slider_Items_"))
                            {
                                switch (__instance.name)
                                {
                                    case "Slider_Items_Drop_Implicits_Min":
                                        {
                                            Save_Manager.instance.data.Items.Drop.Implicits_Min = __0;
                                            if (__0 > Save_Manager.instance.data.Items.Drop.Implicits_Max) { Content.Items.Drop.implicits_slider_max.value = __0; }
                                            break;
                                        }
                                    case "Slider_Items_Drop_Implicits_Max":
                                        {
                                            Save_Manager.instance.data.Items.Drop.Implicits_Max = __0;
                                            if (__0 < Save_Manager.instance.data.Items.Drop.Implicits_Min) { Content.Items.Drop.implicits_slider_min.value = __0; }
                                            break;
                                        }
                                    case "Slider_Items_Drop_ForginPotencial_Min":
                                        {
                                            Save_Manager.instance.data.Items.Drop.ForginPotencial_Min = __0;
                                            if (__0 > Save_Manager.instance.data.Items.Drop.ForginPotencial_Max) { Content.Items.Drop.forgin_potencial_slider_max.value = __0; }
                                            break;
                                        }
                                    case "Slider_Items_Drop_ForginPotencial_Max":
                                        {
                                            Save_Manager.instance.data.Items.Drop.ForginPotencial_Max = __0;
                                            if (__0 < Save_Manager.instance.data.Items.Drop.ForginPotencial_Min) { Content.Items.Drop.forgin_potencial_slider_min.value = __0; }
                                            break;
                                        }
                                    case "Slider_Items_Drop_SealTier_Min":
                                        {
                                            Save_Manager.instance.data.Items.Drop.SealTier_Min = __0;
                                            if (__0 > Save_Manager.instance.data.Items.Drop.SealTier_Max) { Content.Items.Drop.seal_tier_slider_max.value = __0; }
                                            break;
                                        }
                                    case "Slider_Items_Drop_SealTier_Max":
                                        {
                                            Save_Manager.instance.data.Items.Drop.SealTier_Max = __0;
                                            if (__0 < Save_Manager.instance.data.Items.Drop.SealTier_Min) { Content.Items.Drop.seal_tier_slider_min.value = __0; }
                                            break;
                                        }
                                    case "Slider_Items_Drop_SealValue_Min":
                                        {
                                            Save_Manager.instance.data.Items.Drop.SealValue_Min = __0;
                                            if (__0 > Save_Manager.instance.data.Items.Drop.SealValue_Max) { Content.Items.Drop.seal_value_slider_max.value = __0; }
                                            break;
                                        }
                                    case "Slider_Items_Drop_SealValue_Max":
                                        {
                                            Save_Manager.instance.data.Items.Drop.SealValue_Max = __0;
                                            if (__0 < Save_Manager.instance.data.Items.Drop.SealValue_Min) { Content.Items.Drop.seal_value_slider_min.value = __0; }
                                            break;
                                        }
                                    case "Slider_Items_Drop_NbAffixes_Min":
                                        {
                                            Save_Manager.instance.data.Items.Drop.AffixCount_Min = __0;
                                            if (__0 > Save_Manager.instance.data.Items.Drop.AffixCount_Max) { Content.Items.Drop.affix_count_slider_max.value = __0; }
                                            break;
                                        }
                                    case "Slider_Items_Drop_NbAffixes_Max":
                                        {
                                            if (Save_Manager.instance.data.Items.Drop.AffixCount_Max != __0) { Save_Manager.instance.data.Items.Drop.AffixCount_Max = __0; }
                                            if (__0 < Save_Manager.instance.data.Items.Drop.AffixCount_Min) { Content.Items.Drop.affix_count_slider_min.value = __0; }
                                            
                                            break;
                                        }
                                    case "Slider_Items_Drop_AffixesTiers_Min":
                                        {
                                            Save_Manager.instance.data.Items.Drop.AffixTiers_Min = __0;
                                            if (__0 > Save_Manager.instance.data.Items.Drop.AffixTiers_Max)
                                            { Content.Items.Drop.affix_tiers_slider_max.value = __0; }
                                            break;
                                        }
                                    case "Slider_Items_Drop_AffixesTiers_Max":
                                        {
                                            Save_Manager.instance.data.Items.Drop.AffixTiers_Max = __0;
                                            if (__0 < Save_Manager.instance.data.Items.Drop.AffixTiers_Min)
                                            { Content.Items.Drop.affix_tiers_slider_min.value = __0; }
                                            break;
                                        }
                                    case "Slider_Items_Drop_AffixesValues_Min":
                                        {
                                            Save_Manager.instance.data.Items.Drop.AffixValues_Min = __0;
                                            if (__0 > Save_Manager.instance.data.Items.Drop.AffixValues_Max)
                                            { Content.Items.Drop.affix_values_slider_max.value = __0; }
                                            break;
                                        }
                                    case "Slider_Items_Drop_AffixesValues_Max":
                                        {
                                            Save_Manager.instance.data.Items.Drop.AffixValues_Max = __0;
                                            if (Save_Manager.instance.data.Items.Drop.AffixValues_Max < Save_Manager.instance.data.Items.Drop.AffixValues_Min)
                                            { Content.Items.Drop.affix_values_slider_min.value = __0; }
                                            break;
                                        }
                                    case "Slider_Items_Drop_UniqueMods_Min":
                                        {
                                            Save_Manager.instance.data.Items.Drop.UniqueMods_Min = __0;
                                            if (Save_Manager.instance.data.Items.Drop.UniqueMods_Min > Save_Manager.instance.data.Items.Drop.UniqueMods_Max)
                                            { Content.Items.Drop.unique_mods_slider_max.value = __0; }
                                            break;
                                        }
                                    case "Slider_Items_Drop_UniqueMods_Max":
                                        {
                                            Save_Manager.instance.data.Items.Drop.UniqueMods_Max = __0;
                                            if (Save_Manager.instance.data.Items.Drop.UniqueMods_Max < Save_Manager.instance.data.Items.Drop.UniqueMods_Min)
                                            { Content.Items.Drop.unique_mods_slider_min.value = __0; }
                                            break;
                                        }
                                    case "Slider_Items_Drop_LegendaryPotencial_Min":
                                        {
                                            Save_Manager.instance.data.Items.Drop.LegendaryPotencial_Min = __0;
                                            if (Save_Manager.instance.data.Items.Drop.LegendaryPotencial_Min > Save_Manager.instance.data.Items.Drop.LegendaryPotencial_Max)
                                            { Content.Items.Drop.legendary_potencial_slider_max.value = __0; }
                                            break;
                                        }
                                    case "Slider_Items_Drop_LegendaryPotencial_Max":
                                        {
                                            Save_Manager.instance.data.Items.Drop.LegendaryPotencial_Max = __0;
                                            if (__0 < Save_Manager.instance.data.Items.Drop.LegendaryPotencial_Min) { Content.Items.Drop.legendary_potencial_slider_min.value = __0; }
                                            break;
                                        }
                                    case "Slider_Items_Drop_WeaverWill_Min":
                                        {
                                            Save_Manager.instance.data.Items.Drop.WeaverWill_Min = __0;
                                            if (__0 > Save_Manager.instance.data.Items.Drop.WeaverWill_Max) { Content.Items.Drop.weaver_will_slider_max.value = __0; }
                                            break;
                                        }
                                    case "Slider_Items_Drop_WeaverWill_Max":
                                        {
                                            Save_Manager.instance.data.Items.Drop.WeaverWill_Max = __0;
                                            if (__0 < Save_Manager.instance.data.Items.Drop.WeaverWill_Min) { Content.Items.Drop.weaver_will_slider_min.value = __0; }
                                            break;
                                        }
                                    //Craft
                                    case "Slider_Items_Craft_ForginPotencial": { Save_Manager.instance.data.Items.CraftingSlot.ForginPotencial = __0; break; }
                                    case "Slider_Items_Craft_Implicit0": { Save_Manager.instance.data.Items.CraftingSlot.Implicit_0 = __0; break; }
                                    case "Slider_Items_Craft_Implicit1": { Save_Manager.instance.data.Items.CraftingSlot.Implicit_1 = __0; break; }
                                    case "Slider_Items_Craft_Implicit2": { Save_Manager.instance.data.Items.CraftingSlot.Implicit_2 = __0; break; }

                                    case "Slider_Items_Craft_SealTier": { Save_Manager.instance.data.Items.CraftingSlot.Seal_Tier = (int)__0; break; }
                                    case "Slider_Items_Craft_SealValue": { Save_Manager.instance.data.Items.CraftingSlot.Seal_Value = __0; break; }
                                    
                                    case "Slider_Items_Craft_AffixTier0": { Save_Manager.instance.data.Items.CraftingSlot.Affix_0_Tier = (int)__0; break; }
                                    case "Slider_Items_Craft_AffixTier1": { Save_Manager.instance.data.Items.CraftingSlot.Affix_1_Tier = (int)__0; break; }
                                    case "Slider_Items_Craft_AffixTier2": { Save_Manager.instance.data.Items.CraftingSlot.Affix_2_Tier = (int)__0; break; }
                                    case "Slider_Items_Craft_AffixTier3": { Save_Manager.instance.data.Items.CraftingSlot.Affix_3_Tier = (int)__0; break; }
                                    case "Slider_Items_Craft_AffixValue0": { Save_Manager.instance.data.Items.CraftingSlot.Affix_0_Value = __0; break; }
                                    case "Slider_Items_Craft_AffixValue1": { Save_Manager.instance.data.Items.CraftingSlot.Affix_1_Value = __0; break; }
                                    case "Slider_Items_Craft_AffixValue2": { Save_Manager.instance.data.Items.CraftingSlot.Affix_2_Value = __0; break; }
                                    case "Slider_Items_Craft_AffixValue3": { Save_Manager.instance.data.Items.CraftingSlot.Affix_3_Value = __0; break; }

                                    case "Slider_Items_Craft_UniqueMod0": { Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_0 = __0; break; }
                                    case "Slider_Items_Craft_UniqueMod1": { Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_1 = __0; break; }
                                    case "Slider_Items_Craft_UniqueMod2": { Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_2 = __0; break; }
                                    case "Slider_Items_Craft_UniqueMod3": { Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_3 = __0; break; }
                                    case "Slider_Items_Craft_UniqueMod4": { Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_4 = __0; break; }
                                    case "Slider_Items_Craft_UniqueMod5": { Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_5 = __0; break; }
                                    case "Slider_Items_Craft_UniqueMod6": { Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_6 = __0; break; }
                                    case "Slider_Items_Craft_UniqueMod7": { Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_7 = __0; break; }

                                    case "Slider_Items_Craft_LegendaryPotencial": { Save_Manager.instance.data.Items.CraftingSlot.LegendaryPotencial = (int)__0; break; }
                                    case "Slider_Items_Craft_WeaverWill": { Save_Manager.instance.data.Items.CraftingSlot.WeaverWill = (int)__0; break; }
                                }
                            }
                            else if (__instance.name.Contains("Slider_Scenes_"))
                            {
                                switch (__instance.name)
                                {
                                    case "Slider_Scenes_Camera_ZoomMinimum": { Save_Manager.instance.data.Scenes.Camera.ZoomMinimum = __0; break; }
                                    case "Slider_Scenes_Camera_ZoomPerScroll": { Save_Manager.instance.data.Scenes.Camera.ZoomPerScroll = __0; break; }
                                    case "Slider_Scenes_Camera_ZoomSpeed": { Save_Manager.instance.data.Scenes.Camera.ZoomSpeed = __0; break; }
                                    case "Slider_Scenes_Camera_DefaultRotation": { Save_Manager.instance.data.Scenes.Camera.DefaultRotation = __0; break; }
                                    case "Slider_Scenes_Camera_OffsetMinimum": { Save_Manager.instance.data.Scenes.Camera.OffsetMinimum = __0; break; }
                                    case "Slider_Scenes_Camera_OffsetMaximum": { Save_Manager.instance.data.Scenes.Camera.OffsetMaximum = __0; break; }
                                    case "Slider_Scenes_Camera_AngleMinimum": { Save_Manager.instance.data.Scenes.Camera.AngleMinimum = __0; break; }
                                    case "Slider_Scenes_Camera_AngleMaximum": { Save_Manager.instance.data.Scenes.Camera.AngleMaximum = __0; break; }
                                    
                                    case "Slider_Scenes_Monoliths_MaxStability": { Save_Manager.instance.data.Scenes.Monoliths.MaxStability = __0; break; }
                                    case "Slider_Scenes_Monoliths_MobsDensity": { Save_Manager.instance.data.Scenes.Monoliths.MobsDensity = __0; break; }
                                    case "Slider_Scenes_Monoliths_MobsDefeatOnStart": { Save_Manager.instance.data.Scenes.Monoliths.MobsDefeatOnStart = __0; break; }
                                    case "Slider_Scenes_Monoliths_BlessingSlots": { Save_Manager.instance.data.Scenes.Monoliths.BlessingSlots = (int)__0; break; }
                                }
                            }
                            else
                            {
                                switch (__instance.name)
                                {
                                    case "Slider_SpecializationSlots": { Save_Manager.instance.data.Skills.SpecializationSlots = __0; break; }
                                    case "Slider_SkillLevel": { Save_Manager.instance.data.Skills.SkillLevel = __0; break; }
                                    case "Slider_PassivePoints": { Save_Manager.instance.data.Skills.PassivePoints = __0; break; }

                                    case "Slider_MaximumCompanions": { Save_Manager.instance.data.Skills.Companion.Limit = (int)__0; break; }
                                    case "Slider_Wolf_SummonLimit": { Save_Manager.instance.data.Skills.Companion.Wolf.SummonLimit = (int)__0; break; }
                                    case "Slider_Scorpions_SummonLimit": { Save_Manager.instance.data.Skills.Companion.Scorpion.BabyQuantity = (int)__0; break; }

                                    case "Slider_Skeleteon_SummonQuantityFromPassive": { Save_Manager.instance.data.Skills.Minions.Skeletons.additionalSkeletonsFromPassives = (int)__0; break; }
                                    case "Slider_Skeleteon_SummonQuantityFromSkillTree": { Save_Manager.instance.data.Skills.Minions.Skeletons.additionalSkeletonsFromSkillTree = (int)__0; break; }
                                    case "Slider_Skeleteon_SummonQuantityPerCast": { Save_Manager.instance.data.Skills.Minions.Skeletons.additionalSkeletonsPerCast = (int)__0; break; }
                                    case "Slider_Skeleteon_ChanceToResummonOnDeath": { Save_Manager.instance.data.Skills.Minions.Skeletons.chanceToResummonOnDeath = (int)__0; break; }

                                    case "Slider_Wraiths_SummonMax": { Save_Manager.instance.data.Skills.Minions.Wraiths.additionalMaxWraiths = (int)__0; break; }
                                    case "Slider_Wraiths_Delayed": { Save_Manager.instance.data.Skills.Minions.Wraiths.delayedWraiths = (int)__0; break; }
                                    case "Slider_Wraiths_CastSpeed": { Save_Manager.instance.data.Skills.Minions.Wraiths.increasedCastSpeed = (int)__0; break; }

                                    case "Slider_Mages_SummonQuantityFromPassive": { Save_Manager.instance.data.Skills.Minions.Mages.additionalSkeletonsFromPassives = (int)__0; break; }
                                    case "Slider_Mages_SummonQuantityFromSkillTree": { Save_Manager.instance.data.Skills.Minions.Mages.additionalSkeletonsFromSkillTree = (int)__0; break; }
                                    case "Slider_Mages_SummonQuantityFromItems": { Save_Manager.instance.data.Skills.Minions.Mages.additionalSkeletonsFromItems = (int)__0; break; }
                                    case "Slider_Mages_SummonPerCast": { Save_Manager.instance.data.Skills.Minions.Mages.additionalSkeletonsPerCast = (int)__0; break; }
                                    case "Slider_Mages_ChanceForExtraPorjectiles": { Save_Manager.instance.data.Skills.Minions.Mages.chanceForTwoExtraProjectiles = (int)__0; break; }

                                    case "Slider_BoneGolem_GolemPerSkeletons": { Save_Manager.instance.data.Skills.Minions.BoneGolems.addedGolemsPer4Skeletons = (int)__0; break; }
                                    case "Slider_BoneGolem_SelfResurectChance": { Save_Manager.instance.data.Skills.Minions.BoneGolems.selfResurrectChance = (int)__0; break; }
                                    case "Slider_BoneGolem_IncreaseFireAura": { Save_Manager.instance.data.Skills.Minions.BoneGolems.increasedFireAuraArea = (int)__0; break; }
                                    case "Slider_BoneGolem_IncreaseArmorAura": { Save_Manager.instance.data.Skills.Minions.BoneGolems.undeadArmorAura = (int)__0; break; }
                                    case "Slider_BoneGolem_IncreaseMoveSpeedAura": { Save_Manager.instance.data.Skills.Minions.BoneGolems.undeadMovespeedAura = (int)__0; break; }
                                    case "Slider_BoneGolem_IncreaseMoveSpeed": { Save_Manager.instance.data.Skills.Minions.BoneGolems.increasedMoveSpeed = (int)__0; break; }

                                    case "Slider_VolatileZombies_ChanceOnMinionDeath": { Save_Manager.instance.data.Skills.Minions.VolatileZombies.chanceToCastFromMinionDeath = (int)__0; break; }
                                    case "Slider_VolatileZombies_InfernalShadeChance": { Save_Manager.instance.data.Skills.Minions.VolatileZombies.chanceToCastInfernalShadeOnDeath = (int)__0; break; }
                                    case "Slider_VolatileZombies_MarrowShardsChance": { Save_Manager.instance.data.Skills.Minions.VolatileZombies.chanceToCastMarrowShardsOnDeath = (int)__0; break; }

                                    case "Slider_DreadShades_Duration": { Save_Manager.instance.data.Skills.Minions.DreadShades.Duration = (int)__0; break; }
                                    case "Slider_DreadShades_Max": { Save_Manager.instance.data.Skills.Minions.DreadShades.max = (int)__0; break; }
                                    case "Slider_DreadShades_Decay": { Save_Manager.instance.data.Skills.Minions.DreadShades.decay = (int)__0; break; }
                                    case "Slider_DreadShades_Radius": { Save_Manager.instance.data.Skills.Minions.DreadShades.radius = (int)__0; break; }
                                }
                            }
                        }
                    }
                }
            }
        }
        public class Events
        {
            public static void Set_Base_Button_Event(GameObject base_obj, string child, string btn_name, UnityEngine.Events.UnityAction action)
            {
                if (!base_obj.IsNullOrDestroyed())
                {
                    GameObject go = Functions.GetChild(base_obj, child);
                    if (!go.IsNullOrDestroyed())
                    {
                        GameObject btn_obj = Functions.GetChild(go, btn_name);
                        if (!btn_obj.IsNullOrDestroyed())
                        {
                            Button btn = btn_obj.GetComponent<Button>();
                            if (!btn.IsNullOrDestroyed())
                            {
                                btn.onClick = new Button.ButtonClickedEvent();
                                btn.onClick.AddListener(action);
                            }
                            else { Main.logger_instance.Error("Set_Base_Button_Event Can't found button"); }
                        }
                        else { Main.logger_instance.Error("Set_Base_Button_Event Can't found GameObject button " + btn_name); }
                    }
                    else { Main.logger_instance.Error("Set_Base_Button_Event Can't found " + child + " in base_obj"); }
                }
                else { Main.logger_instance.Error("Set_Base_Button_Event base_obj is null"); }
            }
            public static void Set_Button_Event(Button btn, UnityEngine.Events.UnityAction action)
            {
                btn.onClick = new Button.ButtonClickedEvent();
                btn.onClick.AddListener(action);
            }
            public static void Set_Slider_Event(Slider slider, UnityEngine.Events.UnityAction<float> action)
            {
                slider.onValueChanged = new Slider.SliderEvent();
                slider.onValueChanged.AddListener(action);
            }
            public static void Set_Toggle_Event(Toggle toggle, UnityEngine.Events.UnityAction<bool> action)
            {
                toggle.onValueChanged = new Toggle.ToggleEvent();
                toggle.onValueChanged.AddListener(action);
            }
        }
        public class Hud_Base
        {
            public static bool Initialized = false;
            public static bool Initializing = false;
            public static bool initiliazed_events = false;
            public static GameObject Default_PauseMenu_Btns = null;
            public static Button Btn_Resume;
            public static Button Btn_Settings;
            public static Button Btn_GameGuide;
            public static Button Btn_LeaveGame;
            public static Button Btn_ExitDesktop;
            public static GameObject ChapterInfo = null;
            public static GameObject Menu_Fade_Background = null;
            public static GameObject Chapter_Fade_Background = null;
                        
            public static bool Get_DefaultPauseMenu()
            {
                bool result = false;
                if (!Refs_Manager.game_uibase.IsNullOrDestroyed())
                {
                    GameObject Draw_over_login_canvas = Functions.GetChild(UIBase.instance.gameObject, "Draw Over Login Canvas");
                    if (!Draw_over_login_canvas.IsNullOrDestroyed())
                    {
                        game_pause_menu = Functions.GetChild(Draw_over_login_canvas, "Menu");
                        if (!game_pause_menu.IsNullOrDestroyed())
                        {
                            Default_PauseMenu_Btns = Functions.GetChild(game_pause_menu, "Menu Image");
                            Get_Refs();
                            //Set_Events();
                            result = true;
                        }
                        else { Main.logger_instance.Msg("Get_DefaultPauseMenu : game_pause_menu NOT FOUND"); }                        
                    }
                    else { Main.logger_instance.Msg("Get_DefaultPauseMenu : Draw_over_login_canvas NOT FOUND"); }
                }

                return result;
            }
            public static void Set_ChapterInfo(bool show)
            {
                if ((!Refs_Manager.game_uibase.IsNullOrDestroyed()) && (!game_pause_menu.IsNullOrDestroyed()))
                {
                    if (ChapterInfo.IsNullOrDestroyed()) { ChapterInfo = Functions.GetChild(game_pause_menu, "ChapterInfo"); }
                    if (!ChapterInfo.IsNullOrDestroyed()) { ChapterInfo.active = show; }

                    if (Menu_Fade_Background.IsNullOrDestroyed()) { Menu_Fade_Background = Functions.GetChild(game_pause_menu, "Menu_Fade_Background"); }
                    if (!Menu_Fade_Background.IsNullOrDestroyed()) { Menu_Fade_Background.active = show; }

                    if (Chapter_Fade_Background.IsNullOrDestroyed()) { Chapter_Fade_Background = Functions.GetChild(game_pause_menu, "Chapter_Fade_Background"); }
                    if (!Chapter_Fade_Background.IsNullOrDestroyed()) { Chapter_Fade_Background.active = show; }
                }
            }                        
            public static bool Get_DefaultPauseMenu_Open()
            {
                if (!Default_PauseMenu_Btns.IsNullOrDestroyed())
                {
                     return Default_PauseMenu_Btns.active;
                }
                else { return false; }
            }
            public static void Toogle_DefaultPauseMenu(bool show)
            {
                if (!Default_PauseMenu_Btns.IsNullOrDestroyed())
                {
                    Default_PauseMenu_Btns.active = show;
                }
            }
            
            public static void Get_Refs()
            {
                if (!Default_PauseMenu_Btns.IsNullOrDestroyed())
                {
                    GameObject Btns = Functions.GetChild(Default_PauseMenu_Btns, "Buttons");
                    if (!Btns.IsNullOrDestroyed())
                    {
                        Hud_Base.Btn_Resume = Functions.GetChild(Btns, "ResumeButton (1)").GetComponent<Button>();
                        Hud_Base.Btn_Settings = Functions.GetChild(Btns, "SettingsButton").GetComponent<Button>();
                        Hud_Base.Btn_GameGuide = Functions.GetChild(Btns, "GameButton").GetComponent<Button>();
                        Hud_Base.Btn_LeaveGame = Functions.GetChild(Btns, "ExitToCharacterSelectButton").GetComponent<Button>();
                        Hud_Base.Btn_ExitDesktop = Functions.GetChild(Btns, "ExitGameButton").GetComponent<Button>();
                    }
                }
            }            
            public static void Set_Events()
            {
                if ((!Default_PauseMenu_Btns.IsNullOrDestroyed()) && (!hud_object.IsNullOrDestroyed()))
                {
                    Events.Set_Base_Button_Event(hud_object, "Base", "Btn_Base_Resume", Resume_OnClick_Action);
                    Events.Set_Base_Button_Event(hud_object, "Base", "Btn_Base_Settings", Settings_OnClick_Action);
                    Events.Set_Base_Button_Event(hud_object, "Base", "Btn_Base_GameGuide", GameGuide_OnClick_Action);
                    Events.Set_Base_Button_Event(hud_object, "Base", "Btn_Base_LeaveGame", LeaveGame_OnClick_Action);
                    Events.Set_Base_Button_Event(hud_object, "Base", "Btn_Base_ExitDesktop", ExitDesktop_OnClick_Action);
                    initiliazed_events = true;
                }
            }

            private static readonly System.Action Resume_OnClick_Action = new System.Action(Resume_Click);
            public static void Resume_Click()
            {
                if ((!hud_object.gameObject.IsNullOrDestroyed()) && (!Btn_Resume.IsNullOrDestroyed()))
                {
                    Btn_Resume.onClick.Invoke();
                }
            }

            private static readonly System.Action Settings_OnClick_Action = new System.Action(Settings_Click);
            public static void Settings_Click()
            {
                if ((!hud_object.IsNullOrDestroyed()) && (!Btn_Settings.IsNullOrDestroyed()))
                {
                    Btn_Settings.onClick.Invoke();
                }
            }

            private static readonly System.Action GameGuide_OnClick_Action = new System.Action(GameGuide_Click);
            public static void GameGuide_Click()
            {
                if ((!hud_object.IsNullOrDestroyed()) && (!Btn_GameGuide.IsNullOrDestroyed()))
                {
                    Btn_GameGuide.onClick.Invoke();
                }
            }

            private static readonly System.Action LeaveGame_OnClick_Action = new System.Action(LeaveGame_Click);
            public static void LeaveGame_Click()
            {
                if ((!hud_object.IsNullOrDestroyed()) && (!Btn_LeaveGame.IsNullOrDestroyed()))
                {
                    Content.Close_AllContent();
                    Btn_LeaveGame.onClick.Invoke();
                }
            }

            private static readonly System.Action ExitDesktop_OnClick_Action = new System.Action(ExitDesktop_Click);
            public static void ExitDesktop_Click()
            {
                if ((!hud_object.IsNullOrDestroyed()) && (!Btn_ExitDesktop.IsNullOrDestroyed()))
                {
                    Content.Close_AllContent();
                    Btn_ExitDesktop.onClick.Invoke();
                }
            }
        }
        public class Hud_Menu
        {
            public static void Set_Events()
            {
                Events.Set_Base_Button_Event(hud_object, "Menu", "Btn_Menu_Character", Character_OnClick_Action);
                Events.Set_Base_Button_Event(hud_object, "Menu", "Btn_Menu_Items", Items_OnClick_Action);
                Events.Set_Base_Button_Event(hud_object, "Menu", "Btn_Menu_Scenes", Scenes_OnClick_Action);
                Events.Set_Base_Button_Event(hud_object, "Menu", "Btn_Menu_TreeSkills", Skills_OnClick_Action);
                Events.Set_Base_Button_Event(hud_object, "Menu", "Btn_Menu_ForceDrop", OldForceDrop_OnClick_Action);
                Events.Set_Base_Button_Event(hud_object, "Menu", "Btn_Menu_Headhunter", Headhunter_OnClick_Action);
            }
            
            private static readonly System.Action Character_OnClick_Action = new System.Action(Character_Click);
            public static void Character_Click()
            {
                Content.Items.Set_Active(false);
                Content.Scenes.Set_Active(false);
                Content.Skills.Set_Active(false);
                Content.OdlForceDrop.Set_Active(false);
                Content.Headhunter.Set_Active(false);
                Content.Character.Toggle_Active();          
            }

            private static readonly System.Action Items_OnClick_Action = new System.Action(Items_Click);
            public static void Items_Click()
            {
                Content.Character.Set_Active(false);
                Content.Scenes.Set_Active(false);
                Content.Skills.Set_Active(false);
                Content.OdlForceDrop.Set_Active(false);
                Content.Headhunter.Set_Active(false);
                Content.Items.Toggle_Active();
            }

            private static readonly System.Action Scenes_OnClick_Action = new System.Action(Scenes_Click);
            public static void Scenes_Click()
            {
                Content.Character.Set_Active(false);
                Content.Items.Set_Active(false);
                Content.Skills.Set_Active(false);
                Content.OdlForceDrop.Set_Active(false);
                Content.Headhunter.Set_Active(false);
                Content.Scenes.Toggle_Active();
            }

            private static readonly System.Action Skills_OnClick_Action = new System.Action(Skills_Click);
            public static void Skills_Click()
            {
                Content.Character.Set_Active(false);
                Content.Items.Set_Active(false);
                Content.Scenes.Set_Active(false);
                Content.OdlForceDrop.Set_Active(false);
                Content.Headhunter.Set_Active(false);
                Content.Skills.Toggle_Active();
            }

            private static readonly System.Action OldForceDrop_OnClick_Action = new System.Action(OldForceDrop_Click);
            public static void OldForceDrop_Click()
            {
                Content.Character.Set_Active(false);
                Content.Items.Set_Active(false);
                Content.Scenes.Set_Active(false);
                Content.Skills.Set_Active(false);
                Content.Headhunter.Set_Active(false);
                Content.OdlForceDrop.Toggle_Active();
            }

            private static readonly System.Action Headhunter_OnClick_Action = new System.Action(Headhunter_Click);
            public static void Headhunter_Click()
            {
                Content.Character.Set_Active(false);
                Content.Items.Set_Active(false);
                Content.Scenes.Set_Active(false);
                Content.Skills.Set_Active(false);
                Content.OdlForceDrop.Set_Active(false);
                Content.Headhunter.Toggle_Active();
            }
        }                
        public class Content
        {
            public static GameObject content_obj = null;
            public static void Set_Active()
            {
                if (!content_obj.IsNullOrDestroyed())
                {
                    bool show = false;
                    if ((Character.enable) || (Items.enable) || (Scenes.enable) || (Skills.enable) ||
                        (OdlForceDrop.enable) || (Headhunter.enable)) { show = true; }
                    if (content_obj.active != show) { content_obj.active = show; }
                }
            }
            public static void Close_AllContent()
            {
                Character.enable = false;
                Items.enable = false;
                Scenes.enable = false;
                Skills.enable = false;
                OdlForceDrop.enable = false;
                Headhunter.enable = false;
            }

            public class Character
            {
                public static GameObject content_obj = null;
                public static bool controls_initialized = false;
                public static bool enable = false;
                public static bool need_update = true;

                public static void Get_Refs()
                {
                    content_obj = Functions.GetChild(Content.content_obj, "Character_Content");
                    if (!content_obj.IsNullOrDestroyed())
                    {
                        GameObject character_cheats_content = Functions.GetViewportContent(content_obj, "Character_Cheats", "Character_Cheats_Content");
                        if (!character_cheats_content.IsNullOrDestroyed())
                        {
                            Cheats.godmode_toggle = Functions.Get_ToggleInPanel(character_cheats_content, "GodMode", "Toggle_Character_Cheats_GodMode");
                            Cheats.lowlife_toggle = Functions.Get_ToggleInPanel(character_cheats_content, "ForceLowLife", "Toggle_Character_Cheats_LowLife");
                            Cheats.allow_choosing_blessing = Functions.Get_ToggleInPanel(character_cheats_content, "AllowChoosingBlessings", "Toggle_Character_Cheats_AllowChooseBlessings");
                            Cheats.unlock_all_idols = Functions.Get_ToggleInPanel(character_cheats_content, "UnlockAllIdolsSlots", "Toggle_Character_Cheats_UnlockAllIdols");                            

                            Cheats.autoPot_toggle = Functions.Get_ToggleInPanel(character_cheats_content, "AutoPotions", "Toggle_Character_Cheats_AutoPotions");
                            Cheats.autopot_text = Functions.Get_TextInToggle(character_cheats_content, "AutoPotions", "Toggle_Character_Cheats_AutoPotions", "Value");
                            Cheats.autopot_slider = Functions.Get_SliderInPanel(character_cheats_content, "AutoPotions", "Slider_Character_Cheats_AutoPotions");

                            Cheats.density_toggle = Functions.Get_ToggleInPanel(character_cheats_content, "DensityMultiplier", "Toggle_Character_Cheats_DensityMultiplier");
                            Cheats.density_text = Functions.Get_TextInToggle(character_cheats_content, "DensityMultiplier", "Toggle_Character_Cheats_DensityMultiplier", "Value");
                            Cheats.density_slider = Functions.Get_SliderInPanel(character_cheats_content, "DensityMultiplier", "Slider_Character_Cheats_DensityMultiplier");

                            Cheats.experience_toggle = Functions.Get_ToggleInPanel(character_cheats_content, "ExperienceMultiplier", "Toggle_Character_Cheats_ExperienceMultiplier");
                            Cheats.experience_text = Functions.Get_TextInToggle(character_cheats_content, "ExperienceMultiplier", "Toggle_Character_Cheats_ExperienceMultiplier", "Value");
                            Cheats.experience_slider = Functions.Get_SliderInPanel(character_cheats_content, "ExperienceMultiplier", "Slider_Character_Cheats_ExperienceMultiplier");

                            Cheats.ability_toggle = Functions.Get_ToggleInPanel(character_cheats_content, "AbilityMultiplier", "Toggle_Character_Cheats_AbilityMultiplier");
                            Cheats.ability_text = Functions.Get_TextInToggle(character_cheats_content, "AbilityMultiplier", "Toggle_Character_Cheats_AbilityMultiplier", "Value");
                            Cheats.ability_slider = Functions.Get_SliderInPanel(character_cheats_content, "AbilityMultiplier", "Slider_Character_Cheats_AbilityMultiplier");

                            Cheats.favor_toggle = Functions.Get_ToggleInPanel(character_cheats_content, "FavorMultiplier", "Toggle_Character_Cheats_FavorMultiplier");
                            Cheats.favor_text = Functions.Get_TextInToggle(character_cheats_content, "FavorMultiplier", "Toggle_Character_Cheats_FavorMultiplier", "Value");
                            Cheats.favor_slider = Functions.Get_SliderInPanel(character_cheats_content, "FavorMultiplier", "Slider_Character_Cheats_FavorMultiplier");

                            Cheats.itemdropmultiplier_toggle = Functions.Get_ToggleInPanel(character_cheats_content, "ItemDropMultiplier", "Toggle_Character_Cheats_ItemDropMultiplier");
                            Cheats.itemdropmultiplier_text = Functions.Get_TextInToggle(character_cheats_content, "ItemDropMultiplier", "Toggle_Character_Cheats_ItemDropMultiplier", "Value");
                            Cheats.itemdropmultiplier_slider = Functions.Get_SliderInPanel(character_cheats_content, "ItemDropMultiplier", "Slider_Character_Cheats_ItemDropMultiplier");

                            Cheats.itemdropchance_toggle = Functions.Get_ToggleInPanel(character_cheats_content, "ItemDropChance", "Toggle_Character_Cheats_ItemDropChance");
                            Cheats.itemdropchance_text = Functions.Get_TextInToggle(character_cheats_content, "ItemDropChance", "Toggle_Character_Cheats_ItemDropChance", "Value");
                            Cheats.itemdropchance_slider = Functions.Get_SliderInPanel(character_cheats_content, "ItemDropChance", "Slider_Character_Cheats_ItemDropChance");

                            Cheats.golddropmultiplier_toggle = Functions.Get_ToggleInPanel(character_cheats_content, "GoldDropMultiplier", "Toggle_Character_Cheats_GoldDropMultiplier");
                            Cheats.golddropmultiplier_text = Functions.Get_TextInToggle(character_cheats_content, "GoldDropMultiplier", "Toggle_Character_Cheats_GoldDropMultiplier", "Value");
                            Cheats.golddropmultiplier_slider = Functions.Get_SliderInPanel(character_cheats_content, "GoldDropMultiplier", "Slider_Character_Cheats_GoldDropMultiplier");

                            Cheats.golddropchance_toggle = Functions.Get_ToggleInPanel(character_cheats_content, "GoldDropChance", "Toggle_Character_Cheats_GoldDropChance");
                            Cheats.golddropchance_text = Functions.Get_TextInToggle(character_cheats_content, "GoldDropChance", "Toggle_Character_Cheats_GoldDropChance", "Value");
                            Cheats.golddropchance_slider = Functions.Get_SliderInPanel(character_cheats_content, "GoldDropChance", "Slider_Character_Cheats_GoldDropChance");

                            Cheats.waypoints_toggle = Functions.Get_ToggleInPanel(character_cheats_content, "WaypointsUnlock", "Toggle_Character_Cheats_UnlockAllWaypoints");

                            Cheats.level_once_button = Functions.GetChild(character_cheats_content, "Btn_Character_Cheats_LevelOnce").GetComponent<Button>();
                            Cheats.level_max_button = Functions.GetChild(character_cheats_content, "Btn_Character_Cheats_LevelToMax").GetComponent<Button>();
                            Cheats.complete_quest_button = Functions.GetChild(character_cheats_content, "Btn_Character_Cheats_CompleteQuest").GetComponent<Button>();
                            Cheats.masterie_buttons = Functions.GetChild(character_cheats_content, "Btn_Character_Cheats_Masterie").GetComponent<Button>();
                            Cheats.masterie_text = Functions.Get_TextInButton(character_cheats_content, "Btn_Character_Cheats_Masterie", "Label");
                            Cheats.add_runes_button = Functions.GetChild(character_cheats_content, "Btn_Character_Cheats_AddRunes").GetComponent<Button>();
                            Cheats.add_glyphs_button = Functions.GetChild(character_cheats_content, "Btn_Character_Cheats_AddGlyphs").GetComponent<Button>();
                            Cheats.add_shards_button = Functions.GetChild(character_cheats_content, "Btn_Character_Cheats_AddAffixs").GetComponent<Button>();
                            Cheats.discover_blessings_button = Functions.GetChild(character_cheats_content, "Btn_Character_Cheats_DicoverAllBlessings").GetComponent<Button>();
                        }
                        else { Main.logger_instance.Error("Hud Manager : character_cheats_content is null"); }

                        //Data
                        GameObject character_data_content = Functions.GetViewportContent(content_obj, "Character_Data", "Character_Data_Content");
                        if (!character_data_content.IsNullOrDestroyed())
                        {
                            // Login here because it doesn't fit anywhere else
                            Login.GetRefs(character_data_content);

                            Data.class_dropdown = Functions.Get_DopboxInPanel(character_data_content, "Classe", "Dropdown_Character_Data_Classes", new System.Action<int>((_) => { if (!Refs_Manager.player_data.IsNullOrDestroyed()) { Refs_Manager.player_data.CharacterClass = Data.class_dropdown.value; } }));
                            
                            Data.died_toggle = Functions.Get_ToggleInPanel(character_data_content, "Died", "Toggle_Character_Data_Died");
                            Data.hardcore_toggle = Functions.Get_ToggleInPanel(character_data_content, "Hardcore", "Toggle_Character_Data_Hardcore");
                            Data.masochist_toggle = Functions.Get_ToggleInPanel(character_data_content, "Masochist", "Toggle_Character_Data_Masochist");
                            Data.portal_toggle = Functions.Get_ToggleInPanel(character_data_content, "Portal Unlocked", "Toggle_Character_Data_Portal");
                            Data.solo_toggle = Functions.Get_ToggleInPanel(character_data_content, "SoloChallenge", "Toggle_Character_Data_SoloChallenge");

                            Data.deaths_text = Functions.Get_TextInButton(character_data_content, "Deaths", "Value");
                            Data.deaths_slider = Functions.Get_SliderInPanel(character_data_content, "Deaths", "Slider_Character_Data_Deaths");

                            Data.lantern_text = Functions.Get_TextInButton(character_data_content, "LanternLuminance", "Value");
                            Data.lantern_slider = Functions.Get_SliderInPanel(character_data_content, "LanternLuminance", "Slider_Character_Data_LanternLuminance");

                            Data.soul_text = Functions.Get_TextInButton(character_data_content, "Soul Embers", "Value");
                            Data.soul_slider = Functions.Get_SliderInPanel(character_data_content, "Soul Embers", "Slider_Character_Data_SoulEmbers");

                            Data.monolith_stability_basic_go = Functions.GetChild(character_data_content, "Monolith_Stability_Basic");
                            Data.monolith_stability_basic_go.active = false;
                            Data.monolith_stability_basic_text = Functions.Get_TextInButton(character_data_content, "Monolith_Stability_Basic", "Value");
                            Data.monolith_stability_basic_slider = Functions.Get_SliderInPanel(character_data_content, "Monolith_Stability_Basic", "Slider_Basic_Stability");
                            
                            Data.monolith_stability_empower_go = Functions.GetChild(character_data_content, "Monolith_Stability_Empower");
                            Data.monolith_stability_empower_go.active = false;
                            Data.monolith_stability_empower_text = Functions.Get_TextInButton(character_data_content, "Monolith_Stability_Empower", "Value");
                            Data.monolith_stability_empower_slider = Functions.Get_SliderInPanel(character_data_content, "Monolith_Stability_Empower", "Slider_Empower_Stability");

                            Data.monolith_corruption_go = Functions.GetChild(character_data_content, "Monolith_Corruption");
                            Data.monolith_corruption_go.active = false;
                            Data.monolith_corruption_text = Functions.Get_TextInButton(character_data_content, "Monolith_Corruption", "Value");
                            Data.monolith_corruption_slider = Functions.Get_SliderInPanel(character_data_content, "Monolith_Corruption", "Slider_Empower_Corruption");

                            Data.monolith_gaze_go = Functions.GetChild(character_data_content, "Monolith_Gaze");
                            Data.monolith_gaze_go.active = false;
                            Data.monolith_gaze_text = Functions.Get_TextInButton(character_data_content, "Monolith_Gaze", "Value");
                            Data.monolith_gaze_slider = Functions.Get_SliderInPanel(character_data_content, "Monolith_Gaze", "Slider");

                            Data.monolith_dropdown = Functions.Get_DopboxInPanel(character_data_content, "Monoliths", "Dropdown", new System.Action<int>((_) => { Update_Monoliths_Data(); }));
                            Data.monolith_dropdown.options = new List<Dropdown.OptionData>();
                            Data.monolith_dropdown.options.Add(new Dropdown.OptionData { text = "Select" });
                            Data.monolith_dropdown.options.Add(new Dropdown.OptionData { text = "Fall_Of_The_Outcast" });
                            Data.monolith_dropdown.options.Add(new Dropdown.OptionData { text = "The_Stolen_Lance" });
                            Data.monolith_dropdown.options.Add(new Dropdown.OptionData { text = "The_Black_Sun" });
                            Data.monolith_dropdown.options.Add(new Dropdown.OptionData { text = "Blood_Frost_And_Death" });
                            Data.monolith_dropdown.options.Add(new Dropdown.OptionData { text = "Ending_The_Storm" });
                            Data.monolith_dropdown.options.Add(new Dropdown.OptionData { text = "Fall_Of_The_Empire" });
                            Data.monolith_dropdown.options.Add(new Dropdown.OptionData { text = "Reign_Of_Dragon" });
                            Data.monolith_dropdown.options.Add(new Dropdown.OptionData { text = "The_Last_Ruins" });
                            Data.monolith_dropdown.options.Add(new Dropdown.OptionData { text = "The_Age_Of_Winter" });
                            Data.monolith_dropdown.options.Add(new Dropdown.OptionData { text = "Spirits_Of_Fire" });
                            Data.monolith_dropdown.m_CurrentIndex = 0;
                        }
                        else { Main.logger_instance.Error("Hud Manager : character_data_content is null"); }

                        GameObject char_data = Functions.GetChild(content_obj, "Character_Data");
                        if (!char_data.IsNullOrDestroyed())
                        {
                            GameObject panel_save = Functions.GetChild(char_data, "Panel");
                            if (!panel_save.IsNullOrDestroyed())
                            {
                                Data.save_button = Functions.GetChild(panel_save, "Btn_Character_Data_Save").GetComponent<Button>();
                            }
                        }

                        //Buffs
                        Buffs.enable_mod = Functions.Get_ToggleInLabel(content_obj, "Character_Buffs", "Toggle_Character_Buffs_Enable");
                        GameObject character_buffs_content = Functions.GetViewportContent(content_obj, "Character_Buffs", "Character_Buffs_Content");
                        if (!character_buffs_content.IsNullOrDestroyed())
                        {
                            //Movespeed
                            Buffs.movespeed_toggle = Functions.Get_ToggleInPanel(character_buffs_content, "MoveSpeed", "Toggle_Character_Buffs_MoveSpeed");
                            Buffs.movespeed_text = Functions.Get_TextInToggle(character_buffs_content, "MoveSpeed", "Toggle_Character_Buffs_MoveSpeed", "Value");
                            Buffs.movespeed_slider = Functions.Get_SliderInPanel(character_buffs_content, "MoveSpeed", "Slider_Character_Buffs_MoveSpeed");

                            //Damage
                            Buffs.damage_toggle = Functions.Get_ToggleInPanel(character_buffs_content, "Damage", "Toggle_Character_Buffs_Damage");
                            Buffs.damage_text = Functions.Get_TextInToggle(character_buffs_content, "Damage", "Toggle_Character_Buffs_Damage", "Value");
                            Buffs.damage_slider = Functions.Get_SliderInPanel(character_buffs_content, "Damage", "Slider_Character_Buffs_Damage");

                            //AttackSpeed
                            Buffs.attackspeed_toggle = Functions.Get_ToggleInPanel(character_buffs_content, "AttackSpeed", "Toggle_Character_Buffs_AttackSpeed");
                            Buffs.attackspeed_text = Functions.Get_TextInToggle(character_buffs_content, "AttackSpeed", "Toggle_Character_Buffs_AttackSpeed", "Value");
                            Buffs.attackspeed_slider = Functions.Get_SliderInPanel(character_buffs_content, "AttackSpeed", "Slider_Character_Buffs_AttackSpeed");

                            //CastingSpeed
                            Buffs.castingspeed_toggle = Functions.Get_ToggleInPanel(character_buffs_content, "CastingSpeed", "Toggle_Character_Buffs_CastingSpeed");
                            Buffs.castingspeed_text = Functions.Get_TextInToggle(character_buffs_content, "CastingSpeed", "Toggle_Character_Buffs_CastingSpeed", "Value");
                            Buffs.castingspeed_slider = Functions.Get_SliderInPanel(character_buffs_content, "CastingSpeed", "Slider_Character_Buffs_CastingSpeed");

                            //CriticalChance
                            Buffs.criticalchance_toggle = Functions.Get_ToggleInPanel(character_buffs_content, "CriticalChance", "Toggle_Character_Buffs_CriticalChance");
                            Buffs.criticalchance_text = Functions.Get_TextInToggle(character_buffs_content, "CriticalChance", "Toggle_Character_Buffs_CriticalChance", "Value");
                            Buffs.criticalchance_slider = Functions.Get_SliderInPanel(character_buffs_content, "CriticalChance", "Slider_Character_Buffs_CriticalChance");

                            //CriticalMultiplier
                            Buffs.criticalmultiplier_toggle = Functions.Get_ToggleInPanel(character_buffs_content, "CriticalMultiplier", "Toggle_Character_Buffs_CriticalMultiplier");
                            Buffs.criticalmultiplier_text = Functions.Get_TextInToggle(character_buffs_content, "CriticalMultiplier", "Toggle_Character_Buffs_CriticalMultiplier", "Value");
                            Buffs.criticalmultiplier_slider = Functions.Get_SliderInPanel(character_buffs_content, "CriticalMultiplier", "Slider_Character_Buffs_CriticalMultiplier");

                            //HealthRegen
                            Buffs.healthregen_toggle = Functions.Get_ToggleInPanel(character_buffs_content, "HealthRegen", "Toggle_Character_Buffs_HealthRegen");
                            Buffs.healthregen_text = Functions.Get_TextInToggle(character_buffs_content, "HealthRegen", "Toggle_Character_Buffs_HealthRegen", "Value");
                            Buffs.healthregen_slider = Functions.Get_SliderInPanel(character_buffs_content, "HealthRegen", "Slider_Character_Buffs_HealthRegen");

                            //ManaRegen
                            Buffs.manaregen_toggle = Functions.Get_ToggleInPanel(character_buffs_content, "ManaRegen", "Toggle_Character_Buffs_ManaRegen");
                            Buffs.manaregen_text = Functions.Get_TextInToggle(character_buffs_content, "ManaRegen", "Toggle_Character_Buffs_ManaRegen", "Value");
                            Buffs.manaregen_slider = Functions.Get_SliderInPanel(character_buffs_content, "ManaRegen", "Slider_Character_Buffs_ManaRegen");

                            //Strenght
                            Buffs.str_toggle = Functions.Get_ToggleInPanel(character_buffs_content, "Strenght", "Toggle_Character_Buffs_Strenght");
                            Buffs.str_text = Functions.Get_TextInToggle(character_buffs_content, "Strenght", "Toggle_Character_Buffs_Strenght", "Value");
                            Buffs.str_slider = Functions.Get_SliderInPanel(character_buffs_content, "Strenght", "Slider_Character_Buffs_Strenght");

                            //Intelligence
                            Buffs.int_toggle = Functions.Get_ToggleInPanel(character_buffs_content, "Intelligence", "Toggle_Character_Buffs_Intelligence");
                            Buffs.int_text = Functions.Get_TextInToggle(character_buffs_content, "Intelligence", "Toggle_Character_Buffs_Intelligence", "Value");
                            Buffs.int_slider = Functions.Get_SliderInPanel(character_buffs_content, "Intelligence", "Slider_Character_Buffs_Intelligence");

                            //Dexterity
                            Buffs.dex_toggle = Functions.Get_ToggleInPanel(character_buffs_content, "Dexterity", "Toggle_Character_Buffs_Dexterity");
                            Buffs.dex_text = Functions.Get_TextInToggle(character_buffs_content, "Dexterity", "Toggle_Character_Buffs_Dexterity", "Value");
                            Buffs.dex_slider = Functions.Get_SliderInPanel(character_buffs_content, "Dexterity", "Slider_Character_Buffs_Dexterity");

                            //Vitality
                            Buffs.vit_toggle = Functions.Get_ToggleInPanel(character_buffs_content, "Vitality", "Toggle_Character_Buffs_Vitality");
                            Buffs.vit_text = Functions.Get_TextInToggle(character_buffs_content, "Vitality", "Toggle_Character_Buffs_Vitality", "Value");
                            Buffs.vit_slider = Functions.Get_SliderInPanel(character_buffs_content, "Vitality", "Slider_Character_Buffs_Vitality");

                            //Attunement
                            Buffs.att_toggle = Functions.Get_ToggleInPanel(character_buffs_content, "Attunement", "Toggle_Character_Buffs_Attunement");
                            Buffs.att_text = Functions.Get_TextInToggle(character_buffs_content, "Attunement", "Toggle_Character_Buffs_Attunement", "Value");
                            Buffs.att_slider = Functions.Get_SliderInPanel(character_buffs_content, "Attunement", "Slider_Character_Buffs_Attunement");
                        }
                        else { Main.logger_instance.Error("Hud Manager : character_buffs_content is null"); }
                    }
                    else { Main.logger_instance.Error("Hud Manager : Character_Content is null"); }
                }
                public static void Set_Events()
                {
                    Events.Set_Toggle_Event(Cheats.godmode_toggle, Cheats.Godmode_Toggle_Action);
                    Events.Set_Toggle_Event(Cheats.lowlife_toggle, Cheats.Lowlife_Toggle_Action);
                    Events.Set_Toggle_Event(Cheats.allow_choosing_blessing, Cheats.AllowChooseBlessings_Toggle_Action);
                    Events.Set_Toggle_Event(Cheats.unlock_all_idols, Cheats.UnlockAllIdols_Toggle_Action);                    
                    Events.Set_Toggle_Event(Cheats.autoPot_toggle, Cheats.AutoPot_Toggle_Action);

                    Events.Set_Toggle_Event(Cheats.density_toggle, Cheats.Density_Toggle_Action);

                    Events.Set_Toggle_Event(Cheats.experience_toggle, Cheats.Experience_Toggle_Action);

                    Events.Set_Toggle_Event(Cheats.ability_toggle, Cheats.Ability_Toggle_Action);

                    Events.Set_Toggle_Event(Cheats.favor_toggle, Cheats.Favor_Toggle_Action);

                    Events.Set_Toggle_Event(Cheats.itemdropmultiplier_toggle, Cheats.ItemDropMulti_Toggle_Action);

                    Events.Set_Toggle_Event(Cheats.itemdropchance_toggle, Cheats.ItemDropChance_Toggle_Action);

                    Events.Set_Toggle_Event(Cheats.golddropmultiplier_toggle, Cheats.GoldMulti_Toggle_Action);
                    
                    Events.Set_Toggle_Event(Cheats.golddropchance_toggle, Cheats.GoldChance_Toggle_Action);
                    
                    Events.Set_Toggle_Event(Cheats.waypoints_toggle, Cheats.Waypoints_Toggle_Action);
                    Events.Set_Button_Event(Cheats.level_once_button, Cheats.LevelUpOnce_OnClick_Action);
                    Events.Set_Button_Event(Cheats.level_max_button, Cheats.LevelUpMax_OnClick_Action);                    
                    Events.Set_Button_Event(Cheats.complete_quest_button, Cheats.CompleteQuest_OnClick_Action);
                    Events.Set_Button_Event(Cheats.masterie_buttons, Cheats.Masteries_OnClick_Action);
                    Events.Set_Button_Event(Cheats.add_runes_button, Cheats.AddRunes_OnClick_Action);
                    Events.Set_Button_Event(Cheats.add_glyphs_button, Cheats.AddGlyphs_OnClick_Action);
                    Events.Set_Button_Event(Cheats.add_shards_button, Cheats.AddAffixs_OnClick_Action);
                    Events.Set_Button_Event(Cheats.discover_blessings_button, Cheats.DiscoverAllBlessings_OnClick_Action);

                    Events.Set_Slider_Event(Data.monolith_stability_basic_slider, Data.monolith_stability_basic_slider_Action);
                    Events.Set_Slider_Event(Data.monolith_stability_empower_slider, Data.monolith_stability_empower_slider_Action);
                    Events.Set_Slider_Event(Data.monolith_corruption_slider, Data.monolith_corruption_slider_Action);
                    Events.Set_Slider_Event(Data.monolith_gaze_slider, Data.monolith_gaze_slider_Action);

                    Events.Set_Button_Event(Data.save_button, Data.Save_OnClick_Action);
                }
                public static void Set_Active(bool show)
                {
                    if (!content_obj.IsNullOrDestroyed())
                    {
                        content_obj.active = show;
                        enable = show;
                    }
                }
                public static void Toggle_Active()
                {
                    if (!content_obj.IsNullOrDestroyed())
                    {
                        bool show = !content_obj.active;
                        content_obj.active = show;
                        enable = show;
                    }
                }
                public static bool Init_UserData()
                {
                    bool result = false;
                    if (!Save_Manager.instance.IsNullOrDestroyed())
                    {
                        if ((Save_Manager.instance.initialized) && (!Save_Manager.instance.data.IsNullOrDestroyed()))
                        {
                            //Login
                            Login.auto_click_play_offline_toggle.isOn = Save_Manager.instance.data.Login.Enable_AutoLoginOffline;
                            Login.auto_select_char_toggle.isOn = Save_Manager.instance.data.Login.Enable_AutoSelectChar;

                            //Content.Character.Cheats
                            Cheats.godmode_toggle.isOn = Save_Manager.instance.data.Character.Cheats.Enable_GodMode;
                            Cheats.lowlife_toggle.isOn = Save_Manager.instance.data.Character.Cheats.Enable_LowLife;
                            Cheats.allow_choosing_blessing.isOn = Save_Manager.instance.data.Character.Cheats.Enable_CanChooseBlessing;
                            Cheats.unlock_all_idols.isOn = Save_Manager.instance.data.Character.Cheats.Enable_UnlockAllIdolsSlots;
                            
                            Cheats.autoPot_toggle.isOn = Save_Manager.instance.data.Character.Cheats.Enable_AutoPot;
                            Cheats.autopot_slider.value = Save_Manager.instance.data.Character.Cheats.autoPot;

                            Cheats.density_toggle.isOn = Save_Manager.instance.data.Character.Cheats.Enable_DensityMultiplier;
                            Cheats.density_slider.value = Save_Manager.instance.data.Character.Cheats.DensityMultiplier;

                            Cheats.experience_toggle.isOn = Save_Manager.instance.data.Character.Cheats.Enable_ExperienceMultiplier;
                            Cheats.experience_slider.value = Save_Manager.instance.data.Character.Cheats.ExperienceMultiplier;

                            Cheats.ability_toggle.isOn = Save_Manager.instance.data.Character.Cheats.Enable_AbilityMultiplier;
                            Cheats.ability_slider.value = Save_Manager.instance.data.Character.Cheats.AbilityMultiplier;

                            Cheats.favor_toggle.isOn = Save_Manager.instance.data.Character.Cheats.Enable_FavorMultiplier;
                            Cheats.favor_slider.value = Save_Manager.instance.data.Character.Cheats.FavorMultiplier;

                            Cheats.itemdropmultiplier_toggle.isOn = Save_Manager.instance.data.Character.Cheats.Enable_ItemDropMultiplier;
                            Cheats.itemdropmultiplier_slider.value = Save_Manager.instance.data.Character.Cheats.ItemDropMultiplier;

                            Cheats.itemdropchance_toggle.isOn = Save_Manager.instance.data.Character.Cheats.Enable_ItemDropChance;
                            Cheats.itemdropchance_slider.value = Save_Manager.instance.data.Character.Cheats.ItemDropChance;

                            Cheats.golddropmultiplier_toggle.isOn = Save_Manager.instance.data.Character.Cheats.Enable_GoldDropMultiplier;
                            Cheats.golddropmultiplier_slider.value = Save_Manager.instance.data.Character.Cheats.GoldDropMultiplier;

                            Cheats.golddropchance_toggle.isOn = Save_Manager.instance.data.Character.Cheats.Enable_GoldDropChance;
                            Cheats.golddropchance_slider.value = Save_Manager.instance.data.Character.Cheats.GoldDropChance;

                            Cheats.waypoints_toggle.isOn = Save_Manager.instance.data.Character.Cheats.Enable_WaypointsUnlock;

                            //Content.Character.Buffs
                            Buffs.enable_mod.isOn = Save_Manager.instance.data.Character.PermanentBuffs.Enable_Mod;

                            Buffs.movespeed_toggle.isOn = Save_Manager.instance.data.Character.PermanentBuffs.Enable_MoveSpeed_Buff;
                            Buffs.movespeed_slider.value = Save_Manager.instance.data.Character.PermanentBuffs.MoveSpeed_Buff_Value;

                            Buffs.damage_toggle.isOn = Save_Manager.instance.data.Character.PermanentBuffs.Enable_Damage_Buff;
                            Buffs.damage_slider.value = Save_Manager.instance.data.Character.PermanentBuffs.Damage_Buff_Value;

                            Buffs.attackspeed_toggle.isOn = Save_Manager.instance.data.Character.PermanentBuffs.Enable_AttackSpeed_Buff;
                            Buffs.attackspeed_slider.value = Save_Manager.instance.data.Character.PermanentBuffs.AttackSpeed_Buff_Value;

                            Buffs.castingspeed_toggle.isOn = Save_Manager.instance.data.Character.PermanentBuffs.Enable_CastSpeed_Buff;
                            Buffs.castingspeed_slider.value = Save_Manager.instance.data.Character.PermanentBuffs.CastSpeed_Buff_Value;

                            Buffs.criticalchance_toggle.isOn = Save_Manager.instance.data.Character.PermanentBuffs.Enable_CriticalChance_Buff;
                            Buffs.criticalchance_slider.value = Save_Manager.instance.data.Character.PermanentBuffs.CriticalChance_Buff_Value;

                            Buffs.criticalmultiplier_toggle.isOn = Save_Manager.instance.data.Character.PermanentBuffs.Enable_CriticalMultiplier_Buff;
                            Buffs.criticalmultiplier_slider.value = Save_Manager.instance.data.Character.PermanentBuffs.CriticalMultiplier_Buff_Value;

                            Buffs.healthregen_toggle.isOn = Save_Manager.instance.data.Character.PermanentBuffs.Enable_HealthRegen_Buff;
                            Buffs.healthregen_slider.value = Save_Manager.instance.data.Character.PermanentBuffs.HealthRegen_Buff_Value;

                            Buffs.manaregen_toggle.isOn = Save_Manager.instance.data.Character.PermanentBuffs.Enable_ManaRegen_Buff;
                            Buffs.manaregen_slider.value = Save_Manager.instance.data.Character.PermanentBuffs.ManaRegen_Buff_Value;

                            Buffs.str_toggle.isOn = Save_Manager.instance.data.Character.PermanentBuffs.Enable_Str_Buff;
                            Buffs.str_slider.value = Save_Manager.instance.data.Character.PermanentBuffs.Str_Buff_Value;

                            Buffs.int_toggle.isOn = Save_Manager.instance.data.Character.PermanentBuffs.Enable_Int_Buff;
                            Buffs.int_slider.value = Save_Manager.instance.data.Character.PermanentBuffs.Int_Buff_Value;

                            Buffs.dex_toggle.isOn = Save_Manager.instance.data.Character.PermanentBuffs.Enable_Dex_Buff;
                            Buffs.dex_slider.value = Save_Manager.instance.data.Character.PermanentBuffs.Dex_Buff_Value;

                            Buffs.vit_toggle.isOn = Save_Manager.instance.data.Character.PermanentBuffs.Enable_Vit_Buff;
                            Buffs.vit_slider.value = Save_Manager.instance.data.Character.PermanentBuffs.Vit_Buff_Value;

                            Buffs.att_toggle.isOn = Save_Manager.instance.data.Character.PermanentBuffs.Enable_Att_Buff;
                            Buffs.att_slider.value = Save_Manager.instance.data.Character.PermanentBuffs.Att_Buff_Value;

                            controls_initialized = true;
                            result = true;
                        }
                    }

                    return result;
                }
                public static void Update_PlayerData()
                {
                    need_update = false;
                    // Login - Auto Select Char
                    Login.AddAutoSelectCharactersToDropdown();

                    if ((!Refs_Manager.player_treedata.IsNullOrDestroyed()) &&
                        (!Refs_Manager.character_class_list.IsNullOrDestroyed()) &&
                        (!Refs_Manager.player_data.IsNullOrDestroyed()))
                    {
                        Il2CppSystem.Collections.Generic.List<Dropdown.OptionData> options = new Il2CppSystem.Collections.Generic.List<Dropdown.OptionData>();
                        foreach (CharacterClass char_class in Refs_Manager.character_class_list.classes)
                        {
                            options.Add(new Dropdown.OptionData { text = char_class.className });
                        }
                        Data.class_dropdown.options = options;

                        Data.class_dropdown.value = Refs_Manager.player_data.CharacterClass;
                        Data.died_toggle.isOn = Refs_Manager.player_data.Died;
                        Data.deaths_slider.value = Refs_Manager.player_data.Deaths;
                        Data.deaths_text.text = Refs_Manager.player_data.Deaths.ToString();
                        Data.hardcore_toggle.isOn = Refs_Manager.player_data.Hardcore;
                        Data.masochist_toggle.isOn = Refs_Manager.player_data.Masochist;
                        Data.portal_toggle.isOn = Refs_Manager.player_data.PortalUnlocked;
                        Data.solo_toggle.isOn = Refs_Manager.player_data.SoloChallenge;
                        Data.lantern_slider.value = Refs_Manager.player_data.LanternLuminance;
                        Data.lantern_text.text = Refs_Manager.player_data.LanternLuminance.ToString();
                        Data.soul_slider.value = Refs_Manager.player_data.SoulEmbers;
                        Data.soul_text.text = Refs_Manager.player_data.SoulEmbers.ToString();
                    }
                }
                public static void Update_Monoliths_Data()
                {                    
                    if ((!Refs_Manager.player_data.IsNullOrDestroyed()) && (!Data.monolith_dropdown.IsNullOrDestroyed()))
                    {                        
                        int index = Data.monolith_dropdown.value;
                        if (index < 1)
                        {
                            int value = -1;
                            Data.monolith_stability_basic_go.active = false;
                            Data.monolith_stability_basic_slider.value = value;
                            Data.monolith_stability_basic_text.text = value.ToString();

                            Data.monolith_stability_empower_go.active = false;
                            Data.monolith_stability_empower_slider.value = value;
                            Data.monolith_stability_empower_text.text = value.ToString();

                            Data.monolith_corruption_go.active = false;
                            Data.monolith_corruption_slider.value = value;
                            Data.monolith_corruption_text.text = value.ToString();

                            Data.monolith_gaze_go.active = false;
                            Data.monolith_gaze_slider.value = value;
                            Data.monolith_gaze_text.text = value.ToString();
                        }
                        else
                        {
                            SavedMonolithRun basic = null;
                            SavedMonolithRun empower = null;
                            foreach (SavedMonolithRun run in Refs_Manager.player_data.MonolithRuns)
                            {
                                if (run.TimelineID == index)
                                {
                                    if (run.DifficultyIndex == 0) { basic = run; }
                                    else { empower = run; }
                                }
                            }

                            if (!basic.IsNullOrDestroyed())
                            {
                                Data.monolith_stability_basic_go.active = true;
                                int value = basic.Stability;
                                Data.monolith_stability_basic_slider.value = value;
                                Data.monolith_stability_basic_text.text = value.ToString();
                            }
                            else
                            {
                                Data.monolith_stability_basic_go.active = false;
                                int value = -1;
                                Data.monolith_stability_basic_slider.value = value;
                                Data.monolith_stability_basic_text.text = value.ToString();
                            }

                            if (!empower.IsNullOrDestroyed())
                            {
                                Data.monolith_stability_empower_go.active = true;
                                int value = empower.Stability;
                                Data.monolith_stability_empower_slider.value = value;
                                Data.monolith_stability_empower_text.text = value.ToString();
                                if (!empower.SavedEchoWeb.IsNullOrDestroyed())
                                {
                                    Data.monolith_corruption_go.active = true;
                                    int value2 = empower.SavedEchoWeb.Corruption;
                                    Data.monolith_corruption_slider.value = value2;
                                    Data.monolith_corruption_text.text = value2.ToString();

                                    Data.monolith_gaze_go.active = true;
                                    int value3 = empower.SavedEchoWeb.GazeOfOrobyss;
                                    Data.monolith_gaze_slider.value = value3;
                                    Data.monolith_gaze_text.text = value3.ToString();
                                }
                                else
                                {
                                    Data.monolith_corruption_go.active = false;
                                    int value2 = -1;
                                    Data.monolith_corruption_slider.value = value2;
                                    Data.monolith_corruption_text.text = value2.ToString();

                                    Data.monolith_gaze_go.active = false;
                                    int value3 = -1;
                                    Data.monolith_gaze_slider.value = value3;
                                    Data.monolith_gaze_text.text = value3.ToString();
                                }
                            }
                            else
                            {
                                Data.monolith_stability_empower_go.active = false;
                                int value = -1;
                                Data.monolith_stability_empower_slider.value = value;
                                Data.monolith_stability_empower_text.text = value.ToString();
                            }
                        }
                    }
                }
                public static void UpdateVisuals()
                {
                    if ((!Save_Manager.instance.IsNullOrDestroyed()) && (controls_initialized))
                    {
                        if ((Save_Manager.instance.initialized) && (!Save_Manager.instance.data.IsNullOrDestroyed()))
                        {
                            Cheats.autopot_text.text = (int)((Save_Manager.instance.data.Character.Cheats.autoPot / 255) * 100) + " %";
                            Cheats.density_text.text = "x " + (int)(Save_Manager.instance.data.Character.Cheats.DensityMultiplier);
                            Cheats.experience_text.text = "x " + (int)(Save_Manager.instance.data.Character.Cheats.ExperienceMultiplier);
                            Cheats.ability_text.text = "x " + (int)(Save_Manager.instance.data.Character.Cheats.AbilityMultiplier);
                            Cheats.favor_text.text = "x " + (int)(Save_Manager.instance.data.Character.Cheats.FavorMultiplier);
                            Cheats.itemdropmultiplier_text.text = "x " + (int)(Save_Manager.instance.data.Character.Cheats.ItemDropMultiplier);
                            Cheats.itemdropchance_text.text = "+ " + (int)((Save_Manager.instance.data.Character.Cheats.ItemDropChance / 255) * 100) + " %";
                            Cheats.golddropmultiplier_text.text = "x " + (int)(Save_Manager.instance.data.Character.Cheats.GoldDropMultiplier);
                            Cheats.golddropchance_text.text = "+ " + (int)((Save_Manager.instance.data.Character.Cheats.GoldDropChance / 255) * 100) + " %";
                            
                            Buffs.movespeed_text.text = "+ " + (int)(Save_Manager.instance.data.Character.PermanentBuffs.MoveSpeed_Buff_Value * 100) + " %";
                            Buffs.damage_text.text = "+ " + (int)(Save_Manager.instance.data.Character.PermanentBuffs.Damage_Buff_Value * 100) + " %";
                            Buffs.attackspeed_text.text = "+ " + (int)(Save_Manager.instance.data.Character.PermanentBuffs.AttackSpeed_Buff_Value * 100) + " %";
                            Buffs.castingspeed_text.text = "+ " + (int)(Save_Manager.instance.data.Character.PermanentBuffs.CastSpeed_Buff_Value * 100) + " %";
                            int crit_chance = 0;
                            if (Save_Manager.instance.data.Character.PermanentBuffs.CriticalChance_Buff_Value > 0)
                            {
                                crit_chance = (int)(Save_Manager.instance.data.Character.PermanentBuffs.CriticalChance_Buff_Value * 100) + 1;
                            }
                            Buffs.criticalchance_text.text = "+ " + crit_chance + " %";
                            Buffs.criticalmultiplier_text.text = "+ " + (int)(Save_Manager.instance.data.Character.PermanentBuffs.CriticalMultiplier_Buff_Value * 100) + " %";
                            Buffs.healthregen_text.text = "+ " + (int)(Save_Manager.instance.data.Character.PermanentBuffs.HealthRegen_Buff_Value * 100) + " %";
                            Buffs.manaregen_text.text = "+ " + (int)(Save_Manager.instance.data.Character.PermanentBuffs.ManaRegen_Buff_Value * 100) + " %";
                            Buffs.str_text.text = "+ " + (int)(Save_Manager.instance.data.Character.PermanentBuffs.Str_Buff_Value) + " %";
                            Buffs.int_text.text = "+ " + (int)(Save_Manager.instance.data.Character.PermanentBuffs.Int_Buff_Value) + " %";
                            Buffs.dex_text.text = "+ " + (int)(Save_Manager.instance.data.Character.PermanentBuffs.Dex_Buff_Value) + " %";
                            Buffs.vit_text.text = "+ " + (int)(Save_Manager.instance.data.Character.PermanentBuffs.Vit_Buff_Value) + " %";
                            Buffs.att_text.text = "+ " + (int)(Save_Manager.instance.data.Character.PermanentBuffs.Att_Buff_Value) + " %";
                        }
                    }
                }

                public class Cheats
                {
                    // BUG: For some reason the game always return true in action delegates
                    public static Toggle godmode_toggle = null;
                    public static readonly System.Action<bool> Godmode_Toggle_Action = new System.Action<bool>(Set_Godmode_Enable);
                    private static void Set_Godmode_Enable(bool enable)
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_GodMode = godmode_toggle.isOn;
                    }

                    public static Toggle lowlife_toggle = null;
                    public static readonly System.Action<bool> Lowlife_Toggle_Action = new System.Action<bool>(Set_Lowlife_Enable);
                    private static void Set_Lowlife_Enable(bool enable)
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_LowLife = lowlife_toggle.isOn;
                    }

                    public static Toggle allow_choosing_blessing = null;
                    public static readonly System.Action<bool> AllowChooseBlessings_Toggle_Action = new System.Action<bool>(Set_AllowChooseBlessings_Enable);
                    private static void Set_AllowChooseBlessings_Enable(bool enable)
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_CanChooseBlessing = allow_choosing_blessing.isOn;
                    }

                    public static Toggle unlock_all_idols = null;
                    public static readonly System.Action<bool> UnlockAllIdols_Toggle_Action = new System.Action<bool>(Set_UnlockAllIdols_Enable);
                    private static void Set_UnlockAllIdols_Enable(bool enable)
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_UnlockAllIdolsSlots = unlock_all_idols.isOn;
                        Mods.Character.Character_UnlockAllIdols.Update();
                    }

                    public static Toggle autoPot_toggle = null;
                    public static readonly System.Action<bool> AutoPot_Toggle_Action = new System.Action<bool>(Set_AutoPot_Enable);
                    private static void Set_AutoPot_Enable(bool enable)
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_AutoPot = autoPot_toggle.isOn;
                    }
                    public static Text autopot_text = null;
                    public static Slider autopot_slider = null;
                                        
                    public static Toggle density_toggle = null;
                    public static readonly System.Action<bool> Density_Toggle_Action = new System.Action<bool>(Set_Density_Enable);
                    private static void Set_Density_Enable(bool enable)
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_DensityMultiplier = density_toggle.isOn;
                    }
                    public static Text density_text = null;
                    public static Slider density_slider = null;
                    
                    public static Toggle experience_toggle = null;
                    public static readonly System.Action<bool> Experience_Toggle_Action = new System.Action<bool>(Set_Experience_Enable);
                    private static void Set_Experience_Enable(bool enable)
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_ExperienceMultiplier = experience_toggle.isOn;
                    }
                    public static Text experience_text = null;
                    public static Slider experience_slider = null;
                    
                    public static Toggle ability_toggle = null;
                    public static readonly System.Action<bool> Ability_Toggle_Action = new System.Action<bool>(Set_Ability_Enable);
                    private static void Set_Ability_Enable(bool enable)
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_AbilityMultiplier = ability_toggle.isOn;
                    }
                    public static Text ability_text = null;
                    public static Slider ability_slider = null;
                    
                    public static Toggle favor_toggle = null;
                    public static readonly System.Action<bool> Favor_Toggle_Action = new System.Action<bool>(Set_Favor_Enable);
                    private static void Set_Favor_Enable(bool enable)
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_FavorMultiplier = favor_toggle.isOn;
                    }
                    public static Text favor_text = null;
                    public static Slider favor_slider = null;
                    
                    public static Toggle itemdropmultiplier_toggle = null;
                    public static readonly System.Action<bool> ItemDropMulti_Toggle_Action = new System.Action<bool>(Set_ItemDropMulti_Enable);
                    private static void Set_ItemDropMulti_Enable(bool enable)
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_ItemDropMultiplier = itemdropmultiplier_toggle.isOn;
                    }
                    public static Text itemdropmultiplier_text = null;
                    public static Slider itemdropmultiplier_slider = null;
                    
                    public static Toggle itemdropchance_toggle = null;
                    public static readonly System.Action<bool> ItemDropChance_Toggle_Action = new System.Action<bool>(Set_ItemDropChance_Enable);
                    private static void Set_ItemDropChance_Enable(bool enable)
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_ItemDropChance = itemdropchance_toggle.isOn;
                    }
                    public static Text itemdropchance_text = null;
                    public static Slider itemdropchance_slider = null;
                    
                    public static Toggle golddropmultiplier_toggle = null;
                    public static readonly System.Action<bool> GoldMulti_Toggle_Action = new System.Action<bool>(Set_GoldMulti_Enable);
                    private static void Set_GoldMulti_Enable(bool enable)
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_GoldDropMultiplier = golddropmultiplier_toggle.isOn;
                    }
                    public static Text golddropmultiplier_text = null;
                    public static Slider golddropmultiplier_slider = null;
                    
                    public static Toggle golddropchance_toggle = null;
                    public static readonly System.Action<bool> GoldChance_Toggle_Action = new System.Action<bool>(Set_GoldChance_Enable);
                    private static void Set_GoldChance_Enable(bool enable)
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_GoldDropChance = golddropchance_toggle.isOn;
                    }
                    public static Text golddropchance_text = null;
                    public static Slider golddropchance_slider = null;

                    public static Toggle waypoints_toggle = null;
                    public static readonly System.Action<bool> Waypoints_Toggle_Action = new System.Action<bool>(Set_Waypoints_Enable);
                    private static void Set_Waypoints_Enable(bool enable)
                    {
                        Save_Manager.instance.data.Character.Cheats.Enable_WaypointsUnlock = waypoints_toggle.isOn;
                    }

                    public static Button level_once_button = null;
                    public static readonly System.Action LevelUpOnce_OnClick_Action = new System.Action(LevelUpOnce_Click);
                    public static void LevelUpOnce_Click()
                    {
                        Mods.Character.Character_Level.LevelUpOnce();
                    }

                    public static Button level_max_button = null;
                    public static readonly System.Action LevelUpMax_OnClick_Action = new System.Action(LevelUpMax_Click);
                    public static void LevelUpMax_Click()
                    {
                        Mods.Character.Character_Level.LevelUptoMax();
                    }

                    public static Button complete_quest_button = null;
                    public static readonly System.Action CompleteQuest_OnClick_Action = new System.Action(CompleteQuest_Click);
                    public static void CompleteQuest_Click()
                    {
                        Mods.Character.Character_MainQuest.Complete();
                    }

                    public static Button masterie_buttons = null;
                    public static readonly System.Action Masteries_OnClick_Action = new System.Action(Masteries_Click);
                    public static void Masteries_Click()
                    {
                        Mods.Character.Character_Masteries.ResetChooseMasterie();
                    }
                    public static Text masterie_text = null;

                    public static Button add_runes_button = null;
                    public static readonly System.Action AddRunes_OnClick_Action = new System.Action(AddRunes_Click);
                    public static void AddRunes_Click()
                    {
                        Mods.Character.Character_Materials.GetAllRunesX99();
                    }

                    public static Button add_glyphs_button = null;
                    public static readonly System.Action AddGlyphs_OnClick_Action = new System.Action(AddGlyphs_Click);
                    public static void AddGlyphs_Click()
                    {
                        Mods.Character.Character_Materials.GetAllGlyphsX99();
                    }

                    public static Button add_shards_button = null;
                    public static readonly System.Action AddAffixs_OnClick_Action = new System.Action(AddAffixs_Click);
                    public static void AddAffixs_Click()
                    {
                        Mods.Character.Character_Materials.GetAllShardsX10();
                    }

                    public static Button discover_blessings_button = null;
                    public static readonly System.Action DiscoverAllBlessings_OnClick_Action = new System.Action(DiscoverAllBlessings_Click);
                    public static void DiscoverAllBlessings_Click()
                    {
                        Mods.Character.Character_Blessings.DiscoverAllBlessings();
                    }
                }
                public class Data
                {
                    public static Dropdown class_dropdown = null;
                    public static Toggle died_toggle = null;
                    public static Toggle hardcore_toggle = null;
                    public static Toggle masochist_toggle = null;
                    public static Toggle portal_toggle = null;
                    public static Toggle solo_toggle = null;
                    public static Text deaths_text = null;
                    public static Slider deaths_slider = null;
                    public static Text lantern_text = null;
                    public static Slider lantern_slider = null;
                    public static Text soul_text = null;
                    public static Slider soul_slider = null;

                    public static Dropdown monolith_dropdown = null;
                    public static GameObject monolith_stability_basic_go = null;
                    public static Text monolith_stability_basic_text = null;
                    public static Slider monolith_stability_basic_slider = null;
                    public static readonly System.Action<float> monolith_stability_basic_slider_Action = new System.Action<float>(Set_monolith_stability_basic);
                    public static void Set_monolith_stability_basic(float f)
                    {
                        int result = System.Convert.ToInt32(monolith_stability_basic_slider.value);
                        int index = monolith_dropdown.value;
                        if (!Refs_Manager.player_data.IsNullOrDestroyed())
                        {
                            foreach (SavedMonolithRun run in Refs_Manager.player_data.MonolithRuns)
                            {
                                if ((run.TimelineID == index) && (run.DifficultyIndex == 0))
                                {
                                    if (run.Stability != result) { run.Stability = result; }
                                    break;
                                }
                            }
                        }
                        monolith_stability_basic_text.text = result.ToString();
                    }


                    public static GameObject monolith_stability_empower_go = null;
                    public static Text monolith_stability_empower_text = null;
                    public static Slider monolith_stability_empower_slider = null;
                    public static readonly System.Action<float> monolith_stability_empower_slider_Action = new System.Action<float>(Set_monolith_stability_empower);
                    public static void Set_monolith_stability_empower(float f)
                    {
                        int result = System.Convert.ToInt32(monolith_stability_empower_slider.value);
                        int index = monolith_dropdown.value;
                        if (!Refs_Manager.player_data.IsNullOrDestroyed())
                        {
                            foreach (SavedMonolithRun run in Refs_Manager.player_data.MonolithRuns)
                            {
                                if ((run.TimelineID == index) && (run.DifficultyIndex == 1))
                                {
                                    if (run.Stability != result) { run.Stability = result; }
                                    break;
                                }
                            }
                        }
                        monolith_stability_empower_text.text = result.ToString();
                    }


                    public static GameObject monolith_corruption_go = null;
                    public static Text monolith_corruption_text = null;
                    public static Slider monolith_corruption_slider = null;
                    public static readonly System.Action<float> monolith_corruption_slider_Action = new System.Action<float>(Set_monolith_corruption_empower);
                    public static void Set_monolith_corruption_empower(float f)
                    {
                        int result = System.Convert.ToInt32(monolith_corruption_slider.value);
                        int index = monolith_dropdown.value;
                        if (!Refs_Manager.player_data.IsNullOrDestroyed())
                        {
                            foreach (SavedMonolithRun run in Refs_Manager.player_data.MonolithRuns)
                            {
                                if ((run.TimelineID == index) && (run.DifficultyIndex == 1))
                                {
                                    if (!run.SavedEchoWeb.IsNullOrDestroyed())
                                    {
                                        if (run.SavedEchoWeb.Corruption != result) { run.SavedEchoWeb.Corruption = result; }
                                    }
                                    break;
                                }
                            }
                        }
                        monolith_corruption_text.text = result.ToString();
                    }

                    public static GameObject monolith_gaze_go = null;
                    public static Text monolith_gaze_text = null;
                    public static Slider monolith_gaze_slider = null;
                    public static readonly System.Action<float> monolith_gaze_slider_Action = new System.Action<float>(Set_monolith_gaze_empower);
                    public static void Set_monolith_gaze_empower(float f)
                    {
                        int result = System.Convert.ToInt32(monolith_gaze_slider.value);
                        int index = monolith_dropdown.value;
                        if (!Refs_Manager.player_data.IsNullOrDestroyed())
                        {
                            foreach (SavedMonolithRun run in Refs_Manager.player_data.MonolithRuns)
                            {
                                if ((run.TimelineID == index) && (run.DifficultyIndex == 1))
                                {
                                    if (!run.SavedEchoWeb.IsNullOrDestroyed())
                                    {
                                        if (run.SavedEchoWeb.GazeOfOrobyss != result) { run.SavedEchoWeb.GazeOfOrobyss = result; }
                                    }
                                    break;
                                }
                            }
                        }
                        monolith_gaze_text.text = result.ToString();
                    }


                    public static Button save_button = null;

                    public static readonly System.Action Save_OnClick_Action = new System.Action(Save_Click);
                    public static void Save_Click()
                    {
                        if (!Refs_Manager.player_data.IsNullOrDestroyed()) { Refs_Manager.player_data.SaveData(); }
                    }
                }
                public class Buffs
                {
                    public static Toggle enable_mod = null;

                    public static Toggle movespeed_toggle = null;
                    public static Text movespeed_text = null;
                    public static Slider movespeed_slider = null;

                    public static Toggle damage_toggle = null;
                    public static Text damage_text = null;
                    public static Slider damage_slider = null;

                    public static Toggle attackspeed_toggle = null;
                    public static Text attackspeed_text = null;
                    public static Slider attackspeed_slider = null;

                    public static Toggle castingspeed_toggle = null;
                    public static Text castingspeed_text = null;
                    public static Slider castingspeed_slider = null;

                    public static Toggle criticalchance_toggle = null;
                    public static Text criticalchance_text = null;
                    public static Slider criticalchance_slider = null;

                    public static Toggle criticalmultiplier_toggle = null;
                    public static Text criticalmultiplier_text = null;
                    public static Slider criticalmultiplier_slider = null;

                    public static Toggle healthregen_toggle = null;
                    public static Text healthregen_text = null;
                    public static Slider healthregen_slider = null;

                    public static Toggle manaregen_toggle = null;
                    public static Text manaregen_text = null;
                    public static Slider manaregen_slider = null;

                    public static Toggle str_toggle = null;
                    public static Text str_text = null;
                    public static Slider str_slider = null;

                    public static Toggle int_toggle = null;
                    public static Text int_text = null;
                    public static Slider int_slider = null;

                    public static Toggle dex_toggle = null;
                    public static Text dex_text = null;
                    public static Slider dex_slider = null;

                    public static Toggle vit_toggle = null;
                    public static Text vit_text = null;
                    public static Slider vit_slider = null;

                    public static Toggle att_toggle = null;
                    public static Text att_text = null;
                    public static Slider att_slider = null;
                }
            }
            public class Items
            {
                public static GameObject content_obj = null;
                public static bool controls_initialized = false;
                public static bool enable = false;

                public static void Get_Refs()
                {
                    content_obj = Functions.GetChild(Content.content_obj, "Items_Content");
                    if (!content_obj.IsNullOrDestroyed())
                    {
                        GameObject items_drop_content = Functions.GetViewportContent(content_obj, "Items_Drop", "Items_Data_Content");
                        if (!items_drop_content.IsNullOrDestroyed())
                        {
                            Drop.force_unique_toggle = Functions.Get_ToggleInPanel(items_drop_content, "ForceUnique", "Toggle_Items_Drop_ForceUnique");
                            Drop.force_set_toggle = Functions.Get_ToggleInPanel(items_drop_content, "ForceSet", "Toggle_Items_Drop_ForceSet");
                            Drop.force_legendary_toggle = Functions.Get_ToggleInPanel(items_drop_content, "ForceLegendary", "Toggle_Items_Drop_ForceLegendary");

                            Drop.implicits_toggle = Functions.Get_ToggleInPanel(items_drop_content, "Implicits", "Toggle_Items_Drop_Implicits");
                            Drop.implicits_text = Functions.Get_TextInToggle(items_drop_content, "Implicits", "Toggle_Items_Drop_Implicits", "Value");
                            Drop.implicits_slider_min = Functions.Get_SliderInPanel(items_drop_content, "Implicits", "Slider_Items_Drop_Implicits_Min");
                            Drop.implicits_slider_max = Functions.Get_SliderInPanel(items_drop_content, "Implicits", "Slider_Items_Drop_Implicits_Max");

                            Drop.forgin_potencial_toggle = Functions.Get_ToggleInPanel(items_drop_content, "ForginPotencial", "Toggle_Items_Drop_ForginPotencial");
                            Drop.forgin_potencial_text = Functions.Get_TextInToggle(items_drop_content, "ForginPotencial", "Toggle_Items_Drop_ForginPotencial", "Value");
                            Drop.forgin_potencial_slider_min = Functions.Get_SliderInPanel(items_drop_content, "ForginPotencial", "Slider_Items_Drop_ForginPotencial_Min");
                            Drop.forgin_potencial_slider_max = Functions.Get_SliderInPanel(items_drop_content, "ForginPotencial", "Slider_Items_Drop_ForginPotencial_Max");

                            Drop.force_seal_toggle = Functions.Get_ToggleInPanel(items_drop_content, "ForceSeal", "Toggle_Items_Drop_ForceSeal");

                            Drop.seal_tier_toggle = Functions.Get_ToggleInPanel(items_drop_content, "SealTier", "Toggle_Items_Drop_SealTier");
                            Drop.seal_tier_text = Functions.Get_TextInToggle(items_drop_content, "SealTier", "Toggle_Items_Drop_SealTier", "Value");
                            Drop.seal_tier_slider_min = Functions.Get_SliderInPanel(items_drop_content, "SealTier", "Slider_Items_Drop_SealTier_Min");
                            Drop.seal_tier_slider_max = Functions.Get_SliderInPanel(items_drop_content, "SealTier", "Slider_Items_Drop_SealTier_Max");

                            Drop.seal_value_toggle = Functions.Get_ToggleInPanel(items_drop_content, "SealValue", "Toggle_Items_Drop_SealValue");
                            Drop.seal_value_text = Functions.Get_TextInToggle(items_drop_content, "SealValue", "Toggle_Items_Drop_SealValue", "Value");
                            Drop.seal_value_slider_min = Functions.Get_SliderInPanel(items_drop_content, "SealValue", "Slider_Items_Drop_SealValue_Min");
                            Drop.seal_value_slider_max = Functions.Get_SliderInPanel(items_drop_content, "SealValue", "Slider_Items_Drop_SealValue_Max");

                            Drop.affix_count_toggle = Functions.Get_ToggleInPanel(items_drop_content, "NbAffixes", "Toggle_Items_Drop_NbAffixes");
                            Drop.affix_count_text = Functions.Get_TextInToggle(items_drop_content, "NbAffixes", "Toggle_Items_Drop_NbAffixes", "Value");
                            Drop.affix_count_slider_min = Functions.Get_SliderInPanel(items_drop_content, "NbAffixes", "Slider_Items_Drop_NbAffixes_Min");
                            Drop.affix_count_slider_min.maxValue = 6;                            
                            Drop.affix_count_slider_max = Functions.Get_SliderInPanel(items_drop_content, "NbAffixes", "Slider_Items_Drop_NbAffixes_Max");
                            Drop.affix_count_slider_max.maxValue = 6;

                            Drop.affix_tiers_toggle = Functions.Get_ToggleInPanel(items_drop_content, "AffixesTiers", "Toggle_Items_Drop_AffixesTiers");
                            Drop.affix_tiers_text = Functions.Get_TextInToggle(items_drop_content, "AffixesTiers", "Toggle_Items_Drop_AffixesTiers", "Value");
                            Drop.affix_tiers_slider_min = Functions.Get_SliderInPanel(items_drop_content, "AffixesTiers", "Slider_Items_Drop_AffixesTiers_Min");
                            Drop.affix_tiers_slider_max = Functions.Get_SliderInPanel(items_drop_content, "AffixesTiers", "Slider_Items_Drop_AffixesTiers_Max");

                            Drop.affix_values_toggle = Functions.Get_ToggleInPanel(items_drop_content, "AffixesValues", "Toggle_Items_Drop_AffixesValues");
                            Drop.affix_values_text = Functions.Get_TextInToggle(items_drop_content, "AffixesValues", "Toggle_Items_Drop_AffixesValues", "Value");
                            Drop.affix_values_slider_min = Functions.Get_SliderInPanel(items_drop_content, "AffixesValues", "Slider_Items_Drop_AffixesValues_Min");
                            Drop.affix_values_slider_max = Functions.Get_SliderInPanel(items_drop_content, "AffixesValues", "Slider_Items_Drop_AffixesValues_Max");

                            Drop.unique_mods_toggle = Functions.Get_ToggleInPanel(items_drop_content, "UniqueMods", "Toggle_Items_Drop_UniqueMods");
                            Drop.unique_mods_text = Functions.Get_TextInToggle(items_drop_content, "UniqueMods", "Toggle_Items_Drop_UniqueMods", "Value");
                            Drop.unique_mods_slider_min = Functions.Get_SliderInPanel(items_drop_content, "UniqueMods", "Slider_Items_Drop_UniqueMods_Min");
                            Drop.unique_mods_slider_max = Functions.Get_SliderInPanel(items_drop_content, "UniqueMods", "Slider_Items_Drop_UniqueMods_Max");

                            Drop.legendary_potencial_toggle = Functions.Get_ToggleInPanel(items_drop_content, "LegendaryPotencial", "Toggle_Items_Drop_LegendaryPotencial");
                            Drop.legendary_potencial_text = Functions.Get_TextInToggle(items_drop_content, "LegendaryPotencial", "Toggle_Items_Drop_LegendaryPotencial", "Value");
                            Drop.legendary_potencial_slider_min = Functions.Get_SliderInPanel(items_drop_content, "LegendaryPotencial", "Slider_Items_Drop_LegendaryPotencial_Min");
                            Drop.legendary_potencial_slider_max = Functions.Get_SliderInPanel(items_drop_content, "LegendaryPotencial", "Slider_Items_Drop_LegendaryPotencial_Max");

                            Drop.weaver_will_toggle = Functions.Get_ToggleInPanel(items_drop_content, "WeaverWill", "Toggle_Items_Drop_WeaverWill");
                            Drop.weaver_will_text = Functions.Get_TextInToggle(items_drop_content, "WeaverWill", "Toggle_Items_Drop_WeaverWill", "Value");
                            Drop.weaver_will_slider_min = Functions.Get_SliderInPanel(items_drop_content, "WeaverWill", "Slider_Items_Drop_WeaverWill_Min");
                            Drop.weaver_will_slider_max = Functions.Get_SliderInPanel(items_drop_content, "WeaverWill", "Slider_Items_Drop_WeaverWill_Max");
                        }
                        
                        GameObject items_pickup_content = Functions.GetViewportContent(content_obj, "Items_Pickup", "Items_Pickup_Content");
                        if (!items_pickup_content.IsNullOrDestroyed())
                        {
                            Pickup.autopickup_gold_toggle = Functions.Get_ToggleInPanel(items_pickup_content, "AutoPickup_Gold", "Toggle_Items_Pickup_AutoPickup_Gold");
                            Pickup.autopickup_keys_toggle = Functions.Get_ToggleInPanel(items_pickup_content, "AutoPickup_Keys", "Toggle_Items_Pickup_AutoPickup_Keys");
                            Pickup.autopickup_potions_toggle = Functions.Get_ToggleInPanel(items_pickup_content, "AutoPickup_Pots", "Toggle_Items_Pickup_AutoPickup_Pots");
                            Pickup.autopickup_xptome_toggle = Functions.Get_ToggleInPanel(items_pickup_content, "AutoPickup_XpTome", "Toggle_Items_Pickup_AutoPickup_XpTome");
                            Pickup.autopickup_materials_toggle = Functions.Get_ToggleInPanel(items_pickup_content, "AutoPickup_Materials", "Toggle_Items_Pickup_AutoPickup_Materials");
                            Pickup.autopickup_fromfilter_toggle = Functions.Get_ToggleInPanel(items_pickup_content, "AutoPickup_Filters", "Toggle_Items_Pickup_AutoPickup_Filters");
                            
                            Pickup.autostore_materials_ondrop_toggle = Functions.Get_ToggleInPanel(items_pickup_content, "AutoStore_OnDrop", "Toggle_Items_Pickup_AutoStore_OnDrop");
                            Pickup.autostore_materials_oninventoryopen_toggle = Functions.Get_ToggleInPanel(items_pickup_content, "AutoStore_OnInventoryOpen", "Toggle_Items_Pickup_AutoStore_OnInventoryOpen");
                            Pickup.autostore_materials_all10sec_toggle = Functions.Get_ToggleInPanel(items_pickup_content, "AutoStore_10sec", "Toggle_Items_Pickup_AutoStore_10sec");
                            
                            Pickup.autosell_hide_toggle = Functions.Get_ToggleInPanel(items_pickup_content, "AutoSell_All_Hide", "Toggle_Items_Pickup_AutoSell_All_Hide");                            
                            
                            Pickup.range_pickup_toggle = Functions.Get_ToggleInPanel(items_pickup_content, "Range_Pickup", "Toggle_Items_Pickup_Range_Pickup");
                            Pickup.hide_materials_notifications_toggle = Functions.Get_ToggleInPanel(items_pickup_content, "Hide_Notifications", "Toggle_Items_Pickup_Hide_Notifications");
                        }

                        GameObject items_req_content = Functions.GetViewportContent(content_obj, "Items_Pickup", "Items_Req_Content");
                        if (!items_req_content.IsNullOrDestroyed())
                        {
                            Requirements.class_req_toggle = Functions.Get_ToggleInPanel(items_req_content, "RemoveReq_Class", "Toggle_RemoveReq_Class");
                            Requirements.level_req_toggle = Functions.Get_ToggleInPanel(items_req_content, "RemoveReq_Level", "Toggle_RemoveReq_Level");
                            Requirements.set_req_toggle = Functions.Get_ToggleInPanel(items_req_content, "RemoveReq_Set", "Toggle_RemoveReq_Set");
                        }
                        else { Main.logger_instance.Error("Requirements"); }

                        GameObject items_forcedrop_content = Functions.GetViewportContent(content_obj, "Items_Pickup", "Items_ForceDrop_Content");
                        if (!items_forcedrop_content.IsNullOrDestroyed())
                        {
                            ForceDrop.forcedrop_type_dropdown = Functions.Get_DopboxInPanel(items_forcedrop_content, "Type", "Dropdown_Items_ForceDrop_Type", new System.Action<int>((_) => { Content.Items.ForceDrop.SelectType(); }));
                            ForceDrop.forcedrop_rarity_dropdown = Functions.Get_DopboxInPanel(items_forcedrop_content, "Rarity", "Dropdown_Items_ForceDrop_Rarity", new System.Action<int>((_) => { Content.Items.ForceDrop.SelectRarity(); }));
                            ForceDrop.forcedrop_items_dropdown = Functions.Get_DopboxInPanel(items_forcedrop_content, "Item", "Dropdown_Items_ForceDrop_Item", new System.Action<int>((_) => { Content.Items.ForceDrop.SelectItem(); }));
                            ForceDrop.forcedrop_quantity_text = Functions.Get_TextInButton(items_forcedrop_content, "Quantity", "Value");
                            ForceDrop.forcedrop_quantity_slider = Functions.Get_SliderInPanel(items_forcedrop_content, "Quantity", "Slider_Items_ForceDrop_Quantity");
                            GameObject new_obj = Functions.GetChild(content_obj, "Items_Pickup");
                            if (!new_obj.IsNullOrDestroyed())
                            {
                                ForceDrop.forcedrop_drop_button = Functions.Get_ButtonInPanel(new_obj, "Btn_Items_ForceDrop_Drop");
                            }
                        }
                        else { Main.logger_instance.Error("Forcedrop"); }

                        CraftingSlot.enable_mod = Functions.Get_ToggleInLabel(content_obj, "Items_Craft", "Toggle_Items_Craft_Enable", makeSureItsActive: true);
                        GameObject items_craft_content = Functions.GetViewportContent(content_obj, "Items_Craft", "Items_Craft_Content");
                        if (!items_craft_content.IsNullOrDestroyed())
                        {
                            CraftingSlot.forgin_potencial_toggle = Functions.Get_ToggleInPanel(items_craft_content, "ForginPotencial", "Toggle_Items_Craft_ForginPotencial");
                            CraftingSlot.forgin_potencial_text = Functions.Get_TextInToggle(items_craft_content, "ForginPotencial", "Toggle_Items_Craft_ForginPotencial", "Value");
                            CraftingSlot.forgin_potencial_slider = Functions.Get_SliderInPanel(items_craft_content, "ForginPotencial", "Slider_Items_Craft_ForginPotencial");

                            CraftingSlot.implicit_0_toggle = Functions.Get_ToggleInPanel(items_craft_content, "Implicit0", "Toggle_Items_Craft_Implicit0");
                            CraftingSlot.implicit_0_text = Functions.Get_TextInToggle(items_craft_content, "Implicit0", "Toggle_Items_Craft_Implicit0", "Value");
                            CraftingSlot.implicit_0_slider = Functions.Get_SliderInPanel(items_craft_content, "Implicit0", "Slider_Items_Craft_Implicit0");

                            CraftingSlot.implicit_1_toggle = Functions.Get_ToggleInPanel(items_craft_content, "Implicit1", "Toggle_Items_Craft_Implicit1");
                            CraftingSlot.implicit_1_text = Functions.Get_TextInToggle(items_craft_content, "Implicit1", "Toggle_Items_Craft_Implicit1", "Value");
                            CraftingSlot.implicit_1_slider = Functions.Get_SliderInPanel(items_craft_content, "Implicit1", "Slider_Items_Craft_Implicit1");

                            CraftingSlot.implicit_2_toggle = Functions.Get_ToggleInPanel(items_craft_content, "Implicit2", "Toggle_Items_Craft_Implicit2");
                            CraftingSlot.implicit_2_text = Functions.Get_TextInToggle(items_craft_content, "Implicit2", "Toggle_Items_Craft_Implicit2", "Value");
                            CraftingSlot.implicit_2_slider = Functions.Get_SliderInPanel(items_craft_content, "Implicit2", "Slider_Items_Craft_Implicit2");

                            CraftingSlot.seal_tier_toggle = Functions.Get_ToggleInPanel(items_craft_content, "SealTier", "Toggle_Items_Craft_SealTier");
                            CraftingSlot.seal_tier_text = Functions.Get_TextInToggle(items_craft_content, "SealTier", "Toggle_Items_Craft_SealTier", "Value");
                            CraftingSlot.seal_tier_slider = Functions.Get_SliderInPanel(items_craft_content, "SealTier", "Slider_Items_Craft_SealTier");

                            CraftingSlot.seal_value_toggle = Functions.Get_ToggleInPanel(items_craft_content, "SealValue", "Toggle_Items_Craft_SealValue");
                            CraftingSlot.seal_value_text = Functions.Get_TextInToggle(items_craft_content, "SealValue", "Toggle_Items_Craft_SealValue", "Value");
                            CraftingSlot.seal_value_slider = Functions.Get_SliderInPanel(items_craft_content, "SealValue", "Slider_Items_Craft_SealValue");

                            CraftingSlot.affix_0_tier_toggle = Functions.Get_ToggleInPanel(items_craft_content, "AffixTier0", "Toggle_Items_Craft_AffixTier0");
                            CraftingSlot.affix_0_tier_text = Functions.Get_TextInToggle(items_craft_content, "AffixTier0", "Toggle_Items_Craft_AffixTier0", "Value");
                            CraftingSlot.affix_0_tier_slider = Functions.Get_SliderInPanel(items_craft_content, "AffixTier0", "Slider_Items_Craft_AffixTier0");

                            CraftingSlot.affix_0_value_toggle = Functions.Get_ToggleInPanel(items_craft_content, "AffixValue0", "Toggle_Items_Craft_AffixValue0");
                            CraftingSlot.affix_0_value_text = Functions.Get_TextInToggle(items_craft_content, "AffixValue0", "Toggle_Items_Craft_AffixValue0", "Value");
                            CraftingSlot.affix_0_value_slider = Functions.Get_SliderInPanel(items_craft_content, "AffixValue0", "Slider_Items_Craft_AffixValue0");

                            CraftingSlot.affix_1_tier_toggle = Functions.Get_ToggleInPanel(items_craft_content, "AffixTier1", "Toggle_Items_Craft_AffixTier1");
                            CraftingSlot.affix_1_tier_text = Functions.Get_TextInToggle(items_craft_content, "AffixTier1", "Toggle_Items_Craft_AffixTier1", "Value");
                            CraftingSlot.affix_1_tier_slider = Functions.Get_SliderInPanel(items_craft_content, "AffixTier1", "Slider_Items_Craft_AffixTier1");

                            CraftingSlot.affix_1_value_toggle = Functions.Get_ToggleInPanel(items_craft_content, "AffixValue1", "Toggle_Items_Craft_AffixValue1");
                            CraftingSlot.affix_1_value_text = Functions.Get_TextInToggle(items_craft_content, "AffixValue1", "Toggle_Items_Craft_AffixValue1", "Value");
                            CraftingSlot.affix_1_value_slider = Functions.Get_SliderInPanel(items_craft_content, "AffixValue1", "Slider_Items_Craft_AffixValue1");

                            CraftingSlot.affix_2_tier_toggle = Functions.Get_ToggleInPanel(items_craft_content, "AffixTier2", "Toggle_Items_Craft_AffixTier2");
                            CraftingSlot.affix_2_tier_text = Functions.Get_TextInToggle(items_craft_content, "AffixTier2", "Toggle_Items_Craft_AffixTier2", "Value");
                            CraftingSlot.affix_2_tier_slider = Functions.Get_SliderInPanel(items_craft_content, "AffixTier2", "Slider_Items_Craft_AffixTier2");

                            CraftingSlot.affix_2_value_toggle = Functions.Get_ToggleInPanel(items_craft_content, "AffixValue2", "Toggle_Items_Craft_AffixValue2");
                            CraftingSlot.affix_2_value_text = Functions.Get_TextInToggle(items_craft_content, "AffixValue2", "Toggle_Items_Craft_AffixValue2", "Value");
                            CraftingSlot.affix_2_value_slider = Functions.Get_SliderInPanel(items_craft_content, "AffixValue2", "Slider_Items_Craft_AffixValue2");

                            CraftingSlot.affix_3_tier_toggle = Functions.Get_ToggleInPanel(items_craft_content, "AffixTier3", "Toggle_Items_Craft_AffixTier3");
                            CraftingSlot.affix_3_tier_text = Functions.Get_TextInToggle(items_craft_content, "AffixTier3", "Toggle_Items_Craft_AffixTier3", "Value");
                            CraftingSlot.affix_3_tier_slider = Functions.Get_SliderInPanel(items_craft_content, "AffixTier3", "Slider_Items_Craft_AffixTier3");

                            CraftingSlot.affix_3_value_toggle = Functions.Get_ToggleInPanel(items_craft_content, "AffixValue3", "Toggle_Items_Craft_AffixValue3");
                            CraftingSlot.affix_3_value_text = Functions.Get_TextInToggle(items_craft_content, "AffixValue3", "Toggle_Items_Craft_AffixValue3", "Value");
                            CraftingSlot.affix_3_value_slider = Functions.Get_SliderInPanel(items_craft_content, "AffixValue3", "Slider_Items_Craft_AffixValue3");

                            CraftingSlot.uniquemod_0_value_toggle = Functions.Get_ToggleInPanel(items_craft_content, "UniqueMod0", "Toggle_Items_Craft_UniqueMod0");
                            CraftingSlot.uniquemod_0_value_text = Functions.Get_TextInToggle(items_craft_content, "UniqueMod0", "Toggle_Items_Craft_UniqueMod0", "Value");
                            CraftingSlot.uniquemod_0_value_slider = Functions.Get_SliderInPanel(items_craft_content, "UniqueMod0", "Slider_Items_Craft_UniqueMod0");

                            CraftingSlot.uniquemod_1_value_toggle = Functions.Get_ToggleInPanel(items_craft_content, "UniqueMod1", "Toggle_Items_Craft_UniqueMod1");
                            CraftingSlot.uniquemod_1_value_text = Functions.Get_TextInToggle(items_craft_content, "UniqueMod1", "Toggle_Items_Craft_UniqueMod1", "Value");
                            CraftingSlot.uniquemod_1_value_slider = Functions.Get_SliderInPanel(items_craft_content, "UniqueMod1", "Slider_Items_Craft_UniqueMod1");

                            CraftingSlot.uniquemod_2_value_toggle = Functions.Get_ToggleInPanel(items_craft_content, "UniqueMod2", "Toggle_Items_Craft_UniqueMod2");
                            CraftingSlot.uniquemod_2_value_text = Functions.Get_TextInToggle(items_craft_content, "UniqueMod2", "Toggle_Items_Craft_UniqueMod2", "Value");
                            CraftingSlot.uniquemod_2_value_slider = Functions.Get_SliderInPanel(items_craft_content, "UniqueMod2", "Slider_Items_Craft_UniqueMod2");

                            CraftingSlot.uniquemod_3_value_toggle = Functions.Get_ToggleInPanel(items_craft_content, "UniqueMod3", "Toggle_Items_Craft_UniqueMod3");
                            CraftingSlot.uniquemod_3_value_text = Functions.Get_TextInToggle(items_craft_content, "UniqueMod3", "Toggle_Items_Craft_UniqueMod3", "Value");
                            CraftingSlot.uniquemod_3_value_slider = Functions.Get_SliderInPanel(items_craft_content, "UniqueMod3", "Slider_Items_Craft_UniqueMod3");

                            CraftingSlot.uniquemod_4_value_toggle = Functions.Get_ToggleInPanel(items_craft_content, "UniqueMod4", "Toggle_Items_Craft_UniqueMod4");
                            CraftingSlot.uniquemod_4_value_text = Functions.Get_TextInToggle(items_craft_content, "UniqueMod4", "Toggle_Items_Craft_UniqueMod4", "Value");
                            CraftingSlot.uniquemod_4_value_slider = Functions.Get_SliderInPanel(items_craft_content, "UniqueMod4", "Slider_Items_Craft_UniqueMod4");

                            CraftingSlot.uniquemod_5_value_toggle = Functions.Get_ToggleInPanel(items_craft_content, "UniqueMod5", "Toggle_Items_Craft_UniqueMod5");
                            CraftingSlot.uniquemod_5_value_text = Functions.Get_TextInToggle(items_craft_content, "UniqueMod5", "Toggle_Items_Craft_UniqueMod5", "Value");
                            CraftingSlot.uniquemod_5_value_slider = Functions.Get_SliderInPanel(items_craft_content, "UniqueMod5", "Slider_Items_Craft_UniqueMod5");

                            CraftingSlot.uniquemod_6_value_toggle = Functions.Get_ToggleInPanel(items_craft_content, "UniqueMod6", "Toggle_Items_Craft_UniqueMod6");
                            CraftingSlot.uniquemod_6_value_text = Functions.Get_TextInToggle(items_craft_content, "UniqueMod6", "Toggle_Items_Craft_UniqueMod6", "Value");
                            CraftingSlot.uniquemod_6_value_slider = Functions.Get_SliderInPanel(items_craft_content, "UniqueMod6", "Slider_Items_Craft_UniqueMod6");

                            CraftingSlot.uniquemod_7_value_toggle = Functions.Get_ToggleInPanel(items_craft_content, "UniqueMod7", "Toggle_Items_Craft_UniqueMod7");
                            CraftingSlot.uniquemod_7_value_text = Functions.Get_TextInToggle(items_craft_content, "UniqueMod7", "Toggle_Items_Craft_UniqueMod7", "Value");
                            CraftingSlot.uniquemod_7_value_slider = Functions.Get_SliderInPanel(items_craft_content, "UniqueMod7", "Slider_Items_Craft_UniqueMod7");

                            CraftingSlot.legendary_potencial_toggle = Functions.Get_ToggleInPanel(items_craft_content, "LegendaryPotencial", "Toggle_Items_Craft_LegendaryPotencial");
                            CraftingSlot.legendary_potencial_text = Functions.Get_TextInToggle(items_craft_content, "LegendaryPotencial", "Toggle_Items_Craft_LegendaryPotencial", "Value");
                            CraftingSlot.legendary_potencial_slider = Functions.Get_SliderInPanel(items_craft_content, "LegendaryPotencial", "Slider_Items_Craft_LegendaryPotencial");

                            CraftingSlot.weaver_will_toggle = Functions.Get_ToggleInPanel(items_craft_content, "WeaverWill", "Toggle_Items_Craft_WeaverWill");
                            CraftingSlot.weaver_will_text = Functions.Get_TextInToggle(items_craft_content, "WeaverWill", "Toggle_Items_Craft_WeaverWill", "Value");
                            CraftingSlot.weaver_will_slider = Functions.Get_SliderInPanel(items_craft_content, "WeaverWill", "Slider_Items_Craft_WeaverWill");
                        }
                    }
                }
                public static void Set_Events()
                {
                    Events.Set_Toggle_Event(Requirements.level_req_toggle, Requirements.Level_Toggle_Action);
                    Events.Set_Toggle_Event(Requirements.class_req_toggle, Requirements.Class_Toggle_Action);
                    Events.Set_Toggle_Event(Requirements.set_req_toggle, Requirements.Set_Toggle_Action);

                    Events.Set_Button_Event(ForceDrop.forcedrop_drop_button, ForceDrop.Drop_OnClick_Action);
                }
                public static void Set_Active(bool show)
                {
                    if (!content_obj.IsNullOrDestroyed())
                    {
                        content_obj.active = show;
                        enable = show;
                    }
                }
                public static void Toggle_Active()
                {
                    if (!content_obj.IsNullOrDestroyed())
                    {
                        bool show = !content_obj.active;
                        content_obj.active = show;
                        enable = show;
                    }
                }
                public static bool Init_UserData()
                {
                    bool result = false;
                    if (!Save_Manager.instance.IsNullOrDestroyed())
                    {
                        if ((Save_Manager.instance.initialized) && (!Save_Manager.instance.data.IsNullOrDestroyed()))
                        {
                            //Drop
                            Drop.force_unique_toggle.isOn = Save_Manager.instance.data.Items.Drop.Enable_ForceUnique;
                            Drop.force_set_toggle.isOn = Save_Manager.instance.data.Items.Drop.Enable_ForceSet;
                            Drop.force_legendary_toggle.isOn = Save_Manager.instance.data.Items.Drop.Enable_ForceLegendary;

                            Drop.implicits_toggle.isOn = Save_Manager.instance.data.Items.Drop.Enable_Implicits;
                            Drop.implicits_slider_min.value = Save_Manager.instance.data.Items.Drop.Implicits_Min;
                            Drop.implicits_slider_max.value = Save_Manager.instance.data.Items.Drop.Implicits_Max;

                            Drop.forgin_potencial_toggle.isOn = Save_Manager.instance.data.Items.Drop.Enable_ForginPotencial;
                            Drop.forgin_potencial_slider_min.value = Save_Manager.instance.data.Items.Drop.ForginPotencial_Min;
                            Drop.forgin_potencial_slider_max.value = Save_Manager.instance.data.Items.Drop.ForginPotencial_Max;

                            Drop.force_seal_toggle.isOn = Save_Manager.instance.data.Items.Drop.Enable_ForceSeal;

                            Drop.seal_tier_toggle.isOn = Save_Manager.instance.data.Items.Drop.Enable_SealTier;
                            Drop.seal_tier_slider_min.value = Save_Manager.instance.data.Items.Drop.SealTier_Min;
                            Drop.seal_tier_slider_max.value = Save_Manager.instance.data.Items.Drop.SealTier_Max;

                            Drop.seal_value_toggle.isOn = Save_Manager.instance.data.Items.Drop.Enable_SealValue;
                            Drop.seal_value_slider_min.value = Save_Manager.instance.data.Items.Drop.SealValue_Min;
                            Drop.seal_value_slider_max.value = Save_Manager.instance.data.Items.Drop.SealValue_Max;

                            Drop.affix_count_toggle.isOn = Save_Manager.instance.data.Items.Drop.Enable_AffixCount;
                            Drop.affix_count_slider_min.value = Save_Manager.instance.data.Items.Drop.AffixCount_Min;
                            Drop.affix_count_slider_max.value = Save_Manager.instance.data.Items.Drop.AffixCount_Max;

                            Drop.affix_tiers_toggle.isOn = Save_Manager.instance.data.Items.Drop.Enable_AffixTiers;
                            Drop.affix_tiers_slider_min.value = Save_Manager.instance.data.Items.Drop.AffixTiers_Min;
                            Drop.affix_tiers_slider_max.value = Save_Manager.instance.data.Items.Drop.AffixTiers_Max;

                            Drop.affix_values_toggle.isOn = Save_Manager.instance.data.Items.Drop.Enable_AffixValues;
                            Drop.affix_values_slider_min.value = Save_Manager.instance.data.Items.Drop.AffixValues_Min;
                            Drop.affix_values_slider_max.value = Save_Manager.instance.data.Items.Drop.AffixValues_Max;

                            Drop.unique_mods_toggle.isOn = Save_Manager.instance.data.Items.Drop.Enable_UniqueMods;
                            Drop.unique_mods_slider_min.value = Save_Manager.instance.data.Items.Drop.UniqueMods_Min;
                            Drop.unique_mods_slider_max.value = Save_Manager.instance.data.Items.Drop.UniqueMods_Max;

                            Drop.legendary_potencial_toggle.isOn = Save_Manager.instance.data.Items.Drop.Enable_ForceLegendary;
                            Drop.legendary_potencial_slider_min.value = Save_Manager.instance.data.Items.Drop.LegendaryPotencial_Min;
                            Drop.legendary_potencial_slider_max.value = Save_Manager.instance.data.Items.Drop.LegendaryPotencial_Max;

                            Drop.weaver_will_toggle.isOn = Save_Manager.instance.data.Items.Drop.Enable_WeaverWill;
                            Drop.weaver_will_slider_min.value = Save_Manager.instance.data.Items.Drop.WeaverWill_Min;
                            Drop.weaver_will_slider_max.value = Save_Manager.instance.data.Items.Drop.WeaverWill_Max;
                            
                            //Pickup
                            Pickup.autopickup_gold_toggle.isOn = Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_Gold;
                            Pickup.autopickup_keys_toggle.isOn = Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_Keys;
                            Pickup.autopickup_potions_toggle.isOn = Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_Potions;
                            Pickup.autopickup_xptome_toggle.isOn = Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_XpTome;
                            Pickup.autopickup_materials_toggle.isOn = Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_Materials;
                            Pickup.autopickup_fromfilter_toggle.isOn = Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_FromFilter;

                            Pickup.autostore_materials_ondrop_toggle.isOn = Save_Manager.instance.data.Items.Pickup.Enable_AutoStore_OnDrop;
                            Pickup.autostore_materials_oninventoryopen_toggle.isOn = Save_Manager.instance.data.Items.Pickup.Enable_AutoStore_OnInventoryOpen;
                            Pickup.autostore_materials_all10sec_toggle.isOn = Save_Manager.instance.data.Items.Pickup.Enable_AutoStore_All10Sec;

                            Pickup.autosell_hide_toggle.isOn = Save_Manager.instance.data.Items.Pickup.Enable_AutoSell_Hide;                            

                            Pickup.range_pickup_toggle.isOn = Save_Manager.instance.data.Items.Pickup.Enable_RangePickup;
                            Pickup.hide_materials_notifications_toggle.isOn = Save_Manager.instance.data.Items.Pickup.Enable_HideMaterialsNotifications;

                            //Requirements
                            Requirements.class_req_toggle.isOn = Save_Manager.instance.data.Items.Req.classe;
                            Requirements.level_req_toggle.isOn = Save_Manager.instance.data.Items.Req.level;
                            Requirements.set_req_toggle.isOn = Save_Manager.instance.data.Items.Req.set;

                            //Craft
                            CraftingSlot.enable_mod.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_Mod;
                            CraftingSlot.forgin_potencial_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_ForginPotencial;
                            CraftingSlot.forgin_potencial_slider.value = Save_Manager.instance.data.Items.CraftingSlot.ForginPotencial;

                            CraftingSlot.implicit_0_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_Implicit_0;
                            CraftingSlot.implicit_0_slider.value = Save_Manager.instance.data.Items.CraftingSlot.Implicit_0;

                            CraftingSlot.implicit_1_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_Implicit_1;
                            CraftingSlot.implicit_1_slider.value = Save_Manager.instance.data.Items.CraftingSlot.Implicit_1;

                            CraftingSlot.implicit_2_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_Implicit_2;
                            CraftingSlot.implicit_2_slider.value = Save_Manager.instance.data.Items.CraftingSlot.Implicit_2;
                            
                            CraftingSlot.seal_tier_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_Seal_Tier;
                            CraftingSlot.seal_tier_slider.value = Save_Manager.instance.data.Items.CraftingSlot.Seal_Tier;

                            CraftingSlot.seal_value_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_Seal_Value;
                            CraftingSlot.seal_value_slider.value = Save_Manager.instance.data.Items.CraftingSlot.Seal_Value;

                            CraftingSlot.affix_0_tier_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_Affix_0_Tier;
                            CraftingSlot.affix_0_tier_slider.value = Save_Manager.instance.data.Items.CraftingSlot.Affix_0_Tier;

                            CraftingSlot.affix_0_value_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_Affix_0_Value;
                            CraftingSlot.affix_0_value_slider.value = Save_Manager.instance.data.Items.CraftingSlot.Affix_0_Value;

                            CraftingSlot.affix_1_tier_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_Affix_1_Tier;
                            CraftingSlot.affix_1_tier_slider.value = Save_Manager.instance.data.Items.CraftingSlot.Affix_1_Tier;

                            CraftingSlot.affix_1_value_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_Affix_1_Value;
                            CraftingSlot.affix_1_value_slider.value = Save_Manager.instance.data.Items.CraftingSlot.Affix_1_Value;

                            CraftingSlot.affix_2_tier_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_Affix_2_Tier;
                            CraftingSlot.affix_2_tier_slider.value = Save_Manager.instance.data.Items.CraftingSlot.Affix_2_Tier;

                            CraftingSlot.affix_2_value_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_Affix_2_Value;
                            CraftingSlot.affix_2_value_slider.value = Save_Manager.instance.data.Items.CraftingSlot.Affix_2_Value;

                            CraftingSlot.affix_3_tier_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_Affix_3_Tier;
                            CraftingSlot.affix_3_tier_slider.value = Save_Manager.instance.data.Items.CraftingSlot.Affix_3_Tier;

                            CraftingSlot.affix_3_value_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_Affix_3_Value;
                            CraftingSlot.affix_3_value_slider.value = Save_Manager.instance.data.Items.CraftingSlot.Affix_3_Value;

                            CraftingSlot.uniquemod_0_value_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_UniqueMod_0;
                            CraftingSlot.uniquemod_0_value_slider.value = Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_0;

                            CraftingSlot.uniquemod_1_value_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_UniqueMod_1;
                            CraftingSlot.uniquemod_1_value_slider.value = Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_1;
                            
                            CraftingSlot.uniquemod_2_value_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_UniqueMod_2;
                            CraftingSlot.uniquemod_2_value_slider.value = Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_2;

                            CraftingSlot.uniquemod_3_value_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_UniqueMod_3;
                            CraftingSlot.uniquemod_3_value_slider.value = Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_3;

                            CraftingSlot.uniquemod_4_value_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_UniqueMod_4;
                            CraftingSlot.uniquemod_4_value_slider.value = Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_4;

                            CraftingSlot.uniquemod_5_value_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_UniqueMod_5;
                            CraftingSlot.uniquemod_5_value_slider.value = Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_5;

                            CraftingSlot.uniquemod_6_value_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_UniqueMod_6;
                            CraftingSlot.uniquemod_6_value_slider.value = Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_6;

                            CraftingSlot.uniquemod_7_value_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_UniqueMod_7;
                            CraftingSlot.uniquemod_7_value_slider.value = Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_7;

                            CraftingSlot.legendary_potencial_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_LegendaryPotencial;
                            CraftingSlot.legendary_potencial_slider.value = Save_Manager.instance.data.Items.CraftingSlot.LegendaryPotencial;

                            CraftingSlot.weaver_will_toggle.isOn = Save_Manager.instance.data.Items.CraftingSlot.Enable_WeaverWill;
                            CraftingSlot.weaver_will_slider.value = Save_Manager.instance.data.Items.CraftingSlot.WeaverWill;

                            controls_initialized = true;
                            result = true;
                        }
                    }

                    return result;
                }                
                public static void UpdateVisuals()
                {
                    if ((!Save_Manager.instance.IsNullOrDestroyed()) && (controls_initialized))
                    {
                        if ((Save_Manager.instance.initialized) && (!Save_Manager.instance.data.IsNullOrDestroyed()))
                        {
                            //Values
                            Drop.forgin_potencial_text.text = (int)(Save_Manager.instance.data.Items.Drop.ForginPotencial_Min) + " to " + (int)(Save_Manager.instance.data.Items.Drop.ForginPotencial_Max);
                            Drop.affix_count_text.text = (int)(Save_Manager.instance.data.Items.Drop.AffixCount_Min) + " to " + (int)(Save_Manager.instance.data.Items.Drop.AffixCount_Max);                            
                            Drop.legendary_potencial_text.text = (int)(Save_Manager.instance.data.Items.Drop.LegendaryPotencial_Min) + " to " + (int)(Save_Manager.instance.data.Items.Drop.LegendaryPotencial_Max);
                            Drop.weaver_will_text.text = (int)(Save_Manager.instance.data.Items.Drop.WeaverWill_Min) + " to " + (int)(Save_Manager.instance.data.Items.Drop.WeaverWill_Max);
                            ForceDrop.forcedrop_quantity_text.text = "" + (int)(ForceDrop.forcedrop_quantity_slider.value);
                            CraftingSlot.forgin_potencial_text.text = "" + (int)(Save_Manager.instance.data.Items.CraftingSlot.ForginPotencial);
                            CraftingSlot.legendary_potencial_text.text = "" + (int)(Save_Manager.instance.data.Items.CraftingSlot.LegendaryPotencial);
                            CraftingSlot.weaver_will_text.text = "" + (int)(Save_Manager.instance.data.Items.CraftingSlot.WeaverWill);

                            //Tiers
                            Drop.seal_tier_text.text = ((int)(Save_Manager.instance.data.Items.Drop.SealTier_Min) + 1) + " to " + ((int)(Save_Manager.instance.data.Items.Drop.SealTier_Max) + 1);
                            Drop.affix_tiers_text.text = ((int)(Save_Manager.instance.data.Items.Drop.AffixTiers_Min) + 1) + " to " + ((int)(Save_Manager.instance.data.Items.Drop.AffixTiers_Max) + 1);
                            CraftingSlot.seal_tier_text.text = "" + ((int)(Save_Manager.instance.data.Items.CraftingSlot.Seal_Tier) + 1);
                            CraftingSlot.affix_0_tier_text.text = "" + ((int)(Save_Manager.instance.data.Items.CraftingSlot.Affix_0_Tier) + 1);
                            CraftingSlot.affix_1_tier_text.text = "" + ((int)(Save_Manager.instance.data.Items.CraftingSlot.Affix_1_Tier) + 1);
                            CraftingSlot.affix_2_tier_text.text = "" + ((int)(Save_Manager.instance.data.Items.CraftingSlot.Affix_2_Tier) + 1);
                            CraftingSlot.affix_3_tier_text.text = "" + ((int)(Save_Manager.instance.data.Items.CraftingSlot.Affix_3_Tier) + 1);

                            //%
                            Drop.implicits_text.text = (int)((Save_Manager.instance.data.Items.Drop.Implicits_Min / 255) * 100) + " to " + (int)((Save_Manager.instance.data.Items.Drop.Implicits_Max / 255) * 100) + " %";
                            Drop.seal_value_text.text = (int)((Save_Manager.instance.data.Items.Drop.SealValue_Min / 255) * 100) + " to " + (int)((Save_Manager.instance.data.Items.Drop.SealValue_Max / 255) * 100) + " %";
                            Drop.affix_values_text.text = (int)((Save_Manager.instance.data.Items.Drop.AffixValues_Min / 255) * 100) + " to " + (int)((Save_Manager.instance.data.Items.Drop.AffixValues_Max / 255) * 100) + " %";
                            Drop.unique_mods_text.text = (int)((Save_Manager.instance.data.Items.Drop.UniqueMods_Min / 255) * 100) + " to " + (int)((Save_Manager.instance.data.Items.Drop.UniqueMods_Max / 255) * 100) + " %";
                            CraftingSlot.implicit_0_text.text = (int)((Save_Manager.instance.data.Items.CraftingSlot.Implicit_0 / 255) * 100) + " %";
                            CraftingSlot.implicit_1_text.text = (int)((Save_Manager.instance.data.Items.CraftingSlot.Implicit_1 / 255) * 100) + " %";
                            CraftingSlot.implicit_2_text.text = (int)((Save_Manager.instance.data.Items.CraftingSlot.Implicit_2 / 255) * 100) + " %";
                            CraftingSlot.seal_value_text.text = (int)((Save_Manager.instance.data.Items.CraftingSlot.Seal_Value / 255) * 100) + " %";
                            CraftingSlot.affix_0_value_text.text = (int)((Save_Manager.instance.data.Items.CraftingSlot.Affix_0_Value / 255) * 100) + " %";
                            CraftingSlot.affix_1_value_text.text = (int)((Save_Manager.instance.data.Items.CraftingSlot.Affix_1_Value / 255) * 100) + " %";                            
                            CraftingSlot.affix_2_value_text.text = (int)((Save_Manager.instance.data.Items.CraftingSlot.Affix_2_Value / 255) * 100) + " %";                            
                            CraftingSlot.affix_3_value_text.text = (int)((Save_Manager.instance.data.Items.CraftingSlot.Affix_3_Value / 255) * 100) + " %";
                            CraftingSlot.uniquemod_0_value_text.text = (int)((Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_0 / 255) * 100) + " %";
                            CraftingSlot.uniquemod_1_value_text.text = (int)((Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_1 / 255) * 100) + " %";
                            CraftingSlot.uniquemod_2_value_text.text = (int)((Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_2 / 255) * 100) + " %";
                            CraftingSlot.uniquemod_3_value_text.text = (int)((Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_3 / 255) * 100) + " %";
                            CraftingSlot.uniquemod_4_value_text.text = (int)((Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_4 / 255) * 100) + " %";
                            CraftingSlot.uniquemod_5_value_text.text = (int)((Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_5 / 255) * 100) + " %";
                            CraftingSlot.uniquemod_6_value_text.text = (int)((Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_6 / 255) * 100) + " %";
                            CraftingSlot.uniquemod_7_value_text.text = (int)((Save_Manager.instance.data.Items.CraftingSlot.UniqueMod_7 / 255) * 100) + " %";
                        }
                    }
                }

                public class Drop
                {
                    public static Toggle force_unique_toggle = null;
                    public static Toggle force_set_toggle = null;
                    public static Toggle force_legendary_toggle = null;

                    public static Toggle implicits_toggle = null;
                    public static Text implicits_text = null;
                    public static Slider implicits_slider_min = null;
                    public static Slider implicits_slider_max = null;

                    public static Toggle forgin_potencial_toggle = null;
                    public static Text forgin_potencial_text = null;
                    public static Slider forgin_potencial_slider_min = null;
                    public static Slider forgin_potencial_slider_max = null;

                    public static Toggle force_seal_toggle = null;

                    public static Toggle seal_tier_toggle = null;
                    public static Text seal_tier_text = null;
                    public static Slider seal_tier_slider_min = null;
                    public static Slider seal_tier_slider_max = null;

                    public static Toggle seal_value_toggle = null;
                    public static Text seal_value_text = null;
                    public static Slider seal_value_slider_min = null;
                    public static Slider seal_value_slider_max = null;

                    public static Toggle affix_count_toggle = null;
                    public static Text affix_count_text = null;
                    public static Slider affix_count_slider_min = null;
                    public static Slider affix_count_slider_max = null;

                    public static Toggle affix_tiers_toggle = null;
                    public static Text affix_tiers_text = null;
                    public static Slider affix_tiers_slider_min = null;
                    public static Slider affix_tiers_slider_max = null;

                    public static Toggle affix_values_toggle = null;
                    public static Text affix_values_text = null;
                    public static Slider affix_values_slider_min = null;
                    public static Slider affix_values_slider_max = null;

                    public static Toggle unique_mods_toggle = null;
                    public static Text unique_mods_text = null;
                    public static Slider unique_mods_slider_min = null;
                    public static Slider unique_mods_slider_max = null;

                    public static Toggle legendary_potencial_toggle = null;
                    public static Text legendary_potencial_text = null;
                    public static Slider legendary_potencial_slider_min = null;
                    public static Slider legendary_potencial_slider_max = null;

                    public static Toggle weaver_will_toggle = null;
                    public static Text weaver_will_text = null;
                    public static Slider weaver_will_slider_min = null;
                    public static Slider weaver_will_slider_max = null;
                }
                public class Pickup
                {
                    public static Toggle autopickup_gold_toggle = null;
                    public static Toggle autopickup_keys_toggle = null;
                    public static Toggle autopickup_potions_toggle = null;
                    public static Toggle autopickup_xptome_toggle = null;
                    public static Toggle autopickup_materials_toggle = null;
                    public static Toggle autopickup_fromfilter_toggle = null;
                    public static Toggle autostore_materials_ondrop_toggle = null;
                    public static Toggle autostore_materials_oninventoryopen_toggle = null;
                    public static Toggle autostore_materials_all10sec_toggle = null;
                    public static Toggle autosell_hide_toggle = null;
                    public static Toggle range_pickup_toggle = null;
                    public static Toggle hide_materials_notifications_toggle = null;
                }
                public class Requirements
                {
                    // BUG: For some reason the game always return true in action delegates
                    public static Toggle class_req_toggle = null;
                    public static readonly System.Action<bool> Class_Toggle_Action = new System.Action<bool>(Class_Enable);
                    private static void Class_Enable(bool enable)
                    {
                        Save_Manager.instance.data.Items.Req.classe = class_req_toggle.isOn;
                        //Items_Req_Class.Enable();
                    }
                    
                    public static Toggle level_req_toggle = null;
                    public static readonly System.Action<bool> Level_Toggle_Action = new System.Action<bool>(Level_Enable);
                    private static void Level_Enable(bool enable)
                    {
                        Save_Manager.instance.data.Items.Req.level = level_req_toggle.isOn;
                    }

                    public static Toggle set_req_toggle = null;
                    public static readonly System.Action<bool> Set_Toggle_Action = new System.Action<bool>(Set_Enable);
                    private static void Set_Enable(bool enable)
                    {
                        Save_Manager.instance.data.Items.Req.set = set_req_toggle.isOn;
                        //Items_Req_Set.Enable();
                    }
                }
                public class ForceDrop
                {
                    public static Dropdown forcedrop_type_dropdown = null;
                    public static Dropdown forcedrop_rarity_dropdown = null;
                    public static Dropdown forcedrop_items_dropdown = null;
                    public static Text forcedrop_quantity_text = null;
                    public static Slider forcedrop_quantity_slider = null;
                    public static Button forcedrop_drop_button = null;
                    public static int item_type = -1;                    
                    public static int item_rarity = -1;
                    public static int item_subtype = -1;
                    public static int item_unique_id = -1;
                    public static bool btn_enable = false;
                    public static bool Type_Initialized = false;
                    public static bool Initializing_type = false;

                    public static void InitForcedrop()
                    {
                        if ((enable) && (LastEpoch_Hud.Scenes.IsGameScene()) &&
                            (!Type_Initialized) &&
                            (!Initializing_type) &&
                            (!Refs_Manager.item_list.IsNullOrDestroyed()) &&
                            (!forcedrop_type_dropdown.IsNullOrDestroyed()) &&
                            (!forcedrop_rarity_dropdown.IsNullOrDestroyed()) &&
                            (!forcedrop_items_dropdown.IsNullOrDestroyed()))
                        {
                            Initializing_type = true;
                            forcedrop_type_dropdown.ClearOptions();
                            Il2CppSystem.Collections.Generic.List<Dropdown.OptionData> options = new Il2CppSystem.Collections.Generic.List<Dropdown.OptionData>();
                            options.Add(new Dropdown.OptionData { text = "Select" });
                            foreach (ItemList.BaseEquipmentItem item in ItemList.get().EquippableItems)
                            {
                                options.Add(new Dropdown.OptionData { text = item.BaseTypeName });
                            }
                            foreach (ItemList.BaseNonEquipmentItem item in ItemList.get().nonEquippableItems)
                            {
                                options.Add(new Dropdown.OptionData { text = item.BaseTypeName });
                            }
                            forcedrop_type_dropdown.options = options;
                            forcedrop_type_dropdown.value = 0;

                            forcedrop_rarity_dropdown.ClearOptions();
                            forcedrop_rarity_dropdown.enabled = false;

                            forcedrop_items_dropdown.ClearOptions();
                            forcedrop_items_dropdown.enabled = false;

                            //forcedrop_drop_button.enabled = false;

                            Initializing_type = false;
                            Type_Initialized = true;
                        }
                    }
                    public static void SelectType()
                    {
                        if (Type_Initialized)
                        {
                            int index = forcedrop_type_dropdown.value;
                            if (index < forcedrop_type_dropdown.options.Count)
                            {
                                string type_str = forcedrop_type_dropdown.options[forcedrop_type_dropdown.value].text;
                                //Main.logger_instance.Msg("Select : Type = " + type_str);
                                item_type = -1;
                                bool found = false;
                                foreach (ItemList.BaseEquipmentItem item in ItemList.get().EquippableItems)
                                {
                                    if (item.BaseTypeName == type_str)
                                    {
                                        item_type = item.baseTypeID;
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found)
                                {
                                    foreach (ItemList.BaseNonEquipmentItem item in ItemList.get().nonEquippableItems)
                                    {
                                        if (item.BaseTypeName == type_str)
                                        {
                                            item_type = item.baseTypeID;
                                            found = true;
                                            break;
                                        }
                                    }
                                }
                                if (!found) { item_type = -1; }
                                UpdateRarity();
                                UpdateItems();
                                UpdateButton();
                            }
                        }
                    }
                    public static void UpdateRarity()
                    {
                        if ((enable) && (LastEpoch_Hud.Scenes.IsGameScene()) &&
                            (!Refs_Manager.item_list.IsNullOrDestroyed()) &&
                            (Type_Initialized) &&
                            (!forcedrop_type_dropdown.IsNullOrDestroyed()) &&
                            (!forcedrop_rarity_dropdown.IsNullOrDestroyed()) &&
                            (!forcedrop_items_dropdown.IsNullOrDestroyed()))
                        {
                            forcedrop_rarity_dropdown.ClearOptions();
                            Il2CppSystem.Collections.Generic.List<Dropdown.OptionData> options = new Il2CppSystem.Collections.Generic.List<Dropdown.OptionData>();
                            options.Add(new Dropdown.OptionData { text = "Select" });
                            if ((forcedrop_type_dropdown.value > 0) && (item_type > -1))
                            {
                                bool has_unique = false;
                                bool has_set = false;
                                if (UniqueList.instance.IsNullOrDestroyed()) { UniqueList.getUnique(0); }
                                if (!UniqueList.instance.IsNullOrDestroyed())
                                {
                                    foreach (UniqueList.Entry unique in UniqueList.instance.uniques)
                                    {
                                        if (unique.baseType == item_type)
                                        {
                                            if (unique.isSetItem) { has_set = true; }
                                            else { has_unique = true; }
                                        }
                                    }
                                }
                                options.Add(new Dropdown.OptionData { text = "Base Item" });
                                if (has_unique) { options.Add(new Dropdown.OptionData { text = "Unique" }); }
                                if (has_set) { options.Add(new Dropdown.OptionData { text = "Set" }); }
                                forcedrop_rarity_dropdown.enabled = true;
                            }
                            else { forcedrop_rarity_dropdown.enabled = false; }
                            forcedrop_rarity_dropdown.options = options;
                            forcedrop_rarity_dropdown.value = 0;
                            item_rarity = -1;
                        }
                    }
                    public static void SelectRarity()
                    {
                        if (Type_Initialized)
                        {
                            int index = forcedrop_rarity_dropdown.value;
                            if (index < forcedrop_rarity_dropdown.options.Count)
                            {
                                string rarity_str = forcedrop_rarity_dropdown.options[index].text;
                                item_rarity = -1;
                                if (rarity_str == "Base Item") { item_rarity = 0; }
                                else if (rarity_str == "Unique") { item_rarity = 7; }
                                else if (rarity_str == "Set") { item_rarity = 8; }
                                //Main.logger_instance.Msg("Select : Rarity = " + rarity_str);
                                UpdateItems();
                                UpdateButton();
                            }
                        }
                    }
                    public static void UpdateItems()
                    {
                        if ((enable) && (LastEpoch_Hud.Scenes.IsGameScene()) &&
                            (!Refs_Manager.item_list.IsNullOrDestroyed()) &&
                            (Type_Initialized) &&
                            //(!forcedrop_type_dropdown.IsNullOrDestroyed()) &&
                            //(!forcedrop_rarity_dropdown.IsNullOrDestroyed()) &&
                            (!forcedrop_items_dropdown.IsNullOrDestroyed()))
                        {
                            //Main.logger_instance.Msg("Update Items : Type = " + item_type + ", Rarity = " + item_rarity);
                            forcedrop_items_dropdown.ClearOptions();

                            Il2CppSystem.Collections.Generic.List<Dropdown.OptionData> options = new Il2CppSystem.Collections.Generic.List<Dropdown.OptionData>();
                            options.Add(new Dropdown.OptionData { text = "Select" });
                            if ((item_type > -1) && (item_rarity > -1))
                            {
                                if (item_rarity == 0)
                                {
                                    bool type_found = false;
                                    foreach (ItemList.BaseEquipmentItem item_t in ItemList.get().EquippableItems)
                                    {
                                        if (item_t.baseTypeID == item_type)
                                        {
                                            foreach (ItemList.EquipmentItem item in item_t.subItems)
                                            {
                                                string name = item.displayName;
                                                if (name == "" ) { name =  item.name; }
                                                options.Add(new Dropdown.OptionData { text = name });
                                            }
                                            type_found = true;
                                        }
                                    }
                                    if (!type_found)
                                    {
                                        foreach (ItemList.BaseNonEquipmentItem item_t in ItemList.get().nonEquippableItems)
                                        {
                                            if (item_t.baseTypeID == item_type)
                                            {
                                                foreach (ItemList.NonEquipmentItem item in item_t.subItems)
                                                {
                                                    string name = item.displayName;
                                                    if (name == "") { name = item.name; }
                                                    options.Add(new Dropdown.OptionData { text = name });
                                                }

                                                type_found = true;
                                            }
                                        }
                                    }
                                }
                                else if ((item_rarity == 7) || (item_rarity == 8))
                                {
                                    if (UniqueList.instance.IsNullOrDestroyed()) { UniqueList.getUnique(0); }
                                    if (!UniqueList.instance.IsNullOrDestroyed())
                                    {
                                        foreach (UniqueList.Entry unique in UniqueList.instance.uniques)
                                        {
                                            if ((unique.baseType == item_type) &&
                                                (((item_rarity == 7) && (!unique.isSetItem)) ||
                                                ((item_rarity == 8) && (unique.isSetItem))))
                                            {
                                                string name = unique.displayName;
                                                if (name == "") { name = unique.name; }
                                                options.Add(new Dropdown.OptionData { text = name });
                                            }
                                        }
                                    }
                                }
                                forcedrop_items_dropdown.enabled = true;
                            }
                            else { forcedrop_items_dropdown.enabled = false; }
                            forcedrop_items_dropdown.options = options;
                            forcedrop_items_dropdown.value = 0;
                        }
                    }
                    public static void SelectItem()
                    {
                        if (Type_Initialized)
                        {
                            int index = forcedrop_items_dropdown.value;
                            if (index < forcedrop_items_dropdown.options.Count)
                            {
                                string item_str = forcedrop_items_dropdown.options[forcedrop_items_dropdown.value].text;
                                //Main.logger_instance.Msg("Select : Item = " + item_str);

                                item_subtype = -1;
                                item_unique_id = 0;
                                bool item_found = false;
                                if (item_rarity == 0)
                                {
                                    foreach (ItemList.BaseEquipmentItem item_t in ItemList.get().EquippableItems)
                                    {
                                        if (item_t.baseTypeID == item_type)
                                        {
                                            foreach (ItemList.EquipmentItem item in item_t.subItems)
                                            {
                                                if ((item_str == item.displayName) || (item_str == item.name))
                                                {
                                                    item_subtype = item.subTypeID;
                                                    item_found = true;
                                                    break;
                                                }                                                
                                            }
                                        }
                                    }
                                    if (!item_found)
                                    {                                        
                                        foreach (ItemList.BaseNonEquipmentItem item_t in ItemList.get().nonEquippableItems)
                                        {
                                            if (item_t.baseTypeID == item_type)
                                            {
                                                foreach (ItemList.NonEquipmentItem item in item_t.subItems)
                                                {
                                                    if ((item_str == item.displayName) || (item_str == item.name))
                                                    {
                                                        item_subtype = item.subTypeID;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else if ((item_rarity == 7) || (item_rarity == 8))
                                {
                                    if (UniqueList.instance.IsNullOrDestroyed()) { UniqueList.getUnique(0); }
                                    if (!UniqueList.instance.IsNullOrDestroyed())
                                    {
                                        foreach (UniqueList.Entry unique in UniqueList.instance.uniques)
                                        {
                                            if ((item_str == unique.displayName) || (item_str == unique.name))
                                            {
                                                item_subtype = unique.subTypes[0]; //need to be fix here
                                                item_unique_id = unique.uniqueID;
                                                break;
                                            }
                                        }
                                    }
                                }
                                UpdateButton();
                            }
                        }
                    }
                    public static void UpdateButton()
                    {
                        if ((item_type > -1) && (item_rarity > -1) && (item_subtype > -1)) { btn_enable = true; }
                        else { btn_enable = false; }
                    }

                    public static readonly System.Action Drop_OnClick_Action = new System.Action(Drop);
                    public static void Drop()
                    {
                        if (btn_enable)
                        {
                            if ((!Refs_Manager.ground_item_manager.IsNullOrDestroyed()) && (!Refs_Manager.player_actor.IsNullOrDestroyed()))
                            {
                                for (int i = 0; i < forcedrop_quantity_slider.value; i++)
                                {
                                    ItemDataUnpacked item = new ItemDataUnpacked
                                    {
                                        LvlReq = 0,
                                        classReq = ItemList.ClassRequirement.Any,
                                        itemType = (byte)item_type,
                                        subType = (ushort)item_subtype,
                                        rarity = (byte)item_rarity,
                                        forgingPotential = (byte)0,
                                        sockets = (byte)0,
                                        uniqueID = (ushort)item_unique_id,
                                        legendaryPotential = (byte)0,
                                        weaversWill = (byte)0,
                                        hasSealedAffix = false
                                    };

                                    //Random
                                    if (item.itemType < 100)
                                    {
                                        for (int k = 0; k < item.implicitRolls.Count; k++) { item.implicitRolls[k] = (byte)Random.RandomRange(0f, 255f); }
                                        if (!item.isUniqueSetOrLegendary()) { item.forgingPotential = (byte)Random.RandomRange(0f, 255f); }
                                        UniqueList.LegendaryType legendary_type = UniqueList.LegendaryType.LegendaryPotential;
                                        if (item.isUniqueSetOrLegendary())
                                        {
                                            legendary_type = UniqueList.getUnique((ushort)item_unique_id).legendaryType;
                                            for (int k = 0; k < item.uniqueRolls.Count; k++) { item.uniqueRolls[k] = (byte)Random.RandomRange(0f, 255f); }
                                            if (legendary_type == UniqueList.LegendaryType.WeaversWill) { item.weaversWill = (byte)Random.RandomRange(0f, 28f); }
                                            else if (item.isUnique()) { item.legendaryPotential = (byte)Random.RandomRange(0f, 4f); }
                                        }
                                    }
                                    item.RefreshIDAndValues();
                                    Refs_Manager.ground_item_manager.dropItemForPlayer(Refs_Manager.player_actor, item.TryCast<ItemData>(), Refs_Manager.player_actor.position(), false);
                                }
                            }
                        }
                    }
                }
                public class CraftingSlot
                {
                    public static Toggle enable_mod = null;

                    public static Toggle forgin_potencial_toggle = null;
                    public static Text forgin_potencial_text = null;
                    public static Slider forgin_potencial_slider = null;

                    public static Toggle implicit_0_toggle = null;
                    public static Text implicit_0_text = null;
                    public static Slider implicit_0_slider = null;

                    public static Toggle implicit_1_toggle = null;
                    public static Text implicit_1_text = null;
                    public static Slider implicit_1_slider = null;

                    public static Toggle implicit_2_toggle = null;
                    public static Text implicit_2_text = null;
                    public static Slider implicit_2_slider = null;

                    public static Toggle seal_tier_toggle = null;
                    public static Text seal_tier_text = null;
                    public static Slider seal_tier_slider = null;

                    public static Toggle seal_value_toggle = null;
                    public static Text seal_value_text = null;
                    public static Slider seal_value_slider = null;

                    public static Toggle affix_0_tier_toggle = null;
                    public static Text affix_0_tier_text = null;
                    public static Slider affix_0_tier_slider = null;

                    public static Toggle affix_0_value_toggle = null;
                    public static Text affix_0_value_text = null;
                    public static Slider affix_0_value_slider = null;

                    public static Toggle affix_1_tier_toggle = null;
                    public static Text affix_1_tier_text = null;
                    public static Slider affix_1_tier_slider = null;

                    public static Toggle affix_1_value_toggle = null;
                    public static Text affix_1_value_text = null;
                    public static Slider affix_1_value_slider = null;

                    public static Toggle affix_2_tier_toggle = null;
                    public static Text affix_2_tier_text = null;
                    public static Slider affix_2_tier_slider = null;

                    public static Toggle affix_2_value_toggle = null;
                    public static Text affix_2_value_text = null;
                    public static Slider affix_2_value_slider = null;

                    public static Toggle affix_3_tier_toggle = null;
                    public static Text affix_3_tier_text = null;
                    public static Slider affix_3_tier_slider = null;

                    public static Toggle affix_3_value_toggle = null;
                    public static Text affix_3_value_text = null;
                    public static Slider affix_3_value_slider = null;

                    public static Toggle uniquemod_0_value_toggle = null;
                    public static Text uniquemod_0_value_text = null;
                    public static Slider uniquemod_0_value_slider = null;

                    public static Toggle uniquemod_1_value_toggle = null;
                    public static Text uniquemod_1_value_text = null;
                    public static Slider uniquemod_1_value_slider = null;

                    public static Toggle uniquemod_2_value_toggle = null;
                    public static Text uniquemod_2_value_text = null;
                    public static Slider uniquemod_2_value_slider = null;

                    public static Toggle uniquemod_3_value_toggle = null;
                    public static Text uniquemod_3_value_text = null;
                    public static Slider uniquemod_3_value_slider = null;

                    public static Toggle uniquemod_4_value_toggle = null;
                    public static Text uniquemod_4_value_text = null;
                    public static Slider uniquemod_4_value_slider = null;

                    public static Toggle uniquemod_5_value_toggle = null;
                    public static Text uniquemod_5_value_text = null;
                    public static Slider uniquemod_5_value_slider = null;

                    public static Toggle uniquemod_6_value_toggle = null;
                    public static Text uniquemod_6_value_text = null;
                    public static Slider uniquemod_6_value_slider = null;

                    public static Toggle uniquemod_7_value_toggle = null;
                    public static Text uniquemod_7_value_text = null;
                    public static Slider uniquemod_7_value_slider = null;

                    public static Toggle legendary_potencial_toggle = null;
                    public static Text legendary_potencial_text = null;
                    public static Slider legendary_potencial_slider = null;

                    public static Toggle weaver_will_toggle = null;
                    public static Text weaver_will_text = null;
                    public static Slider weaver_will_slider = null;
                }
            }
            public class Login
            {
                public const Cycle current_cycle = Cycle.Trout;

                public static Toggle auto_click_play_offline_toggle = null;
                public static Toggle auto_select_char_toggle = null;
                public static Dropdown auto_select_char_dropdown = null;

                public static void AddAutoSelectCharactersToDropdown()
                {
                    if (!Refs_Manager.character_select.IsNullOrDestroyed() && !Refs_Manager.character_select.AvailableCharacterTiles.IsNullOrDestroyed() && Refs_Manager.character_select.AvailableCharacterTiles.Count > 0)
                    {
                        Il2CppSystem.Collections.Generic.List<Dropdown.OptionData> charNames = new Il2CppSystem.Collections.Generic.List<Dropdown.OptionData>();
                        int index = -1;
                        for (int i = 0; i < Refs_Manager.character_select.AvailableCharacterTiles.Count; i++)
                        {
                            if (!Refs_Manager.character_select.AvailableCharacterTiles[i].characterData.IsNullOrDestroyed())
                            {
                                CharacterTile tile = Refs_Manager.character_select.AvailableCharacterTiles[i];
                                string charName = tile.characterData.CharacterName + (tile.characterCycle < current_cycle ? " (Legacy)" : "");
                                charNames.Add(new Dropdown.OptionData { text = charName });
                                if (!Save_Manager.instance.IsNullOrDestroyed())
                                {
                                    if (string.IsNullOrWhiteSpace(Save_Manager.instance.data.Login.AutoSelectCharName))
                                    {
                                        Save_Manager.instance.data.Login.AutoSelectCharName = tile.characterData.CharacterName;
                                        Save_Manager.instance.data.Login.LegacyCharacter = tile.characterData.Cycle < current_cycle;
                                    }
                                    if (Save_Manager.instance.data.Login.AutoSelectCharName == tile.characterData.CharacterName) { index = i; }
                                }
                            }
                        }
                        auto_select_char_dropdown.options = charNames;
                        if (index > -1) { auto_select_char_dropdown.value = index; }
                    }
                }

                public static void AutoSelectCharDropdown_SelectedIndexChanged(int index)
                {
                    if (!Refs_Manager.character_select.IsNullOrDestroyed() && index > -1 && index < Refs_Manager.character_select.AvailableCharacterTiles.Count &&
                        !Refs_Manager.character_select.AvailableCharacterTiles[index].characterData.IsNullOrDestroyed())
                    {
                        CharacterTile selTile = Refs_Manager.character_select.AvailableCharacterTiles[index];
                        Save_Manager.instance.data.Login.AutoSelectCharName = selTile.characterData.CharacterName;
                        Save_Manager.instance.data.Login.LegacyCharacter = selTile.characterData.Cycle < current_cycle;
                    }
                }

                public static void GetRefs(GameObject parent)
                {
                    auto_click_play_offline_toggle = Functions.Get_ToggleInPanel(parent, "AutoClickPlayOffline", "Toggle_Login_AutoClickPlayOffline");
                    auto_select_char_toggle = Functions.Get_ToggleInPanel(parent, "AutoSelectChar", "Toggle_Login_AutoSelectChar");
                    auto_select_char_dropdown = Functions.Get_DopboxInPanel(parent, "AutoSelectChar", "Dropdown_Login_AutoSelectChar", new System.Action<int>(AutoSelectCharDropdown_SelectedIndexChanged));
                }
            }
            public class Scenes
            {
                public static GameObject content_obj = null;
                public static bool controls_initialized = false;
                public static bool enable = false;

                public static void Get_Refs()
                {
                    content_obj = Functions.GetChild(Content.content_obj, "Scenes_Content");
                    if (!content_obj.IsNullOrDestroyed())
                    {
                        Camera.enable_mod = Functions.Get_ToggleInLabel(content_obj, "Camera", "Toggle_Scenes_Camera_Enable");

                        GameObject scene_camera_content = Functions.GetViewportContent(content_obj, "Camera", "Scenes_Camera_Content");
                        if (!scene_camera_content.IsNullOrDestroyed())
                        {
                            Camera.zoom_minimum_toggle = Functions.Get_ToggleInPanel(scene_camera_content, "ZoomMinimum", "Toggle_Scenes_Camera_ZoomMinimum");
                            Camera.zoom_minimum_text = Functions.Get_TextInToggle(scene_camera_content, "ZoomMinimum", "Toggle_Scenes_Camera_ZoomMinimum", "Value");
                            Camera.zoom_minimum_slider = Functions.Get_SliderInPanel(scene_camera_content, "ZoomMinimum", "Slider_Scenes_Camera_ZoomMinimum");

                            Camera.zoom_per_scroll_toggle = Functions.Get_ToggleInPanel(scene_camera_content, "ZoomPerScroll", "Toggle_Scenes_Camera_ZoomPerScroll");
                            Camera.zoom_per_scroll_text = Functions.Get_TextInToggle(scene_camera_content, "ZoomPerScroll", "Toggle_Scenes_Camera_ZoomPerScroll", "Value");
                            Camera.zoom_per_scroll_slider = Functions.Get_SliderInPanel(scene_camera_content, "ZoomPerScroll", "Slider_Scenes_Camera_ZoomPerScroll");

                            Camera.zoom_speed_toggle = Functions.Get_ToggleInPanel(scene_camera_content, "ZoomSpeed", "Toggle_Scenes_Camera_ZoomSpeed");
                            Camera.zoom_speed_text = Functions.Get_TextInToggle(scene_camera_content, "ZoomSpeed", "Toggle_Scenes_Camera_ZoomSpeed", "Value");
                            Camera.zoom_speed_slider = Functions.Get_SliderInPanel(scene_camera_content, "ZoomSpeed", "Slider_Scenes_Camera_ZoomSpeed");

                            Camera.default_rotation_toggle = Functions.Get_ToggleInPanel(scene_camera_content, "DefaultRotation", "Toggle_Scenes_Camera_DefaultRotation");
                            Camera.default_rotation_text = Functions.Get_TextInToggle(scene_camera_content, "DefaultRotation", "Toggle_Scenes_Camera_DefaultRotation", "Value");
                            Camera.default_rotation_slider = Functions.Get_SliderInPanel(scene_camera_content, "DefaultRotation", "Slider_Scenes_Camera_DefaultRotation");

                            Camera.offset_minimum_toggle = Functions.Get_ToggleInPanel(scene_camera_content, "OffsetMinimum", "Toggle_Scenes_Camera_OffsetMinimum");
                            Camera.offset_minimum_text = Functions.Get_TextInToggle(scene_camera_content, "OffsetMinimum", "Toggle_Scenes_Camera_OffsetMinimum", "Value");
                            Camera.offset_minimum_slider = Functions.Get_SliderInPanel(scene_camera_content, "OffsetMinimum", "Slider_Scenes_Camera_OffsetMinimum");

                            Camera.offset_maximum_toggle = Functions.Get_ToggleInPanel(scene_camera_content, "OffsetMaximum", "Toggle_Scenes_Camera_OffsetMaximum");
                            Camera.offset_maximum_text = Functions.Get_TextInToggle(scene_camera_content, "OffsetMaximum", "Toggle_Scenes_Camera_OffsetMaximum", "Value");
                            Camera.offset_maximum_slider = Functions.Get_SliderInPanel(scene_camera_content, "OffsetMaximum", "Slider_Scenes_Camera_OffsetMaximum");

                            Camera.angle_minimum_toggle = Functions.Get_ToggleInPanel(scene_camera_content, "AngleMinimum", "Toggle_Scenes_Camera_AngleMinimum");
                            Camera.angle_minimum_text = Functions.Get_TextInToggle(scene_camera_content, "AngleMinimum", "Toggle_Scenes_Camera_AngleMinimum", "Value");
                            Camera.angle_minimum_slider = Functions.Get_SliderInPanel(scene_camera_content, "AngleMinimum", "Slider_Scenes_Camera_AngleMinimum");

                            Camera.angle_maximum_toggle = Functions.Get_ToggleInPanel(scene_camera_content, "AngleMaximum", "Toggle_Scenes_Camera_AngleMaximum");
                            Camera.angle_maximum_text = Functions.Get_TextInToggle(scene_camera_content, "AngleMaximum", "Toggle_Scenes_Camera_AngleMaximum", "Value");
                            Camera.angle_maximum_slider = Functions.Get_SliderInPanel(scene_camera_content, "AngleMaximum", "Slider_Scenes_Camera_AngleMaximum");

                            Camera.zoom_load_on_start_toggle = Functions.Get_ToggleInPanel(scene_camera_content, "LoadOnStart", "Toggle_Scenes_Camera_LoadOnStart");

                            Camera.reset_button = Functions.GetChild(scene_camera_content, "Btn_Scenes_Camera_Reset").GetComponent<Button>();
                            Camera.set_button = Functions.GetChild(scene_camera_content, "Btn_Scenes_Camera_Set").GetComponent<Button>();
                        }
                        GameObject scene_dungeons_content = Functions.GetViewportContent(content_obj, "Center", "Scenes_Dungeons_Content");
                        if (!scene_dungeons_content.IsNullOrDestroyed())
                        {
                            Dungeons.enter_without_key_toggle = Functions.Get_ToggleInPanel(scene_dungeons_content, "EnterWithoutKey", "Toggle_Scenes_Dungeons_EnterWithoutKey");
                        }
                        GameObject scene_minimap_content = Functions.GetViewportContent(content_obj, "Center", "Scenes_Minimap_Content");
                        if (!scene_minimap_content.IsNullOrDestroyed())
                        {
                            Minimap.max_zoom_out_toggle = Functions.Get_ToggleInPanel(scene_minimap_content, "MaxZoomOut", "Toggle_Scenes_Minimap_MaxZoomOut");
                            Minimap.remove_fog_of_war_toggle = Functions.Get_ToggleInPanel(scene_minimap_content, "RemoveFogOfWar", "Toggle_Scenes_Minimap_RemoveFogOfWar");
                        }
                        GameObject scene_monoliths_content = Functions.GetViewportContent(content_obj, "Monoliths", "Scenes_Monoliths_Content");
                        if (!scene_monoliths_content.IsNullOrDestroyed())
                        {
                            Monoliths.max_stability_toggle = Functions.Get_ToggleInPanel(scene_monoliths_content, "MaxStability", "Toggle_Scenes_Monoliths_MaxStability");
                            Monoliths.max_stability_text = Functions.Get_TextInToggle(scene_monoliths_content, "MaxStability", "Toggle_Scenes_Monoliths_MaxStability", "Value");
                            Monoliths.max_stability_slider = Functions.Get_SliderInPanel(scene_monoliths_content, "MaxStability", "Slider_Scenes_Monoliths_MaxStability");

                            Monoliths.mob_density_toggle = Functions.Get_ToggleInPanel(scene_monoliths_content, "MobsDensity", "Toggle_Scenes_Monoliths_MobsDensity");
                            Monoliths.mob_density_text = Functions.Get_TextInToggle(scene_monoliths_content, "MobsDensity", "Toggle_Scenes_Monoliths_MobsDensity", "Value");
                            Monoliths.mob_density_slider = Functions.Get_SliderInPanel(scene_monoliths_content, "MobsDensity", "Slider_Scenes_Monoliths_MobsDensity");

                            Monoliths.mob_defeat_toggle = Functions.Get_ToggleInPanel(scene_monoliths_content, "MobsDefeatOnStart", "Toggle_Scenes_Monoliths_MobsDefeatOnStart");
                            Monoliths.mob_defeat_text = Functions.Get_TextInToggle(scene_monoliths_content, "MobsDefeatOnStart", "Toggle_Scenes_Monoliths_MobsDefeatOnStart", "Value");
                            Monoliths.mob_defeat_slider = Functions.Get_SliderInPanel(scene_monoliths_content, "MobsDefeatOnStart", "Slider_Scenes_Monoliths_MobsDefeatOnStart");

                            Monoliths.blessing_slot_toggle = Functions.Get_ToggleInPanel(scene_monoliths_content, "BlessingSlots", "Toggle_Scenes_Monoliths_BlessingSlots");
                            Monoliths.blessing_slot_text = Functions.Get_TextInToggle(scene_monoliths_content, "BlessingSlots", "Toggle_Scenes_Monoliths_BlessingSlots", "Value");
                            Monoliths.blessing_slot_slider = Functions.Get_SliderInPanel(scene_monoliths_content, "BlessingSlots", "Slider_Scenes_Monoliths_BlessingSlots");

                            Monoliths.max_stability_on_start_toggle = Functions.Get_ToggleInPanel(scene_monoliths_content, "MaxStabilityOnStart", "Toggle_Scenes_Monoliths_MaxStabilityOnStart");
                            Monoliths.max_stability_on_stability_changed_toggle = Functions.Get_ToggleInPanel(scene_monoliths_content, "MaxStabilityOnStabilityChanged", "Toggle_Scenes_Monoliths_MaxStabilityOnStabilityChanged");
                            Monoliths.objective_reveal_toggle = Functions.Get_ToggleInPanel(scene_monoliths_content, "ObjectiveReveal", "Toggle_Scenes_Monoliths_ObjectiveReveal");
                            Monoliths.complete_objective_toggle = Functions.Get_ToggleInPanel(scene_monoliths_content, "CompleteObjective", "Toggle_Scenes_Monoliths_CompleteObjective");
                            Monoliths.no_lost_when_die_toggle = Functions.Get_ToggleInPanel(scene_monoliths_content, "NoLostWhenDie", "Toggle_Scenes_Monoliths_NoLostWhenDie");
                        }
                    }
                }
                public static void Set_Events()
                {
                    Events.Set_Button_Event(Camera.reset_button, Camera.Reset_OnClick_Action);
                    Events.Set_Button_Event(Camera.set_button, Camera.Set_OnClick_Action);
                }
                public static void Set_Active(bool show)
                {
                    if (!content_obj.IsNullOrDestroyed())
                    {
                        content_obj.active = show;
                        enable = show;
                    }
                }
                public static void Toggle_Active()
                {
                    if (!content_obj.IsNullOrDestroyed())
                    {
                        bool show = !content_obj.active;
                        content_obj.active = show;
                        enable = show;
                    }
                }
                public static bool Init_UserData()
                {
                    bool result = false;
                    if (!Save_Manager.instance.IsNullOrDestroyed())
                    {
                        if ((Save_Manager.instance.initialized) && (!Save_Manager.instance.data.IsNullOrDestroyed()))
                        {
                            Camera.enable_mod.isOn = Save_Manager.instance.data.Scenes.Camera.Enable_Mod;

                            Camera.zoom_minimum_toggle.isOn = Save_Manager.instance.data.Scenes.Camera.Enable_ZoomMinimum;
                            Camera.zoom_minimum_slider.value = Save_Manager.instance.data.Scenes.Camera.ZoomMinimum;

                            Camera.zoom_per_scroll_toggle.isOn = Save_Manager.instance.data.Scenes.Camera.Enable_ZoomPerScroll;
                            Camera.zoom_per_scroll_slider.value = Save_Manager.instance.data.Scenes.Camera.ZoomPerScroll;

                            Camera.zoom_speed_toggle.isOn = Save_Manager.instance.data.Scenes.Camera.Enable_ZoomSpeed;
                            Camera.zoom_speed_slider.value = Save_Manager.instance.data.Scenes.Camera.ZoomSpeed;

                            Camera.default_rotation_toggle.isOn = Save_Manager.instance.data.Scenes.Camera.Enable_DefaultRotation;
                            Camera.default_rotation_slider.value = Save_Manager.instance.data.Scenes.Camera.DefaultRotation;

                            Camera.offset_minimum_toggle.isOn = Save_Manager.instance.data.Scenes.Camera.Enable_OffsetMinimum;
                            Camera.offset_minimum_slider.value = Save_Manager.instance.data.Scenes.Camera.OffsetMinimum;

                            Camera.offset_maximum_toggle.isOn = Save_Manager.instance.data.Scenes.Camera.Enable_OffsetMaximum;
                            Camera.offset_maximum_slider.value = Save_Manager.instance.data.Scenes.Camera.OffsetMaximum;

                            Camera.angle_minimum_toggle.isOn = Save_Manager.instance.data.Scenes.Camera.Enable_AngleMinimum;
                            Camera.angle_minimum_slider.value = Save_Manager.instance.data.Scenes.Camera.AngleMinimum;

                            Camera.angle_maximum_toggle.isOn = Save_Manager.instance.data.Scenes.Camera.Enable_AngleMaximum;
                            Camera.angle_maximum_slider.value = Save_Manager.instance.data.Scenes.Camera.AngleMaximum;

                            Camera.zoom_load_on_start_toggle.isOn = Save_Manager.instance.data.Scenes.Camera.Enable_LoadOnStart;

                            Dungeons.enter_without_key_toggle.isOn = Save_Manager.instance.data.Scenes.Dungeons.Enable_EnterWithoutKey;

                            Minimap.max_zoom_out_toggle.isOn = Save_Manager.instance.data.Scenes.Minimap.Enable_MaxZoomOut;
                            Minimap.remove_fog_of_war_toggle.isOn = Save_Manager.instance.data.Scenes.Minimap.Enable_RemoveFogOfWar;

                            Monoliths.max_stability_toggle.isOn = Save_Manager.instance.data.Scenes.Monoliths.Enable_MaxStability;
                            Monoliths.max_stability_slider.value = Save_Manager.instance.data.Scenes.Monoliths.MaxStability;

                            Monoliths.mob_density_toggle.isOn = Save_Manager.instance.data.Scenes.Monoliths.Enable_MobsDensity;
                            Monoliths.mob_density_slider.value = Save_Manager.instance.data.Scenes.Monoliths.MobsDensity;

                            Monoliths.mob_defeat_toggle.isOn = Save_Manager.instance.data.Scenes.Monoliths.Enable_MobsDefeatOnStart;
                            Monoliths.mob_defeat_slider.value = Save_Manager.instance.data.Scenes.Monoliths.MobsDefeatOnStart;

                            Monoliths.blessing_slot_toggle.isOn = Save_Manager.instance.data.Scenes.Monoliths.Enable_BlessingSlots;
                            Monoliths.blessing_slot_slider.value = Save_Manager.instance.data.Scenes.Monoliths.BlessingSlots;

                            Monoliths.max_stability_on_start_toggle.isOn = Save_Manager.instance.data.Scenes.Monoliths.Enable_MaxStabilityOnStart;
                            Monoliths.max_stability_on_stability_changed_toggle.isOn = Save_Manager.instance.data.Scenes.Monoliths.Enable_MaxStabilityOnStabilityChanged;
                            Monoliths.objective_reveal_toggle.isOn = Save_Manager.instance.data.Scenes.Monoliths.Enable_ObjectiveReveal;
                            Monoliths.complete_objective_toggle.isOn = Save_Manager.instance.data.Scenes.Monoliths.Enable_CompleteObjective;
                            Monoliths.no_lost_when_die_toggle.isOn = Save_Manager.instance.data.Scenes.Monoliths.Enable_NoLostWhenDie;

                            controls_initialized = true;
                            result = true;
                        }
                    }
                    
                    return result;
                }
                public static void UpdateVisuals()
                {
                    if ((!Save_Manager.instance.IsNullOrDestroyed()) && (controls_initialized))
                    {
                        if ((Save_Manager.instance.initialized) && (!Save_Manager.instance.data.IsNullOrDestroyed()))
                        {
                            Camera.zoom_minimum_text.text = "" + (int)(Save_Manager.instance.data.Scenes.Camera.ZoomMinimum);
                            Camera.zoom_per_scroll_text.text = "" + (int)(Save_Manager.instance.data.Scenes.Camera.ZoomPerScroll);
                            Camera.zoom_speed_text.text = "" + (int)(Save_Manager.instance.data.Scenes.Camera.ZoomSpeed);
                            Camera.default_rotation_text.text = "" + (int)(Save_Manager.instance.data.Scenes.Camera.DefaultRotation);
                            Camera.offset_minimum_text.text = "" + (int)(Save_Manager.instance.data.Scenes.Camera.OffsetMinimum);
                            Camera.offset_maximum_text.text = "" + (int)(Save_Manager.instance.data.Scenes.Camera.OffsetMaximum);
                            Camera.angle_minimum_text.text = "" + (int)(Save_Manager.instance.data.Scenes.Camera.AngleMinimum);
                            Camera.angle_maximum_text.text = "" + (int)(Save_Manager.instance.data.Scenes.Camera.AngleMaximum);
                            Monoliths.max_stability_text.text = "" + (int)(Save_Manager.instance.data.Scenes.Monoliths.MaxStability);
                            Monoliths.mob_density_text.text = "" + (int)(Save_Manager.instance.data.Scenes.Monoliths.MobsDensity);
                            Monoliths.mob_defeat_text.text = "" + (int)(Save_Manager.instance.data.Scenes.Monoliths.MobsDefeatOnStart);
                            Monoliths.blessing_slot_text.text = "" + (int)(Save_Manager.instance.data.Scenes.Monoliths.BlessingSlots);
                        }
                    }
                }

                public class Camera
                {
                    public static Toggle enable_mod = null;

                    public static Toggle zoom_minimum_toggle = null;
                    public static Text zoom_minimum_text = null;
                    public static Slider zoom_minimum_slider = null;

                    public static Toggle zoom_per_scroll_toggle = null;
                    public static Text zoom_per_scroll_text = null;
                    public static Slider zoom_per_scroll_slider = null;

                    public static Toggle zoom_speed_toggle = null;
                    public static Text zoom_speed_text = null;
                    public static Slider zoom_speed_slider = null;

                    public static Toggle default_rotation_toggle = null;
                    public static Text default_rotation_text = null;
                    public static Slider default_rotation_slider = null;

                    public static Toggle offset_minimum_toggle = null;
                    public static Text offset_minimum_text = null;
                    public static Slider offset_minimum_slider = null;

                    public static Toggle offset_maximum_toggle = null;
                    public static Text offset_maximum_text = null;
                    public static Slider offset_maximum_slider = null;

                    public static Toggle angle_minimum_toggle = null;
                    public static Text angle_minimum_text = null;
                    public static Slider angle_minimum_slider = null;

                    public static Toggle angle_maximum_toggle = null;
                    public static Text angle_maximum_text = null;
                    public static Slider angle_maximum_slider = null;

                    public static Toggle zoom_load_on_start_toggle = null;

                    public static Button reset_button = null;
                    public static Button set_button = null;

                    public static readonly System.Action Reset_OnClick_Action = new System.Action(Reset);
                    public static void Reset()
                    {
                        Mods.Camera.Camera_Override.ResetToDefault();
                    }
                    public static readonly System.Action Set_OnClick_Action = new System.Action(Set);
                    public static void Set()
                    {
                        Mods.Camera.Camera_Override.Set();
                    }
                }
                public class Minimap
                {
                    public static Toggle max_zoom_out_toggle = null;
                    public static Toggle remove_fog_of_war_toggle = null;
                }
                public class Dungeons
                {
                    public static Toggle enter_without_key_toggle = null;
                }
                public class Monoliths
                {
                    public static Toggle max_stability_toggle = null;
                    public static Text max_stability_text = null;
                    public static Slider max_stability_slider = null;

                    public static Toggle mob_density_toggle = null;
                    public static Text mob_density_text = null;
                    public static Slider mob_density_slider = null;

                    public static Toggle mob_defeat_toggle = null;
                    public static Text mob_defeat_text = null;
                    public static Slider mob_defeat_slider = null;

                    public static Toggle blessing_slot_toggle = null;
                    public static Text blessing_slot_text = null;
                    public static Slider blessing_slot_slider = null;

                    public static Toggle max_stability_on_start_toggle = null;
                    public static Toggle max_stability_on_stability_changed_toggle = null;
                    public static Toggle objective_reveal_toggle = null;
                    public static Toggle complete_objective_toggle = null;
                    public static Toggle no_lost_when_die_toggle = null;
                }
            }            
            public class Skills
            {
                public static GameObject content_obj = null;
                public static bool controls_initialized = false;
                public static bool enable = false;

                public static void Get_Refs()
                {
                    content_obj = Functions.GetChild(Content.content_obj, "Skill_Tree_Content");
                    if (!content_obj.IsNullOrDestroyed())
                    {
                        GameObject skills_content = Functions.GetViewportContent(content_obj, "Left", "Skills_Content");
                        if (!skills_content.IsNullOrDestroyed())
                        {
                            SkillTree.enable_remove_mana_cost_toggle = Functions.Get_ToggleInPanel(skills_content, "RemoveManaCost", "Toggle_RemoveManaCost");
                            SkillTree.enable_remove_channel_cost_toggle = Functions.Get_ToggleInPanel(skills_content, "RemoveChannelCost", "Toggle_RemoveChannelCost");
                            SkillTree.enable_mana_regen_when_channeling_toggle = Functions.Get_ToggleInPanel(skills_content, "ManaRegenWhenChanneling", "Toggle_ManaRegenWhenChanneling");
                            SkillTree.enable_dont_stop_oom_toggle = Functions.Get_ToggleInPanel(skills_content, "DontStopWhenOOM", "Toggle_DontStopWhenOOM");
                            SkillTree.enable_no_cooldown_toggle = Functions.Get_ToggleInPanel(skills_content, "NoCooldown", "Toggle_NoCooldown");
                            SkillTree.enable_unlock_all_skills_toggle = Functions.Get_ToggleInPanel(skills_content, "UnlockAllSkills", "Toggle_UnlockAllSkills");
                            SkillTree.enable_remove_node_req_toggle = Functions.Get_ToggleInPanel(skills_content, "RemoveNodeRequirements", "Toggle_RemoveNodeRequirements");

                            SkillTree.enable_specialization_slots_toggle = Functions.Get_ToggleInPanel(skills_content, "SpecializationSlots", "Toggle_SpecializationSlots");
                            SkillTree.specialization_slots_text = Functions.Get_TextInToggle(skills_content, "SpecializationSlots", "Toggle_SpecializationSlots", "Value");
                            SkillTree.specialization_slots_slider = Functions.Get_SliderInPanel(skills_content, "SpecializationSlots", "Slider_SpecializationSlots");

                            SkillTree.enable_skill_level_toggle = Functions.Get_ToggleInPanel(skills_content, "SkillLevel", "Toggle_SkillLevel");
                            SkillTree.skill_level_text = Functions.Get_TextInToggle(skills_content, "SkillLevel", "Toggle_SkillLevel", "Value");
                            SkillTree.skill_level_slider = Functions.Get_SliderInPanel(skills_content, "SkillLevel", "Slider_SkillLevel");

                            SkillTree.enable_passive_points_toggle = Functions.Get_ToggleInPanel(skills_content, "PassivePoints", "Toggle_PassivePoints");
                            SkillTree.passive_points_text = Functions.Get_TextInToggle(skills_content, "PassivePoints", "Toggle_PassivePoints", "Value");
                            SkillTree.passive_points_slider = Functions.Get_SliderInPanel(skills_content, "PassivePoints", "Slider_PassivePoints");

                            SkillTree.enable_movement_no_target_toggle = Functions.Get_ToggleInPanel(skills_content, "NoTarget", "Toggle_NoTarget");
                            SkillTree.enable_movement_immune_toggle = Functions.Get_ToggleInPanel(skills_content, "ImmuneDuringMovement", "Toggle_ImmuneDuringMovement");
                            SkillTree.enable_movement_simple_path_toggle = Functions.Get_ToggleInPanel(skills_content, "DisableSimplePath", "Toggle_DisableSimplePath");
                        }
                        else { Main.logger_instance.Error("Skills content is null"); }
                        
                        GameObject companions_content = Functions.GetViewportContent(content_obj, "Center", "Companions_Content");
                        if (!companions_content.IsNullOrDestroyed())
                        {
                            Companions.enable_maximum_companions_toggle = Functions.Get_ToggleInPanel(companions_content, "MaximumCompanions", "Toggle_MaximumCompanions");
                            Companions.maximum_companions_text = Functions.Get_TextInToggle(companions_content, "MaximumCompanions", "Toggle_MaximumCompanions", "Value");
                            Companions.maximum_companions_slider = Functions.Get_SliderInPanel(companions_content, "MaximumCompanions", "Slider_MaximumCompanions");

                            //wolf
                            Companions.enable_wolf_summon_maximum_toggle = Functions.Get_ToggleInPanel(companions_content, "Wolf_SummonToMax", "Toggle_Wolf_SummonToMax");

                            Companions.enable_wolf_summon_limit_toggle = Functions.Get_ToggleInPanel(companions_content, "Wolf_SummonLimit", "Toggle_Wolf_SummonLimit");
                            Companions.wolf_summon_limit_text = Functions.Get_TextInToggle(companions_content, "Wolf_SummonLimit", "Toggle_Wolf_SummonLimit", "Value");
                            Companions.wolf_summon_limit_slider = Functions.Get_SliderInPanel(companions_content, "Wolf_SummonLimit", "Slider_Wolf_SummonLimit");

                            Companions.enable_wolf_stun_immunity_toggle = Functions.Get_ToggleInPanel(companions_content, "Wolf_StunImmunity", "Toggle_Wolf_StunImmunity");

                            //Scorpion
                            Companions.enable_scorpion_summon_limit_toggle = Functions.Get_ToggleInPanel(companions_content, "Scorpions_SummonLimit", "Toggle_Scorpions_SummonLimit");
                            Companions.scorpion_summon_limit_text = Functions.Get_TextInToggle(companions_content, "Scorpions_SummonLimit", "Toggle_Scorpions_SummonLimit", "Value");
                            Companions.scorpion_summon_limit_slider = Functions.Get_SliderInPanel(companions_content, "Scorpions_SummonLimit", "Slider_Scorpions_SummonLimit");
                        }
                        else { Main.logger_instance.Error("Companions content is null"); }

                        GameObject minions_content = Functions.GetViewportContent(content_obj, "Right", "Minions_Content");
                        if (!minions_content.IsNullOrDestroyed())
                        {
                            //Skeletons
                            Minions.enable_skeleton_passive_summon_toggle = Functions.Get_ToggleInPanel(minions_content, "Skeleteon_SummonQuantityFromPassive", "Toggle_Skeleteon_SummonQuantityFromPassive");
                            Minions.skeleton_passive_summon_text = Functions.Get_TextInToggle(minions_content, "Skeleteon_SummonQuantityFromPassive", "Toggle_Skeleteon_SummonQuantityFromPassive", "Value");
                            Minions.skeleton_passive_summon_slider = Functions.Get_SliderInPanel(minions_content, "Skeleteon_SummonQuantityFromPassive", "Slider_Skeleteon_SummonQuantityFromPassive");

                            Minions.enable_skeleton_skilltree_summon_toggle = Functions.Get_ToggleInPanel(minions_content, "Skeleteon_SummonQuantityFromSkillTree", "Toggle_Skeleteon_SummonQuantityFromSkillTree");
                            Minions.skeleton_skilltree_summon_text = Functions.Get_TextInToggle(minions_content, "Skeleteon_SummonQuantityFromSkillTree", "Toggle_Skeleteon_SummonQuantityFromSkillTree", "Value");
                            Minions.skeleton_skilltree_summon_slider = Functions.Get_SliderInPanel(minions_content, "Skeleteon_SummonQuantityFromSkillTree", "Slider_Skeleteon_SummonQuantityFromSkillTree");

                            Minions.enable_skeleton_quantity_per_cast_toggle = Functions.Get_ToggleInPanel(minions_content, "Skeleteon_SummonQuantityPerCast", "Toggle_Skeleteon_SummonQuantityPerCast");
                            Minions.skeleton_quantity_per_cast_text = Functions.Get_TextInToggle(minions_content, "Skeleteon_SummonQuantityPerCast", "Toggle_Skeleteon_SummonQuantityPerCast", "Value");
                            Minions.skeleton_quantity_per_cast_slider = Functions.Get_SliderInPanel(minions_content, "Skeleteon_SummonQuantityPerCast", "Slider_Skeleteon_SummonQuantityPerCast");

                            Minions.enable_skeleton_resummon_on_death_toggle = Functions.Get_ToggleInPanel(minions_content, "Skeleteon_ChanceToResummonOnDeath", "Toggle_Skeleteon_ChanceToResummonOnDeath");
                            Minions.skeleton_resummon_on_death_text = Functions.Get_TextInToggle(minions_content, "Skeleteon_ChanceToResummonOnDeath", "Toggle_Skeleteon_ChanceToResummonOnDeath", "Value");
                            Minions.skeleton_resummon_on_death_slider = Functions.Get_SliderInPanel(minions_content, "Skeleteon_ChanceToResummonOnDeath", "Slider_Skeleteon_ChanceToResummonOnDeath");

                            Minions.enable_skeleton_force_archer_toggle = Functions.Get_ToggleInPanel(minions_content, "Skeleton_ForceArcher", "Toggle_Skeleton_ForceArcher");
                            Minions.enable_skeleton_force_brawler_toggle = Functions.Get_ToggleInPanel(minions_content, "Skeleton_ForceBrawler", "Toggle_Skeleton_ForceBrawler");
                            Minions.enable_skeleton_force_warrior_toggle = Functions.Get_ToggleInPanel(minions_content, "Skeleton_ForceWarrior", "Toggle_Skeleton_ForceWarrior");

                            //Wraiths
                            Minions.enable_wraith_summon_limit_toggle = Functions.Get_ToggleInPanel(minions_content, "Wraiths_SummonMax", "Toggle_Wraiths_SummonMax");
                            Minions.wraith_summon_limit_text = Functions.Get_TextInToggle(minions_content, "Wraiths_SummonMax", "Toggle_Wraiths_SummonMax", "Value");
                            Minions.wraith_summon_limit_slider = Functions.Get_SliderInPanel(minions_content, "Wraiths_SummonMax", "Slider_Wraiths_SummonMax");

                            Minions.enable_wraith_delay_toggle = Functions.Get_ToggleInPanel(minions_content, "Wraiths_Delayed", "Toggle_Wraiths_Delayed");
                            Minions.wraith_delay_text = Functions.Get_TextInToggle(minions_content, "Wraiths_Delayed", "Toggle_Wraiths_Delayed", "Value");
                            Minions.wraith_delay_slider = Functions.Get_SliderInPanel(minions_content, "Wraiths_Delayed", "Slider_Wraiths_Delayed");

                            Minions.enable_wraith_cast_speed_toggle = Functions.Get_ToggleInPanel(minions_content, "Wraiths_CastSpeed", "Toggle_Wraiths_CastSpeed");
                            Minions.wraith_cast_speed_text = Functions.Get_TextInToggle(minions_content, "Wraiths_CastSpeed", "Toggle_Wraiths_CastSpeed", "Value");
                            Minions.wraith_cast_speed_slider = Functions.Get_SliderInPanel(minions_content, "Wraiths_CastSpeed", "Slider_Wraiths_CastSpeed");

                            Minions.enable_wraith_no_limit_toggle = Functions.Get_ToggleInPanel(minions_content, "Wraiths_DisableLimitTo2", "Toggle_Wraiths_DisableLimitTo2");
                            Minions.enable_wraith_no_decay_toggle = Functions.Get_ToggleInPanel(minions_content, "Wraiths_DisableDecay", "Toggle_Wraiths_DisableDecay");

                            //Mages
                            Minions.enable_mage_passive_summon_toggle = Functions.Get_ToggleInPanel(minions_content, "Mages_SummonQuantityFromPassive", "Toggle_Mages_SummonQuantityFromPassive");
                            Minions.mage_passive_summon_text = Functions.Get_TextInToggle(minions_content, "Mages_SummonQuantityFromPassive", "Toggle_Mages_SummonQuantityFromPassive", "Value");
                            Minions.mage_passive_summon_slider = Functions.Get_SliderInPanel(minions_content, "Mages_SummonQuantityFromPassive", "Slider_Mages_SummonQuantityFromPassive");

                            Minions.enable_mage_skilltree_summon_toggle = Functions.Get_ToggleInPanel(minions_content, "Mages_SummonQuantityFromSkillTree", "Toggle_Mages_SummonQuantityFromSkillTree");
                            Minions.mage_skilltree_summon_text = Functions.Get_TextInToggle(minions_content, "Mages_SummonQuantityFromSkillTree", "Toggle_Mages_SummonQuantityFromSkillTree", "Value");
                            Minions.mage_skilltree_summon_slider = Functions.Get_SliderInPanel(minions_content, "Mages_SummonQuantityFromSkillTree", "Slider_Mages_SummonQuantityFromSkillTree");

                            Minions.enable_mage_items_summon_toggle = Functions.Get_ToggleInPanel(minions_content, "Mages_SummonQuantityFromItems", "Toggle_Mages_SummonQuantityFromItems");
                            Minions.mage_items_summon_text = Functions.Get_TextInToggle(minions_content, "Mages_SummonQuantityFromItems", "Toggle_Mages_SummonQuantityFromItems", "Value");
                            Minions.mage_items_summon_slider = Functions.Get_SliderInPanel(minions_content, "Mages_SummonQuantityFromItems", "Slider_Mages_SummonQuantityFromItems");

                            Minions.enable_mage_per_cast_summon_toggle = Functions.Get_ToggleInPanel(minions_content, "Mages_SummonPerCast", "Toggle_Mages_SummonPerCast");
                            Minions.mage_per_cast_summon_text = Functions.Get_TextInToggle(minions_content, "Mages_SummonPerCast", "Toggle_Mages_SummonPerCast", "Value");
                            Minions.mage_per_cast_summon_slider = Functions.Get_SliderInPanel(minions_content, "Mages_SummonPerCast", "Slider_Mages_SummonPerCast");

                            Minions.enable_mage_projectile_chance_toggle = Functions.Get_ToggleInPanel(minions_content, "Mages_ChanceForExtraPorjectiles", "Toggle_Mages_ChanceForExtraPorjectiles");
                            Minions.mage_projectile_chance_text = Functions.Get_TextInToggle(minions_content, "Mages_ChanceForExtraPorjectiles", "Toggle_Mages_ChanceForExtraPorjectiles", "Value");
                            Minions.mage_projectile_chance_slider = Functions.Get_SliderInPanel(minions_content, "Mages_ChanceForExtraPorjectiles", "Slider_Mages_ChanceForExtraPorjectiles");

                            Minions.enable_mage_force_cryomancer_toggle = Functions.Get_ToggleInPanel(minions_content, "Mages_ForceCryomancer", "Toggle_Mages_ForceCryomancer");
                            Minions.enable_mage_force_deathknight_toggle = Functions.Get_ToggleInPanel(minions_content, "Mages_ForceDeathKnight", "Toggle_Mages_ForceDeathKnight");
                            Minions.enable_mage_force_pyromancer_toggle = Functions.Get_ToggleInPanel(minions_content, "Mages_ForcePyromancer", "Toggle_Mages_ForcePyromancer");

                            //Bone Golem
                            Minions.enable_bonegolem_per_skeleton_toggle = Functions.Get_ToggleInPanel(minions_content, "BoneGolem_GolemPerSkeletons", "Toggle_BoneGolem_GolemPerSkeletons");
                            Minions.bonegolem_per_skeleton_text = Functions.Get_TextInToggle(minions_content, "BoneGolem_GolemPerSkeletons", "Toggle_BoneGolem_GolemPerSkeletons", "Value");
                            Minions.bonegolem_per_skeleton_slider = Functions.Get_SliderInPanel(minions_content, "BoneGolem_GolemPerSkeletons", "Slider_BoneGolem_GolemPerSkeletons");

                            Minions.enable_bonegolem_resurect_chance_toggle = Functions.Get_ToggleInPanel(minions_content, "BoneGolem_SelfResurectChance", "Toggle_BoneGolem_SelfResurectChance");
                            Minions.bonegolem_resurect_chance_text = Functions.Get_TextInToggle(minions_content, "BoneGolem_SelfResurectChance", "Toggle_BoneGolem_SelfResurectChance", "Value");
                            Minions.bonegolem_resurect_chance_slider = Functions.Get_SliderInPanel(minions_content, "BoneGolem_SelfResurectChance", "Slider_BoneGolem_SelfResurectChance");

                            Minions.enable_bonegolem_fire_aura_toggle = Functions.Get_ToggleInPanel(minions_content, "BoneGolem_IncreaseFireAura", "Toggle_BoneGolem_IncreaseFireAura");
                            Minions.bonegolem_fire_aura_text = Functions.Get_TextInToggle(minions_content, "BoneGolem_IncreaseFireAura", "Toggle_BoneGolem_IncreaseFireAura", "Value");
                            Minions.bonegolem_fire_aura_slider = Functions.Get_SliderInPanel(minions_content, "BoneGolem_IncreaseFireAura", "Slider_BoneGolem_IncreaseFireAura");

                            Minions.enable_bonegolem_armor_aura_toggle = Functions.Get_ToggleInPanel(minions_content, "BoneGolem_IncreaseArmorAura", "Toggle_BoneGolem_IncreaseArmorAura");
                            Minions.bonegolem_armor_aura_text = Functions.Get_TextInToggle(minions_content, "BoneGolem_IncreaseArmorAura", "Toggle_BoneGolem_IncreaseArmorAura", "Value");
                            Minions.bonegolem_armor_aura_slider = Functions.Get_SliderInPanel(minions_content, "BoneGolem_IncreaseArmorAura", "Slider_BoneGolem_IncreaseArmorAura");

                            Minions.enable_bonegolem_movespeed_aura_toggle = Functions.Get_ToggleInPanel(minions_content, "BoneGolem_IncreaseMoveSpeedAura", "Toggle_BoneGolem_IncreaseMoveSpeedAura");
                            Minions.bonegolem_movespeed_aura_text = Functions.Get_TextInToggle(minions_content, "BoneGolem_IncreaseMoveSpeedAura", "Toggle_BoneGolem_IncreaseMoveSpeedAura", "Value");
                            Minions.bonegolem_movespeed_aura_slider = Functions.Get_SliderInPanel(minions_content, "BoneGolem_IncreaseMoveSpeedAura", "Slider_BoneGolem_IncreaseMoveSpeedAura");

                            Minions.enable_bonegolem_move_speed_toggle = Functions.Get_ToggleInPanel(minions_content, "BoneGolem_IncreaseMoveSpeed", "Toggle_BoneGolem_IncreaseMoveSpeed");
                            Minions.bonegolem_move_speed_text = Functions.Get_TextInToggle(minions_content, "BoneGolem_IncreaseMoveSpeed", "Toggle_BoneGolem_IncreaseMoveSpeed", "Value");
                            Minions.bonegolem_move_speed_slider = Functions.Get_SliderInPanel(minions_content, "BoneGolem_IncreaseMoveSpeed", "Slider_BoneGolem_IncreaseMoveSpeed");

                            Minions.enable_bonegolem_twins_toggle = Functions.Get_ToggleInPanel(minions_content, "BoneGolem_Twins", "Toggle_BoneGolem_Twins");
                            Minions.enable_bonegolem_slam_toggle = Functions.Get_ToggleInPanel(minions_content, "BoneGolem_Slam", "Toggle_BoneGolem_Slam");

                            //Volatile Zombies
                            Minions.enable_volatilezombie_cast_on_death_toggle = Functions.Get_ToggleInPanel(minions_content, "VolatileZombies_ChanceOnMinionDeath", "Toggle_VolatileZombies_ChanceOnMinionDeath");
                            Minions.volatilezombie_cast_on_death_text = Functions.Get_TextInToggle(minions_content, "VolatileZombies_ChanceOnMinionDeath", "Toggle_VolatileZombies_ChanceOnMinionDeath", "Value");
                            Minions.volatilezombie_cast_on_death_slider = Functions.Get_SliderInPanel(minions_content, "VolatileZombies_ChanceOnMinionDeath", "Slider_VolatileZombies_ChanceOnMinionDeath");

                            Minions.enable_volatilezombie_infernal_shade_toggle = Functions.Get_ToggleInPanel(minions_content, "VolatileZombies_InfernalShadeChance", "Toggle_VolatileZombies_InfernalShadeChance");
                            Minions.volatilezombie_infernal_shade_text = Functions.Get_TextInToggle(minions_content, "VolatileZombies_InfernalShadeChance", "Toggle_VolatileZombies_InfernalShadeChance", "Value");
                            Minions.volatilezombie_infernal_shade_slider = Functions.Get_SliderInPanel(minions_content, "VolatileZombies_InfernalShadeChance", "Slider_VolatileZombies_InfernalShadeChance");

                            Minions.enable_volatilezombie_marrow_shards_toggle = Functions.Get_ToggleInPanel(minions_content, "VolatileZombies_MarrowShardsChance", "Toggle_VolatileZombies_MarrowShardsChance");
                            Minions.volatilezombie_marrow_shards_text = Functions.Get_TextInToggle(minions_content, "VolatileZombies_MarrowShardsChance", "Toggle_VolatileZombies_MarrowShardsChance", "Value");
                            Minions.volatilezombie_marrow_shards_slider = Functions.Get_SliderInPanel(minions_content, "VolatileZombies_MarrowShardsChance", "Slider_VolatileZombies_MarrowShardsChance");

                            //Dreadshades
                            Minions.enable_dreadShades_max_toggle = Functions.Get_ToggleInPanel(minions_content, "DreadShades_Max", "Toggle_DreadShades_Max");
                            Minions.dreadShades_max_text = Functions.Get_TextInToggle(minions_content, "DreadShades_Max", "Toggle_DreadShades_Max", "Value");
                            Minions.dreadShades_max_slider = Functions.Get_SliderInPanel(minions_content, "DreadShades_Max", "Slider_DreadShades_Max");

                            Minions.enable_dreadShades_duration_toggle = Functions.Get_ToggleInPanel(minions_content, "DreadShades_Duration", "Toggle_DreadShades_Duration");
                            Minions.dreadShades_duration_text = Functions.Get_TextInToggle(minions_content, "DreadShades_Duration", "Toggle_DreadShades_Duration", "Value");
                            Minions.dreadShades_duration_slider = Functions.Get_SliderInPanel(minions_content, "DreadShades_Duration", "Slider_DreadShades_Duration");

                            Minions.enable_dreadShades_decay_toggle = Functions.Get_ToggleInPanel(minions_content, "DreadShades_Decay", "Toggle_DreadShades_Decay");
                            Minions.dreadShades_decay_text = Functions.Get_TextInToggle(minions_content, "DreadShades_Decay", "Toggle_DreadShades_Decay", "Value");
                            Minions.dreadShades_decay_slider = Functions.Get_SliderInPanel(minions_content, "DreadShades_Decay", "Slider_DreadShades_Decay");

                            Minions.enable_dreadShades_radius_toggle = Functions.Get_ToggleInPanel(minions_content, "DreadShades_Radius", "Toggle_DreadShades_Radius");
                            Minions.dreadShades_radius_text = Functions.Get_TextInToggle(minions_content, "DreadShades_Radius", "Toggle_DreadShades_Radius", "Value");
                            Minions.dreadShades_radius_slider = Functions.Get_SliderInPanel(minions_content, "DreadShades_Radius", "Slider_DreadShades_Radius");

                            Minions.enable_dreadShades_summon_limit_toggle = Functions.Get_ToggleInPanel(minions_content, "DreadShades_DisableLimit", "Toggle_DreadShades_DisableLimit");
                            Minions.enable_dreadShades_health_drain_toggle = Functions.Get_ToggleInPanel(minions_content, "DreadShades_DisableHealthDrain", "Toggle_DreadShades_DisableHealthDrain");
                        }
                        else { Main.logger_instance.Error("Minions content is null"); }
                    }
                    else { Main.logger_instance.Error("Skill Tree content is null"); }
                }
                public static void Set_Active(bool show)
                {
                    if (!content_obj.IsNullOrDestroyed())
                    {
                        content_obj.active = show;
                        enable = show;
                    }
                }
                public static void Toggle_Active()
                {
                    if (!content_obj.IsNullOrDestroyed())
                    {
                        bool show = !content_obj.active;
                        content_obj.active = show;
                        enable = show;
                    }
                }
                public static bool Init_UserData()
                {
                    bool result = false;
                    if (!Save_Manager.instance.IsNullOrDestroyed())
                    {
                        if ((Save_Manager.instance.initialized) && (!Save_Manager.instance.data.IsNullOrDestroyed()))
                        {
                            SkillTree.enable_remove_mana_cost_toggle.isOn = Save_Manager.instance.data.Skills.Enable_RemoveManaCost;
                            SkillTree.enable_remove_channel_cost_toggle.isOn = Save_Manager.instance.data.Skills.Enable_RemoveChannelCost;
                            SkillTree.enable_mana_regen_when_channeling_toggle.isOn = Save_Manager.instance.data.Skills.Enable_NoManaRegenWhileChanneling;
                            SkillTree.enable_dont_stop_oom_toggle.isOn = Save_Manager.instance.data.Skills.Enable_StopWhenOutOfMana;
                            SkillTree.enable_no_cooldown_toggle.isOn = Save_Manager.instance.data.Skills.Enable_RemoveCooldown;
                            SkillTree.enable_unlock_all_skills_toggle.isOn = Save_Manager.instance.data.Skills.Enable_AllSkills;
                            SkillTree.enable_remove_node_req_toggle.isOn = Save_Manager.instance.data.Skills.Disable_NodeRequirement;

                            SkillTree.enable_specialization_slots_toggle.isOn = Save_Manager.instance.data.Skills.Enable_SpecializationSlots;
                            SkillTree.specialization_slots_slider.value = Save_Manager.instance.data.Skills.SpecializationSlots;

                            SkillTree.enable_skill_level_toggle.isOn = Save_Manager.instance.data.Skills.Enable_SkillLevel;
                            SkillTree.skill_level_slider.value = Save_Manager.instance.data.Skills.SkillLevel;

                            SkillTree.enable_passive_points_toggle.isOn = Save_Manager.instance.data.Skills.Enable_PassivePoints;
                            SkillTree.passive_points_slider.value = Save_Manager.instance.data.Skills.PassivePoints;

                            SkillTree.enable_movement_no_target_toggle.isOn = Save_Manager.instance.data.Skills.MovementSkills.Enable_NoTarget;
                            SkillTree.enable_movement_immune_toggle.isOn = Save_Manager.instance.data.Skills.MovementSkills.Enable_ImmuneDuringMovement;
                            SkillTree.enable_movement_simple_path_toggle.isOn = Save_Manager.instance.data.Skills.MovementSkills.Disable_SimplePath;

                            //Companions
                            Companions.enable_maximum_companions_toggle.isOn = Save_Manager.instance.data.Skills.Companion.Enable_Limit;
                            Companions.maximum_companions_slider.value = Save_Manager.instance.data.Skills.Companion.Limit;

                            Companions.enable_wolf_summon_maximum_toggle.isOn = Save_Manager.instance.data.Skills.Companion.Wolf.Enable_SummonMax;

                            Companions.enable_wolf_summon_limit_toggle.isOn = Save_Manager.instance.data.Skills.Companion.Wolf.Enable_SummonLimit;
                            Companions.wolf_summon_limit_slider.value = Save_Manager.instance.data.Skills.Companion.Wolf.SummonLimit;

                            Companions.enable_wolf_stun_immunity_toggle.isOn = Save_Manager.instance.data.Skills.Companion.Wolf.Enable_StunImmunity;

                            Companions.enable_scorpion_summon_limit_toggle.isOn = Save_Manager.instance.data.Skills.Companion.Scorpion.Enable_BabyQuantity;
                            Companions.scorpion_summon_limit_slider.value = Save_Manager.instance.data.Skills.Companion.Scorpion.BabyQuantity;

                            //Skeletons
                            Minions.enable_skeleton_passive_summon_toggle.isOn = Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_additionalSkeletonsFromPassives;
                            Minions.skeleton_passive_summon_slider.value = Save_Manager.instance.data.Skills.Minions.Skeletons.additionalSkeletonsFromPassives;

                            Minions.enable_skeleton_skilltree_summon_toggle.isOn = Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_additionalSkeletonsFromSkillTree;
                            Minions.skeleton_skilltree_summon_slider.value = Save_Manager.instance.data.Skills.Minions.Skeletons.additionalSkeletonsFromSkillTree;

                            Minions.enable_skeleton_quantity_per_cast_toggle.isOn = Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_additionalSkeletonsPerCast;
                            Minions.skeleton_quantity_per_cast_slider.value = Save_Manager.instance.data.Skills.Minions.Skeletons.additionalSkeletonsPerCast;

                            Minions.enable_skeleton_resummon_on_death_toggle.isOn = Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_chanceToResummonOnDeath;
                            Minions.skeleton_resummon_on_death_slider.value = Save_Manager.instance.data.Skills.Minions.Skeletons.chanceToResummonOnDeath;

                            Minions.enable_skeleton_force_archer_toggle.isOn = Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_forceArcher;
                            Minions.enable_skeleton_force_brawler_toggle.isOn = Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_forceBrawler;
                            Minions.enable_skeleton_force_warrior_toggle.isOn = Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_forceWarrior;

                            //Wraiths
                            Minions.enable_wraith_summon_limit_toggle.isOn = Save_Manager.instance.data.Skills.Minions.Wraiths.Enable_additionalMaxWraiths;
                            Minions.wraith_summon_limit_slider.value = Save_Manager.instance.data.Skills.Minions.Wraiths.additionalMaxWraiths;

                            Minions.enable_wraith_delay_toggle.isOn = Save_Manager.instance.data.Skills.Minions.Wraiths.Enable_delayedWraiths;
                            Minions.wraith_delay_slider.value = Save_Manager.instance.data.Skills.Minions.Wraiths.delayedWraiths;

                            Minions.enable_wraith_cast_speed_toggle.isOn = Save_Manager.instance.data.Skills.Minions.Wraiths.Enable_increasedCastSpeed;
                            Minions.wraith_cast_speed_slider.value = Save_Manager.instance.data.Skills.Minions.Wraiths.increasedCastSpeed;

                            Minions.enable_wraith_no_decay_toggle.isOn = Save_Manager.instance.data.Skills.Minions.Wraiths.Enable_wraithsDoNotDecay;
                            Minions.enable_wraith_no_limit_toggle.isOn = Save_Manager.instance.data.Skills.Minions.Wraiths.Enable_limitedTo2Wraiths;

                            //Mage
                            Minions.enable_mage_passive_summon_toggle.isOn = Save_Manager.instance.data.Skills.Minions.Mages.Enable_additionalSkeletonsFromPassives;
                            Minions.mage_passive_summon_slider.value = Save_Manager.instance.data.Skills.Minions.Mages.additionalSkeletonsFromPassives;

                            Minions.enable_mage_skilltree_summon_toggle.isOn = Save_Manager.instance.data.Skills.Minions.Mages.Enable_additionalSkeletonsFromSkillTree;
                            Minions.mage_skilltree_summon_slider.value = Save_Manager.instance.data.Skills.Minions.Mages.additionalSkeletonsFromSkillTree;

                            Minions.enable_mage_items_summon_toggle.isOn = Save_Manager.instance.data.Skills.Minions.Mages.Enable_additionalSkeletonsFromItems;
                            Minions.mage_items_summon_slider.value = Save_Manager.instance.data.Skills.Minions.Mages.additionalSkeletonsFromItems;

                            Minions.enable_mage_per_cast_summon_toggle.isOn = Save_Manager.instance.data.Skills.Minions.Mages.Enable_additionalSkeletonsPerCast;
                            Minions.mage_per_cast_summon_slider.value = Save_Manager.instance.data.Skills.Minions.Mages.additionalSkeletonsPerCast;

                            Minions.enable_mage_projectile_chance_toggle.isOn = Save_Manager.instance.data.Skills.Minions.Mages.Enable_chanceForTwoExtraProjectiles;
                            Minions.mage_projectile_chance_slider.value = Save_Manager.instance.data.Skills.Minions.Mages.chanceForTwoExtraProjectiles;

                            Minions.enable_mage_force_cryomancer_toggle.isOn = Save_Manager.instance.data.Skills.Minions.Mages.Enable_forceCryomancer;
                            Minions.enable_mage_force_deathknight_toggle.isOn = Save_Manager.instance.data.Skills.Minions.Mages.Enable_forceDeathKnight;
                            Minions.enable_mage_force_pyromancer_toggle.isOn = Save_Manager.instance.data.Skills.Minions.Mages.Enable_forcePyromancer;

                            //Bone Golem
                            Minions.enable_bonegolem_per_skeleton_toggle.isOn = Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_addedGolemsPer4Skeletons;
                            Minions.bonegolem_per_skeleton_slider.value = Save_Manager.instance.data.Skills.Minions.BoneGolems.addedGolemsPer4Skeletons;

                            Minions.enable_bonegolem_resurect_chance_toggle.isOn = Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_selfResurrectChance;
                            Minions.bonegolem_resurect_chance_slider.value = Save_Manager.instance.data.Skills.Minions.BoneGolems.selfResurrectChance;

                            Minions.enable_bonegolem_fire_aura_toggle.isOn = Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_increasedFireAuraArea;
                            Minions.bonegolem_fire_aura_slider.value = Save_Manager.instance.data.Skills.Minions.BoneGolems.increasedFireAuraArea;

                            Minions.enable_bonegolem_armor_aura_toggle.isOn = Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_undeadArmorAura;
                            Minions.bonegolem_armor_aura_slider.value = Save_Manager.instance.data.Skills.Minions.BoneGolems.undeadArmorAura;

                            Minions.enable_bonegolem_movespeed_aura_toggle.isOn = Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_undeadMovespeedAura;
                            Minions.bonegolem_movespeed_aura_slider.value = Save_Manager.instance.data.Skills.Minions.BoneGolems.undeadMovespeedAura;

                            Minions.enable_bonegolem_move_speed_toggle.isOn = Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_increasedMoveSpeed;
                            Minions.bonegolem_move_speed_slider.value = Save_Manager.instance.data.Skills.Minions.BoneGolems.increasedMoveSpeed;

                            Minions.enable_bonegolem_twins_toggle.isOn = Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_twins;
                            Minions.enable_bonegolem_slam_toggle.isOn = Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_hasSlamAttack;

                            //Volatile Zombies
                            Minions.enable_volatilezombie_cast_on_death_toggle.isOn = Save_Manager.instance.data.Skills.Minions.VolatileZombies.Enable_chanceToCastFromMinionDeath;
                            Minions.volatilezombie_cast_on_death_slider.value = Save_Manager.instance.data.Skills.Minions.VolatileZombies.chanceToCastFromMinionDeath;

                            Minions.enable_volatilezombie_infernal_shade_toggle.isOn = Save_Manager.instance.data.Skills.Minions.VolatileZombies.Enable_chanceToCastInfernalShadeOnDeath;
                            Minions.volatilezombie_infernal_shade_slider.value = Save_Manager.instance.data.Skills.Minions.VolatileZombies.chanceToCastInfernalShadeOnDeath;

                            Minions.enable_volatilezombie_marrow_shards_toggle.isOn = Save_Manager.instance.data.Skills.Minions.VolatileZombies.Enable_chanceToCastMarrowShardsOnDeath;
                            Minions.volatilezombie_marrow_shards_slider.value = Save_Manager.instance.data.Skills.Minions.VolatileZombies.chanceToCastMarrowShardsOnDeath;

                            //Dreadshades
                            Minions.enable_dreadShades_duration_toggle.isOn = Save_Manager.instance.data.Skills.Minions.DreadShades.Enable_Duration;
                            Minions.dreadShades_duration_slider.value = Save_Manager.instance.data.Skills.Minions.DreadShades.Duration;

                            Minions.enable_dreadShades_max_toggle.isOn = Save_Manager.instance.data.Skills.Minions.DreadShades.Enable_Max;
                            Minions.dreadShades_max_slider.value = Save_Manager.instance.data.Skills.Minions.DreadShades.max;

                            Minions.enable_dreadShades_decay_toggle.isOn = Save_Manager.instance.data.Skills.Minions.DreadShades.Enable_ReduceDecay;
                            Minions.dreadShades_decay_slider.value = Save_Manager.instance.data.Skills.Minions.DreadShades.decay;

                            Minions.enable_dreadShades_radius_toggle.isOn = Save_Manager.instance.data.Skills.Minions.DreadShades.Enable_Radius;
                            Minions.dreadShades_radius_slider.value = Save_Manager.instance.data.Skills.Minions.DreadShades.radius;

                            Minions.enable_dreadShades_summon_limit_toggle.isOn = Save_Manager.instance.data.Skills.Minions.DreadShades.Enable_DisableLimit;
                            Minions.enable_dreadShades_health_drain_toggle.isOn = Save_Manager.instance.data.Skills.Minions.DreadShades.Enable_DisableHealthDrain;

                            controls_initialized = true;
                            result = true;
                        }
                    }

                    return result;
                }
                public static void UpdateVisuals()
                {
                    if ((!Save_Manager.instance.IsNullOrDestroyed()) && (controls_initialized))
                    {
                        if ((Save_Manager.instance.initialized) && (!Save_Manager.instance.data.IsNullOrDestroyed()))
                        {
                            SkillTree.specialization_slots_text.text = "" + (int)Save_Manager.instance.data.Skills.SpecializationSlots;
                            SkillTree.skill_level_text.text = "" + (int)Save_Manager.instance.data.Skills.SkillLevel;
                            SkillTree.passive_points_text.text = "" + (int)Save_Manager.instance.data.Skills.PassivePoints;

                            Companions.maximum_companions_text.text = "" + (int)Save_Manager.instance.data.Skills.Companion.Limit;
                            Companions.wolf_summon_limit_text.text = "" + (int)Save_Manager.instance.data.Skills.Companion.Wolf.SummonLimit;
                            Companions.scorpion_summon_limit_text.text = "" + (int)Save_Manager.instance.data.Skills.Companion.Scorpion.BabyQuantity;

                            Minions.skeleton_passive_summon_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.Skeletons.additionalSkeletonsFromPassives;
                            Minions.skeleton_skilltree_summon_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.Skeletons.additionalSkeletonsFromSkillTree;
                            Minions.skeleton_quantity_per_cast_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.Skeletons.additionalSkeletonsPerCast;
                            Minions.skeleton_resummon_on_death_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.Skeletons.chanceToResummonOnDeath;

                            Minions.wraith_summon_limit_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.Wraiths.additionalMaxWraiths;
                            Minions.wraith_delay_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.Wraiths.delayedWraiths;
                            Minions.wraith_cast_speed_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.Wraiths.increasedCastSpeed;

                            Minions.mage_passive_summon_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.Mages.additionalSkeletonsFromPassives;
                            Minions.mage_skilltree_summon_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.Mages.additionalSkeletonsFromSkillTree;
                            Minions.mage_items_summon_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.Mages.additionalSkeletonsFromItems;
                            Minions.mage_per_cast_summon_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.Mages.additionalSkeletonsPerCast;
                            Minions.mage_projectile_chance_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.Mages.chanceForTwoExtraProjectiles;

                            Minions.bonegolem_per_skeleton_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.BoneGolems.addedGolemsPer4Skeletons;
                            Minions.bonegolem_resurect_chance_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.BoneGolems.selfResurrectChance;
                            Minions.bonegolem_fire_aura_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.BoneGolems.increasedFireAuraArea;
                            Minions.bonegolem_armor_aura_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.BoneGolems.undeadArmorAura;
                            Minions.bonegolem_movespeed_aura_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.BoneGolems.undeadMovespeedAura;
                            Minions.bonegolem_move_speed_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.BoneGolems.increasedMoveSpeed;

                            Minions.volatilezombie_cast_on_death_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.VolatileZombies.chanceToCastFromMinionDeath;
                            Minions.volatilezombie_infernal_shade_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.VolatileZombies.chanceToCastInfernalShadeOnDeath;
                            Minions.volatilezombie_marrow_shards_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.VolatileZombies.chanceToCastMarrowShardsOnDeath;

                            Minions.dreadShades_duration_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.DreadShades.Duration;
                            Minions.dreadShades_max_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.DreadShades.max;
                            Minions.dreadShades_decay_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.DreadShades.decay;
                            Minions.dreadShades_radius_text.text = "" + (int)Save_Manager.instance.data.Skills.Minions.DreadShades.radius;
                        }
                    }
                }

                public class SkillTree
                {
                    public static Toggle enable_remove_mana_cost_toggle = null;
                    public static Toggle enable_remove_channel_cost_toggle = null;
                    public static Toggle enable_mana_regen_when_channeling_toggle = null;
                    public static Toggle enable_dont_stop_oom_toggle = null;
                    public static Toggle enable_no_cooldown_toggle = null;
                    public static Toggle enable_unlock_all_skills_toggle = null;
                    public static Toggle enable_remove_node_req_toggle = null;

                    public static Toggle enable_specialization_slots_toggle = null;
                    public static Text specialization_slots_text = null;
                    public static Slider specialization_slots_slider = null;

                    public static Toggle enable_skill_level_toggle = null;
                    public static Text skill_level_text = null;
                    public static Slider skill_level_slider = null;

                    public static Toggle enable_passive_points_toggle = null;
                    public static Text passive_points_text = null;
                    public static Slider passive_points_slider = null;

                    public static Toggle enable_movement_no_target_toggle = null;
                    public static Toggle enable_movement_immune_toggle = null;
                    public static Toggle enable_movement_simple_path_toggle = null;
                }
                public class Companions
                {
                    public static Toggle enable_maximum_companions_toggle = null;
                    public static Text maximum_companions_text = null;
                    public static Slider maximum_companions_slider = null;

                    //wolf
                    public static Toggle enable_wolf_summon_maximum_toggle = null;

                    public static Toggle enable_wolf_summon_limit_toggle = null;
                    public static Text wolf_summon_limit_text = null;
                    public static Slider wolf_summon_limit_slider = null;

                    public static Toggle enable_wolf_stun_immunity_toggle = null;

                    //Scorpions
                    public static Toggle enable_scorpion_summon_limit_toggle = null;
                    public static Text scorpion_summon_limit_text = null;
                    public static Slider scorpion_summon_limit_slider = null;
                }
                public class Minions
                {
                    //Skeletons
                    public static Toggle enable_skeleton_passive_summon_toggle = null;
                    public static Text skeleton_passive_summon_text = null;
                    public static Slider skeleton_passive_summon_slider = null;

                    public static Toggle enable_skeleton_skilltree_summon_toggle = null;
                    public static Text skeleton_skilltree_summon_text = null;
                    public static Slider skeleton_skilltree_summon_slider = null;

                    public static Toggle enable_skeleton_quantity_per_cast_toggle = null;
                    public static Text skeleton_quantity_per_cast_text = null;
                    public static Slider skeleton_quantity_per_cast_slider = null;

                    public static Toggle enable_skeleton_resummon_on_death_toggle = null;
                    public static Text skeleton_resummon_on_death_text = null;
                    public static Slider skeleton_resummon_on_death_slider = null;

                    public static Toggle enable_skeleton_force_archer_toggle = null;
                    public static Toggle enable_skeleton_force_brawler_toggle = null;
                    public static Toggle enable_skeleton_force_warrior_toggle = null;

                    //Wraiths
                    public static Toggle enable_wraith_summon_limit_toggle = null;
                    public static Text wraith_summon_limit_text = null;
                    public static Slider wraith_summon_limit_slider = null;

                    public static Toggle enable_wraith_delay_toggle = null;
                    public static Text wraith_delay_text = null;
                    public static Slider wraith_delay_slider = null;

                    public static Toggle enable_wraith_cast_speed_toggle = null;
                    public static Text wraith_cast_speed_text = null;
                    public static Slider wraith_cast_speed_slider = null;

                    public static Toggle enable_wraith_no_limit_toggle = null;
                    public static Toggle enable_wraith_no_decay_toggle = null;

                    //Mage
                    public static Toggle enable_mage_passive_summon_toggle = null;
                    public static Text mage_passive_summon_text = null;
                    public static Slider mage_passive_summon_slider = null;

                    public static Toggle enable_mage_items_summon_toggle = null;
                    public static Text mage_items_summon_text = null;
                    public static Slider mage_items_summon_slider = null;

                    public static Toggle enable_mage_skilltree_summon_toggle = null;
                    public static Text mage_skilltree_summon_text = null;
                    public static Slider mage_skilltree_summon_slider = null;

                    public static Toggle enable_mage_per_cast_summon_toggle = null;
                    public static Text mage_per_cast_summon_text = null;
                    public static Slider mage_per_cast_summon_slider = null;

                    public static Toggle enable_mage_projectile_chance_toggle = null;
                    public static Text mage_projectile_chance_text = null;
                    public static Slider mage_projectile_chance_slider = null;

                    public static Toggle enable_mage_force_cryomancer_toggle = null;
                    public static Toggle enable_mage_force_deathknight_toggle = null;
                    public static Toggle enable_mage_force_pyromancer_toggle = null;

                    //Bone Golem
                    public static Toggle enable_bonegolem_per_skeleton_toggle = null;
                    public static Text bonegolem_per_skeleton_text = null;
                    public static Slider bonegolem_per_skeleton_slider = null;

                    public static Toggle enable_bonegolem_resurect_chance_toggle = null;
                    public static Text bonegolem_resurect_chance_text = null;
                    public static Slider bonegolem_resurect_chance_slider = null;

                    public static Toggle enable_bonegolem_fire_aura_toggle = null;
                    public static Text bonegolem_fire_aura_text = null;
                    public static Slider bonegolem_fire_aura_slider = null;

                    public static Toggle enable_bonegolem_armor_aura_toggle = null;
                    public static Text bonegolem_armor_aura_text = null;
                    public static Slider bonegolem_armor_aura_slider = null;

                    public static Toggle enable_bonegolem_movespeed_aura_toggle = null;
                    public static Text bonegolem_movespeed_aura_text = null;
                    public static Slider bonegolem_movespeed_aura_slider = null;

                    public static Toggle enable_bonegolem_move_speed_toggle = null;
                    public static Text bonegolem_move_speed_text = null;
                    public static Slider bonegolem_move_speed_slider = null;

                    public static Toggle enable_bonegolem_twins_toggle = null;
                    public static Toggle enable_bonegolem_slam_toggle = null;

                    //Volatile Zombies
                    public static Toggle enable_volatilezombie_cast_on_death_toggle = null;
                    public static Text volatilezombie_cast_on_death_text = null;
                    public static Slider volatilezombie_cast_on_death_slider = null;

                    public static Toggle enable_volatilezombie_infernal_shade_toggle = null;
                    public static Text volatilezombie_infernal_shade_text = null;
                    public static Slider volatilezombie_infernal_shade_slider = null;

                    public static Toggle enable_volatilezombie_marrow_shards_toggle = null;
                    public static Text volatilezombie_marrow_shards_text = null;
                    public static Slider volatilezombie_marrow_shards_slider = null;

                    //DreadShades
                    public static Toggle enable_dreadShades_duration_toggle = null;
                    public static Text dreadShades_duration_text = null;
                    public static Slider dreadShades_duration_slider = null;

                    public static Toggle enable_dreadShades_max_toggle = null;
                    public static Text dreadShades_max_text = null;
                    public static Slider dreadShades_max_slider = null;

                    public static Toggle enable_dreadShades_decay_toggle = null;
                    public static Text dreadShades_decay_text = null;
                    public static Slider dreadShades_decay_slider = null;

                    public static Toggle enable_dreadShades_radius_toggle = null;
                    public static Text dreadShades_radius_text = null;
                    public static Slider dreadShades_radius_slider = null;

                    public static Toggle enable_dreadShades_summon_limit_toggle = null;
                    public static Toggle enable_dreadShades_health_drain_toggle = null;
                }
            }
            public class OdlForceDrop
            {
                public static bool initialized = false;
                public static bool enable = false;

                public static GameObject content_obj = null;
                public static GameObject left_base_content = null;
                public static GameObject center_content = null;

                //Type
                public static int type_size = 24;
                public static Dropdown type_dropdown = null;
                public static int item_type = -1;               
                public static bool Type_Initialized = false;
                public static bool Initializing_type = false;

                //Rarity
                public static int rarity_size = 24;
                public static Dropdown rarity_dropdown = null;
                public static int item_rarity = -1;

                //Items
                public static int items_size = 24;
                public static Dropdown items_dropdown = null;
                public static int item_subtype = -1;
                public static int item_unique_id = -1;

                //Implicits
                public static bool implicits_enable = false;
                public static int implicits_size = 24;
                public static bool implicits_roll = false;
                public static int implicits_roll_size = 44;
                public static GameObject implicits = null;
                public static GameObject implicits_border = null;
                public static Dropdown implicits_dropdown = null;

                public static GameObject implicit_0 = null;
                public static Text implicit_0_Text = null;
                public static Slider implicit_0_slider = null;
                public static readonly System.Action<float> implicit_0_Action = new System.Action<float>(SetImplicit_0);

                public static GameObject implicit_1 = null;
                public static Text implicit_1_Text = null;
                public static Slider implicit_1_slider = null;
                public static readonly System.Action<float> implicit_1_Action = new System.Action<float>(SetImplicit_1);

                public static GameObject implicit_2 = null;
                public static Text implicit_2_Text = null;
                public static Slider implicit_2_slider = null;
                public static readonly System.Action<float> implicit_2_Action = new System.Action<float>(SetImplicit_2);

                //Forgin potencial
                public static bool forgin_potencial_enable = false;
                public static int forgin_potencial_size = 24;
                public static bool forgin_potencial_roll = false;
                public static int forgin_potencial_roll_size = 42;
                public static GameObject forgin_potencial = null;
                public static GameObject forgin_potencial_border = null;
                public static Dropdown forgin_potencial_dropdown = null;

                public static GameObject forgin_potencial_value = null;
                public static Text forgin_potencial_text = null;
                public static Slider forgin_potencial_slider = null;
                public static readonly System.Action<float> forgin_potencial_Action = new System.Action<float>(SetForginPotencial);

                public static string select_affix = "Select Affix";

                //Seal
                public static bool seal_enable = false;
                public static int seal_id = -1;
                public static string seal_name = "";
                public static bool seal_roll = false;
                public static GameObject seal = null;
                public static GameObject seal_border = null;
                public static Dropdown seal_dropdown = null;

                public static GameObject seal_shard = null;
                public static Button seal_select_btn = null;
                public static Text seal_select_text = null;
                public static readonly System.Action Seal_OnClick_Action = new System.Action(SelectSeal);

                public static GameObject seal_tier = null;
                public static Text seal_tier_text = null;
                public static Slider seal_tier_slider = null;
                public static readonly System.Action<float> seal_tier_Action = new System.Action<float>(SetSealTier);

                public static GameObject seal_value = null;
                public static Text seal_value_text = null;
                public static Slider seal_value_slider = null;
                public static readonly System.Action<float> seal_value_Action = new System.Action<float>(SetSealValue);

                //Affix
                public static bool affixs_enable = false;
                public static bool affixs_roll = false;
                public static GameObject affixs = null;
                public static GameObject affixs_border = null;
                public static Dropdown affixs_dropdown = null;

                public static GameObject affixs_numbers = null;
                public static Text affixs_numbers_text = null;
                public static Slider affixs_numbers_slider = null;

                public static bool affix_0_enable = false;
                public static int affix_0_id = -1;
                public static string affix_0_name = "";
                public static Text affix_0_select_text = null;
                public static GameObject affix_0 = null;
                public static Button affix_0_button = null;
                public static readonly System.Action affix_0_OnClick_Action = new System.Action(SelectAffix_0);
                public static Text affix_0_tier_text = null;
                public static Slider affix_0_tier_slider = null;
                public static readonly System.Action<float> affix_0_tier_Action = new System.Action<float>(SetAffix_0_Tier);
                public static Text affix_0_value_text = null;
                public static Slider affix_0_value_slider = null;
                public static readonly System.Action<float> affix_0_value_Action = new System.Action<float>(SetAffix_0_Value);

                public static bool affix_1_enable = false;
                public static int affix_1_id = -1;
                public static string affix_1_name = "";
                public static Text affix_1_select_text = null;
                public static GameObject affix_1 = null;
                public static Button affix_1_button = null;
                public static readonly System.Action affix_1_OnClick_Action = new System.Action(SelectAffix_1);
                public static Text affix_1_tier_text = null;
                public static Slider affix_1_tier_slider = null;
                public static readonly System.Action<float> affix_1_tier_Action = new System.Action<float>(SetAffix_1_Tier);
                public static Text affix_1_value_text = null;
                public static Slider affix_1_value_slider = null;
                public static readonly System.Action<float> affix_1_value_Action = new System.Action<float>(SetAffix_1_Value);

                public static bool affix_2_enable = false;
                public static int affix_2_id = -1;
                public static string affix_2_name = "";
                public static Text affix_2_select_text = null;
                public static GameObject affix_2 = null;
                public static Button affix_2_button = null;
                public static readonly System.Action affix_2_OnClick_Action = new System.Action(SelectAffix_2);
                public static Text affix_2_tier_text = null;
                public static Slider affix_2_tier_slider = null;
                public static readonly System.Action<float> affix_2_tier_Action = new System.Action<float>(SetAffix_2_Tier);
                public static Text affix_2_value_text = null;
                public static Slider affix_2_value_slider = null;
                public static readonly System.Action<float> affix_2_value_Action = new System.Action<float>(SetAffix_2_Value);

                public static bool affix_3_enable = false;
                public static int affix_3_id = -1;
                public static string affix_3_name = "";
                public static Text affix_3_select_text = null;
                public static GameObject affix_3 = null;
                public static Button affix_3_button = null;
                public static readonly System.Action affix_3_OnClick_Action = new System.Action(SelectAffix_3);
                public static Text affix_3_tier_text = null;
                public static Slider affix_3_tier_slider = null;
                public static readonly System.Action<float> affix_3_tier_Action = new System.Action<float>(SetAffix_3_Tier);
                public static Text affix_3_value_text = null;
                public static Slider affix_3_value_slider = null;
                public static readonly System.Action<float> affix_3_value_Action = new System.Action<float>(SetAffix_3_Value);

                public static bool affix_4_enable = false;
                public static int affix_4_id = -1;
                public static string affix_4_name = "";
                public static Text affix_4_select_text = null;
                public static GameObject affix_4 = null;
                public static Button affix_4_button = null;
                public static readonly System.Action affix_4_OnClick_Action = new System.Action(SelectAffix_4);
                public static Text affix_4_tier_text = null;
                public static Slider affix_4_tier_slider = null;
                public static readonly System.Action<float> affix_4_tier_Action = new System.Action<float>(SetAffix_4_Tier);
                public static Text affix_4_value_text = null;
                public static Slider affix_4_value_slider = null;
                public static readonly System.Action<float> affix_4_value_Action = new System.Action<float>(SetAffix_4_Value);

                public static bool affix_5_enable = false;
                public static int affix_5_id = -1;
                public static string affix_5_name = "";
                public static Text affix_5_select_text = null;
                public static GameObject affix_5 = null;
                public static Button affix_5_button = null;
                public static readonly System.Action affix_5_OnClick_Action = new System.Action(SelectAffix_5);
                public static Text affix_5_tier_text = null;
                public static Slider affix_5_tier_slider = null;
                public static readonly System.Action<float> affix_5_tier_Action = new System.Action<float>(SetAffix_5_Tier);
                public static Text affix_5_value_text = null;
                public static Slider affix_5_value_slider = null;
                public static readonly System.Action<float> affix_5_value_Action = new System.Action<float>(SetAffix_5_Value);

                //Unique mods
                public static bool unique_mods_enable = false;
                public static bool unique_mods_roll = false;
                public static GameObject unique_mods = null;
                public static GameObject unique_mods_border = null;
                public static Dropdown unique_mods_dropdown = null;

                public static GameObject unique_mod_0 = null;
                public static Text unique_mod_0_Text = null;
                public static Slider unique_mod_0_slider = null;
                public static readonly System.Action<float> unique_mod_0_Action = new System.Action<float>(SetUniqueMod_0);

                public static GameObject unique_mod_1 = null;
                public static Text unique_mod_1_Text = null;
                public static Slider unique_mod_1_slider = null;
                public static readonly System.Action<float> unique_mod_1_Action = new System.Action<float>(SetUniqueMod_1);

                public static GameObject unique_mod_2 = null;
                public static Text unique_mod_2_Text = null;
                public static Slider unique_mod_2_slider = null;
                public static readonly System.Action<float> unique_mod_2_Action = new System.Action<float>(SetUniqueMod_2);

                public static GameObject unique_mod_3 = null;
                public static Text unique_mod_3_Text = null;
                public static Slider unique_mod_3_slider = null;
                public static readonly System.Action<float> unique_mod_3_Action = new System.Action<float>(SetUniqueMod_3);

                public static GameObject unique_mod_4 = null;
                public static Text unique_mod_4_Text = null;
                public static Slider unique_mod_4_slider = null;
                public static readonly System.Action<float> unique_mod_4_Action = new System.Action<float>(SetUniqueMod_4);

                public static GameObject unique_mod_5 = null;
                public static Text unique_mod_5_Text = null;
                public static Slider unique_mod_5_slider = null;
                public static readonly System.Action<float> unique_mod_5_Action = new System.Action<float>(SetUniqueMod_5);

                public static GameObject unique_mod_6 = null;
                public static Text unique_mod_6_Text = null;
                public static Slider unique_mod_6_slider = null;
                public static readonly System.Action<float> unique_mod_6_Action = new System.Action<float>(SetUniqueMod_6);

                public static GameObject unique_mod_7 = null;
                public static Text unique_mod_7_Text = null;
                public static Slider unique_mod_7_slider = null;
                public static readonly System.Action<float> unique_mod_7_Action = new System.Action<float>(SetUniqueMod_7);
                                
                public static UniqueList.LegendaryType item_legendary_type = UniqueList.LegendaryType.LegendaryPotential;

                public static bool legenday_potencial_enable = false;
                public static bool legenday_potencial_roll = false;
                public static GameObject legenday_potencial = null;
                public static GameObject legenday_potencial_border = null;
                public static Dropdown legenday_potencial_dropdown = null;
                public static GameObject legenday_potencial_value = null;
                public static Text legenday_potencial_Text = null;
                public static Slider legenday_potencial_slider = null;
                public static readonly System.Action<float> legenday_potencial_Action = new System.Action<float>(SetLegendayPotencial);

                public static bool weaver_will_enable = false;
                public static bool weaver_will_roll = false;
                public static GameObject weaver_will = null;
                public static GameObject weaver_will_border = null;
                public static Dropdown weaver_will_dropdown = null;
                public static GameObject weaver_will_value = null;
                public static Text weaver_will_Text = null;
                public static Slider weaver_will_slider = null;
                public static readonly System.Action<float> weaver_will_Action = new System.Action<float>(SetWeaverWill);

                public static bool quantity_enable = false;
                public static GameObject quantity = null;
                public static GameObject quantity_border = null;
                public static Text quantity_text = null;
                public static Slider forcedrop_quantity_slider = null;

                public static Button forcedrop_drop_button = null;
                public static bool btn_enable = false;
                public static readonly System.Action Drop_OnClick_Action = new System.Action(Drop);

                //Shards Filters
                public static GameObject shard_filters = null;
                public static Dropdown shards_filter_type = null;
                public static Dropdown shards_filter_class = null;
                public static InputField shards_filter_name = null;
                public static Button shards_filters_button = null;
                public static readonly System.Action Resfresh_OnClick_Action = new System.Action(InitializeShardsView);

                //Shards View
                public static GameObject shard_prefab = null;
                public static readonly string shard_btn_name = "ShardBtn_";
                public static bool shard_initialized = false;
                public static bool shard_seal = false;
                public static int shard_number = -1;
                public static int shard_id = -1;

                public static void Get_Refs()
                {
                    bool error = false;
                    content_obj = Functions.GetChild(Content.content_obj, "Old_ForceDrop_Content");
                    if (!content_obj.IsNullOrDestroyed())
                    {
                        left_base_content = Functions.GetViewportContent(content_obj, "Left", "Item");
                        if (!left_base_content.IsNullOrDestroyed())
                        {
                            type_dropdown = Functions.Get_DopboxInPanel(left_base_content, "Type", "Dropdown_Items_ForceDrop_Type", new System.Action<int>((_) => { SelectType(); }));
                            if (type_dropdown.IsNullOrDestroyed()) { error = true; Main.logger_instance.Error("Error type_dropdown not found"); }

                            rarity_dropdown = Functions.Get_DopboxInPanel(left_base_content, "Rarity", "Dropdown_Items_ForceDrop_Rarity", new System.Action<int>((_) => { SelectRarity(); }));
                            if (rarity_dropdown.IsNullOrDestroyed()) { error = true; Main.logger_instance.Error("Error rarity_dropdown not found"); }

                            items_dropdown = Functions.Get_DopboxInPanel(left_base_content, "Item", "Dropdown_Items_ForceDrop_Item", new System.Action<int>((_) => { SelectItem(); }));
                            if (items_dropdown.IsNullOrDestroyed()) { error = true; Main.logger_instance.Error("Error items_dropdown not found"); }

                            implicits = Functions.GetChild(left_base_content, "EnableImplicits");
                            implicits_border = Functions.GetChild(left_base_content, "ImplicitsBorder");
                            implicits_dropdown = Functions.Get_DopboxInPanel(left_base_content, "EnableImplicits", "Dropdown", new System.Action<int>((_) => { EnableImplicits(); }));
                            if (implicits_dropdown.IsNullOrDestroyed()) { error = true; Main.logger_instance.Error("Error implicits_dropdown not found"); }

                            implicit_0 = Functions.GetChild(left_base_content, "Implicit_0");
                            implicit_0_Text = Functions.Get_TextInPanel(left_base_content, "Implicit_0", "Value");
                            implicit_0_slider = Functions.Get_SliderInPanel(left_base_content, "Implicit_0", "Slider");

                            implicit_1 = Functions.GetChild(left_base_content, "Implicit_1");
                            implicit_1_Text = Functions.Get_TextInPanel(left_base_content, "Implicit_1", "Value");
                            implicit_1_slider = Functions.Get_SliderInPanel(left_base_content, "Implicit_1", "Slider");

                            implicit_2 = Functions.GetChild(left_base_content, "Implicit_2");
                            implicit_2_Text = Functions.Get_TextInPanel(left_base_content, "Implicit_2", "Value");
                            implicit_2_slider = Functions.Get_SliderInPanel(left_base_content, "Implicit_2", "Slider");

                            forgin_potencial = Functions.GetChild(left_base_content, "EnableForginPotencial");
                            forgin_potencial_border = Functions.GetChild(left_base_content, "ForginPotencialBorder");
                            forgin_potencial_dropdown = Functions.Get_DopboxInPanel(left_base_content, "EnableForginPotencial", "Dropdown", new System.Action<int>((_) => { EnableForginPotencial(); }));
                            forgin_potencial_value = Functions.GetChild(left_base_content, "ForginPotencial");
                            forgin_potencial_text = Functions.Get_TextInPanel(left_base_content, "ForginPotencial", "Value");
                            forgin_potencial_slider = Functions.Get_SliderInPanel(left_base_content, "ForginPotencial", "Slider");

                            seal = Functions.GetChild(left_base_content, "EnableSeal");
                            seal_border = Functions.GetChild(left_base_content, "SealBorder");
                            seal_dropdown = Functions.Get_DopboxInPanel(left_base_content, "EnableSeal", "Dropdown", new System.Action<int>((_) => { EnableSeal(); }));
                            seal_shard = Functions.GetChild(left_base_content, "SelectSeal");
                            seal_select_btn = Functions.Get_ButtonInPanel(seal_shard, "Button");
                            seal_select_text = Functions.Get_TextInButton(seal_shard, "Button", "Text");
                            seal_tier = Functions.GetChild(left_base_content, "SealTier");
                            seal_tier_text = Functions.Get_TextInPanel(left_base_content, "SealTier", "Value");
                            seal_tier_slider = Functions.Get_SliderInPanel(left_base_content, "SealTier", "Slider");
                            seal_value = Functions.GetChild(left_base_content, "SealValue");
                            seal_value_text = Functions.Get_TextInPanel(left_base_content, "SealValue", "Value");
                            seal_value_slider = Functions.Get_SliderInPanel(left_base_content, "SealValue", "Slider");

                            affixs = Functions.GetChild(left_base_content, "EnableAffixs");
                            if (affixs.IsNullOrDestroyed()) { Main.logger_instance.Error("affixs is null"); }
                            affixs_border = Functions.GetChild(left_base_content, "AffixsBorder");
                            if (affixs_border.IsNullOrDestroyed()) { Main.logger_instance.Error("seal_border is null"); }
                            affixs_dropdown = Functions.Get_DopboxInPanel(left_base_content, "EnableAffixs", "Dropdown", new System.Action<int>((_) => { EnableAffixs(); }));
                            if (affixs_dropdown.IsNullOrDestroyed()) { Main.logger_instance.Error("affixs_dropdown is null"); }
                            affixs_numbers = Functions.GetChild(left_base_content, "AffixsNb");
                            if (affixs_numbers.IsNullOrDestroyed()) { Main.logger_instance.Error("affixs_numbers is null"); }
                            affixs_numbers_text = Functions.Get_TextInPanel(left_base_content, "AffixsNb", "Value");
                            if (affixs_numbers_text.IsNullOrDestroyed()) { Main.logger_instance.Error("affixs_numbers_text is null"); }
                            affixs_numbers_slider = Functions.Get_SliderInPanel(left_base_content, "AffixsNb", "Slider");
                            if (affixs_numbers_slider.IsNullOrDestroyed()) { Main.logger_instance.Error("affixs_numbers_slider is null"); }
                            affix_0 = Functions.GetChild(left_base_content, "Affix_0");
                            affix_0_button = Functions.Get_ButtonInPanel(affix_0, "Button");
                            affix_0_select_text = Functions.Get_TextInButton(affix_0, "Button", "Text");
                            affix_0_tier_text = Functions.Get_TextInPanel(left_base_content, "Affix_0", "TierValue");
                            affix_0_tier_slider = Functions.Get_SliderInPanel(left_base_content, "Affix_0", "TierSlider");
                            affix_0_value_text = Functions.Get_TextInPanel(left_base_content, "Affix_0", "Value");
                            affix_0_value_slider = Functions.Get_SliderInPanel(left_base_content, "Affix_0", "ValueSlider");
                            affix_1 = Functions.GetChild(left_base_content, "Affix_1");
                            affix_1_button = Functions.Get_ButtonInPanel(affix_1, "Button");
                            affix_1_select_text = Functions.Get_TextInButton(affix_1, "Button", "Text");
                            affix_1_tier_text = Functions.Get_TextInPanel(left_base_content, "Affix_1", "TierValue");
                            affix_1_tier_slider = Functions.Get_SliderInPanel(left_base_content, "Affix_1", "TierSlider");
                            affix_1_value_text = Functions.Get_TextInPanel(left_base_content, "Affix_1", "Value");
                            affix_1_value_slider = Functions.Get_SliderInPanel(left_base_content, "Affix_1", "ValueSlider");
                            affix_2 = Functions.GetChild(left_base_content, "Affix_2");
                            affix_2_button = Functions.Get_ButtonInPanel(affix_2, "Button");
                            affix_2_select_text = Functions.Get_TextInButton(affix_2, "Button", "Text");
                            affix_2_tier_text = Functions.Get_TextInPanel(left_base_content, "Affix_2", "TierValue");
                            affix_2_tier_slider = Functions.Get_SliderInPanel(left_base_content, "Affix_2", "TierSlider");
                            affix_2_value_text = Functions.Get_TextInPanel(left_base_content, "Affix_2", "Value");
                            affix_2_value_slider = Functions.Get_SliderInPanel(left_base_content, "Affix_2", "ValueSlider");
                            affix_3 = Functions.GetChild(left_base_content, "Affix_3");
                            affix_3_button = Functions.Get_ButtonInPanel(affix_3, "Button");
                            affix_3_select_text = Functions.Get_TextInButton(affix_3, "Button", "Text");
                            affix_3_tier_text = Functions.Get_TextInPanel(left_base_content, "Affix_3", "TierValue");
                            affix_3_tier_slider = Functions.Get_SliderInPanel(left_base_content, "Affix_3", "TierSlider");
                            affix_3_value_text = Functions.Get_TextInPanel(left_base_content, "Affix_3", "Value");
                            affix_3_value_slider = Functions.Get_SliderInPanel(left_base_content, "Affix_3", "ValueSlider");
                            affix_4 = Functions.GetChild(left_base_content, "Affix_4");
                            affix_4_button = Functions.Get_ButtonInPanel(affix_4, "Button");
                            affix_4_select_text = Functions.Get_TextInButton(affix_4, "Button", "Text");
                            affix_4_tier_text = Functions.Get_TextInPanel(left_base_content, "Affix_4", "TierValue");
                            affix_4_tier_slider = Functions.Get_SliderInPanel(left_base_content, "Affix_4", "TierSlider");
                            affix_4_value_text = Functions.Get_TextInPanel(left_base_content, "Affix_4", "Value");
                            affix_4_value_slider = Functions.Get_SliderInPanel(left_base_content, "Affix_4", "ValueSlider");
                            affix_5 = Functions.GetChild(left_base_content, "Affix_5");
                            affix_5_button = Functions.Get_ButtonInPanel(affix_5, "Button");
                            affix_5_select_text = Functions.Get_TextInButton(affix_5, "Button", "Text");
                            affix_5_tier_text = Functions.Get_TextInPanel(left_base_content, "Affix_5", "TierValue");
                            affix_5_tier_slider = Functions.Get_SliderInPanel(left_base_content, "Affix_5", "TierSlider");
                            affix_5_value_text = Functions.Get_TextInPanel(left_base_content, "Affix_5", "Value");
                            affix_5_value_slider = Functions.Get_SliderInPanel(left_base_content, "Affix_5", "ValueSlider");

                            unique_mods = Functions.GetChild(left_base_content, "EnableUniqueMods");
                            unique_mods_border = Functions.GetChild(left_base_content, "UniqueModsBorder");
                            unique_mods_dropdown = Functions.Get_DopboxInPanel(left_base_content, "EnableUniqueMods", "Dropdown", new System.Action<int>((_) => { EnableUniqueMods(); }));
                            unique_mod_0 = Functions.GetChild(left_base_content, "UniqueMod_0");
                            unique_mod_0_Text = Functions.Get_TextInPanel(left_base_content, "UniqueMod_0", "Value");
                            unique_mod_0_slider = Functions.Get_SliderInPanel(left_base_content, "UniqueMod_0", "Slider");
                            unique_mod_1 = Functions.GetChild(left_base_content, "UniqueMod_1");
                            unique_mod_1_Text = Functions.Get_TextInPanel(left_base_content, "UniqueMod_1", "Value");
                            unique_mod_1_slider = Functions.Get_SliderInPanel(left_base_content, "UniqueMod_1", "Slider");
                            unique_mod_2 = Functions.GetChild(left_base_content, "UniqueMod_2");
                            unique_mod_2_Text = Functions.Get_TextInPanel(left_base_content, "UniqueMod_2", "Value");
                            unique_mod_2_slider = Functions.Get_SliderInPanel(left_base_content, "UniqueMod_2", "Slider");
                            unique_mod_3 = Functions.GetChild(left_base_content, "UniqueMod_3");
                            unique_mod_3_Text = Functions.Get_TextInPanel(left_base_content, "UniqueMod_3", "Value");
                            unique_mod_3_slider = Functions.Get_SliderInPanel(left_base_content, "UniqueMod_3", "Slider");
                            unique_mod_4 = Functions.GetChild(left_base_content, "UniqueMod_4");
                            unique_mod_4_Text = Functions.Get_TextInPanel(left_base_content, "UniqueMod_4", "Value");
                            unique_mod_4_slider = Functions.Get_SliderInPanel(left_base_content, "UniqueMod_4", "Slider");
                            unique_mod_5 = Functions.GetChild(left_base_content, "UniqueMod_5");
                            unique_mod_5_Text = Functions.Get_TextInPanel(left_base_content, "UniqueMod_5", "Value");
                            unique_mod_5_slider = Functions.Get_SliderInPanel(left_base_content, "UniqueMod_5", "Slider");
                            unique_mod_6 = Functions.GetChild(left_base_content, "UniqueMod_6");
                            unique_mod_6_Text = Functions.Get_TextInPanel(left_base_content, "UniqueMod_6", "Value");
                            unique_mod_6_slider = Functions.Get_SliderInPanel(left_base_content, "UniqueMod_6", "Slider");
                            unique_mod_7 = Functions.GetChild(left_base_content, "UniqueMod_7");
                            unique_mod_7_Text = Functions.Get_TextInPanel(left_base_content, "UniqueMod_7", "Value");
                            unique_mod_7_slider = Functions.Get_SliderInPanel(left_base_content, "UniqueMod_7", "Slider");

                            legenday_potencial = Functions.GetChild(left_base_content, "EnableLegendaryPotencial");
                            legenday_potencial_border = Functions.GetChild(left_base_content, "LegendaryPotencialBorder");
                            legenday_potencial_dropdown = Functions.Get_DopboxInPanel(left_base_content, "EnableLegendaryPotencial", "Dropdown", new System.Action<int>((_) => { EnableLegendaryPotencial(); }));
                            legenday_potencial_value = Functions.GetChild(left_base_content, "LegendaryPotencial");
                            legenday_potencial_Text = Functions.Get_TextInPanel(left_base_content, "LegendaryPotencial", "Value");
                            legenday_potencial_slider = Functions.Get_SliderInPanel(left_base_content, "LegendaryPotencial", "Slider");

                            weaver_will = Functions.GetChild(left_base_content, "EnableWeaverWill");
                            weaver_will_border = Functions.GetChild(left_base_content, "WeaverWillBorder");
                            weaver_will_dropdown = Functions.Get_DopboxInPanel(left_base_content, "EnableWeaverWill", "Dropdown", new System.Action<int>((_) => { EnableWeaverWill(); }));
                            weaver_will_value = Functions.GetChild(left_base_content, "WeaverWill");
                            weaver_will_Text = Functions.Get_TextInPanel(left_base_content, "WeaverWill", "Value");
                            weaver_will_slider = Functions.Get_SliderInPanel(left_base_content, "WeaverWill", "Slider");

                            quantity = Functions.GetChild(left_base_content, "Quantity");
                            quantity_border = Functions.GetChild(left_base_content, "QuantityBorder");
                            forcedrop_quantity_slider = Functions.Get_SliderInPanel(left_base_content, "Quantity", "Slider_Items_ForceDrop_Quantity");
                            if (forcedrop_quantity_slider.IsNullOrDestroyed()) { error = true; Main.logger_instance.Error("Error forcedrop_quantity_slider not found"); }
                            quantity_text = Functions.Get_TextInPanel(left_base_content, "Quantity", "Value");
                        }
                        else { error = true; Main.logger_instance.Error("left_content not found"); }

                        //Shards filters
                        GameObject center = Functions.GetChild(content_obj, "Center");
                        if (!center.IsNullOrDestroyed())
                        {
                            shard_filters = Functions.GetChild(center, "Filters");
                            if (!shard_filters.IsNullOrDestroyed())
                            {
                                GameObject line_0 = Functions.GetChild(shard_filters, "Line_0");
                                if (!line_0.IsNullOrDestroyed())
                                {
                                    shards_filter_type = Functions.Get_DopboxInPanel(line_0, "Type", "Dropdown", new System.Action<int>((value) => { }));
                                    shards_filter_class = Functions.Get_DopboxInPanel(line_0, "Class", "Dropdown", new System.Action<int>((_) => { }));
                                }
                                else { error = true; Main.logger_instance.Error("line_0 not found"); }

                                GameObject line_1 = Functions.GetChild(shard_filters, "Line_1");
                                if (!line_1.IsNullOrDestroyed())
                                {
                                    GameObject name = Functions.GetChild(line_1, "Name");
                                    if(!name.IsNullOrDestroyed())
                                    {
                                        GameObject g = Functions.GetChild(name, "InputField");
                                        if (!g.IsNullOrDestroyed()) { shards_filter_name = g.GetComponent<InputField>(); }
                                        else { error = true; Main.logger_instance.Error("g_name not found"); }
                                    }
                                    else { error = true; Main.logger_instance.Error("name not found"); }

                                    GameObject refresh = Functions.GetChild(line_1, "Refresh");
                                    if (!refresh.IsNullOrDestroyed())
                                    {
                                        GameObject g = Functions.GetChild(refresh, "Button");
                                        if (!g.IsNullOrDestroyed()) { shards_filters_button = g.GetComponent<Button>(); }
                                        else { error = true; Main.logger_instance.Error("g_refresh not found"); }
                                    }
                                    else { error = true; Main.logger_instance.Error("refresh not found"); }
                                }
                                else { error = true; Main.logger_instance.Error("line_1 not found"); }
                            }
                            else { error = true; Main.logger_instance.Error("shard_filters not found"); }
                        }
                        else { error = true; Main.logger_instance.Error("center not found"); }

                        //Shards
                        center_content = Functions.GetViewportContent(content_obj, "Center", "Content");
                        if (!center_content.IsNullOrDestroyed())
                        {

                        }
                        else { error = true; Main.logger_instance.Error("center_content not found"); }
                        
                        //Drop button
                        GameObject left_obj = Functions.GetChild(content_obj, "Left");
                        if (!left_obj.IsNullOrDestroyed())
                        {
                            GameObject new_obj = Functions.GetChild(left_obj, "Btn");
                            if (!new_obj.IsNullOrDestroyed())
                            {
                                forcedrop_drop_button = Functions.Get_ButtonInPanel(new_obj, "Btn_Drop");
                                if (forcedrop_drop_button.IsNullOrDestroyed()) { error = true; Main.logger_instance.Error("Error forcedrop_drop_button not found"); }
                            }
                            else { error = true; Main.logger_instance.Error("left Btn panel not found"); }
                        }
                        else { error = true; Main.logger_instance.Error("left_obj not found"); }
                    }
                    else { error = true; Main.logger_instance.Error("content_obj is null"); }

                    if (!error) { initialized = true; }
                }
                public static void Set_Events()
                {
                    if (!forcedrop_drop_button.IsNullOrDestroyed())
                    {
                        Events.Set_Slider_Event(implicit_0_slider, implicit_0_Action);
                        Events.Set_Slider_Event(implicit_1_slider, implicit_1_Action);
                        Events.Set_Slider_Event(implicit_2_slider, implicit_2_Action);
                        Events.Set_Slider_Event(forgin_potencial_slider, forgin_potencial_Action);
                        Events.Set_Button_Event(seal_select_btn, Seal_OnClick_Action);
                        Events.Set_Slider_Event(seal_tier_slider, seal_tier_Action);
                        Events.Set_Slider_Event(seal_value_slider, seal_value_Action);                                                
                        Events.Set_Button_Event(affix_0_button, affix_0_OnClick_Action);
                        Events.Set_Slider_Event(affix_0_tier_slider, affix_0_tier_Action);
                        Events.Set_Slider_Event(affix_0_value_slider, affix_0_value_Action);
                        Events.Set_Button_Event(affix_1_button, affix_1_OnClick_Action);
                        Events.Set_Slider_Event(affix_1_tier_slider, affix_1_tier_Action);
                        Events.Set_Slider_Event(affix_1_value_slider, affix_1_value_Action);
                        Events.Set_Button_Event(affix_2_button, affix_2_OnClick_Action);
                        Events.Set_Slider_Event(affix_2_tier_slider, affix_2_tier_Action);
                        Events.Set_Slider_Event(affix_2_value_slider, affix_2_value_Action);
                        Events.Set_Button_Event(affix_3_button, affix_3_OnClick_Action);
                        Events.Set_Slider_Event(affix_3_tier_slider, affix_3_tier_Action);
                        Events.Set_Slider_Event(affix_3_value_slider, affix_3_value_Action);
                        Events.Set_Button_Event(affix_4_button, affix_4_OnClick_Action);
                        Events.Set_Slider_Event(affix_4_tier_slider, affix_4_tier_Action);
                        Events.Set_Slider_Event(affix_4_value_slider, affix_4_value_Action);
                        Events.Set_Button_Event(affix_5_button, affix_5_OnClick_Action);
                        Events.Set_Slider_Event(affix_5_tier_slider, affix_5_tier_Action);
                        Events.Set_Slider_Event(affix_5_value_slider, affix_5_value_Action);
                        Events.Set_Slider_Event(unique_mod_0_slider, unique_mod_0_Action);
                        Events.Set_Slider_Event(unique_mod_1_slider, unique_mod_1_Action);
                        Events.Set_Slider_Event(unique_mod_2_slider, unique_mod_2_Action);
                        Events.Set_Slider_Event(unique_mod_3_slider, unique_mod_3_Action);
                        Events.Set_Slider_Event(unique_mod_4_slider, unique_mod_4_Action);
                        Events.Set_Slider_Event(unique_mod_5_slider, unique_mod_5_Action);
                        Events.Set_Slider_Event(unique_mod_6_slider, unique_mod_6_Action);
                        Events.Set_Slider_Event(unique_mod_7_slider, unique_mod_7_Action);
                        Events.Set_Slider_Event(legenday_potencial_slider, legenday_potencial_Action);
                        Events.Set_Slider_Event(weaver_will_slider, weaver_will_Action);
                                                
                        Events.Set_Button_Event(shards_filters_button, Resfresh_OnClick_Action);

                        Events.Set_Button_Event(forcedrop_drop_button, Drop_OnClick_Action);
                    }
                }
                public static void Set_Active(bool show)
                {
                    if (!content_obj.IsNullOrDestroyed())
                    {
                        content_obj.active = show;
                        enable = show;
                    }
                }
                public static void Toggle_Active()
                {
                    if (!content_obj.IsNullOrDestroyed())
                    {
                        bool show = !content_obj.active;
                        content_obj.active = show;
                        enable = show;
                    }
                }

                public static void InitForcedrop()
                {
                    if ((!Type_Initialized) && (!Initializing_type))
                    {
                        Initializing_type = true;
                        type_dropdown.ClearOptions();
                        Il2CppSystem.Collections.Generic.List<Dropdown.OptionData> options = new Il2CppSystem.Collections.Generic.List<Dropdown.OptionData>();
                        options.Add(new Dropdown.OptionData { text = "Select" });
                        foreach (ItemList.BaseEquipmentItem item in ItemList.get().EquippableItems)
                        {
                            options.Add(new Dropdown.OptionData { text = item.BaseTypeName });
                        }
                        foreach (ItemList.BaseNonEquipmentItem item in ItemList.get().nonEquippableItems)
                        {
                            options.Add(new Dropdown.OptionData { text = item.BaseTypeName });
                        }
                        type_dropdown.options = options;
                        type_dropdown.value = 0;

                        rarity_dropdown.ClearOptions();
                        rarity_dropdown.enabled = false;

                        items_dropdown.ClearOptions();
                        items_dropdown.enabled = false;

                        //forcedrop_drop_button.enabled = false;

                        Type_Initialized = true;
                        Initializing_type = false;
                    }
                }
                public static void SelectType()
                {
                    if (Type_Initialized)
                    {
                        int index = type_dropdown.value;
                        if (index < type_dropdown.options.Count)
                        {
                            string type_str = type_dropdown.options[type_dropdown.value].text;
                            item_type = -1;
                            bool found = false;
                            foreach (ItemList.BaseEquipmentItem item in ItemList.get().EquippableItems)
                            {
                                if (item.BaseTypeName == type_str)
                                {
                                    item_type = item.baseTypeID;
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                foreach (ItemList.BaseNonEquipmentItem item in ItemList.get().nonEquippableItems)
                                {
                                    if (item.BaseTypeName == type_str)
                                    {
                                        item_type = item.baseTypeID;
                                        found = true;
                                        break;
                                    }
                                }
                            }
                            if (!found) { item_type = -1; }
                            UpdateRarity();
                            UpdateItems();
                            shard_initialized = false; //Reset shards
                            //UpdateUI();
                        }
                    }
                    UpdateUI();
                }
                public static void UpdateRarity()
                {
                    if ((enable) && (LastEpoch_Hud.Scenes.IsGameScene()) &&
                        (!Refs_Manager.item_list.IsNullOrDestroyed()) &&
                        (Type_Initialized) &&
                        (!type_dropdown.IsNullOrDestroyed()) &&
                        (!rarity_dropdown.IsNullOrDestroyed()) &&
                        (!items_dropdown.IsNullOrDestroyed()))
                    {
                        rarity_dropdown.ClearOptions();
                        Il2CppSystem.Collections.Generic.List<Dropdown.OptionData> options = new Il2CppSystem.Collections.Generic.List<Dropdown.OptionData>();
                        options.Add(new Dropdown.OptionData { text = "Select" });
                        if ((type_dropdown.value > 0) && (item_type > -1))
                        {
                            bool has_unique = false;
                            bool has_set = false;
                            if (UniqueList.instance.IsNullOrDestroyed()) { UniqueList.getUnique(0); }
                            if (!UniqueList.instance.IsNullOrDestroyed())
                            {
                                foreach (UniqueList.Entry unique in UniqueList.instance.uniques)
                                {
                                    if (unique.baseType == item_type)
                                    {
                                        if (unique.isSetItem) { has_set = true; }
                                        else { has_unique = true; }
                                    }
                                }
                            }
                            options.Add(new Dropdown.OptionData { text = "Base Item" });
                            if (has_unique) { options.Add(new Dropdown.OptionData { text = "Unique" }); }
                            if (has_set) { options.Add(new Dropdown.OptionData { text = "Set" }); }
                            rarity_dropdown.enabled = true;
                        }
                        else { rarity_dropdown.enabled = false; }
                        rarity_dropdown.options = options;
                        rarity_dropdown.value = 0;
                        item_rarity = -1;
                    }
                }
                public static void SelectRarity()
                {
                    if (Type_Initialized)
                    {
                        int index = rarity_dropdown.value;
                        if (index < rarity_dropdown.options.Count)
                        {
                            string rarity_str = rarity_dropdown.options[index].text;
                            item_rarity = -1;
                            if (rarity_str == "Base Item") { item_rarity = 0; }
                            else if (rarity_str == "Unique") { item_rarity = 7; }
                            else if (rarity_str == "Set") { item_rarity = 8; }
                            UpdateItems();
                            //UpdateUI();
                        }
                    }
                    UpdateUI();
                }
                public static void UpdateItems()
                {
                    if ((enable) && (LastEpoch_Hud.Scenes.IsGameScene()) &&
                        (!Refs_Manager.item_list.IsNullOrDestroyed()) &&
                        (Type_Initialized) &&
                        //(!forcedrop_type_dropdown.IsNullOrDestroyed()) &&
                        //(!forcedrop_rarity_dropdown.IsNullOrDestroyed()) &&
                        (!items_dropdown.IsNullOrDestroyed()))
                    {
                        //Main.logger_instance.Msg("Update Items : Type = " + item_type + ", Rarity = " + item_rarity);
                        items_dropdown.ClearOptions();

                        Il2CppSystem.Collections.Generic.List<Dropdown.OptionData> options = new Il2CppSystem.Collections.Generic.List<Dropdown.OptionData>();
                        options.Add(new Dropdown.OptionData { text = "Select" });
                        if ((item_type > -1) && (item_rarity > -1))
                        {
                            if (item_rarity == 0)
                            {
                                bool type_found = false;
                                foreach (ItemList.BaseEquipmentItem item_t in ItemList.get().EquippableItems)
                                {
                                    if (item_t.baseTypeID == item_type)
                                    {
                                        foreach (ItemList.EquipmentItem item in item_t.subItems)
                                        {
                                            string name = item.displayName;
                                            if (name == "") { name = item.name; }
                                            options.Add(new Dropdown.OptionData { text = name });
                                        }
                                        type_found = true;
                                    }
                                }
                                if (!type_found)
                                {
                                    foreach (ItemList.BaseNonEquipmentItem item_t in ItemList.get().nonEquippableItems)
                                    {
                                        if (item_t.baseTypeID == item_type)
                                        {
                                            foreach (ItemList.NonEquipmentItem item in item_t.subItems)
                                            {
                                                string name = item.displayName;
                                                if (name == "") { name = item.name; }
                                                options.Add(new Dropdown.OptionData { text = name });
                                            }

                                            type_found = true;
                                        }
                                    }
                                }
                            }
                            else if ((item_rarity == 7) || (item_rarity == 8))
                            {
                                if (UniqueList.instance.IsNullOrDestroyed()) { UniqueList.getUnique(0); }
                                if (!UniqueList.instance.IsNullOrDestroyed())
                                {
                                    foreach (UniqueList.Entry unique in UniqueList.instance.uniques)
                                    {
                                        if ((unique.baseType == item_type) &&
                                            (((item_rarity == 7) && (!unique.isSetItem)) ||
                                            ((item_rarity == 8) && (unique.isSetItem))))
                                        {
                                            string name = unique.displayName;
                                            if (name == "") { name = unique.name; }
                                            options.Add(new Dropdown.OptionData { text = name });
                                        }
                                    }
                                }
                            }
                            items_dropdown.enabled = true;
                        }
                        else { items_dropdown.enabled = false; }
                        items_dropdown.options = options;
                        items_dropdown.value = 0;
                    }
                }
                public static void SelectItem()
                {
                    if (Type_Initialized)
                    {
                        int index = items_dropdown.value;
                        if (index < items_dropdown.options.Count)
                        {
                            string item_str = items_dropdown.options[items_dropdown.value].text;
                            //Main.logger_instance.Msg("Select : Item = " + item_str);

                            item_subtype = -1;
                            item_unique_id = 0;

                            bool item_found = false;
                            if (item_rarity == 0)
                            {
                                foreach (ItemList.BaseEquipmentItem item_t in ItemList.get().EquippableItems)
                                {
                                    if (item_t.baseTypeID == item_type)
                                    {
                                        foreach (ItemList.EquipmentItem item in item_t.subItems)
                                        {
                                            if ((item_str == item.displayName) || (item_str == item.name))
                                            {
                                                item_subtype = item.subTypeID;
                                                item_found = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (!item_found)
                                {
                                    foreach (ItemList.BaseNonEquipmentItem item_t in ItemList.get().nonEquippableItems)
                                    {
                                        if (item_t.baseTypeID == item_type)
                                        {
                                            foreach (ItemList.NonEquipmentItem item in item_t.subItems)
                                            {
                                                if ((item_str == item.displayName) || (item_str == item.name))
                                                {
                                                    item_subtype = item.subTypeID;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if ((item_rarity == 7) || (item_rarity == 8))
                            {
                                if (UniqueList.instance.IsNullOrDestroyed()) { UniqueList.getUnique(0); }
                                if (!UniqueList.instance.IsNullOrDestroyed())
                                {
                                    foreach (UniqueList.Entry unique in UniqueList.instance.uniques)
                                    {
                                        if ((item_str == unique.displayName) || (item_str == unique.name))
                                        {
                                            item_subtype = unique.subTypes[0]; //need to be fix here
                                            item_unique_id = unique.uniqueID;
                                            item_legendary_type = unique.legendaryType;
                                            break;
                                        }
                                    }
                                }
                            }
                            UpdateUI();
                        }
                    }
                }
                public static void EnableImplicits()
                {
                    int index = implicits_dropdown.value;
                    if (index < implicits_dropdown.options.Count)
                    {
                        if (index == 1) { implicits_roll = true; }
                        else { implicits_roll = false; }
                    }
                }
                public static void SetImplicit_0(float f)
                {
                    int result = System.Convert.ToInt32((implicit_0_slider.value / 255) * 100);
                    implicit_0_Text.text = result.ToString() + " %";
                }
                public static void SetImplicit_1(float f)
                {
                    int result = System.Convert.ToInt32((implicit_1_slider.value / 255) * 100);
                    implicit_1_Text.text = result.ToString() + " %";
                }
                public static void SetImplicit_2(float f)
                {
                    int result = System.Convert.ToInt32((implicit_2_slider.value / 255) * 100);
                    implicit_2_Text.text = result.ToString() + " %";
                }
                public static void EnableForginPotencial()
                {
                    int index = forgin_potencial_dropdown.value;
                    if (index < forgin_potencial_dropdown.options.Count)
                    {
                        if (index == 1) { forgin_potencial_roll = true; }
                        else { forgin_potencial_roll = false; }
                    }
                }
                public static void SetForginPotencial(float f)
                {
                    forgin_potencial_text.text = System.Convert.ToInt32(forgin_potencial_slider.value).ToString();
                }
                public static void EnableSeal()
                {
                    int index = seal_dropdown.value;
                    if (index < seal_dropdown.options.Count)
                    {
                        if (index == 1) { seal_roll = true; }
                        else { seal_roll = false; }
                    }
                }
                public static void SelectSeal()
                {
                    SetShardsView(0, true);
                }
                public static void SetSealTier(float f)
                {
                    int t = System.Convert.ToInt32(seal_tier_slider.value) + 1;
                    seal_tier_text.text = t.ToString();
                }
                public static void SetSealValue(float f)
                {
                    int result = System.Convert.ToInt32((seal_value_slider.value / 255) * 100);
                    seal_value_text.text = result.ToString() + " %";
                }
                public static void EnableAffixs()
                {
                    int index = affixs_dropdown.value;
                    if (index < affixs_dropdown.options.Count)
                    {
                        if (index == 1) { affixs_roll = true; }
                        else { affixs_roll = false; }
                    }
                }
                public static void SelectAffix_0()
                {
                    SetShardsView(0, false);
                }
                public static void SetAffix_0_Tier(float f)
                {
                    int t = System.Convert.ToInt32(affix_0_tier_slider.value) + 1;
                    affix_0_tier_text.text = t.ToString();
                }
                public static void SetAffix_0_Value(float f)
                {
                    int result = System.Convert.ToInt32((affix_0_value_slider.value / 255) * 100);
                    affix_0_value_text.text = result.ToString() + " %";
                }
                public static void SelectAffix_1()
                {
                    SetShardsView(1, false);
                }
                public static void SetAffix_1_Tier(float f)
                {
                    int t = System.Convert.ToInt32(affix_1_tier_slider.value) + 1;
                    affix_1_tier_text.text = t.ToString();
                }
                public static void SetAffix_1_Value(float f)
                {
                    int result = System.Convert.ToInt32((affix_1_value_slider.value / 255) * 100);
                    affix_1_value_text.text = result.ToString() + " %";
                }
                public static void SelectAffix_2()
                {
                    SetShardsView(2, false);
                }
                public static void SetAffix_2_Tier(float f)
                {
                    int t = System.Convert.ToInt32(affix_2_tier_slider.value) + 1;
                    affix_2_tier_text.text = t.ToString();
                }
                public static void SetAffix_2_Value(float f)
                {
                    int result = System.Convert.ToInt32((affix_2_value_slider.value / 255) * 100);
                    affix_2_value_text.text = result.ToString() + " %";
                }
                public static void SelectAffix_3()
                {
                    SetShardsView(3, false);
                }
                public static void SetAffix_3_Tier(float f)
                {
                    int t = System.Convert.ToInt32(affix_3_tier_slider.value) + 1;
                    affix_3_tier_text.text = t.ToString();
                }
                public static void SetAffix_3_Value(float f)
                {
                    int result = System.Convert.ToInt32((affix_3_value_slider.value / 255) * 100);
                    affix_3_value_text.text = result.ToString() + " %";
                }
                public static void SelectAffix_4()
                {
                    SetShardsView(4, false);
                }
                public static void SetAffix_4_Tier(float f)
                {
                    int t = System.Convert.ToInt32(affix_4_tier_slider.value) + 1;
                    affix_4_tier_text.text = t.ToString();
                }
                public static void SetAffix_4_Value(float f)
                {
                    int result = System.Convert.ToInt32((affix_4_value_slider.value / 255) * 100);
                    affix_4_value_text.text = result.ToString() + " %";
                }
                public static void SelectAffix_5()
                {
                    SetShardsView(5, false);
                }
                public static void SetAffix_5_Tier(float f)
                {
                    int t = System.Convert.ToInt32(affix_5_tier_slider.value) + 1;
                    affix_5_tier_text.text = t.ToString();
                }
                public static void SetAffix_5_Value(float f)
                {
                    int result = System.Convert.ToInt32((affix_5_value_slider.value / 255) * 100);
                    affix_5_value_text.text = result.ToString() + " %";
                }                
                public static void EnableUniqueMods()
                {
                    int index = unique_mods_dropdown.value;
                    if (index < unique_mods_dropdown.options.Count)
                    {
                        if (index == 1) { unique_mods_roll = true; }
                        else { unique_mods_roll = false; }
                    }
                }
                public static void SetUniqueMod_0(float f)
                {
                    int result = System.Convert.ToInt32((unique_mod_0_slider.value / 255) * 100);
                    unique_mod_0_Text.text = result.ToString() + " %";
                }
                public static void SetUniqueMod_1(float f)
                {
                    int result = System.Convert.ToInt32((unique_mod_1_slider.value / 255) * 100);
                    unique_mod_1_Text.text = result.ToString() + " %";
                }
                public static void SetUniqueMod_2(float f)
                {
                    int result = System.Convert.ToInt32((unique_mod_2_slider.value / 255) * 100);
                    unique_mod_2_Text.text = result.ToString() + " %";
                }
                public static void SetUniqueMod_3(float f)
                {
                    int result = System.Convert.ToInt32((unique_mod_3_slider.value / 255) * 100);
                    unique_mod_3_Text.text = result.ToString() + " %";
                }
                public static void SetUniqueMod_4(float f)
                {
                    int result = System.Convert.ToInt32((unique_mod_4_slider.value / 255) * 100);
                    unique_mod_4_Text.text = result.ToString() + " %";
                }
                public static void SetUniqueMod_5(float f)
                {
                    int result = System.Convert.ToInt32((unique_mod_5_slider.value / 255) * 100);
                    unique_mod_5_Text.text = result.ToString() + " %";
                }
                public static void SetUniqueMod_6(float f)
                {
                    int result = System.Convert.ToInt32((unique_mod_6_slider.value / 255) * 100);
                    unique_mod_6_Text.text = result.ToString() + " %";
                }
                public static void SetUniqueMod_7(float f)
                {
                    int result = System.Convert.ToInt32((unique_mod_7_slider.value / 255) * 100);
                    unique_mod_7_Text.text = result.ToString() + " %";
                }
                public static void EnableLegendaryPotencial()
                {
                    int index = legenday_potencial_dropdown.value;
                    if (index < legenday_potencial_dropdown.options.Count)
                    {
                        if (index == 1) { legenday_potencial_roll = true; }
                        else { legenday_potencial_roll = false; }
                    }
                }
                public static void SetLegendayPotencial(float f)
                {
                    legenday_potencial_Text.text = System.Convert.ToInt32(legenday_potencial_slider.value).ToString();
                }
                public static void EnableWeaverWill()
                {
                    int index = weaver_will_dropdown.value;
                    if (index < weaver_will_dropdown.options.Count)
                    {
                        if (index == 1) { weaver_will_roll = true; }
                        else { weaver_will_roll = false; }
                    }
                }
                public static void SetWeaverWill(float f)
                {
                    weaver_will_Text.text = System.Convert.ToInt32(weaver_will_slider.value).ToString();
                }

                public static void SetShardsView(int affix_number, bool seal)
                {
                    shard_seal = seal;
                    shard_number = affix_number;
                    if (!shard_initialized) { InitializeShardsView(); }                    
                }
                public static void InitializeShardsView()
                {
                    RemoveShardsInView();
                    bool filter_by_type = false;                    
                    AffixList.AffixType wanted_type = AffixList.AffixType.PREFIX;
                    if (shards_filter_type.value > 0)
                    {
                        filter_by_type = true;
                        if (shards_filter_type.value == 1) { wanted_type = AffixList.AffixType.PREFIX; }
                        else if (shards_filter_type.value == 2) { wanted_type = AffixList.AffixType.SUFFIX; }
                    }
                    bool filter_by_class = false;
                    AffixList.ClassSpecificity wanted_class = AffixList.ClassSpecificity.None;
                    if (shards_filter_class.value > 0)
                    {
                        filter_by_class = true;
                        if (shards_filter_class.value == 1) { wanted_class = AffixList.ClassSpecificity.NonSpecific; }
                        else if (shards_filter_class.value == 2) { wanted_class = AffixList.ClassSpecificity.Primalist; }
                        else if (shards_filter_class.value == 3) { wanted_class = AffixList.ClassSpecificity.Mage; }
                        else if (shards_filter_class.value == 4) { wanted_class = AffixList.ClassSpecificity.Sentinel; }
                        else if (shards_filter_class.value == 5) { wanted_class = AffixList.ClassSpecificity.Acolyte; }
                        else if (shards_filter_class.value == 6) { wanted_class = AffixList.ClassSpecificity.Rogue; }                        
                    }
                    bool filter_by_name = false;
                    string wanted_name = "";
                    if (shards_filter_name.text != "")
                    {
                        filter_by_name = true;
                        wanted_name = shards_filter_name.text;
                    }
                    bool item_idol = false;
                    if ((item_type > 24) && (item_type < 34)) { item_idol = true; }
                    foreach (AffixList.SingleAffix affix in AffixList.instance.singleAffixes)
                    {
                        bool affix_idol = false;
                        if (affix.affixName.Contains("Idol ")) { affix_idol = true; }

                        if (((item_idol && affix_idol) || (!item_idol && !affix_idol)) &&
                            (((filter_by_name) && (affix.affixName.ToLower().Contains(wanted_name.ToLower()))) || (!filter_by_name)) &&
                            (((filter_by_type) && (affix.type == wanted_type)) || (!filter_by_type)) &&
                            (((filter_by_class) && (affix.classSpecificity == wanted_class)) || (!filter_by_class))
                            )
                        {
                            AddShardInView(affix.affixId, affix.affixName);
                        }
                    }
                    foreach (AffixList.MultiAffix affix in AffixList.instance.multiAffixes)
                    {
                        bool affix_idol = false;
                        if (affix.affixName.Contains("Idol ")) { affix_idol = true; }

                        if (((item_idol && affix_idol) || (!item_idol && !affix_idol)) &&
                            (((filter_by_name) && (affix.affixName.ToLower().Contains(wanted_name.ToLower()))) || (!filter_by_name)) &&
                            (((filter_by_type) && (affix.type == wanted_type)) || (!filter_by_type)) &&
                            (((filter_by_class) && (affix.classSpecificity == wanted_class)) || (!filter_by_class))
                            )
                        {
                            AddShardInView(affix.affixId, affix.affixName);
                        }
                    }
                    shard_initialized = true;
                }
                public static void RemoveShardsInView()
                {
                    foreach (GameObject go in Functions.GetAllChild(center_content))
                    {
                        Destroy(go);
                    }
                        
                }
                public static void AddShardInView(int id, string name)
                {
                    GameObject g = Object.Instantiate(shard_prefab, Vector3.zero, Quaternion.identity);
                    g.transform.SetParent(center_content.transform);
                    GameObject shard_btn_object = Functions.GetChild(g, "shard_btn");
                    Button shard_btn = shard_btn_object.GetComponent<Button>();
                    shard_btn.name = shard_btn_name + id;
                    GameObject shard_id_object = Functions.GetChild(shard_btn_object, "shard_id");
                    Text shard_id = shard_id_object.GetComponent<Text>();
                    shard_id.text = id.ToString();
                    GameObject shard_name_object = Functions.GetChild(shard_btn_object, "shard_name");
                    Text shard_name = shard_name_object.GetComponent<Text>();
                    shard_name.text = name.ToString();
                }
                public static void SelectShard(int id, string name)
                {
                    if (shard_seal) { seal_id = id; seal_name = name; }
                    else
                    {
                        if (shard_number == 0) { affix_0_id = id; affix_0_name = name; }
                        else if (shard_number == 1) { affix_1_id = id; affix_1_name = name; }
                        else if (shard_number == 2) { affix_2_id = id; affix_2_name = name; }
                        else if (shard_number == 3) { affix_3_id = id; affix_3_name = name; }
                        else if (shard_number == 4) { affix_4_id = id; affix_4_name = name; }
                        else if (shard_number == 5) { affix_5_id = id; affix_5_name = name; }
                    }
                }
                public static ItemAffix MakeAffix(int id, byte tier, byte roll, bool seal)
                {
                    ItemAffix new_affix = null;
                    if (id > -1)
                    {
                        bool found = false;
                        foreach (AffixList.SingleAffix affix in AffixList.instance.singleAffixes)
                        {
                            if (id == affix.affixId)
                            {
                                new_affix = new ItemAffix
                                {
                                    affixId = (ushort)affix.affixId,
                                    affixName = affix.affixName,
                                    affixTitle = affix.affixTitle,
                                    affixType = affix.type,
                                    isSealedAffix = seal,
                                    affixTier = tier,
                                    affixRoll = roll
                                };
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            foreach (AffixList.MultiAffix affix in AffixList.instance.multiAffixes)
                            {
                                if (id == affix.affixId)
                                {
                                    new_affix = new ItemAffix
                                    {
                                        affixId = (ushort)affix.affixId,
                                        affixName = affix.affixName,
                                        affixTitle = affix.affixTitle,
                                        affixType = affix.type,
                                        isSealedAffix = seal,
                                        affixTier = tier,
                                        affixRoll = roll
                                    };
                                    break;
                                }
                            }
                        }
                    }

                    return new_affix;
                }

                public static void UpdateUI()
                {
                    implicits_enable = false;
                    seal_enable = false;
                    forgin_potencial_enable = false;
                    affixs_enable = false;
                    unique_mods_enable = false;
                    legenday_potencial_enable = false;
                    weaver_will_enable = false;
                    quantity_enable = false;
                    btn_enable = false;

                    if ((type_dropdown.value > 0) && (rarity_dropdown.value > 0) && (items_dropdown.value > 0))
                    {
                        implicits_enable = true;
                        if (item_type < 100) { seal_enable = true; affixs_enable = true; }
                        if ((item_type < 100) && (item_rarity < 7)) { forgin_potencial_enable = true; }
                        if (item_rarity > 6)
                        {
                            unique_mods_enable = true;
                            if (item_legendary_type == UniqueList.LegendaryType.LegendaryPotential) { legenday_potencial_enable = true; }
                            weaver_will_enable = !legenday_potencial_enable;
                        }
                        quantity_enable = true;
                        btn_enable = true;
                    }
                    else //Reset all dropdown
                    {
                        implicits_dropdown.value = 0;
                        forgin_potencial_dropdown.value = 0;
                        seal_dropdown.value = 0;
                        affixs_dropdown.value = 0;
                        unique_mods_dropdown.value = 0;
                        legenday_potencial_dropdown.value = 0;
                        weaver_will_dropdown.value = 0;
                    }
                }
                public static void Drop()
                {
                    if ((btn_enable) && (!Refs_Manager.ground_item_manager.IsNullOrDestroyed()) && (!Refs_Manager.player_actor.IsNullOrDestroyed()))
                    {
                        for (int i = 0; i < forcedrop_quantity_slider.value; i++)
                        {
                            //Rarity
                            byte ra = (byte)item_rarity;

                            //Forgin potencial
                            byte fg = 0;
                            if ((item_type < 100) && (ra < 7))
                            {                                
                                if (forgin_potencial_roll) { fg = (byte)forgin_potencial_slider.value; }
                                else { fg = (byte)Random.RandomRange(0f, 255f); } //Random
                            }

                            //Affixes
                            bool sa = false; //Seal
                            byte an = 0; //Affix numbers
                            List<ItemAffix> af = new List<ItemAffix>(); //Affixes
                            if (seal_roll)
                            {
                                if (seal_id > -1)
                                {
                                    ItemAffix affix = MakeAffix(seal_id, (byte)seal_tier_slider.value, (byte)seal_value_slider.value, true);
                                    if (!affix.IsNullOrDestroyed())
                                    {
                                        sa = true;
                                        an = 1; //Set affix number
                                        af.Add(affix);
                                    }
                                    else { Main.logger_instance.Error("Seal is null"); }
                                }                                
                            }
                            if (affixs_roll)
                            {
                                System.Collections.Generic.List<ItemAffix> new_affixes = new System.Collections.Generic.List<ItemAffix>();
                                if (affix_0_id > -1)
                                {
                                    new_affixes.Add(MakeAffix(affix_0_id, (byte)affix_0_tier_slider.value, (byte)affix_0_value_slider.value, false));
                                }
                                if (affix_1_id > -1)
                                {
                                    new_affixes.Add(MakeAffix(affix_1_id, (byte)affix_1_tier_slider.value, (byte)affix_1_value_slider.value, false));
                                }
                                if (affix_2_id > -1)
                                {
                                    new_affixes.Add(MakeAffix(affix_2_id, (byte)affix_2_tier_slider.value, (byte)affix_2_value_slider.value, false));
                                }
                                if (affix_3_id > -1)
                                {
                                    new_affixes.Add(MakeAffix(affix_3_id, (byte)affix_3_tier_slider.value, (byte)affix_3_value_slider.value, false));
                                }
                                if (affix_4_id > -1)
                                {
                                    new_affixes.Add(MakeAffix(affix_4_id, (byte)affix_4_tier_slider.value, (byte)affix_4_value_slider.value, false));
                                }
                                if (affix_5_id > -1)
                                {
                                    new_affixes.Add(MakeAffix(affix_5_id, (byte)affix_5_tier_slider.value, (byte)affix_5_value_slider.value, false));
                                }
                                
                                byte new_count = 0;
                                foreach (ItemAffix a in new_affixes)
                                {
                                    af.Add(a);
                                    new_count++;
                                }
                                new_affixes.Clear();
                                an += new_count; //Set affix number
                                if (ra < 7) { ra = new_count; } //Set rarity to affix numbers for base item only
                                //else if (an > 0) { ra = 9; }//Set rarity to legendary if seal or affix
                                else if (new_count > 0) { ra = 9; } //Set rarity to legendary if affix only
                            }

                            //Unique
                            byte lp = 0; //Legendary potencial
                            byte ww = 0; //Weaver will
                            if (ra > 6)                            
                            {
                                if (item_legendary_type == UniqueList.LegendaryType.LegendaryPotential)
                                {                                    
                                    if (legenday_potencial_roll)
                                    {
                                        lp = (byte)legenday_potencial_slider.value;
                                    }
                                    else { lp = (byte)Random.RandomRange(0f, 4f); } //Random
                                }
                                else
                                {                                    
                                    if (weaver_will_roll)
                                    {
                                        ww = (byte)weaver_will_slider.value;
                                    }
                                    else { ww = (byte)Random.RandomRange(0f, 28f); } //Random
                                }                                
                            }

                            //Create item
                            ItemDataUnpacked item = new ItemDataUnpacked
                            {
                                LvlReq = 0,
                                classReq = ItemList.ClassRequirement.Any,
                                itemType = (byte)item_type,
                                subType = (ushort)item_subtype,
                                rarity = (byte)ra,                                
                                forgingPotential = fg,
                                hasSealedAffix = sa,
                                sockets = (byte)an,
                                affixes = af,
                                uniqueID = (ushort)item_unique_id,
                                legendaryPotential = lp,
                                weaversWill = ww
                            };

                            //Set Implicits
                            if (implicits_roll)
                            {
                                item.implicitRolls[0] = (byte)implicit_0_slider.value;
                                item.implicitRolls[1] = (byte)implicit_1_slider.value;
                                item.implicitRolls[2] = (byte)implicit_2_slider.value;
                            }
                            else //Random
                            {
                                for (int k = 0; k < item.implicitRolls.Count; k++)
                                {
                                    item.implicitRolls[k] = (byte)Random.RandomRange(0f, 255f);
                                }
                            }

                            //Set Unique mods
                            if (item.isUniqueSetOrLegendary())
                            {
                                if (unique_mods_roll)
                                {
                                    item.uniqueRolls[0] = (byte)unique_mod_0_slider.value;
                                    item.uniqueRolls[1] = (byte)unique_mod_1_slider.value;
                                    item.uniqueRolls[2] = (byte)unique_mod_2_slider.value;
                                    item.uniqueRolls[3] = (byte)unique_mod_3_slider.value;
                                    item.uniqueRolls[4] = (byte)unique_mod_4_slider.value;
                                    item.uniqueRolls[5] = (byte)unique_mod_5_slider.value;
                                    item.uniqueRolls[6] = (byte)unique_mod_6_slider.value;
                                    item.uniqueRolls[7] = (byte)unique_mod_7_slider.value;
                                }
                                else //Random
                                {
                                    for (int k = 0; k < item.uniqueRolls.Count; k++)
                                    {
                                        item.uniqueRolls[k] = (byte)Random.RandomRange(0f, 255f);
                                    }
                                }
                            }
                            item.RefreshIDAndValues(); //Refresh item for implicits and unique mods
                            
                            Refs_Manager.ground_item_manager.dropItemForPlayer(Refs_Manager.player_actor, item.TryCast<ItemData>(), Refs_Manager.player_actor.position(), false);
                        }
                    }
                }
            }
            public class Headhunter
            {
                public static GameObject content_obj = null;
                public static bool enable = false;

                public static void Get_Refs()
                {
                    content_obj = Functions.GetChild(Content.content_obj, "Headhunter_Content");
                }
                public static void Set_Active(bool show)
                {
                    if (!content_obj.IsNullOrDestroyed())
                    {
                        content_obj.active = show;
                        enable = show;
                    }
                }
                public static void Toggle_Active()
                {
                    if (!content_obj.IsNullOrDestroyed())
                    {
                        bool show = !content_obj.active;
                        content_obj.active = show;
                        enable = show;
                    }
                }
                public static bool Init_Data()
                {
                    bool result = false;



                    return result;
                }
                public static void UpdateVisuals()
                {
                    if (!Save_Manager.instance.IsNullOrDestroyed())
                    {
                        if ((Save_Manager.instance.initialized) && (!Save_Manager.instance.data.IsNullOrDestroyed()))
                        {

                        }
                    }
                }
            }
        }
    }
}