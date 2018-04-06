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

namespace AwaitAsyncAntipattern.Forms.ViewModels
{
   #region Imports

   using System;
   using System.Collections.ObjectModel;
   using System.Diagnostics;
   using System.Threading.Tasks;
   using Models.Services;
   using SharedForms.Common.Utils;
   using SharedForms.ViewModels;

   #endregion

   public interface IBalloonListViewModel : IViewModelBase
   {
      ObservableCollection<string> SubList { get; set; }

      IBalloonListViewModel NextList { get; set; }

      Task HandleListLoadRequest();

      Task HandleNestedListLoadRequest();

      event EventUtils.NoParamsDelegate PopulationCompleted;
   }

   /// <summary>
   ///    A small view model nested inside the main one.
   /// </summary>
   public class BalloonListViewModel : ViewModelBase, IBalloonListViewModel
   {
      private readonly bool _isForegroundAwait;
      private readonly int _listNum;

      public BalloonListViewModel(bool isForegroundAwait, int listNum)
      {
         _listNum = listNum;
         _isForegroundAwait = isForegroundAwait;
         SubList = new ObservableCollection<string>();
      }

      public event EventUtils.NoParamsDelegate PopulationCompleted;

      public ObservableCollection<string> SubList { get; set; }

      public IBalloonListViewModel NextList { get; set; }

      public async Task HandleListLoadRequest()
      {
         if (_isForegroundAwait)
         {
            // Start of nested calls
            if (_listNum == 0 && NextList != null)
            {
               await HandleNestedListLoadRequest().AwaitWithoutChangingContext();
            }
         }
         else
         {
            await InitializeViewModel().AwaitWithoutChangingContext();
         }
      }

      public async Task HandleNestedListLoadRequest()
      {
         if (_isForegroundAwait && NextList != null)
         {
            await NextList.HandleNestedListLoadRequest().AwaitWithoutChangingContext();
         }

         await InitializeViewModel().AwaitWithoutChangingContext();
      }

      private async Task InitializeViewModel()
      {
         if (_isForegroundAwait)
         {
            await CallListLoad().AwaitWithoutChangingContext();
         }
         else
         {
            try
            {
#pragma warning disable 4014
               // NO AWAIT
               Task.Run(async () => { await CallListLoad().AwaitWithoutChangingContext(); });
#pragma warning restore 4014
            }
            catch (Exception ex)
            {
               Debug.WriteLine("Error at Task.Run at a view model constructor ->" + ex.Message + "<-");
            }
         }
      }

      private async Task CallListLoad()
      {
         SubList.Clear();

         // Android "Amimations can only be run on looper threads" when the _isForegroundAwait/notThreaded parameter is true
         // Or it just hangs
         // await FakeStringService.LoadOneList(SubList, _listNum, _isForegroundAwait, (returnList) =>
         await FakeStringService.LoadOneList(SubList, _listNum, false, (returnList) =>
         {
            SubList = returnList;
            PopulationCompleted?.Invoke();
         }).AwaitWithoutChangingContext();
      }
   }
}