using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChobitsMCLauncher.ProgramWindows.Client
{
    public partial class ClientMainWindow : Window{
        public void UpdateStatus(string message, double process)
        {
            Dispatcher.Invoke(() =>
            {
                processInfo.Content = message;
                Thickness thickness = processInfo.Margin;
                processInfo.Margin = thickness;
                if (process < 0 || process > 1)
                {
                    processing.IsIndeterminate = true;
                    ProgressBarHelper.SetIsPercentVisible(processing, false);
                }
                else
                {
                    processing.IsIndeterminate = false;
                    processing.Value = process;
                    ProgressBarHelper.SetIsPercentVisible(processing, true);
                }
            });
        }
        public static void UpdateStatusStatic(string message, double process = -1)
        {
            if (GetWindow() != null) GetWindow().UpdateStatus(message, process);
        }
        private static bool isWaiting = true;
        /// <summary>
        /// 设置按钮的文字
        /// </summary>
        /// <param name="text">按钮显示的文字</param>
        /// <param name="waiting">是否等待，设置为null时显示位原始状态</param>
        public void UpdateButton(string text, bool? waiting = null)
        {
            if (waiting.HasValue) IsWaiting(waiting.Value);
            else IsWaiting(ButtonHelper.GetIsWaiting(LaunchButton));
            void IsWaiting(bool w)
            {
                if (w)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ButtonHelper.SetWaitingContent(LaunchButton, text);
                        ButtonHelper.SetIsWaiting(LaunchButton, true);
                        TabItemChangeButton.IsEnabled = false;
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        LaunchButton.Content = text;
                        ButtonHelper.SetIsWaiting(LaunchButton, false);
                        TabItemChangeButton.IsEnabled = true;
                    });
                }
                isWaiting = waiting.Value;
            }
        }

        /// <summary>
        /// 设置按钮的文字
        /// </summary>
        /// <param name="text">按钮显示的文字</param>
        /// <param name="waiting">是否等待，设置为null时显示位原始状态</param>
        public static void UpdateButtonStatic(string text, bool? waiting = null)
        {
            if (GetWindow() != null) GetWindow().UpdateButton(text, waiting);
            if (waiting.HasValue)
            {
                isWaiting = waiting.Value;
            }
        }
    }
}
