using System;

namespace Utils.Singleton
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SingletonAttribute : Attribute
    {
        public SingletonAttribute(SingletonCreationMode creationMode = SingletonCreationMode.Auto, bool dontDestroyOnLoad = true)
        {
            CreationParams = new SingletonCreationParams(creationMode, dontDestroyOnLoad);
        }

        public SingletonCreationParams CreationParams { get; }
    }
}