using System;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using Bucephalus.Attribute;
using Bucephalus.Enums;
using Bucephalus.MVCS;
using Bucephalus.Preprocessor;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Bucephalus
{
    public sealed class Director : IViewHandler
    {
        private ViewBuilder _viewBuilder = new();
        
        private Camera _camera;
        private Transform _viewRoot;
        
        private List<ViewData> _currentModalViews;
        private Dictionary<string, ViewData> _viewDataDictionary;
        private List<string> _shownViews;
        private readonly CancellationTokenSource _ts = new();
        
        private class ViewData
        {
            private readonly string _viewId;
            private readonly IViewHandler _viewHandler;

            private BaseModel _model;
            private BaseView _view;
            private BaseServicesMediator _servicesMediator;
            private IController<BaseView> _controller;
            
            private Canvas[] _canvases;
            private GraphicRaycaster[] _raycasters;


            public ViewType ViewType { get; private set; }
            public bool IsModal { get; private set; }
            
            public ViewData(string viewId, IViewHandler viewHandler)
            {
                _viewId = viewId;
                _viewHandler = viewHandler;
            }

            public async UniTask Preload(ViewBuilder _viewBuilder, Transform _viewRoot, Camera _camera, Action<BaseView> onViewPreloaded)
            {
                var viewGameObject = await _viewBuilder.Build(_viewId, _viewRoot);
                var view = viewGameObject.GetComponent<BaseView>();
                Assert.IsNotNull(view, $"[Bucephalus] Fail load view {_viewId}, not find BaseView");
                
                _canvases = viewGameObject.GetComponentsInChildren<Canvas>();
                foreach (var canvas in _canvases)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = _camera;
                    canvas.sortingLayerName = "UI";
                }
                
                _raycasters = viewGameObject.GetComponentsInChildren<GraphicRaycaster>();
                
                viewGameObject.SetActive(false);
                
                ViewType = view.Type;
                IsModal = view.IsModal;
                
                var viewType = view.GetType();
                Assert.IsNotNull(viewType, $"[Bucephalus] not find View Type for {_viewId}");
                
                var controllerType = FindViewControllerType(viewType);
                Assert.IsNotNull(controllerType, $"[Bucephalus] not find Controller Type for {viewType}");
                
                var modelAndServicesMediatorTypes = FindModelAndServicesMediatorType(controllerType);
                if (_servicesMediator == null)
                {
                    _servicesMediator = (BaseServicesMediator)Activator.CreateInstance(modelAndServicesMediatorTypes.mediatorType, _viewHandler);
                    Assert.IsNotNull(_servicesMediator, $"[Bucephalus] ServicesMediator is null for {viewType}");
                }

                var model = CreateModel(modelAndServicesMediatorTypes.modelType, viewType);
                
                var controller = Activator.CreateInstance(controllerType, view, model, _servicesMediator) as IController<BaseView>;
                Assert.IsNotNull(controller, $"[Bucephalus] IController is null for {viewType}");
                
                _controller = controller;
                _view = view;

                await _controller.Preload();
                SetSortingOrder();
                    
                onViewPreloaded?.Invoke(_view);
            }

            public void Dispose(Action<BaseView> onViewDisposed = null)
            {
                onViewDisposed?.Invoke(_view);
                _controller.Dispose();
                if (_view != null)
                {
                    UnityEngine.Object.Destroy(_view.gameObject);
                }
                _controller = null;
                _view = null;
                _canvases = null;
                _raycasters = null;
            }
            
            public async UniTask Show()
            {
                SetSortingOrder();
                await _controller.Show();
            }

            public async UniTask Hide() => await _controller.Hide();

            private void SetSortingOrder()
            {
                foreach (var canvas in _canvases)
                {
                    var sortingOrder = (int)_view.Type + _view.SortingOrder + _view.Priority;
                    canvas.sortingOrder = Mathf.Max(sortingOrder, canvas.sortingOrder);
                }

                if (_view.IsModal)
                {
                    _view.transform.SetAsLastSibling();
                }
            }

            public void Disable()
            {
                if(_raycasters == null)
                    return;
                for (var index = 0; index < _raycasters.Length; index++)
                {
                    _raycasters[index].enabled = false;
                }
            }
            
            public void Enable()
            {
                if(_raycasters == null)
                    return;
                for (var index = 0; index < _raycasters.Length; index++)
                {
                    _raycasters[index].enabled = true;
                }
            }

            public async void ReShow()
            {
                var viewType = _view.GetType();
                Assert.IsNotNull(_model, $"[Bucephalus] Model is null for {viewType}");
                var model = await CreateModel(_model.GetType(), viewType);
                _controller.Reshow(model);
            }

            private async UniTask<BaseModel> CreateModel(Type modelType, Type viewType)
            {
                var model = await _servicesMediator.CreateModel(modelType);
                Assert.IsNotNull(model, $"[Bucephalus] Model is null for {viewType}");
                _model = model;
                return model;
            }
        }
        
        public Director(Transform viewRoot, Camera camera)
        {
            _camera = camera;
            _viewRoot = viewRoot;
        }

        public event Action<BaseView> OnViewPreloaded = delegate { };
        public event Action<BaseView> OnViewDisposed = delegate { };
        
        public async UniTask Preload()
        {
#if UNITY_EDITOR
            var viewIds = AsmFinder.FindViewIds();
#else
            var viewIds = Generated.ViewIds;
#endif
            _viewDataDictionary = new Dictionary<string, ViewData>(viewIds.Count);
            _currentModalViews = new List<ViewData>(10);
            
            var preloadViews = new List<UniTask>();
            
            foreach (var viewId in viewIds)
            {
                var viewData = new ViewData(viewId, this);
                _viewDataDictionary.Add(viewId, viewData);
                
                preloadViews.Add(viewData.Preload(_viewBuilder, _viewRoot, _camera, OnViewPreloaded));
            }

            await UniTask.WhenAll(preloadViews);
            
            _shownViews = new List<string>(_viewDataDictionary.Count);
            
            foreach (var viewId in viewIds)
            {
                if (_viewDataDictionary[viewId].ViewType == ViewType.Dynamic)
                {
                    UnloadView(viewId);
                }
                else
                {
                    ShowView(viewId).Forget();
                }
            }
        }

        private void UnloadView(string viewId)
        {
            _viewDataDictionary[viewId].Dispose(OnViewDisposed);
            _viewBuilder.Unload(viewId);
        }
        
        public async UniTask ShowView(string id)
        {
            Assert.IsTrue(_viewDataDictionary.ContainsKey(id));
            
            var viewData = _viewDataDictionary[id];
            
            if (_shownViews.Contains(id))
            {
                viewData.ReShow();
                return;
            }
            
            _shownViews.Add(id);

            if (_viewDataDictionary[id].ViewType == ViewType.Dynamic)
            {
                await _viewDataDictionary[id].Preload(_viewBuilder, _viewRoot, _camera, OnViewPreloaded);
            }

            if (viewData.IsModal)
            {
                _currentModalViews.Add(viewData);
            }

            await viewData.Show();
        }

        public async UniTask HideView(string id)
        {
            Assert.IsTrue(_viewDataDictionary.ContainsKey(id));
            if(!_shownViews.Contains(id))
                return;
            
            _shownViews.Remove(id);

            await _viewDataDictionary[id].Hide();
            
            if (_currentModalViews.Contains(_viewDataDictionary[id]))
            {
                _currentModalViews.Remove(_viewDataDictionary[id]);
            }
            
            
            if (_viewDataDictionary[id].ViewType == ViewType.Dynamic)
            {
                UnloadView(id);
            }
        }
        
        private static (Type modelType, Type mediatorType) FindModelAndServicesMediatorType(Type controllerType)
        {
            var controllerAttribute = controllerType.GetCustomAttribute<ControllerAttribute>();
            Assert.IsNotNull(controllerAttribute, $"[Bucephalus] Controller type {controllerType.FullName} must have ControllerAttribute.");
            return (controllerAttribute.ModelType, controllerAttribute.ServiceMediatorType);
        }
        
        private static Type FindViewControllerType(Type viewType)
        {
            var attributeDataCollection = viewType.GetCustomAttributesData();

            foreach (var data in attributeDataCollection)
            {
                if (data.AttributeType == typeof(ViewAttribute))
                {
                    return data.ConstructorArguments[0].Value as Type;
                }
            }

            return null;
        }
    }
    
}
