namespace Zen.App.Communication
{
    public interface IEmailConfig
    {
        string SmtpServer { get; set; }
        string FallbackPersonEmail { get; set; }
        string GetBodyTemplate();
        string PostProcessContent(string content);
    }
}