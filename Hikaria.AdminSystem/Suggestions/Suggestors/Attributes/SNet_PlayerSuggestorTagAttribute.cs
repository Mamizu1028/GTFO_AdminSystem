using Hikaria.AdminSystem.Suggestions.Suggestors.Tags;
using Hikaria.QC;

namespace Hikaria.AdminSystem.Suggestions.Suggestors.Attributes;

public class SNet_PlayerSuggestorTagAttribute : SuggestorTagAttribute
{
    private readonly IQcSuggestorTag[] _tags = { new SNet_PlayerSuggestorTag() };

    public override IQcSuggestorTag[] GetSuggestorTags()
    {
        return _tags;
    }
}
