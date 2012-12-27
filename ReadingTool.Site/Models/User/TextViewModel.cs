using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReadingTool.Site.Models.User
{
    public class TextViewModel
    {
        [ScaffoldColumn(false)]
        [ReadOnly(true)]
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "L1 Langauge", Order = 1)]
        public Guid L1Id { get; set; }

        [StringLength(250)]
        [Display(Name = "Audio URL", Order = 2)]
        [DataType(DataType.Url)]
        public string AudioUrl { get; set; }

        [StringLength(100)]
        [Display(Name = "Collection Name", Order = 3)]
        public string CollectionName { get; set; }

        [Display(Name = "Collection No", Order = 4)]
        public int? CollectionNo { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Title", Order = 5)]
        public string Title { get; set; }

        [StringLength(40000)]
        [Display(Name = "L1 Text", Order = 6)]
        [DataType(DataType.MultilineText)]
        public string L1Text { get; set; }

        [Display(Name = "L2 Langauge", Order = 7)]
        public Guid? L2Id { get; set; }

        [StringLength(40000)]
        [Display(Name = "L2 Text", Order = 8)]
        [DataType(DataType.MultilineText)]
        public string L2Text { get; set; }

        public string Tags { get; set; }
    }
}