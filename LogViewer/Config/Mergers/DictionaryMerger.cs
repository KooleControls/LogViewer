using System.Collections;

namespace LogViewer.Config.Mergers
{
    public class DictionaryMerger : IConfigTypeMerger
    {
        public bool CanMerge(Type type)
        {
            return typeof(IDictionary).IsAssignableFrom(type) ||
                   (type.IsGenericType && typeof(IDictionary<,>).IsAssignableFrom(type.GetGenericTypeDefinition()));
        }

        public bool TryMerge(IConfigMerger merger, object target, object source)
        {

            if (target is not IDictionary first)
                return false;

            if (source is not IDictionary second)
                return false;

            foreach (DictionaryEntry entry in second)
            {
                if (!first.Contains(entry.Key))
                {
                    first[entry.Key] = entry.Value;
                    continue;
                }

                var targetValue = first[entry.Key];
                var sourceValue = entry.Value;

                // Try merging if both values are non-null and of the same type
                if (targetValue != null && sourceValue != null)
                {
                    if (!merger.TryMerge(targetValue, sourceValue))
                        return false;
                }
            }

            return true;
        }
    }






}
