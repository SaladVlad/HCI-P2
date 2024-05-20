using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NetworkService.Model
{
    public class CanvasState
    {
        public Dictionary<int,FlowMeter> AddedToGrid { get; set; }
        public ObservableCollection<Canvas> CanvasCollection { get; set; }
        public Dictionary<Pair<int, int>, Line> Lines;
        public ObservableCollection<FlowMeter> EntityInfo { get; set; }
        public ObservableCollection<Brush> BorderBrushCollection { get; set; }
        public CanvasState() { }
    }
}
