using System;
using System.Collections.Generic;
using UnityEngine;

namespace KA
{
    public class WorkflowOverrideAttribute : Attribute
    {
        public string Name { get; private set; } = "";

        public WorkflowOverrideAttribute(string name)
        {
            this.Name = name;
        }
    }

    public class WorkflowIgnoreAttribute : Attribute { }

    public abstract class Workflow
    {
        //when select workflow , this function will excute.
        public abstract void Run();
        //when you change another worlflow or main window is disposed.
        public abstract void Clear();

        public class GUIOptions
        {
            public bool showSearchField = true;

            public OnCustomGUI onTopGUICallback;
            public OnCustomGUI onBottomGUICallback;
            public OnSelectGUI<TreeElement> onSelectionGUICallback;

            public delegate void OnCustomGUI(ref Rect rect);
            public delegate void OnSelectGUI<T>(ref Rect rect, List<T> elements, bool lastChanged) where T : TreeElement;
        }

        public virtual GUIOptions GuiOptions { get; } = new GUIOptions();
        public virtual bool CanSearch(TreeElement t) { return false; }
        public virtual void Sort(int columnIndex, bool isAscend) { }
    }


}

