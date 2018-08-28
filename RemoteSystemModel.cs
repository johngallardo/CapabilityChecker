using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.RemoteSystems;

namespace CapabilityChecker
{
    public enum CapabilityState
    {
        NotChecked,
        Checking,
        Capable,
        NotCapable
    }

    public class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void SetPropertyValue<T>(string name, ref T old, T n) where T : IComparable
        {
            if (old.CompareTo(n) != 0)
            {
                old = n;
                NotifyPropertyChanged(name);
            }
        }

        protected void SetPropertyReference<T>(string name, ref T old, T n) where T : class
        {
            if(old != n)
            {
                old = n;
                NotifyPropertyChanged(name);
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class RemoteSystemModel : PropertyChangedBase
    {
        public RemoteSystemModel(RemoteSystem system)
        {
            _remoteSystem = system;
        }

        public string Name { get { return _remoteSystem.DisplayName; } }

        public string Kind { get { return _remoteSystem.Kind; } }

        private CapabilityState _isAppServiceCapable = CapabilityState.NotChecked;
        public CapabilityState IsAppServiceCapable
        {
            get => _isAppServiceCapable;
            set => SetPropertyValue(nameof(IsAppServiceCapable), ref _isAppServiceCapable, value);
        }

        public async Task CheckAppServiceCapableAsync()
        {
            IsAppServiceCapable = CapabilityState.Checking;
            var operation = _remoteSystem.GetCapabilitySupportedAsync(KnownRemoteSystemCapabilities.AppService);
            IsAppServiceCapable = await operation ? CapabilityState.Capable : CapabilityState.NotCapable;
        }

        public async Task LaunchUri(string uri)
        {
            RemoteSystemConnectionRequest request = new RemoteSystemConnectionRequest(_remoteSystem);
            await Windows.System.RemoteLauncher.LaunchUriAsync(request, new Uri(uri));
        }

        private RemoteSystem _remoteSystem;
    }
}
