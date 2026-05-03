using Hikaria.AdminSystem.Suggestions.Suggestors.Tags;
using Hikaria.QC;
using LevelGeneration;

namespace Hikaria.AdminSystem.Suggestions.Suggestors;

public sealed class LG_ZoneSuggestor : BasicQcSuggestor<LG_Zone>
{
    protected override bool CanProvideSuggestions(SuggestionContext context, SuggestorOptions options)
    {
        return context.HasTag<LG_ZoneSuggestorTag>();
    }

    protected override IQcSuggestion ItemToSuggestion(LG_Zone zone)
    {
        return new LG_ZoneSuggestion(zone);
    }

    protected override IEnumerable<LG_Zone> GetItems(SuggestionContext context, SuggestorOptions options)
    {
        return Builder.CurrentFloor?.allZones.ToArray() ?? Array.Empty<LG_Zone>();
    }
}
