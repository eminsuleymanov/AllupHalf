using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P133Allup.DataAccessLayer;
using P133Allup.Models;

namespace P133Allup.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ProductModal(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Product product = await _context.Products.Include(p=>p.ProductImages).FirstOrDefaultAsync(p => p.IsDeleted == false && p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return PartialView("_ModalPartial", product);
        }

        public async Task<IActionResult> Search(int? categoryId, string search)
        {
            if (categoryId != null && categoryId > 0)
            {
                if (!await _context.Categories.AnyAsync(c => c.Id == categoryId))
                {
                    return BadRequest();
                }
            }

            IEnumerable<Product> products = await _context.Products
                   .Where(p => p.IsDeleted == false && categoryId != null && categoryId > 0 ? p.CategoryId == categoryId : true &&
                   (p.Title.ToLower().Contains(search.ToLower()) || p.Brand.Name.ToLower().Contains(search.ToLower()))).ToListAsync();

            return PartialView("_SearchPartial", products);
        }

        //public async Task<IActionResult> ProductModal(int? id)
        //{
        //    Product product = await _context.Products.FirstOrDefaultAsync(p => p.IsDeleted == false && p.Id == id);

        //    return Json(product);
        //}
    }
}
