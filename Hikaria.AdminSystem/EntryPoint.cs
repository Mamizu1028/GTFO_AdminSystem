using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Localization;
using TheArchive.Interfaces;

namespace Hikaria.AdminSystem;

[ArchiveDependency(ItemMarker.PluginInfo.GUID)]
[ArchiveDependency(QC.QuantumGlobal.GUID)]
[ArchiveDependency(QC.QuantumGlobal.GUID)]
[ArchiveModule(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
public class EntryPoint : IArchiveModule
{
    public ILocalizationService LocalizationService { get; set; }
    public IArchiveLogger Logger { get; set; }

    public void Init()
    {
        Logs.Setup(Logger);
    }

    public static class Groups
    {
        public static FeatureGroup ModuleGroup => FeatureGroups.GetOrCreateModuleGroup($"{PluginInfo.GUID}.ModuleGroup");

        public static FeatureGroup Item => ModuleGroup.GetOrCreateSubGroup("Item");

        public static FeatureGroup Weapon => ModuleGroup.GetOrCreateSubGroup("Weapon");

        public static FeatureGroup Player => ModuleGroup.GetOrCreateSubGroup("Player");

        public static FeatureGroup Enemy => ModuleGroup.GetOrCreateSubGroup("Enemy");

        public static FeatureGroup Misc => ModuleGroup.GetOrCreateSubGroup("Misc");

        public static FeatureGroup Security => ModuleGroup.GetOrCreateSubGroup("Security");

        public static FeatureGroup InLevel => ModuleGroup.GetOrCreateSubGroup("InLevel");

        public static FeatureGroup Dev => ModuleGroup.GetOrCreateSubGroup("Develop", true);

        public static FeatureGroup Visual => ModuleGroup.GetOrCreateSubGroup("Visual");
    }
}
