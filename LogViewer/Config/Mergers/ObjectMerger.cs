using System.Reflection;

namespace LogViewer.Config.Mergers
{
    public class ObjectMerger : IConfigTypeMerger
    {
        public bool CanMerge(Type type)
        {
            return type.IsClass && type != typeof(string);
        }

        public bool TryMerge(IConfigMerger merger, object target, object source)
        {
            if (target == null || source == null)
                return false;

            var type = target.GetType();

            if (type != source.GetType())
                return false;

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                var targetValue = prop.GetValue(target);
                var sourceValue = prop.GetValue(source);

                if (sourceValue == null)
                    continue; // Keep target value

                if (targetValue == null)
                {
                    prop.SetValue(target, sourceValue); // Copy over if target is null
                    continue;
                }

                if (merger.TryMerge(targetValue, sourceValue))
                {
                    prop.SetValue(target, targetValue);
                }
                else
                {
                    // Fallback: use source value
                    prop.SetValue(target, sourceValue);
                }
            }

            return true;
        }
    }






}
