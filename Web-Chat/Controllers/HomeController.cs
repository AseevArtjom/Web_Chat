using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using ServiceStack;
using System.Diagnostics;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Web_Chat.Models;
using static Web_Chat.Models.HomeViewModels;

namespace Web_Chat.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly DBContext _dbcontext;
        private readonly ICompositeViewEngine _viewEngine;

        public HomeController(DBContext dbcontext, ICompositeViewEngine viewEngine)
        {
            _dbcontext = dbcontext;
            _viewEngine = viewEngine;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _dbcontext.Users
                .Include(u => u.Chats)
                .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            var imageFile = await _dbcontext.ImagesFiles.FirstOrDefaultAsync(i => i.Id == user.ImageId);

            ViewData["CurrentUserImg"] = imageFile;

            var viewModel = new HomeViewModel
            {
                ChatsListView = new ChatsListViewModel
                {
                    Chats = user?.Chats.ToList() ?? new List<Chat>(),
                },
                ChatMainView = new ChatMainViewModel()
                {
                    CurrentUserImgSrc = imageFile?.ImgSrc ?? ""
                }
            };

            return View(viewModel);
        }

        public async Task<IActionResult> OpenChat(int chatId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _dbcontext.Users
                    .FirstOrDefaultAsync(u => u.Id.ToString() == userId); 

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                ViewData["CurrentUserImageSrc"] = string.IsNullOrEmpty(user.Image?.ImgSrc)
                    ? "path/to/default/image.png"
                    : user.Image.ImgSrc;

                var chat = await _dbcontext.Chats
                    .Include(m => m.Messages)
                    .ThenInclude(s => s.Sender)
                    .Where(m => m.Id == chatId)
                    .Select(m => new
                    {
                        Messages = m.Messages.Select(m => new MessageDto
                        {
                            Id = m.Id,
                            Text = m.Text,
                            SentAt = m.SentAt,
                            Sender = m.Sender,
                            SenderId = m.SenderId,
                            ChatId = chatId,
                            SenderImageUrl = m.Sender.Image.ImgSrc
                        }).ToList(),
                        ChatName = m.Name
                    })
                    .FirstOrDefaultAsync();

                if (chat == null)
                {
                    return NotFound("Chat not found.");
                }

                ChatMainViewModel model = new ChatMainViewModel
                {
                    Messages = chat.Messages,
                    ChatName = chat.ChatName,
                    ChatId = chatId,
                };
                var chatContent = await RenderViewToStringAsync("_ChatMainView", model);

                return Json(new { chatContent });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);
                if (viewResult.View == null)
                {
                    throw new FileNotFoundException("View not found.", viewName);
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }


        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] MessageDto messageDto)
        {
            if (messageDto == null || string.IsNullOrWhiteSpace(messageDto.Text))
            {
                return Json(new { ok = false });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _dbcontext.Users.FindAsync((int)userId.ToInt());

            var message = new Message
            {
                Text = messageDto.Text,
                SentAt = DateTime.UtcNow,
                SenderId = (int)userId.ToInt(),
                ChatId = messageDto.ChatId
            };

            _dbcontext.Messages.Add(message);
            await _dbcontext.SaveChangesAsync();

            var senderFullName = string.IsNullOrWhiteSpace(user?.LastName)
                ? user.FirstName
                : $"{user.FirstName} {user.LastName}";

            var senderImageUrl = (await _dbcontext.ImagesFiles.FirstOrDefaultAsync(i => i.Id == user.ImageId)).ImgSrc;

            return Ok(new
            {
                ok = true,
                message = new MessageDto
                {
                    Id = message.Id,
                    Text = message?.Text,
                    SentAt = message.SentAt,
                    SenderId = message.SenderId,
                    Sender = user,
                    SenderImageUrl = senderImageUrl,
                    ChatId = message.ChatId,
                }
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
