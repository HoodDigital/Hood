namespace Hood.Entities
{
    public interface IBaseEntity<T>
    {
        T Id { get; set; }

        bool Equals(BaseEntity<T> other);
        bool Equals(object obj);
        int GetHashCode();
    }
}