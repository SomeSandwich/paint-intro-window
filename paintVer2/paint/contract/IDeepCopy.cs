namespace Contract;

public interface IDeepCopy<out T>
{
    T DeepClone();
}