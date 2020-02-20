namespace Zen.App.Core.Application {
    public interface IPermission
    {
        string Id { get; set; }
        string Name { get; set; }
        string Code { get; set; }
        string FullCode { get; set; }
        string ApplicationId { get; set; }

    }
}