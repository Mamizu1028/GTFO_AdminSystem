using Hikaria.QC;
using LevelGeneration;

namespace Hikaria.AdminSystem.Suggestions;

public class LG_WardenObjective_ReactorSuggestion : IQcSuggestion
{
    private readonly string _completion;

    public string FullSignature { get; }
    public string PrimarySignature { get; }
    public string SecondarySignature { get; }

    public LG_WardenObjective_ReactorSuggestion(LG_WardenObjective_Reactor reactor)
    {
        FullSignature = $"REACTOR_{reactor.m_serialNumber}";
        PrimarySignature = $"REACTOR_{reactor.m_serialNumber}";
        SecondarySignature = $" {reactor.SpawnNode.m_dimension.CreationOrderIndex} ZONE_{reactor.SpawnNode.m_zone.Alias} {reactor.m_currentState.status}";
        _completion = $"REACTOR_{reactor.m_serialNumber}";
    }

    public bool MatchesPrompt(string prompt)
    {
        return prompt == _completion;
    }

    public string GetCompletion(string prompt)
    {
        return _completion;
    }

    public string GetCompletionTail(string prompt)
    {
        return string.Empty;
    }

    public SuggestionContext? GetInnerSuggestionContext(SuggestionContext context)
    {
        return null;
    }
}
