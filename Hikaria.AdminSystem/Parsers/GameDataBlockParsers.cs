using GameData;

namespace Hikaria.AdminSystem.Parsers;

public class GameDataBlockParsers : GameDataBlockParserBase<ItemDataBlock> { }

public class EnemyDataBlockParser : GameDataBlockParserBase<EnemyDataBlock> { }

public class ItemDataBlockParser : GameDataBlockParserBase<ItemDataBlock> { }

public class FogSettingsDataBlockParser : GameDataBlockParserBase<FogSettingsDataBlock> { }
