using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public class FloorArea
    {
        public int PropertyId { get; set; }
        [Display(Name = "Floor Name", Description = "Enter a name for this floor.")]
        public string Name { get; set; }
        [Display(Name = "Number / Level", Description = "Enter a number (level) for this floor.")]
        public int Number { get; set; }
        [Display(Name = "Size (Square Feet)", Description = "")]
        public decimal SquareFeet { get; set; }
        [Display(Name = "Size (Square Metres)", Description = "")]
        public decimal SquareMetres { get; set; }
    }
}
