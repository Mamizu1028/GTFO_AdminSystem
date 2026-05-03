using Hikaria.AdminSystem.Suggestions.Suggestors.Tags;
using Hikaria.QC;
using Player;
using TheArchive.Utilities;

namespace Hikaria.AdminSystem.Suggestions.Suggestors;

public sealed class PlayerAgentSuggestor : BasicQcSuggestor<PlayerAgent>
{
    protected override bool CanProvideSuggestions(SuggestionContext context, SuggestorOptions options)
    {
        return context.HasTag<PlayerAgentSuggestorTag>();
    }

    protected override IQcSuggestion ItemToSuggestion(PlayerAgent agent)
    {
        return new PlayerAgentSuggestion(agent);
    }

    protected override IEnumerable<PlayerAgent> GetItems(SuggestionContext context, SuggestorOptions options)
    {
        return PlayerManager.PlayerAgentsInLevel.ToSystemList();
    }
}