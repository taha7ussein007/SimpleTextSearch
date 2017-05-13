using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleTextSearch
{
    public sealed class SearchEngine
    {
        #region Singleton instance
        private static volatile SearchEngine instance = null;
        private static readonly object syncLock = new object();
        public static SearchEngine Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncLock)
                    {
                        if (instance == null)
                            instance = new SearchEngine();
                    }
                }
                return instance;
            }
        }
        #endregion
        private ISearchModel SearchMdl_Bool;
        private ISearchModel SearchMdl_InvrtdIdx;
        private ISearchModel SearchMdl_PosIdx;
        private ISearchModel SearchMdl_TFIDF;
        private static volatile bool flagBool;
        private static volatile bool flagInvrtd;
        private static volatile bool flagPos;
        private static volatile bool flagTFIDF;
        public async Task<IDictionary<int, string>> Search(string query, Algorithm alg)
        {
            IDictionary<int, string> finalResult = null;
            await Task.Run(() => 
            {
                IEnumerable<int> result = null;
                if (alg == Algorithm.BooleanSearch)
                {
                    while (!flagBool) ;
                    result = SearchMdl_Bool.searchIndexDic(query);
                }
                else if (alg == Algorithm.InvertedIndexSearch)
                {
                    while (!flagInvrtd) ;
                    result = SearchMdl_InvrtdIdx.searchIndexDic(query);
                }
                else if (alg == Algorithm.PositionalIndexSearch)
                {
                    while (!flagPos) ;
                    result = SearchMdl_PosIdx.searchIndexDic(query);
                }
                else if (alg == Algorithm.TFIDFSearchModel)
                {
                    while (!flagTFIDF) ;
                    result = SearchMdl_TFIDF.searchIndexDic(query);
                }
                if(alg == Algorithm.TFIDFSearchModel)
                    finalResult = GetResultDic(result, false);
                else
                    finalResult = GetResultDic(result, true);
            });
            return finalResult;
        }
        private IDictionary<int, string> GetResultDic(IEnumerable<int> SearchResult, bool Sorted = false)
        {
            IDictionary<int, string> ResultDic;
            if (Sorted)
                 ResultDic = new SortedDictionary<int, string>();
            else
                 ResultDic = new Dictionary<int, string>();
            if (SearchResult != null)
            {
                foreach (var id in SearchResult)
                {
                    string fileUrl = string.Empty;
                    if (FileManager.docsIds.TryGetValue(id, out fileUrl))
                        ResultDic.Add(id, fileUrl);
                }
            }
            return ResultDic;
        }
        public async Task RefreshIndexDic(Algorithm alg)
        {
            await Task.Run(() => 
            {
                while (!FileManager.Ready) ;
            });
            if (alg == Algorithm.BooleanSearch)
            {
                await Task.Run(() =>
                {
                    flagBool = false;
                    SearchMdl_Bool = new BooleanSearchModel();
                    flagBool = SearchMdl_Bool.buildIndexDic();
                });
            }
            else if (alg == Algorithm.InvertedIndexSearch)
            {
                await Task.Run(() =>
              {
                  flagInvrtd = false;
                  SearchMdl_InvrtdIdx = new InvertedIndexSearchModel();
                  flagInvrtd = SearchMdl_InvrtdIdx.buildIndexDic();
              });
            }
            else if (alg == Algorithm.PositionalIndexSearch)
            {
                await Task.Run(() =>
              {
                  flagPos = false;
                  SearchMdl_PosIdx = new PositionalIndexSearchModel();
                  flagPos = SearchMdl_PosIdx.buildIndexDic();
              });
            }
            else if (alg == Algorithm.TFIDFSearchModel)
            {
                await Task.Run(() =>
                {
                    flagTFIDF = false;
                    SearchMdl_TFIDF = new TFIDFSearchModel();
                    flagTFIDF = SearchMdl_TFIDF.buildIndexDic();
                });
            }
        }
    }
}
