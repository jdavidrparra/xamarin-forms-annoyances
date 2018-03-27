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

namespace AwaitAsyncAntipattern.Shared.Utils
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Threading.Tasks;
   using Xamarin.Forms;

   internal static class SharedUtils
   {
      /// <summary>
      /// Convenience method to make view / view model property bindings easier to declare.
      /// </summary>
      internal static void SetUpBinding
      (
         this View view,
         BindableProperty bindableProperty,
         string viewModelPropertyName,
         BindingMode bindingMode = BindingMode.OneWay,
         IValueConverter converter = null,
         object converterParameter = null,
         string stringFormat = null,
         object source = null
      )
      {
         view.SetBinding(bindableProperty, new Binding(viewModelPropertyName, bindingMode, converter, converterParameter, stringFormat, source));
      }

      internal static void AddStarRow(this Grid grid, double factor = 1)
      {
         grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(factor, GridUnitType.Star) });
      }

      internal static void AddAutoRow(this Grid grid)
      {
         grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
      }

      internal static void AddFixedRow(this Grid grid, double height)
      {
         grid.RowDefinitions.Add(new RowDefinition { Height = height });
      }

      internal static void AddAutoColumn(this Grid grid)
      {
         grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
      }

      internal static Label GetSimpleLabel
      (
         string labelText = default(string),
         Color textColor = default(Color),
         TextAlignment textAlignment = TextAlignment.Center,
         NamedSize fontNamedSize = NamedSize.Medium,
         double fontSize = 0.0,
         FontAttributes fontAttributes = FontAttributes.None,
         double width = 0,
         double height = 0,
         string labelBindingPropertyName = default(string),
         object labelBindingSource = null,
         LineBreakMode breakMode = LineBreakMode.WordWrap
      )
      {
         if (textColor.IsAnEqualObjectTo(default(Color)))
         {
            textColor = Color.Black;
         }

         var retLabel =
            new Label
            {
               Text = labelText,
               TextColor = textColor,
               HorizontalTextAlignment = textAlignment,
               VerticalTextAlignment = TextAlignment.Center,
               HorizontalOptions = HorizontalOptionsFromTextAlignment(textAlignment),
               VerticalOptions = LayoutOptions.CenterAndExpand,
               BackgroundColor = Color.Transparent,
               InputTransparent = true,
               FontAttributes = fontAttributes,
               FontSize = fontSize.IsNotEmpty() ? fontSize : Device.GetNamedSize(fontNamedSize, typeof(Label)),
               LineBreakMode = breakMode,
            };

         // Set up the label text binding (if provided)
         if (labelBindingPropertyName.IsNotEmpty())
         {
            if (labelBindingSource != null)
            {
               retLabel.SetUpBinding(Label.TextProperty, labelBindingPropertyName, source: labelBindingSource);
            }
            else
            {
               retLabel.SetUpBinding(Label.TextProperty, labelBindingPropertyName);
            }
         }

         if (width.IsNotEmpty())
         {
            retLabel.WidthRequest = width;
         }

         if (height.IsNotEmpty())
         {
            retLabel.HeightRequest = height;
         }

         return retLabel;
      }

      internal static bool IsAnEqualReferenceTo<T>(this T mainObj, T compareObj)
         where T : class
      {
         return
            (mainObj == null && compareObj == null)
            ||
            (
               ((mainObj == null) == (compareObj == null))
               &&
               (ReferenceEquals(compareObj, mainObj))
            );
      }

      internal static bool IsNotAnEqualReferenceTo<T>(this object mainObj, object compareObj)
         where T : class
      {
         return !mainObj.IsAnEqualReferenceTo(compareObj);
      }

      internal static bool IsAnEqualObjectTo(this object mainObj, object compareObj)
      {
         return
            mainObj == null && compareObj == null
            ||
            mainObj != null && mainObj.Equals(compareObj)
            ||
            compareObj != null && compareObj.Equals(mainObj);
      }

      internal static bool IsNotAnEqualObjectTo(this object mainObj, object compareObj)
      {
         return !mainObj.IsAnEqualObjectTo(compareObj);
      }

      private const double NUMERIC_ERROR = 0.001;

      public const string TRUE_STR = "true";

      public const string FALSE_STR = "false";

      internal static bool? EmptyNullableBool => new bool?();

      internal static bool IsSameAs(this double mainD, double otherD)
      {
         return Math.Abs(mainD - otherD) < NUMERIC_ERROR;
      }

      internal static bool IsDifferentThan(this double mainD, double otherD)
      {
         return !mainD.IsSameAs(otherD);
      }

      internal static bool IsEmpty(this double mainD)
      {
         return mainD.IsSameAs(0);
      }

      internal static bool IsNotEmpty(this double mainD)
      {
         return !mainD.IsEmpty();
      }

      internal static bool IsSameAs(this float mainF, float otherF)
      {
         return Math.Abs(mainF - otherF) < NUMERIC_ERROR;
      }

      internal static bool IsDifferentThan(this float mainF, float otherF)
      {
         return !mainF.IsSameAs(otherF);
      }

      internal static bool IsEmpty(this string str)
      {
         return String.IsNullOrWhiteSpace(str);
      }

      internal static bool IsNotEmpty(this string str)
      {
         return !str.IsEmpty();
      }

      internal static bool IsEmpty<T>(this IEnumerable<T> list)
      {
         return list == null || !list.Any();
      }

      internal static bool IsNotEmpty<T>(this IEnumerable<T> list)
      {
         return !list.IsEmpty();
      }

      internal static bool IsSameAs(this string mainStr, string otherStr)
      {
         return String.Compare(mainStr, otherStr, StringComparison.CurrentCultureIgnoreCase) == 0;
      }

      internal static bool IsDifferentThan(this string mainStr, string otherStr)
      {
         return !mainStr.IsSameAs(otherStr);
      }

      internal static int RoundToInt(this double floatVal)
      {
         return (int)Math.Round(floatVal, 0);
      }

      internal static bool IsLessThanOrEqualTo(this double thisD, double otherD)
      {
         return thisD.IsSameAs(otherD) || thisD < otherD;
      }

      internal static bool IsGreaterThanOrEqualTo(this double thisD, double otherD)
      {
         return thisD.IsSameAs(otherD) || thisD > otherD;
      }

      internal static bool IsTrue(this bool? b)
      {
         return b.HasValue && b.Value;
      }

      internal static bool IsNotTheSame(this bool? first, bool? second)
      {
         return first == null != (second == null)
                ||
                first.IsNotAnEqualObjectTo(second);
      }

      internal static LayoutOptions HorizontalOptionsFromTextAlignment(TextAlignment textAlignment)
      {
         switch (textAlignment)
         {
            case TextAlignment.Center:
               return LayoutOptions.Center;
            case TextAlignment.End:
               return LayoutOptions.End;

            // Covers Start and default
            default:
               return LayoutOptions.Start;
         }
      }

      internal static void CreateRelativeOverlay(this RelativeLayout layout, View viewToAdd)
      {
         layout.Children.Add(
            viewToAdd, Constraint.Constant(0), Constraint.Constant(0),
            Constraint.RelativeToParent(parent => parent.Width),
            Constraint.RelativeToParent(parent => parent.Height));
      }

      internal static RelativeLayout GetExpandingRelativeLayout()
      {
         return new RelativeLayout
         {
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.FillAndExpand,
            BackgroundColor = Color.Transparent
         };
      }

      internal static Grid GetExpandingGrid()
      {
         return new Grid
         {
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.FillAndExpand,
            BackgroundColor = Color.Transparent
         };
      }

      internal static async Task AwaitWithoutChangingContext(this Task task)
      {
         await task.ConfigureAwait(false);
      }

      internal static async Task<T> AwaitWithoutChangingContext<T>(this Task<T> task)
      {
         return await task.ConfigureAwait(false);
      }
   }
}