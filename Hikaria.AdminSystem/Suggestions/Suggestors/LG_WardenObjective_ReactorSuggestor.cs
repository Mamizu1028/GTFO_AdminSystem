using Hikaria.AdminSystem.Suggestions.Suggestors.Tags;
using Hikaria.QC;
using LevelGeneration;

namespace Hikaria.AdminSystem.Suggestions.Suggestors;

public class LG_WardenObjective_ReactorSuggestor : BasicQcSuggestor<LG_WardenObjective_Reactor>
{
    protected override bool CanProvideSuggestions(SuggestionContext context, SuggestorOptions options)
    {
        return context.HasTag<LG_WardenObjective_ReactorSuggestorTag>();
    }

    protected override IQcSuggestion ItemToSuggestion(LG_WardenObjective_Reactor reactor)
    {
        return new LG_WardenObjective_ReactorSuggestion(reactor);
    }

    protected override IEnumerable<LG_WardenObjective_Reactor> GetItems(SuggestionContext context, SuggestorOptions options)
    {
        return UnityEngine.Object.FindObjectsOfType<LG_WardenObjective_Reactor>();
    }
}
