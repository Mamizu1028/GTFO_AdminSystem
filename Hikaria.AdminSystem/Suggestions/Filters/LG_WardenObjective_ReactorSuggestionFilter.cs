using Hikaria.QC;
using LevelGeneration;

namespace Hikaria.AdminSystem.Suggestions.Filters;

public struct LG_WardenObjective_ReactorSuggestionFilter : IQcSuggestionFilter
{
    public bool IsSuggestionPermitted(IQcSuggestion suggestion, SuggestionContext context)
    {
        if (context.TargetType != typeof(LG_WardenObjective_Reactor))
            return true;

        return suggestion is LG_WardenObjective_ReactorSuggestion;
    }
}
