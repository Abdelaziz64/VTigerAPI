using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

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

            // If login is successful, redirect to the contact creation page
            return RedirectToAction("CreateContact", new { username, accessKey });
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine($"Login failed: {ex.Message}");

            // Return a view indicating login failure
            return View("LoginFailed");
        }
    }

    // Action to create a contact

    public IActionResult CreateContact()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateContact(string username, string accessKey, string firstname, string lastname, string assignedUserId)
    {
        try
        {
            var sessionName = HttpContext.Session.GetString("SessionName");
            var vtigerVersion = HttpContext.Session.GetString("VtigerVersion");
            // Call the service to add a contact
            VTigerContact addedContact = await _vtigerService.AddContact(firstname, lastname, assignedUserId);
            if (addedContact != null)
            {
                // Redirect to a different page or return a view for successful contact addition
                return RedirectToAction("ContactAdded", "Home");
            }
            else
            {
                // Handle the case where contact addition failed
                Console.WriteLine("Failed to add contact.");
                return View("ContactAdditionFailed");
            }
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine($"Failed to add contact: {ex.Message}");
            return View("ContactAdditionFailed");
        }
    }
}
