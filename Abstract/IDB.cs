using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstract
{
    public interface IDB
    {
        void AddVehicle(IVehicle vehicle);

        // Floats are multiplied by 10 and inserted into an int index.
        SortedList<int, List<int>> GetIntIndex(string field);
        SortedList<string, List<int>> GetStringIndex(string field);
    }
}
