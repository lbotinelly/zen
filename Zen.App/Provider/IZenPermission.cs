namespace Zen.App.Provider
{
    public interface IZenPermission
    {
        string Id { get; set; }
        string Name { get; set; }
        string Code { get; set; }
        string FullCode { get; set; }
        string ApplicationId { get; set; }

    }
}