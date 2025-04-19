using HarmonyLib;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.Mods.Skills
{
    public class Skills_Level
    {
        public static bool CanRun()
        {
            if ((Scenes.IsGameScene()) && (!Save_Manager.instance.IsNullOrDestroyed()) && (!Refs_Manager.player_treedata.IsNullOrDestroyed()))
            {
                if ((!Save_Manager.instance.data.IsNullOrDestroyed()) && (!Refs_Manager.player_treedata.specialisedSkillTrees.IsNullOrDestroyed()))
                {
                    return Save_Manager.instance.data.Skills.Enable_SkillLevel;
                }
                else { return false; }
            }
            else { return false; }
        }

        [HarmonyPatch(typeof(AbilityXPBar), "updateBar")]
        public class AbilityXPBar_updateBar
        {
            [HarmonyPrefix]
            static bool Prefix(ref AbilityXPBar __instance, ref byte __0, ref int __1)
            {
                //Main.logger_instance.Msg("AbilityXPBar.updateBar() Prefix Level = " + __0 + " Xp = " + __1);
                try
                {
                    if (__instance.IsNullOrDestroyed()) { return false; }
                    else
                    {
                        //if (__0 > 20) { __0 = 20; }
                        //if (__1 > 5700000) { __1 = 5700000; }
                        return true;
                    }
                }
                catch
                {
                    //Main.logger_instance.Error("AbilityXPBar.updateBar() Instance is null");
                    return false;
                }
            }
            /*[HarmonyPostfix]
            static void Postfix(byte __0, int __1)
            {
                Main.logger_instance.Msg("AbilityXPBar.updateBar() Postfix Level = " + __0 + " Xp = " + __1);
            }*/
        }
        
        [HarmonyPatch(typeof(UITreeSkillName), "SetAbility")]
        public class UITreeSkillName_SetAbility
        {
            [HarmonyPrefix]
            static bool Prefix(ref UITreeSkillName __instance, Ability __0)
            {
                //Main.logger_instance.Msg("UITreeSkillName.SetAbility() Prefix Ability = " + __0.abilityName);
                try
                {
                    if (__instance.IsNullOrDestroyed()) { return false; }
                    else { return true; }
                }
                catch
                {
                    //Main.logger_instance.Error("UITreeSkillName.SetAbility() Instance is null");
                    return false;
                }
            }
            /*[HarmonyPostfix]
            static void Postfix(Ability __0)
            {
                Main.logger_instance.Msg("UITreeSkillName.SetAbility() Postfix Ability = " + __0.abilityName);
            }*/
        }

        [HarmonyPatch(typeof(UITreeSkillLevel), "updateForAbility")]
        public class UITreeSkillLevel_updateForAbility
        {
            [HarmonyPrefix]
            static bool Prefix(ref UITreeSkillLevel __instance, Ability __0, bool __1)
            {
                //Main.logger_instance.Msg("UITreeSkillLevel.updateForAbility() Prefix Ability = " + __0.abilityName + ", Locked = " + __1);
                try
                {                    
                    if (__instance.IsNullOrDestroyed()) { return false; }
                    else { return true; }
                }
                catch
                {
                    //Main.logger_instance.Error("UITreeSkillLevel.updateForAbility() Instance is null");
                    return false;
                }
            }
            /*[HarmonyPostfix]
            static void Postfix(Ability __0, bool __1)
            {
                Main.logger_instance.Msg("UITreeSkillLevel.updateForAbility() Postfix Ability = " + __0.abilityName + ", Locked = " + __1);
            }*/
        }

        /*[HarmonyPatch(typeof(TreeUIManager), "onSkillTreeOpened")]
        public class TreeUIManager_onSkillTreeOpened
        {
            [HarmonyPrefix]
            static void Prefix(ref SkillTree __0)
            {
                Main.logger_instance.Msg("TreeUIManager.onSkillTreeOpened() Prefix");
            }
            [HarmonyPostfix]
            static void Postfix(ref SkillTree __0)
            {
                Main.logger_instance.Msg("TreeUIManager.onSkillTreeOpened() Postfix");
            }
        }*/

        [HarmonyPatch(typeof(SkillsPanelManager), "openSkillTree")]
        public class SkillsPanelManager_openSkillTree
        {
            [HarmonyPrefix]
            static bool Prefix(ref SkillsPanelManager __instance, Ability __0)
            {
                //Main.logger_instance.Msg("SkillsPanelManager.openSkillTree() Prefix Ability = " + __0.abilityName);                
                bool r = true;
                try
                {
                    if (!__instance.IsNullOrDestroyed())
                    {
                        if ((CanRun()) && (!__0.IsNullOrDestroyed()))
                        {
                            if (!Refs_Manager.player_treedata.specialisedSkillTrees.IsNullOrDestroyed())
                            {
                                foreach (LocalTreeData.SkillTreeData skill_tree_data in Refs_Manager.player_treedata.specialisedSkillTrees)
                                {
                                    if (!skill_tree_data.ability.IsNullOrDestroyed())
                                    {
                                        if (skill_tree_data.ability.abilityName == __0.abilityName)
                                        {
                                            skill_tree_data.level = (byte)Save_Manager.instance.data.Skills.SkillLevel;
                                            break;
                                        }
                                    }
                                }
                            }
                            __instance.updateVisuals(false);
                        }
                    }
                }
                catch
                {
                    r = false;
                    //Main.logger_instance.Msg("SkillsPanelManager.openSkillTree() ERROR");
                }
                return r;
            }
            /*[HarmonyPostfix]
            static void Postfix(Ability __0)
            {
                Main.logger_instance.Msg("SkillsPanelManager.openSkillTree() Postfix Ability = " + __0.abilityName);

            }*/
        }
    }
}
