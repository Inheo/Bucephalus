using Bucephalus.AssetsManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Bucephalus
{
    internal class ViewBuilder
    {
        AssetProvider _assetProvider = new ();
        
        public async UniTask<GameObject> Build(string viewName, Transform root)
        {
            var viewPrefab = await _assetProvider.Load<GameObject>($"{viewName}.prefab");
            Assert.IsNotNull(viewPrefab, $"[ViewBuilder] Fail Load {viewName}.prefab");
            var instantiateObject = Object.Instantiate(viewPrefab, root);
            Assert.IsNotNull(instantiateObject, $"[ViewBuilder] Fail instantiate {viewName}");
            return instantiateObject;
        }

        public void Unload(string viewName)
        {
            _assetProvider.Release($"{viewName}.prefab");
        }
    }
}