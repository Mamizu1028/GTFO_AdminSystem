using Hikaria.QC;
using Player;
using SNetwork;

namespace Hikaria.AdminSystem.Suggestions.Filters;

public struct SNet_PlayerSuggestionFilter : IQcSuggestionFilter
{
    public bool IsSuggestionPermitted(IQcSuggestion suggestion, SuggestionContext context)
    {
        if (context.TargetType != typeof(SNet_Player))
            return true;

        return suggestion is SNet_PlayerSuggestion;
    }
}
