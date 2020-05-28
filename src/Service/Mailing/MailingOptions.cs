namespace WebApp.Service.Mailing
{
    public class MailingOptions
    {
        public static readonly string DefaultSectionName = "Mailing";

        public string NoReplyMailFrom { get; set; } = null!;
    }
}
