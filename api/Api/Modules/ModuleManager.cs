using SynthesisAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CoreEngine")]
[assembly: InternalsVisibleTo("MockApi")]
[assembly: InternalsVisibleTo("TestApi")]
[assembly: InternalsVisibleTo("PlayMode.Tests")]

namespace SynthesisAPI.Modules
{
    public static class ModuleManager
    {
        public class ModuleInfo
        {
            public string Name { get; }
            public string Version { get; }
            public string Author { get; }
            public string Description { get; }

            public ModuleInfo(string name, string version)
            {
                Name = name;
                Version = version;
                Author = "";
                Description = "";
            }
            public ModuleInfo(string name, string version, string author, string description)
            {
                Name = name;
                Version = version;
                Author = author;
                Description = description;
            }

            public override bool Equals(object obj)
            {
                if(obj is ModuleInfo mod)
                {
                    return mod.Name == Name && mod.Version == Version;
                }
                return false;
            }

            public override int GetHashCode()
            {
                int hashCode = 2112831277;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Version);
                return hashCode;
            }
        }

        [ExposedApi]
        internal static void AddToLoadedModuleList(ModuleInfo module)
        {
            if (!IsModuleLoaded(module))
            {
                Instance.LoadedModules.Add(module);
            }
        }

        public static bool IsFinishedLoading { get => Instance.IsFinishedLoading; }

        internal static void MarkFinishedLoading()
        {
            Instance.IsFinishedLoading = true;
        }

        [ExposedApi]
        public static List<ModuleInfo> GetLoadedModules()
        {
            return Instance.LoadedModules;
        }

        [ExposedApi]
        public static bool IsModuleLoaded(ModuleInfo module)
        {
            return Instance.LoadedModules.Contains(module);
        }

        [ExposedApi]
        public static string GetDeclaringModule(Type type)
        {
            var callSite = type.Assembly.GetName().Name;
            return Instance.AssemblyOwners.ContainsKey(callSite) ? Instance.AssemblyOwners[callSite] : $"{callSite}.dll";
        }

        internal static void RegisterModuleAssemblyName(string assemblyName, string owningModule)
        {
            Instance.AssemblyOwners.Add(assemblyName, owningModule);
        }

        private class Inner
        {
            public Inner()
            {
                LoadedModules = new List<ModuleInfo>();
                AssemblyOwners = new Dictionary<string, string>();
            }

            public List<ModuleInfo> LoadedModules;
            public Dictionary<string, string> AssemblyOwners;
            public bool IsFinishedLoading { get; set; }

            public static readonly Inner Instance = new Inner();
        }

        private static Inner Instance => Inner.Instance;
    }
}
