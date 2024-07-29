using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailAddressController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public EmailAddressController(ApplicationDBContext context)
        {
            this._context = context;
        }

        // GET: api/EmailAddress/5
        [HttpGet("{address}")]
        [Authorize]
        public async Task<ActionResult<EmailAddress>> GetEmailAddress(string address)
        {
            var emailAddress = await this._context.EmailAddress.SingleOrDefaultAsync(ea => ea.Address == address);

            if (emailAddress == null)
            {
                return this.NotFound();
            }

            return emailAddress;
        }
    }
}
