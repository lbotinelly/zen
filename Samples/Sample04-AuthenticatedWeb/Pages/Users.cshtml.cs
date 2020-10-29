using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Zen.Web.Auth.Model;

namespace Sample04_AuthenticatedWeb.Pages
{
    public class UserModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public UserModel(ILogger<IndexModel> logger) => _logger = logger;
        public List<Identity> Items { get; set; }

        public void OnGet()
        {
            Items = Identity.All().ToList();
        }
    }
}