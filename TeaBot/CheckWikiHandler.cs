using DotNetWikiBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace ChainyiBot
{
    class CheckWikiHandler : IHandler
    {
        private const string GetListUrl = "https://checkwiki.toolforge.org/cgi-bin/checkwiki.cgi?project=ruwiki&view=bots&id={0}&offset=0";
        private const string SetAsDoneUrl = "https://checkwiki.toolforge.org/cgi-bin/checkwiki.cgi?project=ruwiki&view=only&id={0}&title={1}";

        private const string ListPattern = @"<pre>((.|\n)*)</pre>";

        private const string ExplicitLinkPattern = @"\[{1}(.*wikipedia.*wiki/.*[^\]]{1})\]{1}[^\]]{1}";
        private const string PageNamePattern1 = @"wiki*\/(\S+)";
        private const string PageTextPattern1 = @"wiki*\/\S+\s+(.*)$";

        private List<(int, string, Func<Page, bool>)> CheckEntities;
        private readonly Site wiki;
        private readonly Random rnd = new Random();

        public CheckWikiHandler()
        {
            wiki = new Site(Constants.RuWikiUrl, SecuredConstants.Username, SecuredConstants.Password);
            CheckEntities = new List<(int, string, Func<Page, bool>)>()
            {
                ( 90, "Внешняя ссылка на русскую Википедию", CheckWikiLinks )
            };
        }

        public void Run()
        {
            int item = rnd.Next(0, CheckEntities.Count);
            int id = CheckEntities[item].Item1;
            string description = CheckEntities[item].Item2;
            var func = CheckEntities[item].Item3;

            Regex listRgx = new Regex(ListPattern);
            var pgListRaw = wiki.GetWebPage(string.Format(GetListUrl, id));

            try
            {
                var pgList = listRgx.Match(pgListRaw).Groups[1].Value.Trim().Split('\n').ToList();
                var index = rnd.Next(0, pgList.Count);

                var pageName = pgList[index];
                var wikiPage = new Page(wiki, pageName);
                wikiPage.Load();

                if (func(wikiPage))
                {
                    wikiPage.Save($"[[Проект:Check Wikipedia|Check Wikipedia]]: {description}", true);

                    wiki.GetWebPage(string.Format(SetAsDoneUrl, id, pageName));

                    Console.WriteLine($"{description}: Статья {pageName} исправлена");
                }
                else
                {
                    Console.WriteLine($"{description}: Статья {pageName} и так сойдёт");
                }
            }
            catch (WikiBotException e)
            {
                Console.WriteLine("Не могу отредактировать страницу");
                Console.WriteLine(e.Message);
            }
            catch { }
        }

        private bool CheckWikiLinks(Page wikiPage)
        {
            string oldText = wikiPage.text;

            var linkRegex = new Regex(ExplicitLinkPattern);
            var matches = linkRegex.Matches(wikiPage.text);

            foreach (var item in matches)
            {
                var match = item as Match;
                string extLink = WebUtility.UrlDecode(match.Groups[1].Value);

                Regex nameRegex = new Regex(PageNamePattern1);
                string targetName = nameRegex.Match(extLink).Groups[1]?.Value.Replace('_', ' ');

                Regex textRegex = new Regex(PageTextPattern1);
                string targetText = textRegex.Match(extLink).Groups[1]?.Value;

                string intLink = string.IsNullOrEmpty(targetText)
                    ? $"[{targetName}]"
                    : $"[{targetName}|{targetText}]"; // Одна пара скобок уже есть

                wikiPage.text = wikiPage.text.Replace(match.Groups[1].Value, intLink);
            }

            return wikiPage.text != oldText;
        }
    }
}
