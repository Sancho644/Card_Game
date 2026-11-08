namespace Game.Services.Collection
{
    public interface ICollectionStore
    {
        string Read(string key);
        void Write(string key, string value);
        void Save();
    }
}
