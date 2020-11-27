using DFC.Api.AppRegistry.Common;
using DFC.Swagger.Standard.Annotations;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace DFC.Api.AppRegistry.Models
{
    [ExcludeFromCodeCoverage]
    public class AjaxRequestModel : IValidatableObject
    {
        [Display(Description = "The Ajax Request endpoint name used by the Shell, that the application will supply data for.")]
        [Example(Description = "Sortby")]
        [Required]
        public string? Name { get; set; }

        [Display(Description = "Indicator stating that the Ajax Request endpoint is working as expected.")]
        [Example(Description = "true")]
        public bool IsHealthy { get; set; } = true;

        [Display(Description = "A url to the Ajax Request that supplies response for the given area.")]
        [Example(Description = "https://localhost:44306/course/search/{0}/SortBy")]
        [Required]
        public string? AjaxEndpoint { get; set; }

        [Display(Description = "Indicator stating that the Ajax Request endpoint has a health check endpoint that requires monitoring by the HealthService. ")]
        [Example(Description = "true")]
        public bool HealthCheckRequired { get; set; }

        [Display(Description = "If the Ajax Request is marked as unhealthy then this markup will be used to populate the response.")]
        [Example(Description = "<b>Service unavailable</b>")]
        public string? OfflineHtml { get; set; }

        [Display(Description = "UTC date and time the Ajax Request was registered.")]
        [Example(Description = "2019-05-01T13:32:00")]
        public DateTime? DateOfRegistration { get; set; }

        [Display(Description = "UTC date and time of when the Ajax Request was last updated.")]
        [Example(Description = "2019-05-01T13:32:00")]
        public DateTime? LastModifiedDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var result = new List<ValidationResult>();

            const string PlaceMarkerStub = "{0}";
            var ajaxEndpoint = AjaxEndpoint;

            if (string.IsNullOrWhiteSpace(Name))
            {
                result.Add(new ValidationResult($"Request value for '{nameof(Name)}' is missing its value"));
            }

            if (string.IsNullOrWhiteSpace(ajaxEndpoint))
            {
                result.Add(new ValidationResult($"Request value for '{nameof(AjaxEndpoint)}' is missing its value"));
            }
            else
            {
                if (ajaxEndpoint.Contains(PlaceMarkerStub, StringComparison.OrdinalIgnoreCase))
                {
                    // this is allowable, so replace with a valid string to permit the Uri.IsWellFormedUriString to check the resulting string
                    ajaxEndpoint = ajaxEndpoint.Replace(PlaceMarkerStub, "valid", StringComparison.OrdinalIgnoreCase);
                }

                if (!Uri.IsWellFormedUriString(ajaxEndpoint, UriKind.Absolute))
                {
                    result.Add(new ValidationResult($"Request value for '{nameof(AjaxEndpoint)}' is not a valid absolute Uri ({AjaxEndpoint})"));
                }
            }

            if (!string.IsNullOrEmpty(OfflineHtml))
            {
                var htmlDoc = new HtmlDocument();

                htmlDoc.LoadHtml(OfflineHtml);

                if (htmlDoc.ParseErrors.Any())
                {
                    result.Add(new ValidationResult(string.Format(CultureInfo.InvariantCulture, ValidationMessages.MalformedHtml, nameof(OfflineHtml))));
                }
            }

            return result;
        }
    }
}
