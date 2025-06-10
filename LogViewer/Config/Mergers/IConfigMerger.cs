namespace LogViewer.Config.Mergers
{
    public interface IConfigMerger
    {
        bool TryMerge(object target, object source);
        void RegisterMerger(IConfigTypeMerger merger);
    }






}
