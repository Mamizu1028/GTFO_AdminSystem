using Hikaria.AdminSystem.Suggestions.Suggestors.Tags;
using Hikaria.QC;

namespace Hikaria.AdminSystem.Suggestions.Suggestors.Attributes;

public class PlayerAgentSuggestorTagAttribute : SuggestorTagAttribute
{
    private readonly IQcSuggestorTag[] _tags = { new PlayerAgentSuggestorTag() };

    public override IQcSuggestorTag[] GetSuggestorTags()
    {
        return _tags;
    }
}
