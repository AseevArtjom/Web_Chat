
namespace Web_Chat.Models
{
    public class HomeViewModels
    {
        public class HomeViewModel
        {
            public ChatMainViewModel ChatMainView { get; set; }
            public ChatsListViewModel ChatsListView { get; set; }
        }

        public class ChatsListViewModel
        {
            public List<Chat> Chats = new List<Chat>();
        }

        public class ChatMainViewModel
        {
            public int ChatId { get; set; }
            public string ChatName { get; set; }
            public List<MessageDto> Messages { get; set; } = new List<MessageDto>();
            public string CurrentUserImgSrc { get; set; }
        }
    }
}
