using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroFramework {
    class FFmpeg {
        Process p;
        public FFmpeg() {
            p = new Process();
        }
        public void showScreen() {
            p.StartInfo.FileName = @"C:\Users\lohas\Desktop\cam_batter\ffmpeg-20160813-ceab04f-win64-static\ffmpeg-20160813-ceab04f-win64-static\bin\ffplay.exe";
            p.StartInfo.Arguments = "-an -fflags nobuffer -f:v mpegts -probesize 8192 rtp://10.5.5.9:8554";
            p.Start(); 
            p.WaitForExit();
            p.Close();
        }

        //public void createPhoto(int item, int startCatchPoint, String path, ArrayList videoList, String rNum) {
        //    Console.WriteLine(item);
        //    Console.WriteLine(startCatchPoint);
        //    Console.WriteLine(rNum);
        //    p.StartInfo.FileName = @"C:\Users\lohas\Desktop\cam_batter\ffmpeg-20160813-ceab04f-win64-static\ffmpeg-20160813-ceab04f-win64-static\bin\ffmpeg.exe";
        //    //ffmpegP.StartInfo.Arguments = "-ss 50 -i http://10.5.5.9/videos/DCIM/100GOPRO/G0011615.MP4 -y -r 1 -t 1 asdf.jpg";
        //    p.StartInfo.Arguments = " -ss " + startCatchPoint + " -i " + path + videoList[item] + " -y -r 1 -t 1 " + rNum + ".jpg";

        //    p.StartInfo.UseShellExecute = false;
        //    p.StartInfo.CreateNoWindow = true;
        //    p.StartInfo.RedirectStandardOutput = false;

        //    p.Start(); // 執行 !
        //    p.WaitForExit();
        //    p.Close();
        //}
    }
}
