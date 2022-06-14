using System.Collections;
using System.Collections.Generic;
using Synthesis.ModelManager;
using Synthesis.ModelManager.Models;
using UnityEngine;

namespace Synthesis.ModelManager
{
    public static class ModelManager
    {
        public static Dictionary<string, Model>  Models = new Dictionary<string, Model>();

        public static Field Field { get; set; }

        public delegate void ModelSpawned(Model model);
        public static event ModelSpawned OnModelSpawned;

        public static Vector3 spawnLocation = new Vector3(0, 0.5f, 0);
        public static Quaternion spawnRotation = Quaternion.identity;

        public static Model primaryModel;

        public static void AddModel(string filePath, bool reverseSideMotors = false)
        {
            GizmoManager.ExitGizmo();
            foreach (var kvp in Models)
            {
                kvp.Value.DestroyModel();
            }
            Models.Clear();
            var m = new Model(filePath, spawnLocation,spawnRotation, reverseSideMotors);
            if (OnModelSpawned != null) OnModelSpawned(m);
            Models.Add(m.Name ?? "Placeholder Name", m);
            primaryModel = m;
        }

        public static void AddModel(string name, Model m) {
            Models.Add(name, m);
        }

        public static void SetField(string filePath)
        {
            if (Field != null)
                Field.Destroy();
            Field = new Field(filePath);
        }

        public static void Remove(string modelName)
        {
            Object.Destroy(Models[modelName]);
            Models.Remove(modelName);
        }

        public static void Remove(this Model model)
        {
            Models.Remove(model.Name);
            Object.Destroy(model);
        }
    }
}

