using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchiSteamFarm.Localization;
using static ArchiSteamFarm.Steam.Integration.ArchiWebHandler;
using ArchiSteamFarm.Core;
using System.Composition;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;


namespace Replay.Replay
{
    [Export(typeof(IPlugin))]
    internal sealed class Replay : IBotCommand2
    {


        public string Name => nameof(Replay);
        public Version Version => typeof(Replay).Assembly.GetName().Version;

        public async Task<string> OnBotCommand(Bot bot, EAccess access, string message, string[] args, ulong steamID)
        {


            switch (args[0].ToUpperInvariant())
            {
                case "REPLAY":

                    {

                        return await ResponseCreateReplay(bot, steamID, ArchiSteamFarm.Core.Utilities.GetArgsAsText(args, 1, ",")).ConfigureAwait(false);

                    }

                default:

                    return null;
            }

        }

        public Task OnLoaded()
        {
            Console.WriteLine("Replay Plugin Loaded!");
            return Task.CompletedTask;
        }



        public static async Task<string> ResponseCreateReplay(Bot Bot, ulong steamID)
        {

            if (!Bot.IsConnectedAndLoggedOn)
            {
                return Bot.Commands.FormatBotResponse(Strings.BotNotConnected);
            }

            await CreateReplay(Bot).ConfigureAwait(false);

            return Bot.Commands.FormatBotResponse(Strings.Done);
        }


        private static async Task<string> ResponseCreateReplay(Bot Bot, ulong steamID, string botNames)
        {
            if (string.IsNullOrEmpty(botNames))
            {
                ASF.ArchiLogger.LogNullError(nameof(steamID) + " || " + nameof(botNames));

                return null;
            }

            HashSet<Bot> bots = Bot.GetBots(botNames);

            if ((bots == null) || (bots.Count == 0))
            {
                return Bot.Commands.FormatBotResponse(string.Format(Strings.BotNotFound, botNames));
            }
            IList<string> results = await Utilities.InParallel(bots.Select(bot => ResponseCreateReplay(bot, steamID))).ConfigureAwait(false);

            List<string> responses = new List<string>(results.Where(result => !string.IsNullOrEmpty(result)));

            return responses.Count > 0 ? string.Join(Environment.NewLine, responses) : null;
        }

        internal static async Task<bool> CreateReplay(Bot Bot)
        {

            string profileURL = await Bot.ArchiWebHandler.GetAbsoluteProfileURL().ConfigureAwait(false);

            if (string.IsNullOrEmpty(profileURL))
            {
                Bot.ArchiLogger.LogGenericWarning(Strings.WarningFailed);

                return false;
            }


            Uri request = new(SteamStoreURL + "/replay/" + Bot.SteamID+"/2022");
            // Extra entry for sessionID
            var response = await Bot.ArchiWebHandler.UrlGetToHtmlDocumentWithSession(request).ConfigureAwait(false);

            return true;
        }

    }
}








