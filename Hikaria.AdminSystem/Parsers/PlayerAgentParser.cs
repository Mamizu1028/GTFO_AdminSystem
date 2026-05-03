using Hikaria.QC;
using Player;

namespace Hikaria.AdminSystem.Parsers;

public class PlayerAgentParser : BasicQcParser<PlayerAgent>
{
    public override PlayerAgent Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("参数不能为空");

        if (!int.TryParse(value, out var slot))
            throw new FormatException($"无效的格式: \"{value}\"。正确格式应为: PlayerSlotIndex，例如 0");

        if (!PlayerManager.TryGetPlayerAgent(ref slot, out var agent))
            throw new ArgumentException($"找不到玩家: {slot}");

        return agent;
    }
}
