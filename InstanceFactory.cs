using System;
using System.Collections.Generic;

namespace IoC.Exercise
{
  public interface IResolver
  {
    T Get<T>() where T:class;
  }

  public class InstanceFactory : IResolver
  {
    private readonly Dictionary<Type, object> _scopedObjects = new Dictionary<Type, object>();
    private readonly List<Module> _modules;

    public InstanceFactory(params Module[] modules)
    {
      _modules = new List<Module>(modules);
    }

    private InstanceFactory(List<Module> parentModules)
    { 
      _modules = parentModules;
    }

    public InstanceFactory CreateChildFactory()
    {
      return new InstanceFactory(_modules);
    }

    public void RegisterScopedInstance<T>(T obj)
    {
      _scopedObjects[typeof (T)] = obj;
    }

    public T Get<T>() where T:class
    {
      Type type = typeof (T);
      object obj;

      if (_scopedObjects.TryGetValue(type, out obj))
        return (T)obj;

      foreach (var module in _modules)
      {
        Func<IResolver, object> recipe = module.TryFindScopedRecipe(type);
        if (recipe != null)
        {
          obj = recipe(this);
          _scopedObjects[type] = obj;
          return (T) obj;
        }
      }

      foreach (var module in _modules)
      {
        object singleton = module.TryGetSingleton(type);
        if (singleton != null)
          return (T) singleton;
      }

      throw new Exception("Cannot resolve "+ type.Name);
    }
  }
}
