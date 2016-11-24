using MetroFramework.Forms;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MetroFramework
{
    public partial class Form1 : MetroForm {
        static Dictionary<IPEndPoint, SocketListener> ThreadList = new Dictionary<IPEndPoint, SocketListener>();
        public static DateTime startRecordTime;
        public static Form1 _Form1;
        DB db = new DB();
        Gopro gp = new Gopro();
        internal static int Read_record = 0;
        public static int senseTime = 600;
        //---------------------
        JObject jsonSearch;
        static GoProStatus_class GoProStatus;
        Timer timerHandle, testTimer;
        Boolean shutter = false;
        //----Async
        delegate string AsycMethod();
        delegate string AsycEventArgs(string Result);
        event AsycEventArgs AsycEventHandler;

        Boolean booooooooooooooo = true;

        Task taskWhile;
        //----

        DateTime startTime = new DateTime();

        public Form1() {

            InitializeComponent();
            _Form1 = this;

            GoProStatus = new GoProStatus_class();
            timerHandle = new Timer();
            testTimer = new Timer();
            timerHandle.Interval = 200; //refresh rate
            testTimer.Interval = 1000;
            //QQQQQ.Text = Properties.Settings.Default.Setting1.ToString();
            yy.Value = DateTime.Now.Year;
            mm.Value = DateTime.Now.Month;
            dd.Value = DateTime.Now.Day;
        }

        private void Form1_Load(object sender, EventArgs e) {
            // TODO: 這行程式碼會將資料載入 'dbDataSet.runnerInfo' 資料表。您可以視需要進行移動或移除。


        }

        private void getRunnerList() {

        }

        public string FirstName {
            get { return listBoxRunner.Text; }
        }

        private void connectMC_Click(object sender, EventArgs e) {

            try {

                SocketListener subThread = new SocketListener();
                subThread.setComp(sumLabel, currentLabel, listBoxRunner);
                db.setComp(currentLabel, listBoxRunner);            
                if ( subThread.handleIPaddr(textIP.Text, textPort.Text) ) {
                    if ( subThread.StartConnect() ) {
                        if ( !ThreadList.ContainsKey(subThread.ipPoint) ) {
                            ThreadList.Add(subThread.ipPoint, subThread);
                            listBoxCM.Items.Add(subThread.ip + ":" + subThread.port);
                            sendSocket.Start();
                        }
                        else {
                            MessageBox.Show("連線失敗，請勿重複輸入");
                        }
                    }
                    else {
                        subThread = null;
                        MessageBox.Show("連線失敗");
                    }
                }
                else {
                    MessageBox.Show("請輸入正確號碼");
                }
            }
            catch { }
        }



        private void sendSocket_Tick(object sender, EventArgs e) {
            try {
                if ( ThreadList.Count > 0 ) {
                    var list = ThreadList.ToList();
                    for ( int i = 0 ; i < list.Count ; i++ ) {
                        if ( !list[i].Value.checkConnect() ) //斷連移除list中的該筆連線
                        {
                            try {
                                ThreadList.Remove(list[i].Key);
                                listBoxCM.Items.Remove(list[i].Value.ip + ":" + list[i].Value.port);
                            }
                            catch ( Exception ex ) { }
                        }
                    }
                }
                if ( ThreadList.Count == 0 ) {
                    sendSocket.Stop();
                }
            }
            catch {
                MessageBox.Show("Thread Connect Check Error!!!!");
            }
        }

        private void listBoxCM_SelectedIndexChanged(object sender, EventArgs e) {

        }



        private void listBoxCM_DoubleClick(object sender, EventArgs e) {
            if ( listBoxCM.SelectedIndex != -1 ) {
                DialogResult AlertMsg = MessageBox.Show("你確定關閉此連線!?", "Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if ( AlertMsg == DialogResult.Yes ) {
                    try {
                        var tmp = listBoxCM.SelectedItem.ToString().Split(':');
                        IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(tmp[0].Trim()), Int32.Parse(tmp[1].Trim()));
                        if ( ThreadList[ipPoint].StopConnect() )  //Stop Connect Thread
                        {
                            MessageBox.Show(ipPoint.Address + ":" + ipPoint.Port + " is disconnect!!");
                            ThreadList[ipPoint] = null;
                            ThreadList.Remove(ipPoint);
                            listBoxCM.Items.RemoveAt(listBoxCM.SelectedIndex);
                        }
                    }
                    catch ( Exception ex ) { }
                }

            }
        }

        private void timerGoproState_Tick(object sender, EventArgs e) {

        }

        private void metroTabPage5_Click(object sender, EventArgs e) {

        }

        private void htmlLabel11_Click(object sender, EventArgs e) {

        }

        private void metroButton1_Click(object sender, EventArgs e) {
            timerHandle.Enabled = true;
            timerHandle.Tick += Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e) {
            AsycMethod asycDelegate = new AsycMethod(handleRES);
            IAsyncResult IAR = asycDelegate.BeginInvoke(EndCallback, asycDelegate);
            this.AsycEventHandler += new AsycEventArgs(Form1_AsycEventHandler);
        }
        private string Form1_AsycEventHandler(string Result) {
            if ( Result != "OK" ) {
                Application.Idle -= Application_Idle;
                this.BeginInvoke(new MethodInvoker(this.DisplayError), null); // UpdateUI
                this.AsycEventHandler -= new AsycEventArgs(Form1_AsycEventHandler);
            }
            else {
                this.BeginInvoke(new MethodInvoker(this.UpdateUI), null); // UpdateUI
            }

            return Result; //message -> OK
        }

        void EndCallback(IAsyncResult ar) {
            AsycMethod asycDelegate = ar.AsyncState as AsycMethod;
            string msg = asycDelegate.EndInvoke(ar);

            if ( AsycEventHandler != null )
                AsycEventHandler(msg);

        }
        private string handleRES() {


            try {
                WebClient wc = new WebClient();
                WebClient wc2 = new WebClient();
                var json = wc.DownloadString("http://10.5.5.9/gp/gpControl/status");

                JObject googleSearch = JObject.Parse(json.ToString());
                GoProStatus.num_total_photos = googleSearch["status"]["38"].ToString();
                GoProStatus.num_total_videos = googleSearch["status"]["39"].ToString();
                GoProStatus.remaining_photos = googleSearch["status"]["34"].ToString();
                GoProStatus.remaining_video_time = googleSearch["status"]["35"].ToString();
                GoProStatus.mode = googleSearch["status"]["43"].ToString();
                GoProStatus.sub_mode = googleSearch["status"]["44"].ToString();
                GoProStatus.system_busy = googleSearch["status"]["8"].ToString();
                GoProStatus.ap_ssid = googleSearch["status"]["30"].ToString();

                //var json2 = wc2.DownloadString("http://10.5.5.9/gp/gpMediaList");
                //jsonSearch = JObject.Parse(json2.ToString());
                return "OK";
            }
            catch ( Exception e ) {
                return e.Message.ToString();
            }

        }
        private void UpdateUI() {
            infoGroup.Text = "GoPro - " + GoProStatus.ap_ssid;
            TotalPhotoNumT.Text = GoProStatus.num_total_photos;
            TotalVideoNumT.Text = GoProStatus.num_total_videos;
            RemainingPhotoNumT.Text = GoProStatus.remaining_photos;
            RemainingVideoNumT.Text = GoProStatus.remaining_video_time;
            int mode = int.Parse(GoProStatus.mode);
            switch ( mode ) {
                case 5:
                    modeT.Text = "Settings";
                    break;
                case 4:
                    modeT.Text = "unknowon";
                    break;
                case 7:
                    modeT.Text = "USB";
                    break;
                default:
                    modeT.Text = modeListBox.GetItemText(modeListBox.Items[int.Parse(GoProStatus.mode)]);
                    break;
            }
            submodeT.Text = GoProStatus.sub_mode;
            if ( GoProStatus.system_busy == "1" ) {
                busyT.Text = "相機忙碌中...";
                busyT.ForeColor = Color.Red;
            } else {
                busyT.Text = "相機閒置中...";
                busyT.ForeColor = Color.Green;
            }

            if ( GoProStatus.system_busy == "1" && GoProStatus.mode != "0" )
                shutterButton.Enabled = false;
            else if ( GoProStatus.system_busy == "1" && GoProStatus.mode == "0" )
                shutterButton.Text = "處理中...";
            else if ( GoProStatus.system_busy == "0" && GoProStatus.mode == "0" )
                shutterButton.Text = "快門";
            else
                shutterButton.Enabled = true;
            /*
            if ((int.Parse(GoProStatus.num_total_photos) + int.Parse(GoProStatus.num_total_photos) ) != 0)
                MediaList(); //Update Media File List
                */
            searchBtn.Enabled = true;
            ConnectButton.Text = "連接成功 GoPro!";
            ConnectButton.Enabled = false;

            timeParse.Text = Properties.Settings.Default.Setting1.ToString();
        }
        async Task<int> AccessTheWebAsync(string url) {
            // You need to add a reference to System.Net.Http to declare client.
            HttpClient client = new HttpClient();

            Task<string> getStringTask = client.GetStringAsync(url);

            string urlContents = await getStringTask;

            // The return statement specifies an integer result.
            // Any methods that are awaiting AccessTheWebAsync retrieve the length value.
            return urlContents.Length;
        }

        private async void shutterButton_Click(object sender, EventArgs e) {

            if ( GoProStatus.mode == "0" ) {
                //shutter switch 0=off 1=on
                if ( shutter == false ) {
                    int msg = await AccessTheWebAsync("http://10.5.5.9/gp/gpControl/command/shutter?p=1");
                    shutterButton.Text = "處理中...";

                    //save dateTime to DB
                    startRecordTime = System.DateTime.Now;
                    Properties.Settings.Default.Setting1 += 1;
                    Properties.Settings.Default.Save();
                    db.setStartTime(Properties.Settings.Default.Setting1, startRecordTime, Int32.Parse(GoProStatus.num_total_videos));
                }
                else {
                    int msg = await AccessTheWebAsync("http://10.5.5.9/gp/gpControl/command/shutter?p=0");
                    shutterButton.Text = "快門";
                }

                shutter = !shutter; //switch
            }
            else {
                if ( GoProStatus.system_busy == "0" ) {
                    int msg = await AccessTheWebAsync("http://10.5.5.9/gp/gpControl/command/shutter?p=1");
                    shutterButton.Enabled = false;
                }


            }
        }

        private async void changeModButton_Click(object sender, EventArgs e) {
            if ( modeListBox.SelectedIndex != -1 ) {
                int contentLength = await AccessTheWebAsync("http://10.5.5.9/gp/gpControl/command/mode?p=" + modeListBox.SelectedIndex.ToString());
            }
        }

        private async void metroButton1_Click_1(object sender, EventArgs e) {
            if ( GoProStatus.system_busy == "0" ) {
                int contentLength = await AccessTheWebAsync("http://10.5.5.9/gp/gpControl/command/storage/delete/all");
                Properties.Settings.Default.Setting1 = 0;
                Properties.Settings.Default.Save();
                db.dropTimeDB();

            }



        }

        private void metroButton1_Click_2(object sender, EventArgs e) {
            Properties.Settings.Default.Setting1 = 0;
            Properties.Settings.Default.Save();
            db.dropTimeDB();

        }

        private void metroTile4_Click(object sender, EventArgs e) {

        }

        private void textBox1_TextChanged(object sender, EventArgs e) {

        }

        private void textBox1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
            if ( e.KeyCode == Keys.Enter ) {
                searchRunner(runnerNum.Text);
            }
        }
        public void searchRunner(String str) {
            String[] runnerInfo = db.getRunnerInfo(runnerNum.Text);
            if ( runnerInfo != null ) {
                String[] timeInfo = db.getStratTime(Int32.Parse(runnerInfo[3]));

                runnerNameT.Text = runnerInfo[0];
                runnerNumT.Text = runnerInfo[1];
                runnerFTimeT.Text = runnerInfo[2];
                startTimeIdT.Text = runnerInfo[3];



            }
            else {
                MessageBox.Show("無此跑者");
            }
        }

        private async void button1_Click(object sender, EventArgs e) {
            if ( runnerFTimeT.Text != "" ) {

                await Task.Run(() => gp.searchVideo(runnerFTimeT.Text,
                    db.getStratTime(Int32.Parse(startTimeIdT.Text))[0],
                    db.getStratTime(Int32.Parse(startTimeIdT.Text))[1],
                    GoProStatus.num_total_videos,
                    endTimeOffset.Value.ToString(),
                    speedOffset.Value.ToString()));                    
            }
            else {
                MessageBox.Show("尚未紀錄時間0");
            }
        }

        private void metroPanel1_Paint(object sender, PaintEventArgs e) {

        }

        private void metroTabPage4_Click(object sender, EventArgs e) {

        }

        public static Dictionary<String, String> chipInNumAndrunnNum;
        private void downloadData_Click(object sender, EventArgs e) {
            chipInNumAndrunnNum = db.loadrunnerChipAndNum();
            if ( chipInNumAndrunnNum.Count != 0 ) {
                MessageBox.Show("資料載入成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                this.runnerInfoTableAdapter.Fill(this.dbDataSet.runnerInfo);
            }


        }

        public void fuckPlay() {
            Process p = new Process();
            p.StartInfo.FileName = @"F:\Download\GOPRO\MetroFramework\bin\Debug\ffmpeg-20160813-ceab04f-win64-static\ffmpeg-20160813-ceab04f-win64-static\bin\ffplay.exe";
            p.StartInfo.Arguments = "-an -fflags nobuffer -f:v mpegts -probesize 8192 rtp://10.5.5.9:8554";


            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.Start();
           
          
        }
        private void metroButton1_Click_3(object sender, EventArgs e) {
            fuckPlay();
            taskWhile = Task.Run(async () => {
                while ( booooooooooooooo ) {
                    await AccessTheWebAsync("http://10.5.5.9/gp/gpControl/execute?p1=gpStream&c1=restart");
                    System.Threading.Thread.Sleep(10000);
                }
            });


            //MessageBox.Show(taskWhile.Status.ToString());
            //taskWhile.Dispose();
        }

        private void metroButton2_Click(object sender, EventArgs e) {
            //booooooooooooooo = !booooooooooooooo;
        }

        private void setSenseTime_Click(object sender, EventArgs e) {
            try {
                //db.setSenseTime(Int32.Parse(senseTimeText.Text));
                senseTime = Int32.Parse(senseTimeText.Text);
            }
            catch { MessageBox.Show("請輸入正確"); }

        }

        private void button1_Click_1(object sender, EventArgs e) {

        }

        private void button1_Click_2(object sender, EventArgs e) {
            try {
                String year = yy.Value.ToString();
                String month = mm.Value.ToString();
                String day = dd.Value.ToString();
                String hour = h.Value.ToString();
                String min = m.Value.ToString();
                String sec = s.Value.ToString();

                DateTime hms = new DateTime(Int32.Parse(year),
                    Int32.Parse(month),
                    Int32.Parse(day),
                    Int32.Parse(hour),
                    Int32.Parse(min),
                    Int32.Parse(sec));

                ArrayList SectionTime = db.getSectionStratTime();


                //DateTime.Compare(runnerSocketInfo[chipInNum].AddSeconds(Form1.senseTime), now) != 1
                bool flag = false;
                DateTime temp = new DateTime();
                String currentCount = "";
                for ( int i = 0 ; i < SectionTime.Count ; i++ ) {
                    String tmpTime = SectionTime[i].ToString();
                    //宣告String
                    if ( DateTime.Compare(hms, DateTime.Parse(tmpTime)) == 1 ) {
                        temp = DateTime.Parse(tmpTime);
                        currentCount = db.getStratTime(i + 1)[1];
                        Console.WriteLine(SectionTime.Count);
                    }
                    else {

                        break;
                    }

                }              
                gp.searchVideo(hms.ToString(),
                    temp.ToString(),
                    currentCount,
                    GoProStatus.num_total_videos,
                    endTimeOffsetByT.Value.ToString(),
                    speedOffsetByT.Value.ToString());
            }
            catch ( Exception ) {

                MessageBox.Show("請輸入正確時間");
            }

            




        }

        private void DisplayError() {
            //UI
            //richTextBox2.AppendText("WIFI connect error!" + DateTime.Now.TimeOfDay.ToString() + "\n");
            TotalPhotoNumT.Text = "0";
            TotalVideoNumT.Text = "0";
            RemainingPhotoNumT.Text = "0";
            RemainingVideoNumT.Text = "0";
            modeT.Text = "--";

            ConnectButton.Text = "連接GoPro";
            ConnectButton.Enabled = true;

            //Timer Handle
            timerHandle.Enabled = false;
            timerHandle.Tick -= Application_Idle;

            searchBtn.Enabled = false;
        }
        //-------------------------------- end gopro delegate
    }
}
