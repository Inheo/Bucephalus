using System;
using System.Threading;
using UnityEngine;
using Bucephalus.Enums;
using Cysharp.Threading.Tasks;

namespace Bucephalus.MVCS
{
    public class BaseView : MonoBehaviour, IDisposable
    {
        public event Action AnyButtonClicked = delegate { };
        public virtual ViewType Type { get; protected set; } = ViewType.Dynamic;
        
        public virtual ushort SortingOrder { get; protected set; } = 0;

        public virtual byte Priority { get; protected set; } = 0;
        
        public virtual bool IsModal { get; protected set; } = false;

        public void OnAnyButtonClicked() => AnyButtonClicked();

        private CancellationTokenSource _cancellationTokenSource;
        protected CancellationToken CancellationToken => _cancellationTokenSource.Token;
        
        public virtual async UniTask Preload()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            await UniTask.CompletedTask;
        }
        
        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            OnDisposed();
        }
        
        protected virtual void OnDisposed()
        {
        }
    }
}