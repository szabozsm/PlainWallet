using System;
using System.Text.Json;
using LogoCreator.Services;

namespace MyProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string outputPath = @".\logos.json"; // Specify the output path for the generated logos
            string logoPath = @"..\..\..\..\logos"; // Specify the log file path

            var logofiles = Directory.GetFiles(logoPath, "*.*");

            var logos = new List<LogoCreator.Models.Logo>();

            foreach (var logo in logofiles)
            {
                Console.WriteLine($"Logo: {Path.GetFileName(logo)}");

                var tmp = new LogoCreator.Models.Logo
                {

                    Name = Path.GetFileNameWithoutExtension(logo),
                };

                if (Path.GetExtension(logo).ToLower() == ".svg")
                {
                    tmp.LogoData =  File.ReadAllBytes(logo);
                    tmp.IsSvg = true;
                }
                else
                {
                    tmp.LogoData = ImageService.ResizeImage(File.ReadAllBytes(logo));;
                }
                tmp.BackgroundColor = ImageService.GetRasterBackgroundColor(tmp.LogoData);

                logos.Add(tmp);

            }

            File.WriteAllText(outputPath, JsonSerializer.Serialize(logos, new JsonSerializerOptions { WriteIndented = true }));
            
        }
    }
}