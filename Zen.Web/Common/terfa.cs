namespace Zen.Web.Common
{
    public interface IZenWebCardRender
    {
        ZenWebCardDetails GetCardDetails(string path, string queryString);
    }

    public class ZenWebCardDetails
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string TwitterCreatorUser { get; set; }
        public string TwitterSiteUser { get; set; }
    }
}
