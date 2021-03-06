﻿using System.Collections;
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

        public static void AddModel(string filePath)
        {
            // Model m = Parse.AsModel(filePath);
            // //Model m = new Model(filePath);
            // OnModelSpawned(m);
            // Models.Add(m.Name,m);
        }

        public static void AddModel(string name, Model m) {
            Models.Add(name, m);
        }

        public static void SetField(string filePath)
        {
            Field = Parse.AsField(filePath);
            //Field = new Field(filePath);
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

