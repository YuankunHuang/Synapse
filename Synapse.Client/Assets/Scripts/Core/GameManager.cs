using System;
using Synapse.Client.Core.Network;
using Synapse.Client.Core.UI;
using Synapse.Client.Core.World;
using UnityEngine;
using Synapse.Client.UI;
using YuankunHuang.Unity.SimpleObjectPool;

namespace Synapse.Client.Core
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private UIRoot _uiRoot;
        [SerializeField] private WorldRoot _worldRoot;
        
        private INetworkManager _networkManager;
        private IUIManager _uiManager;
        private IWorldManager _worldManager;
        
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
            
            _worldManager = new WorldManager(_worldRoot);
            _worldManager.Init();
            ModuleRegistry.Register(_worldManager);

            EventBus.Publish(EventKeys.GameInitialized);
        }

        private void OnDisable()
        {
            PoolService.ClearAll();
            
            ModuleRegistry.Unregister<NetworkManager>();
            ModuleRegistry.Unregister<UIManager>();
            ModuleRegistry.Unregister<WorldManager>();
            
            _networkManager.Dispose();
            _networkManager = null;
            _uiManager.Dispose();
            _uiManager = null;
            _worldManager.Dispose();
            _worldManager = null;
        }
    }
}