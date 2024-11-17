using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Timer.Helper
{
    /// <summary>
    /// 此类用来保存用户的设置信息，比如上次设置的倒计时时间、是否开启全屏铃声和超时等功能
    /// </summary>
    public static class SettingHelper
    {
        /// <summary>
        /// 保存用户设定是否开启铃声提醒
        /// </summary>
        public static bool IsReminderOn { get; set; }

        /// <summary>
        /// 保存用户设定是否全屏
        /// </summary>
        public static bool IsFullscreenOn { get; set; }

        /// <summary>
        /// 保存用户设定是否允许超时
        /// </summary>
        public static bool IsTimeoutOn { get; set; }

        /// <summary>
        /// 保存用户最近一次设置的倒计时
        /// </summary>
        public static int ResetTime { get; set; }

        /// <summary>
        /// 加载配置文件，将之前保存的设置都读取出来
        /// </summary>
        public static void LoadSettingConfig()
        {
            string isReminderOn = "True";
            string isFullscreenOn = "False";
            string isTimeoutOn = "False";
            string resetTime = "0";

            try
            {
                isReminderOn = ConfigurationManager.AppSettings["IsReminderOn"];
                isFullscreenOn = ConfigurationManager.AppSettings["IsFullscreenOn"];
                isTimeoutOn = ConfigurationManager.AppSettings["IsTimeoutOn"];
                resetTime = ConfigurationManager.AppSettings["ResetTime"];
            }
            catch
            { }
            bool result = false;
            bool.TryParse(isReminderOn, out result);
            SettingHelper.IsReminderOn = result;
            bool.TryParse(isFullscreenOn, out result);
            SettingHelper.IsFullscreenOn = result;
            bool.TryParse(isTimeoutOn, out result);
            SettingHelper.IsTimeoutOn = result;
            SettingHelper.ResetTime = Convert.ToInt32(resetTime);
        }

        /// <summary>
        /// 将设置保存到配置文件
        /// </summary>
        public static void SaveSettingConfig()
        {
            Configuration configuration;
            try
            {
                configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }
            catch { return; }


            System.Diagnostics.Debug.WriteLine(SettingHelper.IsReminderOn.ToString());

            configuration.AppSettings.Settings["IsReminderOn"].Value = SettingHelper.IsReminderOn.ToString();
            configuration.AppSettings.Settings["IsFullscreenOn"].Value = SettingHelper.IsFullscreenOn.ToString();
            configuration.AppSettings.Settings["IsTimeoutOn"].Value = SettingHelper.IsTimeoutOn.ToString();
            configuration.AppSettings.Settings["ResetTime"].Value = SettingHelper.ResetTime.ToString();
            configuration.Save();
        }
    }
}
