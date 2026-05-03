using GameData;
using Hikaria.AdminSystem.Suggestions.Suggestors.Tags;
using Hikaria.QC;

namespace Hikaria.AdminSystem.Suggestions.Suggestors.Attributes;

public sealed class ItemDataBlockSuggestorTagAttribute : SuggestorTagAttribute
{
    private readonly IQcSuggestorTag[] _tags = { new GameDataBlockSuggestorTag<ItemDataBlock>() };

    public override IQcSuggestorTag[] GetSuggestorTags()
    {
        return _tags;
    }
}

public sealed class FogSettingsDataBlockSuggestorTagAttribute : SuggestorTagAttribute
{
    private readonly IQcSuggestorTag[] _tags = { new GameDataBlockSuggestorTag<FogSettingsDataBlock>() };

    public override IQcSuggestorTag[] GetSuggestorTags()
    {
        return _tags;
    }
}

public sealed class EnemyDataBlockSuggestorTagAttribute : SuggestorTagAttribute
{
    private readonly IQcSuggestorTag[] _tags = { new GameDataBlockSuggestorTag<EnemyDataBlock>() };

    public override IQcSuggestorTag[] GetSuggestorTags()
    {
        return _tags;
    }
}