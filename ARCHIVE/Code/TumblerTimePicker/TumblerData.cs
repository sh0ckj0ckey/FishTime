using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections;

namespace TumblerTimePicker
{
    public class TumblerData : INotifyPropertyChanged
    {
        public TumblerData()
        {
            this.seperator = "";
            this.Values = new List<object>();
            this.TumblerWidth = Double.NaN;
        }

        public TumblerData(IList values, int selectedValueIndex, string seperator)
        {
            this.values = values;
            this.seperator = seperator;
            this.TumblerWidth = Double.NaN;
            this.SelectedValueIndex = selectedValueIndex;
        }

        private IList values;
        public IList Values
        {
            get { return this.values; }
            set
            {
                this.values = value;
                OnPropertyChanged("Values");
            }
        }

        private string seperator;
        public string Seperator
        {
            get { return this.seperator; }
            set { this.seperator = value; }
        }

        public object SelectedValue
        {
            get
            {
                return this.values == null || this.selVal < 0 || this.selVal >= this.values.Count ? null : this.values[this.selVal];
            }
        }

        private int selVal;
        public int SelectedValueIndex
        {
            get
            {
                return this.selVal;
            }
            set
            {
                this.selVal = value;
                OnPropertyChanged("SelectedValueIndex");
                TriggerUpdate();
            }
        }

        public double TumblerWidth { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        virtual internal protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal void TriggerUpdate()
        {
            // cause binding to refire
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs("SelectedValueIndex"));
                this.PropertyChanged(this, new PropertyChangedEventArgs("SelectedValue"));
            }

        }
    }
}
