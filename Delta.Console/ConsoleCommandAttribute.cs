using System;

namespace Delta.Console
{
    /// <summary>
    /// Must be used when a method should be visible and callable from the Console
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
    public class ConsoleCommandAttribute : Attribute
    {
        public string Name { get; protected set; }
        /// <param name="Name">The Name that should be used to display and call this method from the console</param>
        public ConsoleCommandAttribute(string Name)
        {
            this.Name = Name;
        }
    }
}
