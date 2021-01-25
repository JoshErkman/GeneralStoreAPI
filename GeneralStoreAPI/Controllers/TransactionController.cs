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
            var customer = await _context.Customers.FindAsync(transaction.CustomerId);
            var product = await _context.Products.FindAsync(transaction.ProductSKU);

            // Check if transaction is null
            if (transaction is null)
            {
                return BadRequest("Your Request body cannot be empty");
            }

            // Check if customer is null
            if (customer is null)
            {
                return BadRequest("customer is null");
            }

            // If transaction is valid
            if (ModelState.IsValid)
            {
                // Check if product is in stock
                if (!product.IsInStock)
                    return BadRequest("The item you are trying to purchase is not in stock");

                // Check if there is enough inventory
                if (transaction.ItemCount > product.NumberInInventory)
                {
                    return BadRequest("There is not enough of this product in the inventory.");
                }

                // Store the transaction in the database
                // Remove the items that were purchased
                _context.Transactions.Add(transaction);
                if (await _context.SaveChangesAsync() == 1)
                {
                    product.NumberInInventory = product.NumberInInventory - transaction.ItemCount;
                }

                // Return Ok action result
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
            // crreate list of transactions 
            List<Transaction> transactions = await _context.Transactions.ToListAsync();

            // Return Ok action result
            return Ok(transactions);
        }

        // GET
        // api/Transaction?customerId={customerId}
        [HttpGet]
        public async Task<IHttpActionResult> GetAllTransactionsByCustomerId([FromUri] int customerId)
        {
            // Find customer by Id
            Customer customer = await _context.Customers.FindAsync(customerId);

            // Create list to hold all transactions
            List<Transaction> transactionList = await _context.Transactions.ToListAsync();

            // New up a list to hold all the transactions with the given customerId
            List<Transaction> newTransactionList = new List<Transaction>();

            // Check if customer is null
            if (customer is null)
                return BadRequest("There was no customer found with that Id.");
            
            // For each through each transaction in transaction list
            foreach (var transaction in transactionList)
            {
                // check if the customer Id matches up with the customer Id in the transaction
                if(customer.Id == transaction.CustomerId)
                {
                    // add transaction to the new transaction list
                    newTransactionList.Add(transaction); 
                }
            }
            
            // Check if there were in transactions with that customer id... if so return Ok action result
            if (newTransactionList.Count > 0)
                return Ok(newTransactionList);  
            
            // If no transactions were found with given customerId return Bad action result
            return BadRequest("There are no transactions for this customer.");
        }

        // GET
        // api/Transaction?transactionId={transactionId}
        [HttpGet]
        public async Task<IHttpActionResult> GetTransactionById([FromUri] int transactionId)
        {
            // find transaction by id
            Transaction transaction = await _context.Transactions.FindAsync(transactionId);

            // check if transaction exist
            if (transaction is null)
                return BadRequest("There is no transaction by that Id.");

            // if transaction exist, return Ok action result and transaction
            return Ok(transaction);
        }

        // PUT
        // api/Transaction?transactionId={transactionId}
        [HttpPut]
        public async Task<IHttpActionResult> UpdateTransactionById([FromUri] int transactionId, [FromBody] Transaction updatedTransaction)
        {
            var product = await _context.Products.FindAsync(updatedTransaction.ProductSKU);
            var customer = await _context.Customers.FindAsync(updatedTransaction.CustomerId);

            // Check the model state
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check that the ids match
            if (transactionId != updatedTransaction?.Id)
                return BadRequest("The Ids do not match.");

            // Find The original transaction in the database
            Transaction transaction = await _context.Transactions.FindAsync(transactionId);
            var originalProduct = await _context.Products.FindAsync(transaction.ProductSKU);

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
                originalProduct.NumberInInventory += transaction.ItemCount;
                
                // change the product sku, itemcount, and number in inventory
                transaction.ProductSKU = updatedTransaction.ProductSKU;
                transaction.ItemCount = updatedTransaction.ItemCount;
                product.NumberInInventory -= updatedTransaction.ItemCount;
            }

            // Check if the product skus are the same
            if (transaction.ProductSKU == updatedTransaction.ProductSKU)
            {
                // If the skus are the same see if the item count is different and act accordingly
                if (transaction.ItemCount < updatedTransaction.ItemCount)
                {
                    int transactionDifference;
                    transactionDifference = updatedTransaction.ItemCount - transaction.ItemCount;
                    product.NumberInInventory -= transactionDifference;
                }
                else
                {
                    int transactionDifference;
                    transactionDifference = transaction.ItemCount - updatedTransaction.ItemCount;
                    product.NumberInInventory += transactionDifference;
                }
            }

            transaction.DateOfTransaction = updatedTransaction.DateOfTransaction;

            await _context.SaveChangesAsync();

            return Ok("Your transaction was updated.");
        }

        // DELETE
        // api/Transaction?transactionId={transactionId}
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

            await _context.SaveChangesAsync();

            // return action result
            return Ok("This transaction was deleted.");
        }
    }
}
