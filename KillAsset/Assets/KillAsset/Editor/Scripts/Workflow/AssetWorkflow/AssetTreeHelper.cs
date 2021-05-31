using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;

namespace KA
{
    public class AssetTreeHelper
    {
        public static Action<string, int> onCollectDependencies;

        public static void CollectAssetDependencies(string path, int depth)
        {
            onCollectDependencies?.Invoke(path, depth);
            string[] depends = AssetDatabase.GetDependencies(path, false);
            for (int i = 0; i < depends.Length; i++)
            {
                if (IgnorePath(depends[i]))
                    continue;

                AssetTreeElement element = CreateAssetElement(depends[i], depth + 1);
                AssetSerializeInfo.Inst.AddItem(element);

                CollectAssetDependencies(depends[i], element.depth);
            }
        }

        public static AssetTreeElement CreateAssetElement(string path, int depth)
        {
            AssetTreeElement element = new AssetTreeElement
            {
                id = AssetSerializeInfo.Inst.BuildID,
                depth = depth,
                name = Path.GetFileName(path),
                Path = path,
                Guid = AssetDatabase.AssetPathToGUID(path),
                AssetType = AssetDatabase.GetMainAssetTypeAtPath(path)
            };

            return element;
        }

        public static void ListToTree(List<string> list, List<AssetTreeElement> elements)
        {
            AssetSerializeInfo.Inst.BuildID = 0;
            var root = AssetTreeElement.CreateRoot();
            elements.Add(root);
            for (int i = 0; i < list.Count; i++)
            {
                var element = CreateAssetElement(list[i], 0);
                elements.Add(element);
            }
        }

        public static bool IgnorePath(string path)
        {
            List<string> list = EditorConfig.Instance.ignoreExtension;
            string ext = Path.GetExtension(path);

            for (int i = 0; i < list.Count; i++)
            {
                if (string.CompareOrdinal(list[i], ext) == 0)
                    return true;
            }

            return false;
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
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Path"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 200,
                    minWidth = 80,
                    autoResize = false,
                    allowToggleVisibility = false,
                    canSort = true,
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Size"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 80,
                    minWidth = 60,
                    maxWidth = 120,
                    autoResize = false,
                    allowToggleVisibility = false
                },

                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Ref"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 80,
                    minWidth = 60,
                    maxWidth = 120,
                    autoResize = false,
                    allowToggleVisibility = false
                },
            };

            Assert.AreEqual(columns.Length, Enum.GetValues(typeof(ColumnType)).Length, "Number of columns should match number of enum values: You probably forgot to update one of them.");

            var state = new MultiColumnHeaderState(columns);
            return state;
        }
    }

}

