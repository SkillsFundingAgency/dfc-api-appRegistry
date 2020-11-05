﻿using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class HttpRequestExtensions
    {
        public static async Task<TModel?> GetModelFromBodyAsync<TModel>(this HttpRequest request, ILogger logger)
            where TModel : class
        {
            var body = await request.GetBodyAsync<TModel>().ConfigureAwait(false);

            if (body?.Value == null)
            {
                logger.LogWarning($"Request.Body is malformed");
                return default;
            }

            return body.Value;
        }

        public static async Task<HttpResponseBody<T>> GetBodyAsync<T>(this HttpRequest request)
          where T : class
        {
            var body = new HttpResponseBody<T>();
            var bodyString = await request.ReadAsStringAsync().ConfigureAwait(false);
            body.Value = JsonConvert.DeserializeObject<T>(bodyString);

            var results = new List<ValidationResult>();

            if (body.Value != null)
            {
                body.IsValid = Validator.TryValidateObject(body.Value, new ValidationContext(body.Value, null, null), results, true);
                body.ValidationResults = results;
            }

            return body;
        }
    }
}
