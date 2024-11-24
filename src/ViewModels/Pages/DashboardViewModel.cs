using eXtensionSharp;
using NAudio.Lame;
using NAudio.Wave;
using YoutubeExplode;
using YoutubeExplode.Converter;

namespace WinYoutubeDownloader.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty] private string _downloadUrl;
        [ObservableProperty] private string _downloadTitle;
        [ObservableProperty] private string _downloadDescription;
        [ObservableProperty] private string _thumbnailUrl;

        public DashboardViewModel()
        {
            DownloadUrl = "https://www.youtube.com/watch?v=ekr2nIex040";
        }

        [RelayCommand]
        private async void OnPreview()
        {
            if(DownloadUrl.xIsEmpty()) return;
            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(this.DownloadUrl);

            DownloadTitle = video.Title;
            DownloadDescription = video.Description;
            
            //var duration = video.Duration; // 00:07:20
            if (video.Thumbnails.xIsNotEmpty())
            {
                var item = video.Thumbnails.FirstOrDefault(m => m.Resolution.Width == video.Thumbnails.Max(m2 => m2.Resolution.Width));

                if (item.xIsNotEmpty())
                {
                    ThumbnailUrl = item.Url;
                }
            }
        }

        [RelayCommand]
        private async void OnDownload()
        {
            if (DownloadUrl.xIsEmpty()) return;

            var youtube = new YoutubeClient();

            var mp4file = "d:\\video.mp4";

            await youtube.Videos.DownloadAsync(DownloadUrl, mp4file);

            var mp3file = "d:\\temp.mp3";
            await using var reader = new AudioFileReader(mp4file);
            await using var writer = new LameMP3FileWriter(mp3file, reader.WaveFormat, LAMEPreset.VBR_90);
            await reader.CopyToAsync(writer);
        }
    }
}
