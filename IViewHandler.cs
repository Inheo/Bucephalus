using Cysharp.Threading.Tasks;

namespace Bucephalus
{
    public interface IViewHandler
    {
        UniTask ShowView(string id);
        UniTask HideView(string id);
    }
}