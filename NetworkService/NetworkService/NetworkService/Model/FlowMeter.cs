using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public enum ValueState { Normal, Low, High }
    public class FlowMeter:INotifyPropertyChanged
    {

        ValueState _valueState;
        int _id;
        string _name;
        EntityType _entityType;
        int _value;
        List<Pair<DateTime,int>> _last_5_values;

        public event PropertyChangedEventHandler PropertyChanged;

        public FlowMeter()
        {
            Last_5_Values = new List<Pair<DateTime, int>>();
        }

        public List<Pair<DateTime, int>> Last_5_Values
        {
            get
            {
                return _last_5_values;
            }
            set
            {
                if (_last_5_values != value)
                {
                    _last_5_values = value;
                    OnPropertyChanged(nameof(Last_5_Values));
                }
            }
        }
        public ValueState ValueState
        {
            get
            {
                return _valueState;
            }
            set
            {
                if (_valueState != value)
                {
                    _valueState = value;
                    OnPropertyChanged(nameof(ValueState));
                }
            }
        }
        public int ID 
        { 
            get 
            { 
                return _id;
            } 
            set 
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(ID));
                }
            } 
        }
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public EntityType EntityType
        {
            get
            {
                return _entityType;
            }
            set
            {
                if (_entityType != value)
                {
                    _entityType = value;
                    OnPropertyChanged(nameof(EntityType));
                }
            }
        }
        public int Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));

                    if (_value < 675) ValueState = ValueState.Low;
                    else if (_value > 735) ValueState = ValueState.High;
                    else ValueState = ValueState.Normal;

                }
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
