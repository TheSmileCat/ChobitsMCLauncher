using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChobitsMCLauncher.Tools.CheckUpdate
{
    class UpdaterEvent : EventArgs
    {
        /// <summary>
        /// 事件携带的信息
        /// </summary>
        public string message { get; private set; } = "";
        /// <summary>
        /// 当前数量
        /// </summary>
        public double nowSum { get; private set; } = 0;
        /// <summary>
        /// 合计数量
        /// </summary>
        public double sum { get; private set; } = 0;
        /// <summary>
        /// 进度，在0~1之间
        /// </summary>
        public double process { get; private set; } = 0.0;
        public enum MessageStatus
        {
            Info,
            Failed,
            Done,
            Download
        }
        /// <summary>
        /// 状态
        /// </summary>
        public MessageStatus status { get; private set; } = MessageStatus.Info;
        /// <summary>
        /// 初始化更新事件，用于通知通知信息
        /// </summary>
        /// <param name="message">事件信息</param>
        /// <param name="status">事件状态</param>
        public UpdaterEvent(string message, MessageStatus status = MessageStatus.Info)
        {
            this.message = message;
            this.status = status;
        }
        /// <summary>
        /// 初始化更新事件，用于通知通知信息
        /// </summary>
        /// <param name="now">当先已完成的数量</param>
        /// <param name="sum">总数量</param>
        /// <param name="message">事件信息</param>
        /// <param name="status">事件状态</param>
        public UpdaterEvent(double now, double sum, string message, MessageStatus status = MessageStatus.Info) : this(message, status)
        {
            if (now < 0) throw new NotFiniteNumberException("数字区间不合理，进度值必须大于0", now);
            if (sum < now) throw new NotFiniteNumberException(string.Format("数字区间不合理，进度值不能小于总值，now为{0}，sum为{1}", now, sum));
            this.nowSum = now;
            this.sum = sum;
            this.process = (double)now / sum;
        }
        /// <summary>
        /// 初始化更新事件，用于通知通知信息
        /// </summary>
        /// <param name="process">操作进度</param>
        /// <param name="now">当先已完成的数量</param>
        /// <param name="sum">总数量</param>
        /// <param name="message">事件信息</param>
        /// <param name="status">事件状态</param>
        public UpdaterEvent(double process, double now, double sum, string message, MessageStatus status = MessageStatus.Info) : this(now, sum, message, status)
        {
            if (process > 1 || process < 0) throw new NotFiniteNumberException("数字区间不合理，只能在0~1之间", process);
            this.process = process;
        }
    }
}
