using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZhihuOver
{
    public enum getType
    {
        topic, question, answer, comment, article, user, comment_article, question_list
    }
    public class ZhihuItem
    {
        public string id;
        public string title;
        public string content;
        public string author;
        public getType type;

        public ZhihuItem(string mid, string mtitle, string mcontent,string mauthor, getType mtype)
        {
            id = mid;
            title = mtitle;
            content = mcontent;
            author = mauthor;
            type = mtype;
        }
    }
}
