using Hikaria.AdminSystem.Suggestions.Suggestors.Tags;
using Hikaria.QC;

namespace Hikaria.AdminSystem.Suggestions.Suggestors.Attributes;

public class ItemInLevelSuggestorTagAttribute : SuggestorTagAttribute
{
    private readonly IQcSuggestorTag[] _tags = { new ItemInLevelSuggestorTag() };

    public override IQcSuggestorTag[] GetSuggestorTags()
    {
        return _tags;
    }
}
