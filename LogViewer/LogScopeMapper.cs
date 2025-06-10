

using FormsLib.Scope;
using LogViewer.AppContext;
using LogViewer.Config.Models;
using LogViewer.Logging;
using System.ComponentModel;

namespace LogViewer
{
    public class LogScopeMapper
    {
        private readonly ScopeController _scopeController;
        private readonly LogViewerContext _appContext;


        public LogScopeMapper(ScopeController scopeController, LogViewerContext appContext)
        {
            _scopeController = scopeController;
            _appContext = appContext;
        }

        private bool IsMatch(Dictionary<LogKeys, string> dict, LogEntry entry)
        {
            LogEntryReader reader = new LogEntryReader(entry);

            foreach (var item in dict)
            {
                if (!reader.TryGetByKey(item.Key, out string segmentValue))
                    return false;

                if (item.Value != segmentValue)
                    return false;
            }
            return true;
        }

        public void Redraw()
        {
            SetupTraces(_appContext.Profile);

            foreach (var logEntry in _appContext.LogCollection.Entries)
            {
                LogEntryReader reader = new LogEntryReader(logEntry);

                bool anyMatch = false;
                float measurement;
                DateTime timestamp;

                if (!reader.TryGetByKey(LogKeys.TimeStamp, out timestamp))
                    continue;

                if (reader.TryGetByKey(LogKeys.Measurement, out measurement))
                {
                    foreach (var trace in _scopeController.Traces)
                    {
                        if (trace.Tag is TraceConfig traceConfig)
                        {
                            bool match = IsMatch(traceConfig.IncludeLogs, logEntry);
                            anyMatch |= match;
                            if (match)
                            {
                                trace.Points.Add(new FormsLib.Maths.PointD(timestamp.Ticks, measurement));
                            }
                        }
                    }
                }

                // If datapoint has no association with trace, draw it as a label
                if (!anyMatch)
                {
                    if (Enum.TryParse<LogKeys>(_appContext.Profile.Unhandeled?.NameKey ?? LogKeys.LogCode.ToString(), out var key))
                    {
                        string val;
                        if (reader.TryGetByKey(key, out val))
                        {
                            var label = new FreeLabel(timestamp.Ticks, _appContext.Profile.Unhandeled?.Offset ?? 0);
                            label.Text = val;
                            _scopeController.Labels.Add(label);
                        }
                    }
                }
            }

            // Make traces without explicit visible configuration, invisible when they contain no data.
            foreach (var trace in _scopeController.Traces)
            {
                if (trace.Tag is TraceConfig traceConfig)
                {
                    switch (traceConfig.Visible)
                    {
                        case VisibleOptions.Always:
                            trace.Visible = true;
                            break;
                        case VisibleOptions.WhenData:
                            trace.Visible = trace.Points.Count > 0;
                            break;
                        case VisibleOptions.Never:
                            trace.Visible = false;
                            break;
                    }
                }
            }



            _scopeController.Markers.ListChanged -= Markers_ListChanged;

            foreach (var marker in _appContext.ScopeViewContext.Markers)
                _scopeController.Markers.Add(new Marker() { X = marker.X.Ticks, Name = marker.Name, Id = marker.Id });

            _scopeController.Markers.ListChanged += Markers_ListChanged;

            _scopeController.Settings.SetHorizontal(_appContext.ScopeViewContext.StartDate, _appContext.ScopeViewContext.EndDate);
            _scopeController.RedrawAll();
        }

        private void Markers_ListChanged(object? sender, System.ComponentModel.ListChangedEventArgs e)
        {
            if (sender is BindingList<Marker> list)
            {
                switch (e.ListChangedType)
                {
                    case ListChangedType.ItemAdded:
                        // Handle item addition
                        var newItem = list[e.NewIndex];
                        var newMarker = new ScopeMarkerContext
                        {
                            Name = newItem.Name,
                            X = new DateTime((long)newItem.X),
                            Id = newItem.Id,
                        };
                        _appContext.ScopeViewContext.Markers.Add(newMarker);
                        break;

                    case ListChangedType.ItemDeleted:
                        // Handle item deletion
                        if (e.OldIndex >= 0 && e.OldIndex < _appContext.ScopeViewContext.Markers.Count)
                        {
                            var markerToRemove = _appContext.ScopeViewContext.Markers
                                .FirstOrDefault(m => m.Id == list[e.OldIndex].Id);

                            if (markerToRemove != null)
                            {
                                _appContext.ScopeViewContext.Markers.Remove(markerToRemove);
                            }
                        }
                        break;

                    case ListChangedType.ItemChanged:
                        // Handle item change
                        if (e.OldIndex >= 0 && e.OldIndex < _appContext.ScopeViewContext.Markers.Count)
                        {
                            var updatedItem = list[e.NewIndex];
                            var markerToUpdate = _appContext.ScopeViewContext.Markers
                                .FirstOrDefault(m => m.Id == list[e.OldIndex].Id);

                            if (markerToUpdate != null)
                            {
                                markerToUpdate.Name = updatedItem.Name;
                                markerToUpdate.X = new DateTime((long)updatedItem.X);
                                markerToUpdate.Id = updatedItem.Id; 
                            }
                        }
                        break;
                }
            }
        }

        private void SetupTraces(ProfileConfig profile)
        {
            _scopeController.Traces.Clear();
            _scopeController.Drawables.Clear();
            _scopeController.Labels.Clear();
            _scopeController.Markers.Clear();

            foreach (var traceConfig in profile.Traces.Values.OrderBy(x=>x.Name).OrderByDescending(x=>x.Offset))
            {
                Trace trace = new Trace
                {
                    Name = traceConfig.Name ?? "New trace",
                    Unit = traceConfig.Unit ?? " ",
                    Color = traceConfig.Color ?? Color.Pink,
                    Visible = true,
                    Offset = traceConfig.Offset ?? 0,
                    Scale = traceConfig.Scale ?? 10,
                    DrawStyle = traceConfig.DrawStyle ?? Trace.DrawStyles.Points,
                    DrawOption = traceConfig.DrawOption ?? Trace.DrawOptions.None,
                    Tag = traceConfig
                };

                switch (traceConfig.DrawStyle)
                {
                    case Trace.DrawStyles.State:
                        trace.ToHumanReadable = x =>
                        {
                            if (!double.IsNormal(x) && x!=0)
                                return "";

                            int intValue = (int)x;

                            if (traceConfig?.StateNames?.TryGetValue(intValue, out var name) ?? false)
                                return name;

                            return intValue.ToString();
                        };
                        break;

                    case Trace.DrawStyles.Points:
                    case Trace.DrawStyles.Lines:
                    case Trace.DrawStyles.NonInterpolatedLine:
                    case Trace.DrawStyles.DiscreteSingal:
                    case Trace.DrawStyles.Cross:
                    default:
                        trace.ToHumanReadable = x =>
                        {
                            if (!double.IsNormal(x) && x != 0)
                                return "";

                            return x.ToString("F2");
                        };
                        break;
                }

                _scopeController.Traces.Add(trace);
            }
        }
    }
}







