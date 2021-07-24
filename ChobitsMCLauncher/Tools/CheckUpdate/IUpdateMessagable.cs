using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChobitsMCLauncher.Tools.CheckUpdate
{
    interface IUpdateMessagable
    {
        void onUpdateMessage(UpdaterEvent @event);
    }
}
