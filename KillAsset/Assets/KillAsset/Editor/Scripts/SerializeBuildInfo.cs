﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

namespace KA
{
    [Serializable]
    public class SerializeBuildInfo
    {
        static SerializeBuildInfo _inst;

        internal static SerializeBuildInfo Inst
        {
            get
            {
                if (_inst == null)
                {
                    Debug.LogError("Serialize info not exist");
                }
                return _inst;
            }
        }

        internal static void Init()
        {
            _inst = new SerializeBuildInfo();
        }

        internal bool HasSerialized { get { return _hasSerialized; } }

        internal DateTime StartTime { get; private set; }

        internal BuildTarget Platform { get; private set; }

        internal string OutputPath { get; private set; }
        //private List<string> buildInAssets = new List<string>();
        public List<AssetTreeElement> sceneAssets = new List<AssetTreeElement>();

        public HashSet<string> usedAssetList = new HashSet<string>();

        #region Build Processor
        public void OnProcessScene(Scene scene)
        {
            string[] dependencies = AssetDatabase.GetDependencies(scene.path, true);
            SceneAssetItem item = new SceneAssetItem(scene);
            item.CollectDependicies();
            sceneAssets.Add(item);
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            OutputPath = report.summary.outputPath;
            StartTime = report.summary.buildStartedAt;
            Platform = report.summary.platform;
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            TimeSpan span = (StartTime - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
            string serializePath = string.Format("{0}/{1}_{2}_{3}.{4}",
                Application.dataPath + "/../",
                ((long)span.TotalSeconds).ToString(),
                Platform.ToString(),
                "SerializeInfo",
                EditorConfig.Instance.dataFileExtension);

            string jsonStr = JsonUtility.ToJson(this);
            using (StreamWriter writter = new StreamWriter(serializePath))
            {
                writter.WriteLine(jsonStr);
            }
        }
        #endregion

        #region serialize
        public void Serialize(string selectPath)
        {
            string content = File.ReadAllText(selectPath);
            _inst = JsonUtility.FromJson<SerializeBuildInfo>(content);
            _inst._hasSerialized = true;
        }

        #endregion


        bool _hasSerialized = false;
    }
}

