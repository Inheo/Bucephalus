using System;
using Cysharp.Threading.Tasks;

namespace Bucephalus.MVCS
{
    public class BaseServicesMediator : IDisposable
    {
        protected readonly IViewHandler ViewHandler;
        
        public BaseServicesMediator(IViewHandler viewHandler)
        {
            ViewHandler = viewHandler;
        }
        
        public virtual async UniTask<BaseModel> CreateModel(Type modelType)
        {
            return (BaseModel)Activator.CreateInstance(modelType);
        }
        
        public void Dispose()
        {
            OnDisposed();
        }
        
        protected virtual void OnDisposed() { }
    }
}