using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Web;

namespace ZhihuOver.Analysis
{
    class TextAnalysis
    {
        public static string getTitle(string website, string str = "")
        {
            string res = "";

            Regex reg1;
            string tmp;

            switch (website)
            {
                default:
                    reg1 = new Regex("<title>([^<]*)</title>");
                    str = str.Trim();
                    if (reg1.IsMatch(str))
                    {
                        res = reg1.Match(str).Groups[1].ToString();
                    }
                    else
                    {
                        res = website;
                    }
                    break;
            }

            res = res.Replace('\\', '_');
            res = res.Replace('/', '_');
            res = res.Replace('*', '_');
            res = res.Replace(':', '_');
            res = res.Replace('<', '_');
            res = res.Replace('>', '_');
            res = res.Replace('"', '_');

            if (res.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "").Length > 2)
                return res;
            else return "";
        }

        /// <summary>
        /// Unicode转字符串
        /// </summary>
        /// <param name="source">经过Unicode编码的字符串</param>
        /// <returns>正常字符串</returns>
        public static string Unicode2String(string source)
        {
            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(
                         source, x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)));
        }


        public static Dictionary<string,string> contentFormat(string url, string html,getType type)
        {
            Dictionary<string,string> res = new Dictionary<string, string>();
            //html = formatHtml(html);
            html = Unicode2String(HttpUtility.HtmlDecode(html).Replace("<br>", "\r\n").Replace("<\\/p>", "\r\n").Replace("<b>", "").Replace("<p>", "").Replace("<\\/b>", ""));
            res.Add("all",  html );
            Regex reg1, reg2, reg3, reg4;
            switch (type)
            {
                case getType.question:
                    reg1 = new Regex("editableDetail\":\"(.*?)\",\"collapsed", RegexOptions.Singleline);
                    res.Add("content", reg1.Match(html).Groups[1].Value);
                    reg1=new Regex(">([^>]*?) - 知乎</title>",RegexOptions.Singleline);
                    res.Add("title", reg1.Match(html).Groups[1].Value);
                    reg1=new Regex("question/([\\d]*?)\"",RegexOptions.Singleline);
                    res.Add("id", reg1.Match(html).Groups[1].Value);
                    break;
                case getType.answer:
                    reg1 = new Regex("content\": \"(.*?)\", \"created_time", RegexOptions.Singleline);
                    res.Add("content", reg1.Match(html).Groups[1].Value);
                    reg1=new Regex("answer\", \"id\": ([\\d]*)}",RegexOptions.Singleline);
                    res.Add("id", reg1.Match(html).Groups[1].Value);
                    break;
                case getType.comment:
                    reg1 = new Regex("content\": \"(.*?)\", \"featured", RegexOptions.Singleline);
                    res.Add("content", reg1.Match(html).Groups[1].Value);
                    reg1=new Regex("comment\", \"id\": ([\\d]*),",RegexOptions.Singleline);
                    res.Add("id", reg1.Match(html).Groups[1].Value);
                    break;
                case getType.article:
                    reg1 = new Regex("content\": \"(.*?)\", \"state", RegexOptions.Singleline);
                    res.Add("content", reg1.Match(html).Groups[1].Value);
                    break;
                case getType.comment_article:
                    reg1 = new Regex("content\": \"(.*?)\", \"createdTime", RegexOptions.Singleline);
                    res.Add("content", reg1.Match(html).Groups[1].Value);
                    break;
                case getType.question_list:
                    reg1 = new Regex("<a target=\"_blank\" class=\"question_link\" href=\"/question/([\\d]*?)\">([^<]*?)</a>", RegexOptions.Singleline);
                    var matches = reg1.Matches(html);
                    string ids = "";
                    string names = "";
                    for (int i = 0; i < matches.Count; i++)
                    {
                        string tmpid = matches[i].Groups[1].Value;
                        string tmpname = matches[i].Groups[2].Value;
                        if (!ids.Contains(tmpid))
                        {
                            ids += tmpid + ",";
                            names += tmpname + "|";
                        }
                    }
                    if (ids.EndsWith(",")) ids = ids.Substring(0, ids.Length - 1);
                    if (names.EndsWith("|")) names = names.Substring(0, names.Length - 1);
                    res.Add("id", ids);
                    res.Add("content", names);
                    break;
                case getType.topic:
                    reg1 = new Regex("<title>话题组织 - ([^ ].*?) - 知乎</title>", RegexOptions.Singleline);
                    res.Add("content", reg1.Match(html).Groups[1].Value);
                    break;
                default:
                    break;
            }
            return res;
        }

        
    }
}
