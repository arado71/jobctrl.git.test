using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionTester
{
    public class ConnectionData : INotifyPropertyChanged
    {
        public string EndpointName { get; set; }

        private DateTime _lastActive;
        public DateTime LastActive {
            get { return _lastActive; }
            set { _lastActive = value; NotifyPropertyChanged(); } 
        }

        private long _ping;
        public long Ping {
            get { return _ping; }
            set { _ping = value; NotifyPropertyChanged(); }
        }

        private string _lastException;

        public string LastException
        {
            get { return _lastException; }
            set { _lastException = value; NotifyPropertyChanged(); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
         private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
