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
        public static Action<string, float> onCollectDependencies;

        public static int BuildID { get; internal set; } = 0;

        /// <summary>
        /// try get dependencies via path.
        /// </summary>
        /// <param name="path">asset path</param>
        /// <param name="checkList">check asset is in check asset list. 
        /// this param maybe null, 'null' means all asset is check list</param>
        /// <param name="newDepends">find all matching dependencies.</param>
        /// <returns>return true if have depends or no depand but in check list.</returns>
        public static bool TryGetDependencies(
            string path, 
            List<string> checkList, 
            out List<string> newDepends)
        {
            newDepends = new List<string>();
            string[] depends = AssetDatabase.GetDependencies(path, false);
            if(depends.Length == 0)
            {
                return FindCheckList(checkList, path);
            }

            for (int i = 0; i < depends.Length; i++)
            {
                if (string.IsNullOrEmpty(depends[i]))
                    continue;

                depends[i] = depends[i].NormalizePath();
                if (IgnoreExtension(depends[i]))
                    continue;

                if (IgnoreDirectory(depends[i]))
                    continue;

                if (FindCheckList(checkList, depends[i]))
                {
                    newDepends.Add(depends[i]);
                }
            }

            return newDepends.Count > 0 || FindCheckList(checkList, path);
        }

        public static void CollectAssetDependencies(string parentPath, List<string> dependencies, int depth, List<string> checkList = null)
        {
            for (int i = 0; i < dependencies.Count; i++)
            {
                if (TryGetDependencies(dependencies[i], checkList, out List<string> depends))
                {
                    AssetTreeElement element = CreateAssetElement(dependencies[i], depth);
                    AssetSerializeInfo.Inst.AddDependenceItem(element, incRefPath: parentPath);
                    CollectAssetDependencies(dependencies[i], depends, element.depth + 1, checkList);
                }
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

        /// <summary>
        /// get file size
        /// </summary>
        /// <param name="element"></param>
        public static void CollectFileSize(AssetTreeElement element)
        {
            FileInfo info = new FileInfo(element.Path);
            if (info != null)
                element.Size = info.Length;
        }

        /// <summary>
        /// you can use this function convert path list to assetTree element list.
        /// </summary>
        /// <param name="list">path list</param>
        /// <param name="elements">target assetTreeElement list</param>
        /// <param name="onAction">callback invoke</param>
        public static void ListToTree(
            List<string> list, 
            List<AssetTreeElement> elements, 
            Action<AssetTreeElement> onAction = null)
        {
            if (elements == null)
                elements = new List<AssetTreeElement>();

            AssetSerializeInfo.Inst.BuildID = 0;
            var root = AssetTreeElement.CreateRoot();
            elements.Add(root);
            for (int i = 0; i < list.Count; i++)
            {
                var element = CreateAssetElement(list[i], 0);
                elements.Add(element);
                onAction?.Invoke(element);
            }
        }

        /// <summary>
        /// ignore ext via path
        /// </summary>
        public static bool IgnoreExtension(string path)
        {
            List<string> list = EditorConfig.Inst.ignoreExtension;
            if (list.Count == 0)
            {
                return false;
            }

            string ext = Path.GetExtension(path);
            for (int i = 0; i < list.Count; i++)
            {
                if (string.CompareOrdinal(list[i], ext) == 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// ignore directory
        /// </summary>
        public static bool IgnoreDirectory(string path)
        {
            List<string> list = EditorConfig.Inst.ignoreDirectory;
            if (list.Count == 0)
            {
                return false;
            }
            string directory = Path.GetDirectoryName(path).NormalizePath();

            for (int i = 0; i < list.Count; i++)
            {
                if (directory.IndexOf(list[i]) >= 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// create a default multipul column header state.only use for gui.
        /// if you want to extend column, you can add a new column class and insert it in this class.
        /// </summary>
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(EditorGUIUtility.FindTexture("FilterByLabel"), ""),
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


        static bool FindCheckList(List<string> checkList, string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            if (checkList != null)
            {
                if (checkList.Contains(path))
                {
                    return true;
                }
            }
            else
            {
                return true;
            }

            return false;
        }
    }

}

