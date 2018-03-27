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

   using System.Diagnostics;
   using System.Threading.Tasks;
   using SharedCrossApp.Utils;

   #endregion

   public class MyFirstClass
   {
      private bool _localDataExistsBool;

      public MyFirstClass()
      {
         /*
         // WARNING issued by Resharper and compiler: we are failing to await!
         VerifyDataExists();
         */

         try
         {
            Task.Run(async () =>
            {
               await VerifyDataExists().AwaitWithoutChangingContext();

               // Occurs in the proper order, after VerifyDataExists has completed
               ConstructClass();
            });

            // Occurs immediately; VerifyDataExists has not completed yet!
            ConstructClass();
         }
         catch
         {
            Debug.WriteLine("Error in Task.Run at constructor");
         }
      }

      public async Task<bool> DataExists()
      {
         var retStream = await ReadStreamAsync().AwaitWithoutChangingContext();
         return retStream.Length > 0;
      }

      public async Task VerifyDataExists()
      {
         _localDataExistsBool = await DataExists().AwaitWithoutChangingContext();
      }

      private string ReadStream()
      {
         return string.Empty;
      }

      private Task<string> ReadStreamAsync()
      {
         return Task.FromResult(string.Empty);
      }

      private void ConstructClass()
      {
         // Do some complex construction that relies on _localDataExistsBool
      }
   }
}