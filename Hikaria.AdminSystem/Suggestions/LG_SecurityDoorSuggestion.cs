using Hikaria.QC;
using LevelGeneration;

namespace Hikaria.AdminSystem.Suggestions;

public class LG_SecurityDoorSuggestion : IQcSuggestion
{
    private readonly string _completion;

    public LG_SecurityDoorSuggestion(LG_SecurityDoor door)
    {
        _completion = $"SEC_DOOR_{door.m_serialNumber}";
        PrimarySignature = $"SEC_DOOR_{door.m_serialNumber} {door.Gate.DimensionIndex} ZONE_{door.LinkedToZoneData.Alias}";
        FullSignature = PrimarySignature;
        SecondarySignature = $" 位于象限 {door.Gate.DimensionIndex} 通往 ZONE_{door.LinkedToZoneData.Alias}";
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
