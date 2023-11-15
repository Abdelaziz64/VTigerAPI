using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class VTigerController : Controller
    {
        private readonly VTigerService _vtigerService;

        public VTigerController(VTigerService vtigerService)
        {
            _vtigerService = vtigerService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string accessKey)
        {
            // Validate input as needed

            await _vtigerService.LoginAsync(username, accessKey);

            // Redirect to a different page or return a view
            return RedirectToAction("Index", "Home");
        }
    }
}
