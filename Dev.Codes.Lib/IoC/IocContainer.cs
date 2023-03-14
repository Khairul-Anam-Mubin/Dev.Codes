using System.Composition.Convention;
using System.Composition.Hosting;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Dev.Codes.Lib.Ioc
{
    public sealed class IocContainer : IIocContainer
    {
        private static IocContainer _instance = null;
        private static readonly object _lockObject = new();
        private CompositionHost _container;
        private ContainerConfiguration _containerConfiguration;
        private List<string> _assemblyLists;
        private IServiceProvider ServiceProvider { get; set; }
        private IConfiguration Configuration { get; set; }

        private IocContainer()
        {
            Console.WriteLine("IocContainer Constructor");
            _containerConfiguration = new ContainerConfiguration();
            _assemblyLists = new List<string>();
            AddAllAssemblies();
            CreateContainer();
        }

        void CreateContainer()
        {
            if (_container == null)
            {
                lock(_lockObject)
                {
                    if (_container == null)
                    {
                        _container = _containerConfiguration.CreateContainer();
                    }
                }
                 Console.WriteLine("Container Created");
            }
        }

        public static IocContainer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new IocContainer();
                        }
                    }
                }
                return _instance;
            }
        }

        public void AddAssembly<T>()
        {
            AddAssembly(typeof(T).Assembly);
        }

        public void AddAssembly(Assembly assembly)
        {
            if (_assemblyLists.Contains(assembly.FullName))
            {
                return;
            }
            _containerConfiguration.WithAssembly(assembly);
            _assemblyLists.Add(assembly.FullName);
            Console.WriteLine($"Adding Assembly {assembly.FullName}");
        }

        public T GetService<T>()
        {
            if (ServiceProvider != null)
            {
                Console.WriteLine($"Get Service {typeof(T).Name}");
                var service = ServiceProvider.GetService(typeof(T));
                return (T)service;
            }

            lock (_lockObject)
            {
                try
                {
                    var value = _container.GetExport<T>();
                    return value;
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to resolve type {0}", typeof(T).Name);
                    throw;
                }
            }
        }

        public T Resolve<T>()
        {
            lock (_lockObject)
            {
                try
                {
                    var value = _container.GetExport<T>();
                    Console.WriteLine($"Resolve {typeof(T).Name}");
                    return value;
                }
                catch (Exception)
                {
                    try
                    {
                        return GetService<T>();
                    }
                    catch (System.Exception)
                    {
                        Console.WriteLine($"Failed to resolve type {typeof(T).Name}");
                        throw;
                    }
                }
            }
        }

        public object Resolve(Type type)
        {
            lock (_lockObject)
            {
                try
                {
                    var methodInfo = _container.GetType().GetMethods().First(d => d.Name == "GetExportedValue" && d.GetParameters().Length == 0);
                    Type[] genericTypeArray = { type };
                    methodInfo = methodInfo.MakeGenericMethod(genericTypeArray);
                    return methodInfo.Invoke(_container, null);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Failed to resolve type {type.Name}");
                    return null;
                }
            }
        }

        public T Resolve<T>(string name)
        {
            lock (_lockObject)
            {
                try
                {
                    Console.WriteLine($"Resolve {name}");
                    return _container.GetExport<T>(name);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Failed to resolve type {typeof(T).Name} and key {name}");
                    throw;
                }
            }
        }

        public IEnumerable<object> ResolveMany(string name)
        {
            lock (_lockObject)
            {
                try
                {
                    return _container.GetExports<object>(name);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Failed to resolve with key {name}");
                    throw;
                }
            }
        }

        public IEnumerable<T> ResolveMany<T>()
        {
            lock (_lockObject)
            {
                try
                {
                    return _container.GetExports<T>();
                }
                catch (Exception)
                {
                    Console.WriteLine($"Failed to resolve type {typeof(T).Name}");
                    throw;
                }
            }
        }

        public void AddAllAssemblies()
        {
            AddAllAssemblies(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location));
            AddAllAssemblies(Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location));
        }

        private void AddAllAssemblies(string location, string prefix = "Dev")
        {
            if (string.IsNullOrEmpty(location) == false)
            {
                var files = Directory.GetFiles(location);
                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.Name.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase) &&
                            (fileInfo.Extension == ".dll" || fileInfo.Extension == ".exe"))
                        {
                            var assemblyName = Path.GetFileNameWithoutExtension(file);
                            if (string.IsNullOrEmpty(assemblyName) == false)
                            {
                                var assembly = Assembly.Load(assemblyName);
                                if (assembly != null)
                                {
                                    AddAssembly(assembly);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        public void Register<T>()
        {
            var conventions = new ConventionBuilder();
            conventions.ForType<T>().Export();
            _containerConfiguration.WithPart<T>(conventions);
        }

        public void Register<TI, T>()
        {
            var conventions = new ConventionBuilder();
            conventions.ForType<T>().Export<TI>();
            _containerConfiguration.WithPart(typeof(T), conventions);
        }

        public void RegisterSingleton<T>()
        {
            var conventions = new ConventionBuilder();
            conventions.ForType<T>().Export().Shared();
            _containerConfiguration.WithPart<T>(conventions);
        }

        public void RegisterSingleton<TI, T>()
        {
            var conventions = new ConventionBuilder();
            conventions.ForType<T>().Export<TI>().Shared();
            _containerConfiguration.WithPart(typeof(T), conventions);
        }

        public IConfiguration GetConfiguration()
        {
            return Configuration;
        }

        public void SetConfiguration(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
    }
}