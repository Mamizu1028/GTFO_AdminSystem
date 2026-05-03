using Hikaria.AdminSystem.Features.Item;
using Hikaria.AdminSystem.Suggestions.Suggestors.Tags;
using Hikaria.QC;
using LevelGeneration;

namespace Hikaria.AdminSystem.Suggestions.Suggestors;

public sealed class ItemInLevelSuggestor : BasicQcSuggestor<ItemInLevel>
{
    protected override bool CanProvideSuggestions(SuggestionContext context, SuggestorOptions options)
    {
        return context.HasTag<ItemInLevelSuggestorTag>();
    }

    protected override IQcSuggestion ItemToSuggestion(ItemInLevel itemInLevel)
    {
        return new ItemInLevelSuggestion(itemInLevel);
    }

    protected override IEnumerable<ItemInLevel> GetItems(SuggestionContext context, SuggestorOptions options)
    {
        return ItemLookup.ItemsInLevel.Values;
    }
}