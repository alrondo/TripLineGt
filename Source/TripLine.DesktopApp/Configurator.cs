using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TripLine.DesktopApp
{
    
    public class Configurator
    {
        public string _fileName;
        public Dictionary<string, string> _config = new Dictionary<string, string>();

        private string _path;

        public Configurator(string fileName)
        {
            _fileName = Path.GetFileName(fileName);
            _path = Path.GetDirectoryName(fileName); //Environment.SpecialFolder.ApplicationData.GetPath().PathCombine("Dp Configuration").PathCombine(_fileName);

            //if (File.Exists(fileName))
            //{
            //    _config = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(fileName),
            //        new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Ignore });
            //}
        }

        public virtual T Load<T>(string configObject)
        {
            //if (_config.ContainsKey(configObject))
            //{
            //    return (T)JsonConvert.DeserializeObject(_config[configObject], typeof(T));
            //}
            return default(T);
        }

        public virtual void Save<T>(T obj, string configObject)
        {
            //_config[configObject] = JsonConvert.SerializeObject(obj);

            //SaveDict();
        }

        private void SaveDict()
        {
            //_path.Dir().CreateIfNotExist();

            //File.WriteAllText(_path.PathCombine(_fileName), JsonConvert.SerializeObject(_config));
        }
    }
}
