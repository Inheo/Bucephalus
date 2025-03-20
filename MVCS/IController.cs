using System;
using Cysharp.Threading.Tasks;

namespace Bucephalus.MVCS
{
    
    public interface IController<out T> : IDisposable where T : BaseView
    {
        UniTask Preload();
        UniTask Show();
        UniTask Hide();
        void Reshow(BaseModel model);
    }
}