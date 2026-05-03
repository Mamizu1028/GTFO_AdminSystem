using GameData;
using Hikaria.QC;

namespace Hikaria.AdminSystem.Parsers;

public abstract class GameDataBlockParserBase<TBlock> : BasicQcParser<TBlock> where TBlock : GameDataBlockBase<TBlock>
{
    public override TBlock Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("参数不能为空。格式应为: name.persistentID");
        }

        string[] parts = value.Split('.');

        if (parts.Length != 2)
        {
            throw new FormatException(
                $"无效的格式: \"{value}\"。正确格式应为: name.persistentID");
        }

        return GameDataBlockBase<TBlock>.GetBlock(parts[0]);
    }
}