using FormsLib.Scope;


namespace LogViewer
{
    public static class AppScopeStyles
    {
        public static ScopeViewSettings GetDarkScope() => new ScopeViewSettings
        {
            Style = new Style
            {
                Name = "Dark",
                BackgroundColor = Color.Black,
                ForegroundColor = Color.FromArgb(0xA0, 0xA0, 0xA0),
                Font = new Font("Consolas", 8),
                GridPen = new Pen(Color.FromArgb(0x30, 0x30, 0x30)) { DashPattern = new float[] { 4.0F, 4.0F } },
                GridZeroPen = new Pen(Color.FromArgb(0xA0, 0xA0, 0xA0)),
            },
            DrawScalePosVertical = DrawPosVertical.Right,
            DrawScalePosHorizontal = DrawPosHorizontal.Bottom,
            GridZeroPosition = VerticalZeroPosition.Middle,
            HorizontalDivisions = 12,
            NotifyOnChange = true,
            VerticalDivisions = 8,
            ZeroPosition = VerticalZeroPosition.Middle,
            HorizontalToHumanReadable = (ticks) =>
            {
                if (double.IsNaN(ticks)) return "NaN";
                if (double.IsInfinity(ticks)) return "Inf";
                if (ticks > DateTime.MaxValue.Ticks) return "Inf";
                if (ticks < DateTime.MinValue.Ticks) return "-Inf";

                string result = "";
                try
                {
                    DateTime dt = new DateTime((long)ticks);
                    result = dt.ToString("dd-MM-yyyy") + " \r\n" + dt.ToString("HH:mm:ss");
                }
                catch { }
                return result;
            },
        };
    }


}




