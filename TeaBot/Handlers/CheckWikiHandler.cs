using DotNetWikiBot;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChainyiBot
{
    abstract class CheckWikiHandler : IHandler
    {
        private const string GetListUrl = "https://checkwiki.toolforge.org/cgi-bin/checkwiki.cgi?project=ruwiki&view=bots&id={0}&offset=0";
        private const string SetAsDoneUrl = "https://checkwiki.toolforge.org/cgi-bin/checkwiki.cgi?project=ruwiki&view=only&id={0}&title={1}";

        private const string ListPattern = @"<pre>((.|\n)*)</pre>";

        protected readonly Site wiki;
        protected readonly Random rnd = new Random();

        protected abstract int Id { get; }
        protected abstract string Description { get; }

        public CheckWikiHandler()
        {
            wiki = new Site(Constants.RuWikiUrl, SecuredConstants.Username, SecuredConstants.Password);
        }

        public void Run()
        {
            Regex listRgx = new Regex(ListPattern);
            var pgListRaw = wiki.GetWebPage(string.Format(GetListUrl, Id));

            try
            {
                var pgList = listRgx.Match(pgListRaw).Groups[1].Value.Trim().Split('\n').ToList();
                var index = rnd.Next(0, pgList.Count);

                var pageName = pgList[index];
                var wikiPage = new Page(wiki, pageName);
                wikiPage.Load();

                if (DoCheckWikiAction(wikiPage))
                {
                    wikiPage.Save($"[[Проект:Check Wikipedia|Check Wikipedia]]: {Description}", true);
                    wiki.GetWebPage(string.Format(SetAsDoneUrl, Id, pageName));
                }
                else
                {
                    FailureReporter.ReportFailure(wiki, pageName, Description);
                }
            }
            catch { }
        }

        protected abstract bool DoCheckWikiAction(Page wikiPage);
    }
}
