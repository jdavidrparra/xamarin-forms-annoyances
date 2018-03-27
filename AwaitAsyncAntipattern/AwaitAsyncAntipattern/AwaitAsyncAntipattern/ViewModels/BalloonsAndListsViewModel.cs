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
   using System.Threading.Tasks;
   using PropertyChanged;
   using Views.Pages;
   using Xamarin.Forms;

   #endregion

   public interface IBalloonsAndListsViewModel
   {
      Command StartCommand { get; set; }

      IBalloonListCollectionViewModel AwaitAsyncViewModel { get; set; }

      IBalloonListCollectionViewModel TaskRunViewModel { get; set; }
   }

   /// <summary>
   ///    The view model for the main <see cref="BalloonsAndListsPage" />.
   /// </summary>
   [AddINotifyPropertyChangedInterface]
   public class BalloonsAndListsViewModel : ViewModelBase
   {
      private bool _foregroundAwaitHasRun;
      private bool _taskRunHasRun;

      private static readonly TimeSpan TIME_BETWEEN_STARTS = TimeSpan.FromMilliseconds(2500);

      public BalloonsAndListsViewModel(IBalloonListCollectionViewModel foregroundAwaitViewModel,
         IBalloonListCollectionViewModel taskRunViewModel)
      {
         ForegroundAwaitViewModel = foregroundAwaitViewModel;
         TaskRunViewModel = taskRunViewModel;

         StartCommand = new Command(Start, CanStart);

         // Listen for changes in IsRunning; set the command enabled/disabled
         ForegroundAwaitViewModel.IsAnimatingChanged += async (foregroundAwaitIsAnimating) =>
         {
            await HandleForegroundAwaitAnimatingChanged(foregroundAwaitIsAnimating);
         };
         TaskRunViewModel.IsAnimatingChanged += TaskRunViewModelOnIsAnimatingChanged;

         // We presume that no comparison is running initially.
      }

      private void TaskRunViewModelOnIsAnimatingChanged(bool val)
      {
         ForceStartCommandRefresh();
      }

      private async Task HandleForegroundAwaitAnimatingChanged(bool foregroundAwaitIsAnimating)
      {
         ForceStartCommandRefresh();

         // Fire the second list load
         if (_foregroundAwaitHasRun && !foregroundAwaitIsAnimating)
         {
            await Task.Delay(TIME_BETWEEN_STARTS);
            TaskRunViewModel.IsAnimating = true;
            _taskRunHasRun = true;
         }
      }

      private void ForceStartCommandRefresh()
      {
         StartCommand.ChangeCanExecute();
      }

      public Command StartCommand { get; set; }

      public IBalloonListCollectionViewModel ForegroundAwaitViewModel { get; set; }

      public IBalloonListCollectionViewModel TaskRunViewModel { get; set; }

      private bool CanStart()
      {
         return (!_foregroundAwaitHasRun && !_taskRunHasRun) || _foregroundAwaitHasRun && !ForegroundAwaitViewModel.IsAnimating && _taskRunHasRun && !TaskRunViewModel.IsAnimating;
      }

      private void Start()
      {
         // Run first
         ForegroundAwaitViewModel.IsAnimating = true;
         _foregroundAwaitHasRun = true;
         StartCommand.ChangeCanExecute();
      }
   }
}
