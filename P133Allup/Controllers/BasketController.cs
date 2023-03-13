using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using P133Allup.DataAccessLayer;
using P133Allup.Models;
using P133Allup.ViewModels.BasketViewModels;

namespace P133Allup.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;

        public BasketController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AddBasket(int? id)
        {
            if (id == null) { return BadRequest(); }

            if (!await _context.Products.AnyAsync(p => p.IsDeleted == false && p.Id == id)) { return NotFound(); }

            string basket = HttpContext.Request.Cookies["basket"];

            List<BasketVM> basketVMs = null;

            if (string.IsNullOrWhiteSpace(basket)) 
            {
                basketVMs = new List<BasketVM> 
                {
                    new BasketVM {Id = (int)id,Count = 1}
                };
            }
            else
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);

                if (basketVMs.Exists(b=>b.Id == id))
                {
                    basketVMs.Find(b => b.Id == id).Count += 1;
                }
                else
                {
                    basketVMs.Add(new BasketVM { Id =(int)id,Count = 1});
                }
            }

            basket = JsonConvert.SerializeObject(basketVMs);

            HttpContext.Response.Cookies.Append("basket", basket);

            foreach (BasketVM basketVM in basketVMs)
            {
                Product product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == basketVM.Id && p.IsDeleted == false);

                if (product != null)
                {
                    basketVM.ExTax = product.ExTax;
                    basketVM.Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;
                    basketVM.Title = product.Title;
                    basketVM.Image = product.MainImage;
                }
            }

            return PartialView("_BasketPartial",basketVMs);
        }

        public async Task<IActionResult> GetBasket()
        {
            return Json(JsonConvert.DeserializeObject<List<BasketVM>>(HttpContext.Request.Cookies["basket"]));
        }
    }
}
