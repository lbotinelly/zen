namespace Zen.Web.Auth.Provider {
    public abstract class NotificationPrimitive : INotificationPrimitive
    {
        public NotificationOptions options { get; set; }
        public string title { get; set; }
    }
}