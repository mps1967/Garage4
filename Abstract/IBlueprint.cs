using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstract
{
    public interface IBlueprint
        // Sets of allowable values.
    {
        string Name { get; }

        public SortedSet<string> Set { get; }
        bool Load();
        bool Save();
    }
}
