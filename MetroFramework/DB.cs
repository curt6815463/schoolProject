using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetroFramework
{
    class DB
    {
        public int senseTime = 600;
        private SqlConnection con;
        private SqlCommand cmd;
        private SqlDataReader dataR;
        private ArrayList firstInList = new ArrayList(); //用來判斷是否第一次通過終點
        private ArrayList selectedRNum = new ArrayList(); //用來存放產生照片名單的NUM
        private ArrayList selectedRTime = new ArrayList(); //用來存放產生照片名單的Time
        private Dictionary<String, DateTime> runnerSocketInfo = new Dictionary<String, DateTime>();
        
        public DB()
        {
            con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=F:\Download\GOPRO\MetroFramework\db.mdf;Integrated Security=True");
            try {
                con.Open();
            }
            catch ( Exception e ) {
                Console.WriteLine("資料庫連線失敗");
                throw e;
            }
        }
        
        public void setSenseTime(int senseTime) {
            this.senseTime = senseTime;
        }     
        
        private void disConnectDB()
        {
            con.Close();
        }

        public ArrayList getSectionStratTime() //取得時間區段的每個時間LIST
        {
            ArrayList sectionTime = new ArrayList(); 
            SqlCommand cmdSec = new SqlCommand("select startRecordTime from time", con);
            dataR = cmdSec.ExecuteReader();
            while ( dataR.Read() ) {
                sectionTime.Add(dataR["startRecordTime"].ToString());
            }
            dataR.Close();
            return sectionTime;
        }


        public void setStartTime(int timeID ,DateTime startTime, int count) //設定開錄時間
        {          
            cmd = new SqlCommand("INSERT INTO time VALUES(@timeID,@startTime,@count)", con);
            cmd.Parameters.Add("@timeID", timeID);
            cmd.Parameters.Add("@startTime", startTime);
            cmd.Parameters.Add("@count", count);
            cmd.ExecuteNonQuery();
        }
        public void dropTimeDB() 
        {
            SqlCommand deleteCMD = new SqlCommand("TRUNCATE TABLE time", con);
            deleteCMD.ExecuteNonQuery();
        }

        public void clearStartTime()
        {
         
            cmd = new SqlCommand("UPDATE startTime SET time = null where Id = 1", con);
            cmd.ExecuteNonQuery();
           
        }
        public Dictionary<String, String> loadrunnerChipAndNum() {
            SqlCommand lcan = new SqlCommand("select * from runnerInfo",con);
            Dictionary<String, String> runnerChipAndNum = new Dictionary<String, String>();
            
            try {
                dataR = lcan.ExecuteReader();
                if(dataR.HasRows) {
                    while ( dataR.Read() ) {
                        runnerChipAndNum.Add(dataR["chipInNum"].ToString(), dataR["runnerNum"].ToString());
                    }
                }                
                dataR.Close();

            }
            catch (Exception e) {
                Console.WriteLine("讀取失敗");
            }

            return runnerChipAndNum;
        }


        public String[] getStratTime(int timeId) //取得起跑時間
        {
            String[] timeInfo = new String[2];
            cmd = new SqlCommand("select * from time where timeId = @timeId", con);
            cmd.Parameters.Add("@timeId", timeId);
            dataR = cmd.ExecuteReader();
            if ( dataR.Read() )
            {
                DateTime dt = (DateTime)dataR["startRecordTime"];
                int count = (int)dataR["videoCount"];
                timeInfo[0] = (dt.ToString("yyyy-MM-dd HH:mm:ss.ff"));
                timeInfo[1] = ( count.ToString());
                //timeInfo. = dt.ToString("yyyy-MM-dd HH:mm:ss.ff");    //先取資料後關閉資源
            }
            //else
            //{
            //    startTime = "";
            //}
            dataR.Close();    
            return timeInfo;
        }

        public void updateRunnerTime(DateTime now, String chipInNum)
        {       
            cmd = new SqlCommand("UPDATE runnerInfo SET runnerFinish = @now, runnerCamStartTime = @st WHERE chipInNum = @ChipInNum", con);
            cmd.Parameters.Add("@ChipInNum", chipInNum);
            cmd.Parameters.Add("@now", now);
            cmd.Parameters.Add("@st",1);
            cmd.ExecuteNonQuery();    
        }

        public void writeTime(String socket)  //將收到的跑者socket時間資料寫入DB
        {
            DateTime now = DateTime.Now;
            string chipInNum = socket.Substring(socket.IndexOf("059"), 12); //get chipID
            if (!runnerSocketInfo.ContainsKey(chipInNum))
            {
                runnerSocketInfo.Add(chipInNum,now);
                updateRunnerTime(now,chipInNum);
            }
            else {
                if ( DateTime.Compare(runnerSocketInfo[chipInNum].AddSeconds(Form1.senseTime), now) != 1 ) {
                   
                    updateRunnerTime(now, chipInNum);
                    runnerSocketInfo[chipInNum] = now;
                    SetListBox(runnerList, Form1.chipInNumAndrunnNum[chipInNum].ToString() );
                }
               
            }
        }

        public Label currentLabel;
        public ListBox runnerList;

        public void setComp( Label currentLabel, ListBox runnerList) {         
            this.currentLabel = currentLabel;
            this.runnerList = runnerList;
        }

        private delegate void SetListBoxCallback(ListBox lb, String msg);
        private void SetListBox(ListBox lb, String msg) {
            if ( lb.InvokeRequired ) {
                SetListBoxCallback d = new SetListBoxCallback(SetListBox);
                lb.Invoke(d, new object[] { lb, msg });
            }
            else {
                //if ( !lb.Items.Contains(msg) )
                    lb.Items.Add(msg);
            }

        }

        private delegate void SetLabelCallback(Label lb, String msg);
        private void SetLabel(Label lb, String msg) {
            if ( lb.InvokeRequired ) {
                SetLabelCallback d = new SetLabelCallback(SetLabel);
                lb.Invoke(d, new object[] { lb, msg });
            }
            else {
                lb.Text = msg;
            }

        }

        public String[] getRunnerInfo(String Num) //取得跑者資料秀在form上
        {
            String[] runnerInfo = new String[4];
            cmd = new SqlCommand("select * from runnerInfo where runnerNum = " + Num, con);
            dataR = cmd.ExecuteReader();
            try
            {
                if( dataR.Read() ) {
                    runnerInfo[0] = dataR["runnerName"].ToString();
                    runnerInfo[1] = dataR["chipOutNum"].ToString();
                    runnerInfo[2] = dataR["runnerFinish"].ToString();
                    runnerInfo[3] = dataR["runnerCamStartTime"].ToString();
                }else {
                    dataR.Close();
                    return null;
                }
                
                
            }
            catch (Exception e)
            {
                runnerInfo[0] = "error";
                runnerInfo[1] = "error";
                runnerInfo[2] = "error";
                runnerInfo[3] = "error";
                
            }
            finally {
                dataR.Close();
            }

            return runnerInfo; ;
        }

        public void createPhotoList()
        {
            cmd = new SqlCommand("select runnerNum,finishTime from runnerInfo where finishTime is not NULL", con);
            dataR = cmd.ExecuteReader();
            selectedRNum.Clear();
            selectedRTime.Clear();
            if (dataR.HasRows)
            {
                while (dataR.Read())
                {
                    String runnerNum = dataR["runnerNum"].ToString();
                    //String finishTime = dataR["finishTime"].ToString();
                    DateTime dt = (DateTime)dataR["finishTime"];
                    String finishTime = dt.ToString("yyyy-MM-dd HH:mm:ss.ff");

                    selectedRTime.Add(finishTime);
                    selectedRNum.Add(runnerNum);

                }
            }
            else { Console.WriteLine("no"); }
            disConnectDB();
            dataR.Close();
        }

        //public void createPhotoList(int flag)
        //{
        //    DateTime down = DateTime.Parse(startTime).AddSeconds(flag * 300);
        //    DateTime up = DateTime.Parse(startTime).AddSeconds((flag + 1) * 300);
        //    //Console.WriteLine(down);
        //    //Console.WriteLine(up.ToString());
        //    String dateDown = down.Year + "-" + down.Month + "-" + down.Day + " " + down.Hour + ":" + down.Minute + ":" + down.Second + "." + down.Millisecond;
        //    String dateUp = up.Year + "-" + up.Month + "-" + up.Day + " " + up.Hour + ":" + up.Minute + ":" + up.Second + "." + up.Millisecond;
        //    Console.WriteLine(dateDown);
        //    Console.WriteLine(dateUp);
        //    cmd.Parameters.Add("@down", dateDown);
        //    cmd.Parameters.Add("@up", dateUp);

        //    cmd = new SqlCommand("select runnerNum,finishTime from runnerInfo where finishTime >= '" + dateDown + "' and finishTime < '" + dateUp + "'", con);
        //    dataR = cmd.ExecuteReader();
        //    selectedRNum.Clear();
        //    selectedRTime.Clear();
        //    if (dataR.HasRows)
        //    {
        //        while (dataR.Read())
        //        {
        //            String runnerNum = dataR["runnerNum"].ToString();
        //            //String finishTime = dataR["finishTime"].ToString();
        //            DateTime dt = (DateTime)dataR["finishTime"];
        //            String finishTime = dt.ToString("yyyy-MM-dd HH:mm:ss.ff");

        //            selectedRTime.Add(finishTime);
        //            selectedRNum.Add(runnerNum);

        //        }
        //    }
        //    else { Console.WriteLine("no"); }
        //    disConnectDB();
        //    dataR.Close();
        //}

        //public void updatePhotoCheck(int runnerNum)
        //{
        //    connectDB();
        //    cmd = new SqlCommand("UPDATE runnerInfo SET photoCheck = 1 where runnerNum = @runnerNum", con);
        //    cmd.Parameters.Add("@runnerNum", runnerNum);
        //    cmd.ExecuteNonQuery();
        //    disConnectDB();
        //}
        //public ArrayList getSelectRTime()
        //{
        //    return selectedRTime;
        //}

        //public ArrayList getSelectRNum()
        //{
        //    return selectedRNum;
        //}

        private string cl_ms(string ms)
        {
            try
            {
                return Convert.ToInt32(ms, 16).ToString("0#").Substring(0, 2);
            }
            catch (Exception ex)
            {
                return "00";
            }
        }
    }
}


