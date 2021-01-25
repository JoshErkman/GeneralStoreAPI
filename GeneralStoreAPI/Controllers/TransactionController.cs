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
                // Remove the items that were purchased
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

            // Update the customer Id if they are different
            if (transaction.CustomerId != updatedTransaction.CustomerId)
            {
                transaction.CustomerId = updatedTransaction.CustomerId;
            }

            // Check if product SKUs are different
            if (transaction.ProductSKU != updatedTransaction.ProductSKU)
            {
                // If the product skus are different add the item count for the original sku back to the inventory
                transaction.Product.NumberInInventory = transaction.Product.NumberInInventory + transaction.ItemCount;
                
                // change the product sku, itemcount, and number in inventory
                transaction.ProductSKU = updatedTransaction.ProductSKU;
                transaction.ItemCount = updatedTransaction.ItemCount;
                transaction.Product.NumberInInventory = updatedTransaction.Product.NumberInInventory;

                // subtract the updated product from the inventory
                transaction.Product.NumberInInventory = transaction.Product.NumberInInventory - transaction.ItemCount;
            }

            // Check if the product skus are the same
            if (transaction.ProductSKU == updatedTransaction.ProductSKU)
            {
                // If the skus are the same see if the item count is different and act accordingly
                if (transaction.ItemCount < updatedTransaction.ItemCount)
                {
                    int transactionDifference;
                    transactionDifference = updatedTransaction.ItemCount - transaction.ItemCount;
                    transaction.Product.NumberInInventory = transaction.Product.NumberInInventory - transactionDifference;
                }
                else if (transaction.ItemCount > updatedTransaction.ItemCount)
                {
                    int transactionDifference;
                    transactionDifference = transaction.ItemCount - updatedTransaction.ItemCount;
                    transaction.Product.NumberInInventory = transaction.Product.NumberInInventory + transactionDifference;
                }
                else { }
            }

            transaction.DateOfTransaction = updatedTransaction.DateOfTransaction;

            return Ok("Your transaction was updated.");
        }

        // DELETE
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteTransactionById([FromUri] int transactionId)
        {
            // find transaction by id
            Transaction transaction = await _context.Transactions.FindAsync(transactionId);

            // Check if transaction exist
            if (transaction is null)
                return BadRequest("There is no transaction by that Id");

            // Return product of the transaction to the inventory
            transaction.Product.NumberInInventory = transaction.Product.NumberInInventory + transaction.ItemCount;

            // Remove transaction
            _context.Transactions.Remove(transaction);

            // return action result
            return Ok("This transaction was deleted.");
        }



    }
}
