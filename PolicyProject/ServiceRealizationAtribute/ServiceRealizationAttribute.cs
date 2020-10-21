using System;

namespace ServiceRealizationAtribute
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceRealizationAttribute : Attribute
    {
        public ServiceRealizationAttribute()
        {
        }

        public ServiceRealizationAttribute(string interfaceName, string name, int orderToLoad)
        {
            InterfaceName = interfaceName;
            Name = name;
            OrderToLoad = orderToLoad;
        }

        public string InterfaceName { get; set; }

        public string Name { get; set; }

        public int OrderToLoad { get; set; }
    }
}