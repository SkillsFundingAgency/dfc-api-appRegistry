using DFC.Api.AppRegistry.Enums;
using DFC.Api.AppRegistry.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.ValidationTests
{
    [Trait("Category", "AppRegistrationModel - Validation tests")]
    public class AppRegistrationModelValidationTests
    {
        [Theory]
        [InlineData("ab")]
        [InlineData("abc")]
        [InlineData("123")]
        [InlineData("a.b-c_d,e_f/g")]
        [InlineData("abc/def")]
        [InlineData("action-plans")]
        [InlineData("your-account")]
        [InlineData("job-profiles")]
        [InlineData("explore-careers")]
        [InlineData("skills-assessment")]
        [InlineData("find-a-course")]
        [InlineData("contact-us")]
        [InlineData("webchat")]
        [InlineData("about-us")]
        [InlineData("get-a-job")]
        [InlineData("help")]
        [InlineData("alerts")]
        [InlineData("discover-your-skills-and-careers")]
        [InlineData("matchskills")]
        [InlineData("pages")]
        [InlineData("skills-assessment/skills-health-check")]
        [InlineData("skills-assessment/match-skills")]
        [InlineData("skills-assessment/discover-your-skills-and-careers")]
        public void AppRegistrationModelValidationSuccessful(string pathValue)
        {
            // Arrange
            var model = CreateValidModel(pathValue);

            // Act
            var vr = Validate(model);

            // Assert
            Assert.Empty(vr);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AppRegistrationModelValidationMandatoryFailure(string pathValue)
        {
            // Arrange
            var model = CreateValidModel(pathValue);

            // Act
            var vr = Validate(model);

            // Assert
            Assert.Equal(2, vr.Count);
            Assert.NotNull(vr.First(f => f.MemberNames.Any(a => a == nameof(model.Path))));
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, "The {0} field is required.", nameof(model.Path)), vr.First(f => f.MemberNames.Any(a => a == nameof(model.Path))).ErrorMessage);
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/a")]
        [InlineData("a/")]
        [InlineData("a+b")]
        [InlineData("a(b")]
        [InlineData("a)b")]
        [InlineData("_a")]
        [InlineData("a_")]
        public void AppRegistrationModelValidationFailure(string pathValue)
        {
            // Arrange
            var model = CreateValidModel(pathValue);

            // Act
            var vr = Validate(model);

            // Assert
            Assert.Single(vr);
            Assert.NotNull(vr.First(f => f.MemberNames.Any(a => a == nameof(model.Path))));
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, "{0} is invalid", nameof(model.Path)), vr.First(f => f.MemberNames.Any(a => a == nameof(model.Path))).ErrorMessage);
        }

        [Fact]
        public void RegionModelValidationSuccessful()
        {
            // Arrange
            var model = CreateValidModel("a-valid-path");

            // Act
            var vr = Validate(model);

            // Assert
            Assert.Empty(vr);
        }

        [Fact]
        public void RegionModelValidationMissingMandatoryValues()
        {
            // Arrange
            var model = CreateValidModel("a-valid-path");
            var regionModel = model.Regions.First();
            regionModel.RegionEndpoint = null;

            // Act
            var vr = Validate(regionModel);

            // Assert
            Assert.Single(vr);
            Assert.NotNull(vr.First(f => f.MemberNames.Any(a => a == nameof(RegionModel.RegionEndpoint))));
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, "The {0} field is required.", nameof(RegionModel.RegionEndpoint)), vr.First(f => f.MemberNames.Any(a => a == nameof(RegionModel.RegionEndpoint))).ErrorMessage);
        }

        [Theory]
        [InlineData("ab")]
        [InlineData("abc")]
        [InlineData("123")]
        [InlineData("a.b-c_d,e_f/g")]
        [InlineData("SortBy")]
        public void AjaxRequestModelValidationSuccessful(string nameValue)
        {
            // Arrange
            var model = CreateValidModel("a-valid-path");
            var ajaxRequestModel = model.AjaxRequests.First();
            ajaxRequestModel.Name = nameValue;

            // Act
            var vr = Validate(model);

            // Assert
            Assert.Empty(vr);
        }

        [Fact]
        public void AjaxRequestModelValidationMissingMandatoryValues()
        {
            // Arrange
            var model = CreateValidModel("a-valid-path");
            var ajaxRequestModel = model.AjaxRequests.First();
            ajaxRequestModel.Name = null;
            ajaxRequestModel.AjaxEndpoint = null;

            // Act
            var vr = Validate(ajaxRequestModel);

            // Assert
            Assert.Equal(2, vr.Count);
            Assert.NotNull(vr.First(f => f.MemberNames.Any(a => a == nameof(AjaxRequestModel.Name))));
            Assert.NotNull(vr.First(f => f.MemberNames.Any(a => a == nameof(AjaxRequestModel.AjaxEndpoint))));
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, "The {0} field is required.", nameof(AjaxRequestModel.Name)), vr.First(f => f.MemberNames.Any(a => a == nameof(AjaxRequestModel.Name))).ErrorMessage);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, "The {0} field is required.", nameof(AjaxRequestModel.AjaxEndpoint)), vr.First(f => f.MemberNames.Any(a => a == nameof(AjaxRequestModel.AjaxEndpoint))).ErrorMessage);
        }

        private AppRegistrationModel CreateValidModel(string pathValue)
        {
            return new AppRegistrationModel
            {
                Id = System.Guid.NewGuid(),
                PartitionKey = pathValue,
                Layout = Layout.FullWidth,
                Regions = new List<RegionModel>
                {
                    new RegionModel
                    {
                        PageRegion= PageRegion.Body,
                        RegionEndpoint = "https://somewhere.com",
                    },
                },
                AjaxRequests = new List<AjaxRequestModel>
                {
                    new AjaxRequestModel
                    {
                        Name = "a-name",
                        AjaxEndpoint = "https://somewhere.com",
                    },
                },
            };
        }

        private List<ValidationResult> Validate<TModel>(TModel model)
        {
            var vr = new List<ValidationResult>();
            var vc = new ValidationContext(model);
            Validator.TryValidateObject(model, vc, vr, true);

            return vr;
        }
    }
}
