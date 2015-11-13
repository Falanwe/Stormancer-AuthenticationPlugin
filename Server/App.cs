using Stormancer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Users;

namespace Server
{
    public class App
    {
        public void Run(IAppBuilder builder)
        {
            builder.SceneTemplate("empty", scene => { });

            var userConfig = new Users.UserManagementConfig() { SceneIdRedirect = "main" /*Constants.MATCHMAKER_NAME*/ };
            userConfig.AuthenticationProviders.Add(new LoginPasswordAuthenticationProvider());
            userConfig.AuthenticationProviders.Add(new SteamAuthenticationProvider());

            builder.AddPlugin(new UsersManagementPlugin(userConfig));
        }
    }
}
