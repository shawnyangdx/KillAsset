using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KA
{
    [PipelineAttr(name: "Asset", group: PipelineGroup.Filtration)]
    public class UnuselessPipeline : IPipeline
    {
        public List<string> GetObjectPath()
        {
            SerializeBuildInfo.Inst.CollectAllAssetPaths();

            return Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
                .Where(v => Path.GetExtension(v) != ".meta")
                .Where(v => Path.GetExtension(v) != ".cs")
                .Select(v => FileUtil.GetProjectRelativePath(v)).ToList();
        }

        public void OnGUI(MainWindow window)
        {
            int selected = GUI.Toolbar(GetToolBarRect(), _toolbarSelected, Enum.GetNames(typeof(AssetShowMode)));
            if(_toolbarSelected != selected)
            {
                _toolbarSelected = selected;
                var assetList = GetAssetList();
                window.TreeView.treeModel.SetData(assetList);
                window.TreeView.Reload();
                window.Repaint();
            }
        }

        private Rect GetToolBarRect()
        {
            return new Rect(MainWindow.LeftExpendWidth + 10, 5, 200, 20);
        }

        private void GetUselessAssets()
        {
            var allAssets = SerializeBuildInfo.Inst.allAssetPaths;
        }

        private List<AssetTreeElement> GetAssetList()
        {
            List<AssetTreeElement> elements = new List<AssetTreeElement>();
            if (_toolbarSelected == (int)AssetShowMode.All)
            {
                AssetTreeHelper.ListToTree(SerializeBuildInfo.Inst.allAssetPaths, elements);
            }
            else if (_toolbarSelected == (int)AssetShowMode.Used)
            {
                //var list = SerializeBuildInfo.Inst.useList;
                //for (int i = 0; i < list.Count; i++)
                //{
                //    elements.Find(v => v == list[i]);
                //}
                //elements = SerializeBuildInfo.Inst.useList.Distinct().ToList();
            }
            else
            {
                var guidList = SerializeBuildInfo.Inst.useList
                    .Where(v => !string.IsNullOrEmpty(v.Guid))
                    .Where(v => !v.hasChildren && v.parent.IsRoot)
                    .Select(v => v.Path).Distinct().ToList();

                AssetTreeHelper.ListToTree(guidList, elements);
            }

            return elements;
        }
        private int _toolbarSelected = -1;

    }

}

