namespace AwaitAsyncAntipattern.Shared.Utils
{
   using System;
   using Xamarin.Forms;

   public static class BindableUtils
   {
      public static BindableProperty CreateReadOnlyBindableProperty<T, U>
      (
         string localPropName,
         U defaultVal = default(U),
         BindingMode bindingMode = BindingMode.OneWay,
         Action<T, U, U> callbackAction = null
      )
         where T : class
      {
         return BindableProperty.CreateReadOnly
         (
            localPropName,
            typeof(U),
            typeof(T),
            defaultVal,
            bindingMode,
            propertyChanged: (bindable, oldVal, newVal) =>
            {
               if (callbackAction != null)
               {
                  var bindableAsOverlayButton = bindable as T;
                  if (bindableAsOverlayButton != null)
                  {
                     callbackAction(bindableAsOverlayButton, (U) oldVal, (U) newVal);
                  }
               }
            }).BindableProperty;
      }

      public static BindableProperty CreateBindableProperty<T, U>
      (
         string localPropName,
         U defaultVal = default(U),
         BindingMode bindingMode = BindingMode.OneWay,
         Action<T, U, U> callbackAction = null
      )
         where T : class
      {
         return BindableProperty.Create
         (
            localPropName,
            typeof(U),
            typeof(T),
            defaultVal,
            bindingMode,
            propertyChanged: (bindable, oldVal, newVal) =>
            {
               if (callbackAction != null)
               {
                  var bindableAsOverlayButton = bindable as T;
                  if (bindableAsOverlayButton != null)
                  {
                     callbackAction(bindableAsOverlayButton, (U)oldVal, (U)newVal);
                  }
               }
            });
      }
   }
}