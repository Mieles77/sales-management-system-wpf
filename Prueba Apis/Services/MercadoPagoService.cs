using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Prueba_Apis.Services
{
    public class MercadoPagoService
    {
        private readonly string _accessToken;
        private const string API_PREFERENCES_URL = "https://api.mercadopago.com/checkout/preferences";
        private const string API_SUBSCRIPTIONS_URL = "https://api.mercadopago.com/preapproval";
        private const string API_PAYMENTS_URL = "https://api.mercadopago.com/v1/payments/search";

        public MercadoPagoService()
        {
            _accessToken = "APP_USR-226444179184968-010213-8038a8256b95cdbd72ceee197eeb6065-571557338";
        }

        /// <summary>
        /// Crea una suscripción mensual con 15 días de prueba gratis
        /// </summary>
        public async Task<string> CrearSuscripcionMensual(string correoUsuario, string nombreUsuario)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                    // Fecha de inicio: 15 días después de hoy (prueba gratis)
                    DateTime fechaInicioCobro = DateTime.Now.AddDays(15);

                    var suscripcion = new
                    {
                        reason = "Suscripción Premium - Inventario IA",
                        auto_recurring = new
                        {
                            frequency = 1,
                            frequency_type = "months", // Mensual
                            transaction_amount = 39900, // $39,900 COP
                            currency_id = "COP",
                            free_trial = new
                            {
                                frequency = 15,
                                frequency_type = "days" // 15 días gratis
                            }
                        },
                        back_url = "https://tusitio.com/suscripcion-exitosa",
                        payer_email = correoUsuario,
                        external_reference = correoUsuario,
                        status = "pending"
                    };

                    string json = JsonConvert.SerializeObject(suscripcion);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(API_SUBSCRIPTIONS_URL, content);
                    string resultJson = await response.Content.ReadAsStringAsync();

                    System.Diagnostics.Debug.WriteLine("Respuesta de Mercado Pago:");
                    System.Diagnostics.Debug.WriteLine(resultJson);

                    if (response.IsSuccessStatusCode)
                    {
                        dynamic result = JsonConvert.DeserializeObject(resultJson);
                        return result.init_point.ToString(); // URL de pago
                    }
                    else
                    {
                        throw new Exception($"Error de API: {resultJson}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear suscripción: {ex.Message}");
            }
        }

        /// <summary>
        /// Crea un pago único (para probar sin suscripción)
        /// </summary>
        public async Task<string> CrearPagoUnico(string correoUsuario, string nombreUsuario)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                    var preferencia = new
                    {
                        items = new[]
                        {
                            new
                            {
                                title = "Plan Premium - Inventario IA (Mes 1)",
                                description = "Primer mes + 15 días gratis",
                                quantity = 1,
                                currency_id = "COP",
                                unit_price = 39900m
                            }
                        },
                        payer = new
                        {
                            email = correoUsuario,
                            name = nombreUsuario
                        },
                        back_urls = new
                        {
                            success = "https://tusitio.com/success",
                            failure = "https://tusitio.com/failure",
                            pending = "https://tusitio.com/pending"
                        },
                        auto_return = "approved",
                        statement_descriptor = "INVENTARIO_IA",
                        external_reference = correoUsuario,
                        expires = true,
                        expiration_date_from = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                        expiration_date_to = DateTime.Now.AddDays(30).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz")
                    };

                    string json = JsonConvert.SerializeObject(preferencia);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(API_PREFERENCES_URL, content);
                    string resultJson = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        dynamic result = JsonConvert.DeserializeObject(resultJson);
                        return result.init_point.ToString();
                    }
                    else
                    {
                        throw new Exception($"Error de API: {resultJson}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear pago: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica si el usuario completó el pago
        /// </summary>
        public async Task<bool> VerificarPagoExitoso(string correoUsuario)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Verificando pago para: {correoUsuario}");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                    string url = $"{API_PAYMENTS_URL}?external_reference={correoUsuario}&sort=date_created&criteria=desc";

                    System.Diagnostics.Debug.WriteLine($"URL: {url}");

                    var response = await client.GetAsync(url);
                    string resultJson = await response.Content.ReadAsStringAsync();

                    System.Diagnostics.Debug.WriteLine($"Respuesta API: {resultJson}");

                    if (response.IsSuccessStatusCode)
                    {
                        dynamic result = JsonConvert.DeserializeObject(resultJson);

                        if (result.results != null && result.results.Count > 0)
                        {
                            string status = result.results[0].status.ToString();
                            System.Diagnostics.Debug.WriteLine($"Estado del pago: {status}");
                            return status == "approved";
                        }
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al verificar pago: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Abre la página de pago en el navegador (suscripción mensual)
        /// </summary>
        public async Task AbrirPaginaPago(string correoUsuario, string nombreUsuario)
        {
            // Usar suscripción mensual con prueba gratis
            string url = await CrearSuscripcionMensual(correoUsuario, nombreUsuario);
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        /// <summary>
        /// Abre la página de pago único (alternativa)
        /// </summary>
        public async Task AbrirPaginaPagoUnico(string correoUsuario, string nombreUsuario)
        {
            string url = await CrearPagoUnico(correoUsuario, nombreUsuario);
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }


    }
}