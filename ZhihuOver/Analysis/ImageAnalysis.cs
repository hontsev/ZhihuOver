using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ZhihuOver.Analysis
{
    class ImageAnalysis
    {
        /// <summary>
        /// 根据网站来选择不同的规则，从html代码中获取需要的图片的url
        /// </summary>
        /// <param name="website"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static List<String> getImage(string website, string str = "")
        {
            List<String> imgs = new List<String>();
            Regex reg;
            MatchCollection m;

            reg = new Regex("src=\\\"([^\"]*?\\.(jpeg|jpg|png|gif)[^\"]*?)\\\"", RegexOptions.IgnoreCase);
            m = reg.Matches(str);
            for (int i = 0; i < m.Count; i++)
            {
                string path = m[i].Groups[1].ToString();
                if (path.IndexOf("http") < 0)
                {
                    Regex reg2 = new Regex("(http://.*?/)");
                    string site = reg2.Matches(website)[0].Groups[0].ToString();
                    path = site + path;
                }
                if (imgs.IndexOf(path) < 0) imgs.Add(path);
            }
            reg = new Regex("(http[^\\]?\\.(jpeg|jpg|png|gif)[^\"]*?)", RegexOptions.IgnoreCase);
            m = reg.Matches(str);
            for (int i = 0; i < m.Count; i++)
            {
                string path = m[i].Groups[1].ToString();
                if (imgs.IndexOf(path) < 0) imgs.Add(path);
            }


            return imgs;
        }
    }
}
