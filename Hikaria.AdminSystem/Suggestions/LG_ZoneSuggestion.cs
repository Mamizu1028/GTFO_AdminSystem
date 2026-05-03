using Hikaria.QC;
using LevelGeneration;

namespace Hikaria.AdminSystem.Suggestions;

public sealed class LG_ZoneSuggestion : IQcSuggestion
{
    private readonly string _completion;

    public LG_ZoneSuggestion(LG_Zone zone)
    {
        _completion = $"{zone.DimensionIndex}.{zone.Layer.m_type}.{zone.Alias}";

        PrimarySignature = _completion;
        FullSignature = PrimarySignature;
        SecondarySignature = $" {zone.DimensionIndex} ZONE_{zone.Alias}";
    }

    public string FullSignature { get; }

    public string PrimarySignature { get; }

    public string SecondarySignature { get; }

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

    public bool MatchesPrompt(string prompt)
    {
        return prompt == _completion;
    }
}
