using HarmonyLib;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.Mods.Cosmetics
{
    public class Cosmetics_Mtx
    {
        /*
        public static UIPanel mtx_shop_panel = null;
        public static Il2CppLE.UI.MTXStore.MTXStoreController mtx_store_controller = null;

        private static void SetupMtxRef()
        {
            if (!Refs_Manager.game_uibase.IsNullOrDestroyed())
            {
                if (!Refs_Manager.game_uibase.mtxShopPanel.IsNullOrDestroyed())
                {
                    mtx_shop_panel = Refs_Manager.game_uibase.mtxShopPanel;
                }
            }
        }
        public static void OpenMtx()
        {
            if (mtx_shop_panel.IsNullOrDestroyed()) { SetupMtxRef(); }
            if (!mtx_shop_panel.IsNullOrDestroyed())
            {
                mtx_shop_panel.instance.active = true;
                mtx_shop_panel.isOpen = true;
            }
        }*/
        /*private static readonly System.Action CloseMtxAction = new System.Action(CloseMtx);
        public static void CloseMtx()
        {
            if (mtx_shop_panel.IsNullOrDestroyed()) { SetupMtxRef(); }
            if (!mtx_shop_panel.IsNullOrDestroyed())
            {
                mtx_shop_panel.instance.active = false;
                mtx_shop_panel.isOpen = false;
            }
        }*/
        /*[HarmonyPatch(typeof(Il2CppLE.UI.MTXStore.MTXStoreController), "Init")]
        public class MTXStoreController_Init
        {
            [HarmonyPostfix]
            static void Postfix(ref Il2CppLE.UI.MTXStore.MTXStoreController __instance)
            {
                Main.logger_instance.Msg("MTXStoreController : Init");
                mtx_store_controller = __instance;
                //__instance.loadingUIOverlay.active = false;
                //__instance.loadingStore = false;
            }
        }*/
        /*[HarmonyPatch(typeof(Il2CppLE.UI.MTXStore.MTXStoreController), "OnEnterState")]
        public class MTXStoreController_OnEnterState
        {
            [HarmonyPostfix]
            static void Postfix(ref Il2CppLE.UI.MTXStore.MTXStoreController __instance, Il2CppLE.UI.MTXStore.MTXStoreController.MTXStoreUIState __0, Il2CppLE.UI.MTXStore.MTXStoreController.MTXStoreUIState __1)
            {
                Main.logger_instance.Msg("MTXStoreController : OnEnterState : __0 = " + __0.ToString() + ", __1 = " + __1.ToString());
                //mtx_store_controller = __instance;
                //__instance.loadingUIOverlay.active = false;
                //__instance.loadingStore = false;
            }
        }*/
        /*HarmonyPatch(typeof(Il2CppLE.UI.MTXStore.MTXStoreController), "LoadStore")]
        public class MTXStoreController_LoadStore
        {
            [HarmonyPostfix]
            static void Postfix(Il2CppLE.UI.MTXStore.MTXStoreController __instance, Il2CppCysharp.Threading.Tasks.UniTask __result)
            {
                Main.logger_instance.Msg("MTXStoreController : LoadStore");
            }
        }*/
        /*[HarmonyPatch(typeof(Il2CppLE.UI.MTXStore.MTXStoreController), "LoadInventory")]
        public class MTXStoreController_LoadInventory
        {
            [HarmonyPostfix]
            static void Postfix(Il2CppLE.UI.MTXStore.MTXStoreController __instance, Il2CppCysharp.Threading.Tasks.UniTask __result)
            {
                Main.logger_instance.Msg("MTXStoreController : LoadInventory");
            }
        }*/
        /*[HarmonyPatch(typeof(Il2CppLE.UI.MTXStore.MTXStoreController), "PopulateStoreItems")]
        public class MTXStoreController_PopulateStoreItems
        {
            [HarmonyPostfix]
            static void Postfix(Il2CppLE.UI.MTXStore.MTXStoreController __instance)
            {
                Main.logger_instance.Msg("MTXStoreController : PopulateStoreItems");
            }
        }*/
        /*[HarmonyPatch(typeof(Il2CppLE.UI.MTXStore.MTXStoreController), "OnInventoryUpdated")]
        public class MTXStoreController_OnInventoryUpdated
        {
            [HarmonyPostfix]
            static void Postfix(Il2CppLE.UI.MTXStore.MTXStoreController __instance, Il2CppLE.MicrotransactionSystem.UserInventory __0)
            {
                Main.logger_instance.Msg("MTXStoreController : OnInventoryUpdated");
                //Il2CppLE.MicrotransactionSystem.UserInventory new_user_inventory = new Il2CppLE.MicrotransactionSystem.UserInventory();
                //harmony to write private field
            }
        }
        */
    }
}
