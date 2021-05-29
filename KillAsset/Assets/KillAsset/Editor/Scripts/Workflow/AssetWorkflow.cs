using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KA
{
    [WorkflowIgnore]
    public class AssetWorkflow : IWorkflow
    {
        public virtual void Run() { }
        public virtual void OnGUI(MainWindow window) { }
    }

    [WorkflowOverride("无用资源清理")]
    public class UnuselessWorkflow : AssetWorkflow
    {
        public override void Run()
        {
            SerializeBuildInfo.Inst.AllAssetPaths.ForEach(v =>
            {
                AssetTreeElement element = AssetTreeHelper.CreateAssetElement(v, 0);
                SerializeBuildInfo.Inst.AddItem(element);

                AssetTreeHelper.CollectAssetDependencies(v, 0);
            });

            EditorUtility.ClearProgressBar();
        }

        public override void OnGUI(MainWindow window)
        {
            int selected = GUI.Toolbar(GetToolBarRect(window), _toolbarSelected, Enum.GetNames(typeof(AssetShowMode)));
            if (_toolbarSelected != selected)
            {
                _toolbarSelected = selected;
                RefreshTreeView(window);
            }

            DrawDeleteBtnInfo(window);
        }

        private void DrawDeleteBtnInfo(MainWindow window)
        {
            if (_toolbarSelected != (int)AssetShowMode.Unuse)
                return;

            if (!window.TreeView.HasSelection())
                return;

            string deleteText = window.TreeView.SelectionObjects.Count > 1 ? "Delete All" : "Delete";
            if (GUI.Button(GetDeleteBtnRect(window.position), deleteText))
            {
                bool isOK = EditorUtility.DisplayDialog("Warning",
                    "Cannot revert after delete, it's recommended to delete after backup.",
                    "OK",
                    "Cancel");

                if (!isOK)
                    return;

                AssetTreeElement curElement = null;
                try
                {
                    var objects = window.TreeView.SelectionObjects;
                    for (int i = 0; i < objects.Count; i++)
                    {
                        curElement = objects[i];
                        int index = SerializeBuildInfo.Inst.treeList.FindIndex(v => v.id == curElement.id);
                        if (index >= 0)
                        {
                            SerializeBuildInfo.Inst.treeList.RemoveAt(index);
                            if (SerializeBuildInfo.Inst.guidToAsset.ContainsKey(curElement.Guid))
                                SerializeBuildInfo.Inst.guidToAsset.Remove(curElement.Guid);
                        }

                        AssetDatabase.DeleteAsset(objects[i].Path);

                    }

                    RefreshTreeView(window);
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Deleteing have mistake:{0}, Path : {1}", e.Message, curElement.Path);
                    throw e;
                }
            }
        }

        private void RefreshTreeView(MainWindow window)
        {
            var assetList = GetAssetList();
            window.TreeView.treeModel.SetData(assetList);
            window.TreeView.Reload();
            window.Repaint();
        }

        private Rect GetToolBarRect(MainWindow window)
        {
            return new Rect(Helper.WindowParam.WorkflowBoxWidth + 30, 5, 300, 20);
        }

        private Rect GetDeleteBtnRect(Rect position)
        {
            return new Rect(position.width - Helper.WindowParam.RightExpendOffset, position.height - 105, 100, 30);
        }

        private void GetUselessAssets()
        {
            var allAssets = SerializeBuildInfo.Inst.AllAssetPaths;
        }

        private List<AssetTreeElement> GetAssetList()
        {
            List<AssetTreeElement> elements = new List<AssetTreeElement>();
            if (_toolbarSelected == (int)AssetShowMode.Summary)
            {
                elements = SerializeBuildInfo.Inst.treeList;
            }
            else if (_toolbarSelected == (int)AssetShowMode.All)
            {
                AssetTreeHelper.ListToTree(SerializeBuildInfo.Inst.AllAssetPaths, elements);
            }
            else if (_toolbarSelected == (int)AssetShowMode.Used)
            {
                var useList = SerializeBuildInfo.Inst.treeList
                    .Where(v => !string.IsNullOrEmpty(v.Guid))
                    .Where(v => v.parent != null && !v.parent.IsRoot)
                    .Select(v => v.Path).Distinct().ToList();

                AssetTreeHelper.ListToTree(useList, elements);
            }
            else
            {
                List<AssetTreeElement> newList = SerializeBuildInfo.Inst.treeList;

                var usedGuidList = newList
                .Where(v => !string.IsNullOrEmpty(v.Guid))
                .Where(v => v.parent != null && !v.parent.IsRoot)
                .Select(v => v.Guid)
                .Distinct()
                .ToList();

                List<string> unuseList = new List<string>();
                foreach (var item in SerializeBuildInfo.Inst.guidToAsset)
                {
                    if (string.IsNullOrEmpty(item.Value.Path))
                        continue;

                    if (usedGuidList.Contains(item.Key))
                        continue;

                    unuseList.Add(item.Value.Path);
                }

                AssetTreeHelper.ListToTree(unuseList, elements);
            }

            return elements;
        }
        private int _toolbarSelected = 0;
    }
}


