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

namespace SharedForms.ViewModels
{
   #region Imports

   using Autofac;
   using Common.Interfaces;
   using Common.Navigation;
   using PropertyChanged;
   using SharedGlobals.Container;

   #endregion

   [AddINotifyPropertyChangedInterface]
   [DoNotNotify]
   public abstract class PageViewModelBase : IPageViewModelBase
   {
      protected readonly IStateMachineBase Machine;

      protected PageViewModelBase(IStateMachineBase stateMachine)
      {
         // Request the global interface type so the code is more share-able.
         Machine = stateMachine;
      }

      /// <summary>
      /// Copied from the menu item to this page (at least for now)
      /// </summary>
      public string PageTitle { get; set; }
   }
}
