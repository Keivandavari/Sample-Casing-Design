using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace CasingDesign.Plots_And_Graphs
{
    [TemplatePart(Name = "PART_Content", Type = typeof(Grid))]
    public class AvailableWellConfigControl : Control
    {
        public static readonly DependencyProperty SizesProperty = DependencyProperty.Register("Sizes", typeof(List<string>), typeof(AvailableWellConfigControl), new PropertyMetadata(new List<string>(), (d, e) => ((AvailableWellConfigControl)d).UpdateData()));
        public static readonly DependencyProperty HoleCircleBrushProperty = DependencyProperty.Register("HoleCircleBrush", typeof(Brush), typeof(AvailableWellConfigControl), new PropertyMetadata(null, (d, e) => ((AvailableWellConfigControl)d).UpdateData()));
        public static readonly DependencyProperty CasingCircleBrushProperty = DependencyProperty.Register("CasingCircleBrush", typeof(Brush), typeof(AvailableWellConfigControl), new PropertyMetadata(null, (d, e) => ((AvailableWellConfigControl)d).UpdateData()));
        public static readonly DependencyProperty LineBrushHoleToCasingProperty = DependencyProperty.Register("LineBrushHoleToCasing", typeof(Brush), typeof(AvailableWellConfigControl), new PropertyMetadata(null, (d, e) => ((AvailableWellConfigControl)d).UpdateData()));
        public static readonly DependencyProperty LineBrushCasingToHoleProperty = DependencyProperty.Register("LineBrushCasingToHole", typeof(Brush), typeof(AvailableWellConfigControl), new PropertyMetadata(null, (d, e) => ((AvailableWellConfigControl)d).UpdateData()));

        public Brush HoleCircleBrush
        {
            get { return (Brush)GetValue(HoleCircleBrushProperty); }
            set { SetValue(HoleCircleBrushProperty, value); }
        }
        public Brush CasingCircleBrush
        {
            get { return (Brush)GetValue(CasingCircleBrushProperty); }
            set { SetValue(CasingCircleBrushProperty, value); }
        }
        public Brush LineBrushHoleToCasing
        {
            get { return (Brush)GetValue(LineBrushHoleToCasingProperty); }
            set { SetValue(LineBrushHoleToCasingProperty, value); }
        }
        public Brush LineBrushCasingToHole
        {
            get { return (Brush)GetValue(LineBrushCasingToHoleProperty); }
            set { SetValue(LineBrushCasingToHoleProperty, value); }
        }
        public List<string> Sizes
        {
            get { return (List<string>)GetValue(SizesProperty); }
            set { SetValue(SizesProperty, value); }
        }
        Grid ContentGrid { get; set; }
        protected internal Path PathCore { get; set; }
        protected internal double CircleRadius { get; private set; }
        protected internal double MaxAvailableHeight { get; private set; }
        protected internal double MaxAvailableWidth { get; private set; }
        protected internal double TextFontSize { get; private set; }
        protected internal Point StartPointOfLine { get; private set; }
        public AvailableWellConfigControl()
        {
            DefaultStyleKey = typeof(AvailableWellConfigControl);   
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            bool allowUpdateData = MaxAvailableHeight == 0;
            MaxAvailableHeight = availableSize.Height;
            MaxAvailableWidth = availableSize.Width;
            if(allowUpdateData)
               UpdateData();
            return base.MeasureOverride(availableSize);
        } 
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ContentGrid = (Grid)GetTemplateChild("PART_Content");
            if (ContentGrid != null)
                UpdateData();
        }
        private void OnSizesChanged()
        {
            UpdateData();
        }
        private void UpdateData()
        {
            if (ContentGrid == null)
                return;
            CircleRadius =  GetCirclesRadius();
            StartPointOfLine =  GetStartPointLine();
            TextFontSize = GetFontSize();
            ContentGrid.Children.Clear();
            for (int i = 0; i <= Sizes.Count - 1; i++)
            {
                CreateEllipse(i + 1);
                CreateLine(i + 1);
                CreateSizeBlock(Sizes[i], i + 1);
            }
        }
        protected internal Point GetStartPointLine(){
            return new Point(MaxAvailableWidth / 2 - 1.5*CircleRadius, 0); }
        protected internal double GetCirclesRadius() {
            return MaxAvailableWidth / 20; }
        protected internal double GetFontSize(){
            return .6 * CircleRadius; }
        protected internal Brush GetStrokeOfCircles(int i)
        {
            if (i % 2 == 0)
                return HoleCircleBrush;
            else
                return CasingCircleBrush;
        }
        protected internal Thickness GetTextBoxMargin(TextBlock size, int i)
        {
            double left, top;
            Thickness textBlockMargin;
            if (i == 1)
            {
                left = StartPointOfLine.X - (double)size.GetValue(WidthProperty) / 2;
                top = CircleRadius - 5 * (double)size.GetValue(HeightProperty) / 16;
            }
            else
            {
                left = i % 2 == 0 ? StartPointOfLine.X + 3 * CircleRadius - (double)size.GetValue(WidthProperty) / 2 : StartPointOfLine.X - (double)size.GetValue(WidthProperty) / 2;
                top = 3 * (i - 1) * CircleRadius + CircleRadius - 5 * (double)size.GetValue(HeightProperty) / 16;
            }
            textBlockMargin = new Thickness(left, top, 0, 0);
            return textBlockMargin;
        }
        protected internal Brush GetBrushOfLines(int i)
        {
            if (i % 2 == 0)
                return LineBrushCasingToHole;
            else
                return LineBrushHoleToCasing;
        }
        private void CreateLine(int i)
        {
            if (i == Sizes.Count)
                return;
            Line lineBetweenCircles = new Line
            {
                Stroke = GetBrushOfLines(i),
                StrokeThickness = 1,
                X1 = i % 2 == 0 ? StartPointOfLine.X + 2*CircleRadius : StartPointOfLine.X + CircleRadius,
                Y1 = (3 * i - 1) * CircleRadius,
                X2 = i % 2 == 0 ? StartPointOfLine.X + CircleRadius : StartPointOfLine.X + 2*CircleRadius,
                Y2 = 3 * i * CircleRadius
            };
            if (ContentGrid == null)
                return;
            ContentGrid.Children.Add(lineBetweenCircles);
        }
        private void CreateEllipse(int i)
        {
            Ellipse sizeCircles;
            double left = 0, top = 0;
            if (i == 1)
            {
                left = StartPointOfLine.X - CircleRadius;
                top = 0;
            }
            else
            {
                left = i % 2 == 0 ? StartPointOfLine.X + 2*CircleRadius : StartPointOfLine.X - CircleRadius;
                top = 3 * (i - 1) * CircleRadius;
            }
                sizeCircles = new Ellipse
                {
                    VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top,
                    HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left,
                    Width = 2*CircleRadius,
                    Height = 2*CircleRadius,
                    Margin = new Thickness(left, top, 0, 0),
                    Stroke = GetStrokeOfCircles(i),
                    StrokeThickness = 2
                };
            ContentGrid.Children.Add(sizeCircles);   
        }
        private void CreateSizeBlock(string p, int i)
        {
            TextBlock size;
            size = new TextBlock
            {
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top,
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left,
                Text = p,
                TextAlignment = Windows.UI.Xaml.TextAlignment.Center,
                FontSize = TextFontSize == 0 ? 18 : TextFontSize,
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = Windows.UI.Text.FontWeights.Light,
                Width = (3 ^ (1 / 2)) * CircleRadius,
                Height = CircleRadius,
            };
            ContentGrid.Children.Add(size);
            size.Margin = GetTextBoxMargin(size, i);
        }

    }
}
