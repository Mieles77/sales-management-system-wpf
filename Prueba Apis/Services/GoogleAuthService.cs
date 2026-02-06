using Firebase.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TuApp.Services
{
    public class GoogleAuthService
    {
        // REEMPLAZA ESTOS VALORES CON LOS TUYOS
        private const string ClientId = "Cambia esto";
        private const string ClientSecret = "Private";

        private static readonly string[] Scopes = {
            "https://www.googleapis.com/auth/userinfo.profile",
            "https://www.googleapis.com/auth/userinfo.email"
        };

        private Google.Apis.Auth.OAuth2.UserCredential _credential;

        /// <summary>
        /// Inicia sesión con Google y obtiene las credenciales del usuario
        /// </summary>
        public async Task<bool> LoginAsync()
        {
            try
            {
                // Ruta donde se guardarán los tokens (para no pedir login cada vez)
                string credPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Prueba Apis",
                    "token"
                );

                // Iniciar el flujo de autenticación
                _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = ClientId,
                        ClientSecret = ClientSecret
                    },
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)
                );

                return _credential != null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al autenticar con Google: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene la información del usuario autenticado
        /// </summary>
        public async Task<Userinfo> GetUserInfoAsync()
        {
            if (_credential == null)
            {
                throw new InvalidOperationException("Debe llamar a LoginAsync() primero");
            }

            try
            {
                // Crear el servicio de OAuth2
                var service = new Oauth2Service(new BaseClientService.Initializer
                {
                    HttpClientInitializer = _credential,
                    ApplicationName = "Prueba Apis"
                });

                // Obtener información del usuario
                var userInfo = await service.Userinfo.Get().ExecuteAsync();
                return userInfo;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener información del usuario: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cierra sesión y elimina los tokens guardados
        /// </summary>
        public async Task LogoutAsync()
        {
            if (_credential != null)
            {
                // Revocar el token
                await _credential.RevokeTokenAsync(CancellationToken.None);

                // Eliminar archivos de token guardados
                string credPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Prueba Apis",
                    "token"
                );

                if (Directory.Exists(credPath))
                {
                    Directory.Delete(credPath, true);
                }

                _credential = null;
            }
        }

        /// <summary>
        /// Verifica si el usuario ya está autenticado
        /// </summary>
        public bool IsAuthenticated()
        {
            return _credential != null;
        }
    }

    // Modelo para almacenar datos del usuario
    public class GoogleUser
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public bool VerifiedEmail { get; set; }

        public static GoogleUser FromUserInfo(Userinfo info)
        {
            return new GoogleUser
            {
                Id = info.Id,
                Email = info.Email,
                Name = info.Name,
                Picture = info.Picture,
                VerifiedEmail = info.VerifiedEmail ?? false
            };
        }
    }
}
