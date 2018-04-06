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

namespace SharedForms.Views.SubViews
{
   #region Imports

   using System;
   using System.Linq;
   using Autofac;
   using Common.Interfaces;
   using Common.Navigation;
   using Common.Notifications;
   using Common.Utils;
   using Controls;
   using Pages;
   using PropertyChanged;
   using SharedGlobals.Container;
   using ViewModels;
   using Xamarin.Forms;

   #endregion

   public interface INavAndMenuBar : IDisposable
   {
      Page HostingPage { get; set; }
   }

   [AddINotifyPropertyChangedInterface]
   public class NavAndMenuBar : ContentView, INavAndMenuBar
   {
      public static readonly double OVERALL_HEIGHT = 45.0;

      private const string HAMBURGER_IMAGE = "hamburger_with_shadow_512.png";
      private const string BACK_IMAGE = "left_arrow_with_shadow_512.png";
      private static readonly double BUTTON_HEIGHT = 30.0;
      private static readonly FlexibleStack<string> _appStateBackButtonStack = new FlexibleStack<string>();
      private static readonly Thickness IOS_MARGIN = new Thickness(0, 20, 0, 0);

      public static readonly BindableProperty HostingPageProperty =
         BindableProperty.Create(nameof(HostingPage), typeof(Page), typeof(NavAndMenuBar), default(Page),
            propertyChanged: OnHostingPageChanged);

      private Image _backButton;
      private Page _hostingPage;
      private Image _menuButton;
      private bool _menuButtonEntered;
      private static readonly IStateMachineBase _stateMachine;
      private static readonly IFormsMessenger _formsMessenger;
      private Label _titleLabel;
      private bool IsNavigationAvailable => _appStateBackButtonStack.IsNotEmpty();

      static NavAndMenuBar()
      {
         using (var scope = AppContainer.GlobalVariableContainer.BeginLifetimeScope())
         {
            _formsMessenger = scope.Resolve<IFormsMessenger>();
            _stateMachine = scope.Resolve<IStateMachineBase>();
         }
      }

      public NavAndMenuBar()
      {
         BackgroundColor = Colors.HEADER_AND_TOOLBAR_COLOR_DEEP;

         if (Device.RuntimePlatform.IsSameAs(Device.iOS))
         {
            Margin = IOS_MARGIN;
         }

         // Listen for the static page change
         AskToSetBackButtonVisiblity += SetBackButtonVisiblity;

         _formsMessenger.Subscribe<MenuLoadedMessage>(this, OnMenuLoaded);
      }

      private bool IsMenuLoaded { get; set; }

      private void OnMenuLoaded(object sender, MenuLoadedMessage args)
      {
         IsMenuLoaded = true;
      }

      public Page HostingPage
      {
         get => _hostingPage;
         set
         {
            RemoveHostingPageBindingContextChangedHandler();

            _hostingPage = value;

            _hostingPage.BindingContextChanged += OnHostingPageBindingContextChanged;

            // Add the left-side back and right-side hamburger
            var grid = FormsUtils.GetExpandingGrid();
            grid.HeightRequest = OVERALL_HEIGHT;
            grid.VerticalOptions = LayoutOptions.CenterAndExpand;
            grid.AddStarRow();
            grid.AddFixedColumn(15);
            grid.AddFixedColumn(OVERALL_HEIGHT);
            grid.AddStarColumn();
            grid.AddFixedColumn(OVERALL_HEIGHT);
            grid.AddFixedColumn(15);

            // Wire into navigation, if available
            if (_hostingPage is IMenuNavPageBase)
            {
               _backButton = CreateNavBarButton(BACK_IMAGE, BackButtonTapped);
               _backButton.BindingContext = this;
               _backButton.HorizontalOptions = LayoutOptions.Start;
               grid.Children.Add(_backButton, 1, 0);

               // Initial setting
               SetBackButtonVisiblity();

               // Add the menu if that is allowed.
               _menuButton = CreateNavBarButton(HAMBURGER_IMAGE, MenuButtonTapped);
                _menuButton.HorizontalOptions = LayoutOptions.End;
               _menuButton.SetUpBinding(IsVisibleProperty, nameof(IsMenuLoaded));
               grid.Children.Add(_menuButton, 3, 0);
            }

            // Bind the title, center with margins and overlay
            // NOTE: At this point, the title is always empty.
            _titleLabel =
               FormsUtils.GetSimpleLabel(textColor: Color.White, fontNamedSize: NamedSize.Large,
                  fontAttributes: FontAttributes.Bold, textAlignment: TextAlignment.Start);
            _titleLabel.BackgroundColor = Color.Transparent;
            _titleLabel.HorizontalOptions = LayoutOptions.Center;
            _titleLabel.LineBreakMode = LineBreakMode.WordWrap;
            _titleLabel.SetUpBinding(Label.TextProperty, nameof(IPageViewModelBase.ViewTitle));

            grid.Children.Add(_titleLabel, 2, 0);

            Content = grid;

            // In case the binding context already exists
            SetUpHostingBindingContexts();
         }
      }

      public void Dispose()
      {
         ReleaseUnmanagedResources();
         GC.SuppressFinalize(this);
      }

      private void SetUpHostingBindingContexts()
      {
         BindingContext = _hostingPage.BindingContext;
         _menuButton.BindingContext = this;
         _titleLabel.BindingContext = _hostingPage.BindingContext;
      }

      private void OnHostingPageBindingContextChanged(object sender, EventArgs e)
      {
         SetUpHostingBindingContexts();
      }

      private void RemoveHostingPageBindingContextChangedHandler()
      {
         if (_hostingPage != null)
         {
            _hostingPage.BindingContextChanged -= OnHostingPageBindingContextChanged;
         }
      }

      private Image CreateNavBarButton(string imagePath, EventHandler menuButtonTapped)
      {
         var retImage = FormsUtils.GetImage(imagePath, height: BUTTON_HEIGHT);

         var imageTap = new TapGestureRecognizer();
         imageTap.Tapped += menuButtonTapped;
         retImage.GestureRecognizers.Add(imageTap);
         retImage.VerticalOptions = LayoutOptions.Center;

         return retImage;
      }

      private static void OnHostingPageChanged(BindableObject bindable, object oldvalue, object newvalue)
      {
         if (bindable is NavAndMenuBar bindableAsNavAndMenuBar)
         {
            bindableAsNavAndMenuBar.HostingPage = newvalue as Page;
         }
      }

      public static void OnAppStateChanged(Page oldPage, bool preventNavStackPush)
      {
         // If the old page as a non-navigation page, it cannot go onto the back stack.
         if (!(oldPage is IMenuNavPageBase) || !(oldPage.BindingContext is IPageViewModelBase))
         {
            // Wipe out the stack and restart
            _appStateBackButtonStack.Clear();
            return;
         }

         // 1. Remove the old page state from the stack -- get rid of previous old pages
         //    Note: Safe type-cast
         var bindingContextAsPageViewModelBase = (IPageViewModelBase) oldPage.BindingContext;

         _appStateBackButtonStack.RemoveIfPresent
         (
            bindingContextAsPageViewModelBase.AppState,
            appState => appState.IsSameAs(bindingContextAsPageViewModelBase.AppState)
         );

         // 2. Push the new app state
         if (!preventNavStackPush)
         {
            _appStateBackButtonStack.Push(bindingContextAsPageViewModelBase.AppState);
         }

         AskToSetBackButtonVisiblity?.Invoke();
      }

      public static event EventUtils.NoParamsDelegate AskToSetBackButtonVisiblity;


      private void SetBackButtonVisiblity()
      {
         if (_backButton == null)
         {
            return;
         }

         _backButton.IsVisible = IsNavigationAvailable;
      }

      private void MenuButtonTapped(object sender, EventArgs e)
      {
         if (_menuButtonEntered)
         {
            return;
         }

         _menuButtonEntered = true;

         // Notify the host page so it can close the menu.
         // Ask to close the menu as if the user tapped the hamburger icon.
         _formsMessenger.Send(new NavBarMenuTappedMessage());

         _menuButtonEntered = false;
      }

      private void BackButtonTapped(object sender, EventArgs eventArgs)
      {
         // Navigate back if possible -- should not be tappable if we cannot go back
         if (IsNavigationAvailable)
         {
            // Remove the top app state the stack
            var nextAppState = _appStateBackButtonStack.Pop();

            // Get the app state;
            // Do not add to the back stack, since we are going backwards
            _stateMachine.GoToAppState<NoPayload>(nextAppState, null, true);
         }

         SetBackButtonVisiblity();
      }

      private void ReleaseUnmanagedResources()
      {
         AskToSetBackButtonVisiblity -= SetBackButtonVisiblity;

         RemoveButtonTappedListeners(_backButton, BackButtonTapped);

         RemoveButtonTappedListeners(_menuButton, MenuButtonTapped);
      }

      private void RemoveButtonTappedListeners(Image imageButton, EventHandler buttonTapped)
      {
         if (imageButton.GestureRecognizers.IsNotEmpty())
         {
            var tappableGesture = imageButton.GestureRecognizers.OfType<TapGestureRecognizer>().FirstOrDefault();
            if (tappableGesture != null)
            {
               tappableGesture.Tapped -= buttonTapped;
            }
         }
      }

      ~NavAndMenuBar()
      {
         ReleaseUnmanagedResources();
      }
   }
}