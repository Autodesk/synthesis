using SynthesisAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CoreEngine")]
[assembly: InternalsVisibleTo("MockApi")]

namespace SynthesisAPI.Modules
{
    public static class ModuleManager
    {
        public class ModuleInfo
        {
            public string Name { get; }
            public string Version { get; }
            public ModuleInfo(string name, string version)
            {
                Name = name;
                Version = version;
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

        private class Inner
        {
            public Inner()
            {
                LoadedModules = new List<ModuleInfo>();
            }

            public List<ModuleInfo> LoadedModules;
            public bool IsFinishedLoading { get; set; }

            public static readonly Inner Instance = new Inner();
        }

        private static Inner Instance => Inner.Instance;
    }
}
