using LogViewer.Config.Models;

namespace LogViewer.Config.Mergers
{
    public class LogViewerConfigMerger : IConfigTypeMerger
    {
        public bool CanMerge(Type type) => type == typeof(LogViewerConfig);

        public bool TryMerge(IConfigMerger merger, object target, object source)
        {

            if (target is not LogViewerConfig first)
                return false;

            if (source is not LogViewerConfig second)
                return false;

            if (!merger.TryMerge(first.Organisations, second.Organisations))
                return false;

            if (!merger.TryMerge(first.Profiles, second.Profiles))
                return false;

            return true;
        }
    }






}
