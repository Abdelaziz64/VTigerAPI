using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApplication1.Models;
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

                // If login is successful, add a contact
                string firstname = "Abdelaziz"; // Replace with actual first name
                string lastname = "Amine";   // Replace with actual last name
                string assignedUserId = "2"; // Get the user ID from the service

                // Add contact
                VTigerContact addedContact = await _vtigerService.AddContact(firstname, lastname, assignedUserId);
                if (addedContact != null)
                {
                    // Redirect to a different page or return a view for successful contact addition
                    return RedirectToAction("ContactAdded", "Home");
                }
                else
                {
                    // Handle the case where contact addition failed
                    Console.WriteLine("Failed to add contact after login.");
                    return View("ContactAdditionFailed");
                }

                // Successful login
                // Redirect to a different page or return a view for successful login
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
