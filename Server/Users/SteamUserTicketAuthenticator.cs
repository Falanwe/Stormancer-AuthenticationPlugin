using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Server.Users
{
    public class SteamUserTicketAuthenticator : ISteamUserTicketAuthenticator
    {
        private const string AuthenticateUserTicketUri = "https://api.steampowered.com/ISteamUserAuth/AuthenticateUserTicket/v0001/";

        private readonly string _steamApiKey;
        private readonly uint _appSteamId;

        public SteamUserTicketAuthenticator(string steamApiKey, uint appSteamId)
        {
            _steamApiKey = steamApiKey;
            _appSteamId = appSteamId;
        }

        public async Task<ulong?> AuthenticateUserTicket(string ticket)
        {
            var data = new Dictionary<string, string>
            {
                { "key",  _steamApiKey },
                {"appid", _appSteamId.ToString() },
                {"ticket", ticket }
            };

            var content = new FormUrlEncodedContent(data);

            var client = new HttpClient();

            using (var response = await client.PostAsync(AuthenticateUserTicketUri, content))
            {
                if (response.IsSuccessStatusCode)
                {
                    using (var reader = new BinaryReader(await response.Content.ReadAsStreamAsync()))
                    {
                        return reader.ReadUInt64();
                    }
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
