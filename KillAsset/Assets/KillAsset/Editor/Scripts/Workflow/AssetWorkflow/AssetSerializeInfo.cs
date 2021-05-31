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
    public class AssetSerializeInfo
    {
        static AssetSerializeInfo _inst;

        internal static AssetSerializeInfo Inst
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
            _inst = new AssetSerializeInfo();
            _inst._id = 1;

            _inst.AllAssetPaths = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
              .Where(v => !AssetTreeHelper.IgnorePath(v))
              .Select(v => FileUtil.GetProjectRelativePath(v).NormalizePath())
              .ToList();
        }

        internal int BuildID { get { return _id++; }set { _id = value; } }

        internal List<string> AllAssetPaths = new List<string>();

        internal Dictionary<string, AssetTreeElement> guidToAsset = new Dictionary<string, AssetTreeElement>();

        internal Dictionary<string, int> guidToRef = new Dictionary<string, int>();

        public List<AssetTreeElement> treeList = new List<AssetTreeElement>();

        public void Serialize(string selectPath)
        {
            string content = File.ReadAllText(selectPath);
            _inst = JsonUtility.FromJson<AssetSerializeInfo>(content);
        }

        public void AddItem(AssetTreeElement element)
        {
            treeList.Add(element);
            if(!guidToAsset.TryGetValue(element.Guid, out AssetTreeElement value))
            {
                guidToAsset.Add(element.Guid, element);
                CollectFileSize(element);
                if(element.depth != 0)
                    guidToRef.Add(element.Guid, 1);
                else
                    guidToRef.Add(element.Guid, 0);
            }
            else
            {
                if (element.depth != 0)
                    guidToRef[element.Guid]++;
            }
        }

        void CollectFileSize(AssetTreeElement element)
        {
            FileInfo info = new FileInfo(element.Path);
            if (info != null)
                element.Size = info.Length;
        }



        int _id = 1;
    }
}

