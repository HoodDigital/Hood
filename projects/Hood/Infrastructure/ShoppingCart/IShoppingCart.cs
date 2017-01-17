using Hood.Models;
using Hood.Models.Api;

namespace Hood.Services
{
    public interface IShoppingCart
    {
        Cart Cart { get; set; }
        void Clear();
        void Add(Product product);
        void Update(Product product, int qty);
        void Remove(Product product);
    }
}