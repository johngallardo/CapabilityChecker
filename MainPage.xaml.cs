using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.System.RemoteSystems;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CapabilityChecker
{
    public class PageModel : PropertyChangedBase
    {
        public PageModel()
        {
            RemoteSystemModels = new ObservableCollection<RemoteSystemModel>();
        }
        public ObservableCollection<RemoteSystemModel> RemoteSystemModels { get; private set; }

        private RemoteSystemModel _thisRemoteSystem;

        public RemoteSystemModel ThisRemoteSystem
        {
            get => _thisRemoteSystem;
            set => SetPropertyReference(nameof(ThisRemoteSystem), ref _thisRemoteSystem, value);
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private PageModel _model = new PageModel();

        public MainPage()
        {
            this.DataContext = _model;
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_watcher == null)
            {
                _watcher = await CreateWatcherAsync();
            }
        }

        private async Task<RemoteSystemWatcher> CreateWatcherAsync()
        {
            var access = await RemoteSystem.RequestAccessAsync();
            if (access == RemoteSystemAccessStatus.Allowed)
            {
                var watcher = RemoteSystem.CreateWatcher();
                watcher.RemoteSystemAdded += Watcher_RemoteSystemAdded;
                watcher.Start();
                return watcher;
            }
            return null;
        }

        private async void Watcher_RemoteSystemAdded(RemoteSystemWatcher sender, RemoteSystemAddedEventArgs args)
        {
            await Dispatcher.RunIdleAsync((_) =>
            {
                AddRemoteSystem(args.RemoteSystem);
            });
        }

        void AddRemoteSystem(RemoteSystem system)
        {
            _model.RemoteSystemModels.Add(new RemoteSystemModel(system));
        }

        RemoteSystemWatcher _watcher;

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            var model = element?.DataContext as RemoteSystemModel;
            await model?.CheckAppServiceCapableAsync();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            var model = element?.DataContext as RemoteSystemModel;
            await model?.LaunchUri("jgtest://www.microsoft.com");
        }

        private async void LoadThisClicked(object sender, RoutedEventArgs e)
        {
            var access = await RemoteSystem.RequestAccessAsync();
            if (access == RemoteSystemAccessStatus.Allowed)
            {
                var hn = new HostName("127.0.0.1");
                var rs = await RemoteSystem.FindByHostNameAsync(hn);
                _model.ThisRemoteSystem = new RemoteSystemModel(rs);
            }
        }
    }
}
