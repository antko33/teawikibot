using DotNetWikiBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChainyiBot
{
    class RedRefsHandler : IHandler
    {
        private const string Description = "Обработка ошибок в примечаниях";

        private readonly Regex notSpecifiedRegex = new Regex(@"для сносок <code>([\s\S]*?)</code> не указан текст");
        private readonly Regex notUsedRegex = new Regex(@"с именем «([\s\S]*?)», определённый в <code>&lt;references&gt;</code>, не используется в предшествующем тексте");
        private readonly Site wiki;
        private readonly Random rnd = new Random();

        public RedRefsHandler()
        {
            wiki = new Site(Constants.RuWikiUrl, SecuredConstants.Username, SecuredConstants.Password);
        }

        public void Run()
        {
            var page = GetRandomPage();
            string pageName = page.title;
            page.Load();
            string oldText = page.text;

            string pageUrl = wiki.address + "/wiki/" + pageName;
            string rawPageContent = wiki.GetWebPage(pageUrl);

            var notSpecMatches = notSpecifiedRegex.Matches(rawPageContent);
            foreach (Match match in notSpecMatches)
            {
                string refName = match.Groups[1].Value;
                PageList versions = new PageList(wiki);
                versions.FillFromPageHistory(pageName, 500);
                string refText = "{{подст:нет АИ}}";
                foreach (Page v in versions)
                {
                    Task.Delay(1 * 1000); // Чтобы не нагружать особо
                    v.Load();
                    Regex toSearchExpr = new Regex(@"<ref name=""?" + refName + @"""?>([\s\S]*?)<\/ref>");
                    if (toSearchExpr.IsMatch(v.text))
                    {
                        var refMatch = toSearchExpr.Match(v.text);
                        string refContent = refMatch.Groups[1]?.Value;
                        if (!string.IsNullOrEmpty(refContent))
                        {
                            refText = refMatch.Value;
                            break;
                        }
                    }
                }

                Regex invalidRefExpr = new Regex(@"<ref name=""?" + refName + @"""?\s*/\s*>");
                string textToReplace = invalidRefExpr.Match(page.text).Value;
                page.text = page.text.ReplaceFirst(textToReplace, refText);
            }
            Regex noSrcDoubleExpr = new Regex(@"({{подст:нет АИ}}){2,}");
            page.text = noSrcDoubleExpr.Replace(page.text, "{{подст:нет АИ}}");

            rawPageContent = wiki.GetWebPage(pageUrl);
            var notUsedMatches = notUsedRegex.Matches(rawPageContent);
            foreach (Match match in notUsedMatches)
            {
                string refName = match.Groups[1].Value;
                page.text = Regex.Replace(page.text, @"<ref name=""?" + refName + @"""?>[\s\S]*?</ref>", string.Empty);
            }

            if (page.text != oldText)
            {
                page.Save(Description, true);
            }
            else
            {
                FailureReporter.ReportFailure(wiki, pageName, Description);
            }
        }

        private Page GetRandomPage()
        {
            var pages = new PageList(wiki);
            pages.FillAllFromCategory("Категория:Страницы с ошибками в примечаниях");
            int count = pages.Count();
            int index = rnd.Next(0, count);
            return pages.pages.ElementAt(index);
        }
    }

    public static class StringExtensionMethods
    {
        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
}
