using Newtonsoft.Json;
using SynthesisAPI.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthesisAPI.PreferenceManager
{
    public class JSONEntry : Entry
    {
        public static string Path;

        /// <summary>
        /// Entry for accessing JSON files.
        /// </summary>
        /// <param name="name">Name of entry</param>
        /// <param name="owner">Owners GUID</param>
        /// <param name="perm">Read or Write</param>
        /// <param name="file_path">Filepath on hard drive</param>
        public JSONEntry(string name, Guid owner, Permissions perm, string file_path)
        {
            Init(name, owner, perm);

            Path = file_path;
        }

        public bool Serialize<TObject>(TObject obj, Guid guid)
        {
            if ((int)base.Permissions < 3 && guid != base.Owner)
                throw new Exception(string.Format("You do not have permission to access file \"{0}\"", base.Name));

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, Formatting.Indented));
                File.WriteAllBytes(Path, data);
            } catch
            {
                return false;
            }

            return true;
        }

        public bool Deserialize<TObject>(Guid guid, out TObject result)
        {
            if ((int)base.Permissions < 1 && guid != base.Owner)
                throw new Exception(string.Format("You do not have permission to access file \"{0}\"", base.Name));

            try
            {
                if (File.Exists(Path))
                {
                    byte[] data = File.ReadAllBytes(Path);
                    result = JsonConvert.DeserializeObject<TObject>(Encoding.UTF8.GetString(data));
                    return true;
                }
                else
                {
                    result = default;
                    return false;
                }
            } catch
            {
                result = default;
                return false;
            }
        }
    }
}
