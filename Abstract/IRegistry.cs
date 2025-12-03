using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Abstract
{
    internal interface IRegistry
    {
        IVehicle? Get(int ourid);
        bool Save(int ourid, IVehicle vehicle);
        bool SaveAsTemplate(string filename, IVehicle vehicle);

        IDB AsDatabase();
    }
}
