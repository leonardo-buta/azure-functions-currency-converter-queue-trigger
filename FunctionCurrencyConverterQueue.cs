using System;
using System.Net.Http;
using System.Threading.Tasks;
using CurrencyConverter.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace CurrencyConverter
{
    public class FunctionCurrencyConverterQueue
    {
        private const string QUEUE_NAME = "queue-currency";
        private HttpClient client = new HttpClient();

        [FunctionName(nameof(FunctionCurrencyConverterQueue))]
        public async Task Run([QueueTrigger(QUEUE_NAME, Connection = "AzureWebJobsStorage")] CurrencyConversion item,
            ILogger log)
        {
            log.LogInformation($"Convertendo {item.Value} {item.CurrencyFrom} para {item.CurrencyTo}");

            try
            {
                var url = string.Format(Environment.GetEnvironmentVariable("CurrencyApiUrl"), item.CurrencyFrom, item.CurrencyTo);
                var resultadoResposta = JObject.Parse(await client.GetStringAsync(url));
                var resultadoConversao = item.Value * Convert.ToDecimal(resultadoResposta[item.CurrencyTo]);
                log.LogInformation($"{item.Value} {item.CurrencyFrom} = {resultadoConversao.ToString("0.##")} {item.CurrencyTo}");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ocorreu um erro ao realizar a conversão");
            }
        }
    }
}
