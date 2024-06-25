using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecretController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public SecretController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/Secret
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Secret>>> GetSecret()
        {
            return await _context.Secret.ToListAsync();
        }

        // GET: api/Secret/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Secret>> GetSecret(long id)
        {
            var secret = await _context.Secret.FindAsync(id);

            if (secret == null)
            {
                return NotFound();
            }

            return secret;
        }

        // PUT: api/Secret/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSecret(long id, Secret secret)
        {
            if (id != secret.Id)
            {
                return BadRequest();
            }

            _context.Entry(secret).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SecretExists(id))
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

        // POST: api/Secret
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Secret>> PostSecret(Secret secret)
        {
            _context.Secret.Add(secret);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSecret", new { id = secret.Id }, secret);
        }

        // DELETE: api/Secret/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSecret(long id)
        {
            var secret = await _context.Secret.FindAsync(id);
            if (secret == null)
            {
                return NotFound();
            }

            _context.Secret.Remove(secret);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SecretExists(long id)
        {
            return _context.Secret.Any(e => e.Id == id);
        }
    }
}
