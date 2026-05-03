using Hikaria.AdminSystem.Suggestions.Suggestors.Tags;
using Hikaria.QC;
using LevelGeneration;

namespace Hikaria.AdminSystem.Suggestions.Suggestors;

public class TerminalItemSuggestor : BasicQcSuggestor<iTerminalItem>
{
    protected override bool CanProvideSuggestions(SuggestionContext context, SuggestorOptions options)
    {
        return context.HasTag<TerminalItemSuggestorTag>();
    }

    protected override IQcSuggestion ItemToSuggestion(iTerminalItem item)
    {
        return new TerminalItemSuggestion(item);
    }

    protected override IEnumerable<iTerminalItem> GetItems(SuggestionContext context, SuggestorOptions options)
    {
        foreach (var item in LG_LevelInteractionManager.Current.m_terminalItems.Values)
        {
            yield return item;
        }
    }
}
