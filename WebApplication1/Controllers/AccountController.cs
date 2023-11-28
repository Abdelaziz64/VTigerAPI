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

            // If login is successful, redirect to ManageContacts
            return RedirectToAction("ManageContacts");
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine($"Login failed: {ex.Message}");

            // Return a view indicating login failure
            return View("LoginFailed");
        }
    }

    public IActionResult CreateContact()
    {
        return View();
    }



    public async Task<IActionResult> DisplayContacts()
    {
        try
        {
            // Call the service to retrieve all contacts
            List<VTigerContact> contacts = await _vtigerService.GetContacts();

            if (contacts != null)
            {
                // Pass the list of contacts to the view
                return View("DisplayContacts", contacts);
            }
            else
            {
                // Handle the case where contact retrieval failed
                Console.WriteLine("Failed to retrieve contacts.");
                return View("ContactRetrievalFailed");
            }
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine("Failed to retrieve contacts.");
            return View("ContactRetrievalFailed");
        }
    }

    public async Task<IActionResult> ManageContacts()
    {
        try
        {
            // Retrieve current contacts
            List<VTigerContact> contacts = await _vtigerService.GetContacts();

            // Pass the list of contacts to the view
            return View("ManageContacts", contacts);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine($"Failed to retrieve or display contacts: {ex.Message}");
            return View("ContactOperationFailed");
        }
    }

    [HttpPost]
    public async Task<IActionResult> ManageContacts(string username, string accessKey, string firstname, string lastname, string assignedUserId)
    {
        try
        {
            // Call the service to add a contact
            VTigerContact addedContact = await _vtigerService.AddContact(firstname, lastname, assignedUserId);

            if (addedContact != null)
            {
                // Redirect to the ManageContacts view
                return RedirectToAction("ManageContacts");
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
            Console.WriteLine($"Failed to add or display contacts: {ex.Message}");
            return View("ContactOperationFailed");
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteContact(string contactId)
    {
        try
        {
            bool deleteResult = await _vtigerService.DeleteContactAsync(contactId);

            if (deleteResult)
            {
                // Redirect to the ManageContacts view after successful deletion
                return RedirectToAction("ManageContacts");
            }
            else
            {
                // Handle the case where contact deletion failed
                Console.WriteLine($"Failed to delete contact with ID {contactId}.");
                return View("ContactDeletionFailed");
            }
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine($"Failed to delete contact: {ex.Message}");
            return View("ContactDeletionFailed");
        }
    }

}
