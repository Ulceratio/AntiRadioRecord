using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AntiRadioRecord
{
    /// <summary>
    /// Логика взаимодействия для SongDownloaderWindow.xaml
    /// </summary>
    public partial class SongDownloaderWindow : Window
    {
        #region Fields
        private gateToDB gate;
        private Action BuilderAfterDownload;
        #endregion

        public SongDownloaderWindow(Action BuilderAfterDownload)
        {
            InitializeComponent();
            this.BuilderAfterDownload = BuilderAfterDownload;
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            //gate = new gateToDB(writeInTB,increasePB);
        }

        private async void start()
        {
            await Task.Run(() => 
            {
                gate = new gateToDB(writeInTB,increasePB,CloseFunction);
                this.Dispatcher.Invoke(() => {
                    AmountOfSongs.Minimum = 0;
                    AmountOfSongs.Maximum = gate.numOfDays;
                });
                startGettingSongs();
            });
        }

        private void writeInTB(string text)
        {
            Dispatcher.Invoke(() =>
            {
                CurrentDate.Text = text;
            });
        }

        private void increasePB()
        {
            Dispatcher.Invoke(() =>
            {
                AmountOfSongs.Value++;
            });
        }

        private async void startGettingSongs()
        {
            await Task.Run(() => {
                gate.StartDump();
                BuilderAfterDownload();
            });
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            CloseFunction();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            start();
        }

        private void CloseFunction()
        {
            Dispatcher.Invoke(() => Close());
        }
    }
}
