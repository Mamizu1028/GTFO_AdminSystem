using Hikaria.QC;
using LevelGeneration;

namespace Hikaria.AdminSystem.Suggestions.Filters;

public struct LG_SecurityDoorSuggestionFilter : IQcSuggestionFilter
{
    public bool IsSuggestionPermitted(IQcSuggestion suggestion, SuggestionContext context)
    {
        if (context.TargetType != typeof(LG_SecurityDoor))
            return true;

        return suggestion is LG_SecurityDoorSuggestion;
    }
}
