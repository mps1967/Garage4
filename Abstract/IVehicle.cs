namespace Abstract
{
    public interface IVehicle
    {
        public int OurId();
        VehicleRecord Record();
        public bool Load(string? filename=null);  // filename=null for the standard location.
        public bool Save(string? filename=null);  // filename=null for the standard location.
        public string Line();
    }
}
