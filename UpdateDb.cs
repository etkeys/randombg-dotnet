using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace randombg_dotnet{

    public static class UpdateDb{
        private static readonly List<string> _CATEGORIES = 
            new List<string>{
                "abstract",
                "fantasy",
                "landscapescities",
                "landscapesnature",
                "space"};
        private static readonly string _URL_BASE = "http://www.mydailywallpaper.com";

        public static void Run(){
            List<ImageRecord> recs = GetWebBasedRecords();

            Database db = new Database();
            db.Load();
            db.InsertNewRecords("images", recs.Cast<IDbRecord>());
            db.Save();
        }  

        private static List<ImageRecord> GetWebBasedRecords(){
            List<ImageRecord> result = new List<ImageRecord>();
            foreach(string cat in _CATEGORIES)
                result.AddRange(GetCategoryImageRecords(cat)); 

            return result;
        }


        private static IEnumerable<ImageRecord> GetCategoryImageRecords(string cat){
            LinkedList<ImageRecord> result = new LinkedList<ImageRecord>();

            int pagenum = 1;
            bool keepGoing = true;
            do{
                try{
                    string pageUrl = GetPageUrl(cat, pagenum); 
                    IEnumerable<string> imgPaths = GetPageImagePaths(pageUrl);

                    foreach(string path in imgPaths)
                        result.AddLast(CreateImageRecord(cat, path));

                    pagenum++;
                } catch (Exception ex){
                    keepGoing = false;
                }
            }while(keepGoing);

            return result;
        }

        private static string GetPageUrl(string cat, int pagenum)
        {
            string result =
                $"{_URL_BASE}/wallcat/{cat}/?";

            result = (pagenum > 1) ?
                result.Replace("?", $"index{pagenum}.html"):
                result.Replace("?", "index.html");
                
            return result;
        }

        private static IEnumerable<string> GetPageImagePaths(string url){
            LinkedList<string> result = new LinkedList<string>();

            var web = new HtmlWeb();
            var body = web.Load(url).DocumentNode.SelectSingleNode("//body");
            var imgnodes = body.SelectNodes("//div/table/tr/td/table/tr/td/a");

            foreach(var node in imgnodes){
                var img = node.Elements("img").FirstOrDefault();

                if (img != null){
                    var tmbPath = img.Attributes["src"].Value;
                    var imgPath = tmbPath
                        .Replace("tmb/","img/")
                        .Replace("/tmb","/img");
    
                    result.AddLast(imgPath);
                }
            }   
            
            return result;
        }

        private static ImageRecord CreateImageRecord(string cat, string path){
            string fullUrl = $"{_URL_BASE}{path}";
            string imgName = path.Substring(path.IndexOf('_') + 1);

            return new ImageRecord(false, imgName, fullUrl);
        }
    }
}
