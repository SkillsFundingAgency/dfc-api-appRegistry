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
            Assert.True(vr.Count == 0);
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
            Assert.True(vr.Count == 2);
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
            Assert.True(vr.Count == 1);
            Assert.NotNull(vr.First(f => f.MemberNames.Any(a => a == nameof(model.Path))));
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, "{0} is invalid", nameof(model.Path)), vr.First(f => f.MemberNames.Any(a => a == nameof(model.Path))).ErrorMessage);
        }

        private AppRegistrationModel CreateValidModel(string pathValue)
        {
            return new AppRegistrationModel
            {
                Id = System.Guid.NewGuid(),
                PartitionKey = pathValue,
                Layout = Layout.FullWidth,
            };
        }

        private List<ValidationResult> Validate(AppRegistrationModel model)
        {
            var vr = new List<ValidationResult>();
            var vc = new ValidationContext(model);
            Validator.TryValidateObject(model, vc, vr, true);

            return vr;
        }
    }
}
