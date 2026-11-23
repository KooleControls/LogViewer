using FormsLib.Scope.Controls;
using CoreLib.Misc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using FormsLib.Design;
using FormsLib.Misc;
using FormsLib.Extentions;
using FormsLib.Maths;
using System.Security.Cryptography.X509Certificates;

namespace FormsLib.Scope
{
    public class Trace : PropertySensitive
    {
        [TraceViewAttribute(Width = 20, HideValue = true)]
        public Color Color { get { return GetPar(Colors.Green); } set { SetPar(value); } }
        [TraceViewAttribute(Width = 20, HeaderText = "")]
        public bool Visible { get { return GetPar(true); } set { SetPar(value); } }
        [TraceViewAttribute(Width = 50)]
        public string Name { get { return GetPar("New trace"); } set { SetPar(value); } }

        //[TraceViewAttribute]
        public string Unit { get { return GetPar(""); } set { SetPar(value); } }
        [TraceViewAttribute(Width = 40)]
        public double Scale { get { return GetPar<double>(1f); } set { SetPar<double>(value); } }
        [TraceViewAttribute(Width = 40)]
        public double Offset { get { return GetPar<double>(0f); } set { SetPar<double>(value); } }
        public ThreadedBindingList<PointD> Points { get; } = new ThreadedBindingList<PointD>();
        //public ThreadedBindingList<Mark> Marks { get; } = new ThreadedBindingList<Mark>();
        [TraceViewAttribute(Width = 80)]
        public DrawStyles DrawStyle { get { return GetPar(DrawStyles.Lines); } set { SetPar(value); } }
        [TraceViewAttribute]
        public DrawOptions DrawOption { get { return GetPar(DrawOptions.ShowScale); } set { SetPar(value); } }
        public Func<double, string> ToHumanReadable { get { return GetPar(new Func<double, string>((a) => a.ToHumanReadable(3) + Unit)); } set { SetPar(value); } }
        public PointD Minimum { get { return GetPar(PointD.Empty); } set { SetPar(value); } }
        public PointD Maximum { get { return GetPar(PointD.Empty); } set { SetPar(value); } }
        public Trace Self { get { return this; } }
        public object Tag { get; set; }

        /// <summary>
        /// State segments narrower than this many pixels are not drawn one by one: each run of them
        /// becomes one hatched band. Below this width a segment's brackets merge with its neighbours'
        /// into a smear anyway, and a burst of transitions can put thousands of them in a few pixels.
        /// </summary>
        public const int MinStateSegmentWidth = 4;

        /// <summary>
        /// Whether <see cref="Points"/> is in ascending X order, which lets <see cref="Draw"/> find
        /// the visible points by binary search. Kept up to date from <see cref="Points_ListChanged"/>.
        /// </summary>
        private bool pointsSorted = true;

        public Trace()
        {
            Points.ListChanged += Points_ListChanged;
        }

        public void AddPointSort(PointD pt)
        {
            int i = 0;
            for(i=0; i< Points.Count; i++)
            {
                if (pt.X < Points[i].X)
                {
                    Points.Insert(i, pt);
                    return;
                }
            }
            Points.Add(pt);
        }

        public override string ToString()
        {
            return Name;
        }

        public void AddPoint(DateTime dt, double value) => Points.Add(dt.Ticks, value);
        public void AddPoint(double x, double value) => Points.Add(x, value);


        private void Points_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case System.ComponentModel.ListChangedType.ItemAdded:
                    DoMinMax(Points[e.NewIndex]);
                    if (pointsSorted && !IsInOrderAt(e.NewIndex))
                        pointsSorted = false;
                    break;

                default:
                    //TODO: This has a serious potential to be optimized!
                    Minimum = PointD.Empty;
                    Maximum = PointD.Empty;
                    pointsSorted = true;
                    for (int i = 0; i < Points.Count; i++)
                    {
                        DoMinMax(Points[i]);
                        if (pointsSorted && !IsInOrderAt(i))
                            pointsSorted = false;
                    }
                    break;
            }
        }

        /// <summary>Whether the point at <paramref name="index"/> is not before its neighbours. NaN counts as out of order.</summary>
        private bool IsInOrderAt(int index)
        {
            double x = Points[index].X;
            if (index > 0 && !(Points[index - 1].X <= x))
                return false;
            if (index < Points.Count - 1 && !(x <= Points[index + 1].X))
                return false;
            return true;
        }

        /// <summary>Index of the first point with X at or after <paramref name="x"/>. Needs <see cref="pointsSorted"/>.</summary>
        private int LowerBound(double x)
        {
            int lo = 0, hi = Points.Count;
            while (lo < hi)
            {
                int mid = lo + (hi - lo) / 2;
                if (Points[mid].X < x) lo = mid + 1; else hi = mid;
            }
            return lo;
        }

        /// <summary>Index of the first point with X after <paramref name="x"/>. Needs <see cref="pointsSorted"/>.</summary>
        private int UpperBound(double x)
        {
            int lo = 0, hi = Points.Count;
            while (lo < hi)
            {
                int mid = lo + (hi - lo) / 2;
                if (Points[mid].X <= x) lo = mid + 1; else hi = mid;
            }
            return lo;
        }

        /// <summary>
        /// The points <see cref="Draw"/> has to look at for the view <paramref name="xLeft"/> to
        /// <paramref name="xRight"/>: those inside it plus one either side, so lines and states that
        /// enter or leave the view are still drawn. Everything outside this range would fail
        /// Draw's own viewport test - the right side gets extra slack because
        /// <c>CheckIfPointIsWithin</c> counts the viewport's X offset twice. Falls back to every
        /// point when they are not in order.
        /// </summary>
        private void GetDrawRange(Rectangle viewPort, double xLeft, double xRight, out int from, out int to)
        {
            from = 0;
            to = Points.Count - 1;
            if (!pointsSorted || viewPort.Width <= 0 || !(xRight > xLeft))
                return;

            double unitsPerPx = (xRight - xLeft) / viewPort.Width;
            from = Math.Max(0, LowerBound(xLeft - 2 * unitsPerPx) - 1);
            to = Math.Min(Points.Count - 1, UpperBound(xRight + (viewPort.X + 2) * unitsPerPx));
        }

        void DoMinMax(PointD pt)
        {
            Minimum.KeepMinimum(pt);
            Maximum.KeepMaximum(pt);
        }


        public IEnumerable<PointD> GetPointsBetweenMarkers(Marker marker1, Marker marker2)
        {
            double x1 = 0;
            double x2 = 0;

            if (marker1.X < marker2.X)
            {
                x1 = marker1.X;
                x2 = marker2.X;
            }
            else
            {
                x1 = marker2.X;
                x2 = marker1.X;
            }


            bool startFound = false;
            bool endFound = false;
            for (int i = 0; i < Points.Count; i++)
            {
                if (Points[i].X >= x1)
                    startFound = true;

                if (Points[i].X > x2)
                    endFound = true;

                if (startFound == true && endFound == false)
                    yield return Points[i];

            }
        }

        private int BinarySearchPoint(ThreadedBindingList<PointD> points, double x)
        {
            int left = 0;
            int right = points.Count - 1;

            while (left <= right)
            {
                int mid = (left + right) / 2;

                if (points[mid].X == x)
                    return mid; // Return exact match
                else if (points[mid].X < x)
                    left = mid + 1;
                else
                    right = mid - 1;
            }

            // Return the first index where points[i].X > x, or points.Count if x is beyond the last point
            return left;
        }


        public double GetYValue(double x)
        {
            if (Points.Count == 0)
                return 0;

            // Use binary search to find the closest point
            int i = BinarySearchPoint(Points, x);

            if (i < 0) i = ~i; // Get the insertion point if x is not found exactly

            // If we're beyond the last point, check if we can extend
            bool canExtendEnd = DrawOption.HasFlag(DrawOptions.ExtendEnd);

            switch (DrawStyle)
            {
                case DrawStyles.Lines:
                    if (i > 0 && i < Points.Count)
                    {
                        double x1 = Points[i - 1].X;
                        double x2 = Points[i].X;
                        double y1 = Points[i - 1].Y;
                        double y2 = Points[i].Y;

                        // Linear interpolation formula
                        double a = (y2 - y1) / (x2 - x1);
                        return a * (x - x1) + y1;
                    }
                    break;

                case DrawStyles.DiscreteSingal:
                case DrawStyles.Points:
                case DrawStyles.NonInterpolatedLine:
                case DrawStyles.State:
                    if (i > 0 && (i < Points.Count || (i == Points.Count && canExtendEnd)))
                        return Points[i - 1].Y;
                    break;

                default:
                    throw new NotImplementedException($"Not yet implemented GetYValue for drawstyle '{DrawStyle}'");
            }

            return double.NaN;
        }

        public void Draw(Graphics g, Style style, Rectangle viewPort, Func<PointD, Point> convert, double firstX, double lastX, double xLeft, double xRight)
        {
            int pointCnt = Points.Count;
            if (!Visible || pointCnt == 0) return;

            // Every property here is a dictionary lookup (ToHumanReadable even allocates its default
            // each time) and Font.Height creates a screen DC, so read them once, not once per point.
            DrawStyles drawStyle = DrawStyle;
            DrawOptions drawOption = DrawOption;
            Func<double, string> toHumanReadable = ToHumanReadable;
            Font font = style.Font;
            int fontHeight = GraphicsExt.GetFontHeight(font);

            // Cache flags to avoid repeated calls to HasFlag
            bool extendEnd = drawOption.HasFlag(Trace.DrawOptions.ExtendEnd);
            bool extendBegin = drawOption.HasFlag(Trace.DrawOptions.ExtendBegin);
            bool showCrosses = drawOption.HasFlag(Trace.DrawOptions.ShowCrosses);

            // Cache Pen and Brush to avoid recreating them
            using Pen pen = new Pen(Color);
            using Brush brush = new SolidBrush(Color);
            Brush? stateRunBrush = null;

            double stateY = convert(new PointD(0, 1)).Y;

            // The run of too-narrow state segments waiting to be drawn as one band.
            bool hasStateRun = false;
            int stateRunLeft = 0, stateRunRight = 0;

            try
            {
                // Points outside the view can't pass the viewport test below, so don't visit them -
                // except the first and last, whose extensions to firstX/lastX can still cross it.
                GetDrawRange(viewPort, xLeft, xRight, out int from, out int to);
                if (extendBegin && from > 0)
                    DrawPoint(0);
                for (int i = from; i <= to; i++)
                    DrawPoint(i);
                if (extendEnd && to < pointCnt - 1)
                    DrawPoint(pointCnt - 1);
                FlushStateRun();
            }
            finally
            {
                stateRunBrush?.Dispose();
            }

            void DrawPoint(int i)
            {
                bool last = (i == pointCnt - 1);
                bool first = (i == 0);

                PointD pPrevD = first && extendBegin ? new PointD(firstX, Points[i].Y) : (i > 0 ? Points[i - 1] : PointD.Empty);
                PointD pActD = Points[i];
                PointD pNextD = last && extendEnd ? new PointD(lastX, Points[i].Y) : (i < pointCnt - 1 ? Points[i + 1] : PointD.Empty);

                Point pPrev = convert(pPrevD);
                Point pAct = convert(pActD);
                Point pNext = convert(pNextD);

                // A state bar is drawn at stateY whatever its value - the value is only its text -
                // so the value must not decide whether the bar is within the viewport.
                if (drawStyle == DrawStyles.State)
                    pPrev.Y = pAct.Y = pNext.Y = (int)stateY;

                bool across = pAct.X <= viewPort.X && pNext.X >= (viewPort.X + viewPort.Width);

                // Check if the points are within the viewport or across it
                if (!(viewPort.CheckIfPointIsWithin(pAct) || viewPort.CheckIfPointIsWithin(pPrev) || viewPort.CheckIfPointIsWithin(pNext) || across))
                    return;

                // Show crosses if necessary
                if (showCrosses && viewPort.CheckIfPointIsWithin(convert(pActD)))
                {
                    g.DrawCross(pen, viewPort, pAct, 3);
                }

                // Handle drawing styles
                switch (drawStyle)
                {
                    case Trace.DrawStyles.Points:
                        g.Drawpoint(brush, pAct, 2);
                        break;

                    case Trace.DrawStyles.Cross:
                        g.DrawCross(pen, viewPort, pAct, 5);
                        break;

                    case Trace.DrawStyles.DiscreteSingal:
                        g.Drawpoint(brush, pAct, 4);
                        g.DrawLine(pen, new Point(pAct.X, viewPort.Height / 2), pAct);
                        break;

                    case Trace.DrawStyles.Lines:
                        if (first && extendBegin) g.DrawLine(pen, viewPort, pPrev, pAct, true, false);
                        if (last && extendEnd) g.DrawLine(pen, viewPort, pAct, pNext, false, true);
                        if (!last) g.DrawLine(pen, viewPort, pAct, pNext, false, false);
                        break;

                    case Trace.DrawStyles.NonInterpolatedLine:
                        if (first && extendBegin) g.DrawLine(pen, viewPort, pPrev, pAct, true, false);
                        if (last && extendEnd) g.DrawLine(pen, viewPort, pAct, pNext, false, true);
                        if (!last)
                        {
                            Point pInbetween = new Point(pNext.X, pAct.Y);
                            g.DrawLine(pen, viewPort, pAct, pInbetween, false, false);
                            g.DrawLine(pen, viewPort, pInbetween, pNext, false, false);
                        }
                        break;

                    case Trace.DrawStyles.State:
                        int rectHeight = fontHeight;

                        Rectangle rect = first && extendBegin
                            ? new Rectangle(pPrev.X, (int)stateY - 8, pAct.X - pPrev.X, rectHeight)
                            : last && extendEnd
                                ? new Rectangle(pAct.X, (int)stateY - 8, pNext.X - pAct.X, rectHeight)
                                : new Rectangle(pAct.X, (int)stateY - 8, pNext.X - pAct.X, rectHeight);

                        if (rect.IsEmpty)
                            break;

                        if (rect.Width >= 0 && rect.Width < MinStateSegmentWidth)
                        {
                            // Too narrow to show on its own: grow the band it is part of. Segments
                            // follow on from each other; one that doesn't (points out of order)
                            // starts a band of its own rather than stretching this one across the gap.
                            if (hasStateRun && rect.Left <= stateRunRight + 1 && rect.Right >= stateRunLeft - 1)
                            {
                                stateRunLeft = Math.Min(stateRunLeft, rect.Left);
                                stateRunRight = Math.Max(stateRunRight, rect.Right);
                            }
                            else
                            {
                                FlushStateRun();
                                hasStateRun = true;
                                stateRunLeft = rect.Left;
                                stateRunRight = rect.Right;
                            }
                            break;
                        }

                        FlushStateRun();
                        g.DrawState(pen, viewPort, rect, toHumanReadable(pActD.Y), font, !(first && extendBegin), !(last && extendEnd));
                        break;

                    default:
                        throw new NotImplementedException($"Drawing style '{drawStyle}' not supported.");
                }
            }

            void FlushStateRun()
            {
                if (!hasStateRun)
                    return;
                hasStateRun = false;
                stateRunBrush ??= new HatchBrush(HatchStyle.DiagonalCross, Color, System.Drawing.Color.Transparent);
                g.DrawStateRun(pen, stateRunBrush, viewPort, stateRunLeft, stateRunRight, (int)stateY - 8, fontHeight);
            }
        }



    public enum DrawStyles
        {
            Points,
            Lines,
            NonInterpolatedLine,
            DiscreteSingal,
            State,
            Cross,
        }

        [Flags]
        public enum DrawOptions
        {
            None = 0,
            ShowCrosses =   0x01,
            ExtendBegin =   0x02,
            ExtendEnd =     0x04,
            ShowScale =     0x08,
            DrawNames =     0x10,
        }


    }


}
