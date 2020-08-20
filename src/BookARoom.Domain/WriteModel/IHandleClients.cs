namespace BookARoom.Domain.WriteModel
{
    public interface IHandleClients
    {
        bool IsClientAlready(string clientIdentifier);
        void CreateClient(string clientIdentifier);
    }
}