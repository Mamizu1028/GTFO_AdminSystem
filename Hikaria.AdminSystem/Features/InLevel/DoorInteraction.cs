using Hikaria.AdminSystem.Suggestions;
using Hikaria.AdminSystem.Utilities;
using Hikaria.AdminSystem.Utility;
using Hikaria.QC;
using LevelGeneration;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;

namespace Hikaria.AdminSystem.Features.InLevel;

[HideInModSettings]
[EnableFeatureByDefault]
[DisallowInGameToggle]
public class DoorInteraction : Feature
{
    public override string Name => "操作门";

    public override GroupBase Group => ModuleGroup.GetOrCreateSubGroup("InLevel");

    private static Dictionary<int, LG_SecurityDoor> SecurityDoorsInLevel = new();

    private static Dictionary<int, LG_WeakDoor> WeakDoorsInLevel = new();

    [ArchivePatch(typeof(LG_WeakDoor), nameof(LG_WeakDoor.Setup))]
    private class LG_WeakDoor__Setup__Patch
    {
        private static void Postfix(LG_WeakDoor __instance)
        {
            if (!WeakDoorsInLevel.TryAdd(__instance.m_serialNumber, __instance))
            {
                WeakDoorsInLevel[__instance.m_serialNumber] = __instance;
            }
        }
    }

    [ArchivePatch(typeof(LG_WeakDoor), nameof(LG_WeakDoor.OnDestroy))]
    private class LG_WeakDoor__OnDestroy__Patch
    {
        private static void Prefix(LG_WeakDoor __instance)
        {
            WeakDoorsInLevel.Remove(__instance.m_serialNumber);
        }
    }

    [ArchivePatch(typeof(LG_SecurityDoor), nameof(LG_SecurityDoor.Setup))]
    private class LG_SecurityDoor__Setup__Patch
    {
        private static void Postfix(LG_SecurityDoor __instance)
        {
            if (!SecurityDoorsInLevel.TryAdd(__instance.m_serialNumber, __instance))
            {
                SecurityDoorsInLevel[__instance.m_serialNumber] = __instance;
            }
        }
    }

    [ArchivePatch(typeof(LG_SecurityDoor), nameof(LG_SecurityDoor.OnDestroy))]
    private class LG_SecurityDoor__OnDestroy__Patch
    {
        private static void Prefix(LG_SecurityDoor __instance)
        {
            SecurityDoorsInLevel.Remove(__instance.m_serialNumber);
        }
    }

    [Command("InteractAllWeakDoors")]
    private static void OperateAllWeakDoors(eDoorInteractionType interactionType)
    {
        if (CurrentGameState != (int)eGameStateName.InLevel)
        {
            ConsoleLogs.LogToConsole("不在游戏中", LogLevel.Error);
            return;
        }
        foreach (var door in WeakDoorsInLevel.Values)
        {
            door.m_sync.AttemptDoorInteraction(interactionType, float.MaxValue, float.MaxValue, AdminUtils.LocalPlayerAgent.Position, AdminUtils.LocalPlayerAgent);
        }
        ConsoleLogs.LogToConsole($"所有普通门 {interactionType}");
    }

    [Command("InteractAllSecurityDoors")]
    private static void OperateAllSecurityDoors(eDoorInteractionType interactionType)
    {
        if (CurrentGameState != (int)eGameStateName.InLevel)
        {
            ConsoleLogs.LogToConsole("不在游戏中", LogLevel.Error);
            return;
        }
        foreach (var door in SecurityDoorsInLevel.Values)
        {
            door.m_sync.AttemptDoorInteraction(interactionType, float.MaxValue, float.MaxValue, AdminUtils.LocalPlayerAgent.Position, AdminUtils.LocalPlayerAgent);
        }
        ConsoleLogs.LogToConsole($"所有安全门 {interactionType}");
    }

    [Command("InteractWeakDoor")]
    private static void WeakDoorInteraction([LG_WeakDoorSuggestor] LG_WeakDoor door, eDoorInteractionType interactionType = eDoorInteractionType.Open)
    {
        if (CurrentGameState != (int)eGameStateName.InLevel)
        {
            ConsoleLogs.LogToConsole("不在游戏中", LogLevel.Error);
            return;
        }
        door.m_sync.AttemptDoorInteraction(interactionType, float.MaxValue, float.MaxValue, AdminUtils.LocalPlayerAgent.Position, AdminUtils.LocalPlayerAgent);
        ConsoleLogs.LogToConsole($"WeakDoor_{door.m_serialNumber} {interactionType}");
    }

    [Command("InteractSecurityDoor")]
    private static void SecurityDoorInteraction([LG_SecurityDoorSuggestor] LG_SecurityDoor door, eDoorInteractionType interactionType = eDoorInteractionType.Open)
    {
        if (CurrentGameState != (int)eGameStateName.InLevel)
        {
            ConsoleLogs.LogToConsole("不在游戏中", LogLevel.Error);
            return;
        }
        if (interactionType == eDoorInteractionType.Open && door.LastStatus != eDoorStatus.Open)
        {
            door.ForceOpenSecurityDoor();
            ConsoleLogs.LogToConsole($"<color=green>位于象限</color><color=orange>{door.m_terminalItem.SpawnNode.m_dimension.DimensionIndex}</color><color=green>通往</color><color=orange>ZONE_{door.LinkedToZoneData.Alias}</color><color=green>的安全门已开启</color>");
        }
        else
        {
            door.m_sync.AttemptDoorInteraction(interactionType, 0f, 0f, AdminUtils.LocalPlayerAgent.Position, AdminUtils.LocalPlayerAgent);
            ConsoleLogs.LogToConsole($"<color=red>位于象限</color><color=orange>{door.m_terminalItem.SpawnNode.m_dimension.DimensionIndex}</color><color=red>通往</color><color=orange>ZONE_{door.LinkedToZoneData.Alias}</color><color=red>的安全门已关闭</color>");
        }
    }

    public struct LG_WeakDoorTag : IQcSuggestorTag
    {
    }

    public sealed class LG_WeakDoorSuggestorAttribute : SuggestorTagAttribute
    {
        private readonly IQcSuggestorTag[] _tags = { new LG_WeakDoorTag() };

        public override IQcSuggestorTag[] GetSuggestorTags()
        {
            return _tags;
        }
    }

    public class LG_WeakDoorSuggestor : BasicQcSuggestor<LG_WeakDoor>
    {
        protected override bool CanProvideSuggestions(SuggestionContext context, SuggestorOptions options)
        {
            return context.HasTag<LG_WeakDoorTag>();
        }

        protected override IQcSuggestion ItemToSuggestion(LG_WeakDoor door)
        {
            return new LG_WeakDoorSuggestion(door);
        }

        protected override IEnumerable<LG_WeakDoor> GetItems(SuggestionContext context, SuggestorOptions options)
        {
            return WeakDoorsInLevel.Values;
        }
    }

    public struct LG_SecurityDoorTag : IQcSuggestorTag
    {
    }

    public sealed class LG_SecurityDoorSuggestorAttribute : SuggestorTagAttribute
    {
        private readonly IQcSuggestorTag[] _tags = { new LG_SecurityDoorTag() };

        public override IQcSuggestorTag[] GetSuggestorTags()
        {
            return _tags;
        }
    }

    public class LG_SecurityDoorSuggestor : BasicQcSuggestor<LG_SecurityDoor>
    {
        protected override bool CanProvideSuggestions(SuggestionContext context, SuggestorOptions options)
        {
            return context.HasTag<LG_SecurityDoorTag>();
        }

        protected override IQcSuggestion ItemToSuggestion(LG_SecurityDoor door)
        {
            return new LG_SecurityDoorSuggestion(door);
        }

        protected override IEnumerable<LG_SecurityDoor> GetItems(SuggestionContext context, SuggestorOptions options)
        {
            return SecurityDoorsInLevel.Values;
        }
    }

    public sealed class LG_SecurityDoorParser : BasicQcParser<LG_SecurityDoor>
    {
        public override LG_SecurityDoor Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("参数不能为空。格式应为: SEC_DOOR_<SerialNumber>");
            }

            if (!value.StartsWith("SEC_DOOR_", StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException(
                    $"无效的参数格式: \"{value}\"。正确格式应为: SEC_DOOR_<SerialNumber>，例如 SEC_DOOR_123");
            }
            var serialText = value.Skip(9).ToArray();
            if (!int.TryParse(serialText, out var serialNumber))
            {
                throw new FormatException($"无法解析 SerialNumber: \"{serialText}\"。SerialNumber 必须是整数。");
            }

            if (!SecurityDoorsInLevel.TryGetValue(serialNumber, out var door))
            {
                throw new ArgumentException(
                    $"找不到 SecurityDoor: SerialNumber={serialNumber}");
            }

            return door;
        }
    }

    public sealed class LG_WeakDoorParser : BasicQcParser<LG_WeakDoor>
    {
        public override LG_WeakDoor Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("参数不能为空。格式应为: DOOR_<SerialNumber>");
            }

            if (!value.StartsWith("DOOR_", StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException(
                    $"无效的参数格式: \"{value}\"。正确格式应为: DOOR_<SerialNumber>，例如 DOOR_123");
            }
            var serialText = value.Skip(5).ToArray();
            if (!int.TryParse(serialText, out var serialNumber))
            {
                throw new FormatException($"无法解析 SerialNumber: \"{serialText}\"。SerialNumber 必须是整数。");
            }
            if (!WeakDoorsInLevel.TryGetValue(serialNumber, out var door))
            {
                throw new ArgumentException(
                    $"找不到 WeakDoor: SerialNumber={serialNumber}");
            }

            return door;
        }
    }
}
