using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace KA
{
    class AssetTreeView : TreeViewWithTreeModel<AssetTreeElement>
    {
        public AssetTreeView(TreeViewState state, TreeModel<AssetTreeElement> model)
            : base(state, model)
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
        }
    }

}
