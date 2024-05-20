using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public enum CommandType { SwitchViews, EntityManipulation, CanvasManipulation }

    /// <summary>
    /// This class models an application save state, specified by the command that initiated it's state
    /// </summary>
    /// <typeparam name="CommandType"></typeparam>
    /// <typeparam name="State"></typeparam>
    public class SaveState<CommandType, State>
    {
        private CommandType _commandType;
        private State _state;

        public SaveState(CommandType commandType, State state)
        {
            _commandType = commandType;
            _state = state;
        }
    }
}
