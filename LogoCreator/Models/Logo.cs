using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace LogoCreator.Models
{
    public class Logo
    {
        public string Name { get; set; }
         public byte[] LogoData { get; set; }
         public string LogoSvg { get; set; }
         public string BackgroundColor { get; set; }
         public bool IsSvg {get; set; }= false;
    }
}