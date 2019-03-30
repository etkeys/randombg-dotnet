using System;

namespace randombg_dotnet{
    using DbString = String;
    using KeyNotFoundException = 
        System.Collections.Generic.KeyNotFoundException;

    public interface IDbRecord{
        string Key{get;}
        string ToDbString();
        string this[string prop]{get;}
    }

    public struct ImageRecord: IDbRecord, IEquatable<IDbRecord>{
        private const string _DELIM = ";";
        public bool HasBeenUsed;
        public string Name, Url;

        public string Key{get{return Name;}}
        public string this[string prop]{
            get{
                switch(prop){
                    case "HasBeenUsed":
                        return HasBeenUsed.ToString();
                    case "Name":
                        return Name;
                    case "Url":
                        return Url;
                    default:
                        throw new KeyNotFoundException(
                            String.Format(
                                "{0} does not contain property {1}",
                                this.GetType().ToString(),
                                prop));
                }
            }
        }

        public ImageRecord(DbString dbstring){
            string[] parts = dbstring.Split(_DELIM);
            HasBeenUsed = Boolean.Parse(parts[0]);
            Name = parts[1];
            Url = parts[2];
        }
        public ImageRecord(bool hasBeenUsed, string name, string url){
            HasBeenUsed = hasBeenUsed;
            Name = name;
            Url = url;
        }

        public ImageRecord ToggleHasBeenUsed(){
            return new ImageRecord(!HasBeenUsed, Name, Url);
        }

#region IEquatable<IDbRecord> Implementation
        public bool Equals(IDbRecord other){
            if (Object.ReferenceEquals(other, null)) return false;
            if (Object.ReferenceEquals(this, other)) return true;
            return Key.Equals(other.Key);
        }

        public override int GetHashCode(){
            return Key == null ? 0 : Key.GetHashCode();
        }
#endregion

        public string ToDbString() {
            return string.Join(
                    _DELIM,
                    HasBeenUsed.ToString(),
                    Name,
                    Url);
        }
        public override string ToString(){ return ToDbString(); }
    }

}
