using LevelGeneration;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Members;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using TheArchive.Core.Localization;
using TheArchive.Interfaces;

namespace Hikaria.AdminSystem.Features.Accessibility;

[EnableFeatureByDefault]
internal class TerminalAutoCompleteEnhancement : Feature
{
    public override string Name => "终端自动补全增强";

    public override GroupBase Group => ModuleGroup.GetOrCreateSubGroup("Accessibility");

    public static new IArchiveLogger FeatureLogger { get; set; }

    [FeatureConfig]
    public static TerminalAutoCompleteEnhancementSettings Settings { get; set; }

    public class TerminalAutoCompleteEnhancementSettings
    {
        [FSDisplayName("补全目标")]
        public List<AutoCompleteTargets> Targets { get; set; } = new();
    }

    [Localized]
    public enum AutoCompleteTargets
    {
        LogFile,
        UplinkIP,
        VerifyCode,
        Password
    }

    [ArchivePatch(typeof(LG_TERM_EnterPassword), nameof(LG_TERM_EnterPassword.m_allowAutoComplete), patchMethodType: ArchivePatch.PatchMethodType.Getter)]
    private static class LG_TERM_EnterPassword__get_m_allowAutoComplete__Patch
    {
        private static void Postfix(ref bool __result)
        {
            __result |= Settings.Targets.Contains(AutoCompleteTargets.Password);
        }
    }

    [ArchivePatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.TryUpdateLineForAutoComplete))]
    private static class LG_ComputerTerminalCommandInterpreter__TryUpdateLineForAutoComplete__Patch
    {
        private static bool Prefix(LG_ComputerTerminalCommandInterpreter __instance, string input, ref string autoCompletedLine, ref bool __result)
        {
            __result = TryUpdateLineForAutoComplete(__instance, input, out autoCompletedLine);
            return false;
        }

        public static bool TryUpdateLineForAutoComplete(LG_ComputerTerminalCommandInterpreter __instance, string input, out string autoCompletedLine)
        {
            autoCompletedLine = string.Empty;

            if (__instance.m_terminal.IsPasswordProtected)
            {
                if (!Settings.Targets.Contains(AutoCompleteTargets.Password))
                    return false;
                autoCompletedLine = __instance.m_terminal.m_password.ToUpperInvariant();
                return true;
            }
            
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            var matches = new Il2CppSystem.Collections.Generic.List<string>();
            int lastSpaceIndex = input.LastIndexOf(' ');

            bool noSpace = lastSpaceIndex == -1;
            if (noSpace)
            {
                foreach (var pair in __instance.m_commandsPerString)
                {
                    if (pair.Key.StartsWith(input, StringComparison.OrdinalIgnoreCase) &&
                        !__instance.m_terminal.CommandIsHidden(pair.Value))
                    {
                        matches.Add(pair.Key.ToUpperInvariant());
                    }
                }
            }
            else
            {
                autoCompletedLine = input[..(lastSpaceIndex + 1)];
                string partialWord = input[(lastSpaceIndex + 1)..];

                if (__instance.TryGetCommand(input, out var command, out _, out _))
                {
                    var terminal = __instance.m_terminal;
                    switch (command)
                    {
                        case TERM_Command.ReadLog:
                            if (!Settings.Targets.Contains(AutoCompleteTargets.LogFile))
                                break;
                            foreach (var logFile in terminal.GetLocalLogs().Keys)
                            {
                                if (logFile.StartsWith(partialWord, StringComparison.OrdinalIgnoreCase))
                                {
                                    matches.Add(logFile.ToUpperInvariant());
                                }
                            }
                            break;
                        case TERM_Command.TerminalUplinkConnect:
                        case TERM_Command.TerminalCorruptedUplinkConnect:
                            if (!Settings.Targets.Contains(AutoCompleteTargets.UplinkIP))
                                break;
                            var uplinkIP = terminal.UplinkPuzzle.TerminalUplinkIP.ToUpperInvariant();
                            if (string.IsNullOrEmpty(partialWord))
                            {
                                autoCompletedLine += uplinkIP;
                                return true;
                            }
                            matches.Add(uplinkIP);
                            break;
                        case TERM_Command.TerminalUplinkVerify:
                        case TERM_Command.TerminalCorruptedUplinkVerify:
                            if (!Settings.Targets.Contains(AutoCompleteTargets.VerifyCode))
                                break;
                            var verifyCode = terminal.UplinkPuzzle.CurrentRound.CorrectCode.ToUpperInvariant();
                            if (string.IsNullOrEmpty(partialWord))
                            {
                                autoCompletedLine += verifyCode;
                                return true;
                            }
                            matches.Add(verifyCode);
                            break;
                        case TERM_Command.ReactorVerify:
                            if (!Settings.Targets.Contains(AutoCompleteTargets.VerifyCode))
                                break;
                            var reactorCode = terminal.ConnectedReactor.CurrentStateOverrideCode.ToUpperInvariant();
                            if (string.IsNullOrEmpty(partialWord))
                            {
                                autoCompletedLine += reactorCode;
                                return true;
                            }
                            matches.Add(reactorCode);
                            break;
                        default:
                            var keywords = LG_ComputerTerminalManager.GetAutoCompleteKeywords();
                            foreach (string keyword in keywords)
                            {
                                if (keyword.StartsWith(partialWord, StringComparison.OrdinalIgnoreCase))
                                {
                                    matches.Add(keyword.ToUpperInvariant());
                                }
                            }
                            break;
                    }
                }
                else
                {
                    var keywords = LG_ComputerTerminalManager.GetAutoCompleteKeywords();
                    foreach (string keyword in keywords)
                    {
                        if (keyword.StartsWith(partialWord, StringComparison.OrdinalIgnoreCase))
                        {
                            matches.Add(keyword.ToUpperInvariant());
                        }
                    }
                }
            }

            bool found = LG_ComputerTerminalCommandInterpreter.FindCommonPrefix(matches, out string commonPrefix, noSpace ? " " : string.Empty);
            autoCompletedLine += commonPrefix.ToUpperInvariant();
            return found;
        }
    }
}
