using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace WebApp.Api.Infrastructure.Swagger
{
    // https://github.com/domaindrivendev/Swashbuckle.AspNetCore#assign-actions-to-documents-by-convention
    public sealed class ApiExplorerGroupConvention : IControllerModelConvention
    {
        public static readonly string DefaultGroupName = "Default";

        public void Apply(ControllerModel controller) => controller.ApiExplorer.GroupName = DefaultGroupName;
    }
}
