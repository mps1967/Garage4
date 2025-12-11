using Abstract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib
{
    public class VehicleRegistry:IRegistry

    {
        public class DBException: Exception
        {
            public DBException() { }
        }

        private IGlobals ig_;
        private DB db_ = new();
        public IDB Database { get { return db_; }}
        private IBlueprint vehtypes_;
        private IBlueprint brandmodel_;
        private IBlueprint color_;
        private SortedDictionary<string, Vehicle> classes_ = new();
            // ^ hardcoded vehicles.
        private SortedDictionary<string, Vehicle> templates_ = new();
        // ^the hardcoded vehicles are replaced with the vehicle types
        // saved in the Application directory if any. To those the brandmodel
        // vehicles are added.

        private List<string> first_menu_ = new();
        // The menu of starting templates or one vehicle to change.
        private List<IVehicle> vehicle_menu_ = new();
            // the menu of vehicles corresponding to the strings.
        private IVehicle? target_;
            // ^The one vehicle we focus on when editing ptoperties.
        private VehicleRecord record_;
        // the edited properties that the vehicle will be rebuilt with.

        // Handling of the Second menu of editing and saving actions.

        private List<string> second_menu_ = new();
        private List<string> second_menu_args_ = new();
            // record field name corresponding to the menu entry.
            // symbols for save operations:
            // /: save as vehicle type.
            // +: save as brand-model.
            // *: register or save as OurId.json.
        private string BrandModelTemplateFilename(string name)
        {
            string fn = name.Replace(' ', '@');
            return Path.Join([ig_.DataRoot, "Application", fn + ".json"]);
        }
        private void LoadBrandModelTemplate(string bm)
        {
            if (string.IsNullOrEmpty(bm)) return;
            string fn  = BrandModelTemplateFilename(bm);
            Vehicle bm_vehicle = new Vehicle(ig_, new VehicleRecord());
            if (!bm_vehicle.Load(fn)) return;
            templates_[bm] = bm_vehicle;
        }
        private string VehicleTypeTemplateFilename(string vt)
        {
            return Path.Join([ig_.DataRoot, "Application", vt + ".json"]);
        }
        private void LoadVehicleTypeTemplate(string vt)
            // makes templates_[vt] = vehicle type template if it exists or
            // classes[vt] as fallback.
        {
            Vehicle v = new(ig_, record_);
            string vt_path = VehicleTypeTemplateFilename(vt);
            if ( !v.Load(vt_path))
            {
                if (!classes_.ContainsKey(vt)) return;
                classes_[vt].Save(vt_path);
                v = classes_[vt];
            }
            templates_[vt] = v;
        }

        private void MakeFirstMenu()
        {
            first_menu_.Clear();
            vehicle_menu_.Clear();
            foreach (var kv in classes_)
            {
                LoadVehicleTypeTemplate(kv.Key);
            }
            foreach (string vt in vehtypes_.Set)
            {
                if (templates_.ContainsKey(vt)) continue;
                LoadVehicleTypeTemplate(vt);
            }
            foreach (var bm in brandmodel_.Set)
            {
                LoadBrandModelTemplate(bm);
            }
            if (target_ != null)
            {
                first_menu_.Add(target_.Line());
                vehicle_menu_.Add(target_);
            }
            first_menu_.AddRange(templates_.Keys);
            vehicle_menu_.AddRange(templates_.Values);
        }

        public VehicleRegistry(IGlobals ig) 
        {
            ig_ = ig;
            vehtypes_ = ig_.GetBlueprint(Blueprint.VEHTYPES);
            vehtypes_.Load();
            brandmodel_ = ig_.GetBlueprint(Blueprint.BRANDMODEL);
            brandmodel_.Load();
            color_ = ig_.GetBlueprint(Blueprint.COLOR);
            color_.Load();
            classes_["Bike"] = new Bike(ig_);
            classes_["Boat"] = new Boat(ig_);
            classes_["Bus"] = new Bus(ig_);
            classes_["Car"] = new Car(ig_);
            classes_["Moto"] = new Moto(ig_);
            classes_["Plane"] = new Plane(ig_);
            db_.Load(ig_);
        }

        
        public IVehicle? Get(int ourid)
        {
            VehicleRecord vr = new VehicleRecord(OurId: ourid);
            Vehicle v = new Vehicle(ig_, vr);
            if (v.Load()) return v;
            return null;
        }

        public bool Save(int ourid, IVehicle vehicle)
        {
            throw new NotImplementedException();
        }

        public bool SaveAsTemplate(string filename, IVehicle vehicle)
        {
            throw new NotImplementedException();
        }

        private void MaybeAddSaveAsVehicleType()
        {
            if (string.IsNullOrEmpty(record_.VehicleType)) return;
            second_menu_.Add($"Save as {record_.VehicleType} template");
            second_menu_args_.Add("/");
        }
        private void MaybeAddSaveAsBrandModel()
        {
            if (string.IsNullOrEmpty(record_.BrandModel)) return;
            second_menu_.Add($"Save as {record_.BrandModel} template");
            second_menu_args_.Add("+");
        }
        private string SaveToOurIdText()
        {
            if (record_.OurId == 0) return "Register vehicle.";
            return $"Save vehicle {record_.OurId}";
        }

        bool ConstrainInt(string str, out int value, int minValue=0, int maxValue = int.MaxValue)
            // returns the absolute value of the input.
            // min and max should be positive.
            // For depth it will be stored negative in the record.
        {
            Debug.Assert(minValue >= 0);
            Debug.Assert(maxValue > minValue);
            value = 0;
            if (!int.TryParse(str, out int v)) return false;
            if (v < 0) v = -v;
            if (v < minValue) return false;
            if (v >= maxValue) return false;
            value = v;
            return true;
        }
        bool ConstrainFloat(string str, out float value, float minValue = 0, float maxValue = 4e8f)
            // maxValue Default so than v*10 < int.MaxValue for indexing purposes.
        {
            Debug.Assert(minValue >= 0);
            Debug.Assert(maxValue > minValue);
            value = 0;
            if (!float.TryParse(str, out float v)) return false;
            if (v < 0) v = -v;
            if (v < minValue) return false;
            if (v > maxValue) return false;
            value = v;
            return true;
        }

        static private Func<char, bool> 
            AlNum = c => char.IsLetterOrDigit(c);
        static private Func<char, bool> 
            SpaceSepAlNum = c => AlNum(c) || Menu.WHITESPACE.Contains(c);

        bool FilterString(string str, out string s, Func<char, bool> is_good, int minlen = 0, int maxlen = 1000)
        {
            Debug.Assert(minlen >= 0);
            Debug.Assert(maxlen > minlen);
            StringBuilder sb = new();
            s = "";
            foreach (char c in str)
            {
                if (!is_good(c)) continue;
                if (sb.Length < maxlen) sb.Append(c);
            }
            string ss = sb.ToString().Trim();
            if (ss.Length < minlen) return false;
            if (ss.Length > maxlen) return false;
            s = ss;
            return true;
        }
        void SetField(string field, string value)
        {
            switch (field)
            {
                case "OfficialId":
                    if (!FilterString(value, out string oi, AlNum, 0, 16)) return;
                    // duplicate oi not allowed.
                    EnforceUniqueOfficialId(oi);
                    SetRecordValue(field, oi);
                    return;
                case "VehicleType":
                    if (!FilterString(value, out string vt, AlNum, 0, 16)) return;
                    // new types are allowed.
                    SetRecordValue(field, vt);
                    return;
                case "BrandModel":
                    if (!FilterString(value, out string bm, SpaceSepAlNum, 0, 16)) return;
                    // new bm are allowed.
                    SetRecordValue(field, bm);
                    return;
                case "Color":
                    if (!FilterString(value, out string c, AlNum, 1, 16)) return;
                    // new colors allowed.
                    SetRecordValue(field, c); return;
                case "RequiredSpaces":
                    if (!ConstrainInt(value, out int spaces, 1, 40)) return;
                    SetRecordValue(field, spaces);
                    return;
                case "IsSmall":
                    if (!bool.TryParse(value, out bool v)) return;
                    SetRecordValue(field, v);
                    return;
                case "WheelCount":
                    if (!ConstrainInt(value, out int wheel_count, 1, 20)) return;
                    SetRecordValue(field, wheel_count);
                    return;
                case "Persons":
                    if (!ConstrainInt(value, out int persons, 1, 100)) return;
                    SetRecordValue(field, persons);
                    return;
                case "Height":
                    if (!ConstrainFloat(value, out float h, 0, 25f)) return;  // a380
                    SetRecordValue(field, h);
                    return;
                case "Width":
                    if (!ConstrainFloat(value, out float w, 0, 81f)) return;  // a380.
                    SetRecordValue(field, w);
                    return;
                case "Length":
                    if (!ConstrainFloat(value, out float l, 0, 75f)) return;  // a380
                    SetRecordValue(field, l);
                    return;
                case "Depth":
                    if (!ConstrainFloat(value, out float d)) return;
                    SetRecordValue(field, d);
                    return;
                case "Extra":
                    if (!FilterString(value, out string e, SpaceSepAlNum, 0)) return;
                    SetRecordValue(field, e);
                    return;
            }
        }


            string RecordField(string field)
        {
            switch (field)
            {
                case "OurId": return $"{record_.OurId}";
                case "OfficialId": return $"{record_.OfficialId}";
                case "VehicleType": return $"{record_.VehicleType}";
                case "BrandModel": return $"{record_.BrandModel}";
                case "Color": return $"{record_.Color}";
                case "RequiredSpaces": return $"{record_.RequiredSpaces}";
                case "IsSmall": return $"{record_.IsSmall}";
                case "WheelCount": return $"{record_.WheelCount}";
                case "Persons": return $"{record_.Persons}";
                case "Height": return $"{record_.Height}";
                case "Width": return $"{record_.Width}";
                case "Length": return $"{record_.Length}";
                case "Depth": return $"{Math.Abs(record_.Depth)}";
                case "Extra": return $"{record_.Extra}";
                default: Debug.Assert(false); return "";
            }
        }

        void SecondMenuAdd(string field)
        {
            second_menu_.Add($"{field}: {RecordField(field)}");
            second_menu_args_.Add(field);
        }

        void GetRecordValue(string field, out string v)
        {
            switch (field)
            {
                case "OfficialId": v = record_.OfficialId; return;
                case "VehicleType": v = record_.VehicleType; return;
                case "BrandModel": v = record_.BrandModel; return;
                case "Color": v = record_.Color; return;
                case "Extra": v = record_.Extra; return;
                default: Debug.Assert(false); v = ""; return;
            }
        }

        void GetRecordValue(string field, out int v)
        {
            switch (field)
            {
                case "OurId": v = record_.OurId; return;
                case "RequiredSpaces": v = record_.RequiredSpaces; return;
                case "WheelCount": v = record_.WheelCount; return;
                case "Persons": v = record_.Persons; return;
                default: Debug.Assert(false); v = 0; return;
            }
        }

        void GetRecordValue(string field, out bool v)
        {
            switch(field)
            {
                case "IsSmall": v = record_.IsSmall;return;
                default: Debug.Assert(false); v = false; return;
            }
        }

        void GetRecordValue(string field, out float v)
        {
            switch (field)
            {
                case "Height": v = record_.Height; return;
                case "Width": v = record_.Width; return;
                case "Length": v = record_.Length; return;
                case "Depth": v = Math.Abs(record_.Depth); return;
                default: Debug.Assert(false); v = 0; return;
            }
        }

        void SetRecordValue(string field, string v)
        {
            switch (field)
            {
                case "OfficialId":
                    record_.OfficialId = v; return;
                case "VehicleType": record_.VehicleType = v; return;
                case "BrandModel": record_.BrandModel = v; return;
                case "Color": record_.Color = v; return;
                case "Extra": record_.Extra = v; return;
                default: Debug.Assert(false); return;
            }
        }
        void SetRecordValue(string field, int v)
        {
            switch (field)
            {
                case "OurId": record_.OurId = v; return;
                case "RequiredSpaces": record_.RequiredSpaces = v; return;
                case "WheelCount": record_.WheelCount = v; return;
                case "Persons": record_.Persons = v; return;
                default: Debug.Assert(false); return;
            }
        }
        void SetRecordValue(string field, bool v)
        {
            switch (field)
            {
                case "IsSmall": record_.IsSmall = v; return;
                default: Debug.Assert(false); return;
            }
        }
        void SetRecordValue(string field, float v)
        {
            switch (field)
            {
                case "Height": record_.Height = v; return;
                case "Width": record_.Width = v; return;
                case "Length": record_.Length = v; return;
                case "Depth": record_.Depth = - Math.Abs(v); return;
                default: Debug.Assert(false); return;
            }
        }
        private void MakeSecondMenu()
        {
            SecondMenuAdd("OfficialId");
            SecondMenuAdd("VehicleType");
            SecondMenuAdd("BrandModel");
            SecondMenuAdd("Color");
            SecondMenuAdd("RequiredSpaces");
            SecondMenuAdd("IsSmall");
            SecondMenuAdd("WheelCount");
            SecondMenuAdd("Persons");
            SecondMenuAdd("Height");
            SecondMenuAdd("Width");
            SecondMenuAdd("Length");
            SecondMenuAdd("Depth");
            SecondMenuAdd("Extra");
            // save options below.
            MaybeAddSaveAsVehicleType();
            MaybeAddSaveAsBrandModel();
            second_menu_.Add("Register/Save/Select");
            second_menu_args_.Add("*");
        }

        public int Run(int ourid=0)
        {
            try { return MustRun(ourid); }
            catch { return 0; }
        }

        int MustRun(int ourid=0)
            // Returns an OurId for a newly registered vehicle.
            // Returns 0 if registration is abandoned.
            // Receives an OurId whose registration can then be edited.
        {
            target_ = null;
            if (0 != ourid)
            {
                target_ = Get(ourid);
            }
            while (true)
            {
                MakeFirstMenu();
                int sel = Menu.Run(first_menu_, 
                    "Select starting point or write new vehicle type :", 
                    out string vt);
                if (sel < 0) continue;
                if (sel == first_menu_.Count)
                {
                    if (first_menu_.Contains(vt)) continue;
                    record_.VehicleType = vt;
                    target_ = new Vehicle(ig_, record_);
                }
                else 
                {
                    target_ = vehicle_menu_[sel];
                    record_ = vehicle_menu_[sel].Record();
                    EnforceUniqueOfficialId(record_.OfficialId);
                }
                second_menu_ = new();
                MakeSecondMenu();
                int sel2 = Menu.Run(second_menu_, "Select field to edit or save :", out string text);
                if (sel2 < 0) continue;
                if (sel2 >= second_menu_args_.Count) continue;
                if (!ProcessSecondMenu(second_menu_args_[sel2])) continue;
                if (record_.OurId == 0) continue;
                return record_.OurId;
            }
        }
        bool ProcessSecondMenu(string field)
            // / = save as vehicle_type.
            // + = save as BrandModel.
            // * = register/save as OurId.
        {
            switch (field)
            {
                case "/": SaveAsVehicleType(); return false;
                case "+": SaveAsBrandModel(); return false;
                case "*": Save(); return true;
                default: 
                    EditField(field);
                    CreateTarget();
                    return false;
            }
        }

        void EnforceUniqueOfficialId(string oi)
        {
            if (oi.Length == 0) return;
            var oiIndex = Database.GetIndex("OfficialId", "");
            if (!oiIndex.ContainsKey(oi)) return;
            var ourids = oiIndex[oi];
            if (ourids == null || ourids.Count == 0) return;
            List<IVehicle> dups = new();
            foreach (int o in ourids)
            {
                if (o != record_.OurId)
                {
                    IVehicle? v = Get(o);
                    if (v == null) continue; 
                    dups.Add(v);
                }
            }
            if (dups.Count == 0) return;
            Console.WriteLine("Database error: Duplicate OfficialId.");
            Console.WriteLine("Edit or delete the following vehicles:");
            foreach (var v in dups)
            {
                Console.WriteLine(v.Line());
            }
            throw new DBException();
        }

        string EditVehicleType()
        {
            List<string> vtypes = vehtypes_.Set.ToList();
            string value = RecordField("VehicleType");
            string prompt = $"VehicleType [{value}] :";
            int sel = Menu.Run(vtypes, prompt, out string vt);
            if (sel < 0) return value;
            if (sel >= vtypes.Count) return vt;
            return vtypes[sel];
        }

        string EditBrandModel()
        {
            List<string> brands = brandmodel_.Set.ToList();
            string value = RecordField("BrandModel");
            int sel = Menu.Run(brands, "BrandModel [{value}] :", out string bm);
            if (sel < 0) return value;
            if (sel >= brands.Count) return bm;
            return brands[sel];
        }

        string EditColor()
        {
            List<string> colors = color_.Set.ToList();
            string value = RecordField("Color");
            int sel = Menu.Run(colors, $" Color [{value}] :", out string c);
            if (sel < 0) return value;
            if (sel >= colors.Count) return c;
            return colors[sel];

        }

         private void EditField(string field)
        {
            string value = RecordField(field);
            switch (field)
            {
                case "VehicleType":
                    value = EditVehicleType();
                    break;
                case "BrandModel":
                    value = EditBrandModel();
                    break;
                case "Color":
                    value = EditColor();
                    break;
                default:
                    string prompt = $"Value for {field} [{value}] :";
                    int sel = Menu.Run(new(), prompt, out string v);
                    if (sel == 0) value = v;
                    break;

            }
            SetField(field, value);
        }

        void CreateTarget()
        {
            switch (record_.VehicleType)
            {
                case "Bike":
                    target_ = new Bike(ig_, record_);
                    return;
                case "Boat":
                    target_ = new Boat(ig_, record_);
                    return;
                case "Bus":
                    target_ = new Bus(ig_, record_);
                    return;
                case "Car":
                    target_ = new Car(ig_, record_);
                    return;
                case "Moto":
                    target_ = new Moto(ig_, record_);
                    return;
                case "Plane":
                    target_ = new Plane(ig_, record_);
                    return;
                default:
                    target_ = new Vehicle(ig_, record_);
                    return;
            }
        }
        void SaveAsVehicleType()
        {
            if (record_.VehicleType.Length == 0) return;
            var saved = record_;
            record_.OurId = 0;
            record_.OfficialId = "";
            CreateTarget();
            record_ = saved;
            if (target_ == null) return;
            string filename = VehicleTypeTemplateFilename(record_.VehicleType);
            if (!target_!.Save(filename)) return;
            vehtypes_.Set.Add(record_.VehicleType);
            vehtypes_.Save();
            color_.Set.Add(record_.Color);
            color_.Save();
        }
        void SaveAsBrandModel()
        {
            if (record_.VehicleType.Length == 0) return;
            if (record_.BrandModel.Length == 0) return;
            if (!vehtypes_.Set.Contains(record_.VehicleType)) return;
            var saved = record_;
            record_.OurId = 0;
            record_.OfficialId = "";
            CreateTarget();
            record_ = saved;
            if (target_ == null) return;
            string filename = BrandModelTemplateFilename(record_.BrandModel);
            if (!target_.Save(filename)) return;
            brandmodel_.Set.Add(record_.BrandModel);
            brandmodel_.Save();
            color_.Set.Add(record_.Color);
            color_.Save();
        }
        void Save()
        {

            EnforceUniqueOfficialId(record_.OfficialId);
            if (record_.OurId == 0) record_.OurId = ig_.NewNumber();
            CreateTarget();
            if (target_ == null) return;
            target_.Save();
        }

        public string Line(int ourid)
        {
            if (ourid == 0) return "";
            var v = Get(ourid);
            if (v == null) return "";
            return v.Line();
        }
    }
}
