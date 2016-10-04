using NUnit.Framework;

namespace IoC.Exercise
{
  [TestFixture]
  public class InstanceFactoryTests
  {
    class A {}

    class B
    {
      public A Dependency { get; }

      public B(A dependency)
      {
        Dependency = dependency;
      }
    }

    [Test]
    public void HasBothClassesInRecipes_CanConstructDependant()
    {
      var module = new Module();
      module.RegisterScoped<A>(resolver => new A());
      module.RegisterScoped<B>(resolver => new B(resolver.Get<A>()));

      var factory = new InstanceFactory(module);
      var a = factory.Get<A>();
      var b = factory.Get<B>();

      Assert.NotNull(a);
      Assert.AreSame(a, b.Dependency);
    }

    [Test]
    public void HasExternalDependency_CanConstructDependentObject()
    {
      var module = new Module();
      module.RegisterScoped<B>(resolver => new B(resolver.Get<A>()));

      var a = new A();
      var factory = new InstanceFactory(module);
      factory.RegisterScopedInstance(a);
      var b = factory.Get<B>();

      Assert.AreSame(a, b.Dependency);
    }

    [Test]
    public void HasSingleton_AllFactoriesReturnIt()
    {
      var singleton = new A();
      var module = new Module();
      module.RegisterSingletonInstance(singleton);

      var parentFactory = new InstanceFactory(module);
      var childFactory = parentFactory.CreateChildFactory();
      
      Assert.AreSame(singleton, parentFactory.Get<A>());
      Assert.AreSame(singleton, childFactory.Get<A>());
    }

    [Test]
    public void HasSingleton_CanConstructDependentObject()
    {
      var singleton = new A();

      var module = new Module();
      module.RegisterSingletonInstance(singleton);
      module.RegisterScoped<B>(resolver => new B(resolver.Get<A>()));

      var factory = new InstanceFactory(module);
      var b = factory.Get<B>();

      Assert.AreSame(singleton, b.Dependency);
    }

    [Test]
    public void HasScopedRecipe_ChildFactoriesReturnDifferentObjects()
    {
      var module = new Module();
      module.RegisterScoped<A>(resolver => new A());

      var parentFactory = new InstanceFactory(module);

      var childFactory1 = parentFactory.CreateChildFactory();
      var childFactory2 = parentFactory.CreateChildFactory();

      var parentObject = parentFactory.Get<A>();
      var childObject1 = childFactory1.Get<A>();
      var childObject2 = childFactory2.Get<A>();

      Assert.AreNotSame(parentObject, childObject1);
      Assert.AreNotSame(parentObject, childObject2);
      Assert.AreNotSame(childObject1, childObject2);
    }

    [Test]
    public void HasScopedRecipe_RecipeIsUsedJustOncPerFactory()
    {
      int recipeCallCount = 0;
      var module = new Module();
      module.RegisterScoped<A>(resolver =>
      {
        recipeCallCount++;
        return new A();
      });

      var parentFactory = new InstanceFactory(module);

      var childFactory = parentFactory.CreateChildFactory();

      parentFactory.Get<A>();
      Assert.AreEqual(1, recipeCallCount);

      parentFactory.Get<A>();
      Assert.AreEqual(1, recipeCallCount); //still the same

      childFactory.Get<A>();
      Assert.AreEqual(2, recipeCallCount); // called for child

      childFactory.Get<A>();
      Assert.AreEqual(2, recipeCallCount); // returned from cache
    }
  }
}
