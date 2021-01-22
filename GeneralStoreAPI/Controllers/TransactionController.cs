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
    public class TransactionController : ApiController
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        // POST
        // api/Transaction
        [HttpPost]
        public async Task<IHttpActionResult> CreateTransaction([FromBody] Transaction transaction)
        {

            if (transaction is null)
            {
                return BadRequest("Your Request body cannot be empty");
            }

            // If transaction is valid
            if (ModelState.IsValid)
            {
                // Check if product is in stock
                if (!transaction.Product.IsInStock)
                    return BadRequest("The item you are trying to purchase is not in stock");

                // Check if there is enough inventory
                if (transaction.ItemCount > transaction.Product.NumberInInventory)
                {
                    return BadRequest("There is not enough of this product in the inventory.");
                }

                // Store the transaction in the database
                _context.Transactions.Add(transaction);
                if (await _context.SaveChangesAsync() == 1)
                {
                    transaction.Product.NumberInInventory = transaction.Product.NumberInInventory - transaction.ItemCount;
                }

                return Ok("Your Transaction was added to the database");
            }

            // The model is not valid, go ahead and reject it
            return BadRequest(ModelState);
        }

        // GET
        // api/Transaction
        [HttpGet]
        public async Task<IHttpActionResult> GetAllTransactions()
        {
            List<Transaction> transactions = await _context.Transactions.ToListAsync();
            return Ok(transactions);
        }

        // GET
        // api/Transaction/{id}
        [HttpGet]
        public async Task<IHttpActionResult> GetAllTransactionsByCustomerId([FromUri] int customerId)
        {
            Customer customer = await _context.Customers.FindAsync(customerId);
            List<Transaction> transactionList = await _context.Transactions.ToListAsync();

            List<Transaction> newTransactionList = new List<Transaction>();

            if (customer is null)
                return BadRequest("There was no customer found with that Id.");
            
            foreach (var transaction in transactionList)
            {
                if(customer.Id == transaction.CustomerId)
                {
                    newTransactionList.Add(transaction); 

                }
                
            }
            
            if (newTransactionList.Count > 0)
                return Ok(newTransactionList);  
          
            return BadRequest("There are no transactions for this customer.");
        }

        // GET
        [HttpGet]
        public async Task<IHttpActionResult> GetTransactionById([FromUri] int transactionId)
        {
            // find transaction by id
            Transaction transaction = await _context.Transactions.FindAsync(transactionId);

            // check if transaction exist
            if (transaction is null)
                return BadRequest("There is no transaction by that Id.");

            // if transaction exist, return Ok and transaction
            return Ok(transaction);
        }

        // PUT
        [HttpPut]
        public async Task<IHttpActionResult> UpdateTransactionById([FromUri] int transactionId, [FromBody] Transaction updatedTransaction)
        {
            // Check the model state
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check that the ids match
            if (transactionId != updatedTransaction?.Id)
                return BadRequest("The Ids do not match.");

            // Find The transaction in the database
            Transaction transaction = await _context.Transactions.FindAsync(transactionId);

            // If the transaction does not exist then do something
            if (transaction is null)
                return NotFound();

            // Update the properties
            transaction.CustomerId = updatedTransaction.CustomerId;
            transaction.ProductSKU = updatedTransaction.ProductSKU;
            transaction.ItemCount = updatedTransaction.ItemCount;
            transaction.DateOfTransaction = transaction.DateOfTransaction;

        }

        // DELETE
    }
}
