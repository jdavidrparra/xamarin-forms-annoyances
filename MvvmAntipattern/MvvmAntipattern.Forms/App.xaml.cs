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

namespace MvvmAntipattern.Forms
{
   #region Imports

   using System;
   using Autofac;
   using Common.Container;
   using SharedForms.Common.Generators;
   using SharedForms.Common.Interfaces;
   using SharedForms.Common.Navigation;
   using SharedForms.Common.Notifications;
   using SharedForms.Common.Utils;
   using SharedForms.Views.Pages;
   using SharedForms.Views.SubViews;
   using SharedGlobals.Container;
   using Xamarin.Forms;

   #endregion

   public partial class App
   {
      private readonly IFormsMessenger _formsMessenger;
      private readonly IStateMachineBase _stateMachine;
      private Page _lastMainPage;

      public App()
      {
         var appSetup = new FormsContainerSetup();
         AppContainer.GlobalVariableContainer = appSetup.CreateContainer();

         using (var scope = AppContainer.GlobalVariableContainer.BeginLifetimeScope())
         {
            _formsMessenger = scope.Resolve<IFormsMessenger>();
            _stateMachine = scope.Resolve<IStateMachineBase>();
         }

         InitializeComponent();

         // MainPage = new MvvmAntipattern.MainPage();
         // MainPage = new TiredAndTrueMainPage { BindingContext = new TiredAndTrueMainViewModel() };

         // If the MainPage gets set too late, serious problems occur -- so this is just a precaution
         MainPage = new ContentPage();
      }

      protected override void OnStart()
      {
         // Handle when your app starts
         RestartSettings();
      }

      protected override void OnSleep()
      {
         // Handle when your app sleeps
         UsubscribeFromPageChangedMessages();
      }

      //------------------------------------------------------------------------------------------
      /// <summary>
      ///    Never occurs
      /// </summary>
      protected override void OnResume()
      {
         // Handle when your app resumes
         RestartSettings();
      }

      private void SetUpMenu()
      {
         // Ask for the menu to get generated in the background
         _formsMessenger.Send(new MenuReflectionRequestMessage());
      }

      private void RestartSettings()
      {
         SubscribeToPageChangedMessages();

         SetUpMenu();

         _stateMachine?.GoToStartUpState();
      }

      private void SubscribeToPageChangedMessages()
      {
         _formsMessenger?.Subscribe<MainPageChangeRequestMessage>(this, MainPageChanged);
         _formsMessenger?.Subscribe<BindingContextChangeRequestMessage>(this, BindingContextPageChanged);
      }

      private void UsubscribeFromPageChangedMessages()
      {
         _formsMessenger?.Unsubscribe<MainPageChangeRequestMessage>(this);
         _formsMessenger?.Unsubscribe<BindingContextChangeRequestMessage>(this);
      }

      private void MainPageChanged(object sender, MainPageChangeRequestMessage messageArgs)
      {
         // Try to avoid changing the page is possible
         if (
               messageArgs?.Payload == null
               ||
               MainPage == null
               ||
               (
                  _lastMainPage != null
                  &&
                  _lastMainPage.GetType() == messageArgs.Payload.GetType()
               )
            )
         {
            return;
         }

         // Notify the nav bar directly before the change so it can preserve the existing main page binding context app state
         NavAndMenuBar.OnAppStateChanged(MainPage, messageArgs.PreventNavStackPush);

         MainPage = messageArgs.Payload;

         // IMPORTANT -- The assignment above often fails to cause Page.Disappearing for some reason
         if (_lastMainPage != null)
         {
            if (_lastMainPage is IDisposable lastMainPageAsDisposablew)
            {
               lastMainPageAsDisposablew.Dispose();
            }

            _lastMainPage = null;
         }

         _lastMainPage = MainPage;
      }

      private void BindingContextPageChanged(object sender, BindingContextChangeRequestMessage messageArgs)
      {
         if (MainPage != null)
         {
            // Same as with the page; the app state is about to change
            NavAndMenuBar.OnAppStateChanged(MainPage, messageArgs.PreventNavStackPush);

            MainPage.BindingContext = messageArgs.Payload;
         }
      }
   }
}