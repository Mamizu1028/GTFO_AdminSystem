using Hikaria.QC;
using LevelGeneration;

namespace Hikaria.AdminSystem.Parsers;

public sealed class ZoneParser : BasicQcParser<LG_Zone>
{
    public override LG_Zone Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("参数不能为空");
        }

        string[] parts = value.Split('.');

        if (parts.Length != 3)
        {
            throw new FormatException(
                $"无效的格式: \"{value}\"。正确格式应为: dimensionIndex.layerType.alias，例如 Reality.MainLayer.42");
        }

        string dimensionText = parts[0];
        string layerText = parts[1];
        string aliasText = parts[2];

        if (!Enum.TryParse(dimensionText, true, out eDimensionIndex dimensionIndex))
        {
            throw new FormatException(
                $"无法解析 Dimension: \"{dimensionText}\"。请输入有效的 {nameof(eDimensionIndex)} 名称或数字。");
        }

        if (!Enum.TryParse(layerText, true, out LG_LayerType layerType))
        {
            throw new FormatException(
                $"无法解析 Layer: \"{layerText}\"。请输入有效的 {nameof(LG_LayerType)} 名称或数字。");
        }

        if (!int.TryParse(aliasText, out int alias))
        {
            throw new FormatException($"无法解析 Zone Alias: \"{aliasText}\"。Alias 必须是整数。");
        }

        if (!Builder.CurrentFloor.TryGetZoneByAlias(dimensionIndex, layerType, alias, out var zone))
        {
            throw new ArgumentException(
                $"找不到 Zone: Dimension={dimensionIndex}, Layer={layerType}, Alias={alias}");
        }

        return zone;
    }
}