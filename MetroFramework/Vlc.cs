using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroFramework {
    class Vlc {
        Process p;
        public Vlc() {
            p = new Process();
        }
        public void play(double item, double startPlayT, ArrayList videoList, String path,String videoCount,String speedRate) {
            p.StartInfo.FileName = @"F:\VideoLAN\VLC\vlc.exe"; //http://10.5.5.9/videos/DCIM/100GOPRO/G0011355.MP4
            p.StartInfo.Arguments = path + videoList[(int)item+ Int32.Parse(videoCount)] + " --start-time " + startPlayT + " --rate " + speedRate;
            p.Start();
            p.WaitForExit();
            p.Close();
        }
    }
}
