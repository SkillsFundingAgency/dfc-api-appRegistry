using DFC.Api.AppRegistry.Common;
using DFC.Api.AppRegistry.Enums;
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
    public class RegionModel : IValidatableObject
    {
        [Display(Description = "The region on a page to which the application endpoint will supply data for.")]
        [Example(Description = "5")]
        [Required]
        public PageRegion PageRegion { get; set; }

        [Display(Description = "Indicator stating that the application endpoint is working as expected. ")]
        [Example(Description = "true")]
        public bool IsHealthy { get; set; } = true;

        [Display(Description = "A url to the application that supplies content for the given area.")]
        [Example(Description = "https://ncs.careers.azurewebsites/explore/sidebar or https://ncs.careers.azurewebsites/explore/{0}/contents")]
        [Required]
        public string? RegionEndpoint { get; set; }

        [Display(Description = "Indicator stating that the application endpoint has a health check endpoint that requires monitoring by the HealthService. ")]
        [Example(Description = "true")]
        public bool HealthCheckRequired { get; set; }

        [Display(Description = "If the application is marked as unhealthy then this markup will be used to populate the region.")]
        [Example(Description = "<b>Service unavailable</b>")]
        public string? OfflineHtml { get; set; }

        [Display(Description = "UTC date and time the region was registered.")]
        [Example(Description = "2019-05-01T13:32:00")]
        public DateTime? DateOfRegistration { get; set; }

        [Display(Description = "UTC date and time of when the region was last updated.")]
        [Example(Description = "2019-05-01T13:32:00")]
        public DateTime? LastModifiedDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var result = new List<ValidationResult>();

            const string PlaceMarkerStub = "{0}";
            var regionEndpoint = RegionEndpoint;

            if (string.IsNullOrWhiteSpace(regionEndpoint))
            {
                result.Add(new ValidationResult($"Request value for '{nameof(RegionEndpoint)}' is missing its value"));
            }
            else
            {
                if (regionEndpoint.Contains(PlaceMarkerStub, StringComparison.OrdinalIgnoreCase))
                {
                    // this is allowable, so replace with a valid string to permit the Uri.IsWellFormedUriString to check the resulting string
                    regionEndpoint = regionEndpoint.Replace(PlaceMarkerStub, "valid", StringComparison.OrdinalIgnoreCase);
                }

                if (!Uri.IsWellFormedUriString(regionEndpoint, UriKind.Absolute))
                {
                    result.Add(new ValidationResult($"Request value for '{nameof(RegionEndpoint)}' is not a valid absolute Uri ({RegionEndpoint})"));
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
