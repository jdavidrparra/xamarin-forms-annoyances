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

namespace SharedForms.Common.Utils
{
   #region Imports

   using System;
   using ViewModels;
   using Xamarin.Forms;

   #endregion

   public interface IMessage
   {
   }

   /// <summary>
   /// A global static utilty library to assist with Xamarin.Forms.MessagingCenter calls.
   /// </summary>
   public static class FormsMessengerUtils
   {
      public static void Send<TMessage>(TMessage message, object sender = null) where TMessage : IMessage
      {
         if (sender == null)
         {
            sender = new object();
         }

         MessagingCenter.Send(sender, typeof(TMessage).FullName, message);
      }

      public static void Subscribe<TMessage>(object subscriber, Action<object, TMessage> callback) where TMessage : IMessage
      {
         MessagingCenter.Subscribe(subscriber, typeof(TMessage).FullName, callback);
      }

      public static void Unsubscribe<TMessage>(object subscriber) where TMessage : IMessage
      {
         MessagingCenter.Unsubscribe<object, TMessage>(subscriber, typeof(TMessage).FullName);
      }
   }

   public class NoPayloadMessage : IMessage
   {
   }

   public abstract class GenericMessageWithPayload<T> : IMessage
   {
      public T Payload { get; set; }
   }

   public class MainPageChangeRequestMessage : GenericMessageWithPayload<Page>
   {
      public bool PreventNavStackPush { get; set; }
   }

   public class BindingContextChangeRequestMessage : GenericMessageWithPayload<IViewModelBase>
   {
      public bool PreventNavStackPush { get; set; }
   }

   public class NavBarMenuTappedMessage : NoPayloadMessage
   {
   }

   public class MenuLoadedMessage : NoPayloadMessage
   {
   }

   public class AppStateChangedMessage : GenericMessageWithPayload<AppStateChanges>
   {
      public AppStateChangedMessage(string oldAppState, bool preventNavStackPush)
      {
         Payload = new AppStateChanges(oldAppState, preventNavStackPush);
      }
   }

   public class AppStateChanges
   {
      public AppStateChanges(string oldAppState, bool preventNavStackPush)
      {
         OldAppState = oldAppState;
         PreventNavStackPush = preventNavStackPush;
      }

      public string OldAppState { get; set; }

      public bool PreventNavStackPush { get; set; }
   }
}
