using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using HP = KA.Helper.WindowParam;

namespace KA
{
    [WorkflowIgnore]
    public class AssetWorkflow : Workflow
    {
        public override void Run() { }
        public override void Clear() { }
    }

    [WorkflowOverride("Useless Clean")]
    public class UselessWorkflow : AssetWorkflow
    {
        public override GUIOptions GuiOptions => new GUIOptions()
        {
            onBottomGUICallback = OnBottomGUICallback,
            onTopGUICallback = OnTopGUICallback,
            onSelectionGUICallback = OnSelectionGUICallback,
        };

        public override void Run()
        {
            AssetSerializeInfo.Init();

            List<string> checkList = null;
            if(FileUtil.GetProjectRelativePath(EditorConfig.Inst.RootPath) != "Assets")
            {
                checkList = Helper.Path.CollectAssetPaths(EditorConfig.Inst.RootPath);
            }

            var root = AssetTreeElement.CreateRoot();
            AssetSerializeInfo.Inst.AddItem(root, true);
            AssetSerializeInfo.Inst.AllAssetPaths.ForEach(v =>
            {
                AssetSerializeInfo.Inst.guidRefSet.Clear();
                if (AssetTreeHelper.TryGetDependencies(v, checkList, out List<string> depends))
                {
                    AssetTreeElement element = AssetTreeHelper.CreateAssetElement(v, 0);
                    AssetSerializeInfo.Inst.AddItem(element);
                    AssetTreeHelper.CollectAssetDependencies(depends, element.depth + 1, checkList);
                }
            });

            EditorUtility.ClearProgressBar();
        }

        public override void Clear()
        {
            if (uselessListCache != null)
            {
                uselessListCache.Clear();
                uselessListCache = null;
            }

            if (usedListCache != null)
            {
                usedListCache.Clear();
                usedListCache = null;
            }

            AssetSerializeInfo.Inst.Clear();
        }

        public override bool CanSearch(TreeElement t)
        {
            if (_toolbarSelected == (int)AssetShowMode.Summary)
            {
                if (!t.parent.IsRoot)
                    return false;
            }

            return true;
        }

        public override void Sort(int columnIndex, bool isAscend)
        {
            List<AssetTreeElement> assetList = MainWindow.Inst.TreeView.treeModel.Data as List<AssetTreeElement>;
            AssetTreeElement root = assetList[0];

            assetList = assetList.Where(v => v.depth == 0).ToList();
            if (columnIndex == (int)ColumnType.Icon)
            {
                if (isAscend)
                    assetList = assetList.OrderBy(v => v.AssetType.ToString()).ToList();
                else
                    assetList = assetList.OrderByDescending(v => v.AssetType.ToString()).ToList();
            }
            else if (columnIndex == (int)ColumnType.Name)
            {
                if (isAscend)
                    assetList = assetList.OrderBy(v => v.name).ToList();
                else
                    assetList = assetList.OrderByDescending(v => v.name).ToList();
            }
            else if (columnIndex == (int)ColumnType.Size)
            {
                if (isAscend)
                {
                    assetList.Sort((l, r) =>
                        {
                            var dic = AssetSerializeInfo.Inst.guidToAsset;
                            dic.TryGetValue(l.Guid, out AssetTreeElement le);
                            dic.TryGetValue(r.Guid, out AssetTreeElement re);
                            long leftSize = le != null ? le.Size : 0;
                            long rightSize = re != null ? re.Size : 0;
                            return leftSize.CompareTo(rightSize);
                        }
                    );
                }
                else
                {
                    assetList.Sort((l, r) =>
                        {
                            var dic = AssetSerializeInfo.Inst.guidToAsset;
                            dic.TryGetValue(l.Guid, out AssetTreeElement le);
                            dic.TryGetValue(r.Guid, out AssetTreeElement re);
                            long leftSize = le != null ? le.Size : 0;
                            long rightSize = re != null ? re.Size : 0;
                            return -leftSize.CompareTo(rightSize);
                        }
                   );
                }
            }
            else if (columnIndex == (int)ColumnType.Ref)
            {
                if (isAscend)
                {
                    assetList.Sort((l, r) =>
                    {
                        var dic = AssetSerializeInfo.Inst.guidToRef;
                        dic.TryGetValue(l.Guid, out int lr);
                        dic.TryGetValue(r.Guid, out int rr);
                        return lr.CompareTo(rr);
                    }
                    );
                }
                else
                {
                    assetList.Sort((l, r) =>
                    {
                        var dic = AssetSerializeInfo.Inst.guidToRef;
                        dic.TryGetValue(l.Guid, out int lr);
                        dic.TryGetValue(r.Guid, out int rr);
                        return -lr.CompareTo(rr);
                    }
                   );
                }
            }

            List<AssetTreeElement> newList = new List<AssetTreeElement>();
            newList.Add(root);
            for (int i = 0; i < assetList.Count; i++)
                RebuildList(assetList[i], newList);

            assetList = newList;
            RefreshTreeView(assetList);
        }

        List<string> CollectRootPath()
        {
            return Directory.GetFiles(EditorConfig.Inst.RootPath, "*.*", SearchOption.AllDirectories)
                .Where(v => !AssetTreeHelper.IgnoreExtension(v))
                .Select(v => FileUtil.GetProjectRelativePath(v).NormalizePath())
                .Where(v => !AssetTreeHelper.IgnoreDirectory(v))
                .ToList();
        }

        void RebuildList(AssetTreeElement element, List<AssetTreeElement> newList)
        {
            newList.Add(element);
            if (!element.hasChildren)
                return;

            for (int i = 0; i < element.children.Count; i++)
            {
                AssetTreeElement assetElement = element.children[i] as AssetTreeElement;
                RebuildList(assetElement, newList);
            }
        }

        private void RefreshTreeView(List<AssetTreeElement> assetList)
        {
            MainWindow.Inst.TreeView.treeModel.SetData(assetList);
            MainWindow.Inst.TreeView.Reload();
        }

        private List<AssetTreeElement> GetAssetList()
        {
            List<AssetTreeElement> elements = new List<AssetTreeElement>();
            if (_toolbarSelected == (int)AssetShowMode.Summary)
            {
                elements = AssetSerializeInfo.Inst.treeList;
            }
            else if (_toolbarSelected == (int)AssetShowMode.All)
            {
                AssetTreeHelper.ListToTree(AssetSerializeInfo.Inst.AllAssetPaths, elements);
            }
            else if (_toolbarSelected == (int)AssetShowMode.Used)
            {
                if(usedListCache == null)
                {
                    usedListCache = new List<string>();
                    foreach (var item in AssetSerializeInfo.Inst.guidToRef)
                    {
                        if (item.Value <= 0)
                            continue;

                        if (AssetSerializeInfo.Inst.guidToAsset.TryGetValue(item.Key, out AssetTreeElement ele))
                        {
                            usedListCache.Add(ele.Path);
                        };
                    }
                }

                AssetTreeHelper.ListToTree(usedListCache, elements);
            }
            else
            {
                if (uselessListCache == null)
                {
                    uselessListCache = new List<string>();
                    foreach (var item in AssetSerializeInfo.Inst.guidToRef)
                    {
                        if (item.Value > 0)
                            continue;

                        if (AssetSerializeInfo.Inst.guidToAsset.TryGetValue(item.Key, out AssetTreeElement ele))
                        {
                            uselessListCache.Add(ele.Path);
                        };
                    }
                }

                AssetTreeHelper.ListToTree(uselessListCache, elements);
            }

            return elements;
        }

        private void OnBottomGUICallback(ref Rect rect)
        {
            if (MainWindow.Inst.TreeView != null && MainWindow.Inst.TreeView.HasSelection())
                rect.height -= 200;
        }

        private void OnTopGUICallback(ref Rect rect)
        {
            var topRect = rect;
            topRect.x = HP.WorkflowBoxWidth + 5;
            topRect.y = 5;
            topRect.width = topRect.height = 20;
            if (GUI.Button(topRect, EditorGUIUtility.IconContent("TreeEditor.Refresh")))
            {
                Clear();
                Run();
                var assetList = GetAssetList();
                RefreshTreeView(assetList);
            }

            topRect.x += 25;
            topRect.width = 300;
            int selected = GUI.Toolbar(topRect, _toolbarSelected, Enum.GetNames(typeof(AssetShowMode)));
            if (_toolbarSelected != selected)
            {
                _toolbarSelected = selected;
                var assetList = GetAssetList();
                RefreshTreeView(assetList);
            }

            topRect.x = rect.width - 5;
            topRect.width = 100;
            if (GUI.Button(topRect, "Export"))
            {
                AssetSerializeInfo.Inst.Export();
            }

            rect.y += 25;
            rect.height -= 25;

        }

        private void OnSelectionGUICallback(ref Rect rect, List<TreeElement> elements)
        {
            if (elements.Count == 1)
            {
                var obj = elements[0] as AssetTreeElement;
                if (obj.Icon != null)
                {
                    GUI.DrawTexture(new Rect(HP.WorkflowBoxWidth, rect.height + 50, 80, 80), obj.Icon);
                    GUI.Label(new Rect(HP.WorkflowBoxWidth + 80, rect.height + 60, rect.width / 2, 20), "Info:");
                    GUI.Label(new Rect(HP.WorkflowBoxWidth + 80, rect.height + 75, rect.width / 2, 20), obj.Path);
                }
                else
                {
                    GUI.Label(new Rect(HP.WorkflowBoxWidth, rect.height + 60, rect.width / 2, 20), "Info:");
                    GUI.Label(new Rect(HP.WorkflowBoxWidth, rect.height + 75, rect.width / 2, 20), obj.Path);
                }
            }
            else
            {
                string msg = string.Format("Select {{{0}}} Items", elements.Count);
                GUI.Label(new Rect(HP.WorkflowBoxWidth, rect.height + 60, rect.width / 2, 20), msg);
            }
        }

        private int _toolbarSelected = 0;
        private List<string> usedListCache = null;
        private List<string> uselessListCache = null;
    }
}


