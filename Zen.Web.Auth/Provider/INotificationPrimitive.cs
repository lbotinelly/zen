namespace Zen.Web.Auth.Provider {
    public interface INotificationPrimitive
    {
        NotificationOptions options { get; set; }
        string title { get; set; }
    }
}