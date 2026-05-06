using Hikaria.QC;
using LevelGeneration;

namespace Hikaria.AdminSystem.Parsers;

internal class LG_WardenObjective_ReactorParser : BasicQcParser<LG_WardenObjective_Reactor>
{
    public override LG_WardenObjective_Reactor Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("参数不能为空");

        var serialNumberText = value.Skip(8).ToArray();
        if (!value.StartsWith("REACTOR_", StringComparison.OrdinalIgnoreCase) || !int.TryParse(serialNumberText, out var serialNumber))
            throw new FormatException($"无效的格式: \"{value}\"。正确格式应为: REACTOR_<SerialNumber>，例如 REACTOR_123");

        foreach (var reactor in UnityEngine.Object.FindObjectsOfType<LG_WardenObjective_Reactor>())
        {
            if (reactor.m_serialNumber == serialNumber)
                return reactor;
        }
        return null;
    }
}