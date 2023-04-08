/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model.Predicates;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Interface;
using System;
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Predicates
{
    public class P_CompositeVM : P_BaseVM<P_CompositeM, object>
    {
        public P_CompositeVM(P_CompositeM model, object parent) : base(model, parent)
        {
            IsFirstSelected = true;
        }

        Type subType;
        public Type SubType
        {
            get => subType;
            set
            {
                if (subType != value)
                {
                    subType = value;

                    if (value != null)
                    {
                        if (IsFirstSelected)
                        {
                            PredicateA = PredicatesHelper.CreatePredicateByType(subType, Parent);
                            Model.predicateA = PredicateA.GetModel<P_BaseM>();
                            Notify(nameof(PredicateA));
                        }
                        else
                        {
                            PredicateB = PredicatesHelper.CreatePredicateByType(subType, Parent);
                            Model.predicateB = PredicateB.GetModel<P_BaseM>();
                            Notify(nameof(PredicateB));
                        }
                    }
                }
            }
        }

        public bool IsFirstSelected { get; set; }

        public IPredicate PredicateA { get; set; }

        public IPredicate PredicateB { get; set; }

        public byte CompositionType
        {
            get => Model.compositionType;
            set
            {
                if (value != Model.compositionType)
                {
                    Model.compositionType = value;
                    Notify(nameof(CompositionType));
                }
            }
        }


        ICommand selectCommand;
        public ICommand SelectCommand => selectCommand ?? (selectCommand = new RelayCommand<bool>((isFirstSelected) =>
        {
            IsFirstSelected = isFirstSelected;
            Notify(nameof(IsFirstSelected));

            subType = IsFirstSelected ? Model.predicateA?.GetType() : Model.predicateB?.GetType();
            Notify(nameof(SubType));
        }));
    }
}