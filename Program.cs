using System;

namespace consoleproject
{
    class Program
    {
        static void Main(string[] args)
        {
			Server server = Server.Instance;
			server.Execute();
        }
    }
}
