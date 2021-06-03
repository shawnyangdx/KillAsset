﻿using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

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

            _inst.AllAssetPaths = Helper.Path.CollectAssetPaths(Application.dataPath);
        }

        internal int BuildID { get { return _id++; }set { _id = value; } }

        internal List<string> AllAssetPaths = new List<string>();

        internal Dictionary<string, AssetTreeElement> guidToAsset = new Dictionary<string, AssetTreeElement>();

        internal Dictionary<string, int> guidToRef = new Dictionary<string, int>();
        //use for calculate reference.
        internal HashSet<string> guidRefSet = new HashSet<string>();

        public List<AssetTreeElement> treeList = new List<AssetTreeElement>();

        public void Clear()
        {
            treeList.Clear();
            guidToAsset.Clear();
            guidRefSet.Clear();
            _id = 1;
        }

        public void Export()
        {
            string content = "";
            Encoding targetEncoding;
            if (EditorConfig.Inst.exportType == EditorConfig.ExportType.Json)
            {
                content = JsonUtility.ToJson(this);
                targetEncoding = Encoding.UTF8;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < Enum.GetNames(typeof(ColumnType)).Length; i++)
                {
                    sb.Append((ColumnType)i);
                    sb.Append("\t");
                }

                content = sb.ToString();
                targetEncoding = Encoding.GetEncoding("GB2312");
            }

            string targetPath = Path.Combine(Application.dataPath, EditorConfig.Inst.OutputPath);
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            string path = string.Format("{0}/{1}_{2}{3}{4}_{5}{6}.{7}",
                          targetPath,
                          Application.platform,
                          DateTime.Now.Year,
                          DateTime.Now.Month,
                          DateTime.Now.Day,
                          DateTime.Now.Hour,
                          DateTime.Now.Minute,
                          EditorConfig.Inst.dataFileExtension);

            File.WriteAllText(path, content, targetEncoding);
        }

        public void AddItem(AssetTreeElement element, bool isRoot = false, bool incRef = false)
        {
            treeList.Add(element);
            if (isRoot)
                return;

            if (!guidToAsset.TryGetValue(element.Guid, out AssetTreeElement value))
            {
                guidToAsset.Add(element.Guid, element);
                CollectFileSize(element);
                guidToRef[element.Guid] = 0;
            }

            if (incRef && guidRefSet.Add(element.Guid))
            {
                if (!guidToRef.TryGetValue(element.Guid, out int val))
                {
                    guidToRef.Add(element.Guid, 1);
                }
                else
                {
                    guidToRef[element.Guid]++;
                }
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

