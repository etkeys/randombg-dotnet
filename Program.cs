using System;

namespace randombg_dotnet {
    public static class Program {
        private static string _command = null;

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
                default:
                    Console.WriteLine("Unknown command {0}.", cmd);
                    PrintUsage();
                    break;
            }
        }

        private static void PrintUsage(){
            Environment.Exit(1);
        }
    }
}
