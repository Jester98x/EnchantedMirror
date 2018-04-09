using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace EnchantedMirror.Modules
{
    public sealed partial class RadioPlayer : UserControl
    {
        private Dictionary<string, Uri> _radioStations;

        public RadioPlayer()
        {
            this.InitializeComponent();

            App.ConfigurationLoaded += App_ConfigurationLoaded;

            LoadConfig();
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
                .FirstOrDefault(c => c.module == "RadioPlayer");

            if (config != null)
            {
                LoadFeeds(config);
                SetDefaultFeed(config);
            }
        }

        private void LoadFeeds(dynamic config)
        {
            var stations = new List<dynamic>(config.attributes.stations);
            _radioStations = new Dictionary<string, Uri>(stations.Count);
            foreach (dynamic station in stations)
            {
                _radioStations.Add((string)station.name, new Uri((string)station.uri));
            }
        }

        private void SetDefaultFeed(dynamic config)
        {
            var defaultStation = (string)config.attributes.defaultStation;

            if (_radioStations.ContainsKey(defaultStation))
            {
                player.Source = _radioStations[defaultStation];
            }
            else
            {
                player.Source = _radioStations.FirstOrDefault().Value;
            }
        }
    }
}
