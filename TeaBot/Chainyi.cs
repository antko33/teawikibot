using System;
using System.Collections.Generic;
using DotNetWikiBot;

namespace ChainyiBot
{
    class Chainyi : Bot
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static void Main()
        {
            FailureReporter.CleanOldCases();
            List<IHandler> handlers = new List<IHandler>
            {
                new WikiLinksHandler()
            };
            handlers.ForEach(handler =>
            {
                for (int i = 0; i < 10; i++)
                {
                    handler.Run();
                    System.Threading.Thread.Sleep(60 * 1000);
                }
            });
            Console.ReadKey();
        }
    }
}
