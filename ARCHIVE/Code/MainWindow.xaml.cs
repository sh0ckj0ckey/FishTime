using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Timer.Helper;
using TumblerTimePicker;

namespace Timer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 标识计时器当前的状态
        /// </summary>
        enum TimerStatus
        {
            /// <summary>
            /// 准备开始
            /// </summary>
            Start,
            /// <summary>
            /// 正在运行
            /// </summary>
            Running,
            /// <summary>
            /// 暂停
            /// </summary>
            Paused,
            /// <summary>
            /// 计时结束
            /// </summary>
            End,
            /// <summary>
            /// 正在超时计算
            /// </summary>
            Overtime
        }

        /// <summary>
        /// 标识当前显示模式
        /// </summary>
        enum DisplayStyle
        {
            /// <summary>
            /// 全屏模式
            /// </summary>
            Fullscreen,
            /// <summary>
            /// 精简模式
            /// </summary>
            Lite,
            /// <summary>
            /// 普通模式
            /// </summary>
            Normal
        }

        TimerHelper timerHelper;

        /// <summary>
        /// 系统计时器
        /// </summary>
        System.Threading.Timer timer;

        /// <summary>
        /// 点击暂停后启动，这一秒结束后暂停，记录暂停的这一秒进行到了多少毫秒
        /// </summary>
        Stopwatch stopwatch;

        long pauseDelay = 1000;

        /// <summary>
        /// 记录当前计时器的状态
        /// </summary>
        TimerStatus timerStatus = TimerStatus.Start;

        /// <summary>
        /// 进入全屏的时间(延迟三秒进入)
        /// </summary>
        int fullscreenTime = -1;

        /// <summary>
        /// 界面无操作三秒后隐藏
        /// </summary>
        int disappearTime = -1;

        /// <summary>
        /// 用于控制所有控件可见性
        /// </summary>
        UIControlHelper globalUIControl;

        public MainWindow()
        {
            InitializeComponent();

            globalUIControl = new UIControlHelper();
            this.DataContext = globalUIControl;

            InitializeSettings();
            InitializeTumbler();
            stopwatch = new Stopwatch();
        }

        /// <summary>
        /// 初始化滚轮控件，绑定选项列表
        /// </summary>
        private void InitializeTumbler()
        {
            List<TumblerData> tumblerDatas;
            List<string> min = new List<string>();
            for (int i = 0; i <= 60; ++i)
            {
                min.Add(string.Format("{0:d2}", i));
            }

            List<string> sec = new List<string>();
            for (int i = 0; i <= 5; ++i)
            {
                sec.Add(string.Format("{0:d2}", i * 10));
            }

            int selectedMinIndex = SettingHelper.ResetTime / 60;
            int selectedSecIndex = (SettingHelper.ResetTime % 60) / 10;

            tumblerDatas = new List<TumblerData>
            {
                new TumblerData(min, selectedMinIndex, ""),
                new TumblerData(sec, selectedSecIndex, "")
            };

            TimepickerControl.Tumblers = tumblerDatas;
        }

        /// <summary>
        /// 初始化用户设置，将按钮都设置成用户上次使用的样子
        /// </summary>
        private void InitializeSettings()
        {
            SettingHelper.LoadSettingConfig();
            SwitchReminder(SettingHelper.IsReminderOn);
            SwitchFullscreen(SettingHelper.IsFullscreenOn);
            SwitchTimeout(SettingHelper.IsTimeoutOn);
        }

        /// <summary>
        /// 回到开始计时的界面，隐藏时间和全屏时间归-1，重新判断是否需要全屏
        /// </summary>
        private void InitializeAndStart()
        {
            fullscreenTime = -1;
            disappearTime = -1;
            SwitchButtonsVisibility(TimerStatus.Running);

            if (!SettingHelper.IsFullscreenOn)
            {
                SwitchDisplayStyle(DisplayStyle.Lite);
            }
            else
            {
                SwitchDisplayStyle(DisplayStyle.Fullscreen);
            }
        }

        /// <summary>
        /// 处理主计时器过去1秒钟后要做的事情
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OneSecondPass(object o)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (timerHelper.TimeLeft > 0)
                {
                    // 计时
                    globalUIControl.LeftTime = timerHelper.PassTime();

                    stopwatch.Restart();

                    // 判断是否应该进入全屏或者精简模式
                    if (SettingHelper.IsFullscreenOn)
                    {
                        if (timerHelper.TimeLeft == fullscreenTime && timerHelper.TimeOut == -1)
                        {
                            SwitchDisplayStyle(DisplayStyle.Fullscreen);
                            fullscreenTime = -1;
                        }
                    }
                    else
                    {
                        if (timerHelper.TimeLeft == disappearTime)
                        {
                            SwitchDisplayStyle(DisplayStyle.Lite);
                            disappearTime = -1;
                        }
                    }

                    // 防止在进入Lite的瞬间点击暂停导致界面错乱的代码，待验证
                    if (timerStatus == TimerStatus.Paused)
                    {
                        SwitchDisplayStyle(DisplayStyle.Normal);
                        SwitchButtonsVisibility(TimerStatus.Paused);
                    }
                }
                else
                {
                    // 结束和超时
                    globalUIControl.LeftTime = "计时结束";
                    stopwatch.Stop();

                    // 判断是否需要播放铃声
                    if (SettingHelper.IsReminderOn && timerHelper.TimeOut == 0)
                    {
                        SoundPlayer player = new SoundPlayer
                        {
                            SoundLocation = @"../../Assets/reminder.wav"
                        };
                        try
                        {
                            player.LoadAsync();
                            player.Play();
                        }
                        catch (TimeoutException)
                        {
                            MessageBox.Show("请求超时", "ERROR");
                        }
                        catch (System.IO.FileNotFoundException)
                        {
                            MessageBox.Show("音频文件错误", "ERROR");
                        }
                        catch (InvalidOperationException)
                        {
                            Debug.WriteLine("方法调用对于对象的当前状态无效");
                            MessageBox.Show("播放失败", "ERROR");
                        }
                    }

                    // 判断是否开启了全屏，没开的话要改为普通模式而非精简模式
                    if (!SettingHelper.IsFullscreenOn)
                    {
                        SwitchDisplayStyle(DisplayStyle.Normal);
                    }
                    else
                    {
                        SwitchDisplayStyle(DisplayStyle.Fullscreen);
                    }

                    // 判断是否需要计算超时
                    if (!SettingHelper.IsTimeoutOn)
                    {
                        if (timer != null)
                        {
                            timer.Dispose();
                            timer = null;
                        }
                        SwitchButtonsVisibility(TimerStatus.End);
                    }
                    else
                    {
                        TimeOutTextBlock.Text = timerHelper.OverTime();

                        // 超过24小时就停止
                        if (timerHelper.TimeOut > 86400)
                        {
                            if (timer != null)
                            {
                                timer.Dispose();
                                timer = null;
                            }
                        }

                        SwitchButtonsVisibility(TimerStatus.Overtime);
                    }
                }
            }), null);
        }

        #region 操作区按钮
        /// <summary>
        /// 点击“开始计时”
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartTimerButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedValuesList = TimepickerControl.SelectedValues;
            int min = Convert.ToInt32(selectedValuesList[0]);
            int sec = Convert.ToInt32(selectedValuesList[1]);

            SettingHelper.ResetTime = min * 60 + sec;

            timerHelper = new TimerHelper(SettingHelper.ResetTime + 1);

            OneSecondPass(null);
            InitializeAndStart();
            timer = new System.Threading.Timer(OneSecondPass, null, 1000, 1000);
            stopwatch.Restart();
        }

        /// <summary>
        /// 点击“暂停”倒计时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PauseTimerButton_Click(object sender, RoutedEventArgs e)
        {
            stopwatch.Stop();
            pauseDelay = 1000 - stopwatch.ElapsedMilliseconds;

            Debug.WriteLine(stopwatch.ElapsedMilliseconds);

            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }

            // 修改滚轮列表内容，“秒”的某一项要变化为当前时间
            int selectedMinIndex = Convert.ToInt32(globalUIControl.LeftTime.Substring(0, 2));
            int selectedSecIndex = Convert.ToInt32(globalUIControl.LeftTime[3].ToString());

            List<string> min = new List<string>();
            for (int i = 0; i <= 60; ++i)
            {
                min.Add(string.Format("{0:d2}", i));
            }

            List<string> sec = new List<string>();
            for (int i = 0; i <= 5; ++i)
            {
                sec.Add(string.Format("{0:d2}", i * 10));
            }
            sec[selectedSecIndex] = globalUIControl.LeftTime.Substring(3, 2);

            TimepickerControl.Tumblers[0].Values = min;
            TimepickerControl.Tumblers[1].Values = sec;
            TimepickerControl.Tumblers[0].SelectedValueIndex = selectedMinIndex;
            TimepickerControl.Tumblers[1].SelectedValueIndex = selectedSecIndex;

            if (!SettingHelper.IsFullscreenOn)
            {
                SwitchDisplayStyle(DisplayStyle.Normal);
            }
            SwitchButtonsVisibility(TimerStatus.Paused);
        }

        /// <summary>
        /// 点击“继续”倒计时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContinueTimerButton_Click(object sender, RoutedEventArgs e)
        {
            timer = new System.Threading.Timer(OneSecondPass, null, pauseDelay, 1000);
            stopwatch.Start();
            pauseDelay = 1000;
            InitializeAndStart();
        }

        /// <summary>
        /// 点击“重设”计时器，时间重置，按钮变回“开始计时”，计时停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetTimerButton_Click(object sender, RoutedEventArgs e)
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }

            // 将滚轮列表内容重置
            int selectedMinIndex = Convert.ToInt32(SettingHelper.ResetTime / 60);
            int selectedSecIndex = Convert.ToInt32((SettingHelper.ResetTime % 60) / 10);

            List<string> min = new List<string>();
            for (int i = 0; i <= 60; ++i)
            {
                min.Add(string.Format("{0:d2}", i));
            }

            List<string> sec = new List<string>();
            for (int i = 0; i <= 5; ++i)
            {
                sec.Add(string.Format("{0:d2}", i * 10));
            }

            TimepickerControl.Tumblers[0].Values = min;
            TimepickerControl.Tumblers[1].Values = sec;
            TimepickerControl.Tumblers[0].SelectedValueIndex = selectedMinIndex;
            TimepickerControl.Tumblers[1].SelectedValueIndex = selectedSecIndex;

            SwitchButtonsVisibility(TimerStatus.Start);
            SwitchDisplayStyle(DisplayStyle.Normal);
        }

        /// <summary>
        /// 计时结束后点击“再来一次”按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RestartTimerButton_Click(object sender, RoutedEventArgs e)
        {
            timerHelper.TimeLeft = SettingHelper.ResetTime + 1;
            timerHelper.TimeOut = -1;

            OneSecondPass(null);
            InitializeAndStart();
            timer = new System.Threading.Timer(OneSecondPass, null, 1000, 1000);
            stopwatch.Restart();
        }
        #endregion

        #region 功能区按钮
        /// <summary>
        /// 点击按钮开关铃声提醒
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SettingHelper.IsReminderOn = !SettingHelper.IsReminderOn;
            SwitchReminder(SettingHelper.IsReminderOn);
            ShowUIForAWhile();
        }

        /// <summary>
        /// 点击按钮开关全屏显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            SettingHelper.IsFullscreenOn = !SettingHelper.IsFullscreenOn;
            if (!SettingHelper.IsFullscreenOn && timerStatus == TimerStatus.Running && this.WindowState == WindowState.Maximized)
            {
                SwitchDisplayStyle(DisplayStyle.Lite);
            }
            else if (SettingHelper.IsFullscreenOn && timerStatus == TimerStatus.Running)
            {
                // 3s后进入全屏
                fullscreenTime = timerHelper.TimeLeft >= 3 ? timerHelper.TimeLeft - 3 : 0;
                ShowUIForAWhile();
            }
            SwitchFullscreen(SettingHelper.IsFullscreenOn);
        }

        /// <summary>
        /// 点击按钮开关超时计算
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            SettingHelper.IsTimeoutOn = !SettingHelper.IsTimeoutOn;
            SwitchTimeout(SettingHelper.IsTimeoutOn);
            ShowUIForAWhile();
        }

        /// <summary>
        /// 开关铃声提醒，修改按钮样式
        /// </summary>
        /// <param name="isSwitchOn"></param>
        private void SwitchReminder(bool isSwitchOn)
        {
            globalUIControl.ReminderButtonColor =
                (isSwitchOn ? new SolidColorBrush(Color.FromRgb(18, 150, 219)) : new SolidColorBrush(Colors.Gray));
        }

        /// <summary>
        /// 开关全屏，修改按钮样式
        /// </summary>
        /// <param name="isSwitchOn"></param>
        private void SwitchFullscreen(bool isSwitchOn)
        {
            globalUIControl.FullscreenButtonColor =
                (isSwitchOn ? new SolidColorBrush(Color.FromRgb(18, 150, 219)) : new SolidColorBrush(Colors.Gray));
        }

        /// <summary>
        /// 开关允许超时，修改按钮样式
        /// </summary>
        /// <param name="isSwitchOn"></param>
        private void SwitchTimeout(bool isSwitchOn)
        {
            globalUIControl.TimeoutButtonColor =
                (isSwitchOn ? new SolidColorBrush(Color.FromRgb(18, 150, 219)) : new SolidColorBrush(Colors.Gray));
        }
        #endregion

        /// <summary>
        /// 拖拽窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// 点击关闭按钮退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 鼠标在滚轮控件处进行了操作，界面变更为“重设”后的样式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimepickerControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (timerStatus == TimerStatus.Paused)
            {
                ResetTumbler();
                SwitchButtonsVisibility(TimerStatus.Start);
            }
        }

        /// <summary>
        /// 鼠标在滚轮控件处进行了操作，界面变更为“重设”后的样式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimepickerControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (timerStatus == TimerStatus.Paused && e.LeftButton == MouseButtonState.Pressed)
            {
                ResetTumbler();
                SwitchButtonsVisibility(TimerStatus.Start);
            }
        }

        /// <summary>
        /// 将已经初始化完成的滚轮控件内容重置
        /// </summary>
        private void ResetTumbler()
        {
            List<string> min = new List<string>();
            for (int i = 0; i <= 60; ++i)
            {
                min.Add(string.Format("{0:d2}", i));
            }

            List<string> sec = new List<string>();
            for (int i = 0; i <= 5; ++i)
            {
                sec.Add(string.Format("{0:d2}", i * 10));
            }

            TimepickerControl.Tumblers[0].Values = min;
            TimepickerControl.Tumblers[1].Values = sec;
        }

        /// <summary>
        /// 点击最小化的计时器后显示操作界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ShowUIForAWhile();
        }

        /// <summary>
        /// 点击最小化的计时器后显示操作界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowUIForAWhile();
        }

        /// <summary>
        /// 应用程序退出前先保存设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TheWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
            SettingHelper.SaveSettingConfig();
        }

        /// <summary>
        /// 计时过程中点击界面会显示3秒的完整界面
        /// </summary>
        private void ShowUIForAWhile()
        {
            if (timerStatus == TimerStatus.Running && SettingHelper.IsFullscreenOn == false)
            {
                SwitchDisplayStyle(DisplayStyle.Normal);
                SwitchButtonsVisibility(TimerStatus.Running);

                // 3s后隐藏
                disappearTime = timerHelper.TimeLeft - 3;
            }
        }

        /// <summary>
        /// 根据传入的参数决定该显示哪些按钮和文字，隐藏哪些按钮和文字
        /// </summary>
        /// <param name="status">枚举类型标识计时器的状态</param>
        private void SwitchButtonsVisibility(TimerStatus status)
        {
            #region 隐藏所有控件
            globalUIControl.CanSeeStartTimerGrid = false;
            globalUIControl.CanSeeRunningTimerStackPanel = false;
            globalUIControl.CanSeePausedTimerStackPanel = false;
            globalUIControl.CanSeeEndTimerStackPanel = false;
            globalUIControl.CanSeeTimePickerGrid = false;
            globalUIControl.CanSeeTimeLeftStackPanel = false;
            globalUIControl.CanSeeFeatureButtonsPanelGrid = true;
            globalUIControl.CanSeeTimeOutTextBlock = false;
            #endregion
            timerStatus = status;

            switch (status)
            {
                case TimerStatus.Start:
                    globalUIControl.CanSeeStartTimerGrid = true;
                    globalUIControl.CanSeeTimePickerGrid = true;
                    break;
                case TimerStatus.Running:
                    globalUIControl.CanSeeRunningTimerStackPanel = true;
                    globalUIControl.CanSeeTimeLeftStackPanel = true;
                    break;
                case TimerStatus.Paused:
                    globalUIControl.CanSeePausedTimerStackPanel = true;
                    globalUIControl.CanSeeTimePickerGrid = true;
                    break;
                case TimerStatus.End:
                    globalUIControl.CanSeeEndTimerStackPanel = true;
                    globalUIControl.CanSeeTimeLeftStackPanel = true;
                    globalUIControl.CanSeeFeatureButtonsPanelGrid = false;
                    break;
                case TimerStatus.Overtime:
                    globalUIControl.CanSeeEndTimerStackPanel = true;
                    globalUIControl.CanSeeTimeLeftStackPanel = true;
                    globalUIControl.CanSeeFeatureButtonsPanelGrid = false;
                    globalUIControl.CanSeeTimeOutTextBlock = true;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 根据传入的参数调整显示模式，最小化(只显示时间)、全屏、普通
        /// </summary>
        private void SwitchDisplayStyle(DisplayStyle displayStyle)
        {
            switch (displayStyle)
            {
                case DisplayStyle.Fullscreen:
                    SwitchDisplayUIVisibility(Colors.Black, Colors.Black, 3, true, true, true, true, WindowState.Maximized, 64, 46);
                    break;
                case DisplayStyle.Lite:
                    SwitchButtonsVisibility(TimerStatus.Running);
                    SwitchDisplayUIVisibility(Colors.Transparent, Colors.Transparent, 2, false, false, false, false, WindowState.Normal, 42, 24);
                    break;
                case DisplayStyle.Normal:
                    SwitchDisplayUIVisibility(Colors.Black, Colors.Transparent, 2, true, true, true, true, WindowState.Normal, 42, 24);
                    break;
                default:
                    break;
            }
        }

        private void SwitchDisplayUIVisibility(Color solidColor, Color backgroundColor, int backgroundRectangleRowSpan,
            bool canSeeCloseButton, bool canSeeTitleTextBlock, bool canSeeFeatureButtonPanelGrid, bool canSeeControlButtonPanelGrid,
            WindowState windowState, int timeLeftFontSize, int timeOutFontSize)
        {
            BackgroundRectangle.Fill = new SolidColorBrush(solidColor);
            ApplicationBackgroundGrid.Background = new SolidColorBrush(backgroundColor);
            Grid.SetRowSpan(BackgroundRectangle, backgroundRectangleRowSpan);
            globalUIControl.CanSeeCloseButton = canSeeCloseButton;
            globalUIControl.CanSeeTitleTextBlock = canSeeTitleTextBlock;
            globalUIControl.CanSeeFeatureButtonsPanelGrid = canSeeFeatureButtonPanelGrid;
            globalUIControl.CanSeeControlButtonsPanelGrid = canSeeControlButtonPanelGrid;
            this.WindowState = windowState;
            TimeLeftTextBlock.FontSize = timeLeftFontSize;
            TimeOutTextBlock.FontSize = timeOutFontSize;
        }

    }
}
