using System;

namespace KA
{
    public interface IWorkflow
    {
        void Run();
        void OnGUI(MainWindow window);
    }

    public interface IWorkflowSearch<T> where T : TreeElement
    {
        bool CanSearch(T t);
    }

    public class WorkflowOverrideAttribute : Attribute
    {
        public string Name { get; private set; } = "";

        public WorkflowOverrideAttribute(string name)
        {
            this.Name = name;
        }
    }

    public class WorkflowIgnoreAttribute : Attribute { }
}

