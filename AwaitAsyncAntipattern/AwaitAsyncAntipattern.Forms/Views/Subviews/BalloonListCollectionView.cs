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

namespace AwaitAsyncAntipattern.Forms.Views.Subviews
{
   #region Imports

   using System;
   using AnimatedBalloonView;
   using Common.Converters;
   using Controls;
   using Models.Services;
   using SharedForms.Common.Utils;
   using ViewModels;
   using Xamarin.Forms;

   #endregion

   /// <summary>
   ///    Displays a series of lists that are loaded on command.
   /// </summary>
   public class BalloonListCollectionView : ContentView
   {
      private const double INSIDE_MARGIN = 10;

      public static readonly BindableProperty TitleTextProperty =
         CreateBalloonListCollectionViewBindableProperty
         (
            nameof(TitleText),
            default(string),
            BindingMode.OneWay,
            (balloonView, oldVal, newVal) => { balloonView.TitleText = newVal; }
         );

      public static readonly BindableProperty DescriptionTextProperty =
         CreateBalloonListCollectionViewBindableProperty
         (
            nameof(DescriptionText),
            default(string),
            BindingMode.OneWay,
            (balloonView, oldVal, newVal) => { balloonView.DescriptionText = newVal; }
         );

      public static readonly BindableProperty TitleColorProperty =
         CreateBalloonListCollectionViewBindableProperty
         (
            nameof(TitleColor),
            default(Color),
            BindingMode.OneWay,
            (balloonView, oldVal, newVal) => { balloonView.TitleColor = newVal; }
         );

      public BalloonListCollectionView()
      {
         CreateContent();
      }

      public string TitleText
      {
         get => (string) GetValue(TitleTextProperty);
         set => SetValue(TitleTextProperty, value);
      }

      public string DescriptionText
      {
         get => (string) GetValue(DescriptionTextProperty);
         set => SetValue(DescriptionTextProperty, value);
      }

      public Color TitleColor
      {
         get => (Color) GetValue(TitleColorProperty);
         set => SetValue(TitleColorProperty, value);
      }

      private void CreateContent()
      {
         Content = null;

         // Get the existing relative layout
         var baseLayout = FormsUtils.GetExpandingRelativeLayout();

         // Just need a grid for this view
         var mainGrid = FormsUtils.GetExpandingGrid();
         mainGrid.Padding = new Thickness(INSIDE_MARGIN);
         mainGrid.BackgroundColor = Color.Transparent;

         // The title is at top with "auto" height.  It is itself a grid with two labels
         var titleGrid = FormsUtils.GetExpandingGrid();
         titleGrid.Margin = new Thickness(0, 0, 0, INSIDE_MARGIN);
         titleGrid.BackgroundColor = Color.Transparent;

         titleGrid.AddAutoRow();
         var titleTextLabel = FormsUtils.GetSimpleLabel(TitleText, TitleColor, fontNamedSize: NamedSize.Large,
            fontAttributes: FontAttributes.Bold, labelBindingPropertyName: nameof(TitleText), labelBindingSource: this);
         titleTextLabel.SetUpBinding(Label.TextColorProperty, nameof(TitleColor), source: this);
         titleGrid.Children.Add(titleTextLabel, 0, 0);

         titleGrid.AddAutoRow();
         var descLabel = FormsUtils.GetSimpleLabel(DescriptionText, Color.Black, fontNamedSize: NamedSize.Micro,
            fontAttributes: FontAttributes.Bold, labelBindingPropertyName: nameof(DescriptionText),
            labelBindingSource: this);
         titleGrid.Children.Add(descLabel, 0, 1);

         mainGrid.AddAutoRow();
         mainGrid.Children.Add(titleGrid, 0, 0);

         var nextPhysicalRow = 1;

         for (var listRow = 0; listRow < FakeStringService.LIST_COUNT; listRow++)
         {
            mainGrid.AddStarRow();

            // Create a new list view for this row
            var rowListView = CreateListView(listRow);

            mainGrid.Children.Add(rowListView, 0, nextPhysicalRow++);
         }

         // Add the load timer label and value
         var loadTimerGrid = FormsUtils.GetExpandingGrid();
         loadTimerGrid.HorizontalOptions = LayoutOptions.Center;
         loadTimerGrid.AddAutoColumn();
         loadTimerGrid.AddAutoColumn();
         loadTimerGrid.AddAutoRow();

         var timerLabel = FormsUtils.GetSimpleLabel("Load Time", fontNamedSize: NamedSize.Small,
            textAlignment: TextAlignment.End);
         timerLabel.Margin = new Thickness(0, 0, 5, 0);
         loadTimerGrid.Children.Add(timerLabel, 0, 0);

         var loadTimeLabel =
            FormsUtils.GetSimpleLabel(fontNamedSize: NamedSize.Small, textAlignment: TextAlignment.Start);
         loadTimeLabel.Margin = new Thickness(5, 0, 0, 0);
         loadTimerGrid.Children.Add(loadTimeLabel, 1, 0);
         loadTimeLabel.SetUpBinding(Label.TextProperty, nameof(IBalloonListCollectionViewModel.TimeToLoad),
            stringFormat: "{0:mm\\:ss\\:ff}");

         mainGrid.AddAutoRow();
         mainGrid.Children.Add(loadTimerGrid, 0, nextPhysicalRow++);

         baseLayout.CreateRelativeOverlay(mainGrid);

         // Lay the balloons on top
         var balloons = new AnimatedBalloonView();
         balloons.SetUpBinding(AnimatedBalloonView.IsRunningProperty,
            nameof(IBalloonListCollectionViewModel.IsAnimating));

         baseLayout.CreateRelativeOverlay(balloons);

         Content = baseLayout;
      }

      /// <summary>
      ///    Intended to be over-ridden along with a base call to preserve the code below.
      ///    Derivers will set the list BindingContext.
      /// </summary>
      /// <param name="row"></param>
      /// <returns></returns>
      private ListView CreateListView(int row)
      {
         var retListView = new SimpleListView();

         if (row < FakeStringService.LIST_COUNT - 1)
         {
            retListView.Margin = new Thickness(0, 0, 0, INSIDE_MARGIN);
         }

         retListView.SetUpBinding(ListView.ItemsSourceProperty, nameof(IBalloonListCollectionViewModel.SubViewModels),
            converter: NumberedListSourceConverter.Instance, converterParameter: row);

         return retListView;
      }

      private static BindableProperty CreateBalloonListCollectionViewBindableProperty<PropertyTypeT>
      (
         string localPropName,
         PropertyTypeT defaultVal = default(PropertyTypeT),
         BindingMode bindingMode = BindingMode.OneWay,
         Action<BalloonListCollectionView, PropertyTypeT, PropertyTypeT> callbackAction = null
      )
      {
         return BindableUtils.CreateBindableProperty(localPropName, defaultVal, bindingMode, callbackAction);
      }
   }
}