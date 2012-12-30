using System.ComponentModel.DataAnnotations;

namespace ReadingTool.Site.Models.User
{
    public class JsonSampleModel
    {
        [Display(Name = "Number of items", Order = 1)]
        [Range(0, 100)]
        public int? NumberOfItems { get; set; }

        [Display(Name = "Default L1 Name", Order = 2)]
        public string L1Name { get; set; }

        [Display(Name = "Default L2 Name", Order = 3)]
        public string L2Name { get; set; }

        [Display(Name = "Default Collection Name", Order = 4)]
        public string CollectionName { get; set; }

        [Display(Name = "Auto Number Collection", Order = 5)]
        public bool AutoNumberCollection { get; set; }

        [Display(Name = "Start Collection With Number", Order = 6)]
        [Range(0, 1000)]
        public int? StartCollectionWithNumber { get; set; }

        [Display(Name = "Default Tags", Order = 7)]
        public string Tags { get; set; }
    }
}