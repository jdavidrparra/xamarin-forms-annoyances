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
   using System;
   using System.Diagnostics;
   using System.Threading.Tasks;
   using Autofac;
   using Common.Notifications;
   using Common.Utils;
   using SharedGlobals.Container;
   using Xamarin.Forms;

   /// <remarks>
   /// WARNING: NO I CONTENT PAGE, so cannot reference the view from this interface without a hard cast!
   /// </remarks>
   public interface ITypeSafePageBase
   {
   }

   /// <summary>
   /// A base class for content pages that protects the type safety of the binding context.
   /// </summary>
   /// <typeparam name="InterfaceT">The required interface for this view.</typeparam>
   public abstract class TypeSafePageBase<InterfaceT> : ContentPage, ITypeSafePageBase
      where InterfaceT : class
   {
      protected static readonly IFormsMessenger Messenger;

      //private static readonly double TOP_MARGIN = 50.0;
      //private static readonly double STACK_MARGIN = 25.0;
      //private static readonly double INSET = 50.0;
      //private static readonly double PAGE_CONTENT_SPACING = 12.0;

      private readonly RelativeLayout _contentRelativeLayout = FormsUtils.GetExpandingRelativeLayout();

      static TypeSafePageBase()
      {
         using (var scope = AppContainer.GlobalVariableContainer.BeginLifetimeScope())
         {
            Messenger = scope.Resolve<IFormsMessenger>();
         }
      }

      protected TypeSafePageBase()
      {
         NavigationPage.SetHasNavigationBar(this, false);
         BackgroundColor = Color.Transparent;
      }

      /// <summary>
      /// T is normally an interface -- not a class -- but there is no such constraint available.
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

            /*
            // Use a stack layout to keep the message indicator apart
            var titleLayout = FormsUtils.GetExpandingStackLayout();
            titleLayout.Margin = STACK_MARGIN;
            titleLayout.Spacing = PAGE_CONTENT_SPACING;
            titleLayout.VerticalOptions = LayoutOptions.StartAndExpand;

            // Need a little expander at the top
            var expander = FormsUtils.GetSpacer(TOP_MARGIN);
            titleLayout.Children.Add(expander);

            // Add the message, if any
            var titleLabel =
               FormsUtils.GetSimpleLabel(fontNamedSize: NamedSize.Large, fontAttributes: FontAttributes.Bold);
            titleLabel.BindingContext = BindingContext;
            titleLayout.Children.Add(titleLabel);

            _contentRelativeLayout.Children.Add(
               titleLayout,
               Constraint.RelativeToParent(parent => parent.X + INSET),
               Constraint.RelativeToParent(parent => parent.Y + INSET),
               Constraint.RelativeToParent(parent => parent.Width - 2 * INSET),
               Constraint.RelativeToParent(parent => parent.Height));
            */

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
