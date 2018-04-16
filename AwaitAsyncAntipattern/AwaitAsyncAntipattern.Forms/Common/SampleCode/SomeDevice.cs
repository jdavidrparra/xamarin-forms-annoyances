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
   using SharedForms.Common.Utils;

   #endregion

   /// <remarks>
   ///    Instantiated on the foreground thread.
   /// </remarks>
   public class SomeDevice : BaseDevice
   {
      private bool _isFeatureCreated;

      public DeviceFeature Feature { get; set; }

      // Occurs automatically as a part of the device start-up; cannot be controlled; occurs on the foreground thread
      protected override async void RequestFeatureCreation()
      {
         // This while loop runs *forever*, since the device has unanticipated problems starting up.  
         // Yet it is awaited on the foreground thread. So it stops the app entirely.  
         // The user, meanwhile, can tap the keyboard at will, since the app is not technically “blocked”.
         while (!_isFeatureCreated)
         {
            _isFeatureCreated = await CreateFeature().WithoutChangingContext();
         }
      }

      private async Task<bool> CreateFeature()
      {
         await Task.Delay(1000);

         // For demonstration purposes, this function always returns false.  
         // This aggravates our example and causes errors.
         return false;
      }
   }
}