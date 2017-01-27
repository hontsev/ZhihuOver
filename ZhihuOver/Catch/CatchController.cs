using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZhihuOver.Catch;
using System.Web;
using System.Net;
using System.IO;


namespace ZhihuOver.Catch
{
    class CatchController
    {
        public string nowNum;
        //public string nowStr;
        public EncodingState encoding;

        public getType type;
        public int offset;

        public string cookies;


        public CatchController()
        {
            //addState = AddState.num_staticLength;
            type = getType.question;
            cookies = "";
            nowNum = "0";
            encoding = EncodingState.utf8;
            offset = 0;
        }

        private Encoding getEncoding()
        {
            if (this.encoding == EncodingState.utf8) return Encoding.UTF8;
            else if (this.encoding == EncodingState.gbk) return Encoding.GetEncoding("gbk");
            else return Encoding.Default;
        }

        public string getNowUrl(getType ttype = getType.question, string tnowNum = "", int toffset = 0)
        {
            getType mtype = type;
            string mnowNum = nowNum;
            int moffset = offset;
            if (!string.IsNullOrWhiteSpace(tnowNum))
            {
                mtype = ttype;
                mnowNum = tnowNum;
                moffset = toffset;
            }
            string url = "";
            switch (mtype)
            {
                case getType.answer:
                    url = string.Format("http://www.zhihu.com/api/v4/questions/{0}/answers?include=data%5B*%5D.content&offset={1}&limit=1", mnowNum, moffset);
                    break;
                case getType.article:
                    url = string.Format("http://zhuanlan.zhihu.com/api/posts/{0}", mnowNum);
                    break;
                case getType.comment:
                    url = string.Format("http://www.zhihu.com/api/v4/answers/{0}/comments?include=data%5B%2A%5D.content&limit=1&offset={1}", mnowNum, moffset);
                    break;
                case getType.question:
                    url = string.Format("http://www.zhihu.com/question/{0}", mnowNum);
                    break;
                case getType.topic:
                    url = string.Format("http://www.zhihu.com/topic/{0}/organize", mnowNum);
                    break;
                case getType.user:
                    url = string.Format("http://www.zhihu.com/people/{0}/answers", mnowNum);
                    break;
                case getType.comment_article:
                    url = string.Format("http://zhuanlan.zhihu.com/api/posts/{0}/comments?limit=1&offset={1}", mnowNum, moffset);
                    break;
                case getType.question_list:
                    url = string.Format("https://www.zhihu.com/topic/{0}/unanswered?page={1}", mnowNum, moffset);
                    break;
                default: break;
            }
            return url;
        }

        ///// <summary>
        ///// 获取下一个要扫描的号码
        ///// </summary>
        ///// <returns></returns>
        //private void getNext()
        //{

        //    if (nowNum == 0)
        //    {
        //        //init
        //        nowStr = nowNum.ToString();
        //    }
        //    else
        //    {
        //        string beforeStr = nowStr;
        //        WordsIncrement incStr = new WordsIncrement();

        //        nowStr = incStr.getNext(beforeStr);

        //    }
        //}

        ///// <summary>
        ///// 获取下一个要扫描的号码
        ///// </summary>
        ///// <returns></returns>
        //private void getNextOffset()
        //{
        //    if (offset == 0)
        //    {
        //        //init
        //        nowStr = nowNum.ToString();
        //    }
        //    else
        //    {
        //        string beforeStr = nowStr;
        //        WordsIncrement incStr = new WordsIncrement();

        //        nowStr = incStr.getNext(beforeStr);

        //    }
        //}

        public void gotoNext(bool isoffset = false)
        {
            if (isoffset) offset++;
            else nowNum = new WordsIncrement().getNext(nowNum.ToString());
        }


        public string catchHtml(string post=null)
        {
            string html = "";
            html = WebConnection.getDataWithCookie(getNowUrl(), cookies, getEncoding(), post);

            //nowNum += 1;
            return html;
        }

        public string catchHtml(getType ttype, string tnowNum, int toffset,string post = null)
        {
            return WebConnection.getDataWithCookie(getNowUrl(ttype, tnowNum, toffset), cookies, getEncoding(), post);
        }

        public void saveFile(string name, string path)
        {
            WebClient client = new WebClient();
            client.DownloadFile(path.Replace('\\', '/'), name);
        }

        /// <summary>
        /// 文件名非法字符
        /// </summary>
        private static readonly char[] InvalidFileNameChars = new[]
        {
            '"',
            '<',
            '>',
            '|',
            '\0',
            '\u0001',
            '\u0002',
            '\u0003',
            '\u0004',
            '\u0005',
            '\u0006',
            '\a',
            '\b',
            '\t',
            '\n',
            '\v',
            '\f',
            '\r',
            '\u000e',
            '\u000f',
            '\u0010',
            '\u0011',
            '\u0012',
            '\u0013',
            '\u0014',
            '\u0015',
            '\u0016',
            '\u0017',
            '\u0018',
            '\u0019',
            '\u001a',
            '\u001b',
            '\u001c',
            '\u001d',
            '\u001e',
            '\u001f',
            ':',
            '*',
            '?',
            '\\',
            '/'
        };

        public static string CleanInvalidFileName(string fileName)
        {
            fileName = fileName + "";
            fileName = InvalidFileNameChars.Aggregate(fileName, (current, c) => current.Replace(c + "", ""));
            if (fileName.Length > 1)
                if (fileName[0] == '.')
                    fileName = "dot" + fileName.TrimStart('.');
            if (fileName.Length > 20)
            {
                fileName = string.Format("{0}_{1}", fileName.Substring(0, 10), fileName.Substring(fileName.Length - 10, 10));
            }
            return fileName;
        }

        public static string getImagePathExt(string path)
        {
            string[] allows = { ".jpeg", ".jpg", ".png", ".gif" };
            string ext = ".jpg";
            foreach (var a in allows)
            {
                if (path.Contains(a)) 
                { 
                    ext = a; 
                    break; 
                }
            }
            return ext;
        }

        private static string rebuildUrl(string url)
        {
            return url.Replace("\\/", "/");
        }

        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="imgPath"></param>
        /// <param name="dirPath"></param>
        /// <param name="imgTitle"></param>
        public void saveImg(List<string> imgPath, string dirPath,string imgTitle)
        {
            imgTitle = CleanInvalidFileName(imgTitle);
            for (int i = 0; i < imgPath.Count; i++)
            {
                //Invoke(printEvent, (object)imgPath[i]);
                string path = rebuildUrl(imgPath[i]);
                string name = string.Format("{0}/{1}_{2}{3}", 
                    dirPath ,
                    imgTitle,
                    i.ToString().PadLeft(imgPath.Count.ToString().Length,'0'),
                    getImagePathExt(path)
                    );
                Directory.CreateDirectory(dirPath);

                WebClient imgClient = new WebClient();
                try
                {
                    Stream imgStream = imgClient.OpenRead(path);
                    BinaryReader r = new BinaryReader(imgStream);
                    byte[] mbyte = new byte[100000000];
                    int allmybyte = (int)mbyte.Length;
                    int startmbyte = 0;
                    while (allmybyte > 0)
                    {
                        int m = r.Read(mbyte, startmbyte, allmybyte);
                        if (m == 0)
                            break;

                        startmbyte += m;
                        allmybyte -= m;
                    }

                    if (startmbyte < 51200)
                    {
                        //Invoke(printEvent, (object)("img too small:" + startmbyte + "bytes"));
                        //continue;
                    }

                    imgStream.Dispose();
                    FileStream img = new FileStream(name, FileMode.Create, FileAccess.Write);
                    img.Write(mbyte, 0, startmbyte);
                    img.Flush();
                    img.Close();
                    //Invoke(printEvent, (object)"save:" + name);
                    imgStream.Close();
                    imgClient.Dispose();
                }
                catch (Exception e)
                {
                    //Invoke(printEvent, (object)"exception:" + e.Message);
                    continue;
                }
            }
        }
    }
}
