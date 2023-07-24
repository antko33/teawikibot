using System;
using System.Collections.Generic;
using DotNetWikiBot;

namespace ChainyiBot
{
    class Chainyi : Bot
    {
#if TEST
        private const int IntervalInMinutes = 1;
#endif

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static void Main()
        {
            FailureReporter.CleanOldCases();
            List<IHandler> handlers = new List<IHandler>
            {
#if !DEBUG
                new WikiLinksHandler(),
#endif
                new InvalidRefHandler()
            };
#if TEST
            handlers.ForEach(handler =>
            {
                for (int i = 0; i < 10; i++)
                {
                    handler.Run();
                    System.Threading.Thread.Sleep(IntervalInMinutes * 60 * 1000);
                }
            });
#else
            handlers.ForEach(handler => handler.Run());
            Console.ReadKey();
#endif
        }
    }
}
