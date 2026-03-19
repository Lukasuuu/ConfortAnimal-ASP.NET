using ConfortAnimal.Data;
using ConfortAnimal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ConfortAnimal.Controllers
{
    [Authorize(Roles = "Admin,Proprietario")]
    public class AmbientesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AmbientesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Ambientes
        public async Task<IActionResult> Index()
        {
            // Admin vê tudo, Proprietario vê só os seus
            if (User.IsInRole("Admin"))
            {
                var ambientes = await _context.Ambiente
                .Include(a => a.Proprietario) //  carrega o utilizador
                .ToListAsync();

                return View(ambientes);
            }

            
            var userId = _userManager.GetUserId(User); // Busca o ID do utilizador logado

            // Filtra só os ambientes do proprietário logado
            if (userId != null)
            {
                var ambientes = await _context.Ambiente
                    .Where(a => a.ProprietarioId == userId)
                    .ToListAsync();

                return View(ambientes);
            }

            return View(new List<Ambiente>()); // Retorna uma lista vazia se não houver usuário logado ou se o usuário não for Proprietário
        }

       


        // GET: Ambientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ambiente = await _context.Ambiente
                .FirstOrDefaultAsync(m => m.id == id);
            if (ambiente == null)
            {
                return NotFound();
            }

            return View(ambiente);
        }

        // GET: Ambientes/Create
        public async Task<IActionResult> CreateAsync()
        {
            return View();
        }

        // POST: Ambientes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,temperatura,umidade,local,dataRegisto")] Ambiente ambiente)
        {
            if (ModelState.IsValid)
            {
                ambiente.ProprietarioId = _userManager.GetUserId(User);

                _context.Add(ambiente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ambiente);
        }

        // GET: Ambientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ambiente = await _context.Ambiente.FindAsync(id);
            if (ambiente == null)
            {
                return NotFound();
            }
            return View(ambiente);
        }

        // POST: Ambientes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,temperatura,umidade,local,dataRegisto")] Ambiente ambiente)
        {
            if (id != ambiente.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ambiente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AmbienteExists(ambiente.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ambiente);
        }

        // GET: Ambientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ambiente = await _context.Ambiente
                .FirstOrDefaultAsync(m => m.id == id);
            if (ambiente == null)
            {
                return NotFound();
            }

            return View(ambiente);
        }

        // POST: Ambientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ambiente = await _context.Ambiente.FindAsync(id);
            if (ambiente != null)
            {
                _context.Ambiente.Remove(ambiente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AmbienteExists(int id)
        {
            return _context.Ambiente.Any(e => e.id == id);
        }
    }
}
