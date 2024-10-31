using CustomerApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace CustomerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        //private static List<Customer> customers = new List<Customer>();

        /// <summary>
        /// 查詢所有客戶資料:
        /// </summary>
        /// <returns>目前客戶清單:</returns>
        /// <response code="200">顯示所有客戶.</response>
        /// <response code="401">沒有權限可開啟本頁.</response>
        /// <response code="404">尚無資料.</response>
        /// 
        [HttpGet]
        [Authorize(Roles = "User,Admin")]
         [SwaggerResponse(StatusCodes.Status200OK, "查詢成功", typeof(Customer))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "無查詢權限")]
        
        //[ProducesResponseType(typeof(IEnumerable<Customer>), 200)]
        //[ProducesResponseType(401)]
        //[ProducesResponseType(404)]
        //public ActionResult<IEnumerable<Customer>> Get()
        //{
        //    return customers;
        //}
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _context.Customers.ToListAsync();
        }

        /// <summary>
        /// 查詢客戶資料:
        /// </summary>
        /// <param name="id">要查詢的客戶ID:</param>
        /// <returns>該ID的客戶資料是:</returns>
        /// <response code="200">查詢到客戶資料.</response>
        /// <response code="401">沒有權限可開啟本頁.</response>
        /// <response code="404">尚無資料.</response>
        [HttpGet("{id}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(typeof(Customer), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        //public ActionResult<Customer> Get(int id)
        //{
        //    var customer = customers.FirstOrDefault(c => c.Id == id);
        //    if (customer == null)
        //    {
        //        return NotFound();
        //    }
        //    return customer;
        //}
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }

        /// <summary>
        /// 建立一位新客戶.
        /// </summary>
        /// <param name="customer">要建立的客戶資料內容.</param>
        /// <returns>已建立的客戶資料內容:</returns>
        /// <response code="201">新客戶添加成功.</response>
        /// <response code="400">無效的輸入值.</response>
        /// <response code="401">沒有權限可開啟本頁.</response>
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(typeof(Customer), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        //public ActionResult<Customer> Post([FromBody] Customer customer)
        //{
        //    customers.Add(customer);
        //    customer.Id = customers.Count + 1;
        //    return CreatedAtAction(nameof(Get), new { id = customer.Id }, customer);
        //}
        public async Task<ActionResult<Customer>> PostCustomer(Customer customer)
        {

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
        }
        /// <summary>
        /// 更改客戶資料:
        /// </summary>
        /// <param name="id">要更改的客戶 ID:</param>
        /// <param name="customer">要更改的客戶資料:</param>
        /// <returns>No content.</returns>
        /// <response code="204">客戶資料更改成功.</response>
        /// <response code="400">無效的輸入值.</response>
        /// <response code="401">沒有權限可開啟本頁.</response>
        /// <response code="404">無此ID資料.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        //public IActionResult Put(int id, [FromBody] Customer customer)
        //{
        //    var existingCustomer = customer.FirstOrDefault(c => c.Id == id);
        //    if (existingCustomer == null)
        //    {
        //        return NotFound();
        //    }
        //    existingCustomer.Name = customer.Name;
        //    existingCustomer.Birthday = customer.Birthday;
        //    existingCustomer.Gender = customer.Gender;
        //    existingCustomer.Address = customer.Address;
        //    existingCustomer.Phone = customer.Phone;
        //    existingCustomer.Note1 = customer.Note1;
        //    existingCustomer.Note2 = customer.Note2;

        //    return NoContent();
        //}
        public async Task<IActionResult> PutCustomer(int id, Customer customer)
        {
            if (id != customer.Id)
            {
                return BadRequest();
            }

            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// 刪除客戶資料.
        /// </summary>
        /// <param name="id">要刪除的客戶 ID:</param>
        /// <returns>No content.</returns>
        /// <response code="204">客戶資料刪除成功.</response>
        /// <response code="401">尚未通過認證.</response>
        /// <response code="403">權限不足.</response>
        /// <response code="404">無此資料.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary="刪除指定客戶",Description= "刪除指定客戶")]
        [SwaggerResponse(204, "刪除成功.", typeof(Customer))]
        [SwaggerResponse(401, "尚未通過認證.")]
        [SwaggerResponse(403, "沒有權限可執行本功能.")]
        [SwaggerResponse(404, "無此資料.")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        //public IActionResult Delete(int id)
        //{
        //    var customer = customers.FirstOrDefault(c => c.Id == id);
        //    if (customer == null)
        //    {
        //        return NotFound();
        //    }
        //    customers.Remove(customer);
        //    return NoContent();
        //}
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
    }
}
