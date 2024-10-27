using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace factoryos_10x_shell.Services.Helpers
{
    public class AppState
    {
        private static AppState _instance;
        public static AppState Instance => _instance ?? (_instance = new AppState());

        private bool _isSearchButtonVisible = true;
        public bool IsSearchButtonVisible
        {
            get => _isSearchButtonVisible;
            set
            {
                _isSearchButtonVisible = value;
                OnSearchButtonVisibilityChanged?.Invoke(value);
            }
        }

        private bool _isCopilotButtonVisible = true;
        public bool IsCopilotButtonVisible
        {
            get => _isCopilotButtonVisible;
            set
            {
                _isCopilotButtonVisible = value;
                OnCopilotButtonVisibilityChanged?.Invoke(value);
            }
        }


        private bool _isBgChangeButtonVisible;
        public bool IsBgChangeButtonVisible
        {
            get => _isBgChangeButtonVisible;
            set
            {
                if (_isBgChangeButtonVisible != value)
                {
                    _isBgChangeButtonVisible = value;
                    OnBgChangeButtonVisibilityChanged?.Invoke(_isBgChangeButtonVisible);
                }
            }
        }


        public event Action<bool> OnBgChangeButtonVisibilityChanged;

        public event Action<bool> OnSearchButtonVisibilityChanged;
        public event Action<bool> OnCopilotButtonVisibilityChanged;
    }
}
