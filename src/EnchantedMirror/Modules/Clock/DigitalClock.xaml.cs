using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using EnchantedMirror.Extensions;
using System.Collections.Generic;

namespace EnchantedMirror.Modules.Clock
{
    public sealed partial class DigitalClock : UserControl, IModule, INotifyPropertyChanged
    {
        private DispatcherTimer _clockTimer = new DispatcherTimer();
        private string _timeFormat;
        private Dictionary<string, string> _timeFormats = new Dictionary<string, string>
        {
            {"24hr", "HH:mm" },
            {"12hr", "h:mm tt" }
        };
        private string _theTime;

        public DigitalClock()
        {
            this.InitializeComponent();
            App.ConfigurationLoaded += App_ConfigurationLoaded;

            LoadConfig();

            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += Timer_Tick;
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
                string vert = Convert.ToString(config.position.verticalAlignment);
                string hori = Convert.ToString(config.position.horizontalAlignment);
                VerticalAlignment = vert.ToVerticalAlignment();
                HorizontalAlignment = hori.ToHorizontalAlignment();

                Thickness margin = Margin;
                margin.Left = (double)config.position.margin.left;
                margin.Top = (double)config.position.margin.top;
                margin.Right = (double)config.position.margin.right;
                margin.Bottom = (double)config.position.margin.bottom;
                Margin = margin;

                var format = (string)config.attributes.format;
                if (_timeFormats.ContainsKey(format))
                {
                    _timeFormat = _timeFormats[format];
                }
                else if (format.ToUpperInvariant() == "CUSTOM")
                {
                    _timeFormat = (string)config.attributes.format.custom;
                }
                else
                { 
                    _timeFormat = "HH:mm";
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

        private void Timer_Tick(object sender, object e)
        {
            _clockTimer.Stop();

            DateTime d = DateTime.Now;

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
