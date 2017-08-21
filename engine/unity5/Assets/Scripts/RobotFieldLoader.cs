using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RobotFieldLoader {
    
    /// <summary>
    /// Loads the field from a given directory
    /// </summary>
    /// <param name="directory">field directory</param>
    /// <returns>whether the process was successful</returns>
    public static bool LoadField(string directory = null)
    {
        string fieldPath;
        GameObject fieldObject;
        UnityFieldDefinition fieldDefinition;
        
        if (directory == null)
            directory = PlayerPrefs.GetString("simSelectedField");
        
        fieldPath = directory;

        fieldObject = new GameObject("Field");

        FieldDefinition.Factory = delegate (Guid guid, string name)
        {
            return new UnityFieldDefinition(guid, name);
        };

        string loadResult;
        fieldDefinition = (UnityFieldDefinition)BXDFProperties.ReadProperties(directory + "\\definition.bxdf", out loadResult);
        Debug.Log(loadResult);
        fieldDefinition.CreateTransform(fieldObject.transform);
        
        return fieldDefinition.CreateMesh(directory + "\\mesh.bxda");
    }
}