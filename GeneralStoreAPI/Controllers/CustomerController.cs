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
    public class CustomerController : ApiController
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        // POST
        // api/Customer
        [HttpPost]
        public async Task<IHttpActionResult> CreateCustomer([FromBody]Customer customer)
        {
            // If the customer is valid
            if (ModelState.IsValid)
            {
                // Store the customer in the database
                _context.Customers.Add(customer);

                // save to database
                int changeCount = await _context.SaveChangesAsync();

                return Ok("The customer was created");
            }

            // The customer is not valid, go ahead and reject it
            return BadRequest(ModelState);
        }

        // GET
        // api/Customer
        [HttpGet]
        public async Task<IHttpActionResult> GetAllCustomers()
        {
            List<Customer> customers = await _context.Customers.ToListAsync();
            return Ok(customers);
        }

        // GET
        // api/Customer/{id}
        [HttpGet]
        public async Task<IHttpActionResult> GetCustomerById([FromUri] int id)
        {
            Customer customer = await _context.Customers.FindAsync(id);
            
            if (customer != null)
            {
                return Ok(customer);
            }

            return NotFound();
        }


        // PUT
        // api/Customer/{id}
        [HttpPut]
        public async Task<IHttpActionResult> UpdateCustomerById([FromUri] int id, [FromBody] Customer updatedCustomer)
        {
            // Check if the ids match
            if (id != updatedCustomer?.Id)
            {
                return BadRequest("The IDs do not match.");
            }

            // Check the model state  // go back to this part in the videos and study
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Find the customer in the database
            Customer customer = await _context.Customers.FindAsync(id);

            // If the customer does not exist then do something
            if (customer is null)
                return NotFound();

            // Update the customer properties
            customer.FirstName = updatedCustomer.FirstName;
            customer.LastName = updatedCustomer.LastName;

            // Save the changes
            await _context.SaveChangesAsync();

            return Ok("The customer was updated.");
        }

        // DELETE
        // api/Customer/{id}
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteCustomerById([FromUri] int id)
        {
            // Find Customer
            Customer customer = await _context.Customers.FindAsync(id);

            // Check if customer exists
            if (customer is null)
                return NotFound();

            // if customer exists, remove customer
            _context.Customers.Remove(customer);

            // check if customer was deleted and say so  ----> go back to this portion of the video and study
            if (await _context.SaveChangesAsync() == 1)
            {
                return Ok("The Customer was deleted from the database.");
            }

            return InternalServerError();
        }
    }
}
