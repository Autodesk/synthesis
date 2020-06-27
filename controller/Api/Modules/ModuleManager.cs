using SynthesisAPI.Utilities;
using System;
using System.Collections.Generic;

namespace SynthesisAPI.Modules
{
    public static class ModuleManager
    {
        [ExposedApi] // TODO make internal somehow?
        public static void AddToLoadedModuleList(string name)
        {
            if (IsModuleLoaded(name))
            {
                // TODO
            }
            Instance.LoadedModules.Add(name);
        }

        public static bool IsFinishedLoading { get => Instance.IsFinishedLoading; }

        public static void MarkFinishedLoading()
        {
            Instance.IsFinishedLoading = true;
        }

        [ExposedApi]
        public static string[] GetLoadedModules()
        {
            return Instance.LoadedModules.ToArray();
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

            public List<string> LoadedModules; // TODO maybe include metadata too (and move that class to the API)?
            public bool IsFinishedLoading { get; set; } = false;

            internal static readonly Inner instance = new Inner();
        }

        private static Inner Instance
        {
            get
            {
                try
                {
                    return Inner.instance;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
