using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AnkaPTT.UI
{
    class IntTextBox : TextBox
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(int), typeof(IntTextBox), new PropertyMetadata(0));
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(
            "MinValue", typeof(int), typeof(IntTextBox), new PropertyMetadata(0));
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
            "MaxValue", typeof(int), typeof(IntTextBox), new PropertyMetadata(100));

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set
            {
                if (value < MinValue) return;
                if (value > MaxValue) return;
                SetValue(ValueProperty, value);
            }
        }

        public int MinValue
        {
            get { return (int)GetValue(MinValueProperty); }
            set
            {
                SetValue(MinValueProperty, value);
            }
        }

        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set
            {
                SetValue(MaxValueProperty, value);
            }
        }

        private void OnValueChanged()
        {
            updateText();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ValueChanged;

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == ValueProperty)
                OnValueChanged();
        }

        public IntTextBox()
        {
            
            updateText();
        }

        private void updateText()
        {
            Text = Value.ToString();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                Value = Value + 1;
            }
            if (e.Key == Key.Down)
            {
                Value = Value - 1;
            }
            
            base.OnPreviewKeyDown(e);
        }


        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            updateText();
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            int newValue;
            if (int.TryParse(Text, out newValue))
            {
                Value = newValue;
            }
        }
    }
}
