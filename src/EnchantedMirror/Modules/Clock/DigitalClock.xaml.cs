using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using EnchantedMirror.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace EnchantedMirror.Modules
{
    public sealed partial class DigitalClock : UserControl, IModule, INotifyPropertyChanged
    {
        private DispatcherTimer _clockTimer = new DispatcherTimer();
        private string _dateFormat;
        private string _dateCulture;
        private string _timeFormat;
        private Dictionary<string, string> _timeFormats = new Dictionary<string, string>
        {
            {"24hr", "HH:mm" },
            {"12hr", "h:mm tt" }
        };
        private string _theTime;
        private string _theDate;

        public DigitalClock()
        {
            this.InitializeComponent();
            App.ConfigurationLoaded += App_ConfigurationLoaded;

            LoadConfig();

            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += ClockTimer_Tick;
            _clockTimer.Start();
        }

        private void App_ConfigurationLoaded(object sender, EventArgs e)
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            if (App.Configurations == null)
            {
                return;
            }

            dynamic config = App.Configurations
                .FirstOrDefault(c => c.module == "DigitalClock");

            if (config != null)
            {
                SetAlignments(config);
                SetMargin(config);
                SetTimeFormat(config);
                SetDateSettings(config);
            }
        }

        private void SetDateSettings(dynamic config)
        {
            _dateFormat = (string)config.attributes.dateFormat;
            _dateCulture = (string)config.attributes.dateCulture;
        }

        private void SetAlignments(dynamic config)
        {
            string vert = Convert.ToString(config.position.verticalAlignment);
            string hori = Convert.ToString(config.position.horizontalAlignment);
            VerticalAlignment = vert.ToVerticalAlignment();
            HorizontalAlignment = hori.ToHorizontalAlignment();
        }

        private void SetTimeFormat(dynamic config)
        {
            var format = (string)config.attributes.timeFormat;
            if (_timeFormats.ContainsKey(format))
            {
                _timeFormat = _timeFormats[format];
            }
            else if (format.ToUpperInvariant() == "CUSTOM")
            {
                _timeFormat = (string)config.attributes.timeFormat.custom;
            }
            else
            {
                _timeFormat = "HH:mm";
            }
        }

        private void SetMargin(dynamic config)
        {
            Thickness margin = Margin;
            margin.Left = (double)config.position.margin.left;
            margin.Top = (double)config.position.margin.top;
            margin.Right = (double)config.position.margin.right;
            margin.Bottom = (double)config.position.margin.bottom;
            Margin = margin;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string TheDate
        {
            get => _theDate;
            set
            {
                if (value != _theDate)
                {
                    _theDate = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string TheTime
        {
            get => _theTime;
            set
            {
                if (value != _theTime)
                {
                    _theTime = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ClockTimer_Tick(object sender, object e)
        {
            _clockTimer.Stop();

            DateTime d = DateTime.Now;
            TheDate = d.ToString(_dateFormat, CultureInfo.CreateSpecificCulture(_dateCulture));
            TheTime = d.ToString(_timeFormat, CultureInfo.InvariantCulture);
            if (time.ActualWidth > 100)
            {
                seconds.Margin = new Thickness(time.ActualWidth + 4, -2, 0, 0);
                seconds.Text = d.Second.ToString("D2", CultureInfo.InvariantCulture);
            }
            else
            {
                seconds.Text = string.Empty;
            }

            _clockTimer.Start();
        }
    }
}
