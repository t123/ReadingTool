using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReadingTool.Site.Models.User
{
    public class KeyModel
    {
        public int? KeyCode { get; set; }
        public string Character { get; set; }
    }

    [Description("Controls")]
    public class KeyBindingsModel
    {
        [Display(Name = "Enable controls?")]
        public bool IsEnabled { get; set; }

        [Display(Name = "Pause when modal appears")]
        public bool AutoPause { get; set; }

        [Display(Name = "Rewind/fast forward by...")]
        [Range(0, 100)]
        public decimal SecondsToRewind { get; set; }

        [Display(Name = "Reset Word")]
        public KeyModel Reset { get; set; }

        [Display(Name = "Speed up playback")]
        public KeyModel SpeedUp { get; set; }

        [Display(Name = "Slow down playback")]
        public KeyModel SpeedDown { get; set; }

        [Display(Name = "Increase Volume")]
        public KeyModel VolumeUp { get; set; }

        [Display(Name = "Decrease Volumne")]
        public KeyModel VolumeDown { get; set; }

        [Display(Name = "Rewind to beginning")]
        public KeyModel RewindToBeginning { get; set; }

        [Display(Name = "Rewind")]
        public KeyModel Rewind { get; set; }

        [Display(Name = "Play/Pause")]
        public KeyModel PlayPause { get; set; }

        [Display(Name = "Stop")]
        public KeyModel Stop { get; set; }

        [Display(Name = "Fast Forward")]
        public KeyModel FastForward { get; set; }

        [Display(Name = "Change to known")]
        public KeyModel Known { get; set; }

        [Display(Name = "Change to unknown")]
        public KeyModel NotKnown { get; set; }

        [Display(Name = "Change to ignored")]
        public KeyModel Ignored { get; set; }

        [Display(Name = "Change to not seen")]
        public KeyModel NotSeen { get; set; }

        public KeyBindingsModel()
        {
            SpeedUp = new KeyModel();
            SpeedDown = new KeyModel();
        }
    }
}