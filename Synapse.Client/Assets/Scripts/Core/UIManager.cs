using System.Collections.Generic;
using UnityEngine;
using Synapse.Client.UI;

namespace Synapse.Client.Core
{
    public class UIManager : ICoreManager
    {
        private UIRoot _uiRoot;

        public UIManager(UIRoot uiRoot)
        {
            _uiRoot = uiRoot;
        }
        
        public void Init()
        {
        }

        public void Dispose()
        {
        }
    }    
}