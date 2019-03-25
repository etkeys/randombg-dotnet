using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace randombg_dotnet{
    using Entity = Dictionary<string, IDbRecord>;
    
    public class Database{
        private enum DbExistsOnFileSystem{
            Unknown=-1,
            False,
            True
        }
        private readonly string _DB_NAME = "randombg.db";
        private readonly string _PATH_TO_DB = 
            Path.Combine("/","home","erik","repos","randombg-dotnet");

        private DbExistsOnFileSystem _dbExistsOnFs = 
            DbExistsOnFileSystem.Unknown;
        private string _dbFullName;
        private Entity _images = new Entity();
        private bool _isDbLoaded = false;

        
        public Database(){
            _dbFullName = Path.Combine(_PATH_TO_DB, _DB_NAME);
            _dbExistsOnFs = File.Exists(Path.Combine(_dbFullName)) ?
                DbExistsOnFileSystem.True :
                DbExistsOnFileSystem.False;
        }

        public void Create(){
            if (File.Exists(_dbFullName))
                throw new InvalidOperationException(
                    $"Cannot create {_dbFullName} because it already exists.");

            using(var temp = File.Create(_dbFullName)){}
            _dbExistsOnFs = DbExistsOnFileSystem.True;
            Console.WriteLine($"Created datatabase: {_dbFullName}");
        }

        public List<ImageRecord> GetUnusedImages(){
            if (!_isDbLoaded) Load();
            
            return _images.Select(kvp => kvp.Value)
                    .Where(i => Boolean.Parse(i["HasBeenUsed"]) == false)
                    .Cast<ImageRecord>()
                    .ToList();

        }

        public void InsertNewRecords(string table, IEnumerable<IDbRecord> recs){
            if (!_isDbLoaded) Load();

            bool append = _images.Count > 0;

            var distincts = recs.Distinct();

            long inserted = 0;
            foreach(var item in distincts)
                if (!append || !_images.ContainsKey(item.Key)){
                    _images.Add(item.Key, item);
                    inserted++;
                }

            Console.WriteLine($"{inserted} rows inserted");
        }

        public void Load(){
            if (_isDbLoaded) return;

            if (_dbExistsOnFs == DbExistsOnFileSystem.False)
                Create();

            using (StreamReader sr = new StreamReader(_dbFullName)){
                string line, table = String.Empty;
                while ((line = sr.ReadLine()) != null){
                    if (IsIgnoreableLine(line))
                        continue;

                    if (IsTableHeader(line)){
                        table = GetTableName(line);
                    }else{
                        switch(table.ToUpper()){
                            case "IMAGES":
                                var record = new ImageRecord(line);
                                _images.Add(record.Key, record);
                                break;
                            default:
                                throw new ArgumentException(
                                        $"Unhandled database table {table}");
                            
                        }
                    }
                }
            }

            _isDbLoaded = true;
        }

        public bool IsIgnoreableLine(string line){
            return String.IsNullOrWhiteSpace(line) || line.StartsWith("#");
        }

        public bool IsTableHeader(string line){
            return line.StartsWith("[") && line.EndsWith("]");
        }

        public string GetTableName(string line){
            // Sample: [images]
            return line.Substring(1, line.Length-2);
        }

        public void Save(){
            if (_dbExistsOnFs == DbExistsOnFileSystem.True && !_isDbLoaded) return;

            using (StreamWriter sw = new StreamWriter(_dbFullName, false)){
                sw.WriteLine($"[images]");
                foreach(var kvp in _images)
                    sw.WriteLine(kvp.Value.ToDbString());
            }
        }

    }
}
