using Host.Builder.Interfaces;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
            => CreateServerHostBuilder(args).Build().Run();

        static IHostBuilder CreateServerHostBuilder(string[] args)
            => Server.CreateDeafaultBuilder<Startup>(args);
    }
}
