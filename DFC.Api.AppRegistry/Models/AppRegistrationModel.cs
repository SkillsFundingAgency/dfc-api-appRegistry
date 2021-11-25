using DFC.Api.AppRegistry.Common;
using DFC.Api.AppRegistry.Enums;
using DFC.Api.AppRegistry.Models.Pages;
using DFC.Compui.Cosmos.Contracts;
using DFC.Swagger.Standard.Annotations;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DFC.Api.AppRegistry.Models
{
    [ExcludeFromCodeCoverage]
    public class AppRegistrationModel : DocumentModel, IValidatableObject
    {
        [Display(Description = "The path of the application. This should match the url value immediately after the domain. i.e. https://nationalcareeers.service.gov.uk/explore-careers.")]
        [Example(Description = "explore-careers")]
        [Required]
        [MaxLength(100)]
        public string? Path { get; set; }

        [Display(Description = "Internal storage partition key value. (same value as Path)")]
        [Example(Description = "explore-careers")]
        public override string? PartitionKey
        {
            get => Path;

            set => Path = value;
        }

        [Display(Description = "Text value that appears on the Top Navigation section of the National Careers Service website. If this value is NOT present no menu option will be displayed.")]
        [Example(Description = "Explore Careers")]
        public string? TopNavigationText { get; set; }

        [Display(Description = "Number indicating the position in the top navigation bar.  This value will have to agreed with Product owners and Service designers before deployment. Its suggested that numbers are allocated in increments of 100 or so.")]
        [Example(Description = "200")]
        public int TopNavigationOrder { get; set; }

        [Display(Description = "Location of CDN containing assets")]
        [Example(Description = "https://dev-cdn.nationalcareersservice.org.uk")]
        public string? CdnLocation { get; set; }

        [Display(Description = "Which page layout the application should us.")]
        [Example(Description = "FullWidth")]
        [Required]
        [EnumDataType(typeof(Layout))]
        public Layout Layout { get; set; }

        [Display(Description = "Indicator stating that the application is online and ready to use.")]
        [Example(Description = "true")]
        public bool IsOnline { get; set; }

        [Display(Description = "If the application is marked as offline (IsOnline = false) then this text is displayed on any application path (or child path).")]
        [Example(Description = "<strong>Sorry this application is offline</strong>")]
        public string? OfflineHtml { get; set; }

        [Display(Description = "This property is used to store the HTML markup for the 'Phase Banner' if any")]
        [Example(Description = "<strong>Alpha/Beta - This is a new service</strong>")]
        public string? PhaseBannerHtml { get; set; }

        [Display(Description = "Optional Url endpoint for the retrieval of an application sitemap.")]
        [Example(Description = "https://nationalcareeers.service.gov.uk/explore-careers/sitemap")]
        public Uri? SitemapURL { get; set; }

        [Display(Description = "External Url endpoint.")]
        [Example(Description = "https://nationalcareeers.service.gov.uk/explore-careers")]
        public Uri? ExternalURL { get; set; }

        [Display(Description = "Optional Url endpoint for the retrieval of an application Robots.txt file.")]
        [Example(Description = "https://nationalcareeers.service.gov.uk/explore-careers/robots.txt")]
        public Uri? RobotsURL { get; set; }

        [Display(Description = "UTC date and time the application was registered. This is auto generated.")]
        [Example(Description = "2019-05-01T13:32:00")]
        public DateTime? DateOfRegistration { get; set; }

        [Display(Description = "UTC date and time of when the application was last updated. This is auto generated.")]
        [Example(Description = "2019-05-01T13:32:00")]
        public DateTime? LastModifiedDate { get; set; }

        [Display(Description = "List of Regions registered to the application.")]
        public List<RegionModel>? Regions { get; set; }

        [Display(Description = "List of Ajax Requests registered to the application.")]
        public List<AjaxRequestModel>? AjaxRequests { get; set; }

        [Display(Description = "List of page location supported by the application.")]
        public Dictionary<Guid, PageLocationModel>? PageLocations { get; set; }

        [Display(Description = "List of JavaScripts required by the application.")]
        public Dictionary<string, string?>? JavaScriptNames { get; set; }

        [Display(Description = "List of CSS Scripts required by the application.")]
        public Dictionary<string, string?>? CssScriptNames { get; set; }

        [Display(Description = "Indicator stating that the application is interactive")]
        public bool IsInteractiveApp { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var result = new List<ValidationResult>();

            var pathRegex = new Regex(ValidationRegularExpressions.Path);

            if (!pathRegex.IsMatch(Path))
            {
                result.Add(new ValidationResult(ValidationMessages.PathIsInvalid, new string[] { nameof(Path) }));
            }

            if (!string.IsNullOrEmpty(OfflineHtml))
            {
                var htmlDoc = new HtmlDocument();

                htmlDoc.LoadHtml(OfflineHtml);

                if (htmlDoc.ParseErrors.Any())
                {
                    result.Add(new ValidationResult(string.Format(CultureInfo.InvariantCulture, ValidationMessages.MalformedHtml, nameof(OfflineHtml)), new string[] { nameof(OfflineHtml) }));
                }
            }

            if (!string.IsNullOrEmpty(PhaseBannerHtml))
            {
                var htmlDoc = new HtmlDocument();

                htmlDoc.LoadHtml(PhaseBannerHtml);

                if (htmlDoc.ParseErrors.Any())
                {
                    result.Add(new ValidationResult(string.Format(CultureInfo.InvariantCulture, ValidationMessages.MalformedHtml, nameof(PhaseBannerHtml))));
                }
            }

            if (Regions != null && Regions.Any())
            {
                foreach (var region in Regions)
                {
                    result.AddRange(region.Validate(new ValidationContext(region)));
                }
            }

            if (AjaxRequests != null && AjaxRequests.Any())
            {
                foreach (var ajaxRequests in AjaxRequests)
                {
                    result.AddRange(ajaxRequests.Validate(new ValidationContext(ajaxRequests)));
                }
            }

            return result;
        }
    }
}
