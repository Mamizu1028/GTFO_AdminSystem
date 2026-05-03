using Hikaria.QC;
using LevelGeneration;

namespace Hikaria.AdminSystem.Suggestions;

public class ItemInLevelSuggestion : IQcSuggestion
{
    private readonly string _completion;

    public ItemInLevelSuggestion(ItemInLevel itemInLevel)
    {
        _completion = string.Empty;
        PrimarySignature = string.Empty;
        FullSignature = PrimarySignature;
        SecondarySignature = string.Empty;
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
