namespace LogViewer.AppContext
{
    public class ScopeViewContext
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ScopeMarkerContext> Markers { get; set; } = new List<ScopeMarkerContext>();

    }

}




