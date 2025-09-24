﻿using CellMenu;
using Hikaria.QC;
using Player;
using SNetwork;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Members;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;

namespace Hikaria.AdminSystem.Features.Misc
{
    [DoNotSaveToConfig]
    [EnableFeatureByDefault]
    [DisallowInGameToggle]
    public class SwapGear : Feature
    {
        public override string Name => "游戏内变更装备";

        public override string Description => "允许玩家在游戏内更换装备";

        public override GroupBase Group => ModuleGroup.GetOrCreateSubGroup("Misc");

        [FeatureConfig]
        public static SwapGearInLevelSettings Settings { get; set; }

        public class SwapGearInLevelSettings
        {
            [FSDisplayName("解锁装备")]
            public bool EnableSwapGearInLevel { get => _enableSwapGearInLevel; set => _enableSwapGearInLevel = value; }
        }

        [Command("SwapGearInLevel")]
        private static bool _enableSwapGearInLevel = true;


        [ArchivePatch(typeof(CM_PlayerLobbyBar), nameof(CM_PlayerLobbyBar.HideLoadoutUI))]
        private class CM_PlayerLobbyBar__HideLoadoutUI__Patch
        {
            private static void Prefix(ref bool hide)
            {
                if (!_enableSwapGearInLevel)
                {
                    return;
                }
                if (hide)
                {
                    hide = false;
                }
            }
        }

        [ArchivePatch(typeof(CM_InventorySlotItem), nameof(CM_InventorySlotItem.LoadData))]
        private class CM_InventorySlotItem__LoadData__Patch
        {
            private static void Prefix(CM_InventorySlotItem __instance, ref bool clickable)
            {
                if (!_enableSwapGearInLevel)
                {
                    return;
                }
                if (!clickable)
                {
                    var player = __instance.m_parentBar.m_player;
                    clickable = player.IsLocal || SNet.IsMaster && player.IsBot;
                }
            }
        }

        [ArchivePatch(typeof(PlayerBackpackManager), nameof(PlayerBackpackManager.ResetLocalAmmoStorage))]
        private class PlayerBackpackManager__ResetLocalAmmoStorage__Patch
        {
            private static bool Prefix()
            {
                if (!_enableSwapGearInLevel)
                {
                    return true;
                }
                return CurrentGameState != (int)eGameStateName.InLevel;
            }
        }

        [ArchivePatch(typeof(PlayerBackpackManager), nameof(PlayerBackpackManager.EquipLocalGear))]
        private class PlayerBackpackManager__EquipLocalGear__Patch
        {
            private static void Prefix()
            {
                if (!_enableSwapGearInLevel || CurrentGameState != (int)eGameStateName.InLevel)
                {
                    return;
                }
                SavePrevAmmoPercent();
                WaitingForGearEquiped = true;
            }
        }

        [ArchivePatch(typeof(PlayerSync), nameof(PlayerSync.WantsToWieldSlot))]
        private class PlayerSync__WantsToWieldSlot__Postfix
        {
            private static void Postfix(PlayerSync __instance)
            {
                if (!__instance.m_agent.Owner.IsLocal || !_enableSwapGearInLevel || CurrentGameState != (int)eGameStateName.InLevel || !WaitingForGearEquiped)
                {
                    return;
                }
                RestoreAmmoPercent();
                WaitingForGearEquiped = false;
            }
        }

        private static void SavePrevAmmoPercent()
        {
            PlayerAmmoStorage ammoStorage = PlayerBackpackManager.LocalBackpack.AmmoStorage;
            foreach (InventorySlot slot in ValidSlots)
            {
                InventorySlotAmmo inventorySlotAmmo = ammoStorage.GetInventorySlotAmmo(slot);
                float clipAmmoFromSlot = ammoStorage.GetClipAmmoFromSlot(slot) * (slot == InventorySlot.GearClass ? 0f : inventorySlotAmmo.CostOfBullet);
                PrevAmmoInPack[slot] = clipAmmoFromSlot + inventorySlotAmmo.AmmoInPack;
            }
        }

        private static void RestoreAmmoPercent()
        {
            PlayerAmmoStorage ammoStorage = PlayerBackpackManager.LocalBackpack.AmmoStorage;
            foreach (InventorySlot slot in ValidSlots)
            {
                InventorySlotAmmo inventorySlotAmmo = ammoStorage.GetInventorySlotAmmo(slot);
                inventorySlotAmmo.AmmoInPack = PrevAmmoInPack[slot];
                ammoStorage.SetClipAmmoInSlot(slot);
                ammoStorage.UpdateSlotAmmoUI(slot);
                ammoStorage.NeedsSync = true;
            }
        }

        private static bool WaitingForGearEquiped;

        private static readonly Dictionary<InventorySlot, float> PrevAmmoInPack = new();

        private static readonly List<InventorySlot> ValidSlots = new()
        {
            InventorySlot.GearStandard,
            InventorySlot.GearSpecial,
            InventorySlot.GearClass
        };
    }
}
