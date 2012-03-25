using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMongo.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ReadingTool.Common.Enums;
using ReadingTool.Entities;

namespace ReadingTool.Tasks
{
    public class StatisticsTask : DefaultTask
    {
        protected override void DoWork()
        {
            var statsDb = _db.GetCollection(Statistics.CollectionName);
            var languagesDb = _db.GetCollection<Language>(Language.CollectionName);
            var wordsDb = _db.GetCollection<Word>(Word.CollectionName).AsQueryable();

            var users = _db.GetCollection<User>(User.CollectionName)
                .FindAll()
                .Select(x => x.UserId);

            foreach(var id in users)
            {
                var statistics = new Statistics()
                                     {
                                         Modified = DateTime.Now,
                                         Owner = id
                                     };

                var languages = languagesDb.Find(Query.EQ("Owner", id)).SetSortOrder(SortBy.Ascending("LanguageName"));

                foreach(var language in languages)
                {
                    var details = new Statistics.Details()
                                      {
                                          LanguageId = language.LanguageId,
                                          TotalKnownWords = wordsDb.Count(x => x.State == WordState.Known && x.LanguageId == language.LanguageId),
                                          TotalUnknownWords = wordsDb.Count(x => x.State == WordState.Unknown && x.LanguageId == language.LanguageId)
                                      };

                    for(int i = 0; i < Statistics.Period.Length - 1; i++)
                    {
                        var start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0).AddDays(-1 * Statistics.Period[i]);
                        var end = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0).AddDays(-1 * Statistics.Period[i + 1]);

                        details.KnownPeriod[i] = wordsDb
                            .Count(x =>
                                   x.State == WordState.Known &&
                                   x.LanguageId == language.LanguageId &&
                                   x.Modified < start && x.Modified > end
                            );

                        details.UnknownPeriod[i] = wordsDb
                            .Count(x =>
                                   x.State == WordState.Unknown &&
                                   x.LanguageId == language.LanguageId &&
                                   x.Modified < start && x.Modified > end
                            );
                    }

                    statistics.Languages.Add(details);
                }

                statsDb.Save(statistics);
            }
        }
    }
}
