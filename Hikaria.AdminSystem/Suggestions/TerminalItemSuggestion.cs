using Hikaria.QC;
using LevelGeneration;

namespace Hikaria.AdminSystem.Suggestions;

public class TerminalItemSuggestion : IQcSuggestion
{
    private readonly string _completion;

    public TerminalItemSuggestion(iTerminalItem terminalItem)
    {
        _completion = $"{terminalItem.TerminalItemKey}.{terminalItem.SpawnNode.m_dimension.DimensionIndex}";
        PrimarySignature = $"{terminalItem.TerminalItemKey}";
        FullSignature = $"{terminalItem.TerminalItemKey}";
        SecondarySignature = $" {terminalItem.SpawnNode.m_dimension.DimensionIndex} ZONE_{terminalItem.SpawnNode.m_area.m_zone.Alias} Area_{terminalItem.SpawnNode.m_area.m_navInfo.Suffix}";
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
