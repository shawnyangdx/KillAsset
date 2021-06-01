using System;

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
        public abstract void Run();
        public abstract void OnGUI();
        public abstract void Clear();

        public virtual bool CanSearch(TreeElement t) { return false; }
        public virtual void Sort(int columnIndex, bool isAscend) { }
    }

}

