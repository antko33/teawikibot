using DotNetWikiBot;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ChainyiBot
{
    class InvalidRefHandler : CheckWikiHandler
    {
        private readonly Regex refRegex = new Regex(@"ref name=([^/>]+)/?");
        private readonly List<char> invalidChars = new List<char>()
            {'\"', '\'', '/', '\\', '=', '?', '#', ' ', '\n'};

        protected override int Id => 104;
        protected override string Description => "Сноска с некорректным названием";

        protected override bool DoCheckWikiAction(Page wikiPage)
        {
            var oldText = wikiPage.text;

            var matches = refRegex.Matches(wikiPage.text);
            foreach (Match match in matches)
            {
                string name = match.Groups[1].Value;
                bool closedTag = name.EndsWith("/");
                name = name.TrimEnd('/');
                bool startQuote = name.StartsWith("\"");
                bool endQoute = name.EndsWith("\"");
                invalidChars.ForEach(ch => name = name.Replace(ch.ToString(), string.Empty));

                if (startQuote || endQoute)
                {
                    name = $"\"{name}\"";
                }
                if (closedTag)
                {
                    name = $"{name}/";
                }

                if (name != match.Groups[1].Value)
                {
                    wikiPage.text = wikiPage.text.Replace(match.Value, $"ref name={name}");
                }
            }

            return oldText != wikiPage.text;
        }
    }
}
