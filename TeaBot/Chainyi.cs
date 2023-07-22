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
            List<IHandler> handlers = new List<IHandler>
            {
                new CheckWikiHandler()
            };
            handlers.ForEach(handler => handler.Run());
        }
    }
}
