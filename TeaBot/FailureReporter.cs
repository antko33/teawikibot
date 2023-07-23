using DotNetWikiBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChainyiBot
{
    static class FailureReporter
    {
        public const int MaxRecordIntervalInDays = 7;

        public static void ReportFailure(Site wiki, string pageName, string desc)
        {
            var reportPage = new Page(wiki, "Участник:Чайный/PleaseCheck");
            reportPage.Load();

            if (reportPage.text.Contains($"[[{pageName}]]"))
            {
                return;
            }

            StringBuilder sb = new StringBuilder(reportPage.text);
            sb.Append($"[[{pageName}]];");
            sb.Append($"{desc};");
            sb.Append($"{DateTime.UtcNow}\n");

            reportPage.text = sb.ToString();
            reportPage.Save($"Страница [[{pageName}]] добавлена для проверки", false);
        }

        public static void CleanOldCases()
        {
            Site wiki = new Site(Constants.RuWikiUrl, SecuredConstants.Username, SecuredConstants.Password);
            var reportPage = new Page(wiki, "Участник:Чайный/PleaseCheck");
            reportPage.Load();

            List<string> recordsToRemove = new List<string>();

            foreach (string record in reportPage.text.Split('\n'))
            {
                if (string.IsNullOrEmpty(record))
                {
                    continue;
                }

                DateTime recordDt = DateTime.Parse(record.Split(';').Last());
                if (DateTime.UtcNow - recordDt > TimeSpan.FromDays(MaxRecordIntervalInDays))
                {
                    recordsToRemove.Add(record);
                }
            }

            if (recordsToRemove.Any())
            {
                StringBuilder sb = new StringBuilder(reportPage.text);
                recordsToRemove.ForEach(record => sb.Replace(record, string.Empty));
                reportPage.text = sb.ToString();
                reportPage.Save("Очистка списка", false, allowEmpty: true);
            }
        }
    }
}
