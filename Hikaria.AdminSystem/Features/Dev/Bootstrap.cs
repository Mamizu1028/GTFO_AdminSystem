using Hikaria.AdminSystem.Extensions;
using Hikaria.AdminSystem.Utilities;
using Hikaria.AdminSystem.Utility;
using Player;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using TheArchive.Loader;
using UnityEngine;

namespace Hikaria.AdminSystem.Features.Develop;

[HideInModSettings]
[EnableFeatureByDefault]
[DisallowInGameToggle]
[DoNotSaveToConfig]
internal class Bootstrap : Feature
{
    public override string Name => "Bootstrap";

    public override GroupBase Group => ModuleGroup.GetOrCreateSubGroup("Develop", true);

    public override void OnGameDataInitialized()
    {
        LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<UnityMainThreadDispatcher>();
        GameObject obj = new("Hikaria.AdminSystem.ScriptsHolder");
        obj.DoNotDestroyAndSetHideFlags();
        obj.AddComponent<UnityMainThreadDispatcher>();
    }

    [ArchivePatch(typeof(LocalPlayerAgent), nameof(LocalPlayerAgent.Setup))]
    private static class LocalPlayerAgent__Setup__Patch
    {
        public static void Prefix(LocalPlayerAgent __instance)
        {
            AdminUtils.LocalPlayerAgent = __instance;
        }
    }
}
