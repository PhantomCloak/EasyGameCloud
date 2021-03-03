namespace GlobalOptions
{
    public interface IGenericPool<T>
    {
        T Get();
        bool Pool(T objToPool);
    }
}