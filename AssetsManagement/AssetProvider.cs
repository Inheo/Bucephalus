using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Bucephalus.AssetsManagement
{
    public class AssetProvider
    {
        private readonly Dictionary<string, AsyncOperationHandle> _completedCache = new Dictionary<string, AsyncOperationHandle>();
        private readonly Dictionary<string, List<AsyncOperationHandle>> _handles = new Dictionary<string, List<AsyncOperationHandle>>();

        public AssetProvider()
        {
            Addressables.InitializeAsync();
        }

        public async UniTask<T> Load<T>(string address) where T : class
        {
            if (_completedCache.TryGetValue(address, out AsyncOperationHandle completedHandle))
                return completedHandle.Result as T;

            return await RunWithCacheOnComplete(Addressables.LoadAssetAsync<T>(address), address);
        }

        public void Release(string address)
        {
            if (_handles.TryGetValue(address, out var resourceHandles))
            {
                foreach (var handle in resourceHandles)
                {
                    Addressables.Release(handle);
                }
            }
            
            if (_completedCache.TryGetValue(address, out var completedHandle))
            {
                Addressables.Release(completedHandle);
            }
        }

        public void CleanUp()
        {
            foreach (List<AsyncOperationHandle> resourceHandles in _handles.Values)
            {
                foreach (AsyncOperationHandle handle in resourceHandles)
                {
                    Addressables.Release(handle);
                }
            }

            _completedCache.Clear();
            _handles.Clear();
        }

        private async UniTask<T> RunWithCacheOnComplete<T>(AsyncOperationHandle<T> handle, string key) where T : class
        {
            handle.Completed += completeHandler =>
            {
                _completedCache[key] = completeHandler;
            };

            AddHandle<T>(handle, key);

            return await handle.Task;
        }

        private void AddHandle<T>(AsyncOperationHandle handle, string key) where T : class
        {
            if (!_handles.TryGetValue(key, out List<AsyncOperationHandle> resourceHandles))
            {
                resourceHandles = new List<AsyncOperationHandle>();
                _handles[key] = resourceHandles;
            }

            resourceHandles.Add(handle);
        }
    }
}
