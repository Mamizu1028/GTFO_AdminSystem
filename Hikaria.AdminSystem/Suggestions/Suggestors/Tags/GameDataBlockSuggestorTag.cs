using GameData;
using Hikaria.QC;

namespace Hikaria.AdminSystem.Suggestions.Suggestors.Tags;

public struct GameDataBlockSuggestorTag<TBlock> : IQcSuggestorTag where TBlock : GameDataBlockBase<TBlock>
{
}
