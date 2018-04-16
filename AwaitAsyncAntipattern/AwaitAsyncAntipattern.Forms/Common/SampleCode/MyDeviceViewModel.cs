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

namespace AwaitAsyncAntipattern.Forms.Common.SampleCode
{
   #region Imports

   using System.Threading.Tasks;
   using System.Windows.Input;
   using SharedForms.Common.Utils;
   using Xamarin.Forms;

   #endregion

   public class MyDeviceViewModel
   {
      private SomeDevice _device;

      public MyDeviceViewModel()
      {
         RetryStartDeviceCommand = new Command(async () => { await StartDevice().WithoutChangingContext(); });
      }

      // A clever programmer has decided to add a button to the UI to retry if the device appears to fail to start up. The button is tied to this command.
      public ICommand RetryStartDeviceCommand { get; }

      // Called by the PageView at its construction
      public async Task InitializeViewModel()
      {
         // Benignly ask to start the device – foreground await
         await StartDevice().WithoutChangingContext();

         // This call relies on the device Feature that fails to construct.
         await SomeVeryLongRunningTask(_device.Feature).WithoutChangingContext();
      }

      private Task<SomeDevice> StartDevice()
      {
         // A seemingly harmless request to set up a device goes wrong; the foreground thread is now hopelessly stopped, though "unblocked".  The app will never construct!
         _device = new SomeDevice();

         return Task.FromResult(_device);
      }

      private async Task SomeVeryLongRunningTask(DeviceFeature feature)
      {
         await Task.Delay(5000);
      }
   }
}