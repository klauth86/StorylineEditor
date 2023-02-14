/*
StorylineEditor
Copyright(C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ValueType = System.Int32;

namespace StorylineEditor.App.Controls
{
    /// <summary>
    /// Логика взаимодействия для NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        private static readonly DependencyProperty ValueProperty = DependencyProperty.Register
            (
            "Value",
            typeof(ValueType),
            typeof(NumericUpDown),
            new FrameworkPropertyMetadata(default(ValueType), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValuePropertyChanged)
            );

        private static void OnValuePropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (dp is NumericUpDown numericUpDown && !numericUpDown.tb_Value_Source)
            {
                numericUpDown.tb_Value.Text = e.NewValue?.ToString();
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
            typeof(NumericUpDown),
            new FrameworkPropertyMetadata(default(ValueType), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnMinValuePropertyChanged)
            );

        private static void OnMinValuePropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (dp is NumericUpDown numericUpDown)
            {
                numericUpDown.Value = Math.Max((ValueType)e.NewValue, numericUpDown.Value);
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
            typeof(NumericUpDown),
            new FrameworkPropertyMetadata(default(ValueType), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnMaxValuePropertyChanged)
            );

        private static void OnMaxValuePropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (dp is NumericUpDown numericUpDown)
            {
                numericUpDown.Value = Math.Min((ValueType)e.NewValue, numericUpDown.Value);
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
            typeof(NumericUpDown),
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

        public NumericUpDown()
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