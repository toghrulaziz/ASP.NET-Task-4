using ASP.NET_Task4.Data;
using ASP.NET_Task4.Helpers;
using ASP.NET_Task4.Models;
using ASP.NET_Task4.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASP.NET_Task4.Controllers
{
    [Route("product")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _appDbContext;

        public ProductController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        // GetAll +
        [Route("all")]
        public IActionResult Index()
        {
            var products = _appDbContext.Products.Include(p => p.Category).ToList();
            return View(products);
        }



        // Get +
        [Route("get/{id:int?}")]
        public async Task<IActionResult> Get(int id)
        {
            var product = await _appDbContext.Products.Include(p => p.Category).Include(p => p.Tags).FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }



        // Add +
        [Route("add")]
        public IActionResult Add()
        {
            var categories = _appDbContext.Categories.ToList();
            var tags = _appDbContext.Tags.ToList();
            ViewData["Categories"] = categories;
            ViewData["Tags"] = tags;
            return View();
        }

        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> Add(AddProductViewModel product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var tags = _appDbContext.Tags.Where(t => product.TagIds.Contains(t.Id)).ToList();

                    var newProduct = new Product
                    {
                        CategoryId = product.CategoryId,
                        Price = product.Price,
                        Description = product.Description,
                        Title = product.Title,
                        Tags = tags
                    };

                    if (product.ImageUrl != null)
                    {
                        newProduct.ImageUrl = await UploadFileHelper.UploadFile(product.ImageUrl);
                    }

                    _appDbContext.Products.Add(newProduct);
                    await _appDbContext.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(product);
                }
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "The file type is not accepted.";
                return View(product);
            }
        }


        // Delete +
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _appDbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is not null)
            {
                _appDbContext.Products.Remove(product);
                await _appDbContext.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }


        // Update +
        [Route("update/{id:int?}")]
        public async Task<IActionResult> Update(int id)
        {
            var product = await _appDbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == id);

            var categories = _appDbContext.Categories.ToList();
            var tags = _appDbContext.Tags.ToList();

            ViewData["Categories"] = categories;
            ViewData["Tags"] = tags;

            var editViewModel = new EditProductViewModel
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                Price = product.Price,
                Description = product.Description,
                ImageUrl = null,
                Title = product.Title,
                TagIds = product.Tags.Select(tag => tag.Id).ToList(),
            };

            return View(editViewModel);
        }




        [HttpPost]
        [Route("update/{id:int?}")]
        public async Task<IActionResult> Update(EditProductViewModel updatedProduct)
        {
            if (ModelState.IsValid)
            {
                var productToUpdate = await _appDbContext.Products
                    .Include(p => p.Tags)
                    .FirstOrDefaultAsync(p => p.Id == updatedProduct.Id);

                if (productToUpdate == null)
                {
                    return NotFound();
                }

                productToUpdate.CategoryId = updatedProduct.CategoryId;
                productToUpdate.Price = updatedProduct.Price;
                productToUpdate.Description = updatedProduct.Description;
                productToUpdate.ImageUrl = await UploadFileHelper.UploadFile(updatedProduct.ImageUrl);
                productToUpdate.Title = updatedProduct.Title;
                

                if (updatedProduct.TagIds != null && updatedProduct.TagIds.Any())
                {
                    productToUpdate.Tags.Clear();

                    var tags = await _appDbContext.Tags
                        .Where(tag => updatedProduct.TagIds.Contains(tag.Id))
                        .ToListAsync();

                    productToUpdate.Tags.AddRange(tags);
                }


                await _appDbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            else
            {
                return View(updatedProduct);
            }

        }



    }
}
