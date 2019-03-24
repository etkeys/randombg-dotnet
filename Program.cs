using System;

namespace randombg_dotnet {
    public static class Program {
        private static string _command = null;

        public static void Main(string[] args) {
            Setup(args);
            switch (_command){
                case "SET":
                    throw new NotImplementedException();
                    break;
                case "UPDATEDB":
                    UpdateDb.Run();
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
                case "SET":
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
