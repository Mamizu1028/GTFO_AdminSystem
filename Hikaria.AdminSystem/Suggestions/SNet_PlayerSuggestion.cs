using Hikaria.AdminSystem.Extensions;
using Hikaria.QC;
using SNetwork;

namespace Hikaria.AdminSystem.Suggestions;

public sealed class SNet_PlayerSuggestion : IQcSuggestion
{
    private readonly string _completion;
    private readonly string _completionTail;

    public SNet_PlayerSuggestion(SNet_Player player)
    {
        _completion = $"{player.Lookup}";
        _completionTail = $" {player.GetColoredNameWithoutRichTextTags()}";
        FullSignature = $"{player.Lookup}";
        PrimarySignature = $"{player.CharacterIndex} {player.GetColoredNameWithoutRichTextTags()} {player.Lookup}";
    }

    public string FullSignature { get; }

    public string PrimarySignature { get; }

    public string SecondarySignature => string.Empty;

    public string GetCompletion(string prompt)
    {
        return _completion;
    }

    public string GetCompletionTail(string prompt)
    {
        return _completionTail;
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