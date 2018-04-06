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

namespace MvvmAntipattern.Forms.ViewModels
{
   using Common.Interfaces;
   using Common.Navigation;
   using Models;
   using PropertyChanged;
   using Xamarin.Forms;

   [AddINotifyPropertyChangedInterface]
   [DoNotNotify]
   public class BirdViewModel : AnimalViewModelBase, IBird
   {
      public BirdViewModel(IAnimalDataBase animalData) : base(animalData)
      {
      }

      /// <summary>
      /// Necessary for menu reflection. Otherwise harmless :)
      /// </summary>
      public BirdViewModel() : this(null)
      {
      }

      public override string WhatAmI => "A Bird";

      public override string LikeToEat => IAmBig ? "Paragliders" : "Seeds";

      // Not implemented
      public override Command MakeNoiseCommand => new Command(() =>{});

      // Not implemented
      public override Command MoveCommand => new Command(() => { });

      public override string AppState => FormsStateMachine.BIRD_ANIMAL_APP_STATE;

      public override int MenuOrder => FormsStateMachine.GetMenuOrderFromAppState(AppState);

      public override string MenuTitle => "Bird";
   }
}