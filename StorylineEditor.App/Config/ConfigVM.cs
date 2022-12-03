using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Config;
using System.Collections.Generic;

namespace StorylineEditor.App.Config
{
    public class ConfigVM : SimpleVM<ConfigM>
    {
        public ConfigVM(ConfigM model, ICallbackContext callbackContext) : base(model, callbackContext)
        {
            UserActions = new List<UserActionVM>();
            
            foreach (var userAction in Model.UserActions)
            {
                UserActions.Add(new UserActionVM(userAction, callbackContext));
            }
        }

        public override string Id => throw new System.NotImplementedException();

        public List<UserActionVM> UserActions { get; }

        public override string Title => null;

        public override string Stats => null;
    }
}