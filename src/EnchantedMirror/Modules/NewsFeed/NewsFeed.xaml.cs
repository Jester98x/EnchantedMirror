using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EnchantedMirror.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Syndication;

namespace EnchantedMirror.Modules
{
    public sealed partial class NewsFeed : UserControl, INotifyPropertyChanged
    {
        private string _newsAge;
        private List<SyndicationItem> _newsFeedItems = new List<SyndicationItem>();
        private DispatcherTimer _newsFeedTimer = new DispatcherTimer();
        private string _newsHeadline;
        private string _newsSource;
        private int _currentNewsItem = 0;
        private Dictionary<string, Uri> _newsFeeds;
        private string _newsFeedTitle;
        private Uri _newsFeed;

        public NewsFeed()
        {
            InitializeComponent();
            App.ConfigurationLoaded += App_ConfigurationLoaded;

            LoadConfig();

            fadeNewsOut.Completed += FadeNewsOut_Completed;
            fadeNewsIn.Completed += FadeNewsIn_Completed;

            _newsFeedTimer.Interval = TimeSpan.FromSeconds(1);
            _newsFeedTimer.Tick += News_Tick;

            NewsHeadline = "loading news feed ...";
            NewsAge = "... please wait";

            _newsFeedTimer.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
                .FirstOrDefault(c => c.module == "NewsFeed");

            if (config != null)
            {
                SetAlignments(config);
                LoadFeeds(config);
                SetDefaultFeed(config);
            }

            if (_newsFeedTitle.Length == 0)
            {
                Visibility = Visibility.Collapsed;
            }
        }

        private void SetAlignments(dynamic config)
        {
            string vert = Convert.ToString(config.position.verticalAlignment);
            string hori = Convert.ToString(config.position.horizontalAlignment);
            VerticalAlignment = vert.ToVerticalAlignment();
            HorizontalAlignment = hori.ToHorizontalAlignment();
        }

        private void LoadFeeds(dynamic config)
        {
            var feeds = new List<dynamic>(config.attributes.feeds);
            _newsFeeds = new Dictionary<string, Uri>(feeds.Count);
            foreach (var feed in feeds)
            {
                _newsFeeds.Add((string)feed.name, new Uri((string)feed.uri));
            }
        }

        private void SetDefaultFeed(dynamic config)
        {
            var defaultFeed = (string)config.attributes.defaultFeed;

            if (_newsFeeds.ContainsKey(defaultFeed))
            {
                _newsFeedTitle = defaultFeed;
                _newsFeed = _newsFeeds[defaultFeed];
            }
            else
            {
                _newsFeedTitle = _newsFeeds.FirstOrDefault().Key;
                _newsFeed = _newsFeeds.FirstOrDefault().Value;
            }
        }

        public string NewsSource
        {
            get => _newsSource;
            set
            {
                if (value != _newsSource)
                {
                    _newsSource = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string NewsHeadline
        {
            get => _newsHeadline;
            set
            {
                if (value != _newsHeadline)
                {
                    _newsHeadline = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string NewsAge
        {
            get => _newsAge;
            set
            {
                if (value != _newsAge)
                {
                    _newsAge = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async void News_Tick(object sender, object e)
        {
            _newsFeedTimer.Stop();
            if (_newsFeedItems.Count == 0 || _currentNewsItem + 1 > _newsFeedItems.Count - 1)
            {
                var client = new SyndicationClient();
                SyndicationFeed feed = await client.RetrieveFeedAsync(_newsFeed);
                foreach (SyndicationItem item in feed.Items)
                {
                    _newsFeedItems.Add(item);
                }

                _currentNewsItem = 0;
                NewsSource = _newsFeedTitle;
            }

            if (_newsFeedItems.Count > 0)
            {
                fadeNewsOut.Begin();
            }
        }

        private void FadeNewsOut_Completed(object sender, object e)
        {
            NewsHeadline = _newsFeedItems[_currentNewsItem].Title.Text;
            var d = DateTime.Now.Ticks;
            var nD = _newsFeedItems[_currentNewsItem].PublishedDate.Ticks;
            var tickDiff = TimeSpan.FromTicks(d - nD);
            var minDiff = Math.Floor(tickDiff.TotalMinutes);
            var hourDiff = Math.Floor(tickDiff.TotalHours);
            var dayDiff = Math.Floor(tickDiff.TotalDays);

            NewsAge = dayDiff > 0 ? $"{dayDiff} day{(dayDiff == 1 ? string.Empty : "s")} ago"
                : hourDiff > 0 ? $"{hourDiff} hour{(hourDiff == 1 ? string.Empty : "s")} ago"
                : minDiff > 0 ? $"{minDiff} minute{(minDiff == 1 ? string.Empty : "s")} ago"
                : "less than a minute ago";

            _currentNewsItem++;
            if (_currentNewsItem > _newsFeedItems.Count - 1)
            {
                _newsFeedItems.Clear();
            }

            fadeNewsIn.Begin();
            fadeNewsIn.Completed += FadeNewsIn_Completed;
        }

        private void FadeNewsIn_Completed(object sender, object e)
        {
            _newsFeedTimer.Interval = TimeSpan.FromSeconds(17);
            _newsFeedTimer.Start();
        }
    }
}
