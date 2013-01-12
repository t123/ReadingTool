using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingTool.Entities
{
    public class Review
    {
        public const int MAX_BOXES = 9;

        public int? Box1Minutes { get; set; }
        public int? Box2Minutes { get; set; }
        public int? Box3Minutes { get; set; }
        public int? Box4Minutes { get; set; }
        public int? Box5Minutes { get; set; }
        public int? Box6Minutes { get; set; }
        public int? Box7Minutes { get; set; }
        public int? Box8Minutes { get; set; }
        public int? Box9Minutes { get; set; }
        public int? KnownAfterBox { get; set; }

        public static Review Default
        {
            get
            {
                return new Review()
                {
                    Box1Minutes = 3 * 60 * 24,
                    Box2Minutes = 7 * 60 * 24,
                    Box3Minutes = 15 * 60 * 24,
                    Box4Minutes = 30 * 60 * 24,
                    Box5Minutes = 60 * 60 * 24,
                    Box6Minutes = 90 * 60 * 24,
                    Box7Minutes = 180 * 60 * 24,
                    Box8Minutes = 210 * 60 * 24,
                    Box9Minutes = 240 * 60 * 24,
                    KnownAfterBox = MAX_BOXES
                };
            }
        }
    }
}
