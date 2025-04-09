using Il2Cpp;

namespace LastEpoch_Hud.Scripts.Mods.Cosmetics
{
    public class Cosmetics_Visual
    {
        public static void EquipSkin()
        {
            if (Cosmetics_Flyout.selected_slot == 50)
            {
                PlayerFinder.getPlayerVisuals().GetComponent<EquipmentVisualsManager>().EquipWeapon(Cosmetics_Flyout.selected_type, Cosmetics_Flyout.selected_subtype, Cosmetics_Flyout.selected_rarity, (ushort)Cosmetics_Flyout.selected_unique, IMSlotType.MainHand, WeaponEffect.None);
            }
            else if (Cosmetics_Flyout.selected_slot == 99)
            {
                PlayerFinder.getPlayerVisuals().GetComponent<EquipmentVisualsManager>().EquipWeapon(Cosmetics_Flyout.selected_type, Cosmetics_Flyout.selected_subtype, Cosmetics_Flyout.selected_rarity, (ushort)Cosmetics_Flyout.selected_unique, IMSlotType.OffHand, WeaponEffect.None);
            }
            else
            {
                bool isUnique = false;
                if (Cosmetics_Flyout.selected_rarity > 6) { isUnique = true; }
                PlayerFinder.getPlayerVisuals().GetComponent<EquipmentVisualsManager>().EquipGear((EquipmentType)Cosmetics_Flyout.selected_type, Cosmetics_Flyout.selected_subtype, isUnique, (ushort)Cosmetics_Flyout.selected_unique);
            }
        }
        public static void RemoveSkin()
        {
            PlayerFinder.getPlayerVisuals().GetComponent<EquipmentVisualsManager>().RemoveGear((byte)Cosmetics_Flyout.selected_type);
        }
    }
}
