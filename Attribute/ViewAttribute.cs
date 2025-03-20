using System;

namespace Bucephalus.Attribute
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ViewAttribute : System.Attribute
    {
        public Type ControllerType { get; }

        public ViewAttribute(Type controllerType)
        {
            ControllerType = controllerType;
        }
    }
}