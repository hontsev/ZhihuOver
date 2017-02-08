using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Web;
using System.Net;
using System.Text.RegularExpressions;
using CCWin;
using System.Diagnostics;
using ZhihuOver.Analysis;
using ZhihuOver.Catch;
using ZhihuOver.Output;

namespace ZhihuOver
{
    public partial class WSForm : CCSkinMain
    {
        private AnalysisController ac;
        private CatchController cc;
        private OutputController oc;

        private const string catchConfigPath = "cconfig";
        private const string outputConfigPath = "oconfig";
        private const string analysisConfigPath = "aconfig";

        bool run = false;

        private List<Thread> td;

        bool multi = false;

        private List<ZhihuItem> items;
        private string thisid;
        private getType thistype; 

        public WSForm()
        {
            InitializeComponent();
            cc = new CatchController();
            ac = new AnalysisController();
            oc = new OutputController();
            initConfig();
            //comboBox1.SelectedIndex = 0;
            //comboBox2.SelectedIndex = 0;
        }

        private void initConfig()
        {
            loadConfig();
            updateUI();
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndex = 0;
            items = new List<ZhihuItem>();
        }

        private void loadConfig()
        {
            try
            {
                JsonController.getConfigInfoFromJson(catchConfigPath, cc);
            }
            catch (Exception e)
            {
                print("读取配置文件失败："+e.Message);
            }

            try
            {
                JsonController.getOutputConfigInfoFromJson(outputConfigPath, oc);
            }
            catch (Exception e)
            {
                print("读取配置文件失败：" + e.Message);
            }

            try
            {
                JsonController.getAnalysisConfigInfoFromJson(analysisConfigPath, ac);
            }
            catch (Exception e)
            {
                print("读取配置文件失败：" + e.Message);
            }
        }

        private void saveConfig()
        {
            try
            {
                JsonController.saveConfigAsJson(catchConfigPath, cc);
            }
            catch (Exception e)
            {
                print("保存配置文件失败：" + e.Message);
            }

            try
            {
                JsonController.saveOutputConfigAsJson(outputConfigPath, oc);
            }
            catch (Exception e)
            {
                print("保存配置文件失败：" + e.Message);
            }

            try
            {
                JsonController.saveAnalysisConfigAsJson(analysisConfigPath, ac);
            }
            catch (Exception e)
            {
                print("保存配置文件失败：" + e.Message);
            }
        }

        /// <summary>
        /// 根据配置记录来刷新界面元素显示
        /// </summary>
        private void updateUI()
        {
            checkBox1.Checked = oc.isSaveOneTxt;
            textBox4.Text = oc.savePath;
            textBox8.Text = cc.cookies;
            textBox3.Text = cc.nowNum;
            comboBox3.SelectedIndex = (int)cc.type;
        }

        /// <summary>
        /// 根据界面元素来刷新配置属性
        /// </summary>
        private void updateConfig()
        {
            oc.isSaveOneTxt = checkBox1.Checked;
            oc.savePath = textBox4.Text;
            cc.cookies = textBox8.Text;
            cc.nowNum = textBox3.Text;
            cc.type = (getType)comboBox3.SelectedIndex;
            ac.keys = textBox2.Text;
            multi = (comboBox1.SelectedIndex == 1 ? true : false);
        }




        /// <summary>
        /// 在界面输出log信息
        /// </summary>
        /// <param name="str"></param>
        public void print(string str)
        {
            if (textBox6.InvokeRequired)
            {
                MyDelegate.sendStringDelegate printEvent = new MyDelegate.sendStringDelegate(print);
                Invoke(printEvent, (object)str);
            }
            else
            {
                if (textBox6.Text.Length > 500000) textBox6.Text = "";
                textBox6.AppendText(str + "\r\n");
            }
            
        }

        public void printInfo1(string str)
        {
            if (textBox7.InvokeRequired)
            {
                MyDelegate.sendStringDelegate printEvent = new MyDelegate.sendStringDelegate(printInfo1);
                Invoke(printEvent, (object)str);
            }
            else
            {
                textBox7.Text=str;
            }
        }


        public void updateList()
        {
            if (listView1.InvokeRequired)
            {
                MyDelegate.sendVoidDelegate mEvent = new MyDelegate.sendVoidDelegate(updateList);
                Invoke(mEvent);
            }
            else
            {
                listView1.Items.Clear();
                foreach (var item in items)
                {
                    ListViewItem i = new ListViewItem(item.id);
                    i.SubItems.Add(item.content);
                    listView1.Items.Add(i);
                }
                listView1.Refresh();
            }
        }

        private void updateState(string str)
        {
            if (label6.InvokeRequired)
            {
                MyDelegate.sendStringDelegate updateStateEvent = new MyDelegate.sendStringDelegate(updateState);
                Invoke(updateStateEvent, (object)str);
            }
            else
            {
                label6.Text = str;
            }
        }

        /// <summary>
        /// 更新当前进度显示
        /// </summary>
        /// <param name="str"></param>
        public void updateNo(string str)
        {
            if (textBox1.InvokeRequired)
            {
                MyDelegate.sendStringDelegate updateNoEvent = new MyDelegate.sendStringDelegate(updateNo);
                Invoke(updateNoEvent, (object)str);
            }
            else
            {
                textBox1.Text = str;
            }
        }

        /// <summary>
        /// 将内容输出到文件
        /// </summary>
        /// <param name="str"></param>
        public void printToFile(string str)
        {
            //string fileName = textBox5.Text;
            string fileName = "";
            using(FileStream s = File.Open(fileName, FileMode.OpenOrCreate))
            {
                s.Position = s.Length;
                StreamWriter w = new StreamWriter(s);
                w.WriteLine(str);
                w.Close();
            }
        }
        private string ToGBK(string str)
        {
            return System.Text.Encoding.GetEncoding("UTF-8").GetString(System.Text.Encoding.Default.GetBytes(str));
        }

        private void startWorkOnce()
        {
            try
            {
                if (!run)
                {
                    cc.nowNum = "0";
                    //开始
                    updateConfig();
                    //this.domainName = config.url1.Split('/')[2];
                    run = false;
                    int tdnum = Int32.Parse(numericUpDown1.Value.ToString());
                    print("program begin. thread number:" + tdnum);
                    Thread td = new Thread(work);
                    td.Start(0);
                }
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        private void startTestWorkOnce()
        {
            try
            {
                if (!run)
                {
                    cc.nowNum = "0";
                    updateConfig();
                    run = false;
                    //print("program begin. ");
                    Thread td = new Thread(testwork);
                    td.Start();
                }
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        private void startWork()
        {
            try
            {
                if (!run)
                {
                    updateState("爬取中");
                    //开始
                    updateConfig();
                    //this.domainName = config.url1.Split('/')[2];
                    run = true;
                    int tdnum = Int32.Parse(numericUpDown1.Value.ToString());
                    print("program begin. thread number:" + tdnum);
                    td = new List<Thread>();
                    for (int i = 1; i <= tdnum; i++)
                    {
                        Thread ttd = new Thread(work);
                        td.Add(ttd);
                        ttd.Start(i);
                    }
                }
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        private void stopWork()
        {
            run = false;
            updateState("停止");
        }

        private void testwork()
        {
            string html = cc.catchHtml();
            ac.analysis(cc.getNowUrl(), html, cc.type);
            print("分析结果：");
            print(ac.title);
            foreach (var a in ac.content)
            {
                string outputstr = "";
                outputstr += a.Value + "\r\n";
                print(string.Format("【【【{0}】】】:\r\n\r\n\r\n\r\n{1}\r\n\r\n\r\n\r\n", a.Key, outputstr));
            }
            if (ac.strList.Count > 0) print(ac.strList[0].ToString());
        }

        

        private void dealAnalysisResult(Dictionary<string, string> res,bool multi)
        {
            string nowid = "";
            string nowcontent = "";
            nowid = cc.nowNum;
            res.TryGetValue("content", out nowcontent);
            printInfo1(string.Format("type:{0}\r\nid:{1}\r\n{2}\r\nauthor:{3}\r\ntitle:{4}\r\n{5}", cc.type.ToString(), nowid, nowcontent));
            if (multi)
            {
                //下载整个列表

                if (!string.IsNullOrWhiteSpace(nowid)) cc.nowNum = nowid;
                cc.offset = 0;
                if (cc.type == getType.topic) cc.type = getType.question_list;
                else if (cc.type == getType.question) cc.type = getType.answer;
                else if (cc.type == getType.answer) cc.type = getType.comment;
                else if (cc.type == getType.article) cc.type = getType.comment_article;
                this.items = new List<ZhihuItem>();
                for (int i = 0; i < 100; i++)
                {
                    ac.analysis(cc.getNowUrl(), cc.catchHtml(), cc.type);
                    cc.gotoNext(true);
                    string id = "";
                    string title = "";
                    string author = "";
                    string content = "";
                    ac.content.TryGetValue("id", out id);
                    ac.content.TryGetValue("title", out title);
                    ac.content.TryGetValue("author", out author);
                    ac.content.TryGetValue("content", out content);
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        if (cc.type == getType.question_list && i == 0) continue;
                        else break;
                    }
                    print(string.Format("({0})[{1}]:[{2}]", cc.type.ToString(), id, content));
                    if (cc.type == getType.question_list)
                    {
                        string[] ids = id.Split(',');
                        string[] contents = content.Split('|');
                        for (int j = 0; j < ids.Length; j++)
                        {
                            this.items.Add(new ZhihuItem(ids[j], title, author, contents[j], cc.type));
                        }
                    }
                    else
                    {
                        this.items.Add(new ZhihuItem(id, title, author, content, cc.type));
                    }
                    
                    updateList();
                }
            }
            else
            {
                //单个
            }

            ////储存为txt。
            //string savestr = "";
            ////以下划线开头的，不打印其key名
            //if (!item.Key.ToLower().StartsWith("_"))
            //    savestr += string.Format("{0}:\r\n", item.Key);
            //foreach (var s in item.Value)
            //{
            //    savestr += string.Format("{0}\r\n", s);
            //}
            ////savestr += "\r\n";
            //oc.output(ac.title, savestr);

        }

        /// <summary>
        /// 主循环工作函数
        /// </summary>
        private void work(object threadNum)
        {
            do
            {
                updateNo(cc.nowNum.ToString());
                ac.analysis(cc.getNowUrl(), cc.catchHtml(), cc.type);
                //处理正则匹配结果
                dealAnalysisResult(ac.content,multi);
                cc.gotoNext();
                //if (ac.content.Count > 1 && !string.IsNullOrWhiteSpace(ac.content.ElementAt(1).Value)) oc.output(ac.title, oc.formatOutputText(ac.content), OutputType.txt);
                
                //处理图片链接
                //if (ac.strList.Count > 0) print(ac.strList[0].ToString());
                //cc.saveImg(ac.strList, oc.savePath, ac.title + "_" + cc.nowNum.ToString());
            } while (run);
            if (threadNum != null) print("thread " + threadNum.ToString() + " end.");
        }

        private void workGetInfo()
        {
            getType type = getType.question;
            if (thistype== getType.topic) type = getType.question_list;
            else if (thistype == getType.question) type = getType.answer;
            else if (thistype == getType.answer) type = getType.comment;
            else if (thistype == getType.article) type = getType.comment_article;
            ac.analysis(cc.getNowUrl(type, thisid), cc.catchHtml(type, thisid, 0), type);
            dealAnalysisResult(ac.content, false);
        }

        private void workLoop()
        {
            for (int i = 0; i < 300; i++)
            {
                print(string.Format("loop:{0}", i.ToString()));
                WebConnection.getData(html, Encoding.Default);
                Thread.Sleep(100);
            }
        }

        
        /// <summary>
        /// 获取字符串中的文件后缀名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string getExt(string name)
        {
            if (name.Split('.').Length <= 0) return "";
            return name.Split('.')[name.Split('.').Length - 1];
        }
        

       
        

        private void openAddress(string filePath)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = "explorer";
            proc.StartInfo.Arguments = @"/select," + filePath;
            proc.Start();
        }
       



        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox4.Text = folderBrowserDialog1.SelectedPath;
            }
        }


        private void button13_Click(object sender, EventArgs e)
        {
            startTestWorkOnce();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            startWorkOnce();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            startWork();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            stopWork();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openAddress(textBox4.Text.ToString());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox4.Text = @"tmp\";
        }

        string html;


        private void WSForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            updateConfig();
            saveConfig();
            Environment.Exit(0);
        }

        private void WSForm_Shown(object sender, EventArgs e)
        {
            updateUI();
        }


        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            cc.cookies = textBox8.Text;
        }




        private void textBox6_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(MousePosition);
            }
        }

        private void 清空ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox6.Text = "";
        }

        private void workClearMultiImages()
        {
            print("clear multi-images");
            string path2 = "tmp_sames/";
            try
            {
                string path = textBox4.Text;
                List<string> files = new List<string>(Directory.GetFiles(path));
                List<FileInfo> fileinfos = new List<FileInfo>();
                foreach (var f in files) fileinfos.Add(new FileInfo(f));
                for (int i = 0; i < files.Count; i++)
                {
                    print(string.Format("{0}/{1}", i, files.Count));
                    //if (!isImage(files[i])) continue;
                    for (int j = i + 1; j < files.Count; j++)
                    {
                        if (j >= files.Count) break;
                        //if (!isImage(files[j])) continue;
                        try
                        {
                            //print(string.Format("at{0}", j));
                            if (fileinfos[i].Length == fileinfos[j].Length)
                            {
                                //if (!Directory.Exists(path2)) Directory.CreateDirectory(path2);
                                //File.Move(files[j], string.Format("{0}{1}", path2, Path.GetFileName(files[j])));
                                File.Delete(files[j]);
                                fileinfos.RemoveAt(j);
                                files.RemoveAt(j);
                                j--;
                            }
                        }catch(Exception e)
                        {

                        }

                    }
                }
            }
            catch(Exception e)
            {
                print("error");
            }
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            new Thread(workClearMultiImages).Start();
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedIndices.Count <= 0) return;
            thisid = listView1.SelectedItems[0].Text;
            thistype = cc.type;
            new Thread(workGetInfo).Start();
        }


        private string tid;
        private string qid;
        private string aid;
        private string wid;
        private string keys;
        private int searchtype;
        private int reporttype;
        private int allnum;
        private int MAXALLNUM = 1000000;
        private void workGetTargets()
        {
            print("开始检索待举报内容");
            
            CatchController cc = new CatchController();
            cc.offset = 0;
            switch (searchtype)
            {
                case 0:
                    //question
                    if (string.IsNullOrWhiteSpace(tid)) break;
                    cc.type = getType.question_list;
                    cc.nowNum = tid;
                    cc.offset = 1;
                    break;
                case 1:
                    //answer
                    if (string.IsNullOrWhiteSpace(qid)) break;
                    cc.type = getType.question_list;
                    cc.nowNum = tid;
                    break;
                case 2:
                    //comment
                    if (string.IsNullOrWhiteSpace(tid)) break;
                    cc.type = getType.question_list;
                    cc.nowNum = tid;
                    break;
                case 3:
                    //article
                    if (string.IsNullOrWhiteSpace(tid)) break;
                    cc.type = getType.article;
                    cc.nowNum = tid;
                    break;
                default: break;
            }

            for (int i = 0; i < allnum; i++)
            {

            }

            print("检索完毕 ");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            tid = textBox9.Text;
            qid = textBox5.Text;
            aid = textBox11.Text;
            wid = textBox12.Text;
            keys = textBox10.Text;
            allnum = int.Parse(numericUpDown2.Value.ToString());
            if (allnum < 0) allnum = MAXALLNUM;
            searchtype = comboBox4.SelectedIndex;
            reporttype = comboBox2.SelectedIndex;
            new Thread(workGetTargets).Start();
        }
       
    }
}
