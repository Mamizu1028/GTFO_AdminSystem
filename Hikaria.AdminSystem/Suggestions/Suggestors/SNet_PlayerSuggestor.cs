using Hikaria.AdminSystem.Suggestions.Suggestors.Tags;
using Hikaria.QC;
using SNetwork;

namespace Hikaria.AdminSystem.Suggestions.Suggestors;

public sealed class SNet_PlayerSuggestor : BasicQcSuggestor<SNet_Player>
{
    protected override bool CanProvideSuggestions(SuggestionContext context, SuggestorOptions options)
    {
        return context.HasTag<SNet_PlayerSuggestorTag>();
    }

    protected override IQcSuggestion ItemToSuggestion(SNet_Player player)
    {
        return new SNet_PlayerSuggestion(player);
    }

    protected override IEnumerable<SNet_Player> GetItems(SuggestionContext context, SuggestorOptions options)
    {
        foreach (var player in SNet.SessionHub.PlayersInSession)
        {
            yield return player;
        }
    }
}