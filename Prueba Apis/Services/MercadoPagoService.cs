using MercadoPago;
using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Preference;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Markup;
using MercadoPago.Client.Payment;
using System.Linq;

namespace Prueba_Apis.Services
{
    public class MercadoPagoService
    {
        private readonly string _accessToken;

        public MercadoPagoService()
        {
            // ⚠️ Obtén tu Access Token en: https://www.mercadopago.com.co/developers/panel/app
            _accessToken = "APP_USR-83911519100970-123119-add59c4550a5b802be2dc1337442d2c9-3102930545"; // Usa TEST para pruebas
            MercadoPagoConfig.AccessToken = _accessToken;
        }

        /// <summary>
        /// Crea una preferencia de pago con suscripción mensual
        /// </summary>
        public async Task<string> CrearPreferenciaPago(string correoUsuario, string nombreUsuario)
        {
            try
            {
                var request = new PreferenceRequest
                {
                    Items = new List<PreferenceItemRequest>
                    {
                        new PreferenceItemRequest
                        {
                            Title = "Plan Premium - Inventario IA",
                            Description = "Acceso completo con 15 días de prueba gratis",
                            Quantity = 1,
                            CurrencyId = "COP", // Pesos colombianos
                            UnitPrice = 39900m, // $39,900 COP (~$10 USD)
                        }
                    },
                    Payer = new PreferencePayerRequest
                    {
                        Email = correoUsuario,
                        Name = nombreUsuario
                    },
                    BackUrls = new PreferenceBackUrlsRequest
                    {
                        Success = "https://tusitio.com/success",
                        Failure = "https://tusitio.com/failure",
                        Pending = "https://tusitio.com/pending"
                    },
                    AutoReturn = "approved",
                    StatementDescriptor = "INVENTARIO_IA",
                    ExternalReference = correoUsuario, // Para identificar al usuario
                    NotificationUrl = "https://tusitio.com/webhook" // Para recibir notificaciones
                };

                var client = new PreferenceClient();
                Preference preference = await client.CreateAsync(request);

                return preference.InitPoint; // URL de pago
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear preferencia de pago: {ex.Message}");
            }
        }

        /// <summary>
        /// Abre el link de pago en el navegador
        /// </summary>
        public async Task AbrirPaginaPago(string correoUsuario, string nombreUsuario)
        {
            string url = await CrearPreferenciaPago(correoUsuario, nombreUsuario);
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        /// <summary>
        /// Crea un plan de suscripción (requiere configuración en dashboard)
        /// </summary>
        public async Task<string> CrearSuscripcion(string correoUsuario)
        {
            // Para suscripciones recurrentes en Mercado Pago, 
            // necesitas crear un "Plan de Suscripción" en el dashboard
            // URL: https://www.mercadopago.com.co/tools/subscriptions

            // Aquí puedes redirigir al plan creado
            string planId = "TU_PLAN_ID"; // ID del plan creado en el dashboard
            string urlSuscripcion = $"https://www.mercadopago.com.co/subscriptions/checkout?preapproval_plan_id={planId}";

            return urlSuscripcion;
        }
     

public async Task<bool> VerificarPagoExitoso(string correoUsuario)
    {
        try
        {
            var client = new PaymentClient();

            // Buscamos los últimos pagos
            var searchRequest = new MercadoPago.Client.SearchRequest
            {
                Filters = new Dictionary<string, object>
            {
                { "payer.email", correoUsuario },
                { "status", "approved" } // Solo los aprobados
            }
            };

            var payments = await client.SearchAsync(searchRequest);

            // Si hay al menos uno hoy o reciente, devolvemos true
            return payments.Results.Any();
        }
        catch (Exception)
        {
            return false;
        }
    }
}

    public class InfoPago
    {
        public string Estado { get; set; } // approved, pending, rejected
        public string IdTransaccion { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
    }
}