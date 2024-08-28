using PNGTuberManager.EventArgs;
using System.Windows.Controls;

namespace PNGTuberManager.Service
{
    internal class AnimationStateMachine
    {
        public EventHandler<AnimationStateArgs> OnAnimationStateChange;

        private readonly List<Image> _idleImages = new();
        private readonly List<Image> _talkingImages = new();
        private readonly List<Image> _drawingImages = new();

        private AnimationState _currentState = AnimationState.Idle;
        private int _timeLeftIdle = 0;
        private int _timeLeftTalking = 0;
        private int _timeLeftDrawing = 0;
        private int _timeLeftSwitchIdle = 0;
        private int _timeLeftSwitchTalking = 0;
        private int _timeLeftSwitchDrawing = 0;
        private TimeSpan _timeSwitchTimeIdel = TimeSpan.FromMilliseconds(2000);
        private TimeSpan _timeSwitchTimeTalking = TimeSpan.FromMilliseconds(2000);
        private TimeSpan _timeSwitchTimeDrawing = TimeSpan.FromMilliseconds(2000);
        private TimeSpan _timeActiveTimeIdle = TimeSpan.FromMilliseconds(1000);
        private TimeSpan _timeActiveTimeTalking = TimeSpan.FromMilliseconds(1000);
        private TimeSpan _timeActiveTimeDrawing = TimeSpan.FromMilliseconds(1000);


        public List<Image> GetImages(AnimationState state)
        {
            switch (state)
            {
                case AnimationState.Idle:
                    return _idleImages;
                case AnimationState.Talking:
                    return _talkingImages;
                case AnimationState.Drawing:
                    return _drawingImages;
                default:
                    return _idleImages;
            }
        }

        public void SetTimeActiveTime(TimeSpan time, AnimationState state)
        {

            switch (state)
            {
                case AnimationState.Idle:
                    _timeActiveTimeIdle = time;
                    break;
                case AnimationState.Talking:
                    _timeActiveTimeTalking = time;
                    break;
                case AnimationState.Drawing:
                    _timeActiveTimeDrawing = time;
                    break;
            }
            
        }

        public void AddImage(Image image, AnimationState state)
        {
            switch (state)
            {
                case AnimationState.Idle:
                    _idleImages.Add(image);
                    break;
                case AnimationState.Talking:
                    _talkingImages.Add(image);
                    break;
                case AnimationState.Drawing:
                    _drawingImages.Add(image);
                    break;
            }
        }

    }
    public enum AnimationState
    {
        Idle,
        Talking,
        Drawing
    }
}
