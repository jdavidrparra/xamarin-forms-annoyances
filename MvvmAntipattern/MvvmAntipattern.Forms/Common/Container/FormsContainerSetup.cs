namespace MvvmAntipattern.Forms.Common.Container
{
   #region Imports

   using Autofac;
   using Navigation;
   using SharedForms.Common.Generators;
   using SharedForms.Common.Navigation;
   using SharedForms.Common.Notifications;
   using SharedForms.Views.SubViews;
   using SharedGlobals.Container;

   #endregion

   public class FormsContainerSetup : ContainerSetup
   {
      //------------------------------------------------------------------------------------------
      protected override void RegisterDependencies(ContainerBuilder containerBuilder)
      {
         base.RegisterDependencies(containerBuilder);

         containerBuilder.RegisterType<FormsMessenger>().As<IFormsMessenger>().SingleInstance();
         containerBuilder.RegisterType<FormsStateMachine>().As<IStateMachineBase>().SingleInstance();
         containerBuilder.RegisterType<MainMenu>().As<IMainMenu>().SingleInstance();
      }
   }
}
