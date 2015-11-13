using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stormancer.Core;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.IO;
using Stormancer.Server.Components;

namespace Server.Users
{
    public class SteamAuthenticationProvider : IAuthenticationProvider
    {
        private const string Provider_Name = "steam";
        private const string ClaimPath = "steamid";

        private ISteamUserTicketAuthenticator _authenticator;

        public SteamAuthenticationProvider()
        {            
        }

        public void AddMetadata(Dictionary<string, string> result)
        {
            result.Add("provider.steamauthentication", "enabled");
        }

        public void AdjustScene(ISceneHost scene)
        {
            var steamConfig = scene.GetComponent<IEnvironment>().Configuration.steam;

            //_authenticator = new SteamUserTicketAuthenticator( steamConfig.apiKey, steamConfig.appId);

            _authenticator = new SteamUserTicketAuthenticatorMockup();
        }

        public async Task<AuthenticationResult> Authenticate(Dictionary<string, string> authenticationCtx, IUserService userService)
        {
            if (authenticationCtx["provider"] != Provider_Name)
            {
                return null;
            }

            string ticket;
            if (!authenticationCtx.TryGetValue("ticket", out ticket) || string.IsNullOrWhiteSpace(ticket))
            {
                return AuthenticationResult.CreateFailure("Steam session ticket must not be empty.", Provider_Name, authenticationCtx);
            }

            var steamId = await _authenticator.AuthenticateUserTicket(ticket);

            if (!steamId.HasValue)
            {
                return AuthenticationResult.CreateFailure("Invalid steam session ticket.", Provider_Name, authenticationCtx);
            }

            var steamIdString = steamId.GetValueOrDefault().ToString();
            var user = await userService.GetUserByClaim(Provider_Name, ClaimPath, steamIdString);

            if (user == null)
            {
                var uid = Provider_Name + "-" + steamIdString;
                user = await userService.CreateUser(uid, new JObject());

                user = await userService.AddAuthentication(user, Provider_Name, JObject.FromObject(new { CLAIMPATH = steamIdString }));
            }

            return AuthenticationResult.CreateSuccess(user, Provider_Name, authenticationCtx);
        }

    }
}
