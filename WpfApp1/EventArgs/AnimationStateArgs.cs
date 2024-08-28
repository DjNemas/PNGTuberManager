using PNGTuberManager.Service;
using System.Collections.Generic;
using System.Windows.Controls;

namespace PNGTuberManager.EventArgs
{
    internal class AnimationStateArgs
    {
        public AnimationState _state { get; set; }
        public List<Image> _stateImages { get; set; }
    }
}
