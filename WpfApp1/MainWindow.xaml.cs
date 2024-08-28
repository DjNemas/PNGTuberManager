using Microsoft.Win32;
using NAudio.Wave;
using PNGTuberManager.EventArgs;
using PNGTuberManager.Models;
using PNGTuberManager.Service;
using PNGTuberManager.UIFeatures;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace PNGTuberManager
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<AudioDevice> Microphones = new ObservableCollection<AudioDevice>();
        public ObservableCollection<CustomImages> IdleImages = new ObservableCollection<CustomImages>();
        public ObservableCollection<CustomImages> PenImages = new ObservableCollection<CustomImages>();
        public ObservableCollection<CustomImages> SpeakImages = new ObservableCollection<CustomImages>();

        private Action _touchEnded;

        private MicAudio _micAudio = new();
        private float _silenceThreshold = 0.02F;
        private static TimeSpan _timerTouchResetTime = TimeSpan.FromMilliseconds(500);
        private TimeSpan _timereReduceTimer = TimeSpan.FromMilliseconds(10);
        private TimeSpan _timerTouch = _timerTouchResetTime;

        private readonly Brush _defaultButtonColor = Brushes.LightGray;
        private readonly Brush _selectedButtonColor = Brushes.LightGreen;

        private bool _dragDropEnabled = true;


        private object _timeLocker = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            FillListCANBEREMOVEDLATER(5, IdleImages, LB_IdleListBox);
            FillListCANBEREMOVEDLATER(2, PenImages, LB_PenListBox);
            FillListCANBEREMOVEDLATER(4, SpeakImages, LB_SpeakListBox);

            _ = Task.Run(ReduceTouchTimer);
            _touchEnded += OnTouchEnded;

            LowLevelMouseTouchHook.OnTouchInput += OnTouching;

            GradientSlider.SetGradientPosition(SL_MicLevel, 0); // Default

            DataContext = this;

            foreach (var mic in _micAudio.GetMicrophones())
            {
                Microphones.Add(mic);
            };
            LV_MicList.ItemsSource = Microphones;

            if(Microphones.Count() > 0)
            {
                var micDevice = 0; // Read From Settings!
                LV_MicList.SelectedIndex = micDevice;
                _micAudio.SetMicrophone(micDevice);
            }

            var silenceThreshold = 0.02F; // Read From Settings!
            SL_MicLevel.Value = silenceThreshold;
            _silenceThreshold = silenceThreshold;
            
            _micAudio.VoiceRecognized(OnVoiceRecognized);

            _micAudio.StartRecording();
        }

        private void FillListCANBEREMOVEDLATER(int count, ObservableCollection<CustomImages> list, ListBox box)
        {
            for (int i = 0; i < count; i++)
            {
                var image = new CustomImages();
                var frames = GetGifAnimation(@"C:\Users\denis\Desktop\MIP PNGTuber Gif\pen1.gif");
                image.BeginAnimation(Image.SourceProperty, frames);

                var image2 = new CustomImages();
                var frames2 = GetGifAnimation(@"C:\Users\denis\Desktop\MIP PNGTuber Gif\pen2.gif");
                image2.BeginAnimation(Image.SourceProperty, frames2);
                list.Add(image);
                list.Add(image2);
            }
            box.ItemsSource = list;
        }

        #region Voice
        private void OnVoiceRecognized(object sender, WaveInEventArgs args)
        {
            double rms = _micAudio.CalculateRMS(args.Buffer, args.BytesRecorded);

            try
            {
                Dispatcher.Invoke(() => GradientSlider.SetGradientPosition(SL_MicLevel, rms));
            }
            catch { }// Ignore

            if (rms > _silenceThreshold)
            {
                //Console.ForegroundColor = ConsoleColor.Cyan;
                //Console.WriteLine($"RMS: {rms.ToString("0.00000000000")} (Speech)");
            }
            else
            {
                //Console.ForegroundColor = ConsoleColor.DarkCyan;
                //Console.WriteLine($"RMS: {rms.ToString("0.00000000000")} (Silence)");
            }
            //Console.ResetColor();
        }

        private void SL_MicLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _silenceThreshold = (float)e.NewValue;
        }

        private void LV_MicList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selecedItem = (AudioDevice)((ComboBox)e.Source).SelectedItem;
            _micAudio.SetMicrophone(selecedItem.DeviceNumber);
            _micAudio.RestartRecording();
        }
        #endregion

        #region Touch
        private void OnTouching(object sender, TouchPenArgs args)
        {
            lock (_timeLocker)
            {
                _timerTouch = _timerTouchResetTime;
                Dispatcher.InvokeAsync(() =>
                {
                    LBL_IsTouching.Content = "Touching :)";
                    LBL_IsTouching.Background = Brushes.Green;
                });
            }
        }

        private void OnTouchEnded()
        {
            Dispatcher.InvokeAsync(() =>
            {
                LBL_IsTouching.Content = "Not Touching";
                LBL_IsTouching.Background = Brushes.Red;
            });
        }

        private async Task ReduceTouchTimer()
        {
            while(true)
            {
                lock (_timeLocker)
                {
                    _timerTouch = _timerTouch.Subtract(_timereReduceTimer);
                    if (_timerTouch.TotalMilliseconds <= 0)
                    {
                        _touchEnded.Invoke();
                    }
                }

                await Task.Delay(_timereReduceTimer);
            };
        }
        #endregion

        #region Button Animation
        private void BTN_OpenAnimation_Click(object sender, RoutedEventArgs e)
        {
            var animationWindow = new Animation();
            animationWindow.Show();
            BTN_OpenAnimation.IsEnabled = false;
        }

        public static void EnableOpenAnimationButton()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ((MainWindow)Application.Current.MainWindow).BTN_OpenAnimation.IsEnabled = true;
            });
        }
        #endregion

        #region Button Open/Close
        private void BTN_OpenIdleImage_Click(object sender, RoutedEventArgs e)
        {
            if(GD_IdleImages.Visibility == Visibility.Visible)
            {
                GD_IdleImages.Visibility = Visibility.Collapsed;
                BTN_OpenIdleImage.Background = _defaultButtonColor;
            }                
            else
            {
                GD_IdleImages.Visibility = Visibility.Visible;
                BTN_OpenIdleImage.Background = _selectedButtonColor;
            }

            GD_PenImages.Visibility = Visibility.Collapsed;
            GD_SpeakImages.Visibility = Visibility.Collapsed;

            BTN_OpenPenImage.Background = _defaultButtonColor;
            BTN_OpenSpeakImage.Background = _defaultButtonColor;
        }

        private void BTN_OpenPenImage_Click(object sender, RoutedEventArgs e)
        {
            if (GD_PenImages.Visibility == Visibility.Visible)
            {
                GD_PenImages.Visibility = Visibility.Collapsed;
                BTN_OpenPenImage.Background = _defaultButtonColor;
            }
            else
            {
                GD_PenImages.Visibility = Visibility.Visible;
                BTN_OpenPenImage.Background = _selectedButtonColor;
            }                

            GD_IdleImages.Visibility = Visibility.Collapsed;
            GD_SpeakImages.Visibility = Visibility.Collapsed;

            BTN_OpenIdleImage.Background = _defaultButtonColor;
            BTN_OpenSpeakImage.Background = _defaultButtonColor;
        }

        private void BTN_OpenSpeakImage_Click(object sender, RoutedEventArgs e)
        {
            if (GD_SpeakImages.Visibility == Visibility.Visible)
            {
                GD_SpeakImages.Visibility = Visibility.Collapsed;
                BTN_OpenSpeakImage.Background = _defaultButtonColor;
            }               
            else
            {
                GD_SpeakImages.Visibility = Visibility.Visible;
                BTN_OpenSpeakImage.Background = _selectedButtonColor;
            }

            GD_IdleImages.Visibility = Visibility.Collapsed;
            GD_PenImages.Visibility = Visibility.Collapsed;

            BTN_OpenIdleImage.Background = _defaultButtonColor;
            BTN_OpenPenImage.Background = _defaultButtonColor;

        }
        #endregion

        #region DRAG AND DROP
        private void LB_IdleListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectImage(e, IdleImages, LB_IdleListBox, BTN_DeleteIdle);
            DragAndDropLeftButtonDown(e, IdleImages);
        }

        private void LB_IdleListBox_Drop(object sender, DragEventArgs e)
        {
            DragAndDropDrop(e, IdleImages);
        }

        private void LB_PenListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectImage(e, PenImages, LB_PenListBox, BTN_DeletePen);
            DragAndDropLeftButtonDown(e, PenImages);
        }

        private void LB_PenListBox_Drop(object sender, DragEventArgs e)
        {
            DragAndDropDrop(e, PenImages);
        }

        private void LB_SpeakListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectImage(e, SpeakImages, LB_SpeakListBox, BTN_DeleteSpeak);
            DragAndDropLeftButtonDown(e, SpeakImages);
        }

        private void LB_SpeakListBox_Drop(object sender, DragEventArgs e)
        {
            DragAndDropDrop(e, SpeakImages);
        }

        private void DragAndDropLeftButtonDown(MouseButtonEventArgs e, ObservableCollection<CustomImages> list)
        {
            if (!_dragDropEnabled) return;
            if (e.OriginalSource is Image image)
            {
                var startItem = list.FirstOrDefault(x => x.Source == image.Source);
                startItem.IsDrag = true;
                DragDrop.DoDragDrop(image, image, DragDropEffects.Move);
            }
        }

        private void DragAndDropDrop(DragEventArgs e, ObservableCollection<CustomImages> list)
        {
            if (!_dragDropEnabled) return;
            if (e.OriginalSource is Image originalSource)
            {
                var startObject = list.FirstOrDefault(x => x.IsDrag == true);
                startObject.IsDrag = false;

                var indexStart = list.IndexOf(startObject);
                var indexDrop = list.IndexOf(IdleImages.FirstOrDefault(x => x.Source == originalSource.Source));

                list.RemoveAt(indexStart);
                list.Insert(indexDrop, startObject);
            }
        }

        private void SelectImage(MouseButtonEventArgs e, ObservableCollection<CustomImages> list, ListBox listBox, Button button)
        {
            if (e.OriginalSource is Image image)
            {
                var index = list.IndexOf(IdleImages.FirstOrDefault(i => i == image));
                listBox.SelectedIndex = index;
                button.IsEnabled = true;
            }
        }

        #endregion

        #region Add/Delete Buttons
        private void BTN_AddIdle_Click(object sender, RoutedEventArgs e)
        {
            AddImage(IdleImages);
        }

        private void BTN_DeleteIdle_Click(object sender, RoutedEventArgs e)
        {
            RemoveImage(LB_IdleListBox, IdleImages);
        }

        private void BTN_AddPen_Click(object sender, RoutedEventArgs e)
        {
            AddImage(PenImages);
        }

        private void BTN_DeletePen_Click(object sender, RoutedEventArgs e)
        {
            RemoveImage(LB_PenListBox, PenImages);
        }

        private void BTN_AddSpeak_Click(object sender, RoutedEventArgs e)
        {
            AddImage(SpeakImages);
        }

        private void BTN_DeleteSpeak_Click(object sender, RoutedEventArgs e)
        {
            RemoveImage(LB_SpeakListBox, SpeakImages);
        }

        private void AddImage(ObservableCollection<CustomImages> list)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Multiselect = true,
                Filter = "Image Files (*.gif;*.jpg;*.jpeg)|*.gif;*.jpg;*.jpeg",
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = "Select Images or Gifs"                
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string fileName in openFileDialog.FileNames)
                {
                    if(new FileInfo(fileName).Extension == ".gif")
                    {
                        var image = new CustomImages();
                        var frames = GetGifAnimation(fileName);
                        image.BeginAnimation(Image.SourceProperty, frames);
                        list.Add(image);
                    }
                    else
                    {
                        var image = new CustomImages();
                        image.Source = new BitmapImage(new Uri(fileName));
                        list.Add(image);
                    }                    
                }
            }
        }

        private void RemoveImage(ListBox lb, ObservableCollection<CustomImages> list)
        {
            if (lb.SelectedItem is not null)
            {
                var image = (CustomImages)lb.SelectedItem;
                list.Remove(image);
            }
        }


        private ObjectAnimationUsingKeyFrames GetGifAnimation(string path)
        {
            var gifUri = new Uri(path, UriKind.RelativeOrAbsolute);
            var gifDecoder = new GifBitmapDecoder(gifUri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

            // Erstelle ein ImageAnimationClock, um die Animation zu steuern
            var animation = new ObjectAnimationUsingKeyFrames();
            foreach (var frame in gifDecoder.Frames)
            {
                var keyFrame = new DiscreteObjectKeyFrame(frame, KeyTime.Paced);
                animation.KeyFrames.Add(keyFrame);
            }
            animation.RepeatBehavior = RepeatBehavior.Forever;
            return animation;
        }
        #endregion

        private void CB_DragDropEnabled_Checked(object sender, RoutedEventArgs e)
        {
            _dragDropEnabled = true;
            if(BTN_DeleteIdle is not null && BTN_DeletePen is not null && BTN_DeleteSpeak is not null)
            {
                BTN_DeleteIdle.IsEnabled = false;
                BTN_DeletePen.IsEnabled = false;
                BTN_DeleteSpeak.IsEnabled = false;
            }            
        }

        private void CB_DragDropEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            _dragDropEnabled = false;
        }

        #region SelectionChanged
        private void LB_IdleListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(((ListBox)e.Source).SelectedItem == null)
                BTN_DeleteIdle.IsEnabled = false;
        }

        private void LB_PenListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)e.Source).SelectedItem == null)
                BTN_DeletePen.IsEnabled = false;
        }

        private void LB_SpeakListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)e.Source).SelectedItem == null)
                BTN_DeleteSpeak.IsEnabled = false;
        }
        #endregion
    }
}
