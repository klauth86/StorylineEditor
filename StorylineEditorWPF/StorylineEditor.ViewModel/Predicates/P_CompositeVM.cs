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
using StorylineEditor.ViewModel.Interface;
using System;

namespace StorylineEditor.ViewModel.Predicates
{
    public class P_CompositeVM : P_BaseVM<P_CompositeM, object>
    {
        public P_CompositeVM(P_CompositeM model, object parent) : base(model, parent) { }

        public Type SubTypeA
        {
            get => null;
            set
            {
                if (value != null)
                {
                    PredicateA = PredicatesHelper.CreatePredicateByType(value, Parent);
                    Model.predicateA = PredicateA.GetModel<P_BaseM>();
                    Notify(nameof(PredicateA));
                }
                Notify(nameof(SubTypeA));
            }
        }

        public IPredicate PredicateA { get; set; }

        public Type SubTypeB
        {
            get => null;
            set
            {
                if (value != null)
                {
                    PredicateB = PredicatesHelper.CreatePredicateByType(value, Parent);
                    Model.predicateB = PredicateB.GetModel<P_BaseM>();
                    Notify(nameof(PredicateB));
                }
                Notify(nameof(SubTypeB));
            }
        }

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
    }
}