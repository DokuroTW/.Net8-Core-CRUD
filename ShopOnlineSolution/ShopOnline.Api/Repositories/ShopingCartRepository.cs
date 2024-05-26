using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using ShopOnline.Api.Data;
using ShopOnline.Api.Entities;
using ShopOnline.Api.Repositories.Contracts;
using ShopOnline.Models.Dtos;

namespace ShopOnline.Api.Repositories
{
    public class ShopingCartRepository : IShopingCartRepository
    {
        //DI DbContext
        private readonly ShopOnlineDbContext shopOnlineDbContext;

        public ShopingCartRepository(ShopOnlineDbContext shopOnlineDbContext) 
        {
            this.shopOnlineDbContext = shopOnlineDbContext;
        }
        //Add Item in Cart
        private async Task<bool> CartItemExists(int cartId, int productId)
        {
            return await this.shopOnlineDbContext.CartItems.AnyAsync(c => c.CartId == cartId && c.ProductId == productId);
        }
        public async Task<CartItem> AddItem(CartItemToAddDto cartItemToAddDto)
        {
            if (await CartItemExists(cartItemToAddDto.CartId, cartItemToAddDto.ProductId) == false) 
            {
                var Item = await (from product in this.shopOnlineDbContext.Products
                                  where product.Id == cartItemToAddDto.ProductId
                                  select new CartItem
                                  {
                                      CartId = cartItemToAddDto.CartId,
                                      ProductId = product.Id,
                                      Qty = cartItemToAddDto.Qty
                                  }).SingleOrDefaultAsync();
                if (Item != null)
                {
                    var result = await this.shopOnlineDbContext.CartItems.AddAsync(Item);
                    await this.shopOnlineDbContext.SaveChangesAsync();
                    return result.Entity;
                }
            }
            return null;
        }

        //Delete Item in Cart
        public async Task<CartItem> DeleteItem(int id)
        {
            var item = await this.shopOnlineDbContext.CartItems.FindAsync(id);

            if (item != null)
            {
                this.shopOnlineDbContext.CartItems.Remove(item);
                await this.shopOnlineDbContext.SaveChangesAsync();
            }

            return item;

        }

        //Get Item in Cart
        public async Task<CartItem> GetItem(int id)
        {
            return await (from cart in this.shopOnlineDbContext.Carts
                                  join cartItem in this.shopOnlineDbContext.CartItems
                                  on cart.Id equals cartItem.CartId
                                  where cart.UserId == id
                                  select new CartItem
                                  {
                                      Id = cartItem.Id,
                                      ProductId = cartItem.ProductId,
                                      Qty = cartItem.Qty,
                                      CartId = cartItem.CartId
                                  }).SingleOrDefaultAsync();
        }

        //Get all items in Cart
        public async Task<IEnumerable<CartItem>> GetItems(int userId)
        {
            return await (from cart in this.shopOnlineDbContext.Carts
                                  join cartItem in this.shopOnlineDbContext.CartItems
                                  on cart.Id equals cartItem.CartId
                                  where cart.UserId == userId
                                  select new CartItem
                                  {
                                      Id = cartItem.Id,
                                      ProductId = cartItem.ProductId,
                                      Qty = cartItem.Qty,
                                      CartId = cartItem.CartId
                                  }).ToListAsync();
        }

        //Edit item in Cart
        public async Task<CartItem> UpdateQty(int id, CartItemQtyUpdateDto cartItemQtyUpdateDto)
        {
            var item = await this.shopOnlineDbContext.CartItems.FindAsync(id);

            if (item != null) 
            {
                item.Qty = cartItemQtyUpdateDto.Qty;
                await this.shopOnlineDbContext.SaveChangesAsync();
                return item;
            }
            return null;
        }
    }
}
