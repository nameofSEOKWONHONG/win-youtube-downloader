using System.IO;
using System.Net.Http;
using eXtensionSharp;
using NAudio.Lame;
using NAudio.Wave;
using TagLib;
using WinYoutubeDownloader.ViewModels.Windows;
using WinYoutubeDownloader.Views.Windows;
using YoutubeExplode;
using YoutubeExplode.Converter;

namespace WinYoutubeDownloader.ViewModels.Pages
{
    public partial class YoutubeDownloadViewModel : ObservableObject
    {
        [ObservableProperty] private string _downloadUrl;
        [ObservableProperty] private string _downloadTitle;
        [ObservableProperty] private string _downloadDescription;
        [ObservableProperty] private string _thumbnailUrl;
        [ObservableProperty] private Visibility _ringVisibility;

        private byte[] _thumbnailBytes;
        private Queue<ValueTuple<string, string>> _downloadQueue = new();
        private Task _task;

        public YoutubeDownloadViewModel()
        {
            DownloadUrl = "https://www.youtube.com/watch?v=ekr2nIex040";
            RingVisibility = Visibility.Hidden;
            //_task = Task.Run(async () => await WorkProcess());            
        }

        [RelayCommand]
        private async void OnPreview()
        {
            RingVisibility = Visibility.Visible;

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

            RingVisibility = Visibility.Hidden;
        }

        [RelayCommand]
        private async void OnDownload()
        {
            RingVisibility = Visibility.Visible;

            if (DownloadUrl.xIsEmpty()) return;

            var downloadPath = Settings.Default.DownloadPath.xValue<string>(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));

            var youtube = new YoutubeClient();

            var mp4file = $"{downloadPath}\\{Guid.NewGuid()}.mp4";

            await youtube.Videos.DownloadAsync(DownloadUrl, mp4file);

            var mp3file = $"{downloadPath}\\{this.DownloadTitle.Replace("|", "_")}.mp3";
            using (var reader = new AudioFileReader(mp4file))
            {
                using (var writer = new LameMP3FileWriter(mp3file, reader.WaveFormat, LAMEPreset.VBR_90))
                {
                    await reader.CopyToAsync(writer);
                    await writer.DisposeAsync();
                }

                await reader.DisposeAsync();
            }

            var video = await youtube.Videos.GetAsync(this.DownloadUrl);
            byte[] imageBytes = [];
            if (video.Thumbnails.xIsNotEmpty())
            {
                var item = video.Thumbnails.FirstOrDefault(m => m.Resolution.Width == video.Thumbnails.Max(m2 => m2.Resolution.Width));

                if (item.xIsNotEmpty())
                {
                    using var client = new HttpClient();
                    imageBytes = await client.GetByteArrayAsync(item.Url);
                }
            }

            using (var tfile = TagLib.File.Create(mp3file))
            {
                tfile.Tag.Title = video.Title;
                tfile.Tag.Description = video.Description;
                var pic = new Picture
                {
                    Type = PictureType.Other,
                    MimeType = "image/jpeg",
                    Description = "Cover",
                    Data = imageBytes
                };
                tfile.Tag.Pictures = new Picture[1] { pic };                
                tfile.Save();
            }

            System.IO.File.Delete(mp4file);

            RingVisibility = Visibility.Hidden;
        }
    }
}
