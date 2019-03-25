using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace randombg_dotnet{
    public static class SetBg{
        private static Database _db;
        private static string _DESTINATION =
            Path.Combine("/","home","erik","repos","randombg-dotnet","temp");

        public static void Run(Database db){
            _db = db;
            //Get an image
            ImageRecord selected = GetRandomImageRecord();
            //Download the image
            DownloadImage(selected);
            //Mark the image as downloaded
        }

        private static ImageRecord GetRandomImageRecord(){
            List<ImageRecord> images = _db.GetUnusedImages();
            ImageRecord result = 
                images[new System.Random().Next(images.Count)];
            Console.WriteLine($"Random image: {result.Url}");

            return result;
        }

        private static void DownloadImage(ImageRecord img){
            using (WebClient client = new WebClient()){
                client.DownloadFile(
                        img["Url"],
                        Path.Combine(_DESTINATION, img["Name"]));
            }

            Process link = new Process{
                StartInfo = new ProcessStartInfo {
                    FileName = "ln",
                    Arguments = String.Format("-sfn {0}{1} {0}{2}",
                            "/home/erik/repos/randombg-dotnet/temp/",
                            img["Name"],
                            "current"),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            link.Start();
            while (!link.StandardOutput.EndOfStream){
                string line = link.StandardOutput.ReadLine();
                Console.WriteLine(line);
            }

        }
    }

}
