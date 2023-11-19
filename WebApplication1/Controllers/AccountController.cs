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
            try
            {


                await _vtigerService.LoginAsync(username, accessKey);

                // Successful login
                // Redirect to a different page or return a view for successful login
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"Login failed: {ex.Message}");

                // Return a view indicating login failure
                return View("LoginFailed");
            }
        }


    }
}
