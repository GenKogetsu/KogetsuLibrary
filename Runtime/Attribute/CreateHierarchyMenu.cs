using System;

namespace Genoverrei.Library.Attribute
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CreateHierarchyMenu : System.Attribute
    {
        public string MenuPath { get; }
        public CreateHierarchyMenu(string menuPath) => MenuPath = menuPath;
    }
}