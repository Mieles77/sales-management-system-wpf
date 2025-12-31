using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Prueba_Apis.Services
{
    public class GroqService
    {
        private readonly string _apiKey = "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";
        private readonly string _endpoint = "https://api.groq.com/openai/v1/chat/completions";

        public async Task<string> ConsultarIA(string preguntaUsuario, string contextoTienda)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                    var requestBody = new
                    {
                        model = "llama-3.1-8b-instant", // Modelo gratuito y rápido
                        messages = new[]
                        {
                            new {
                                role = "system",
                                content = $"Eres un asistente de inventario, responde con buen humor. Datos del inventario: {contextoTienda}"
                            },
                            new {
                                role = "user",
                                content = preguntaUsuario
                            }
                        },
                        temperature = 0.7,
                        max_tokens = 500
                    };

                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(_endpoint, content);
                    string resultJson = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var data = JObject.Parse(resultJson);
                        return data["choices"]?[0]?["message"]?["content"]?.ToString() ?? "Sin respuesta";
                    }

                    return $"Error: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}