using LintingHelper.Helpers;
using PluginCore;
using PluginCore.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LintingHelper
{
    class LintingCache
    {
        Dictionary<string, List<LintingCacheItem>> results = new Dictionary<string, List<LintingCacheItem>>();

        public List<LintingResult> GetResultsFromPosition(ITabbedDocument document, int position)
        {
            if (results.ContainsKey(document.FileName))
            {
                var list = results[document.FileName];
                var line = document.SciControl.LineFromPosition(position);

                var localResults = new List<LintingResult>();
                var pos = 0;
                foreach (var item in list)
                {
                    pos += item.Position;
                    if (pos <=  position && (pos + item.Result.Length >= position || item.Result.Length < 0 && item.Result.Line == line + 1))
                        localResults.Add(item.Result);

                    //not needed anymore
                    //var start = document.SciControl.PositionFromLine(item.Result.Line - 1);
                    //var len = document.SciControl.LineLength(item.Result.Line - 1);
                    //start += item.Result.FirstChar;
                    //if (item.Result.Length > 0)
                    //{
                    //    len = item.Result.Length;
                    //}
                    //else
                    //{
                    //    len -= item.Result.FirstChar;
                    //}
                    //var end = start + len;
                    
                    //if (start <= position && (end >= position || item.Result.Length < 0 && item.Result.Line == line + 1))
                    //{
                    //    //suitable result
                    //    localResults.Add(item);
                    //}
                }

                return localResults;
            }

            return null;
        }

        /// <summary>
        /// Adds the given <paramref name="results"/>.
        /// Uses DocumentManager to look up the documents.
        /// </summary>
        public void AddResults(List<LintingResult> results)
        {
            results = results.OrderBy(r => GetResultPosition(DocumentManager.FindDocument(r.File), r)).ToList();

            foreach (var result in results)
            {
                AddResult(result);
            }
        }

        /// <summary>
        /// Adds the given <paramref name="result"/>.
        /// Uses DocumentManager to look up the document.
        /// </summary>
        private void AddResult(LintingResult result)
        {
            var document = DocumentManager.FindDocument(result.File);
            var list = results.GetOrCreate(document.FileName);
            var position = GetResultPosition(document, result);

            if (list.Count > 0)
            {
                var lastPos = GetCacheItemPosition(list, list[list.Count - 1]);
                position -= lastPos;
            }

            var item = new LintingCacheItem
            {
                Position = position,
                Length = GetResultLength(document, result),
                Result = result
            };

            if (list.Find(item.Equals) == null)
                list.Add(item);
        }

        public void OnTextChange(ITabbedDocument document, int position, int length, int linesAdded)
        {
            var sci = document.SciControl;
            var list = results[document.FileName];
            var itemPosition = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                itemPosition += item.Position;

                if (itemPosition >= position) //first affected item
                {
                    //fix position
                    item.Position += length;
                    itemPosition += length;
                    var line = sci.LineFromPosition(itemPosition);
                    item.Result.Line = line + 1;
                    item.Result.FirstChar = itemPosition - sci.PositionFromLine(line);
                    break;
                }

                var text = sci.GetTextRange(itemPosition, itemPosition + item.Length - 1);
                var mouseText = sci.GetTextRange(position, position + 10);
                if (itemPosition + item.Length - 1 > position) //position is within item
                {
                    list.RemoveAt(i);
                    if (i < list.Count) //if there is a next item
                    {
                        //adjust it's position
                        var next = list[i];
                        var pos = itemPosition + next.Position + length;
                        next.Position = item.Position + next.Position + length;
                        var line = item.Result.Line = sci.LineFromPosition(pos);
                        item.Result.FirstChar = pos - sci.PositionFromLine(line);
                    }
                    break;
                }
            }
        }

        public void RemoveDocument(string document)
        {
            if (results.ContainsKey(document))
            {
                results.Remove(document);
            }
        }

        /// <summary>
        /// Removes documents that are not opened anymore
        /// </summary>
        /// <param name="documents">The documents that are currently opened</param>
        public void RemoveAllExcept(IList<string> documents)
        {
            var copy = new List<string>(results.Keys);
            foreach (var doc in copy)
            {
                if (!documents.Contains(doc))
                {
                    results.Remove(doc);
                }
            }
        }

        private int GetCacheItemPosition(List<LintingCacheItem> list, LintingCacheItem item)
        {
            var pos = 0;
            foreach (var i in list)
            {
                pos += i.Position;
                if (i == item) return pos;
            }

            return -1;
        }

        private int GetResultLength(ITabbedDocument document, LintingResult result)
        {
            return result.Length >= 0 ? result.Length : document.SciControl.LineLength(result.Line - 1) - result.FirstChar - 1;
        }

        private int GetResultPosition(ITabbedDocument document, LintingResult result)
        {
            return document.SciControl.PositionFromLine(result.Line - 1) + result.FirstChar;
        }

    }

    class LintingCacheItem
    {
        public int Position;
        public int Length;
        public LintingResult Result;

        public bool Equals(LintingCacheItem other)
        {
            return Position == other.Position && Result.Equals(other.Result);
        }
    }
}
