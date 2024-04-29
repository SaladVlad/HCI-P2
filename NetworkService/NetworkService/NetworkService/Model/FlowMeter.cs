using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class FlowMeter:INotifyPropertyChanged
    {
        int _id;
        string _name;
        EntityType _entityType;
        int _value;

        public event PropertyChangedEventHandler PropertyChanged;

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
                }
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
