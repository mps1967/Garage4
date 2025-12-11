using Abstract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Lib
{
    public class Space : ISpace

    {
        public  struct ParkStatus
        {
            public ParkStatus() { }

            public Space? ParkingSpace = null;
                // subspace where the vehicle could park.
            public int Planes = 0;
                // number of available Plane spaces after the vehicle parks at ParkingSpace.
            public int Busses = 0;
                // number of available Bus spaces after vehicle parks at ParkingSpace.
            public int Cars = 0;
                // number of available car spaces after vehicle parks at ParkingSpace.
            public int Boats = 0;
                // the number of available boat spaces after the vehicle parks at ParkingSpace.
            public int Small = 0;
                // number of small vehicles that can still park after vehicle parks at ParkingSpace.

        public int Score()
            {
                if (ParkingSpace == null) return 0;
                return Planes * Plane.DefaultRecord.RequiredSpaces
                    + Busses * Bus.DefaultRecord.RequiredSpaces
                    + Cars + Boats + (1 + Small) / 2;
            }
            public void Accumulate(ParkStatus ps)
            {
                Planes += ps.Planes;
                Busses += ps.Busses;
                Cars += ps.Cars;
                Boats += ps.Boats;
                Small += ps.Small;
                if (ps.ParkingSpace != null) 
                {
                    ParkingSpace = ps.ParkingSpace; 
                }
            }
        }
        private SortedDictionary<int, IVehicle> parked_ = new();

        void ParseRecord(out bool boat, out bool car, out bool bus, out bool plane)
        {
            boat = car = bus = plane = false;
            boat = Record.Depth < 0;
            if (boat) return;
            int last_dot_index = Name.LastIndexOf('.');
            if (last_dot_index == -1) return;
            string last_comp = Name.Substring(last_dot_index + 1);
            if (int.TryParse(last_comp, out int num))
            {
                car = true;
                return;
            }
            Debug.Assert(last_comp.Length > 0);
            switch (last_comp[0])
            {
                case 'P': plane = true; return;
                case 'B': bus = true; return;
                default: return; ;
            }
        }

        private static bool Is(IVehicle? v, string vt)
        {
            if (v == null) return false;
            switch(vt)
            {
                case "Boat": return v!.Record().VehicleType == "Boat";
                case "Bus": return v!.Record().VehicleType == "Bus";
                case "Plane": return v!.Record().VehicleType == "Plane";
                case "Small": return v!.Record().IsSmall;
                case "Car":
                    if (v!.Record().IsSmall) return false;
                    if (v!.Record().VehicleType == "Boat") return false;
                    if (v!.Record().RequiredSpaces != 1) return false;
                    return true;
                default: Debug.Assert(false); return false;
            }
        }
        public ParkStatus IntrinsicStatus(IVehicle? vehicle = null)
        {
            ParseRecord(out bool boat, out bool car, out bool bus, out bool plane);
            ParkStatus ps = new();
            if (boat)
            {
                // leaf.
                int boats = Is(vehicle, "Boat") ? 1 : 0;
                boats += parked_.Count;
                switch(boats)
                {
                    case 0:
                        ps.Boats = 1;
                        break;
                    case 1:
                        ps.Boats = 0;
                        if (Is(vehicle, "Boat")) ps.ParkingSpace = this;
                        break;
                    case 2:
                        ps.Boats = 0;
                        break;
                }
                return ps;
            }

            if (car)
            {
                // leaf.
                ps.Cars = 1;
                ps.Small = 2;
                if (parked_.Count != 0)
                {
                    ps.Cars = 0;
                    foreach (var v in parked_.Values)
                    {
                        if (v.Record().IsSmall) ps.Small -= 1;
                        else ps.Small = 0;
                    }
                    Debug.Assert(0 <= ps.Small);
                    Debug.Assert(ps.Small <= 2);
                }
                if (Is(vehicle, "Car"))
                {
                    if (ps.Cars == 1)
                    {
                        ps.Cars = 0;
                        ps.ParkingSpace = this;
                        return ps;
                    }
                    return ps;
                }
                if (Is(vehicle, "Small"))
                {
                    switch (ps.Small)
                    {
                        case 0: return ps;
                        case 1:
                            ps.Small = 0;
                            ps.ParkingSpace = this;
                            return ps;
                        case 2:
                            ps.Small = 1;
                            ps.Cars = 0;
                            ps.ParkingSpace = this;
                            return ps;
                        default: Debug.Assert(false); return ps;
                    }
                }
                return ps;
            }
            if (bus)
            {
                ps.Busses = 1;
                if (parked_.Count != 0)
                {
                    ps.Busses = 0;
                    return ps;
                }
                if (Is(vehicle, "Bus"))
                {
                    ps.ParkingSpace = this;
                    ps.Busses = 0;
                }
                return ps;
            }
            if (plane) ps.Planes = 1;
            if (parked_.Count != 0)
            {
                ps.Planes = 0;
                return ps;
            }
            if (Is(vehicle, "Plane"))
            { 
                ps.ParkingSpace = this;
                ps.Planes = 0;
            }

            return ps;


        }
        public ParkStatus ComputeStatus()
        {
            ParkStatus ps = IntrinsicStatus();
            foreach (var s in spaces_)
            {
                ps.Accumulate(s.ComputeStatus());
            }
            return ps;
        }
        public SpaceRecord Record {  get; private set; }
        public string Name => Record.Name;

        public ISpace? Parent {  get; private set; }

        public ParkStatus ParkWithStatus(IVehicle vehicle)
        {
            ParkStatus ips_with_vehicle = IntrinsicStatus(vehicle);
            ParkStatus ips_without_vehicle = IntrinsicStatus(null);
            ParkStatus best = ips_with_vehicle;
            List<ParkStatus> without_vehicle = new();
            List<ParkStatus> with_vehicle = new();
            // If we park the vehicle at this, no child spaces are available.
            foreach (var s in spaces_)
            {
                with_vehicle.Add(s.ParkWithStatus(vehicle));
                without_vehicle.Add(s.ComputeStatus());
            }
            for (int i = 0; i < spaces_.Count; ++i)
            {
                ParkStatus acc = ips_without_vehicle;
                for (int j = 0; j < spaces_.Count; ++j)
                {
                    if (i == j)acc.Accumulate(with_vehicle[i]);
                    else acc.Accumulate(without_vehicle[j]);
                }
                if (acc.Score() > best.Score()) best = acc;
            }
            return best;
        }

        public void ForcePark(IVehicle vehicle)
            // to be used when loading the garage from file (with cast, not in ISpace).
        {
            parked_[vehicle.OurId()] = vehicle;
        }

        public ISpace? Park(IVehicle vehicle)
        {
            var ps = ParkWithStatus(vehicle);
            if (ps.ParkingSpace == null) return null;
            ps.ParkingSpace!.parked_[vehicle.OurId()] = vehicle;
            return ps.ParkingSpace;
        }
        private List<Space> spaces_;
        public IEnumerable<ISpace> Spaces()
        { 
            foreach (ISpace i in spaces_) yield return i;    
        }
                
        public IEnumerable<IVehicle> Vehicles()
        {
            foreach (var v in parked_.Values) yield return v;
            foreach (var s in spaces_)
            {
                foreach (var w in s.Vehicles()) yield return w;
            }
        }
        public void SetParent(Space? parent)
        {
            Parent = parent;
            if (spaces_ == null) return;
            foreach (Space s in spaces_!) s.SetParent(this);
        }

        public bool Unpark(IVehicle vehicle)
        {
            return parked_.Remove(vehicle.OurId());
        }

        public Space(SpaceRecord record, IEnumerable<Space> spaces)
        {
            Record = record;
            var sl = spaces.ToList();
            if (sl == null)
            {
                spaces_ = new();
            }
            else
            {
                spaces_ = sl!;
            }
        }
    }
}
