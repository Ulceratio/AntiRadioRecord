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

namespace AntiRadioRecord
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        SoundPlayer player;
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
            vkmDownload download = new vkmDownload();
            var res = await download.GetMp3ForMusic7sAsync("levels");
            File.WriteAllBytes("temp.mp3", res);
            //SongDownloaderWindow downloaderWindow = new SongDownloaderWindow(Builder);
            //downloaderWindow.Owner = this;
            //downloaderWindow.ShowDialog();
        }

        private void PlayStop_Click(object sender, RoutedEventArgs e)
        {
            ChangeMusicInPlayer();
            player.Play();
        }

        private async void ChangeMusicInPlayer()
        {
            await Task.Run(() =>
            {
                if (antiRadioRecord.ReadyToPlay)
                {
                    var songToPlay = antiRadioRecord.GetCurrentSongOnRadioPlaying();
                    while (songToPlay == null)
                    {
                        songToPlay = antiRadioRecord.GetCurrentSongOnRadioPlaying();
                    }
                    player.Stream = new MemoryStream(songToPlay);
                }
                else
                {
                    while (!antiRadioRecord.ReadyToPlay)
                    { }
                    var songToPlay = antiRadioRecord.GetCurrentSongOnRadioPlaying();
                    while (songToPlay == null)
                    {
                        songToPlay = antiRadioRecord.GetCurrentSongOnRadioPlaying();
                    }
                    player.Stream = new MemoryStream(songToPlay);
                }
            });
        }

        private void Builder()
        {
            Task.Run(() => 
            {
                player = new SoundPlayer();
                antiRadioRecord = new AntiRadioRecordWave();
                antiRadioRecord.changeMusic += ChangeMusicInPlayer;
            });
        }
    }
}
