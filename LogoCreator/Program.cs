using System;
using System.Text.Json;
using System.Text;
using System.IO;
using System.IO.Compression;
using LogoCreator.Services;

namespace MyProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string outputPath = @"..\..\..\..\PlainWallet\Resources\Logos\logos.json"; // Specify the output path for the generated logos
            string binoutputPath = @"..\..\..\..\PlainWallet\Resources\Logos\logos.bin"; // Specify the output path for the generated logos
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
                    //tmp.LogoData =  File.ReadAllBytes(logo);
                    var t1 = File.ReadAllText(logo);
                    // byte[] utf8Bytes = Encoding.UTF8.GetBytes(t1);

                    // // 2. Convert the UTF-8 bytes into ASCII bytes
                    // byte[] asciiBytes = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, utf8Bytes);

                    // // 3. Convert the ASCII bytes back into a C# string
                    // string asciiString = Encoding.ASCII.GetString(asciiBytes);

                    tmp.LogoSvg = t1;
                    tmp.IsSvg = true;
                }
                else
                {
                    tmp.LogoData = ImageService.ResizeImage(File.ReadAllBytes(logo)); ;
                }

                if (tmp.IsSvg)
                {
                    tmp.BackgroundColor = ImageService.GetRasterBackgroundColor(ImageService.RasterizeAndResizeSvg(Encoding.UTF8.GetBytes(tmp.LogoSvg)));
                }
                else
                {
                    tmp.BackgroundColor = ImageService.GetRasterBackgroundColor(tmp.LogoData);
                }

                logos.Add(tmp);

            }

            File.WriteAllText(outputPath, JsonSerializer.Serialize(logos, new JsonSerializerOptions { WriteIndented = true }));
            // File.WriteAllBytes(outputPath, JsonSerializer.SerializeToUtf8Bytes(logos, new JsonSerializerOptions { WriteIndented = false }));

            using var outputStream = new MemoryStream();
            // Use BrotliStream for maximum compression ratios
            using (var compressor = new BrotliStream(outputStream, CompressionLevel.Optimal))
            {
                JsonSerializer.Serialize(compressor, logos);
            }

            File.WriteAllBytes(binoutputPath, outputStream.ToArray());

        }
    }
}