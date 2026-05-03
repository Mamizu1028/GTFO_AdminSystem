using GameData;
using Hikaria.QC;

namespace Hikaria.AdminSystem.Suggestions;

public class GameDataBlockSuggestion<TBlock> : IQcSuggestion where TBlock : GameDataBlockBase<TBlock>
{
    private readonly string _completion;

    public string FullSignature { get; }
    public virtual string PrimarySignature { get; }
    public virtual string SecondarySignature => string.Empty;

    public GameDataBlockSuggestion(TBlock block)
    {
        _completion = $"{block.name}.{block.persistentID}";
        FullSignature = _completion;
        PrimarySignature = $"{block.name} [{block.persistentID}]";
    }

    public virtual bool MatchesPrompt(string prompt)
    {
        return prompt == _completion;
    }

    public virtual string GetCompletion(string prompt)
    {
        return _completion;
    }

    public virtual string GetCompletionTail(string prompt)
    {
        return string.Empty;
    }

    public SuggestionContext? GetInnerSuggestionContext(SuggestionContext context)
    {
        return null;
    }
}
