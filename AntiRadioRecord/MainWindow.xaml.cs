using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Media;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using NAudio;
using NAudio.Wave;

namespace AntiRadioRecord
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        WaveOutEvent player;
        AntiRadioRecordWave antiRadioRecord;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;            
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //SongDownloaderWindow downloaderWindow = new SongDownloaderWindow(Builder);
            //downloaderWindow.Owner = this;
            //downloaderWindow.ShowDialog();
            LockAllElements();
            Builder();
        }

        private async void LockAllElements()
        {
            await mainGrid.Dispatcher.InvokeAsync(() => { mainGrid.Visibility = Visibility.Collapsed; });
        }

        private async void UnLockAllElements()
        {
            await mainGrid.Dispatcher.InvokeAsync(() => { mainGrid.Visibility = Visibility.Visible; });
        }

        private void PlayStop_Click(object sender, RoutedEventArgs e)
        {
            if (!PlayerInitialized)
            {
                ChangeMusicInPlayer();
            }
            if (PlayerInitialized)
            {
                player.Play();
                IsMusicPlaying = true;
            }
            else
            {
                MessageBox.Show("Ждем");
            }
        }

        private bool PlayerInitialized { get; set; }
        private bool IsMusicPlaying { get; set; }

        private async void ChangeMusicInPlayer()
        {
            await Task.Run(async () =>
            {
                try
                {
                    var songToPlay = antiRadioRecord.GetCurrentSongOnRadioPlaying();
                    Mp3FileReader reader = new Mp3FileReader(new MemoryStream(songToPlay));
                    player.Init(reader);
                    if(IsMusicPlaying)
                    {
                        player.Play();
                    }
                    if(!PlayerInitialized)
                    {
                        PlayerInitialized = true;
                    }
                }
                catch 
                {
                    if(antiRadioRecord.BufferedSongs.Count==0)
                    {
                        var songToPlay = antiRadioRecord.GetDownloadedSong(antiRadioRecord.SongsOnAir[0].ToString());
                        antiRadioRecord.BufferedSongs.Add(songToPlay);
                        Mp3FileReader reader = new Mp3FileReader(new MemoryStream(songToPlay));
                        player.Init(reader);
                        if (IsMusicPlaying)
                        {
                            player.Play();
                        }
                        if (!PlayerInitialized)
                        {
                            PlayerInitialized = true;
                        }
                    }
                    else
                    {
                        OnMusicStopped(null, null);
                    }
                }                
                await Dispatcher.InvokeAsync(() =>
                {
                    CurrentSongOnRadio.Text = antiRadioRecord.SongsOnAir[0].ToString();
                    UnLockAllElements();
                });
                ChangeCover();
            });
        }

        private void Builder()
        {
            Task.Run(() => 
            {
                IsMusicPlaying = false;
                player = new WaveOutEvent();
                PlayerInitialized = false;
                Volume.Dispatcher.Invoke(() =>
                {
                    Volume.Value = 1;
                });
                player.Volume = 0.1f;
                player.PlaybackStopped += new EventHandler<StoppedEventArgs>(OnMusicStopped);
                antiRadioRecord = new AntiRadioRecordWave(ChangeMusicInPlayer);
            });
        }

        private void ReBuildPlayer()
        {
            player = new WaveOutEvent();
            player.Volume = 0.1f;
            player.PlaybackStopped += new EventHandler<StoppedEventArgs>(OnMusicStopped);
        }

        private void CoverImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChangeCover();
        }

        private async void ChangeCover()
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                byte[] buffer = await antiRadioRecord.GetNewCover();
                ImageSource result;
                using (var stream = new MemoryStream(buffer))
                {
                    result = BitmapFrame.Create(
                        stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
                CoverImage.Source = result;
            });
        }

        private void OnMusicStopped(object sender, StoppedEventArgs e)
        {
            player.Stop();
            antiRadioRecord.SongOnRadioFinished();
            ChangeMusicInPlayer();
        }

        private void Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.Volume = (float)(Volume.Value / 10.0);
        }
    }
}
