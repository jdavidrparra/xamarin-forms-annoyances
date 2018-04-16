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

namespace SharedForms.Common.Navigation
{
   #region Imports

   using System;
   using Autofac;
   using Interfaces;
   using SharedGlobals.Container;
   using Utils;
   using ViewModels;
   using Views.SubViews;
   using Xamarin.Forms;

   #endregion

   public interface IStateMachineBase : IDisposable
   {
      // A way of knowing the current app state, though this should not be commonly referenced.
      // string CurrentAppState { get; }

      IMenuNavigationState[] MenuItems { get; }

      // The normal way of changing states
      void GoToAppState<T>(string newState, T payload = default(T), bool preventStackPush = false);

      // Sets the startup state for the app on initial start (or restart).
      void GoToStartUpState();

      // Goes to the default landing page; for convenience only
      void GoToLandingPage(bool preventStackPush = true);
   }

   /// <summary>
   ///    A controller to manage which views and view models are shown for a given state
   /// </summary>
   public abstract class StateMachineBase : IStateMachineBase
   {
      private Page _lastPage;
      private string _lastAppState;

      public abstract string AppStartUpState { get; }

      public void GoToAppState<T>(string newState, T payload = default(T), bool preventStackPush = false)
      {
         if (_lastAppState.IsSameAs(newState))
         {
            return;
         }

         // Raise an event to notify the nav bar that the back-stack requires modification.
         // Send in the last app state, *not* the new one.
         FormsMessengerUtils.Send(new AppStateChangedMessage(_lastAppState, preventStackPush));

         // CurrentAppState = newState;
         _lastAppState = newState;

         // Not awaiting here because we do not directly change the Application.Current.MainPage.  That is done through a message.
         RespondToAppStateChange(newState, payload, preventStackPush);
      }

      // public string CurrentAppState { get; private set; }

      public abstract IMenuNavigationState[] MenuItems { get; }

      // Sets the startup state for the app on initial start (or restart).
      public void GoToStartUpState()
      {
         FormsMessengerUtils.Send(new AppStartUpMessage());

         GoToAppState<NoPayload>(AppStartUpState, null, true);
      }

      public abstract void GoToLandingPage(bool preventStackPush = true);

      public void Dispose()
      {
         ReleaseUnmanagedResources();
         GC.SuppressFinalize(this);
      }

      protected abstract void RespondToAppStateChange<PayloadT>(string newState, PayloadT payload, bool preventStackPush);

      protected void CheckAgainstLastPage(Type pageType, Func<Page> pageCreator, Func<IViewModelBase> viewModelCreator,
         bool preventStackPush)
      {
         // If the same page, keep it
         if (_lastPage != null && _lastPage.GetType() == pageType)
         {
            FormsMessengerUtils.Send(new BindingContextChangeRequestMessage
            {
               Payload = viewModelCreator(),
               PreventNavStackPush = preventStackPush
            });
            return;
         }

         // ELSE create both the page and view model
         var page = pageCreator();
         page.BindingContext = viewModelCreator();

         FormsMessengerUtils.Send(new MainPageChangeRequestMessage
         {
            Payload = page,
            PreventNavStackPush = preventStackPush
         });

         _lastPage = page;
      }

      protected virtual void ReleaseUnmanagedResources()
      {
      }

      ~StateMachineBase()
      {
         ReleaseUnmanagedResources();
      }

      public class AppStartUpMessage : NoPayloadMessage
      {
      }
   }
}
