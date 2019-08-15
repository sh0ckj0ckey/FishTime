using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Timer.Helper
{
    public class UIControlHelper : INotifyPropertyChanged
    {
        // 记录一些数据绑定到界面
        private string _leftTime;
        public string LeftTime
        {
            get
            {
                return this._leftTime;
            }
            set
            {
                this._leftTime = value;
                OnPropertyChanged("LeftTime");
            }
        }

        // 标识一些控件可见性
        private bool _canSeeCloseButton;
        public bool CanSeeCloseButton
        {
            get
            {
                return this._canSeeCloseButton;
            }
            set
            {
                this._canSeeCloseButton = value;
                OnPropertyChanged("CanSeeCloseButton");
            }
        }

        private bool _canSeeTitleTextBlock;
        public bool CanSeeTitleTextBlock
        {
            get
            {
                return this._canSeeTitleTextBlock;
            }
            set
            {
                this._canSeeTitleTextBlock = value;
                OnPropertyChanged("CanSeeTitleTextBlock");
            }
        }

        private bool _canSeeTimePickerGrid;
        public bool CanSeeTimePickerGrid
        {
            get
            {
                return this._canSeeTimePickerGrid;
            }
            set
            {
                this._canSeeTimePickerGrid = value;
                OnPropertyChanged("CanSeeTimePickerGrid");
            }
        }

        private bool _canSeeTimeLeftStackPanel;
        public bool CanSeeTimeLeftStackPanel
        {
            get
            {
                return this._canSeeTimeLeftStackPanel;
            }
            set
            {
                this._canSeeTimeLeftStackPanel = value;
                OnPropertyChanged("CanSeeTimeLeftStackPanel");
            }
        }

        private bool _canSeeTimeOutTextBlock;
        public bool CanSeeTimeOutTextBlock
        {
            get
            {
                return this._canSeeTimeOutTextBlock;
            }
            set
            {
                this._canSeeTimeOutTextBlock = value;
                OnPropertyChanged("CanSeeTimeOutTextBlock");
            }
        }

        private bool _canSeeFeatureButtonsPanelGrid;
        public bool CanSeeFeatureButtonsPanelGrid
        {
            get
            {
                return this._canSeeFeatureButtonsPanelGrid;
            }
            set
            {
                this._canSeeFeatureButtonsPanelGrid = value;
                OnPropertyChanged("CanSeeFeatureButtonsPanelGrid");
            }
        }

        private bool _canSeeControlButtonsPanelGrid;
        public bool CanSeeControlButtonsPanelGrid
        {
            get
            {
                return this._canSeeControlButtonsPanelGrid;
            }
            set
            {
                this._canSeeControlButtonsPanelGrid = value;
                OnPropertyChanged("CanSeeControlButtonsPanelGrid");
            }
        }

        private bool _canSeeStartTimerGrid;
        public bool CanSeeStartTimerGrid
        {
            get
            {
                return this._canSeeStartTimerGrid;
            }
            set
            {
                this._canSeeStartTimerGrid = value;
                OnPropertyChanged("CanSeeStartTimerGrid");
            }
        }

        private bool _canSeeRunningTimerStackPanel;
        public bool CanSeeRunningTimerStackPanel
        {
            get
            {
                return this._canSeeRunningTimerStackPanel;
            }
            set
            {
                this._canSeeRunningTimerStackPanel = value;
                OnPropertyChanged("CanSeeRunningTimerStackPanel");
            }
        }

        private bool _canSeePausedTimerStackPanel;
        public bool CanSeePausedTimerStackPanel
        {
            get
            {
                return this._canSeePausedTimerStackPanel;
            }
            set
            {
                this._canSeePausedTimerStackPanel = value;
                OnPropertyChanged("CanSeePausedTimerStackPanel");
            }
        }

        private bool _canSeeEndTimerStackPanel;
        public bool CanSeeEndTimerStackPanel
        {
            get
            {
                return this._canSeeEndTimerStackPanel;
            }
            set
            {
                this._canSeeEndTimerStackPanel = value;
                OnPropertyChanged("CanSeeEndTimerStackPanel");
            }
        }

        private SolidColorBrush _reminderButtonColor;
        public SolidColorBrush ReminderButtonColor
        {
            get
            {
                return this._reminderButtonColor;
            }
            set
            {
                this._reminderButtonColor = value;
                OnPropertyChanged("ReminderButtonColor");
            }
        }

        private SolidColorBrush _fullscreenButtonColor;
        public SolidColorBrush FullscreenButtonColor
        {
            get
            {
                return this._fullscreenButtonColor;
            }
            set
            {
                this._fullscreenButtonColor = value;
                OnPropertyChanged("FullscreenButtonColor");
            }
        }

        private SolidColorBrush _timeoutButtonColor;
        public SolidColorBrush TimeoutButtonColor
        {
            get
            {
                return this._timeoutButtonColor;
            }
            set
            {
                this._timeoutButtonColor = value;
                OnPropertyChanged("TimeoutButtonColor");
            }
        }


        /// <summary>
        /// 构造函数，初始化为准备开始的界面
        /// </summary>
        public UIControlHelper()
        {
            this.LeftTime = "00:00";

            this.CanSeeCloseButton = true;
            this.CanSeeTitleTextBlock = true;
            this.CanSeeTimePickerGrid = true;
            this.CanSeeTimeLeftStackPanel = false;
            this.CanSeeTimeOutTextBlock = false;
            this.CanSeeFeatureButtonsPanelGrid = true;
            this.CanSeeControlButtonsPanelGrid = true;
            this.CanSeeStartTimerGrid = true;
            this.CanSeeRunningTimerStackPanel = false;
            this.CanSeePausedTimerStackPanel = false;
            this.CanSeeEndTimerStackPanel = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        virtual internal protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
