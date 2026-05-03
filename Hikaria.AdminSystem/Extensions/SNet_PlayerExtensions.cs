using Hikaria.QC.Utilities;
using SNetwork;

namespace Hikaria.AdminSystem.Extensions;

public static class SNet_PlayerExtensions
{
    public static string GetColoredNameWithoutRichTextTags(this SNet_Player player)
    {
        return $"<color=#{ColorExt.ToHex(player.PlayerColor)}>{player.NickName.RemoveRichTextTags()}</color>";
    }
}
