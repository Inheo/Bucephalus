using System;

namespace Bucephalus.Attribute
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ControllerAttribute : System.Attribute
    {
        public Type ModelType { get; }
        public Type ServiceMediatorType { get; }

        public ControllerAttribute(Type modelType, Type serviceMediatorType)
        {
            ModelType = modelType;
            ServiceMediatorType = serviceMediatorType;
        }
    }
}