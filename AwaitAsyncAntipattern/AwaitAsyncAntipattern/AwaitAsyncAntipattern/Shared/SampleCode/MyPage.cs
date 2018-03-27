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

namespace AwaitAsyncAntipattern.Shared.SampleCode
{
   #region Imports

   using Utils;
   using Xamarin.Forms;

   #endregion

   public class MyPage : ContentPage
   {
      // private MyViewModel _viewModel;
      private MyDeviceViewModel _viewModel;

      public MyPage()
      {
         var button = new Button
         {
            Text = "Start Up Device",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
         };

         Content = button;

         // BindingContextChanged += (s, e) => { _viewModel = BindingContext as MyViewModel; };
         BindingContextChanged += (s, e) =>
         {
            _viewModel = BindingContext as MyDeviceViewModel;

            if (_viewModel != null)
            {
               button.Command = _viewModel.RetryStartDeviceCommand;
            }
         };
      }

      // Async signature is legal here
      protected override async void OnAppearing()
      {
         // This call then cascades into dozens of other awaits, all on the foreground thread.
         if (_viewModel != null)
         {
            await _viewModel.InitializeViewModel().AwaitWithoutChangingContext();
         }
      }
   }
}