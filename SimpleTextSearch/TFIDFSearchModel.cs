using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimpleTextSearch
{
    public class TFIDFSearchModel : ISearchModel
    {
        public SortedDictionary<string, TFIDF_WordInfo> indexDic { get; private set; }
        public bool buildIndexDic()
        {
            indexDic = new SortedDictionary<string, TFIDF_WordInfo>();
            var fm = new FileManager();
            var tp = new TextParser(Algorithm.TFIDFSearchModel);
            foreach (var doc in FileManager.docsIds)
            {
                var rawText = fm.ReadFileText(doc.Value);
                if (rawText != string.Empty)
                {
                    var textTokens = tp.parseText(rawText);
                    foreach (var token in textTokens)
                    {
                        TFIDF_WordInfo _TFIDF_WordInfo;
                        if (indexDic.TryGetValue(token, out _TFIDF_WordInfo))
                        {
                            if (_TFIDF_WordInfo.TFIDFVector[doc.Key] == 0)
                                _TFIDF_WordInfo.DF++;
                            _TFIDF_WordInfo.TFIDFVector[doc.Key]++; //Increase Term Count
                        }
                        else // New token
                        {
                            _TFIDF_WordInfo = new TFIDF_WordInfo();
                            _TFIDF_WordInfo.DF = 1;
                            _TFIDF_WordInfo.TFIDFVector = new double[FileManager.docsIds.Count];
                            _TFIDF_WordInfo.TFIDFVector[doc.Key] = 1; //First Time Term Count
                            indexDic.Add(token, _TFIDF_WordInfo);
                        }
                    }
                }
            } //Terms Counts Initialized
            //Compute TF-IDF Foreach Term (Token)
            foreach (var token in indexDic)
            {
                for (int i = 0; i < token.Value.TFIDFVector.Length; i++) 
                //Convert (token.Value.TFIDFVector[i] = count) TO (token.Value.TFIDFVector[i] = TF-IDF)
                {
                    if(token.Value.TFIDFVector[i] > 0)
                    {
                        var TF = 1 + Math.Log10(token.Value.TFIDFVector[i]);
                        var IDF = Math.Log10((double)FileManager.docsIds.Count / token.Value.DF);
                        token.Value.TFIDFVector[i] = TF * IDF;
                    }
                } 
            }
            return true;
        }
        public IEnumerable<int> searchIndexDic(string Query)
        {
            var tp = new TextParser(Algorithm.TFIDFSearchModel);
            var queryTokens = tp.parseQuery(Query);
            tp.optimizeQueryTokens(ref queryTokens);

            var PotentialDocs = new List<int>();
            foreach (var token in queryTokens)
            {
                TFIDF_WordInfo TFIDF_WI;
                if(indexDic.TryGetValue(token, out TFIDF_WI))
                {
                    PotentialDocs = Union(PotentialDocs, GetWordDocs(TFIDF_WI.TFIDFVector)); //=(OR)ing
                    PotentialDocs.Sort();
                }
            }

            var PotentialDocs_Scores = new Dictionary<int, double>(PotentialDocs.Count);
            foreach (var Doc in PotentialDocs)
                PotentialDocs_Scores.Add(Doc, 0.0);

            foreach (var PotentialDoc in PotentialDocs)
            {
                var TokensIntersection_qd = Intersect(queryTokens, GetAllTokens(PotentialDoc));
                double Score = 0.0;
                foreach (var IntersectedToken in TokensIntersection_qd)
                    Score += indexDic[IntersectedToken].TFIDFVector[PotentialDoc];
                if (Score > 0.0)
                    PotentialDocs_Scores[PotentialDoc] = Score;
                else
                    PotentialDocs_Scores.Remove(PotentialDoc);
            }
            var SortedResultPairList = from entry in PotentialDocs_Scores orderby entry.Value descending select entry;
            //
            ConsoleDebug(queryTokens, SortedResultPairList);
            //
            return (from kvp in SortedResultPairList select kvp.Key).ToList();
        }
        private List<int> GetWordDocs(double[] WordVector)
        {
            var WordDocsList = new List<int>();
            for (int i = 0; i < WordVector.Length; i++)
                if(WordVector[i] > 0)
                    WordDocsList.Add(i);
            return WordDocsList;
        }
        private List<string> GetAllTokens(int DocID)
        {
            List<string> DocTokens = new List<string>();
            foreach (var tokenEntry in indexDic)
                if (tokenEntry.Value.TFIDFVector[DocID] > 0)
                    DocTokens.Add(tokenEntry.Key);
            return DocTokens;
        }
        private List<string> Intersect(List<string> Operand1, List<string> Operand2)
        {
            var intersection = Operand1.Intersect(Operand2).ToList();
            intersection.Sort();
            return intersection;
        }
        private List<int> Union(List<int> Operand1, List<int> Operand2)
        {
            var union = Operand1.Union(Operand2).ToList();
            union.Sort();
            return union;
        }
        public void ConsoleDebug(params dynamic[] Objects)
        {
            FileManager.ShowConsoleSafely();
            Console.Clear();
            Console.WriteLine(" >>> This info for debugging purposes only <<< \n");
            //Print Token(Doc#: Weight, ...)
            foreach (var tkn in Objects[0])
            {
                TFIDF_WordInfo wi;
                if (indexDic.TryGetValue(tkn, out wi))
                {
                    Console.Write(tkn + " (");
                    for (int i = 0; i < wi.TFIDFVector.Length; i++)
                    {
                        string docPath;
                        FileManager.docsIds.TryGetValue(i, out docPath);
                        string docName = Path.GetFileNameWithoutExtension(docPath);
                        Console.Write(docName + ": " + "TFIDF(" + wi.TFIDFVector[i].ToString() + "), ");
                    }
                    Console.Write("\b" + "\b" + ")" + "\n");
                }
            }
            Console.WriteLine();
            //Print Doc#: Score(...)
            foreach (var kvp in Objects[1])
            {
                string docPath;
                FileManager.docsIds.TryGetValue(kvp.Key, out docPath);
                string docName = Path.GetFileNameWithoutExtension(docPath);
                Console.WriteLine(docName + ": Score({0})", kvp.Value.ToString());
            }

            Console.ReadKey(true);
            Console.Clear();
            FileManager.HideConsoleSafely();
        }
    }
    public class TFIDF_WordInfo
    {
        public int DF { get; set; }
        public double[] TFIDFVector { get; set; }
    }
}
