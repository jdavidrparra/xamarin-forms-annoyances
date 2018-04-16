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

namespace AwaitAsyncAntipattern.Forms.Models.Services
{
   #region Imports

   using System;
   using System.Collections.Generic;
   using System.Collections.ObjectModel;
   using System.Linq;
   using System.Threading.Tasks;
   using Bogus;
   using SharedForms.Common.Utils;
   using Xamarin.Forms;

   #endregion

   public static class FakeStringService
   {
      public const int LIST_COUNT = 5;
      public const int LIST_ITEM_COUNT = 100;
      public const int DEFAULT_DELAY_MILLISECONDS = 2500;

      private static readonly IList<IList<string>> _rawStringLists = new List<IList<string>>();

      /// <summary>
      ///    Create all of the lists right away
      /// </summary>
      static FakeStringService()
      {
         for (var listIdx = 0; listIdx < LIST_COUNT; listIdx++)
         {
            var newList = new List<string>();

            for (var itemIdx = 0; itemIdx < LIST_ITEM_COUNT; itemIdx++)
            {
               var faker = new Faker("en");

               newList.Add(faker.Person.FullName);
            }

            newList = newList.OrderBy(i => i).ToList();

            _rawStringLists.Add(newList);
         }
      }

      /// <summary>
      ///    Ensures that the static constructor has been run
      /// </summary>
      public static void Init()
      {
      }

      /// <summary>
      ///    Typical task-based async-able service call
      /// </summary>
      /// <param name="listNum"></param>
      /// <param name="delay"></param>
      /// <returns></returns>
      public static Task<IList<string>> GetListAsync(int listNum, int delay = DEFAULT_DELAY_MILLISECONDS)
      {
         return Task.Run(async () =>
         {
            await Task.Delay(delay);
            return _rawStringLists[listNum];
         });
      }

      public static async Task LoadOneList(ObservableCollection<string> list, int listNum, bool notThreaded, Action<ObservableCollection<string>> callbackAction)
      {
         var rawList = await GetListAsync(listNum).WithoutChangingContext();

         // Foreground await case
         if (notThreaded)
         {
            list = new ObservableCollection<string>(rawList);
            callbackAction?.Invoke(list);
         }
         else
         {
            // Task run case
            Device.BeginInvokeOnMainThread
            (
               () =>
               {
                  list = new ObservableCollection<string>(rawList);
                  callbackAction?.Invoke(list);
               }
            );
         }
      }
   }
}