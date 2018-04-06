#region License
// MIT License
// 
// Copyright (c) 2018 
// Marcus Technical Services, Inc.
// http://www.marcusts.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion

namespace SharedForms.Common.Generators
{
   using System;
   using System.Collections.Generic;
   using System.Diagnostics;
   using System.Linq;
   using System.Reflection;
   using System.Threading.Tasks;
   using Autofac;
   using Interfaces;
   using Microsoft.Extensions.DependencyModel;
   using Notifications;
   using SharedGlobals.Container;
   using Xamarin.Forms;

   public interface IMenuGenerator : IDisposable
   {
      
   }

   public class MenuGenerator : IMenuGenerator
   {
      public IDictionary<Type, MenuData> ReflectedMenuData = new Dictionary<Type, MenuData>();
      private readonly IFormsMessenger _formsMessenger;

      public MenuGenerator()
      {
         using (var scope = AppContainer.GlobalVariableContainer.BeginLifetimeScope())
         {
            _formsMessenger = scope.Resolve<IFormsMessenger>();

            _formsMessenger.Subscribe<MenuReflectionRequestMessage>(this, OnMenuReflectionRequest);
         }
      }

      private void OnMenuReflectionRequest(object o, MenuReflectionRequestMessage menuReflectionRequestMessage)
      {
         GetReflectedMenuData();
      }

      public void GetReflectedMenuData()
      {
         try
         {
            Task.Run
               (
                  () =>
                  {
                     ReflectedMenuData.Clear();

                     var menuDataToPublish = CreateMenuData();

                     Device.BeginInvokeOnMainThread(() => { _formsMessenger.Send(new MenuReflectionResultMessage { Payload = menuDataToPublish }); });
                  }
               );
         }
         catch (Exception ex)
         {
            Debug.WriteLine("GetReflectedMenuData: Error at Task.Run ->" + ex.Message + "<-");
         }
      }

      private static IEnumerable<Type> GetAllTypesOf<T>()
      {
         // var platform = Environment.OSVersion.Platform.ToString();
         // var runtimeAssemblyNames = DependencyContext.Default.GetRuntimeAssemblyNames(platform);
         var runtimeAssemblyNames = AppDomain.CurrentDomain.GetAssemblies().SelectMany(ass => ass.GetReferencedAssemblies()).Distinct();

         return runtimeAssemblyNames
            .Select(Assembly.Load)
            .SelectMany(a => a.ExportedTypes)
            .Where(x => typeof(T).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).Distinct();
      }

      private static IMenuData[] CreateMenuData()
      {
         var retMenuData = new List<IMenuData>();
         var pageViewModelTypes = GetAllTypesOf<IPageViewModelBase>();

         foreach (var classType in pageViewModelTypes)
         {
            var ctor = classType.GetConstructor(Type.EmptyTypes);
            if (ctor != null && ctor.IsPublic)
            {
               var classInstance = Activator.CreateInstance(classType);

               if (classInstance is IPageViewModelBase viewModelClass)
               {
                  retMenuData.Add( new MenuData( classType, viewModelClass.MenuOrder, viewModelClass.MenuTitle, viewModelClass.ViewTitle, viewModelClass.AppState ) );
               }

               if (classInstance is IDisposable disposable)
               {
                  disposable.Dispose();
               }
            }
         }

         return retMenuData.OrderBy(md => md.MenuOrder).ToArray();
      }

      private void ReleaseUnmanagedResources()
      {
         _formsMessenger.Unsubscribe<MenuReflectionRequestMessage>(this);
      }

      public void Dispose()
      {
         ReleaseUnmanagedResources();
         GC.SuppressFinalize(this);
      }

      ~MenuGenerator()
      {
         ReleaseUnmanagedResources();
      }
   }
}
