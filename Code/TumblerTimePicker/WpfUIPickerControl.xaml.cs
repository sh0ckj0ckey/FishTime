using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

namespace TumblerTimePicker
{
    /// <summary>
    /// WpfUIPickerControl.xaml 的交互逻辑
    /// </summary>
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class WpfUIPickerControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public WpfUIPickerControl()
        {
            InitializeComponent();
        }

        public List<object> SelectedValues
        {
            get
            {
                List<object> retVal = new List<object>();
                foreach (var tumbler in this.Tumblers)
                {
                    retVal.Add(tumbler.SelectedValue);
                }
                return retVal;
            }
        }

        private bool open = false;
        /// <summary>
        /// Flag to indicate whether the user control is "open" (i.e. displaying tumblers).
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return this.open || this.AlwaysOpen;
            }
            set
            {
                bool newVal = value || this.AlwaysOpen;
                if (this.open != newVal)
                {
                    this.open = newVal;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("IsOpen"));
                    }
                }

            }
        }

        #region Tumblers Dependency Property
        /// <summary>
        /// Dependency property for List of Tumblers
        /// </summary>
        public List<TumblerData> Tumblers
        {
            get
            {
                return (List<TumblerData>)GetValue(TumblersProperty);
            }
            set
            {
                SetValue(TumblersProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Tumblers.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TumblersProperty = DependencyProperty.Register("Tumblers", typeof(List<TumblerData>), typeof(WpfUIPickerControl), new UIPropertyMetadata(null, OnTumblersPropertyChanged));

        private static void OnTumblersPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            // TODO: update display
            // 通过给 WpfUIPickerControl.Tumblers[0].Values 重新赋值新的列表即可触发 TumblerData 的 OnPropertyChanged 事件，因此此处似乎无需实现
        }
        #endregion


        #region AlwaysOpen Dependency Property
        public bool AlwaysOpen
        {
            get
            {
                return (bool)GetValue(AlwaysOpenProperty);
            }
            set
            {
                SetValue(AlwaysOpenProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for AlwaysOpen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AlwaysOpenProperty = DependencyProperty.Register("AlwaysOpen", typeof(bool), typeof(WpfUIPickerControl), new UIPropertyMetadata(false));
        #endregion


        /// <summary>
        /// Mouse wheel handler for tumblers.  This is not a fired event because we need to mouse capture on the main grid
        /// so we can know when to close the control.  Instead this event gets called via the MouseWheel event on the main grid.
        /// </summary>
        /// <param name="tumbler">The tumbler (grid) that should receive the mouse wheel event</param>
        /// <param name="e">the MouseWheelEventArgs forwarded from the main grid MouseWheel event</param>
        private void Tumbler_PreviewMouseWheel(Grid tumbler, MouseWheelEventArgs e)
        {
            if (tumbler != null)
            {
                TumblerData td = tumbler.Tag as TumblerData;
                // Each click in the mouse will increment or decrement the tumbler value by one
                int newIdx = td.SelectedValueIndex + (e.Delta > 0 ? -1 : 1);
                if (newIdx >= 0 && newIdx < td.Values.Count)
                {
                    // Set the new index which will cause NotifyPropertyChanged to fire, which in turn will cause
                    // the binding/converter to re-evaluate, which will result in the tumbler animating to the correct 
                    // canvas offset.
                    td.SelectedValueIndex = newIdx;
                }
            }
        }

        private Grid dragTumbler = null;
        private Point dragPt = new Point(0, 0);
        private double originalDragOffset = 0;

        private void mainGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.mainGrid.IsMouseCaptured == false)
            {
                this.IsOpen = true;
                this.mainGrid.CaptureMouse();
                StartDrag();
            }
            else if (this.mainGrid.ContainsMouse())
            {
                StartDrag();
            }
            else
            {
                // close ui picker
                this.mainGrid.ReleaseMouseCapture();
                this.IsOpen = false;
            }
        }

        private void StartDrag()
        {
            this.dragTumbler = getTargetTumbler();
            this.dragPt = Mouse.GetPosition(this.mainGrid);
            this.originalDragOffset = Canvas.GetTop(this.dragTumbler);
        }

        private void mainGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // figure out which tumbler should get the mouse wheel
            Grid targetTumbler = this.getTargetTumbler();
            if (targetTumbler != null && this.IsOpen)
            {
                Tumbler_PreviewMouseWheel(targetTumbler, e);
            }
        }

        private void mainGrid_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // is this the end of a drag operation?
            if (this.dragTumbler != null)
            {
                double offset = -Canvas.GetTop(this.dragTumbler);

                Point mousePt = Mouse.GetPosition(this.mainGrid);
                if (mousePt.Equals(this.dragPt))
                {
                    // user clicked on tumbler without dragging, select the clicked-on value
                    offset += mousePt.Y - (this.mainGrid.ActualHeight / 2);
                }
                // find the Canvas offset for the value closest to where the drag ended
                TumblerData td = this.dragTumbler.Tag as TumblerData;

                double itemHeight = this.dragTumbler.ActualHeight / (td.Values.Count);
                double newVal = offset / itemHeight + 2;
                int iVal = (int)Math.Round(newVal);

                // update index, limit to valid values.  The update will cause NotifyPropertyChanged which
                // will animate the tumbler into the appropriate position for the selected value
                td.SelectedValueIndex = Math.Min(Math.Max(0, iVal), td.Values.Count - 1);

                this.dragTumbler = null;
            }

            if (this.mainGrid.IsMouseCaptured && this.AlwaysOpen)
            {
                this.mainGrid.ReleaseMouseCapture();
            }
        }

        private void mainGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // are we dragging a tumbler?
            if (Mouse.LeftButton == MouseButtonState.Pressed && this.dragTumbler != null)
            {
                Point pt = Mouse.GetPosition(this.mainGrid);
                double diff = pt.Y - this.dragPt.Y;
                // have to remove the animation before setting it manually (otherwise nothing happens because
                // the animation fill behavior is set to HoldEnd)
                this.dragTumbler.BeginAnimation(Canvas.TopProperty, null);
                Canvas.SetTop(this.dragTumbler, this.originalDragOffset + diff);
            }

        }

        /// <summary>
        /// Gets the Tumbler (grid) that currently contains the mouse.  Useful for forwarding mouse events to the specific tumbler.
        /// </summary>
        /// <returns>the Grid (itemsGrid) that contains the mouse pointer</returns>
        private Grid getTargetTumbler()
        {
            Grid targetTumbler = null;
            for (int i = 0; i < this.Tumblers.Count; ++i)
            {
                Border foundTumbler = this.mainGrid.FindChild<Border>("tumblerBorder", i);
                if (foundTumbler != null && foundTumbler.ContainsMouse())
                {
                    targetTumbler = foundTumbler.FindChild<Grid>("itemsGrid", 0);
                    break;
                }
            }
            return targetTumbler;
        }

        private void thisCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            this.IsOpen = this.AlwaysOpen;
            // Trigger an update on each tumbler so its canvas offset is set properly for displaying the selected value
            foreach (var td in this.Tumblers)
            {
                td.TriggerUpdate();
            }
        }
    }

}
