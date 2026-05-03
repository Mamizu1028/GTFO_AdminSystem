using Hikaria.QC;
using Player;

namespace Hikaria.AdminSystem.Suggestions.Filters;

public struct PlayerAgentSuggestionFilter : IQcSuggestionFilter
{
    public bool IsSuggestionPermitted(IQcSuggestion suggestion, SuggestionContext context)
    {
        if (context.TargetType != typeof(PlayerAgent))
            return true;

        return suggestion is PlayerAgentSuggestion;
    }
}
