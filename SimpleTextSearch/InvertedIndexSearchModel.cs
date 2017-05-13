using System;
using System.Collections.Generic;

namespace SimpleTextSearch
{
    public class InvertedIndexSearchModel : ISearchModel
    {
        public SortedDictionary<string, WordInfo> indexDic { get; private set; }
        public bool buildIndexDic()
        {
            indexDic = new SortedDictionary<string, WordInfo>();
            var fm = new FileManager();
            var tp = new TextParser(Algorithm.InvertedIndexSearch);
            foreach (var doc in FileManager.docsIds)
            {
                var rawText = fm.ReadFileText(doc.Value);
                if (rawText != string.Empty)
                {
                    var textTokens = tp.parseText(rawText);
                    foreach (var token in textTokens)
                    {
                        WordInfo wi;
                        if (indexDic.TryGetValue(token, out wi))
                        {
                            if (!indexDic[token].Posting.Contains(doc.Key))
                            {
                                indexDic[token].Frequency++;
                                indexDic[token].Posting.Add(doc.Key);
                            }
                        }
                        else // New token
                        {
                            wi = new WordInfo();
                            wi.Frequency = 1;
                            wi.Posting = new List<int>() { doc.Key };
                            indexDic.Add(token, wi);
                        }
                    }
                }
            }
            //Sort all postings
            foreach (var record in indexDic)
                record.Value.Posting.Sort();
            return true;
        }
        public IEnumerable<int> searchIndexDic(string Query)
        {
            var tp = new TextParser(Algorithm.InvertedIndexSearch);
            var queryTokens = tp.parseQuery(Query);
            tp.optimizeQueryTokens(ref queryTokens);
            tp.replaceQueryTokens(ref queryTokens, FileManager.docsIds.Count, indexDic);
            var queryExps = tp.buildExps(queryTokens);
            var resultStr = string.Empty;
            for (int i = 0; i < queryExps.Count; i++)
            {
                if (i + 1 == queryExps.Count)
                    resultStr += tp.evalExp(queryExps[i], true);
                else
                    resultStr += tp.evalExp(queryExps[i], false);
            }
            return tp.Base2ToPos(resultStr);
        }
        public void ConsoleDebug(params dynamic[] ObjectsToDisplay)
        {
            throw new NotImplementedException();
        }
    }
    public class WordInfo
    {
        public int Frequency { get; set; }
        public List<int> Posting { get; set; }
    }
}
