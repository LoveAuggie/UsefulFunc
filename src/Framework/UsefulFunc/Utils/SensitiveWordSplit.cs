using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auggie.Lib.Utils
{
    public class SensitiveWordSplit
    {
        private static Dictionary<char, SwNode> swTree = new Dictionary<char, SwNode>();

        public static string Proc(string article)
        {
            int iStart = 0;

            SwNode curNode = null;
            while (iStart< article.Length)
            {
                int iNum = iStart;
                Dictionary<char, SwNode> CheckTree = swTree;

                CStart:
                var ic = article[iNum];
                if (CheckTree.TryGetValue(ic, out curNode))
                {
                    if (curNode.isEnd)
                    {
                        article = article.Remove(iStart, iNum - iStart + 1);
                        article = article.Insert(iStart, "***");
                        iStart = iStart + 3;
                        continue;
                    }
                    else
                    {
                        CheckTree = curNode.childs;
                        iNum++;
                        goto CStart;
                    }
                }
                else
                {
                    iStart++;
                }
            }
            return article;
        }

        public static void LoadWords(List<string> words)
        {
            Dictionary<char, SwNode> tree = swTree;
            foreach(var word in words)
            {
                tree = swTree;
                SwNode lNode = null;
                foreach (var wc in word)
                {
                    if (tree.TryGetValue(wc, out lNode))
                    {
                        tree = lNode.childs;
                    }
                    else
                    {
                        lNode = new SwNode() { word = wc };
                        tree[wc] = lNode;
                        tree = lNode.childs;
                    }
                }
                lNode.isEnd = true;
            }
        }
    }

    public class SwNode
    {
        public char word { get; set; }

        public Dictionary<char, SwNode> childs = new Dictionary<char, SwNode>();

        public bool isEnd { get; set; }
    }
}
