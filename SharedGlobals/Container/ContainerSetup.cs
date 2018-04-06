namespace SharedGlobals.Container
{
   #region Imports

   using Autofac;

   #endregion

   public class ContainerSetup
   {
      //------------------------------------------------------------------------------------------
      public IContainer CreateContainer()
      {
         var containerBuilder = new ContainerBuilder();
         RegisterDependencies(containerBuilder);

         return containerBuilder.Build();
      }

      //------------------------------------------------------------------------------------------
      protected virtual void RegisterDependencies(ContainerBuilder containerBuilder)
      {
      }
   }
}
