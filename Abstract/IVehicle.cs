namespace Abstract
{
    public interface IVehicle
    {
        int OurId();
        VehicleRecord Record();
        bool Load(string? filename=null);  // filename=null for the standard location.
        bool Save(string? filename=null);  // filename=null for the standard location.
    }
}
