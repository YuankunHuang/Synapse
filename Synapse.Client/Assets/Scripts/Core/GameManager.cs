using System;
using UnityEngine;
using Synapse.Client.UI;

namespace Synapse.Client.Core
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private UIRoot _uiRoot;
        
        private NetworkManager _networkManager;
        private UIManager _uiManager;
        
        private void OnEnable()
        {
            var targetFrameRateVar = Environment.GetEnvironmentVariable("SYNAPSE_TARGET_FRAME_RATE");
            if (!int.TryParse(targetFrameRateVar, out var targetFrameRate))
            {
                targetFrameRate = 60;
            }
            Application.targetFrameRate = targetFrameRate;
            
            _networkManager = new NetworkManager();
            _networkManager.Init();
            ModuleRegistry.Register(_networkManager);
            
            _uiManager = new UIManager(_uiRoot);
            _uiManager.Init();
            ModuleRegistry.Register(_uiManager);

            EventBus.Publish(EventKeys.GameInitialized);
        }

        private void OnDisable()
        {
            ModuleRegistry.Unregister<NetworkManager>();
            ModuleRegistry.Unregister<UIManager>();
            
            _networkManager.Dispose();
            _networkManager = null;
            _uiManager.Dispose();
            _uiManager = null;
        }
    }
}