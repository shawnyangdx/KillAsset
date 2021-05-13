using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace KA
{
    public class AssetTreeHelper
    {
        public static void CollectAssetDependencies(string path, int depth)
        {
            string[] depends = AssetDatabase.GetDependencies(path, false);
            for (int i = 0; i < depends.Length; i++)
            {
                if (Path.GetExtension(depends[i]) == ".cs" ||
                    Path.GetExtension(depends[i]) == ".meta")
                    return;

                AssetTreeElement element = CreateAssetElement(depends[i], depth + 1);
                SerializeBuildInfo.Inst.AddItem(element);

                CollectAssetDependencies(depends[i], element.depth);
            }
        }

        public static AssetTreeElement CreateAssetElement(string path, int depth)
        {
            AssetTreeElement element = new AssetTreeElement
            {
                id = SerializeBuildInfo.Inst.BuildID,
                depth = depth,
                name = Path.GetFileName(path),
                Path = path,
                Guid = AssetDatabase.AssetPathToGUID(path)
            };

            var extension = Path.GetExtension(path);
            switch (extension)
            {
                case "unity":
                    element.AssetType = (int)AssetType.Scene;
                    Debug.Log(extension);
                    break;
                default:
                    break;
            }

            return element;
        }

        public static void ListToTree(List<string> list, List<AssetTreeElement> elements)
        {
            SerializeBuildInfo.Inst.BuildID = 0;
            var root = AssetTreeElement.CreateRoot();
            elements.Add(root);
            for (int i = 0; i < list.Count; i++)
            {
                var element = CreateAssetElement(list[i], 0);
                elements.Add(element);
            }
        }



        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(EditorGUIUtility.FindTexture("FilterByLabel"), "Lorem ipsum dolor sit amet, consectetur adipiscing elit. "),
                    contextMenuText = "Asset",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 30,
                    minWidth = 30,
                    maxWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Name"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 150,
                    minWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = false
                },
            };

            //Assert.AreEqual(columns.Length, Enum.GetValues(typeof(MyColumns)).Length, "Number of columns should match number of enum values: You probably forgot to update one of them.");

            var state = new MultiColumnHeaderState(columns);
            return state;
        }
    }

}

