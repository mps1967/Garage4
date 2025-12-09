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
        SortedList<int, SortedSet<int>> GetIndex(string field, int blahblah);
        SortedList<string, SortedSet<int>> GetIndex(string field, string blablah);
        IEnumerable<string> GetFields(string blahblah);
        IEnumerable<string> GetFields(int blahblah);
    }
}