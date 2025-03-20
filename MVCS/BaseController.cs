using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Bucephalus.MVCS
{
    public class BaseController<TBaseView, TBaseModel, TBaseServicesMediator> : IController<BaseView>
        where TBaseView : BaseView
        where TBaseModel : BaseModel
        where TBaseServicesMediator : BaseServicesMediator
    {
        protected TBaseView View { get; }
        protected TBaseModel Model { get; private set; }
        protected TBaseServicesMediator ServicesMediator { get; }
        protected CompositeDisposable Disposables { get; } = new();
        protected CancellationTokenSource DisposeCancellation { get; private set; }
        
        protected BaseController(TBaseView view, TBaseModel model, TBaseServicesMediator servicesMediator)
        {
            View = view;
            Model = model;
            ServicesMediator = servicesMediator;
        }

        public virtual async UniTask Preload()
        {
            DisposeCancellation = new CancellationTokenSource();
            
            await View.Preload();
        }
        
        public void Dispose()
        {
            Disposables.Dispose();
            View.Dispose();
            
            OnDisposed();
            
            DisposeCancellation?.Cancel();
            DisposeCancellation?.Dispose();
        }

        public virtual UniTask Show()
        {
            View.gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        public virtual UniTask Hide()
        {
            View.gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }
        
        public virtual void Reshow(BaseModel model)
        {
            Model = (TBaseModel)model;
            OnReshowed(model);
        }

        protected virtual void OnDisposed() { }
        protected virtual void OnReshowed(BaseModel model) { }
    }
}