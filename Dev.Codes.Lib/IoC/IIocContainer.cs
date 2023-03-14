using System.Reflection;

namespace Dev.Codes.Lib.Ioc
{
    public interface IIocContainer
    {
        void AddAssembly<T>();
        void AddAssembly(Assembly assembly);
        void AddAllAssemblies();
        T GetService<T>();
        T Resolve<T>();
        T Resolve<T>(string name);
        object Resolve(Type type);
        IEnumerable<T> ResolveMany<T>();
        IEnumerable<object> ResolveMany(string name);
        void Register<T>();
        void Register<TI, T>();
        void RegisterSingleton<T>();
        void RegisterSingleton<TI, T>();
    }
}