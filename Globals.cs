using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace randombg_dotnet{
    public static class Globals{

# if LINUX
        private static readonly string _OS_CONFIG_KEY = "linux";
        private static readonly string _USER_CONFIG_VAR =
            Path.Combine("%HOME%",".config");
        private static readonly string _USER_HOME_VAR = "%HOME%";
# elif WINDOWS
        private static readonly string _OS_CONFIG_KEY = "windows";
        private static readonly string _USER_CONFIG_VAR = "%APPDATA%";
        private static readonly string _USER_HOME_VAR = "%USERPROFILE%";
# endif
        private static string[] possibleConfigLocs = new string[] {
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            Environment.ExpandEnvironmentVariables(
                Path.Combine(_USER_CONFIG_VAR,"randombg-dotnet")),
            Environment.ExpandEnvironmentVariables(
                Path.Combine(_USER_HOME_VAR,"Pictures","Wallpapers","random")),
            Environment.ExpandEnvironmentVariables(
                Path.Combine(_USER_HOME_VAR,"Pictures","Wallpapers")),
            Environment.ExpandEnvironmentVariables(
                Path.Combine(_USER_HOME_VAR,"Pictures"))};

        public static readonly string WEB_SOURCE_BASE_URL = 
            @"http://www.mydailywallpaper.com";

        public static string ConfigFileName { get; } = "randombg-dotnet-config.json";
        public static string DbDir { get; private set; }
        public static bool KeepOldImages { get; private set; } = false;
        public static string PictureDir { get; private set; }
        public static IEnumerable<string> SourceImageCategories 
            {get; private set;}
        public static bool SymlinkToCurrent { get; private set; } = false;

        
        public static void LoadConfig(){
            string path = GetConfigPath();
            Console.WriteLine($"Found config file at {path}.");

            ReadConfig(path);
        }

        private static string GetConfigPath(){
            string result = String.Empty;

            for(int p = 0; 
                p < possibleConfigLocs.Length || 
                    String.IsNullOrWhiteSpace(result);
                p++){

                var aPath = Path.Combine(
                       possibleConfigLocs[p],
                       ConfigFileName);
                
                if (File.Exists(aPath))
                    result = aPath;

            }

            if (String.IsNullOrEmpty(result))
                throw new FileNotFoundException(
                        $"Could not file {ConfigFileName}.");

            return result;
        }

        private static void ReadConfig(string file){
            var config = JObject.Parse(
                    File.ReadAllText(file));

            SourceImageCategories = config.SelectTokensCoalesceArray(
                    new string[] {"source.categories"});

            DbDir = Environment.ExpandEnvironmentVariables(
                config.SSelectToken($"{_OS_CONFIG_KEY}.randomBgDbDir"));
            PictureDir = Environment.ExpandEnvironmentVariables(
                config.SSelectToken($"{_OS_CONFIG_KEY}.pictureDir"));
            
            if (config.HasKey($"{_OS_CONFIG_KEY}.keepOldImages"))
                KeepOldImages = Boolean.Parse(
                    config.SSelectToken($"{_OS_CONFIG_KEY}.keepOldImages"));

# if LINUX
            if (config.HasKey($"{_OS_CONFIG_KEY}.symlinkToCurrent"))
                SymlinkToCurrent = Boolean.Parse(
                    config.SSelectToken($"{_OS_CONFIG_KEY}.symlinkToCurrent"));
# endif

        }
    }

    public static class JsonExtensions{
        public static string FirstPath(this JObject json, string[] paths){
            string result = String.Empty;
            result = paths.FirstOrDefault(p => json.SelectTokens(p).Any());
            return result;
        }

        public static bool HasKey(this JObject json, string path){
            bool result = false;
            path = json.FirstPath(new string[] {path});

            result = !String.IsNullOrWhiteSpace(path);
            return result;
        }

        public static string SelectTokensCoalesce(
            this JObject json,
            string[] paths){
      
            string firstPath = json.FirstPath(paths);
            string result = string.Empty;

            if (!String.IsNullOrWhiteSpace(firstPath))
                result = json.SelectToken(firstPath).ToString();
     
            return result;
        }

        public static string[] SelectTokensCoalesceArray(
            this JObject json,
            string[] paths){
      
            string firstPath = json.FirstPath(paths);
            string[] result;

            if (!String.IsNullOrWhiteSpace(firstPath))
                result = json.SelectTokens(firstPath)
                    .First()
                    .Select(t => t.ToString())
                    .ToArray();
            else
                result = new string[]{};

            return result;      
        }

        public static string SSelectToken(
            this JObject json,
            string path){

            return json.SelectToken(path).ToString() ??
                String.Empty;
        }
    }

}
