using Hikaria.ItemMarker.Managers;
using Hikaria.QC;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Members;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;

namespace Hikaria.AdminSystem.Features.Item
{
    [EnableFeatureByDefault]
    [DisallowInGameToggle]
    [DoNotSaveToConfig]
    public class SuperItemMarker : Feature
    {
        public override string Name => "物品标记";

        public override GroupBase Group => ModuleGroup.GetOrCreateSubGroup("Item");

        [FeatureConfig]
        public static SuperItemMarkerSettings Settings { get; set; }

        public class SuperItemMarkerSettings
        {
            [FSDisplayName("状态")]
            public bool EnableItemMarker { get => ItemMarkerManager.DevMode; set => ItemMarkerManager.DevMode = value; }
        }

        [Command("ItemMarker")]
        private static bool EnableItemMarker { get => ItemMarkerManager.DevMode; set => ItemMarkerManager.DevMode = value; }
    }
}