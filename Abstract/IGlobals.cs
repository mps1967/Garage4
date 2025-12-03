using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstract
{
    public interface IGlobals
    {
        string DataRoot { get; }
        void Add(IBlueprint bp);
        IBlueprint GetBlueprint(string name);
        int NewNumber();
    }
}
