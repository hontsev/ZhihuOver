using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ZhihuOver.Analysis
{
    class AnalysisController
    {
        public bool isWord;
        public bool isImage;
        public bool isFile;

        public string title;
        public Dictionary<string,string> content;
        public List<string> strList;
        public string keys;


        public AnalysisController()
        {
            isWord = true;
            isImage = false;
            isFile = false;
            title = "";
            content = new Dictionary<string,string>();
            strList = new List<string>();
            keys = "";
        }

        /// <summary>
        /// 统计字符串中的汉字个数
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public int getHanNum(string str)
        {
            if (str.Length > 1000) str = str.Substring(0, 1000);
            int count = 0;
            Regex regex = new Regex(@"[\u4E00-\u9FA5]+$");
            for (int i = 0; i < str.Length; i++)
            {
                if (regex.IsMatch(str[i].ToString()))
                {
                    count++;
                }
            }
            return count;
        }

        public string getWebsiteName(string url)
        {
            string name = "";
            if (url.Split('\\').Length <= 1) name = url;
            else name = url.Split('\\')[2].ToString();
            return name;
        }

        public void analysis(string url , string html,getType type)
        {
            string sitename = getWebsiteName(url);


            if (isWord)
            {
                //title = TextAnalysis.getTitle(sitename, html);
                content = TextAnalysis.contentFormat(url, html, type);
                string[] key = keys.Split(' ');
                bool suit=true;
                foreach (var k in key)
                {
                    if (!content.ContainsKey("content")) break;
                    if (string.IsNullOrWhiteSpace(k)) continue;
                    if (!content["content"].Contains(k))
                    {
                        suit = false;
                        break;
                    }
                }
                if (!suit)
                {
                    content["content"] = "";
                }
            }


            if (isImage)
            {
                strList = ImageAnalysis.getImage(sitename, html);
            }
        }
    }
}
