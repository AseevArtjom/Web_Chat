using Microsoft.JSInterop;

namespace Web_Chat.Models
{
    public class AppTheme
    {
        private readonly IJSRuntime js;

        public AppTheme(IJSRuntime js)
        {
            this.js = js;
        }

        private bool isDarkMode = false;

        public bool IsDarkMode
        {
            get => isDarkMode;
            set {
                isDarkMode = value;
                js.InvokeVoidAsync("setDarkMode",value);
            }
        }
    }
}
