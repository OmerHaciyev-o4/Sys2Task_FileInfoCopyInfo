using Microsoft.Win32;
using SysProgTask2_FileAnotherFileCopy.MVVM.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; 
using System.Text; 
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using mdt = MaterialDesignThemes.Wpf;

namespace SysProgTask2_FileAnotherFileCopy.MVVM.ViewModel
{
    public class MainViewModel
    {
        private bool _startState;
        private bool _pauseState;
        private mdt.PackIcon start;
        private mdt.PackIcon stop;
        private mdt.PackIcon pause;
        private Thread mainWork;


        public MainScreen MainScreen { get; set; }


        public MainViewModel()
        {
            start = new mdt.PackIcon() { Kind = mdt.PackIconKind.Play };
            stop = new mdt.PackIcon() { Kind = mdt.PackIconKind.Stop };
            pause = new mdt.PackIcon() { Kind = mdt.PackIconKind.Pause };

            Thread setData = new Thread(() =>
            {
                MainScreen.OpenFromButton.Click += OpenFromButton_Click;
                MainScreen.OpenToButton.Click += OpenToButton_Click;
                MainScreen.StartStop.Click += StartStop_Click;
                MainScreen.PauseResume.Click += PauseResumeOnClick;
            });
            setData.Start();
        }

        private void PauseResumeOnClick(object sender, RoutedEventArgs e)
        {
            if (!_pauseState)
            {
                mainWork.Suspend();
                MainScreen.PauseResume.Content = start;
                _pauseState = true;
            }
            else if (_pauseState)
            {
                mainWork.Resume();
                MainScreen.PauseResume.Content = pause;
                _pauseState = false;
            }
        }

        private void StartStop_Click(object sender, RoutedEventArgs e)
        {
            if (!_startState)
            {
                if (!File.Exists(MainScreen.FromPath.Text))
                {
                    MessageBox.Show("Please set correct FROM Text file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (!File.Exists(MainScreen.ToPath.Text))
                {
                    MessageBox.Show("Please set correct TO Text file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    _startState = true;
                    
                    MainScreen.StartStop.Content = stop;
                    MainScreen.PauseResume.Content = pause;
                    MainScreen.PauseResume.IsEnabled = true;

                    var from = MainScreen.FromPath.Text;
                    var to = MainScreen.ToPath.Text;

                    var file = new FileInfo(from);
                        
                    if (file.Length > 0)
                    {
                        mainWork = new Thread(() =>
                        {
                            string[] allData = File.ReadAllLines(from);
                            File.WriteAllText(to, "");

                            double count = 100.0 / allData.Length;

                            foreach (var data in allData)
                            {
                                Thread.Sleep(1000);
                                File.AppendAllText(to, data + "\n");
                                MainScreen.State.Dispatcher.BeginInvoke(new Action(() => { MainScreen.State.Value += count; }));
                            }

                            MessageBox.Show("Information copy successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            _startState = false;
                            MainScreen.StartStop.Dispatcher.BeginInvoke(new Action(() => { MainScreen.StartStop.Content = start; }));
                            MainScreen.PauseResume.Dispatcher.BeginInvoke(new Action(() => { MainScreen.PauseResume.Content = pause; MainScreen.PauseResume.IsEnabled = false; }));
                            MainScreen.State.Dispatcher.BeginInvoke(new Action(() => { MainScreen.State.Value = 0; }));
                        });
                        mainWork.Start();
                    }
                    else
                    {
                        MainScreen.StartStop.Content = start;
                        _startState = false;
                    }
                }
            }
            else if (_startState)
            {
                _startState = false;
                MainScreen.StartStop.Content = start;
                MainScreen.PauseResume.IsEnabled = false;
                MainScreen.PauseResume.Content = pause;
                MainScreen.State.Value = 0;
                if (!_pauseState)
                    mainWork.Abort();

                MessageBox.Show("Copy stoped successfully", "Information", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private string Encrypt(string data)
        {
            char[] resultText = new char[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                int charToNumber = Convert.ToInt32(data[i]);
                charToNumber += 5;
                char numberToChar = Convert.ToChar(charToNumber);
                resultText[i] = numberToChar;
            }

            return new string(resultText);
        }

        private void OpenToButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "txt file (*.txt)|*.txt";
            openFile.Title = "Select File";
            var state = openFile.ShowDialog();
            if (state != null)
            {
                MainScreen.ToPath.Text = openFile.FileName;
            }
        }

        private void OpenFromButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "txt file (*.txt)|*.txt";
            openFile.Title = "Select File";
            var state = openFile.ShowDialog();
            if (state != null)
            {
                MainScreen.FromPath.Text = openFile.FileName;
            }
        }
    }
}