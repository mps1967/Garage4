using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Abstract
{
    public interface IRegistry
    {
        IVehicle? Get(int ourid);
        bool Save(int ourid, IVehicle vehicle);
        bool SaveAsTemplate(string filename, IVehicle vehicle);

        IDB Database {  get; }

        string Line(int ourid);
    }
}
