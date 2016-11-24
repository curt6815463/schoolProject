using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroFramework {
    class GoProStatus_class {
        public string internal_battery_present { get; set; }
        public string internal_battery_level { get; set; }
        public string external_battery_present { get; set; }
        public string external_battery_level { get; set; }
        public string system_hot { get; set; }
        public string system_busy { get; set; }
        public string quick_capture_active { get; set; }
        public string encoding_active { get; set; }
        public string lcd_lock_active { get; set; }
        public string video_progress_counter { get; set; }
        public string broadcast_progress_counter { get; set; }
        public string broadcast_viewers_count { get; set; }
        public string broadcast_bstatus { get; set; }
        public string enable { get; set; }
        public string state { get; set; }
        public string type { get; set; }
        public string pair_time { get; set; }
        public string stateQ { get; set; }
        public string scan_time_msec { get; set; }
        public string provision_status { get; set; }
        public string remote_control_version { get; set; }
        public string remote_control_connected { get; set; }
        public string pairing { get; set; }
        public string wlan_ssid { get; set; }
        public string ap_ssid { get; set; }
        public string app_count { get; set; }
        public string enableQ { get; set; }
        public string sd_status { get; set; }
        public string remaining_photos { get; set; }
        public string remaining_video_time { get; set; }
        public string num_group_photos { get; set; }
        public string num_group_videos { get; set; }
        public string num_total_photos { get; set; }
        public string num_total_videos { get; set; }
        public string date_time { get; set; }
        public string ota_status { get; set; }
        public string download_cancel_request_pending { get; set; }
        public string mode { get; set; }
        public string sub_mode { get; set; }
        public string camera_locate_active { get; set; }
        public string video_protune_default { get; set; }
        public string photo_protune_default { get; set; }
        public string multi_shot_protune_default { get; set; }
        public string multi_shot_count_down { get; set; }
        public string remaining_space { get; set; }
        public string supported { get; set; }
        public string wifi_bars { get; set; }
        public string current_time_msec { get; set; }
        public string num_hilights { get; set; }
        public string last_hilight_time_msec { get; set; }
        public string next_poll_msec { get; set; }
        public string analytics_ready { get; set; }
        public string analytics_size { get; set; }
        public string in_contextual_menu { get; set; }
        public string remaining_timelapse_time { get; set; }
    }
}
