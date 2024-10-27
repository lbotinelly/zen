namespace Zen.App.Model.Communication {
    public interface IAppScopedContent : IScopedContent
    {
        string ApplicationCode { get; set; }
    }
}