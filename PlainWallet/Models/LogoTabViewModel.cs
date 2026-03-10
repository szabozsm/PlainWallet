using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlainWallet.Models
{
    public class LogoTabViewModel
    {
        public List<string> Logos { get; set; }
        public ImageSource UrlPreviewSource { get; set; }
        public ImageSource FilePreviewSource { get; set; }
        public string CurrentUrl { get; set; }
        public string CurrentUri { get; set; }

    }
}