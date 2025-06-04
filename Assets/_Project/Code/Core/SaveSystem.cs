using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Project.Core
{
    public interface ISaveSystem
    {
        void WriteData<TData>(string key, TData value);
        TData GetData<TData>(string key, TData defaultValue = default);
        bool HasKey(string key);
        void DeleteKey(string key);
        void DeleteAll();
    }

    public class SaveSystem : ISaveSystem
    {
        private const string FILE_NAME = "save.json";
        private readonly string _filePath;
        private Dictionary<string, object> _data = new ();

        public SaveSystem()
        {
            _filePath = Path.Combine(Application.persistentDataPath, FILE_NAME);
            LoadFromFile();
        }

        public void WriteData<TData>(string key, TData data)
        {
            _data[key] = data;
            SaveToFile();
        }

        public TData GetData<TData>(string key, TData defaultValue = default)
        {
            if (_data.TryGetValue(key, out var value))
            {
                if (value is TData tValue)
                    return tValue;

                return JsonConvert.DeserializeObject<TData>(value.ToString());
            }
            return defaultValue;
        }

        public bool HasKey(string key) => 
            _data.ContainsKey(key);

        public void DeleteKey(string key)
        {
            if (_data.Remove(key))
                SaveToFile();
        }

        public void DeleteAll()
        {
            _data.Clear();
            SaveToFile();
        }

        private void SaveToFile()
        {
            var json = JsonConvert.SerializeObject(_data, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }

        private void LoadFromFile()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
            }
        }
    }
}