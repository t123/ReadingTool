using System.ComponentModel;

namespace ReadingTool.Core.Enums
{
    public enum ModalBehaviour : short
    {
        [Description("Left Click")]
        LeftClick = 1,
        Rollover = 2,

        [Description("Control Left Click")]
        CtrlLeftClick = 3,

        [Description("Shift Left Click")]
        ShiftLeftClick = 4,

        [Description("Middle Click")]
        MiddleClick = 5,

        [Description("Right Click")]
        RightClick = 6
    }
}
