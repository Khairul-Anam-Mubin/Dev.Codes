using Dev.Codes.Lib.Database.Interfaces;

namespace Dev.Codes.Lib.Database.Models
{
    public abstract class ARepositoryItem : IRepositoryItem
    {
        public string Id { get; set; }
        
        public virtual void CreateGuidId()
        {
            Id = Guid.NewGuid().ToString();
        }
        
        public string GetId() 
        {
            return Id;
        }
    }
}