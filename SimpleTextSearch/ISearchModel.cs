using System.Collections.Generic;

namespace SimpleTextSearch
{
    public interface ISearchModel
    {
        /// <summary>
        /// Build the main index dictionery for search model
        /// </summary>
        /// <returns>true if build succeed and false otherwise</returns>
        bool buildIndexDic();
        /// <summary>
        /// Handles search operation for search model
        /// </summary>
        /// <param name="Query">custom user query that support 
        /// boolean operators (AND, OR, NOT) and priority</param>
        /// <returns>list of documents or empty list</returns>
        IEnumerable<int> searchIndexDic(string Query);
        /// <summary>
        /// Display some internal search logic only
        /// for debugging purposes!
        /// </summary>
        /// <param name="Objects"></param>
        void ConsoleDebug(params dynamic[] Objects);
    }
}
