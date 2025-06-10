using KCObjectsStandard.Data.Api.KC;
using Org.BouncyCastle.Pqc.Crypto.Lms;

namespace LogViewer.Config.Mergers
{

    public interface IConfigTypeMerger
    {
        bool CanMerge(Type type);
        bool TryMerge(IConfigMerger merger, object target, object source);
    }

}
