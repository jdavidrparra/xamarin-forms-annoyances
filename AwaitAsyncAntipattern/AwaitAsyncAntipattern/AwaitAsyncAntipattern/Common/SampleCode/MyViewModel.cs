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

namespace AwaitAsyncAntipattern.Common.SampleCode
{
   #region Imports

   using System.Collections.ObjectModel;
   using System.Diagnostics;
   using System.Threading.Tasks;
   using SharedCrossApp.Utils;
   using Xamarin.Forms;

   #endregion

   public class MyViewModel
   {
      public MyViewModel()
      {
         try
         {
            // Use the Task Parallel Library to run independent threads with no await – these do not hamper the foreground thread.
            var tasks = new[]
            {
               SomeTask(),
               SomeOtherTask(),
               YetAnotherTask()
            };

            Task.WaitAll(tasks);
         }
         catch
         {
            Debug.WriteLine("Error in Task.Run");
         }
      }

      public ObservableCollection<string> UIBoundList { get; private set; }

      // Never called on a foreground thread
      public async Task SomeTask()
      {
         var list = await SomeWebServiceOperation().AwaitWithoutChangingContext();

         // Use this trick to come back from a threaded call
         Device.BeginInvokeOnMainThread(() => { UIBoundList = new ObservableCollection<string>(list); });
      }

      public Task SomeOtherTask()
      {
         return Task.CompletedTask;
      }

      public Task YetAnotherTask()
      {
         return Task.CompletedTask;
      }

      private Task<string[]> SomeWebServiceOperation()
      {
         return Task.FromResult(new[] { "dog", "cat", "tree" });
      }

      public async Task InitializeViewModel()
      {
         await Task.Delay(1000);
      }
   }
}