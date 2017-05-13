using System;
using System.Collections.Generic;

namespace SimpleTextSearch
{
    public class BooleanSearchModel : ISearchModel
    {
        public SortedDictionary<string, bool[]> indexDic { get; private set; }
        public bool buildIndexDic()
        {
            indexDic = new SortedDictionary<string, bool[]>();
            var fm = new FileManager();
            var tp = new TextParser(Algorithm.BooleanSearch);
            foreach (var doc in FileManager.docsIds)
            {
                var rawText = fm.ReadFileText(doc.Value);
                if (rawText != string.Empty)
                {
                    var textTokens = tp.parseText(rawText);
                    foreach (var token in textTokens)
                    {
                        bool[] boolWordVector;
                        if (indexDic.TryGetValue(token, out boolWordVector))
                        {
                            boolWordVector[doc.Key] = true;
                        }
                        else // New token
                        {
                            boolWordVector = new bool[FileManager.docsIds.Count];
                            boolWordVector[doc.Key] = true;
                            indexDic.Add(token, boolWordVector);
                        }
                    }
                }
            }
            return true;
        }
        public IEnumerable<int> searchIndexDic(string Query)
        {
            var tp = new TextParser(Algorithm.BooleanSearch);
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
}
