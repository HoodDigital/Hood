using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public class CheckoutModel
    {
        public Cart ShoppingCart { get; set; }
        public bool ShowCheckout { get; set; }

        [Required]
        [Display(Name = "Card Security Number")]
        public string CVV2 { get; set; }

        [Required]
        [Display(Name = "Expiry Month")]
        public int ExpiryMonth { get; set; }

        [Required]
        [Display(Name = "Expiry Year")]
        public int ExpiryYear { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Card Number")]
        public string Number { get; set; }

        [Required]
        [Display(Name = "Card Type")]
        public string Type { get; set; }

        public string Message { get; internal set; }

        [Required]
        public int BillingAddressId { get; set; }
        [Required]
        public int DeliveryAddressId { get; set; }
    }
}