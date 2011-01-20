namespace Framework
{
    public interface IFluentBuilder<out T>
    {
        T Build();
    }
}
