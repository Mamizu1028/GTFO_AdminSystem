using Hikaria.QC;
using LevelGeneration;

namespace Hikaria.AdminSystem.Suggestions;

public class LG_WeakDoorSuggestion : IQcSuggestion
{
    private readonly string _completion;

    public LG_WeakDoorSuggestion(LG_WeakDoor door)
    {
        _completion = $"DOOR_{door.m_serialNumber}";

        PrimarySignature = $"DOOR_{door.m_serialNumber} {door.Gate.DimensionIndex} ZONE_{door.Gate.m_linksFrom.m_zone.Alias}";
        FullSignature = _completion;
        SecondarySignature = $" Area_{door.Gate.m_linksFrom.m_navInfo.Suffix} Area_{door.Gate.m_linksTo.m_navInfo.Suffix}";
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
