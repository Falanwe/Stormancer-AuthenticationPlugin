﻿using Newtonsoft.Json;
using Stormancer.Authentication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace Stormancer.Authentication
{
    public class AuthenticatorService
    {
        #region Config
        private string _authenticatorSceneName = "authenticator";
        public string AuthenticatorSceneName
        {
            get { return _authenticatorSceneName; }
            set { _authenticatorSceneName = value; }
        }

        private string _createUserRoute = "provider.loginpassword.createAccount";
        public string CreateUserRoute
        {
            get { return _createUserRoute; }
            set { _createUserRoute = value; }
        }

        private string _loginRoute = "login";
        public string LoginRoute
        {
            get { return _loginRoute; }
            set { _loginRoute = value; }
        }
        #endregion

        #region fields
        private readonly Client _client;
        private Task<Scene> _authenticatorScene;
        #endregion

        public AuthenticatorService(Client client)
        {
            _client = client;
        }

        public Task<Scene> Login(string login, string password)
        {
            return Login(new Dictionary<string, string>
            {
                { "login", login },
                {"password", password },
                { "provider", "loginpassword" }
            });
        }

        public async Task<Scene> Login(Dictionary<string, string> authenticationContext)
        {
            EnsureAuthenticatorSceneExists();

            var scene = await _authenticatorScene;

            var loginResult = await scene.RpcTask<Dictionary<string, string>, LoginResult>(LoginRoute, authenticationContext, Core.PacketPriority.HIGH_PRIORITY);

            if (!loginResult.Success)
            {
                throw new InvalidCredentialException(loginResult.ErrorMsg);
            }

            return await _client.GetScene(loginResult.Token);
        }

        public async Task CreateLoginPasswordAccount<T>(string login, string password, string email, T userData)
        {
            EnsureAuthenticatorSceneExists();

            var scene = await _authenticatorScene;

            if (!scene.RemoteRoutes.Any(r => r.Name == CreateUserRoute))
            {
                throw new InvalidOperationException("User creation is disabled in this application.");
            }

            var createAccountRequest = new CreateAccountRequest
            {
                Login = login,
                Password = password,
                Email = email,
                UserData = JsonConvert.SerializeObject(userData)
            };

            var createAccountResult = await scene.RpcTask<CreateAccountRequest, LoginResult>(CreateUserRoute, createAccountRequest);

            if (!createAccountResult.Success)
            {
                throw new InvalidOperationException(createAccountResult.ErrorMsg);
            }
        }

        private void EnsureAuthenticatorSceneExists()
        {
            _authenticatorScene = _authenticatorScene ?? GetAuthenticatorScene();
        }

        private async Task<Scene> GetAuthenticatorScene()
        {
            var result = await _client.GetPublicScene(AuthenticatorSceneName, "");

            await result.Connect();

            return result;
        }

        public async Task Logout()
        {
            var scene = await _authenticatorScene;
            _authenticatorScene = null;

            if (scene != null)
            {
                if (scene.Connected)
                {
                    await scene.Disconnect();
                }
            }
        }
    }
}
