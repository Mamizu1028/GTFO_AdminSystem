using Hikaria.QC;
using LevelGeneration;

namespace Hikaria.AdminSystem.Parsers;

public class iTerminalItemParser : BasicQcParser<iTerminalItem>
{
    public override iTerminalItem Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("参数不能为空");

        string[] parts = value.Split('.');

        if (parts.Length != 2)
        {
            throw new FormatException(
                $"无效的格式: \"{value}\"。正确格式应为: terminalItemKey.dimensionIndex，例如 AMMOPACK_123.Reality");
        }

        string terminalItemKey = parts[0];
        string dimensionText = parts[1];

        if (!Enum.TryParse(dimensionText, true, out eDimensionIndex dimensionIndex))
            throw new FormatException($"无效的格式: \"{dimensionText}\"。无法解析为 {nameof(eDimensionIndex)}");

        if (!LG_LevelInteractionManager.TryGetTerminalInterface(terminalItemKey, dimensionIndex, out var item))
            throw new ArgumentException($"找不到 TerminalItem: Key={terminalItemKey}, Dimension={dimensionIndex}");

        return item;
    }
}
