using System.Diagnostics;

namespace LogViewer.Config.Mergers
{
    public class ConfigMerger : IConfigMerger
    {
        private readonly List<IConfigTypeMerger> _mergers = new();

        public void RegisterMerger(IConfigTypeMerger merger)
        {
            _mergers.Add(merger);
        }

        public bool TryMerge(object target, object source)
        {
            var merger = _mergers.FirstOrDefault(m => m.CanMerge(target.GetType()));

            if (merger == null)
            {
                Debug.WriteLine($"No merger registered for type {target.GetType().Name}");
                return false;
            }

            return merger.TryMerge(this, target, source);
        }
    }






}
