using Host;
using Host.Builder;
using System;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
            => ProgramLoop(CreateServerHostBuilder(args).Build());

        static IHostBuilder CreateServerHostBuilder(string[] args)
            => Server.CreateDeafaultBuilder<Startup>(args);

        static void ProgramLoop(IHost host)
        {
            string command;
            do
            {
                Console.WriteLine("Enter command: [run/stop/exit]");
                command = Console.ReadLine().Trim();

                switch (command.ToLower())
                {
                    case "run":
                        host.Run();
                        break;
                    case "stop":
                        host.Stop();
                        break;
                    case "exit":
                        if (host.IsActive) host.Stop();
                        break;
                    default:
                        Console.WriteLine("Unrecognized command!");
                        break;
                }
            }
            while (!command.Equals("exit"));
        }
    }
}
