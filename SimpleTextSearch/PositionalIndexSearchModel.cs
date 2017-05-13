using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimpleTextSearch
{
    public class PositionalIndexSearchModel : ISearchModel
    {
        public SortedDictionary<string, PosInfo> indexDic { get; private set; }
        public bool buildIndexDic()
        {
            indexDic = new SortedDictionary<string, PosInfo>();
            var fm = new FileManager();
            var tp = new TextParser(Algorithm.InvertedIndexSearch);
            foreach (var doc in FileManager.docsIds)
            {
                var rawText = fm.ReadFileText(doc.Value);
                if (rawText != string.Empty)
                {
                    var textTokens = tp.parseText(rawText);
                    var tokenPosition = 0;
                    foreach (var token in textTokens)
                    {
                        PosInfo pi;
                        if (indexDic.TryGetValue(token, out pi)) //already existed
                        {
                            if (indexDic[token].PostingDic.Keys.ToList().Contains(doc.Key)) //doc exist
                            { //append new position
                                indexDic[token].PostingDic[doc.Key].Add(tokenPosition);
                            }
                            else //doc not exist
                            { //append new doc & position then increase Freq.
                                indexDic[token].PostingDic.Add(doc.Key, new List<int>() { tokenPosition });
                                indexDic[token].Frequency++;
                            }
                        }
                        else // New token
                        {
                            pi = new PosInfo();
                            pi.Frequency = 1;
                            pi.PostingDic = new SortedDictionary<int, List<int>>();
                            pi.PostingDic.Add(doc.Key, new List<int>() { tokenPosition });
                            indexDic.Add(token, pi);
                        }
                        tokenPosition++;
                    }
                }
            }
            return true;
        }
        public IEnumerable<int> searchIndexDic(string Query)
        {
            var tp = new TextParser(Algorithm.PositionalIndexSearch);
            var queryTokens = tp.parseQuery(Query);
            tp.optimizeQueryTokens(ref queryTokens, true); //true means insert ANDs
            var optimizedQueryTokens = new List<string>(queryTokens);
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
            var PotentialResult = tp.Base2ToPos(resultStr);
            //PotentialResult -> all docIDs that contain all query tokens
            //maybe not fully CWP? Ok, Let's verify the PotentialResult
            var notTkn = new string[] { "(", ")", "NOT", "AND", "OR" }; //token validation comparer array
            var finalResult = new List<int>();
            //First: get all positions lists
            foreach (var docID in PotentialResult)
            {
                //for each doc we try to extract positionsLists for each token
                var positionsLists = new List<List<int>>();
                foreach (var tkn in optimizedQueryTokens)
                {
                    if (!notTkn.Contains(tkn)) //if valid token
                    {
                        PosInfo pi;
                        //get positions info for this token
                        if (indexDic.TryGetValue(tkn, out pi))
                        {
                            List<int> doc_tkn_positions;
                            //extract positions_list for this token
                            if (pi.PostingDic.TryGetValue(docID, out doc_tkn_positions))
                            {
                                //add positions_list to positionsLists
                                positionsLists.Add(new List<int>(doc_tkn_positions));
                            }
                        }
                    }
                }
                //Second: verify that positions lists have sequence
                if (findSequence(positionsLists))
                    finalResult.Add(docID);
            }
            //
            ConsoleDebug(optimizedQueryTokens);
            //
            return finalResult;
        }
        /// <summary>
        /// positionsLists is a List of Lists of Sorted Integer values (not distinct)
        /// then verify that this positionsLists have any sequence?
        /// If yes? return true, otherwise return false
        /// </summary>
        private bool findSequence(List<List<int>> positionsLists)
        {
            if (positionsLists.Count == 0) return false; //case empty list
            if (positionsLists.Count == 1) return true; //case one token
            var mergedPositions = new List<KeyValuePair<int, int>>(); //key = position, value = token_index
            for (int i = 0; i < positionsLists.Count; i++) // i -> refer to a list
                foreach (var position in positionsLists[i])
                    mergedPositions.Add(new KeyValuePair<int, int>(position, i));
            mergedPositions.Sort((kvp1, kvp2) => kvp1.Key.CompareTo(kvp2.Key));
            var sequences = ConsecutiveSequences(mergedPositions, positionsLists.Count).ToList();
            if (sequences.Count > 0)
                return true;
            return false;
        }
        /// <summary>Returns a collection containing all consecutive sequences of
        /// Key-Value Pair integers in the input collection.</summary>
        /// <param name="input">The collection of  Key-Value Pair integers
        /// in which to find Key-Value Pair consecutive sequences.</param>
        /// <param name="length">length that a sequence should have
        /// to be returned.</param>
        private List<List<KeyValuePair<int, int>>> ConsecutiveSequences(
            IEnumerable<KeyValuePair<int, int>> input, int length = 2)
        {
            //Get Distinct Elements Based On Values (case: query contains the same word)
            SortedDictionary<int, int> distinctInput = new SortedDictionary<int, int>();
            foreach (var kvp in input)
                if(!(distinctInput.ContainsKey(kvp.Key) || distinctInput.ContainsValue(kvp.Value)))
                    distinctInput.Add(kvp.Key, kvp.Value);
            input = null;

            var Sequences = new List<List<KeyValuePair<int, int>>>();
            var Sequence = new List<KeyValuePair<int, int>>();
            for (int i = 1; i < distinctInput.Count(); i++)
            {
                var prev = new KeyValuePair<int, int>(distinctInput.ElementAt(i - 1).Key, distinctInput.ElementAt(i - 1).Value);
                var curr = new KeyValuePair<int, int>(distinctInput.ElementAt(i).Key, distinctInput.ElementAt(i).Value);
                if (prev.Key + 1 == curr.Key)
                {
                    Sequence.Add(new KeyValuePair<int, int>(prev.Key, prev.Value));
                    if (Sequence.Count == length)
                    {
                        Sequences.Add(new List<KeyValuePair<int, int>>(Sequence));
                        Sequence.Clear();
                    }
                    Sequence.Add(new KeyValuePair<int, int>(curr.Key, curr.Value));
                    if (Sequence.Count == length)
                    {
                        Sequences.Add(new List<KeyValuePair<int, int>>(Sequence));
                        Sequence.Clear();
                    }
                    Sequence.Remove(curr);
                }
                else
                    Sequence.Clear();
            }
            return Sequences;
        }
        public void ConsoleDebug(params dynamic[] Objects)
        {
            FileManager.ShowConsoleSafely();
            Console.Clear();
            Console.WriteLine(" >>> This info for debugging purposes only <<< \n");

            foreach (var tkn in Objects[0])
            {
                PosInfo pi;
                if (indexDic.TryGetValue(tkn, out pi))
                {
                    Console.Write(tkn + " (");
                    foreach (var doc_posList in pi.PostingDic)
                    {
                        string docPath;
                        FileManager.docsIds.TryGetValue(doc_posList.Key, out docPath);
                        string docName = Path.GetFileNameWithoutExtension(docPath);
                        Console.Write(docName + ": ");
                        foreach (var pos in doc_posList.Value)
                            Console.Write(pos.ToString() + ',');
                        Console.Write(' ');
                    }
                    Console.Write("\b" + "\b" + ")" + "\n");
                }
            }
            Console.ReadKey(true);
            Console.Clear();
            FileManager.HideConsoleSafely();
        }
    }
    public class PosInfo
    {
        //<token -> Frequency, [DocID -> [PositionsList], DocID -> [PositionsList], ... ]>
        public int Frequency { get; set; }
        public SortedDictionary<int, List<int>> PostingDic { get; set; }
    }
}