using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Deployment;
using OrchardCore.Json;

namespace OrchardCore.AdminMenu.Deployment
{
    public class AdminMenuDeploymentSource : IDeploymentSource
    {
        private readonly IAdminMenuService _adminMenuService;
        private readonly JsonSerializerOptions _serializationOptions;

        public AdminMenuDeploymentSource(IAdminMenuService adminMenuService, IOptions<JsonDerivedTypesOptions> derivedTypesOptions)
        {
            _adminMenuService = adminMenuService;

            // The recipe step contains polymorphic types which need to be resolved
            _serializationOptions = new()
            {
                TypeInfoResolver = new PolymorphicJsonTypeInfoResolver(derivedTypesOptions.Value)
            };
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var adminMenuStep = step as AdminMenuDeploymentStep;

            if (adminMenuStep == null)
            {
                return;
            }

            var data = new JsonArray();
            result.Steps.Add(new JsonObject
            {
                ["name"] = "AdminMenu",
                ["data"] = data,
            });

            foreach (var adminMenu in (await _adminMenuService.GetAdminMenuListAsync()).AdminMenu)
            {
                var objectData = JObject.FromObject(adminMenu, _serializationOptions);
                data.Add(objectData);
            }

            return;
        }
    }
}
