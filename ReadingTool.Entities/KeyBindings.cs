using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingTool.Entities
{
    public class Key
    {
        public int? KeyCode { get; set; }
        public string Character { get; set; }
    }

    public class KeyBindings
    {
        public bool IsEnabled { get; set; }
        public bool AutoPause { get; set; }
        public decimal SecondsToRewind { get; set; }

        public Key Reset { get; set; }
        public Key SpeedUp { get; set; }
        public Key SpeedDown { get; set; }
        public Key VolumeUp { get; set; }
        public Key VolumeDown { get; set; }
        public Key RewindToBeginning { get; set; }
        public Key Rewind { get; set; }
        public Key PlayPause { get; set; }
        public Key Stop { get; set; }
        public Key FastForward { get; set; }
        public Key Known { get; set; }
        public Key NotKnown { get; set; }
        public Key Ignored { get; set; }
        public Key NotSeen { get; set; }

        public KeyBindings()
        {
            SecondsToRewind = 3;
        }
    }
}
