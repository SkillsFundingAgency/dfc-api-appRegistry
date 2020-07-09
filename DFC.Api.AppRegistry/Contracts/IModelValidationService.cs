using DFC.Api.AppRegistry.Models;

namespace DFC.Api.AppRegistry.Contracts
{
    public interface IModelValidationService
    {
        bool ValidateModel(AppRegistrationModel? appRegistrationModel);
    }
}