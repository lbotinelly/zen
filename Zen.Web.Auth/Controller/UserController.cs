using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Zen.Base.Module.Identity.Collections;
using Zen.Base.Module.Identity.Model;

namespace Zen.Web.Auth.Controller
{
    [Route("api/auth/[controller]/[action]"), ApiController]
    public class UserController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly IIdentityUserCollection<User> _userUserCollection;

        public UserController(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IIdentityUserCollection<User> userCollection)
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

            if (!await _roleManager.RoleExistsAsync(roleName)) await _roleManager.CreateAsync(new Role(roleName));

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
        public async Task<ActionResult> Edit(User user)
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