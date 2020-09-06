using System;
using System.ComponentModel;

namespace Ingvilt.Models {
    public class BaseObservableProperty : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string p) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
        }

        // TODO: test this
        /*public void ClearPropertyChangedEventListeners() {
            foreach (Delegate d in PropertyChanged.GetInvocationList()) {
                PropertyChanged -= (PropertyChangedEventHandler)d;
            }
        }*/
    }
}
