using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Contract.Settings
{
    [DataContract]
    public class UpdateSettingCommand : ICommand
    {
        [Required]
        [DataMember(Order = 1)] public string Name { get; set; } = null!;

        [DataMember(Order = 2)] public string? Value { get; set; }
    }
}
