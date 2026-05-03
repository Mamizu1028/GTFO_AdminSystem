using Hikaria.AdminSystem.Suggestions.Suggestors.Tags;
using Hikaria.QC;

namespace Hikaria.AdminSystem.Suggestions.Suggestors.Attributes;

public sealed class LG_WardenObjective_ReactorSuggestorTagAttribute : SuggestorTagAttribute
{
    private readonly IQcSuggestorTag[] _tags = { new LG_WardenObjective_ReactorSuggestorTag() };

    public override IQcSuggestorTag[] GetSuggestorTags()
    {
        return _tags;
    }
}
