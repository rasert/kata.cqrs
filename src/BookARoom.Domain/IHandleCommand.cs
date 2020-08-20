namespace BookARoom.Domain
{
    public interface IHandleCommand<T>
    {
        void Handle(T command);
    }
}