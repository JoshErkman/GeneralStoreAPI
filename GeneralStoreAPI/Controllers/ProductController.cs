using GeneralStoreAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace GeneralStoreAPI.Controllers
{
    public class ProductController : ApiController
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        // POST
        // api/Product
        [HttpPost]
        public async Task<IHttpActionResult> CreateProduct([FromBody] Product product)
        {
            // If the product is valid
            if (ModelState.IsValid)
            {
                // Store the product in the database
                _context.Products.Add(product);

                // Save changes
                await _context.SaveChangesAsync();

                // Return 200 OK
                return Ok("The product was create");
            }

            // The product is not valid
            return BadRequest(ModelState);
        }

        // GET
        // api/Product
        [HttpGet]
        public async Task<IHttpActionResult> GetAllProducts()
        {
            List<Product> products = await _context.Products.ToListAsync();
            return Ok(products);
        }

        // GET
        // api/Product/{id}
        [HttpGet]
        public async Task<IHttpActionResult> GetProductById([FromUri] int id)
        {
            Product product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                return Ok(product);
            }

            return NotFound();
        }

        // PUT
        // api/Product/{id}
        [HttpPut]
        public async Task<IHttpActionResult> UpdateProductBySKU([FromUri] string sku, [FromBody] Product updatedProduct)
        {
            // Check if the IDs match
            if (sku != updatedProduct?.SKU)
            {
                return BadRequest("The SKUs do not match.");
            }

            // Check the model state
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Find the product in the database by its sku (primary key)
            Product product = await _context.Products.FindAsync(sku);

            // If the product does not exist then send not found
            if (product is null)
                return NotFound();

            // Update the properties of the product
            product.Name = updatedProduct.Name;
            product.Cost = updatedProduct.Cost;
            product.NumberInInventory = updatedProduct.NumberInInventory;

            // Save the changes
            await _context.SaveChangesAsync();

            return Ok("The product was updated.");   
        }

        // DELETE
        // api/Product/{id}
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteProductBySKU([FromUri] string sku)
        {

            // Find product
            Product product = await _context.Products.FindAsync(sku);

            // Check if product exist
            if (product is null)
                return NotFound();

            // If product extist, remove it
            _context.Products.Remove(product);

            // Save changes 
            if  (await _context.SaveChangesAsync() == 1)
            {
                return Ok("The product was deleted.");
            }

            return InternalServerError();
        }
    }
}
