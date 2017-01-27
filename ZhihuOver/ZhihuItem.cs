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
        public string content;
        public getType type;

        public ZhihuItem(string mid, string mcontent, getType mtype)
        {
            id = mid;
            content = mcontent;
            type = mtype;
        }
    }
}
