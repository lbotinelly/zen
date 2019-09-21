// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
namespace Zen.Web.Communication.Push
{
    public class EndpointEntry
    {
        public class Keys
        {
            public string p256dh { get; set; }
            public string auth { get; set; }
        }

        public string endpoint { get; set; }
        public Keys keys { get; set; }
    }
}
