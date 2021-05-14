using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

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
            _inst._id = 1;
        }

        internal int BuildID { get { return _id++; }set { _id = value; } }

        internal bool HasSerialized { get { return _hasSerialized; } }

        internal DateTime StartTime { get; private set; }

        internal BuildTarget Platform { get; private set; }

        internal string OutputPath { get; private set; }

        internal List<string> allAssetPaths = new List<string>();

        internal Dictionary<string, AssetTreeElement> guidToAsset = new Dictionary<string, AssetTreeElement>();
        //internal List<string> allAssetGuid = new List<string>();

        public List<AssetTreeElement> treeList = new List<AssetTreeElement>();

        public void CollectAllAssetPaths()
        {
            if(!_hasCollectAllAssets)
            {
                allAssetPaths = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
               .Where(v => !AssetTreeHelper.IgnorePath(v))
               .Select(v => FileUtil.GetProjectRelativePath(v)).ToList();

                _hasCollectAllAssets = true;
            }
        }

        //public void OnPreprocessBuild(BuildReport report)
        //{
        //    OutputPath = report.summary.outputPath;
        //    StartTime = report.summary.buildStartedAt;
        //    Platform = report.summary.platform;
        //}

        //public void OnPostprocessBuild(BuildReport report)
        //{
        //    TimeSpan span = (StartTime - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
        //    string serializePath = string.Format("{0}/{1}_{2}_{3}.{4}",
        //        Application.dataPath + "/../",
        //        ((long)span.TotalSeconds).ToString(),
        //        Platform.ToString(),
        //        "SerializeInfo",
        //        EditorConfig.Instance.dataFileExtension);

        //    string jsonStr = JsonUtility.ToJson(this);
        //    using (StreamWriter writter = new StreamWriter(serializePath))
        //    {
        //        writter.WriteLine(jsonStr);
        //    }
        //}

        public void Serialize(string selectPath)
        {
            string content = File.ReadAllText(selectPath);
            _inst = JsonUtility.FromJson<SerializeBuildInfo>(content);
            _inst._hasSerialized = true;
        }

        public void AddItem(AssetTreeElement element)
        {
            treeList.Add(element);
            if(!guidToAsset.TryGetValue(element.Guid, out AssetTreeElement value))
            {
                guidToAsset.Add(element.Guid, element);
            }
        }

        public string GetPathFromGuid(string guid)
        {
            if (guidToAsset.TryGetValue(guid, out AssetTreeElement value))
            {
                return value.Path;
            }

            return "";
        }


        bool _hasSerialized = false;
        int _id = 1;
        bool _hasCollectAllAssets = false;
    }
}

