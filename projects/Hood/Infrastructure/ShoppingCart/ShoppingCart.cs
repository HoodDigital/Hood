using Hood.Models;
using Hood.Models.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Linq;

namespace Hood.Services
{
    /// <summary>
    /// Configuration and access tokens for accessing the PayPal Api
    /// </summary>
    public class ShoppingCart : IShoppingCart
    {
        private readonly IConfiguration _config;
        private readonly IPayPalService _paypal;
        private readonly IContentRepository _data;
        private readonly IHttpContextAccessor _context;
        private Cart _cart;

        public ShoppingCart(IConfiguration config, IPayPalService paypal, IContentRepository data, IHttpContextAccessor contextAccessor)
        {
            _config = config;
            _paypal = paypal;
            _data = data;
            _context = contextAccessor;
        }

        private void Load()
        {
            if (!_context.HttpContext.Session.Keys.Contains("Cart-" + _context.HttpContext.Session.Id))
            {
                _context.HttpContext.Session.SetString("Cart-" + _context.HttpContext.Session.Id, JsonConvert.SerializeObject(new Cart()));
            }
            _cart = JsonConvert.DeserializeObject<Cart>(_context.HttpContext.Session.GetString("Cart-" + _context.HttpContext.Session.Id));
        }

        private void Save()
        {
            _context.HttpContext.Session.SetString("Cart-" + _context.HttpContext.Session.Id, JsonConvert.SerializeObject(_cart));
        }

        /// <summary>
        /// Gets the Cart Object from the current session.
        /// </summary>
        public Cart Cart
        {
            get
            {
                Load();
                return _cart;
            }
            set
            {
                _cart = value;
                Save();
            }
        }

        public void Add(Product product)
        {
            Cart.UpdateLine(product, 1);
            Save();
        }

        public void Clear()
        {
            Cart.ClearCart();
            Save();
        }

        public void Update(Product product, int qty)
        {
            Cart.UpdateLine(product, qty);
            Save();
        }

        public void Remove(Product product)
        {
            Cart.RemoveLine(product);
            Save();
        }
    }

}