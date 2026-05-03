using GameData;
using Hikaria.AdminSystem.Suggestions.Suggestors.Tags;
using Hikaria.QC;

namespace Hikaria.AdminSystem.Suggestions.Suggestors;

public abstract class GameDataBlockSuggestorBase<TBlock> : BasicCachedQcSuggestor<TBlock> where TBlock : GameDataBlockBase<TBlock>
{
    protected override bool CanProvideSuggestions(SuggestionContext context, SuggestorOptions options)
    {
        return context.HasTag<GameDataBlockSuggestorTag<TBlock>>();
    }

    protected override IQcSuggestion ItemToSuggestion(TBlock block)
    {
        return new GameDataBlockSuggestion<TBlock>(block);
    }

    protected override IEnumerable<TBlock> GetItems(SuggestionContext context, SuggestorOptions options)
    {
        return GameDataBlockBase<TBlock>.GetAllBlocks();
    }
}
