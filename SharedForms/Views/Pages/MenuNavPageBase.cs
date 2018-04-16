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

   using System.Threading.Tasks;
   using Autofac;
   using Common.Interfaces;
   using Common.Utils;
   using PropertyChanged;
   using SharedGlobals.Container;
   using SubViews;
   using Xamarin.Forms;

   #endregion

   public interface IMenuNavPageBase : ITypeSafePageBase
   {
   }

   /// <summary>
   ///    A page with a navigation header.
   /// </summary>
   [AddINotifyPropertyChangedInterface]
   public abstract class MenuNavPageBase<InterfaceT> : TypeSafePageBase<InterfaceT>, IMenuNavPageBase
      where InterfaceT : class, IPageViewModelBase
   {
      private const string NAV_BAR_CONTROL_TEMPLATE = "NavBarControlTemplate";

      private const int MENU_ANIMATE_MILLISECONDS = 400;
      private const int MENU_FADE_MILLISECONDS = 200;

      private volatile bool _isPageMenuShowing;
      private readonly AbsoluteLayout _canvas = FormsUtils.GetExpandingAbsoluteLayout();

      protected MenuNavPageBase()
      {
         // NOTE: This scope doesn't work acording to internal testing, but it appears harmless otherwise.
         //       The PageMenu must be yanked out of the container each time to manage its state.
         //       Declaring the PageMenu as global static created instability, hence this hack.
         using (var scope = AppContainer.GlobalVariableContainer.BeginLifetimeScope())
         {
            PageMenu = scope.Resolve<IMainMenu>();
         }

         FormsMessengerUtils.Subscribe<NavBarMenuTappedMessage>(this, OnMainMenuItemSelected);

         BackgroundColor = Color.Transparent;

         PageMenuView.Opacity = 0;

         var controlTemplateNotSet = true;

         BindingContextChanged +=
            (sender, args) =>
            {
               if (controlTemplateNotSet)
               {
                  ControlTemplate = Application.Current.Resources[NAV_BAR_CONTROL_TEMPLATE] as ControlTemplate;
                  controlTemplateNotSet = false;
               }
            };

         _canvas.InputTransparent = true;
      }

      protected bool IsPageMenuShowing
      {
         get => _isPageMenuShowing;
         set
         {
            Device.BeginInvokeOnMainThread
            (
               async () =>
               {
                  _isPageMenuShowing = value;

                  await AnimatePageMenu().WithoutChangingContext();

                  // HACK to fix dead UI in the main menu, which sits on top of this canvas
                  _canvas.InputTransparent = !_isPageMenuShowing;
               }
            );
         }
      }

      protected IMainMenu PageMenu { get; set; }

      protected View PageMenuView => PageMenu as View;

      /// <summary>
      ///    Animates the panel in our out depending on the state
      /// </summary>
      private async Task AnimatePageMenu()
      {
         if (IsPageMenuShowing)
         {
            // Slide the menu up from the bottom
            var rect = 
               new Rectangle
                  (
                     Width - MainMenu.MENU_GROSS_WIDTH, 
                     0, 
                     MainMenu.MENU_GROSS_WIDTH, 
                     PageMenu.MenuHeight
                  );

            await Task.WhenAll(new [] { PageMenuView.LayoutTo(rect, MENU_ANIMATE_MILLISECONDS, Easing.CubicIn), PageMenuView.FadeTo(1.0, MENU_FADE_MILLISECONDS)}).WithoutChangingContext();
         }
         else
         {
            // Retract the menu
            var rect = CreateOfflineRectangle();

            await Task.WhenAll(new[] { PageMenuView.LayoutTo(rect, MENU_ANIMATE_MILLISECONDS, Easing.CubicOut), PageMenuView.FadeTo(0.0, MENU_FADE_MILLISECONDS) }).WithoutChangingContext();
         }
      }

      private Rectangle CreateOfflineRectangle()
      {
         return new Rectangle(Width, 0, 0, PageMenu.MenuHeight);
      }

      private void RemoveMenuFromLayout()
      {
         if (_canvas != null && _canvas.Children.Contains(PageMenuView))
         {
            _canvas.Children.Remove(PageMenuView);
         }
      }

      protected override void AfterContentSet(RelativeLayout layout)
      {
         // No need to add it twice
         if (_canvas != null && _canvas.Children.Contains(PageMenuView))
         {
            return;
         }

         PageMenuView.Opacity = 0;

         var targetRect = CreateOfflineRectangle();

         layout.CreateRelativeOverlay(_canvas);

         // A slight cheat; using protected property
         _canvas.Children.Add(PageMenuView, targetRect);
      }

      protected override void OnDisappearing()
      {
         base.OnDisappearing();

         FormsMessengerUtils.Unsubscribe<NavBarMenuTappedMessage>(this);

         RemoveMenuFromLayout();
      }

      private void OnMainMenuItemSelected(object sender, NavBarMenuTappedMessage args)
      {
         // Close the menu upon selection
         IsPageMenuShowing = !IsPageMenuShowing;
      }
   }
}
