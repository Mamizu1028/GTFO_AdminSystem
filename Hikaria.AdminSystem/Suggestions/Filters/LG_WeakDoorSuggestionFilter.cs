using Hikaria.QC;
using LevelGeneration;

namespace Hikaria.AdminSystem.Suggestions.Filters;

public struct LG_WeakDoorSuggestionFilter : IQcSuggestionFilter
{
    public bool IsSuggestionPermitted(IQcSuggestion suggestion, SuggestionContext context)
    {
        if (context.TargetType != typeof(LG_WeakDoor))
            return true;

        return suggestion is LG_WeakDoorSuggestion;
    }
}