using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P133Allup.DataAccessLayer;
using P133Allup.Extentions;
using P133Allup.Helpers;
using P133Allup.Models;
using P133Allup.ViewModels;

namespace P133Allup.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index(int pageIndex=1)
        {
            IQueryable<Product> products = _context.Products.Where(p => p.IsDeleted == false);

            return View(PageNatedList<Product>.Create(products,pageIndex,3));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Brands = await _context.Brands.Where(b => b.IsDeleted == false).ToListAsync();
            ViewBag.Categories = await _context.Categories
                .Include(c=>c.Children.Where(c=>c.IsDeleted==false))
                .Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(t => t.IsDeleted == false).ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            ViewBag.Brands = await _context.Brands.Where(b => b.IsDeleted == false).ToListAsync();
            ViewBag.Categories = await _context.Categories
                .Include(c => c.Children.Where(c => c.IsDeleted == false))
                .Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(t => t.IsDeleted == false).ToListAsync();

            if (!ModelState.IsValid) return View(product);
            if (!await _context.Brands.AnyAsync(b=>b.IsDeleted==false && b.Id == product.BrandId))
            {
                ModelState.AddModelError("BrandId", $"Daxil olunan Brand Id {product.BrandId} yanlishdir");
                return View(product);
            }
            if (!await _context.Categories.AnyAsync(b => b.IsDeleted == false && b.Id == product.CategoryId))
            {
                ModelState.AddModelError("CategoryId", $"Daxil olunan Category Id {product.CategoryId} yanlishdir");
                return View(product);
            }

            if (product.TagIds != null && product.TagIds.Count()>0)
            {
                List<ProductTag> productTags = new List<ProductTag>();

                foreach (int tagId in product.TagIds)
                {
                    if (!await _context.Tags.AnyAsync(b => b.IsDeleted == false && b.Id == tagId))
                    {
                        ModelState.AddModelError("TagIds", $"Daxil olunan Tag Id {tagId} yanlishdir");
                        return View(product);
                    }
                    ProductTag productTag = new ProductTag
                    {
                        TagId = tagId,
                        CreatedAt = DateTime.UtcNow.AddHours(4),
                        CreatedBy = "System"

                    };
                    productTags.Add(productTag);
                }
                product.ProductTags = productTags;

            }
            else
            {
                ModelState.AddModelError("TagIds", "Mutleq secilmelidir");
                return View(product);
            }

            if (product.MainFile != null)
            {
                if (!product.MainFile.CheckFileContentType("image/jpeg"))
                {
                    ModelState.AddModelError("MainFile", "Main File Jpg olmalidir");
                    return View(product);
                }
                if (!product.MainFile.CheckFileLength(300))
                {

                    ModelState.AddModelError("MainFile", "Main File 300kb olmalidir");
                    return View(product);
                }
                product.MainImage = await product.MainFile.CraeteFileAsync(_env, "assets", "images","product");

            }
            else
            {
                ModelState.AddModelError("MainFile", "  File Mutleq Daxil Olmalidir");
                return View(product);
            }

            if (product.HoverFile != null)
            {
                if (!product.HoverFile.CheckFileContentType("image/jpeg"))
                {
                    ModelState.AddModelError("HoverFile", "Hover File Jpg olmalidir");
                    return View(product);
                }
                if (!product.HoverFile.CheckFileLength(300))
                {

                    ModelState.AddModelError("HoverFile", "Hover File 300kb olmalidir");
                    return View(product);
                }
                product.HoverImage = await product.HoverFile.CraeteFileAsync(_env, "assets", "images", "product");

            }
            else
            {
                ModelState.AddModelError("HoverFile", "Hover File Mutleq Daxil Olmalidir");
                return View(product);
            }

            if (product.Files == null)
            {
                ModelState.AddModelError("Files", "Wekil mutleq secilmelidir");
                return View(product);
            }

            if (product.Files.Count() > 6)
            {
                ModelState.AddModelError("Files", "Max 6 wekil ola biler");
                return View(product);

            }

            if (product.Files.Count()>0 )
            {
                List<ProductImage> productImages = new List<ProductImage>();
                foreach (IFormFile file in product.Files)
                {
                    if (!file.CheckFileContentType("image/jpeg"))
                    {
                        ModelState.AddModelError("Files", $"{file.FileName} Jpg olmalidir");
                        return View(product);
                    }
                    if (!file.CheckFileLength(300))
                    {

                        ModelState.AddModelError("Files", $"{file.FileName} 300kb olmalidir");
                        return View(product);
                    }
                    ProductImage productImage = new ProductImage
                    {
                        Image = await file.CraeteFileAsync(_env, "assets", "images", "product"),
                        CreatedAt = DateTime.UtcNow.AddHours(4),
                        CreatedBy = "System"

                    };
                    productImages.Add(productImage);
                }
                product.ProductImages = productImages;

            }

            string code = product.Title.Substring(0, 2);
            code = code + _context.Brands.FirstOrDefault(b => b.Id == product.BrandId).Name.Substring(0, 1);
            code = code + _context.Categories.FirstOrDefault(b => b.Id == product.CategoryId).Name.Substring(0, 1);
            product.Seria = code.ToLower().Trim();
            product.Code = _context.Products.Where(p => p.Seria == product.Seria).OrderByDescending(p => p.Id).FirstOrDefault() != null ?
                _context.Products.Where(p => p.Seria == product.Seria).OrderByDescending(p => p.Id).FirstOrDefault().Code + 1 : 1;

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }


        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();
            Product product = await _context.Products
                .Include(p=>p.ProductImages.Where(pi=>pi.IsDeleted==false))
                .Include(p=>p.ProductTags.Where(pt=>pt.IsDeleted==false))
                .FirstOrDefaultAsync(p => p.IsDeleted == false && p.Id == id);
            if (product == null) return NotFound();

            ViewBag.Brands = await _context.Brands.Where(b => b.IsDeleted == false).ToListAsync();
            ViewBag.Categories = await _context.Categories
                .Include(c => c.Children.Where(c => c.IsDeleted == false))
                .Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(t => t.IsDeleted == false).ToListAsync();

            product.TagIds = product.ProductTags != null && product.ProductTags.Count() > 0 ?
                product.ProductTags.Select(x => (byte)x.TagId).ToList(): new List<byte>();
            return View(product);
            


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Product product)
        {

            ViewBag.Brands = await _context.Brands.Where(b => b.IsDeleted == false).ToListAsync();
            ViewBag.Categories = await _context.Categories
                .Include(c => c.Children.Where(c => c.IsDeleted == false))
                .Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(t => t.IsDeleted == false).ToListAsync();

            if (!ModelState.IsValid)
            {
                return View();
            }
            if (id == null) return BadRequest();

            if (id != product.Id) return BadRequest();

            Product dbProduct = await _context.Products
                .Include(p=>p.ProductImages .Where(pi=>pi.IsDeleted==false))
                .Include(p=>p.ProductTags.Where(pt=>pt.IsDeleted==false ))
                .FirstOrDefaultAsync(p => p.IsDeleted == false && p.Id == id);

            if (dbProduct == null) return NotFound();

            int canUpload = 6 - dbProduct.ProductImages.Count();

            if ( product.Files!=null && canUpload < product.Files.Count())
            {
                ModelState.AddModelError("Files",$"Max {canUpload} qeder wekil yukleye bilersiz ");
                return View(product);
            }
            if (product.Files != null && product.Files.Count() > 0)
            {
                List<ProductImage> productImages = new List<ProductImage>();
                foreach (IFormFile file in product.Files)
                {
                    if (!file.CheckFileContentType("image/jpeg"))
                    {
                        ModelState.AddModelError("Files", $"{file.FileName} Jpg olmalidir");
                        return View(product);
                    }

                    if (!file.CheckFileLength(300))
                    {

                        ModelState.AddModelError("Files", $"{file.FileName} 300kb olmalidir");
                        return View(product);
                    }

                    ProductImage productImage = new ProductImage
                    {
                        Image = await file.CraeteFileAsync(_env, "assets", "images", "product"),
                        CreatedAt = DateTime.UtcNow.AddHours(4),
                        CreatedBy = "System"

                    };

                    productImages.Add(productImage);
                }
                product.ProductImages.AddRange(productImages);
            }

            if (product.MainFile!=null)
            {
                if (!product.MainFile.CheckFileContentType("image/jpeg"))
                {
                    ModelState.AddModelError("MainFile", "Main File Jpg olmalidir");
                    return View(product);
                }
                if (!product.MainFile.CheckFileLength(300))
                {

                    ModelState.AddModelError("MainFile", "Main File 300kb olmalidir");
                    return View(product);
                }


                FileHelper.DeleteFile(dbProduct.MainImage, _env, "assets", "images", "product");

                dbProduct.MainImage = await product.MainFile.CraeteFileAsync(_env, "assets", "images", "product");

            }

            if (product.HoverFile != null)
            {
                if (!product.HoverFile.CheckFileContentType("image/jpeg"))
                {
                    ModelState.AddModelError("HoverFile", "Hover File Jpg olmalidir");
                    return View(product);
                }
                if (!product.HoverFile.CheckFileLength(300))
                {

                    ModelState.AddModelError("HoverFile", "Hover File 300kb olmalidir");
                    return View(product);
                }
                FileHelper.DeleteFile(dbProduct.HoverImage, _env, "assets", "images", "product");

                dbProduct.HoverImage = await product.HoverFile.CraeteFileAsync(_env, "assets", "images", "product");

            }

            if (product.TagIds != null && product.TagIds.Count() > 0)
            {
                _context.ProductTags.RemoveRange(dbProduct.ProductTags);
                List<ProductTag> productTags = new List<ProductTag>();

                foreach (int tagId in product.TagIds)
                {
                    if (!await _context.Tags.AnyAsync(b => b.IsDeleted == false && b.Id == tagId))
                    {
                        ModelState.AddModelError("TagIds", $"Daxil olunan Tag Id {tagId} yanlishdir");
                        return View(product);
                    }
                    ProductTag productTag = new ProductTag
                    {
                        TagId = tagId,
                        CreatedAt = DateTime.UtcNow.AddHours(4),
                        CreatedBy = "System"

                    };
                    productTags.Add(productTag);
                }
                dbProduct.ProductTags = productTags;

            }
            else
            {
                ModelState.AddModelError("TagIds", "Mutleq secilmelidir");
                return View(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteImage(int? id, int? imageId)
        {

            if (id == null) return BadRequest();

            if (imageId == null) return BadRequest();
            Product product = await _context.Products
                .Include(p=>p.ProductImages.Where(pi=>pi.IsDeleted == false))
                .FirstOrDefaultAsync(p => p.IsDeleted == false && p.Id == id);


            if (product == null) return NotFound();

            if (product.ProductImages?.Count()<=1)
            {
                return BadRequest();
            }

            if (!product.ProductImages.Any(p => p.Id == imageId)) return BadRequest();
            product.ProductImages.FirstOrDefault(p => p.Id == imageId).IsDeleted = true;
            await _context.SaveChangesAsync();

            FileHelper.DeleteFile(product.ProductImages.FirstOrDefault(p => p.Id == imageId).Image,_env,"assets","images","product");
            List<ProductImage> productImages = product.ProductImages.Where(pi => pi.IsDeleted == false).ToList();
            return PartialView("_ProductImagePartial", productImages);


        }

    }
}

