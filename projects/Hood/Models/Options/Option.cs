using Hood.Entities;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public partial class Option : BaseEntity
    {
        [Key]
        public new string Id { get; set; }
        public string Value { get; set; }
    }
}

