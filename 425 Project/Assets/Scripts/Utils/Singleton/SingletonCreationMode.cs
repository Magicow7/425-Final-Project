namespace Utils.Singleton
{
    public enum SingletonCreationMode
    {
        /// <summary>
        /// Automatically create the singleton if it doesn't exist.
        /// </summary>
        Auto,
        /// <summary>
        ///  Throw an error if the singleton doesn't exist.
        /// </summary>
        Throw,
        /// <summary>
        ///  Wait for the singleton to be created: does not throw, but does return a null instance.
        /// </summary>
        Wait
    }
}