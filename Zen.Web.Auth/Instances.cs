using Microsoft.AspNetCore.Authentication;
using Zen.Web.Auth.Configuration;

namespace Zen.Web.Auth
{
    public static class Instances
    {
        public static AuthenticationBuilder AuthenticationBuilder;
        public static Options Options;
    }
}