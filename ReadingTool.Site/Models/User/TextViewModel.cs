using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using MongoDB.Bson;

namespace ReadingTool.Site.Models.User
{
    [Description("Text")]
    public class TextViewModel
    {
        [HiddenInput(DisplayValue = false)]
        public ObjectId Id { get; set; }

        [Required]
        [Display(Name = "L1 Langauge", Order = 1)]
        public ObjectId L1Id { get; set; }

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

        [Display(Name = "L1 Text", Order = 6)]
        [DataType(DataType.MultilineText)]
        public string L1Text { get; set; }

        [Display(Name = "L2 Langauge", Order = 7)]
        public ObjectId? L2Id { get; set; }

        [Display(Name = "L2 Text", Order = 8)]
        [DataType(DataType.MultilineText)]
        public string L2Text { get; set; }

        public string Tags { get; set; }
    }
}