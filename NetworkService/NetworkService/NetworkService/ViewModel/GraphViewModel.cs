using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.ViewModel
{
    public class GraphViewModel : BindableBase
    {
        private MainWindowViewModel _mainWindowViewModel;

        public ObservableCollection<FlowMeter> FlowMeters => _mainWindowViewModel.FlowMeters;

        public GraphViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
        }
    }
}
