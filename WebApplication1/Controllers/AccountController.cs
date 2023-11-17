using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class VTigerController : Controller
    {
        private readonly VTigerService _vtigerService;
        private string _sessionId;

        public string GetSessionId()
        {
            return _sessionId;
        }

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
                _sessionId = null; // Reset session ID

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

        public async Task<IActionResult> Logout()
        {
            try
            {
                if (!string.IsNullOrEmpty(_sessionId))
                {
                    await _vtigerService.LogoutAsync();

                    // Reset session ID after logout
                    _sessionId = null;

                    // Redirect to a different page or return a view after logout
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Handle the case where the session ID is not available
                    return View("LogoutFailed");
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"Logout failed: {ex.Message}");

                // Return a view indicating logout failure
                return View("LogoutFailed");
            }
        }
    }
}
