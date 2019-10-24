namespace Zen.App.Communication
{
    public interface IZenEmailConfig
    {
        string SmtpServer { get; set; }
        string FallbackPersonEmail { get; set; }
        string GetBodyTemplate();
        string PostProcessContent(string content);
    }
}