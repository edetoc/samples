using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfBasicMVVM.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        /// 
        private bool isGuiEnabled = true;
        
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
            //IsGuiEnabled = false;

        }

        public bool IsGuiEnabled
        {
            get
            {
                return isGuiEnabled;
            }
            set
            {
                isGuiEnabled = value;
                RaisePropertyChanged("IsGuiEnabled");
            }
        }


        public ICommand DoWorkCommand
        {
            get
            {
                return new RelayCommand(
                    ()=>this.DoSomeWork());
            }
        }


        private async void DoSomeWork()
        {
            IsGuiEnabled = false;
            try
            {
                await Task.Factory.StartNew(() => Thread.Sleep(3000));
            }
            finally
            {
                IsGuiEnabled = true;
            }
        }
    }
}