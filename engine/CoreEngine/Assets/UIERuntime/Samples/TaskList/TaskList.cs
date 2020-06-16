using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.UIElements.Runtime;

namespace UIElementsExamples
{
    public class TaskList : PanelRenderer
    {
        [SerializeField] List<string> m_Tasks;

        TextField m_TextField;
        ScrollView m_TasksContainer;

        public void AddTask(KeyDownEvent e)
        {
            if (e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Return)
            {
                AddTask();
                // Prevent the text field from handling this key.
                e.StopPropagation();
            }
        }

        public new void Start()
        {
            base.Start();

            var root = visualTree;

            var popupWindow = new PopupWindow();
            popupWindow.text = "New Task";
            root.Add(popupWindow);

            m_TextField = new TextField { name = "input", viewDataKey = "input", isDelayed = true };
            popupWindow.Add(m_TextField);
            m_TextField.RegisterCallback<KeyDownEvent>(AddTask);

            var button = new Button(AddTask) { text = "Save task" };
            popupWindow.Add(button);

            var box = new Box();
            m_TasksContainer = new ScrollView();
            m_TasksContainer.showHorizontal = false;
            box.Add(m_TasksContainer);

            root.Add(box);
        }

        public new void OnEnable()
        {
            base.OnEnable();

            if (m_Tasks != null)
            {
                foreach (string task in m_Tasks)
                {
                    m_TasksContainer.Add(CreateTask(task));
                }
            }
        }

        public new void OnDisable()
        {
            m_Tasks = new List<string>();
            foreach (VisualElement task in m_TasksContainer.Children())
            {
                m_Tasks.Add(task.name);
            }
            m_TasksContainer.Clear();

            base.OnDisable();
        }

        public void DeleteTask(KeyDownEvent e, VisualElement task)
        {
            if (e.keyCode == KeyCode.Delete)
            {
                if (task != null)
                {
                    task.parent.Remove(task);
                }
            }
        }

        public VisualElement CreateTask(string name)
        {
            var task = new VisualElement();
            task.focusable = true;
            task.tabIndex = 0;
            task.name = name;
            task.AddToClassList("task");

            task.RegisterCallback<KeyDownEvent, VisualElement>(DeleteTask, task);

            var taskName = new Toggle() { text = name, name = "checkbox" };
            task.Add(taskName);

            var taskDelete = new Button(() => task.parent.Remove(task)) { name = "delete", text = "Delete" };
            task.Add(taskDelete);

            return task;
        }

        void AddTask()
        {
            if (!string.IsNullOrEmpty(m_TextField.text))
            {
                m_TasksContainer.contentContainer.Add(CreateTask(m_TextField.text));
                m_TextField.value = "";

                // Give focus back to text field.
                m_TextField.Focus();
            }
            else
            {
                m_TasksContainer.contentContainer.Add(CreateTask("No text"));
                // Give focus back to text field.
                m_TextField.Focus();
            }
        }
    }
}
