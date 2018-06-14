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
using System.Reflection;

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
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
                if(!IsMusicPlaying)
                {
                    player.Play();
                    IsMusicPlaying = true;
                }
                else
                {
                    player.Stop();
                    IsMusicPlaying = false;
                }
            }
            else
            {
                MessageBox.Show("Ждем");
            }
        }

        private bool PlayerInitialized { get; set; }

        private bool _IsMusicPlaying { get; set; }
        private bool IsMusicPlaying
        {
            get
            {
                return _IsMusicPlaying;
            }
            set
            {                
                _IsMusicPlaying = value;

                ChangePlayStopImage();
            }
        }

        private void ChangePlayStopImage()
        {
            if (IsMusicPlaying)
            {
                Dispatcher.Invoke(() =>
                {
                    //BitmapImage myBitmapImage = new BitmapImage();
                    //myBitmapImage.BeginInit();
                    //myBitmapImage.UriSource = new Uri("Pause.png", UriKind.Relative);
                    //myBitmapImage.EndInit();
                    byte[] buffer = File.ReadAllBytes("Pause.png");
                    ImageSource result;
                    using (var stream = new MemoryStream(buffer))
                    {
                        result = BitmapFrame.Create(
                            stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                    PlayStopImage.Source = result;
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    byte[] buffer = File.ReadAllBytes("Play.png");
                    ImageSource result;
                    using (var stream = new MemoryStream(buffer))
                    {
                        result = BitmapFrame.Create(
                            stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                    PlayStopImage.Source = result;
                });
            }
        }

        private async void ChangeMusicInPlayer()
        {
            await Task.Run(async () =>
            {
                Song songToPlay = null;
                try
                {
                    songToPlay = antiRadioRecord.GetCurrentSongOnRadioPlaying();
                    //File.WriteAllBytes("t.mp3", songToPlay.SongFile);
                    Mp3FileReader reader = new Mp3FileReader(new MemoryStream(songToPlay.SongFile));
                    var totalTime = reader.TotalTime;
                    if(totalTime.TotalMinutes < 3)
                    {
                        player.Init(reader);
                    }
                    if(totalTime.TotalMinutes >= 3 && totalTime.TotalMinutes < 4)
                    {
                        var needToSkip = totalTime.TotalMinutes * 0.1;
                        reader.CurrentTime = TimeSpan.FromMinutes(needToSkip);
                        player.Init(reader);
                    }
                    if (totalTime.TotalMinutes >= 4 && totalTime.TotalMinutes < 5)
                    {
                        var needToSkip = totalTime.TotalMinutes * 0.2;
                        reader.CurrentTime = TimeSpan.FromMinutes(needToSkip);
                        player.Init(reader);
                    }
                    if (totalTime.TotalMinutes >= 5)
                    {
                        var needToSkip = totalTime.TotalMinutes * 0.3;
                        reader.CurrentTime = TimeSpan.FromMinutes(needToSkip);
                        player.Init(reader);
                    }
                    if (IsMusicPlaying)
                    {
                        player.Play();
                    }
                    if(!PlayerInitialized)
                    {
                        PlayerInitialized = true;
                    }
                }
                catch(Exception ex) 
                {
                    MessageBox.Show(ex.Message);
                    //if(antiRadioRecord.BufferedSongs.Count==0)
                    //{
                    //    var songToPlay = antiRadioRecord.GetDownloadedSong(antiRadioRecord.SongsOnAir[0].ToString());
                    //    antiRadioRecord.BufferedSongs.Add(songToPlay);
                    //    Mp3FileReader reader = new Mp3FileReader(new MemoryStream(songToPlay));
                    //    player.Init(reader);
                    //    if (IsMusicPlaying)
                    //    {
                    //        player.Play();
                    //    }
                    //    if (!PlayerInitialized)
                    //    {
                    //        PlayerInitialized = true;
                    //    }
                    //}
                    //else
                    //{
                    if(antiRadioRecord.SongsOnAir.Count>1)
                    if(antiRadioRecord.SongsOnAir[1].SongFile!=null)
                    {
                        OnMusicStopped(null, null);
                    }
                    //}
                }                
                await Dispatcher.InvokeAsync(() =>
                {
                    if(songToPlay != null)
                    {
                        CurrentSongOnRadio.Text = songToPlay.ToString();
                    }
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
                    Volume.Value = 0.5;
                    player.Volume = (float)Volume.Value;
                });
                player.Volume = 0.1f;
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
            
        }

        private void Volume_ValueChanged()
        {
            player.Volume = (float)(Volume.Value);
        }
    }
}
