using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorConfig : ScriptableObject
{
    private static EditorConfig _instance;
    public static EditorConfig Instance
    {
        get
        {
            if (_instance == null)
                _instance = LoadData();

            if (_instance == null)
                Debug.LogErrorFormat("[KA]Missing Editor Config Data.");
            return _instance;
        }
    }

    private static EditorConfig LoadData()
    {
        string[] configData = AssetDatabase.FindAssets("EditorConfig t:" + typeof(EditorConfig).ToString(), null);
        if (configData.Length >= 1)
        {
            return (EditorConfig)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(configData[0]), typeof(EditorConfig));
        }

        return null;
    }

    public string SceneRootPath = "";
    public string dataFileExtension = ".kainfo";

}
