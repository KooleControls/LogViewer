using FormsLib.Scope.Controls;
using LogViewer.Logging;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Mapping.Models
{
    public interface ITraceMapper
    {
        IEnumerable<TraceDescriptor> Map(IEnumerable<LogEntry> entries);
    }
}
