using WinYoutubeDownloader.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace WinYoutubeDownloader.Views.Pages
{
    public partial class DashboardPage : INavigableView<YoutubeDownloadViewModel>
    {
        public YoutubeDownloadViewModel ViewModel { get; }

        public DashboardPage(YoutubeDownloadViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
