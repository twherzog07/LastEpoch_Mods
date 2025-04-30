using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Shrines
{
    public class Shrines_ClickUnlimited
    {
        public static bool enable = true;

        [HarmonyPatch(typeof(WorldObjectClickListener), "ObjectClick")]
        public class WorldObjectClickListener_ObjectClick
        {
            [HarmonyPostfix]
            static void Postfix(ref WorldObjectClickListener __instance, UnityEngine.GameObject __0, bool __1)
            {
                Main.logger_instance.Msg("DisableComponentInteraction : " + __instance.gameObject.name);
                if ((__instance.gameObject.name.ToLower().Contains("shrine")) && (__1 == true) && (enable))
                { 
                    GameObject copy = GameObject.Instantiate(__instance.gameObject);
                    Vector3 position = __instance.gameObject.transform.position;
                    Object.Destroy(__instance.gameObject);
                    ShrinePlacementManager shrine_placement_manager = GameObject.FindObjectOfType<ShrinePlacementManager>();
                    if (!shrine_placement_manager.IsNullOrDestroyed()) { shrine_placement_manager.PlaceNewShrine(copy, position); }
                    else { Main.logger_instance.Error("ShrinePlacementManager not Found"); }
                }
            }
        }
    }
}
