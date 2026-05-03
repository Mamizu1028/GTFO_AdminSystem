using GameData;
using Hikaria.AdminSystem.Extensions;
using Hikaria.AdminSystem.Suggestions.Suggestors.Attributes;
using Hikaria.AdminSystem.Suggestions.Suggestors.Tags;
using Hikaria.AdminSystem.Utilities;
using Hikaria.AdminSystem.Utility;
using Hikaria.QC;
using Il2CppInterop.Runtime;
using LevelGeneration;
using Player;
using SNetwork;
using System.Text;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using UnityEngine;

namespace Hikaria.AdminSystem.Features.Item;

[DoNotSaveToConfig]
[EnableFeatureByDefault]
[DisallowInGameToggle]
[HideInModSettings]
public class ItemLookup : Feature
{
    public override string Name => "物品";

    public override GroupBase Group => ModuleGroup.GetOrCreateSubGroup("Item");

    public static Dictionary<string, ItemInLevel> ItemsInLevel { get; } = new();

    [ArchivePatch(typeof(LG_GenericTerminalItem), nameof(LG_GenericTerminalItem.Setup))]
    private class LG_GenericTerminalItem__Setup__Patch
    {
        private static void Postfix(LG_GenericTerminalItem __instance)
        {
            var itemInLevel = __instance.GetComponentInParent<ItemInLevel>();
            if (itemInLevel == null)
                return;
            var key = __instance.TerminalItemKey.ToUpperInvariant();
            ItemsInLevel[key] = itemInLevel;
            var sync = itemInLevel.GetSyncComponent()?.TryCast<LG_PickupItem_Sync>();
            if (sync == null)
                return;
            sync.OnSyncStateChange += new Action<ePickupItemStatus, pPickupPlacement, PlayerAgent, bool>((status, placement, player, isRecall) =>
            {
                if (status == ePickupItemStatus.PickedUp)
                {
                    ItemsInLevel.Remove(key);
                }
                else if (status == ePickupItemStatus.PlacedInLevel)
                {
                    ItemsInLevel[key] = itemInLevel;
                }
            });
        }
    }

    public override void OnGameStateChanged(int state)
    {
        if (state == (int)eGameStateName.AfterLevel)
        {
            ItemsInLevel.Clear();
        }
    }

    [Command("PickupItem")]
    private static void PlayerPickupItem([PlayerAgentSuggestorTag] PlayerAgent playerAgent, [TerminalItemSuggestorTag] iTerminalItem terminalItem)
    {
        if (!ItemsInLevel.TryGetValue(terminalItem.TerminalItemKey, out var itemInLevel))
        {
            ConsoleLogs.LogToConsole($"不存在物品 {terminalItem.TerminalItemKey}");
            return;
        }
        itemInLevel.GetSyncComponent().AttemptPickupInteraction(ePickupItemInteractionType.Pickup, playerAgent.Owner, default(pItemData_Custom), default(Vector3), default(Quaternion), null, false, true);
        ConsoleLogs.LogToConsole($"{playerAgent.GetColoredNameWithoutRichTextTags()} 已捡起 {terminalItem.TerminalItemKey}");
    }

    [Command("PickupItemEye")]
    private static void PickupItemInEyePos([PlayerAgentSuggestorTag] PlayerAgent playerAgent)
    {
        if (Physics.Raycast(AdminUtils.LocalPlayerAgent.FPSCamera.Position, AdminUtils.LocalPlayerAgent.FPSCamera.Forward, out RaycastHit raycastHit, 10f, LayerManager.MASK_APPLY_CARRY_ITEM))
        {
            var componentInParent = raycastHit.collider.GetComponentInParent<global::Item>();
            if (componentInParent != null)
            {
                componentInParent.Cast<ItemInLevel>().GetSyncComponent().AttemptPickupInteraction(0, playerAgent.Owner, default(pItemData_Custom), default(Vector3), default(Quaternion), null, false, false);
                ConsoleLogs.LogToConsole($"{playerAgent.GetColoredNameWithoutRichTextTags()} 捡起了 {componentInParent.PublicName}");
                return;
            }
        }
        ConsoleLogs.LogToConsole("目标物品为空", LogLevel.Error);
    }

    private enum SpawnItemMode
    {
        Pickup,
        Instance
    }

    [Command("SpawnItem")]
    private static void SpawnItem([ItemDataBlockSuggestorTag] ItemDataBlock block, SpawnItemMode mode = SpawnItemMode.Pickup)
    {
        InventorySlot slot = block.inventorySlot;
        float maxAmmo = block.ConsumableAmmoMax;
        if (slot == InventorySlot.ResourcePack)
            maxAmmo = 100f;
        var itemMode = mode switch
        {
            SpawnItemMode.Pickup => ItemMode.Pickup,
            SpawnItemMode.Instance => ItemMode.Instance,
            _ => ItemMode.Pickup
        };
        var localPlayer = AdminUtils.LocalPlayerAgent;
        pItemData data = new()
        {
            custom = new pItemData_Custom
            {
                ammo = maxAmmo,
                byteId = 0,
                byteState = 0
            },
            itemID_gearCRC = block.persistentID,
            slot = slot,
            originLayer = localPlayer.CourseNode.LayerType
        };
        data.originCourseNode.Set(localPlayer.CourseNode);
        ItemReplicationManager.SpawnItem(data, DelegateSupport.ConvertDelegate<ItemReplicationManager.delItemCallback>(new Action<ISyncedItem, PlayerAgent>((item, player) =>
        {
            var itemInLevel = item.TryCast<ItemInLevel>();
            if (itemInLevel == null)
                return;
            itemInLevel.CourseNode ??= localPlayer.CourseNode;
            itemInLevel.internalSync.AttemptPickupInteraction(ePickupItemInteractionType.UpdateCustomData, SNet.LocalPlayer, new()
            {
                ammo = maxAmmo,
            });
        })), itemMode, localPlayer.FPSCamera.CameraRayPos, localPlayer.Rotation, localPlayer.CourseNode, localPlayer);
    }

    [Command("GiveItem")]
    private static void GiveItem([ItemDataBlockSuggestorTag] ItemDataBlock block, [PlayerAgentSuggestorTag] PlayerAgent playerAgent = null)
    {
        if (block.PickupPrefabs.Count == 0)
        {
            ConsoleLogs.LogToConsole($"非法物品 {block.name}", LogLevel.Error);
            return;
        }
        if (playerAgent == null)
            playerAgent = AdminUtils.LocalPlayerAgent;
        InventorySlot itemSlot = block.inventorySlot;
        float maxAmmo = block.ConsumableAmmoMax;
        if (itemSlot == InventorySlot.ResourcePack)
            maxAmmo = 100f;
        var localPlayer = AdminUtils.LocalPlayerAgent;
        pItemData data = new()
        {
            custom = new pItemData_Custom
            {
                ammo = maxAmmo,
                byteId = 0,
                byteState = 0
            },
            itemID_gearCRC = block.persistentID,
            slot = itemSlot,
            originLayer = localPlayer.CourseNode.LayerType
        };
        data.originCourseNode.Set(playerAgent.CourseNode);
        ItemReplicationManager.SpawnItem(data, DelegateSupport.ConvertDelegate<ItemReplicationManager.delItemCallback>(new Action<ISyncedItem, PlayerAgent>((item, player) =>
        {
            var itemInLevel = item.TryCast<ItemInLevel>();
            if (itemInLevel == null)
                return;
            itemInLevel.CourseNode ??= playerAgent.CourseNode;
            itemInLevel.internalSync.AttemptPickupInteraction(ePickupItemInteractionType.UpdateCustomData, playerAgent.Owner, new()
            {
                ammo = maxAmmo,
            });
            itemInLevel.internalSync.AttemptPickupInteraction(ePickupItemInteractionType.Pickup, playerAgent.Owner);
        })), ItemMode.Pickup, playerAgent.Position, playerAgent.Rotation, playerAgent.CourseNode, localPlayer);
    }

    [Command("ListItemData")]
    private static void ListItemData()
    {
        var sb = new StringBuilder(1024);
        sb.AppendLine("----------------------------------------------------------------");
        foreach (var block in ItemDataBlock.GetAllBlocks())
        {
            sb.AppendLine($"{block.name} [{block.persistentID}]");
        }
        sb.AppendLine("----------------------------------------------------------------");
        ConsoleLogs.LogToConsole(sb.ToString());
    }

    private enum TripMineType
    {
        Explosive,
        Glue
    }

    [Command("SpawnMine")]
    private static void SpawnMine(TripMineType type)
    {
        pItemData data = new()
        {
            itemID_gearCRC = type switch
            {
                TripMineType.Explosive => 125U,
                TripMineType.Glue => 126U,
                _ => 125U
            }
        };
        ItemReplicationManager.SpawnItem(data, null, ItemMode.Instance, AdminUtils.LocalPlayerAgent.FPSCamera.CameraRayPos, Quaternion.LookRotation(AdminUtils.LocalPlayerAgent.FPSCamera.CameraRayNormal * -1f, AdminUtils.LocalPlayerAgent.Forward), AdminUtils.LocalPlayerAgent.CourseNode, AdminUtils.LocalPlayerAgent);
    }

    [Command("ListItemsInZone")]
    private static void ListItemsInZone([LG_ZoneSuggestorTag] LG_Zone zone)
    {
        if (CurrentGameState != (int)eGameStateName.InLevel)
        {
            ConsoleLogs.LogToConsole("不在游戏中", LogLevel.Error);
            return;
        }
        var resourcesInZone = new Dictionary<LG_Area, Dictionary<string, int>>();
        var consumableInZone = new Dictionary<LG_Area, Dictionary<string, int>>();
        foreach (LG_Area area in zone.m_areas)
        {
            if (!resourcesInZone.TryGetValue(area, out var resources))
            {
                resources = new();
                resourcesInZone.Add(area, resources);
            }
            if (!consumableInZone.TryGetValue(area, out var consumables))
            {
                consumables = new();
                consumableInZone.Add(area, consumables);
            }
            foreach (ItemInLevel item in area.m_courseNode.m_itemsInNode)
            {
                InventorySlot slot = item.pItemData.slot;
                if (slot < InventorySlot.ResourcePack || slot > InventorySlot.ConsumableHeavy)
                {
                    continue;
                }

                string itemName = item.ItemDataBlock.publicName;
                int count = (int)item.pItemData.custom.ammo;
                if (slot == InventorySlot.ResourcePack)
                {
                    count /= 20;

                    if (!resources.TryAdd(itemName, count))
                    {
                        resources[itemName] += count;
                    }
                }
                else
                {
                    if (!consumables.TryAdd(itemName, count))
                    {
                        consumables[itemName] += count;
                    }
                }
            }
        }

        var sb = new StringBuilder(500);

        if (resourcesInZone.Count == 0 && consumableInZone.Count == 0)
        {
            ConsoleLogs.LogToConsole($"ZONE_{zone.Alias}中没有资源", LogLevel.Error);
            return;
        }
        resourcesInZone = resourcesInZone.OrderBy(x => x.Key.m_navInfo.UID).ToDictionary(x => x.Key, x => x.Value.OrderBy(y => y.Key).ToDictionary(y => y.Key, y => y.Value));
        consumableInZone = consumableInZone.OrderBy(x => x.Key.m_navInfo.UID).ToDictionary(x => x.Key, x => x.Value.OrderBy(y => y.Key).ToDictionary(y => y.Key, y => y.Value));
        var totalResource = new Dictionary<string, int>();
        var totalConsumable = new Dictionary<string, int>();

        sb.AppendLine("-------------------------------------------------------------------------");
        sb.AppendLine($"                  象限 {zone.DimensionIndex}  ZONE_{zone.Alias} 资源统计");
        foreach (LG_Area area in resourcesInZone.Keys)
        {
            if (resourcesInZone[area].Count == 0 && consumableInZone[area].Count == 0)
            {
                continue;
            }
            sb.AppendLine("-------------------------------------------------------------------------");
            sb.AppendLine($"{area.m_navInfo.GetFormattedText(LG_NavInfoFormat.Full_And_Number_With_Underscore)}:");
            foreach (string itemName in resourcesInZone[area].Keys)
            {
                if (!totalResource.ContainsKey(itemName))
                {
                    totalResource.Add(itemName, resourcesInZone[area][itemName]);
                }
                else
                {
                    totalResource[itemName] += resourcesInZone[area][itemName];
                }
                sb.AppendLine($"           资源包: {itemName,-36}数量: {resourcesInZone[area][itemName]}次");
            }
            foreach (string itemName in consumableInZone[area].Keys)
            {
                if (!totalConsumable.ContainsKey(itemName))
                {
                    totalConsumable.Add(itemName, consumableInZone[area][itemName]);
                }
                else
                {
                    totalConsumable[itemName] += consumableInZone[area][itemName];
                }
                sb.AppendLine($"           可消耗品: {itemName,-35}数量: {consumableInZone[area][itemName]}次");
            }
        }

        sb.AppendLine("-------------------------------------------------------------------------");
        sb.AppendLine("总计:");
        if (totalResource.Count == 0 && totalConsumable.Count == 0)
        {
            sb.AppendLine("           没有资源");
        }
        else
        {
            totalResource = totalResource.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            totalConsumable = totalConsumable.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            foreach (string itemName in totalResource.Keys)
            {
                sb.AppendLine($"           资源包: {itemName,-36}数量:{totalResource[itemName]}次");
            }
            foreach (string itemName in totalConsumable.Keys)
            {
                sb.AppendLine($"           可消耗品: {itemName,-35}数量:{totalConsumable[itemName]}次");
            }
        }
        sb.AppendLine("-------------------------------------------------------------------------");
        ConsoleLogs.LogToConsole(sb.ToString());
    }

    [Command("ItemPing")]
    private static void PingItem([TerminalItemSuggestorTag] iTerminalItem terminalItem)
    {
        terminalItem.PlayPing();
    }

    [Command("ItemQuery")]
    private static void QueryItem([TerminalItemSuggestorTag] iTerminalItem terminalItem)
    {
        var itemDetails = new Il2CppSystem.Collections.Generic.List<string>();
        itemDetails.Add("ID: " + terminalItem.TerminalItemKey);
        itemDetails.Add("物品状态: " + terminalItem.FloorItemStatus);
        string locationText = $"{terminalItem.FloorItemLocation} Area_{terminalItem.SpawnNode.m_area.m_navInfo.Suffix}";
        itemDetails.Add($"位置: {locationText}");
        var sb = new StringBuilder(200);
        foreach (string detailInfo in terminalItem.GetDetailedInfo(itemDetails))
        {
            sb.AppendLine(detailInfo);
        }
        ConsoleLogs.LogToConsole(sb.ToString());
    }

    [Command("ItemList")]
    private static void ListItem(string param1, string param2 = "")
    {
        var sb = new StringBuilder(500);
        bool flag2 = param1 == string.Empty;
        bool flag3 = param1 != string.Empty;
        bool flag4 = param2 != string.Empty;
        if (flag2)
        {
            ConsoleLogs.LogToConsole("参数1不可为空", LogLevel.Error);
            return;
        }
        sb.AppendLine("-------------------------------------------------------------------------------------------");
        sb.AppendLine("ID                       物品类型             物品状态              位置");
        foreach (var keyValuePair in LG_LevelInteractionManager.Current.m_terminalItemsByKeyString)
        {
            if (keyValuePair.Value.ShowInFloorInventory)
            {
                var terminalItem = keyValuePair.Value;
                string locationInfo = $"象限 {terminalItem.SpawnNode.m_dimension.DimensionIndex} {terminalItem.FloorItemLocation} Area_{terminalItem.SpawnNode.m_area.m_navInfo.Suffix}";
                string text2 = string.Concat(new object[]
                {
                    terminalItem.TerminalItemKey,
                    " ",
                    terminalItem.FloorItemType,
                    " ",
                    terminalItem.FloorItemStatus,
                    " ",
                    terminalItem.FloorItemLocation,
                    " ",
                    eFloorInventoryObjectBeaconStatus.NoBeacon.ToString()
                });
                bool flag5 = flag3 && text2.Contains(param1, StringComparison.InvariantCultureIgnoreCase);
                bool flag6 = flag4 && text2.Contains(param2, StringComparison.InvariantCultureIgnoreCase);
                bool flag7 = !flag3 && !flag4;
                bool flag8 = (!flag3 || flag5) && (!flag4 || flag6);
                if (flag7 || flag8)
                {
                    sb.AppendLine(terminalItem.TerminalItemKey.PadRight(25) + terminalItem.FloorItemType.ToString().PadRight(20) + terminalItem.FloorItemStatus.ToString().PadRight(20) + locationInfo);
                }
            }
        }
        sb.AppendLine("-------------------------------------------------------------------------------------------");
        ConsoleLogs.LogToConsole(sb.ToString());
    }
}
