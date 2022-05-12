
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DispoBaseLib

{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propName);
                PropertyChanged(this, args);
            }
        }
    }
}
