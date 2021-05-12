using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace KA
{
    enum ColumnType
    {
        Icon1,
        Name,
    }

    class AssetTreeView : TreeViewWithTreeModel<AssetTreeElement>
    {
        static float _spaceLabel = 2f;


        public AssetTreeView(TreeViewState state, MultiColumnHeader multicolumnHeader, TreeModel<AssetTreeElement> model)
            : base(state, multicolumnHeader, model)
        {
            rowHeight = 20;
            columnIndexForTreeFoldouts = 1;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset = (20 - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
            extraSpaceBeforeIconAndLabel = _spaceLabel;
            //multicolumnHeader.sortingChanged += OnSortingChanged;

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


        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<AssetTreeElement>)args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (ColumnType)args.GetColumn(i), ref args);
            }
        }

        void CellGUI(Rect cellRect, TreeViewItem<AssetTreeElement> item, ColumnType column, ref RowGUIArgs args)
        {
            // Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {
                case ColumnType.Icon1:
                    {
                        GUI.DrawTexture(cellRect, DetermineIconType(item.data), ScaleMode.ScaleToFit);
                    }
                    break;
                case ColumnType.Name:
                    {
                        // Do toggle
                        // Default icon and label
                        //var nameRect = cellRect;
                        //nameRect.x += GetContentIndent(item);
                        args.rowRect = cellRect;
                        base.RowGUI(args);
                    }
                    break;
            }
        }

        Texture2D DetermineIconType(AssetTreeElement element)
        {
            switch (element.GetAssetType())
            {
                case AssetType.None:
                    return null;
                case AssetType.Scene:
                    Texture2D sceneTex = EditorGUIUtility.FindTexture("sceneasset icon.asset");
                    return sceneTex;
                case AssetType.Prefab:
                    return null;
                default:
                    break;
            }

            return null;
        }   
    }

}
