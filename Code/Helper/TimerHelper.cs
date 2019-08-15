using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timer.Helper
{
    /// <summary>
    /// 此类负责倒计时器的运行，根据设置的参数进行启动和暂停计时器，并将剩余时间等信息返回
    /// </summary>
    class TimerHelper
    {
        /// <summary>
        /// 计时器剩余的时间，折算为秒数
        /// </summary>
        public int TimeLeft { get; set; }

        /// <summary>
        /// 计时器超时的时间，折算为秒数
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="totalseconds"></param>
        public TimerHelper(int seconds)
        {
            this.TimeLeft = seconds;
            this.TimeOut = -1;
        }

        /// <summary>
        /// 倒计时，秒数递减，并返回字符串显示到屏幕上
        /// </summary>
        /// <returns>返回格式化的剩余时间，格式为 “00:00” </returns>
        public string PassTime()
        {
            this.TimeLeft -= 1;
            return string.Format("{0:D2}", (this.TimeLeft) / 60) + ":" + string.Format("{0:D2}", this.TimeLeft % 60);
        }

        /// <summary>
        /// 如果设置了超时计算，那么当时间为0时会开始从0开始递增
        /// </summary>
        /// <returns>返回格式化的时间，格式为 “00:00:00” </returns>
        public string OverTime()
        {
            this.TimeOut += 1;
            if (TimeOut > 86400)
            {
                return "已超时 超过24小时";
            }
            return "已超时 " + string.Format("{0:D2}", (this.TimeOut / 3600)) + ":" + string.Format("{0:D2}", (this.TimeOut % 3600) / 60) + ":" + string.Format("{0:D2}", this.TimeOut % 60);
        }
    }
}
