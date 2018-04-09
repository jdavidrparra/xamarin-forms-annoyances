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

namespace IOCAntipattern.Forms
{
   using System;
   using System.Diagnostics;
   using Xamarin.Forms;

   public interface ISecondViewModel
   {
   }

   public class SecondViewModel : ISecondViewModel
   {
      private bool _isAlive;

      public SecondViewModel()
      {
         _isAlive = true;

         Device.StartTimer(TimeSpan.FromSeconds(1), () =>
         {
            if (_isAlive)
            {
               Debug.WriteLine("Second View Model is still alive");
            }

            return _isAlive;
         });
      }

      // The only way to determine if this object is being garbage-collected
      ~SecondViewModel()
      {
         _isAlive = false;
         Debug.WriteLine("Second View Model is being garbage collected");
      }
   }
}