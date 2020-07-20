using SynthesisAPI.Utilities;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CoreEngine")]

namespace SynthesisAPI.Modules
{
    public static class ModuleManager
    {
        internal static void AddToLoadedModuleList(string name)
        {
            if (IsModuleLoaded(name))
            {
                throw new System.Exception($"Adding already loaded module to list of loaded modules: {name}");
            }
            Instance.LoadedModules.Add(name);
        }

        [ExposedApi]
        public static bool IsFinishedLoading { get => Instance.IsFinishedLoading; }

        internal static void MarkFinishedLoading()
        {
            Instance.IsFinishedLoading = true;
        }

        [ExposedApi]
        public static List<string> GetLoadedModules()
        {
            return Instance.LoadedModules;
        }

        [ExposedApi]
        public static bool IsModuleLoaded(string name)
        {
            return Instance.LoadedModules.Contains(name);
        }

        private class Inner
        {
            public Inner()
            {
                LoadedModules = new List<string>();
            }

            public List<string> LoadedModules;
            public bool IsFinishedLoading { get; internal set; }

            public static readonly Inner Instance = new Inner();
        }

        private static Inner Instance => Inner.Instance;
    }
}
