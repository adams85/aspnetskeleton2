using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace WebApp.Api.Infrastructure.Swagger
{
    // https://github.com/domaindrivendev/Swashbuckle.AspNetCore#assign-actions-to-documents-by-convention
    public sealed class ApiExplorerGroupConvention : IControllerModelConvention
    {
        public const string DefaultGroupName = "Default";

        public void Apply(ControllerModel controller) => controller.ApiExplorer.GroupName = DefaultGroupName;
    }
}
