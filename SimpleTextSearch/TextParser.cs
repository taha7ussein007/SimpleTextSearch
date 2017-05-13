using ExpressionEvaluator;
using Porter2Stemmer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleTextSearch
{
    public class TextParser
    {
        public const int MAX_BUFFER = 64;
        private int LAST_STRING_LENGTH = 0;  //to add missing 0's for last expression eval result
        private Algorithm _Algorithm { get; set; }
        private static readonly char[] ArabicAlpha = 
            "ذضصثقفغعهخحجدشسيبلأاتنمكطئءؤرآىةوزظ۰۱۲۳٤٥٦٧۸۹".ToCharArray();
        public TextParser(Algorithm Algorithm)
        {
            _Algorithm = Algorithm;
        }
        private bool IsArabic(char ch)
        {
            if (ArabicAlpha.Contains(ch))
                return true;
            else
                return false;
        }
        public List<string> parseText(string text)
        {
            var charText = text.ToCharArray();
            string token = string.Empty;
            List<string> textTokens = new List<string>();
            var ep2s = new EnglishPorter2Stemmer();

            for (int i = 0; i < charText.Length; i++)
            {
                char c = charText[i];
                if (char.IsLetterOrDigit(c) || IsArabic(c)) //Accept english and arabic only
                {
                    token += c;
                }
                else
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        if(token.All(ch => char.IsLetterOrDigit(ch)))
                            textTokens.Add(ep2s.Stem(token).Value); //stem then add
                        else //for arabic
                            textTokens.Add(token);
                        token = string.Empty;
                    }
                }
            }
            if (!string.IsNullOrEmpty(token)) //for last token
            {
                if (token.All(ch => char.IsLetterOrDigit(ch)))
                    textTokens.Add(ep2s.Stem(token).Value); //stem then add
                else //for arabic
                    textTokens.Add(token);
                token = string.Empty;
            }
            return textTokens;
        }
        public List<string> parseQuery(string query)
        {
            var charQuery = query.ToCharArray();
            string token = string.Empty;
            List<string> queryTokens = new List<string>();

            for (int i = 0; i < charQuery.Length; i++)
            {
                char c = charQuery[i];
                if (c == '(' || c == ')')
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        queryTokens.Add(token);
                        token = string.Empty;
                    }
                    queryTokens.Add(c.ToString());
                }
                else if (char.IsLetterOrDigit(c) || IsArabic(c)) //Accept english and arabic only
                {
                    token += c;
                }
                else
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        queryTokens.Add(token);
                        token = string.Empty;
                    }
                }
            }
            if (!string.IsNullOrEmpty(token)) //for last token
            {
                queryTokens.Add(token);
                token = string.Empty;
            }

            return queryTokens;
        }
        public void optimizeQueryTokens(ref List<string> tokens, bool INSERT_AND = false)
        {
            var ep2s = new EnglishPorter2Stemmer();
            if (_Algorithm != Algorithm.TFIDFSearchModel)
            {
                for (int i = 0; i < tokens.Count; i++)
                {
                    if (tokens[i] != "(" && tokens[i] != ")" && tokens[i] != "NOT" && tokens[i] != "AND" && tokens[i] != "OR")
                    {
                        //Insert Missinng ORs
                        if (i + 1 < tokens.Count)
                        {
                            if (tokens[i + 1] != "(" && tokens[i + 1] != ")" && tokens[i + 1] != "AND" && tokens[i + 1] != "OR")
                            {
                                if (INSERT_AND)
                                    tokens.Insert(i + 1, "AND");
                                else
                                    tokens.Insert(i + 1, "OR");
                            }
                        }
                        //Stem Tokens using porter2Stemmer
                        if (tokens[i].All(ch => char.IsLetterOrDigit(ch))) //if english
                            tokens[i] = ep2s.Stem(tokens[i].ToLower()).Value; //stem then add
                    }
                    else if (tokens[i] == ")")
                    {
                        if (i + 1 < tokens.Count)
                        {
                            if (tokens[i + 1] != ")" && tokens[i + 1] != "AND" && tokens[i + 1] != "OR")
                            {
                                if (INSERT_AND)
                                    tokens.Insert(i + 1, "AND");
                                else
                                    tokens.Insert(i + 1, "OR");
                            }
                        }
                    }
                }
            }
            else // if(_Algorithm == Algorithm.TFIDFSearchModel)
            {
                for (int i = 0; i < tokens.Count; i++)
                {
                    //Stem Tokens using porter2Stemmer
                    if (tokens[i].All(ch => char.IsLetterOrDigit(ch))) //if english
                        tokens[i] = ep2s.Stem(tokens[i].ToLower()).Value; //stem then add
                }
            }
        }
        private string PosToBase2(List<int> PostingList, int size)
        {
            int[] base2intArr = new int[size];
            string result = string.Empty;
            foreach (var id in PostingList)
                base2intArr[id] = 1;
            for (int i = 0; i < base2intArr.Length; i++)
                result += base2intArr[i];
            return result;
        }
        public List<int> Base2ToPos(string Base2PostingStr)
        {
            List<int> Base2PosList = new List<int>();
            for (int i = 0; i < Base2PostingStr.Length; i++)
            {
                if (Base2PostingStr[i] == '1')
                    Base2PosList.Add(i);
            }
            return Base2PosList;
        }
        public string BoolToBase2(List<bool> boolList)
        {
            string Base2Str = string.Empty;
            foreach (var boolNum in boolList)
            {
                Base2Str += boolNum ? '1' : '0';
            }
            return Base2Str;
        }
        public void replaceQueryTokens(ref List<string> tokens, int docsCount, dynamic indexDictionary)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                switch (tokens[i])
                {
                    case "(":
                    case ")":
                        break;
                    case "NOT":
                        tokens[i] = "~";
                        break;
                    case "AND":
                        tokens[i] = "&";
                        break;
                    case "OR":
                        tokens[i] = "|";
                        break;
                    default:
                        if(_Algorithm  == Algorithm.InvertedIndexSearch)
                        {
                            var wi = new WordInfo();
                            //search dictionary for the word
                            if (indexDictionary.TryGetValue(tokens[i], out wi))
                                //get base2 equivelant word string
                                tokens[i] = PosToBase2(wi.Posting, docsCount);
                            else //word not found
                                tokens[i] = "0";
                        }
                        else if (_Algorithm == Algorithm.BooleanSearch)
                        {
                            bool[] boolVector;
                            //search dictionary for the word
                            if (indexDictionary.TryGetValue(tokens[i], out boolVector))
                                //get base2 equivelant word string
                                tokens[i] = BoolToBase2(boolVector.ToList());
                            else //word not found
                                tokens[i] = "0";
                        }
                        else if (_Algorithm == Algorithm.PositionalIndexSearch)
                        {
                            var pi = new PosInfo();
                            //search dictionary for the word
                            if (indexDictionary.TryGetValue(tokens[i], out pi))
                                //get base2 equivelant word string
                                tokens[i] = PosToBase2(pi.PostingDic.Keys.ToList(), docsCount);
                            else //word not found
                                tokens[i] = "0";
                        }
                        break;
                }
            }
        }
        public List<string> buildExps(List<string> tokens)
        {
            List<string> exps = new List<string>();
            bool hasRest = true;
            if(tokens.Count < 1)
                hasRest = false;
            for (int begin = 0; hasRest; begin += MAX_BUFFER)
            {
                var exp = string.Empty;
                foreach (var token in tokens)
                {
                    switch (token)
                    {
                        case "(":
                        case ")":
                        case "~":
                        case "&":
                        case "|":
                            exp += token;
                            break;
                        //Get MAX Buffer Portion OF The Binary String
                        //Then Convert it to int64 then to string then add it.
                        default:
                            if (begin + MAX_BUFFER < token.Length)
                                exp += Convert.ToInt64(token.Substring(begin, MAX_BUFFER), 2).ToString();
                            else
                            {
                                var lastStr = token.Substring(begin);
                                LAST_STRING_LENGTH = lastStr.Length;
                                exp += Convert.ToInt64(lastStr, 2).ToString();
                                hasRest = false;
                            }
                            break;
                    }
                }
                try
                {
                    exps.Add(exp);
                }
                catch(Exception)
                {
                    exps.Clear();
                    break;
                }

            }
            return exps;
        }
        public string evalExp(string queryExp, bool isLastExp)
        {
            string result = string.Empty;
            object result_int;
            try
            {
                var exp = new CompiledExpression(queryExp);
                result_int = exp.Eval();
            }
            catch (Exception)
            {
                result_int = 0;
            }
            //convert result_int to binary char string
            var result_base2str = Convert.ToString(Convert.ToInt64(result_int), 2);
            //restore missing left most 0's after evaluation
            var sizeDiff = MAX_BUFFER - result_base2str.Length;
            if(isLastExp) //Last Buffer Portion Case
            {
                sizeDiff = LAST_STRING_LENGTH - result_base2str.Length;
                LAST_STRING_LENGTH = 0;
            }
            for (int i = 0; i < sizeDiff; i++)
                result += '0';
            result += result_base2str;
            return result;
        }
    }
}
