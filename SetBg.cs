using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace randombg_dotnet{
    public static class SetBg{
        private static Database _db;

        public static void Run(Database db){
            _db = db;
            ImageRecord selected = GetRandomImageRecord();
            CleanDestination();
            DownloadImage(selected, Globals.SymlinkToCurrent);
            _db.MarkImageDownloaded(selected);
            _db.Save();
        }

        private static ImageRecord GetRandomImageRecord(){
            List<ImageRecord> images = _db.GetUnusedImages();
            ImageRecord result = 
                images[new System.Random().Next(images.Count)];
            Console.WriteLine($"Random image: {result.Url}");

            return result;
        }

        private static void CleanDestination(){
            if (!Globals.KeepOldImages)
                foreach(string file in Directory.GetFiles(Globals.PictureDir))
                    if (!file.EndsWith(_db.DbName) && 
                        !file.EndsWith(Globals.ConfigFileName))
                        File.Delete(file);
        }


        private static void DownloadImage(ImageRecord img, bool createSymlink){
            string filename = createSymlink ?
                img["Name"] :
                $"{DateTime.Now.ToString("yyMMddHHmmss")}.jpg";

            if (!Directory.Exists(Globals.PictureDir))
                Directory.CreateDirectory(Globals.PictureDir);

            Console.WriteLine("Downlaoding image...");
            using (WebClient client = new WebClient()){
                client.DownloadFile(
                        img["Url"],
                        Path.Combine(Globals.PictureDir, filename));
            }

            if (createSymlink)
                CreateSymlink(img);
        }

        private static void CreateSymlink(ImageRecord img){
            Process link = new Process{
                StartInfo = new ProcessStartInfo {
# if LINUX
                    FileName = "ln",
                    Arguments = String.Format("-sfn {0} {1}",
                            Path.Combine(
                                Globals.PictureDir,
                                img["Name"]),
                            Path.Combine(
                                Globals.PictureDir,
                                "current")),
# else
                    FileName = "echo",
                    Arguments = 
                        "Symlinking not supported for this platform. " +
                        "Skipping.",
# endif
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
