using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stormancer.Authentication.TestClient
{
    class Program
    {
        private const string _accountId = "d9590543-56c3-c94a-f7bf-c394b26deb15";
        private const string _appName = "authentication-test";
        private const string _authenticatorSceneName = "authenticator";

        static void Main(string[] args)
        {
            MainImpl().Wait();

            Console.WriteLine("done.");
            Console.ReadLine();
        }

        private static async Task MainImpl()
        {
            Console.WriteLine("creating account config.");

            var stormancerConfig = ClientConfiguration.ForAccount(_accountId, _appName);
            stormancerConfig.AddPlugin(new AuthenticationPlugin());

            Console.WriteLine("creating client.");

            var client = new Client(stormancerConfig);

            Console.WriteLine("Getting authenticator.");

            var authenticator = client.Authenticator();

            Console.WriteLine("Authenticating.");

            var mainScene = await authenticator.SteamLogin("myTicket");
            
            Console.WriteLine("Connecting to the authenticated scene.");

            await mainScene.Connect();

            Console.WriteLine("Connected.");
        }
    }
}
