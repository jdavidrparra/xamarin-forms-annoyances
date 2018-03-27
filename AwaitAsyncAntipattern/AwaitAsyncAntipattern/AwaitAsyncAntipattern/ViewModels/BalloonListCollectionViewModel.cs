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

namespace AwaitAsyncAntipattern.ViewModels
{
   #region Imports

   using System;
   using System.Collections.Generic;
   using System.ComponentModel;
   using System.Diagnostics;
   using System.Linq;
   using System.Runtime.CompilerServices;
   using System.Threading.Tasks;
   using Models.Services;
   using PropertyChanged;
   using Shared.Utils;
   using Views.Controls;
   using Views.Subviews;
   using Xamarin.Forms;

   #endregion
   
   public interface IBalloonListCollectionViewModel : INotifyPropertyChanged
   {
      bool IsAnimating { get; set; }

      IList<IBalloonListViewModel> SubViewModels { get; set; }

      event EventUtils.GenericDelegate<bool> IsAnimatingChanged;

      event EventUtils.NoParamsDelegate RequestDataLoad;

      TimeSpan TimeToLoad { get; set; }
   }

   /// <summary>
   ///    Provides data for a <see cref="BalloonListCollectionView" />
   /// </summary>
   [AddINotifyPropertyChangedInterface]
   public class BalloonListCollectionViewModel : IBalloonListCollectionViewModel
   {
      private bool _isAnimating;
      private readonly Stopwatch _stopwatch = new Stopwatch();
      // private volatile int _isPopulatedSuccessCount;
      private readonly IThreadSafeAccessor _threadSafePopulatedSuccessCount = new ThreadSafeAccessor(0);

      public BalloonListCollectionViewModel(bool isForeground)
      {
         SubViewModels = new List<IBalloonListViewModel>();

         for (var row = 0; row < FakeStringService.LIST_COUNT; row++)
         {
            var newList = new BalloonListViewModel(isForeground, row);
            newList.PopulationCompleted += HandleAnyListPopulationCompleted;
            RequestDataLoad += async () =>
            {
               await newList.HandleListLoadRequest();
            };

            SubViewModels.Add(newList);
         }

         // Create artificial nesting for the foreground case
         for (var idx = FakeStringService.LIST_COUNT - 2; idx >= 0; idx--)
         {
            SubViewModels[idx].NextList = SubViewModels[idx + 1];
         }
      }

      public event EventUtils.GenericDelegate<bool> IsAnimatingChanged;

      public event PropertyChangedEventHandler PropertyChanged;

      public event EventUtils.NoParamsDelegate RequestDataLoad;

      public bool IsAnimating
      {
         get => _isAnimating;
         set
         {
            if (_isAnimating == value)
            {
               return;
            }

            _isAnimating = value;

            IsAnimatingChanged?.Invoke(_isAnimating);

            if (_isAnimating)
            {
               _threadSafePopulatedSuccessCount.WriteStoredValue(0);
               TimeToLoad = default(TimeSpan);
               _stopwatch.Reset();
               _stopwatch.Start();

               // Raise the event to ask for data loads
               RequestDataLoad?.Invoke();
            }
         }
      }

      public IList<IBalloonListViewModel> SubViewModels { get; set; }

      public TimeSpan TimeToLoad { get; set; }

      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      /// <summary>
      /// Run each time a list is populated (completion case only).
      /// If all lists are populated, set IsAnimating to FALSE.
      /// </summary>
      /// <remarks>THIS ORIGINATES IN VARIOUS THREADS!!!</remarks>
      private void HandleAnyListPopulationCompleted()
      {
         _threadSafePopulatedSuccessCount.WriteStoredValue(((int)_threadSafePopulatedSuccessCount.ReadStoredValue()) + 1);
         var _isPopulatedSuccessCount = (int) _threadSafePopulatedSuccessCount.ReadStoredValue();

         // Can only succeed once when it reaches this value
         if (_isPopulatedSuccessCount == FakeStringService.LIST_COUNT)
         {
            IsAnimating = false;
            _stopwatch.Stop();
            TimeToLoad = _stopwatch.Elapsed;
         }

         OnPropertyChanged(nameof(SubViewModels));
      }
   }
}
