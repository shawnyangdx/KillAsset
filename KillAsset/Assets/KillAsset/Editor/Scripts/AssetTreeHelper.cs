using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace KA
{
    public class AssetTreeHelper
    {
        public static List<AssetTreeElement> CollectAssetTreeElement(string path, int depth)
        {
            List<AssetTreeElement> dependencies = new List<AssetTreeElement>();

            string[] depends = AssetDatabase.GetDependencies(path, false);
            for (int i = 0; i < depends.Length; i++)
            {
                AssetTreeElement element = new AssetTreeElement();
                element.id = SerializeBuildInfo.Inst.BuildID;
                element.depth = depth + 1;
                element.name = Path.GetFileName(depends[i]);
                dependencies.Add(element);

                SerializeBuildInfo.Inst.AddItem(element);
                CollectAssetTreeElement(depends[i], element.depth);
            }

            return dependencies;
        }
    }

}

