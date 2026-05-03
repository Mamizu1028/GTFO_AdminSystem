using Hikaria.AdminSystem.Suggestions.Suggestors.Tags;
using Hikaria.QC;

namespace Hikaria.AdminSystem.Suggestions.Suggestors.Attributes;

public sealed class LG_ZoneSuggestorTagAttribute : SuggestorTagAttribute
{
    private readonly IQcSuggestorTag[] _tags = { new LG_ZoneSuggestorTag() };

    public override IQcSuggestorTag[] GetSuggestorTags()
    {
        return _tags;
    }
}