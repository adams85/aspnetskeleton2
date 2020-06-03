using WebApp.Service.Contract.Settings;

namespace WebApp.Api.Helpers
{
    public static class DtoMappingHelper
    {
        public static UpdateSettingCommand ToUpdateCommand(this SettingData data) => new UpdateSettingCommand()
        {
            Name = data.Name,
            Value = data.Value
        };
    }
}
