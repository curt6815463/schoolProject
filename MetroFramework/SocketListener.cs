using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetroFramework
{
    class SocketListener
    {
        private Socket socket;
        private Thread Connectthread;
        public IPAddress ip;
        public Int32 port;
        public IPEndPoint ipPoint;
        DB db = new DB();
        private Label sumLabel;
        private Label currentLabel;
        private ListBox runnerList;




        public void setComp(Label sumLabel, Label currentLabel, ListBox runnerList) {
            this.sumLabel = sumLabel;
            this.currentLabel = currentLabel;
            this.runnerList = runnerList;
            db.setComp(currentLabel, runnerList);
        }
        public bool handleIPaddr(String ip_input, String port_input)
        {
            try
            {
                ip = IPAddress.Parse(ip_input);
                port = Int32.Parse(port_input);
                ipPoint = new IPEndPoint(ip, port);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool StopConnect()
        {
            try
            {
                if (socket != null)
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                if (Connectthread != null)
                {
                    Connectthread.Abort();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public bool StartConnect() //StartConnect
        {

            try
            {
                Ping ping = new Ping();  //先pingIP是否存在在做Socket Connect 也可設Socket Timeout
                PingReply r = ping.Send(ipPoint.Address, 3);
                if (r.Status == IPStatus.Success)
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //設定socket連接的格式
                    socket.Connect(ipPoint);//Connect
                    if (socket.IsBound)
                    {
                        Connectthread = new Thread(Show_Msg);  //New a Thread do Show_Msg()                  
                        Connectthread.Start();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                { return false; }
            }
            catch (Exception ex)
            {
                try
                {
                    socket.Close();
                    Connectthread.Abort();

                }
                catch
                { }
                return false;
            }
        }
        void Show_Msg(object cc)
        {

            while (true)  //Socket Listener
            {
                try
                {
                    byte[] bytes = new byte[1024 * 1024 * 2];   //buffer
                    int length = socket.Receive(bytes);
                    string Msg = System.Text.Encoding.ASCII.GetString(bytes, 0, length);  //Encode   
                    //Console.WriteLine(Msg);              
                    Receive_Show(Msg);
                }
                catch (Exception ex)
                {
                    StopConnect();                   
                }
            }
        }
        string tmp = "";
        void Receive_Show(string Msg)
        {
            try
            {
                if (Msg.Length == 38 && !Msg.Contains("\0"))    //Msg will be "aa0058xxxxxxxxxxxxxxxxxxxxxx"  38bytes
                {
                    if ( LRCCheck(Msg) ) {
                        db.writeTime(Msg);
                        processData(Msg);
                    }//LRC CHECK
                       
                }
                else
                {
                    if (Msg != "" && !Msg.Contains("\0"))   //Msg will be "aa0058xxxxxxxxxxx" or "XXXXXXXXXXXX"
                    {
                        if (Msg.Contains("\r\n"))
                        {
                            if (tmp.Length + Msg.IndexOf("\r\n") < 36)  //if temp="" and Msg = "0058xxxxxxxxxxxxxxxxxx/r/naa0058xxxxxxxxxxxxxxxxx/r/n"
                            {
                                Msg = Msg.Substring(Msg.IndexOf("\r\n") + 2);
                                tmp = "";
                            }
                        }
                        tmp += Msg;  //Merge  "aa0058xxxxxxxxxxx" + "XXXXXXXXXXXX"  
                    }
                    if (tmp.Length == 38)
                    {

                        if ( LRCCheck(tmp) ) {
                            db.writeTime(tmp);
                            processData(tmp);
                        }
                            
                        tmp = "";
                    }
                    if (tmp.Length > 38)    //over 38bytes Msg will be "aa0058xxxxxxxxxxxxxxxxxxxxx\r\naa0058xxxxxxxxxxxxxxxxxxx/r/naa0058xxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
                    {
                        int len = tmp.Length;
                        for (int i = 0; i < len / 38; i++)
                        {
                            string spilit_str = tmp.Substring(0, 38);
                            if (LRCCheck(spilit_str)) {
                                db.writeTime(spilit_str);
                                processData(spilit_str);
                            }
                                
                            tmp = tmp.Substring(38, tmp.Length - 38);
                        }
                    }
                    if (Msg.Contains("\0"))
                    {
                        Msg.Replace("\0", "");
                    }
                    if (Msg.Contains("?"))
                    {
                        Msg = "";
                    }
                }
            }
            catch (Exception ex)
            {
                Msg = "";
                tmp = "";
            }
        }

        private void processData(string payload) {
            payload = payload.Substring(payload.IndexOf("059"), 12);
            lock ( this ) {
                SetLabel(sumLabel,(++Form1.Read_record).ToString());             
            }
            SetLabel(currentLabel, Form1.chipInNumAndrunnNum[payload].ToString());
            SetListBox(runnerList, Form1.chipInNumAndrunnNum[payload].ToString());
        }

        private delegate void SetListBoxCallback(ListBox lb, String msg);
        private void SetListBox(ListBox lb, String msg) {
            if ( lb.InvokeRequired ) {
                SetListBoxCallback d = new SetListBoxCallback(SetListBox);
                lb.Invoke(d, new object[] { lb,msg });
            }
            else {
                if(!lb.Items.Contains(msg))
                    lb.Items.Add(msg);
            }

        }

        private delegate void SetLabelCallback(Label lb,String msg);
        private void SetLabel(Label lb, String msg) {
            if ( lb.InvokeRequired ) {
                SetLabelCallback d = new SetLabelCallback(SetLabel);
                lb.Invoke(d, new object[] { lb, msg});
            }
            else {
                lb.Text = msg;
            }

        }
        //private void SetTextBox(TextBox tb, string Msg) {
        //    if ( tb.InvokeRequired ) {
        //        SetLabelCallback d = new SetLabelCallback(SetTextBox);
        //        tb.Invoke(d, new object[] { tb, Msg });
        //    }
        //    else {
        //        tb.Text = Msg;
        //    }
        //}

        public bool LRCCheck(string payload)
        {
            int LRC = 0;
            try
            {
                payload.Substring(2, 32).ToList().ForEach(x => LRC += x);
                return Convert.ToString(LRC, 16).Substring(1).Equals(payload.Substring(34, 2));
            }
            catch
            {
                return false;
            }
        }


        public bool checkConnect()
        {
            try
            {
                if (socket.Connected)
                {
                    byte[] bytes = System.Text.Encoding.ASCII.GetBytes("HiAreUHere");
                    socket.Send(bytes);
                    return true;
                }
                else
                {
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
