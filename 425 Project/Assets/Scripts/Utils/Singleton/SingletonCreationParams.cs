namespace Utils.Singleton
{
    public struct SingletonCreationParams
    {
        public readonly SingletonCreationMode CreationMode;
        public bool DontDestroyOnLoad;

        public SingletonCreationParams(SingletonCreationMode creationMode, bool dontDestroyOnLoad)
        {
            CreationMode = creationMode;
            DontDestroyOnLoad = dontDestroyOnLoad;
        }
    }
}