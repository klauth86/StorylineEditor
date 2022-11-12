using StorylineEditor.Model.GameEvents;
using StorylineEditor.ViewModel.Common;
using System;

namespace StorylineEditor.ViewModel.GameEvents
{
    public abstract class GE_BaseVM<T> : BaseVM<T> where T : GE_BaseM
    {
        public GE_BaseVM(T model, ICallbackContext callbackContext) : base(model, callbackContext) { }

        public Type GameEventType => Model?.GetType();

        public byte ExecutionMode
        {
            get => Model.executionMode;
            set
            {
                if (Model.executionMode != value)
                {
                    Model.executionMode = value;
                    Notify(nameof(ExecutionMode));
                }

            }
        }
    }
}