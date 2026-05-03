using Hikaria.AdminSystem.Features.Item;
using Hikaria.AdminSystem.Suggestions.Suggestors.Attributes;
using Hikaria.AdminSystem.Utilities;
using Hikaria.AdminSystem.Utility;
using Hikaria.QC;
using LevelGeneration;
using Player;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using UnityEngine;

namespace Hikaria.AdminSystem.Features.Player;

[HideInModSettings]
[EnableFeatureByDefault]
[DisallowInGameToggle]
public class WarpPlayer : Feature
{
    public override string Name => "传送玩家";

    public override string Description => "传送玩家";

    public override GroupBase Group => ModuleGroup.GetOrCreateSubGroup("Player");

    private static Dictionary<string, Tuple<eDimensionIndex, Vector3, Vector3>> WarpStoresLookup = new();

    [Command("WarpToPlayer")]
    private static void WarpPlayerToPlayer([PlayerAgentSuggestorTag] PlayerAgent playerAgent, [PlayerAgentSuggestorTag] PlayerAgent targetPlayerAgent)
    {
        playerAgent.RequestWarpToSync(targetPlayerAgent.DimensionIndex, targetPlayerAgent.Position, targetPlayerAgent.Forward, PlayerAgent.WarpOptions.ShowScreenEffectForLocal);
    }

    [Command("WarpAllToPlayer")]
    private static void WarpAllPlayersToPlayer([PlayerAgentSuggestorTag] PlayerAgent targetPlayerAgent)
    {
        foreach (var player in PlayerManager.PlayerAgentsInLevel)
        {
            if (player.GlobalID != targetPlayerAgent.GlobalID)
                WarpPlayerToPlayer(player, targetPlayerAgent);
        }
    }

    [Command("WarpStorePos")]
    private static void StoreWarpPos(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;
        key = key.ToUpperInvariant();
        var localPlayer = AdminUtils.LocalPlayerAgent;
        WarpStoresLookup[key] = new Tuple<eDimensionIndex, Vector3, Vector3>(localPlayer.DimensionIndex, localPlayer.Position, localPlayer.Forward);
    }

    [Command("WarpToStore")]
    private static void WarpPlayerToStoredPos([PlayerAgentSuggestorTag] PlayerAgent playerAgent, [WarpStoredPosition] string key)
    {
        if (WarpStoresLookup.TryGetValue(key.ToUpperInvariant(), out var pair))
        {
            playerAgent.RequestWarpToSync(pair.Item1, pair.Item2, pair.Item3, PlayerAgent.WarpOptions.ShowScreenEffectForLocal);
        }
    }

    [Command("WarpAllToStore")]
    private static void WarpAllBackToStoredPos([WarpStoredPosition] string key)
    {
        foreach (var player in PlayerManager.PlayerAgentsInLevel)
        {
            WarpPlayerToStoredPos(player, key);
        }
    }

    [Command("TeleportToEye")]
    private static void TeleportToEyePos()
    {
        AdminUtils.LocalPlayerAgent.TeleportTo(AdminUtils.LocalPlayerAgent.FPSCamera.CameraRayPos);
    }

    [Command("TeleportToPlayer")]
    private static void TeleportToPlayer([PlayerAgentSuggestorTag] PlayerAgent playerAgent)
    {
        AdminUtils.LocalPlayerAgent.TeleportTo(playerAgent.Position);
    }

    [Command("WarpToEye")]
    private static void WarpPlayerToEyePos([PlayerAgentSuggestorTag] PlayerAgent playerAgent)
    {
        playerAgent.RequestWarpToSync(AdminUtils.LocalPlayerAgent.DimensionIndex, AdminUtils.LocalPlayerAgent.FPSCamera.CameraRayPos, playerAgent.Forward, PlayerAgent.WarpOptions.All);
    }

    [Command("WarpToDimension")]
    private static void WarpPlayerToDimension([PlayerAgentSuggestorTag] PlayerAgent playerAgent, eDimensionIndex dimensionIndex)
    {
        if (!Dimension.GetDimension(dimensionIndex, out var dimension))
            return;

        playerAgent.RequestWarpToSync(dimensionIndex, dimension.GetStartCourseNode().Position, playerAgent.Forward, PlayerAgent.WarpOptions.ShowScreenEffectForLocal);
    }

    [Command("WarpAllToDimension")]
    private static void WarpAllPlayerToDimension(eDimensionIndex dimensionIndex)
    {
        foreach (var player in PlayerManager.PlayerAgentsInLevel)
        {
            WarpPlayerToDimension(player, dimensionIndex);
        }
    }

    [Command("WarpToItem")]
    private static void WarpPlayerToItem([PlayerAgentSuggestorTag] PlayerAgent playerAgent, [TerminalItemSuggestorTag] iTerminalItem terminalItem)
    {
        if (terminalItem.SpawnNode == null)
        {
            ConsoleLogs.LogToConsole($"物品处于未知位置, 传送失败");
            return;
        }

        playerAgent.RequestWarpToSync(terminalItem.SpawnNode.m_dimension.DimensionIndex, terminalItem.LocatorBeaconPosition, Vector3.down, PlayerAgent.WarpOptions.ShowScreenEffectForLocal);
    }

    [Command("WarpAllToItem")]
    private static void WarpAllPlayersToItem([TerminalItemSuggestorTag] iTerminalItem terminalItem)
    {
        if (terminalItem.SpawnNode == null)
        {
            ConsoleLogs.LogToConsole($"物品处于未知位置, 传送失败");
            return;
        }
        var item = terminalItem.Cast<LG_GenericTerminalItem>();
        foreach (var player in PlayerManager.PlayerAgentsInLevel)
        {
            player.RequestWarpToSync(item.SpawnNode.m_dimension.DimensionIndex, item.transform.position, Vector3.down, PlayerAgent.WarpOptions.ShowScreenEffectForLocal); }
    }

    public override void OnGameStateChanged(int state)
    {
        if (state == (int)eGameStateName.AfterLevel)
        {
            WarpStoresLookup.Clear();
        }
    }


    public struct WarpStoredPositionTag : IQcSuggestorTag
    {
    }

    public sealed class WarpStoredPositionAttribute : SuggestorTagAttribute
    {
        private readonly IQcSuggestorTag[] _tags = { new WarpStoredPositionTag() };

        public override IQcSuggestorTag[] GetSuggestorTags()
        {
            return _tags;
        }
    }

    public class WarpStoredPositionSuggestor : BasicQcSuggestor<string>
    {
        protected override bool CanProvideSuggestions(SuggestionContext context, SuggestorOptions options)
        {
            return context.HasTag<WarpStoredPositionTag>();
        }

        protected override IQcSuggestion ItemToSuggestion(string item)
        {
            return new RawSuggestion(item.ToUpperInvariant());
        }

        protected override IEnumerable<string> GetItems(SuggestionContext context, SuggestorOptions options)
        {
            return WarpStoresLookup.Keys;
        }
    }
}
