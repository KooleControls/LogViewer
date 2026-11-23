using FormsLib.Scope.Controls;
using System;
using System.Drawing;
using FormsLib.Extentions;
using FormsLib.Maths;

namespace FormsLib.Scope
{
    public abstract class Label
    {
        public virtual bool Visible { get; set; } = true;
        public virtual double Scale { get; set; } = 1;
        public virtual double Offset { get; set; } = 0;
        public virtual Color Color { get; set; } = Color.Green;
        public virtual PointD Point { get; set; }
        public string Text { get; set; }
        public int MaxLines { get; set; } = 1;

        // Text cut down to MaxLines, rebuilt only when Text or MaxLines changes rather than every frame.
        private string? displaySource;
        private int displayMaxLines;
        private string displayText = "";
        private int displayLineCount = 1;

        public void Draw(Graphics g, Style style, Rectangle viewPort, Func<PointD, Point> convert)
        {
            using Pen pen = new Pen(Color);
            Draw(g, style, viewPort, convert, pen, new LabelRow());
        }

        /// <summary>
        /// Draws the label, but leaves out what would only overprint what the labels before it in the
        /// same <paramref name="row"/> drew: the cross when it is on the previous one's spot, the
        /// text when it would land on text already there. Zoomed out, thousands of labels share a
        /// few hundred pixels; their text piled up into an unreadable smear, and drawing it was most
        /// of the frame time. Ctrl-hover still lists the text of every label under the cursor.
        /// </summary>
        internal void Draw(Graphics g, Style style, Rectangle viewPort, Func<PointD, Point> convert, Pen pen, LabelRow row)
        {
            if (!Visible)
                return;

            Point pt = convert(Point);
            if (!viewPort.CheckIfPointIsWithin(pt))
                return;

            if (pt != row.LastCross)
            {
                g.DrawCross(pen, viewPort, pt, 5);
                row.LastCross = pt;
            }

            UpdateDisplayText();
            Font font = style.Font;
            int textHeight = GraphicsExt.GetFontHeight(font) * displayLineCount;
            int textTop = pt.Y - textHeight;

            // The text plus a little room after it, so neighbouring texts don't run into one word.
            Rectangle textArea = new Rectangle(pt.X + 8, textTop, GraphicsExt.GetTextWidth(displayText, font) + LabelRow.TextSpacing, textHeight);
            if (row.Overlaps(textArea))
                return;

            g.DrawFitTextToRectangle(pen, viewPort, new Rectangle(pt.X + 8, textTop, viewPort.Width - 8, textHeight), displayText, font);
            row.AddText(textArea);
        }

        private void UpdateDisplayText()
        {
            string text = Text ?? "";
            if (ReferenceEquals(text, displaySource) && MaxLines == displayMaxLines)
                return;

            string[] lines = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            displayText = string.Join(Environment.NewLine, lines.Take(MaxLines));
            displayLineCount = displayText.Count(c => c == '\n') + (displayText.EndsWith('\n') ? 0 : 1);
            displaySource = text;
            displayMaxLines = MaxLines;
        }
    }

    /// <summary>What the labels of one row (trace) drew so far, so the next can skip what would overprint it.</summary>
    internal sealed class LabelRow
    {
        public const int TextSpacing = 4;

        public Point LastCross = new Point(int.MinValue, int.MinValue);

        /// <summary>
        /// Every text drawn in this row, filed under each 64 px wide column it covers, so a new text
        /// is only checked against the few near it. All of them, not just the last: labels on a line
        /// trace sit at their value and can alternate between heights, and labels don't always come
        /// in time order.
        /// </summary>
        private readonly Dictionary<int, List<Rectangle>> textsByColumn = new Dictionary<int, List<Rectangle>>();
        private const int ColumnShift = 6;

        public bool Overlaps(Rectangle textArea)
        {
            for (int column = textArea.Left >> ColumnShift; column <= textArea.Right >> ColumnShift; column++)
            {
                if (textsByColumn.TryGetValue(column, out List<Rectangle>? texts))
                {
                    foreach (Rectangle text in texts)
                    {
                        if (text.IntersectsWith(textArea))
                            return true;
                    }
                }
            }
            return false;
        }

        public void AddText(Rectangle textArea)
        {
            for (int column = textArea.Left >> ColumnShift; column <= textArea.Right >> ColumnShift; column++)
            {
                if (!textsByColumn.TryGetValue(column, out List<Rectangle>? texts))
                    textsByColumn[column] = texts = new List<Rectangle>();
                texts.Add(textArea);
            }
        }
    }


    public class FreeLabel : Label
    {
        public FreeLabel(double x, double y) { Point = new PointD(x, y); }
    }


    public class LinkedLabel : Label
    {
        public Trace Trace { get; set; }
        public LinkedLabel(Trace trace) { Trace = trace; }
        public LinkedLabel(Trace trace, double x) { Trace = trace; Point = new PointD(x, trace.GetYValue(x)); }
        public LinkedLabel(Trace trace, double x, double y) { Trace = trace; Point = new PointD(x, y); }

        public override Color Color { get { return Trace.Color; } }
        public override double Scale { get => Trace.Scale; }
        public override double Offset { get => Trace.Offset; }
        public override bool Visible { get => Trace.Visible; }


    }


}
