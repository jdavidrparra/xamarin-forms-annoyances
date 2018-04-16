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

namespace SharedForms.Views.Pages
{
   #region Imports

   using System;
   using System.Diagnostics;
   using Common.Utils;
   using Xamarin.Forms;

   #endregion

   /// <remarks>
   ///    WARNING: NO ICONTENTPAGE, so cannot reference the view from this interface without a hard cast!
   /// </remarks>
   public interface ITypeSafePageBase
   {
   }

   /// <summary>
   ///    A base class for content pages that protects the type safety of the binding context.
   /// </summary>
   /// <typeparam name="InterfaceT">The required interface for this view.</typeparam>
   public abstract class TypeSafePageBase<InterfaceT> : ContentPage, ITypeSafePageBase
      where InterfaceT : class
   {
      private readonly RelativeLayout _contentRelativeLayout = FormsUtils.GetExpandingRelativeLayout();

      protected TypeSafePageBase()
      {
         NavigationPage.SetHasNavigationBar(this, false);
         BackgroundColor = Color.Transparent;
      }

      /// <summary>
      ///    T is normally an interface -- not a class -- but there is no such constraint available.
      /// </summary>
      public new InterfaceT BindingContext
      {
         get => base.BindingContext as InterfaceT;
         set => base.BindingContext = value;
      }

      /// <summary>
      ///    Requests that the deriver create the physical view.
      /// </summary>
      /// <returns></returns>
      protected abstract View ConstructPageView();

      protected virtual void AfterContentSet(RelativeLayout layout)
      {
      }

      /// <summary>
      ///    We create an "is busy" view by default so it is always available.
      ///    We insert the deriver's content below this is busy view.
      /// </summary>
      private void ConstructTypeSafePageView()
      {
         try
         {
            var derivedView = ConstructPageView();

            _contentRelativeLayout.CreateRelativeOverlay(derivedView);

            Content = _contentRelativeLayout;

            // Notify of this final step
            AfterContentSet(_contentRelativeLayout);
         }
         catch (Exception ex)
         {
            Debug.WriteLine("TYPE SAFE PAGE BASE: ConstructTypeSafePageView: ERROR ->" + ex.Message + "<-");
         }
      }

      /// <summary>
      ///    Called when the page appears.
      /// </summary>
      protected override void OnAppearing()
      {
         base.OnAppearing();

         ConstructTypeSafePageView();
      }
   }
}