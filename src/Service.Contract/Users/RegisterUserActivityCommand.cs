using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Users
{
    [DataContract]
    public class RegisterUserActivityCommand : ICommand
    {
        [Required]
        [DataMember(Order = 1)] public string UserName { get; set; } = null!;

        [DataMember(Order = 2)] public bool? SuccessfulLogin { get; set; }

        [DataMember(Order = 3)] public bool UIActivity { get; set; }
    }
}
