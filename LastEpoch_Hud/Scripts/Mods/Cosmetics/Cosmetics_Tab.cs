using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Cosmetics
{
    public class Cosmetics_Tab
    {
        public static TabUIElement tab_element = null;        

        [HarmonyPatch(typeof(InventoryPanelUI), "OnEnable")]
        public class InventoryPanelUI_OnEnable
        {
            [HarmonyPostfix]
            static void Postfix(ref InventoryPanelUI __instance)
            {
                if (!Refs_Manager.InventoryPanelUI.IsNullOrDestroyed())
                {
                    if (tab_element.IsNullOrDestroyed())
                    {
                        if (!Refs_Manager.InventoryPanelUI.tabController.IsNullOrDestroyed())
                        {
                            foreach (TabUIElement tab in Refs_Manager.InventoryPanelUI.tabController.tabElements)
                            {
                                if (tab.gameObject.name == "AppearanceTab") { tab_element = tab; break; }
                            }
                        }
                    }
                    if (!tab_element.IsNullOrDestroyed())
                    {
                        tab_element.isDisabled = false; //Enable Tab
                        tab_element.canvasGroup.TryCast<Behaviour>().enabled = false; //Hide Mask
                    }
                }
            }
        }
    }
}
