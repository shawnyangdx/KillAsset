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

            _inst.AllAssetPaths = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
              .Where(v => !AssetTreeHelper.IgnorePath(v))
              .Select(v => FileUtil.GetProjectRelativePath(v).NormalizePath())
              .ToList();
        }

        internal int BuildID { get { return _id++; }set { _id = value; } }

        internal bool HasSerialized { get { return _hasSerialized; } }

        internal List<string> AllAssetPaths = new List<string>();

        internal Dictionary<string, AssetTreeElement> guidToAsset = new Dictionary<string, AssetTreeElement>();

        public List<AssetTreeElement> treeList = new List<AssetTreeElement>();

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

