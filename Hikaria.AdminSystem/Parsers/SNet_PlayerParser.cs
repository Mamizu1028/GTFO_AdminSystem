using Hikaria.QC;
using SNetwork;

namespace Hikaria.AdminSystem.Parsers;

public class SNet_PlayerParser : BasicQcParser<SNet_Player>
{
    public override SNet_Player Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("参数不能为空");

        if (!ulong.TryParse(value, out var lookup))
            throw new FormatException($"无效的格式: \"{value}\"。正确格式应为: SteamID64，例如 76561197960287930");

        if (!SNet.TryGetPlayer(lookup, out var player))
            throw new ArgumentException($"找不到玩家: {lookup}");

        return player;
    }
}
