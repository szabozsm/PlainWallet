using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PlainWallet.Models
{
    public class LogoTabViewModel : INotifyPropertyChanged
    {
        private List<string> _logos;
        private ImageSource _urlPreviewSource;
        private ImageSource _filePreviewSource;
        private string _currentUrl;
        private string _currentUri;
        private bool _isUrlLoading;

        public List<string> Logos 
        { 
            get => _logos; 
            set { _logos = value; OnPropertyChanged(); }
        }

        public ImageSource UrlPreviewSource 
        { 
            get => _urlPreviewSource; 
            set { _urlPreviewSource = value; OnPropertyChanged(); }
        }

        public ImageSource FilePreviewSource 
        { 
            get => _filePreviewSource; 
            set { _filePreviewSource = value; OnPropertyChanged(); }
        }

        public string CurrentUrl 
        { 
            get => _currentUrl; 
            set { _currentUrl = value; OnPropertyChanged(); }
        }

        public string CurrentUri 
        { 
            get => _currentUri; 
            set { _currentUri = value; OnPropertyChanged(); }
        }

        public bool IsUrlLoading 
        { 
            get => _isUrlLoading; 
            set { _isUrlLoading = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}