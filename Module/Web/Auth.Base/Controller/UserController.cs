using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Zen.Base.Module.Identity.Collections;
using Zen.Base.Module.Identity.Model;
using Zen.Module.Web.REST;

namespace Zen.Module.Web.Auth.Base.Controller
{
    [Route("api/auth/[controller]/[action]"), ApiController]
    public class UserController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly RoleManager<ZenRole> _roleManager;
        private readonly UserManager<ZenUser> _userManager;
        private readonly IIdentityUserCollection<ZenUser> _userUserCollection;

        public UserController(
            UserManager<ZenUser> userManager,
            RoleManager<ZenRole> roleManager,
            IIdentityUserCollection<ZenUser> userCollection)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _userUserCollection = userCollection;
        }

        [Route("")]
        public IActionResult Index() { return Response.FromObject(_userManager.Users); }

        public async Task<IActionResult> Current()
        {
            var user = await _userManager.GetUserAsync(User);

            return Response.FromObject(user);
        }


        public async Task<IActionResult> AddToRole(string roleName, string userName)
        {
            var u = await _userManager.FindByNameAsync(userName);

            if (!await _roleManager.RoleExistsAsync(roleName)) await _roleManager.CreateAsync(new ZenRole(roleName));

            if (u == null) return NotFound();

            await _userManager.AddToRoleAsync(u, roleName);

            return Response.Success();
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByNameAsync(id);

            if (user == null) return NotFound();

            return Response.FromObject(user);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ZenUser user)
        {
            await _userUserCollection.UpdateAsync(user);
            return Redirect("/user");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            var user = await _userUserCollection.FindByIdAsync(id);
            await _userUserCollection.DeleteAsync(user);
            return Redirect("/user");
        }
    }
}