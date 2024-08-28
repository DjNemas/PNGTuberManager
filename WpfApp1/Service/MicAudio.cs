using NAudio.Wave;

namespace PNGTuberManager.Service
{
    internal class MicAudio
    {
        private int? _currentDeviceNumber = null;
        private WaveInEvent _waveIn = new();

        private bool _isRecording = false;

        public MicAudio()
        {
            _waveIn.RecordingStopped += OnRecordingStopped;
        }

        public List<AudioDevice> GetMicrophones() => Enumerable.Range(0, WaveIn.DeviceCount)
            .Select(i => new AudioDevice { DeviceNumber = i, Capabilities = WaveIn.GetCapabilities(i) })
            .ToList();

        public void RestartRecording()
        {
            _ = Task.Run(() => 
            {
                _waveIn.StopRecording();
                while (_isRecording) { }
                StartRecording();
            });
        }

        public void SetMicrophone(int deviceNumber) => _currentDeviceNumber = deviceNumber;

        public bool HasMicrophone() => _currentDeviceNumber is not null;

        public void StartRecording()
        {
            if (_currentDeviceNumber is null)
            {
                Console.WriteLine("No microphone set up");
                return;
            }

            if (_isRecording)
            {
                Console.WriteLine("Already recording. Please use RestartRecording() for restart.");
                return;
            }

            _isRecording = true;
            _waveIn.DeviceNumber = (int)_currentDeviceNumber;
            _waveIn.WaveFormat = new WaveFormat(48000, 16, 2);
            _waveIn.StartRecording();
        }

        public void VoiceRecognized(EventHandler<WaveInEventArgs> callback)
        {
            _waveIn.DataAvailable += callback;
        }

        public double CalculateRMS(byte[] buffer, int bytesRecorded)
        {
            int bytesPerSample = 2; // Assuming 16-bit audio
            int sampleCount = bytesRecorded / bytesPerSample;
            double sumOfSquares = 0.0;

            for (int i = 0; i < bytesRecorded; i += bytesPerSample)
            {
                short sample = BitConverter.ToInt16(buffer, i);
                double sample32 = sample / 32768.0; // Convert to range -1.0 to 1.0
                sumOfSquares += sample32 * sample32;
            }

            return Math.Sqrt(sumOfSquares / sampleCount);
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs args)
        {
            _isRecording = false;
        }
    }

    public class AudioDevice
    {
        public int DeviceNumber { get; set; }
        public WaveInCapabilities Capabilities { get; set; }
    }
}
