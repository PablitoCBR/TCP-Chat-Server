using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClientDemo
{
    class Program
    {
        private static ConsoleClient _client;
        static void Main(string[] args)
        {
            ConsoleKeyInfo option;

            do
            {
                Console.Write("Username: ");
                string username = Console.ReadLine();
                Console.Write("Password: ");
                string password = Console.ReadLine();
                _client = new ConsoleClient(username, password);
                Console.Clear();

                Console.WriteLine("USER: {0}", _client.Username);
                Console.WriteLine();
                Console.WriteLine("======== MENU ========");
                Console.WriteLine("1. Login");
                Console.WriteLine("2. Register");
                Console.WriteLine("======================");
                option = Console.ReadKey();

                Console.Clear();

                switch (option.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        _client.Login();
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        _client.Register();
                        break;
                }

                Console.WriteLine();
            }
            while (_client.LoggedIn == false);

            Console.Clear();

            do
            {
                Console.WriteLine("USER: {0}", _client.Username);
                Console.WriteLine();
                Console.WriteLine("======== MENU ========");
                Console.WriteLine("1. Send message");
                Console.WriteLine("2. Exit");
                Console.WriteLine("======================");
                option = Console.ReadKey();

                switch (option.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        SendMessage();
                        break;
                }
            }
            while ((option.Key == ConsoleKey.D2 || option.Key == ConsoleKey.NumPad2) == false);
        }

        private static void SendMessage()
        {
            Console.Clear();
            Console.WriteLine("======== MESSAGE SENDING ========");
            Console.Write("Recipient: ");
            string recipient = Console.ReadLine();
            Console.Write("Message: ");
            string message = Console.ReadLine();

            _client.SendMessage(recipient, message);
            Console.WriteLine("==================================");
        }
    }
}
