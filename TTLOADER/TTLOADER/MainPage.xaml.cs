using HtmlAgilityPack;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Essentials;

using Xamarin.Forms;

namespace TTLOADER
{
    public partial class MainPage : ContentPage
    {
        HttpClient _httpClient = new HttpClient();
       
        (List<string> images, string audio) DownloadData;
       
        int Downloaded = 0;
        int DownloadCount = 0;
       
        public MainPage()
        {
            InitializeComponent();
            CheckPermissions();
            ClearProgressData();
            _httpClient.Timeout = TimeSpan.FromSeconds(60000);
            LoadImages.IsVisible = false;
            LoadMusic.IsVisible = false;
            UriWebView.IsVisible = false;

        }
        public async Task CheckPermissions()
        {
          
            var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
        }
        private void ClearProgressData()
        {
            ProgressPanel.IsVisible = false;
            DownloadResult.Text = string.Empty;
            DownloadBar.Progress = 0;
        }


        private void DownloadButton_Clicked(object sender, EventArgs e)
        {
            ClearProgressData();
            LoadImages.IsVisible = false;
            LoadMusic.IsVisible = false;
            if (string.IsNullOrEmpty(UriEntry.Text))
            {
                return;
            }

            UrlWebViewSource urlWebViewSource = new UrlWebViewSource()
            {
                Url = UriEntry.Text
            };
            UriWebView.Source = urlWebViewSource;
           
           
           
        }
        private async void UriWebView_Navigated(object sender, WebNavigatedEventArgs e)
        {
            
            if (e.Result == WebNavigationResult.Success)
            {
                ProgressPanel.IsVisible = true;
                try
                {
                    DownloadResult.Text = "Получение данных";

                    var html = await GetHtml();

                    var data = DataParser.GetData(html);
                    DownloadData = data;
                    if (string.IsNullOrEmpty(data.audio) is false)
                    {
                        LoadMusic.IsVisible = false;
                    }
                    if (data.images.Count() > 0)
                    {
                        LoadImages.IsVisible = true;
                        FlowView.ItemsSource = data.images;
                        //ImagesBytes = data.images.Select(img => _httpClient.GetByteArrayAsync(img).Result).ToList();
                    }
                }
                catch(Exception ex)
                {
                    DownloadResult.Text = "Ошибка:"+ex.Message;
                }
               
            }
        }

        private async Task<string> GetHtml()
        {
            string data = await UriWebView.EvaluateJavaScriptAsync("document.body.innerHTML");
            data = Regex.Unescape(data);
            if (string.IsNullOrEmpty(data.Trim()) is false)
            {
                string res =
                    $@"<html>  
                        <body>  
                            {data}
                        </body> 
                    </html>""";
                return res;
            }
            return null;
        }

        private void LoadImages_Clicked(object sender, EventArgs e)
        {
            ClearProgressData();
            FileEngine fileEngine  = new FileEngine();
            ProgressPanel.IsVisible = true;
           
            Task.Run(async () =>
            {

                bool isErrors = false;
                foreach (var image in DownloadData.images)
                {
                    MemoryStream memoryStream = new MemoryStream(await _httpClient.GetByteArrayAsync(image));
                    var res = await fileEngine.WriteFile(memoryStream, "Download",ext:".jpg");
                   
                    Debug.WriteLine("URI: "+image+" OK ");
                    Dispatcher.BeginInvokeOnMainThread(() =>
                    {
                        DownloadBar.Progress++;

                        if (res is false) isErrors = true;
                        
                    });

                }
                Dispatcher.BeginInvokeOnMainThread(() =>
                {
                    DownloadResult.Text = isErrors ? "Загрузка файлов завершилась с ошибкой" : "Загрузка успешно завершена";
                });
            });
           
        }

        private void LoadMusic_Clicked(object sender, EventArgs e)
        {
            ClearProgressData();
            FileEngine fileEngine = new FileEngine();
            ProgressPanel.IsVisible = true;
            Task.Run(async () =>
            {
                MemoryStream memoryStream = new MemoryStream(await _httpClient.GetByteArrayAsync(DownloadData.audio));
               
                bool res = await fileEngine.WriteFile(memoryStream, "Download",ext:".mp3");
                
                Dispatcher.BeginInvokeOnMainThread(() =>
                {
                    DownloadBar.Progress++;
                    DownloadResult.Text = res == false ? "Загрузка файлов завершилась с ошибкой" : "Загрузка успешно завершена";
                });
            });
        }

     
    }
    public static class DataParser
    {
        public static (List<string> images, string audio) GetData(string html)
        {

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            return (GetImages(document), GetAudio(document));
        }

        private static List<string> GetImages(HtmlDocument htmlDocument)
        {
            try
            {
                var nodes = htmlDocument.DocumentNode.SelectNodes("//div[@class='swiper-slide']/img");
                var images = nodes.Select(x => x.GetAttributeValue("src", ""))
                    .Where(x => string.IsNullOrEmpty(x) is false).Distinct().ToList();
                return images;
            }
            catch(Exception ex)
            {
                return new List<string>();
            }
        }


       
        private static string GetAudio(HtmlDocument htmlDocument)
        {
            try
            {
                var data = htmlDocument.DocumentNode.SelectSingleNode("//audio");
                return data.GetAttributeValue("src", "");
            }
            catch(Exception ex)
            {
                return string.Empty;
            }
        }
    }
}

