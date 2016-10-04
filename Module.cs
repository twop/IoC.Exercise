using System;
using System.Collections.Generic;

namespace IoC.Exercise
{
  public class Module
  {
    private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();
    private readonly Dictionary<Type, Func<IResolver, object>> _scopedRecipes = new Dictionary<Type, Func<IResolver, object>>();

    internal Func<IResolver, object> TryFindScopedRecipe(Type type)
    {
      Func<IResolver, object> resolveFunc;
      _scopedRecipes.TryGetValue(type, out resolveFunc);
      return resolveFunc;
    }

    internal object TryGetSingleton(Type type)
    {
      object singleton;
      _singletons.TryGetValue(type, out singleton);
      return singleton;
    }

    public void RegisterScoped<T>(Func<IResolver, T> getFunc)
    {
      _scopedRecipes[typeof (T)] = (factory) => getFunc(factory);
    }

    public void RegisterSingletonInstance<T>(T obj)
    {
      _singletons[typeof (T)] = obj;
    }
  }
}