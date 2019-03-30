using System;

namespace randombg_dotnet {
    public static class Program {
        private static string _command = String.Empty;

        public static void Main(string[] args) {
            Setup(args);
            
            Database db = new Database();
            switch (_command){
                case "SETBG":
                    SetBg.Run(db);
                    break;
                case "UPDATEDB":
                    UpdateDb.Run(db);
                    break;
            }

            Console.WriteLine("Done.");
        }

        public static void Setup(string[] args){
            if (args.Length < 1){
                Console.WriteLine("Required arguments missing.");
                PrintUsage();
            }

            string cmd = args[0].ToUpper();
            switch (cmd){
                case "SETBG":
                case "UPDATEDB":
                    _command = cmd;
                    break;
                case "HELP":
                    PrintUsage();
                    break;
                default:
                    Console.WriteLine("Unknown command {0}.", cmd);
                    PrintUsage();
                    break;
            }

            Globals.LoadConfig();
        }

        private static void PrintUsage(){
            string message = @"
usage: COMMAND [options]

COMMAND

  HELP      Display this message

  SETBG     Download a random picture and set it as current wallpaper. The
            source for the image is taken from the local randombg.db database.

  UPDATEDB  Create the local randombg.db database, if it doesn't already exist,
            and add new image urls to the database from the source website.

OPTIONS
        TBD

";
            Console.WriteLine(message);
            Environment.Exit(1);
        }
    }
}
