using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using Web_Chat.Models;
using Web_Chat.Models.Form;
using Web_Chat.Services;

namespace Web_Chat.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly LocalUploadedFileStorage _localUploadedFileStorage;
        private readonly DBContext _dbContext;
        public ProfileController(UserManager<User> userManager, LocalUploadedFileStorage localUploadedFileStorage,DBContext dBContext)
        {
            _userManager = userManager;
            _localUploadedFileStorage = localUploadedFileStorage;
            _dbContext = dBContext;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _dbContext.Users.Include(i => i.Image).FirstOrDefaultAsync(x=>x.Id.ToString() == userId);

            if (user != null)
            {
                ViewData["FirstName"] = user.FirstName;
                ViewData["LastName"] = user.LastName;
                ViewData["Email"] = user.Email;
                ViewData["PhoneNumber"] = user.PhoneNumber;
                ViewData["Location"] = user.Location;
                ViewData["Birthday"] = user.Birthday;
                ViewData["Bio"] = user.Bio;
                ViewData["ProfileImageSrc"] = user.Image?.ImgSrc ?? "";
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] EditProfileForm form)
        {
            if (string.IsNullOrWhiteSpace(form.FirstName))
            {
                return Json(new { ok = false,name = "FirstName", error = "First name is required" });
            }
            if (string.IsNullOrWhiteSpace(form.Email))
            {
                return Json(new { ok = false,name = "Email", error = "Email is required" });
            }
            if (form.LastName?.Length > 50)
            {
                return Json(new { ok = false, name = "LastName", error = "Last is must not exceed 50 characters" });
            }
            if (form.Bio?.Length > 350)
            {
                return Json(new { ok = false, name = "Bio" ,error = "Bio must not exceed 350 characters" });
            }
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Json(new { ok = false });
            }
            user.FirstName = form.FirstName;
            user.Email = form.Email;
            user.LastName = form.LastName ?? user.LastName;
            user.PhoneNumber = form.Phone ?? user.PhoneNumber;
            user.Location = form.Location ?? user.Location;
            user.Birthday = form.BirthDay;

            user.Bio = form.Bio ?? user.Bio;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Json(new { ok = true });
            }

            return Json(new { ok = false });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfileImage(IFormFile ProfileImage)
        {
            if (ProfileImage != null)
            {
                if (ProfileImage.Length == 0)
                {
                    return Json(new { ok = false, errormessage = "image is null" });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var user = await _dbContext.Users.Include(i => i.Image).FirstOrDefaultAsync(x => x.Id.ToString() == userId);

                if (user == null)
                {
                    return Json(new { ok = false, errormessage = "User not found." });
                }

                try
                {
                    if (user.Image != null)
                    {
                        _localUploadedFileStorage.DeleteUploadedFile(user.Image);

                        _dbContext.ImagesFiles.Remove(user.Image);
                        user.Image = null;
                        await _dbContext.SaveChangesAsync();
                    }

                    var savedImage = await _localUploadedFileStorage.SaveUploadedFileAsync(ProfileImage);
                    _dbContext.ImagesFiles.Add(savedImage);
                    await _dbContext.SaveChangesAsync();
                    user.Image = savedImage;

                    var res = await _userManager.UpdateAsync(user);

                    if (res.Succeeded)
                    {
                        return Json(new { ok = true, imagePath = savedImage.ImgSrc });
                    }

                    return Json(new { ok = false, errormessage = "Failed to update user with new image." });
                }
                catch (Exception ex)
                {
                    return Json(new { ok = false, errormessage = ex.Message });
                }
            }

            return Json(new { ok = false, errormessage = "image != null false" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProfileImage()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _dbContext.Users.Include(u => u.Image).FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null)
            {
                return Json(new { ok = false, errormessage = "User not found." });
            }

            try
            {
                if (user.Image != null)
                {
                    _localUploadedFileStorage.DeleteUploadedFile(user.Image);

                    _dbContext.ImagesFiles.Remove(user.Image);
                    user.Image = null;
                    await _dbContext.SaveChangesAsync();
                }

                return Json(new { ok = true, message = "Profile image deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, errormessage = ex.Message });
            }
        }



    }
}
