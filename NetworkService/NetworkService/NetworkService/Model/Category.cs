using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{

    public class Category
    {
        public string Name { get; set; }
        public ObservableCollection<FlowMeter> FlowMeters { get; set; }

        public Category(string name)
        {
            Name = name;
            FlowMeters = new ObservableCollection<FlowMeter>();
        }
    }

}
