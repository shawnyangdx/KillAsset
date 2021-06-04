using System;
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

        internal Dictionary<string, List<string>> guidToRef = new Dictionary<string, List<string>>();
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

        public void Export(List<AssetTreeElement> list, string fileAlias = "")
        {
            string content = "";
            Encoding targetEncoding = Encoding.UTF8;
            if (EditorConfig.Inst.exportType == EditorConfig.ExportType.Json)
            {
                //content = JsonUtility.ToJson(list);
                //targetEncoding = Encoding.UTF8;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < Enum.GetNames(typeof(ColumnType)).Length; i++)
                {
                    sb.Append((ColumnType)i);
                    sb.Append("\t");
                }
                sb.Append("\n");

                for (int i = 0; i < list.Count; i++)
                {
                    sb.Append(list[i].AssetType);
                    sb.Append("\t");
                    sb.Append(list[i].name);
                    sb.Append("\t");
                    sb.Append(list[i].Path);
                    sb.Append("\t");
                    guidToAsset.TryGetValue(list[i].Guid, out AssetTreeElement ele);
                    sb.Append(ele != null ? ele.Size : 0);
                    sb.Append("\t");

                if (guidToRef.TryGetValue(list[i].Guid, out List<string> valList) && valList.Count > 0)
                    {
                        sb.Append(valList.Count);
                        sb.Append("\t");
                    }
                    else
                    {
                        sb.Append(0);
                        sb.Append("\t");
                    }

                    sb.Append("\n");
                }

                content = sb.ToString();
                targetEncoding = Encoding.GetEncoding("GB2312");
            }

            string targetPath = Path.Combine(Application.dataPath, EditorConfig.Inst.OutputPath);
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            string path = string.Format("{0}/{1}_{2}_{3}{4}{5}_{6}{7}.{8}",
                          targetPath,
                          Application.platform,
                          fileAlias,
                          DateTime.Now.Year,
                          DateTime.Now.Month,
                          DateTime.Now.Day,
                          DateTime.Now.Hour,
                          DateTime.Now.Minute,
                          EditorConfig.Inst.dataFileExtension);

            File.WriteAllText(path, content, targetEncoding);
        }

        public void AddDependenceItem(AssetTreeElement element, bool isRoot = false, string incRefPath = "")
        {
            treeList.Add(element);
            if (isRoot)
                return;

            if (!guidToAsset.TryGetValue(element.Guid, out AssetTreeElement value))
            {
                guidToAsset.Add(element.Guid, element);
                AssetTreeHelper.CollectFileSize(element);
                guidToRef[element.Guid] = new List<string>();
            }

            if (!string.IsNullOrEmpty(incRefPath) && guidRefSet.Add(element.Guid))
            {
                if (!guidToRef.TryGetValue(element.Guid, out List<string> valList))
                {
                    valList = new List<string>();
                    valList.Add(incRefPath);
                    guidToRef.Add(element.Guid, valList);
                }
                else
                {
                    valList.Add(incRefPath);
                }
            }
        }


        int _id = 1;
    }
}

