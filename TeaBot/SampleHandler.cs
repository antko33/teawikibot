using DotNetWikiBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainyiBot
{
    class SampleHandler : IHandler
    {
        public void Run()
        {
            Site ruwiki = new Site(Constants.RuWikiUrl, SecuredConstants.Username, SecuredConstants.Password);
            var a = ruwiki.GetWebPage(ruwiki.apiPath + "?action=query&prop=categoryinfo&titles=Category:Foo|Category:Bar&format=json");
        }
    }
}
