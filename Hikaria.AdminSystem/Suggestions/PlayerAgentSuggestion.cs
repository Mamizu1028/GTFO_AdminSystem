using Hikaria.AdminSystem.Extensions;
using Hikaria.QC;
using Player;

namespace Hikaria.AdminSystem.Suggestions;

public sealed class PlayerAgentSuggestion : IQcSuggestion
{
    private readonly string _completion;
    private readonly string _completionTail;

    public PlayerAgentSuggestion(PlayerAgent agent)
    {
        _completion = $"{agent.PlayerSlotIndex}";
        _completionTail = $" {agent.GetColoredNameWithoutRichTextTags()}";
        FullSignature = $"{agent.PlayerSlotIndex}";
        PrimarySignature = $"{agent.PlayerSlotIndex} {agent.GetColoredNameWithoutRichTextTags()}";
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
