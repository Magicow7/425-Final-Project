namespace Utils.Singleton
{
    public struct SingletonCreationParams
    {
        public readonly SingletonCreationMode creationMode;
        public bool dontDestroyOnLoad;

        public SingletonCreationParams(SingletonCreationMode creationMode, bool dontDestroyOnLoad)
        {
            this.creationMode = creationMode;
            this.dontDestroyOnLoad = dontDestroyOnLoad;
        }
    }
}