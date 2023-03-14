namespace Dev.Codes.Lib.Database.Interfaces
{
    public interface IRepositoryItem
    {
        string Id {get; set;}
        string GetId();
        void CreateGuidId();
    }
}