using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ValueType = System.Single;

namespace StorylineEditor.App.Controls
{
    /// <summary>
    /// Логика взаимодействия для NumericUpDown_Float.xaml
    /// </summary>
    public partial class NumericUpDown_Float : UserControl
    {
        private static readonly DependencyProperty ValueProperty = DependencyProperty.Register
            (
            "Value",
            typeof(ValueType),
            typeof(NumericUpDown_Float),
            new FrameworkPropertyMetadata(default(ValueType), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValuePropertyChanged)
            );

        private static void OnValuePropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (dp is NumericUpDown_Float NumericUpDown_Float && !NumericUpDown_Float.tb_Value_Source)
            {
                NumericUpDown_Float.tb_Value.Text = e.NewValue?.ToString();
            }
        }

        public ValueType Value
        {
            get => (ValueType)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private static readonly DependencyProperty MinValueProperty = DependencyProperty.Register
            (
            "MinValue",
            typeof(ValueType),
            typeof(NumericUpDown_Float),
            new FrameworkPropertyMetadata(default(ValueType), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnMinValuePropertyChanged)
            );

        private static void OnMinValuePropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (dp is NumericUpDown_Float NumericUpDown_Float)
            {
                NumericUpDown_Float.Value = Math.Max((ValueType)e.NewValue, NumericUpDown_Float.Value);
            }
        }

        public ValueType MinValue
        {
            get => (ValueType)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        private static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register
            (
            "MaxValue",
            typeof(ValueType),
            typeof(NumericUpDown_Float),
            new FrameworkPropertyMetadata(default(ValueType), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnMaxValuePropertyChanged)
            );

        private static void OnMaxValuePropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (dp is NumericUpDown_Float NumericUpDown_Float)
            {
                NumericUpDown_Float.Value = Math.Min((ValueType)e.NewValue, NumericUpDown_Float.Value);
            }
        }

        public ValueType MaxValue
        {
            get => (ValueType)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        private static readonly DependencyProperty StepProperty = DependencyProperty.Register
            (
            "Step",
            typeof(ValueType),
            typeof(NumericUpDown_Float),
            new FrameworkPropertyMetadata(default(ValueType), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );

        public ValueType Step
        {
            get => (ValueType)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        public Style ButtonIncStyle
        {
            get => btn_Inc.Style;
            set => btn_Inc.Style = value;
        }

        public Style ButtonDecStyle
        {
            get => btn_Dec.Style;
            set => btn_Dec.Style = value;
        }

        public object ButtonIncContent
        {
            get => btn_Inc.Content;
            set => btn_Inc.Content = value;
        }

        public object ButtonDecContent
        {
            get => btn_Dec.Content;
            set => btn_Dec.Content = value;
        }

        public NumericUpDown_Float()
        {
            InitializeComponent();

            tb_Value.Text = Value.ToString();
        }

        protected int StepDirection = 0;

        private async void StartStepRoutine()
        {
            while ((btn_Inc.IsMouseOver || btn_Dec.IsMouseOver) && (Mouse.LeftButton == MouseButtonState.Pressed) && StepDirection != 0)
            {
                var value = Math.Min(Math.Max(Value + StepDirection * Step, MinValue), MaxValue);
                Value = value;
                await System.Threading.Tasks.Task.Delay(100);
            }
        }

        protected bool tb_Value_Source = false;
        protected Regex regex = new Regex("[^0-9.]+");
        private void tb_Value_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = regex.IsMatch(e.Text);

            ValueType value;

            if (ValueType.TryParse(e.Text, out value))
            {
                tb_Value_Source = true;
                Value = Math.Min(Math.Max(value, MinValue), MaxValue);
                tb_Value_Source = false;
            }
        }

        private void btn_Inc_MouseDown(object sender, MouseButtonEventArgs e) { StepDirection = 1; StartStepRoutine(); }

        private void btn_Dec_MouseDown(object sender, MouseButtonEventArgs e) { StepDirection = -1; StartStepRoutine(); }
    }
}