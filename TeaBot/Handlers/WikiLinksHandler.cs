using DotNetWikiBot;
using System.Net;
using System.Text.RegularExpressions;

namespace ChainyiBot
{
    class WikiLinksHandler : CheckWikiHandler
    {
        private readonly Regex explicitLinkExpr1 = new Regex(@"\[{1}(\S*wikipedia.*wiki/.*[^\]]{1})\]{1}[^\]]{1}");
        private readonly Regex explicitLinkExpr2 = new Regex(@"[^\[]{1}\[{1}[^\[]{1}\S*ru.wikipedia.*title=([^&#\s\]]*)[^#]*(#[^&#\s\]]*)?\s*([^\]]*)");
        private readonly Regex pageNameExpr1 = new Regex(@"wiki*\/(\S+)");
        private readonly Regex pageTextExpr1 = new Regex(@"wiki*\/\S+\s+(.*)$");

        protected override int Id => 90;
        protected override string Description => "Внешняя ссылка на русскую Википедию";

        protected override bool DoCheckWikiAction(Page wikiPage)
        {
            string oldText = wikiPage.text;

            var matches = explicitLinkExpr1.Matches(wikiPage.text);

            foreach (var item in matches)
            {
                var match = item as Match;
                string extLink = WebUtility.UrlDecode(match.Groups[1].Value);

                string targetName = pageNameExpr1.Match(extLink).Groups[1]?.Value?.Replace('_', ' ');
                string targetText = pageTextExpr1.Match(extLink).Groups[1]?.Value;

                string intLink = string.IsNullOrEmpty(targetText)
                    ? $"[{targetName}]"
                    : $"[{targetName}|{targetText}]"; // Одна пара скобок уже есть

                wikiPage.text = wikiPage.text.Replace(match.Groups[1].Value, intLink);
            }

            matches = explicitLinkExpr2.Matches(wikiPage.text);

            foreach (var item in matches)
            {
                var match = item as Match;
                string targetName = WebUtility.UrlDecode(match.Groups[1]?.Value?.Replace('_', ' '));
                if (match.Groups.Count > 2)
                {
                    string section = WebUtility.UrlDecode(match.Groups[2]?.Value?.Replace('_', ' '));
                    targetName += section;
                }

                string targetText = match.Groups.Count > 3
                    ? match.Groups[3]?.Value : string.Empty;

                string intLink = string.IsNullOrEmpty(targetText)
                    ? $"[[{targetName}]"
                    : $"[[{targetName}|{targetText}]";

                wikiPage.text = wikiPage.text.Replace(match.Value, intLink);
            }

            return wikiPage.text != oldText;
        }
    }
}
