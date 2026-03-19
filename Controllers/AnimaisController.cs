using ConfortAnimal.Data;
using ConfortAnimal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConfortAnimal.Controllers
{
    [Authorize(Roles = "Admin,Proprietario")]
    public class AnimaisController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AnimaisController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Animais
        public async Task<IActionResult> Index()
        {
            // Admin vê tudo, Proprietario vê só os seus
            if (User.IsInRole("Admin"))
            {
                var animais = await _context.Animais
                    .OfType<Bovino>()              // filtra só Bovinos na tabela Animais (herança)
                    .Include(a => a.Proprietario)  // carrega o utilizador associado
                    .ToListAsync();

                return View(animais);
            }

            var userId = _userManager.GetUserId(User); // Busca o ID do utilizador logado

            // Filtra só os animais do proprietário logado
            if (userId != null)
            {
                var animais = await _context.Animais
                    .OfType<Bovino>()              // filtra só Bovinos na tabela Animais (herança)
                    .Where(a => a.ProprietarioId == userId)
                    .ToListAsync();

                return View(animais);
            }

            return View(new List<Bovino>()); // Retorna lista vazia se não houver utilizador logado
        }

        // GET: Animais/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animal = await _context.Animais
                .FirstOrDefaultAsync(m => m.Id == id); // Busca o animal pelo ID

            if (animal == null)
            {
                return NotFound();
            }

            return View(animal);
        }

        // GET: Animais/Create
        public IActionResult Create()
        {
            return View(new Bovino()); // instancia Bovino para o Discriminator ficar correto na BD
        }

        // POST: Animais/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,Peso,Idade,Raca,ProdutividadeLeite")] Bovino animal)
        {
            if (ModelState.IsValid) // Verifica se os dados são válidos
            {
                animal.ProprietarioId = _userManager.GetUserId(User); // Atribui o ID do utilizador logado como proprietário

                _context.Add(animal);              // Adiciona o bovino ao contexto
                await _context.SaveChangesAsync(); // Guarda na base de dados
                return RedirectToAction(nameof(Index));
            }
            return View(animal);
        }

        // GET: Animais/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animal = await _context.Animais
                .OfType<Bovino>()                        // filtra só Bovinos — necessário para carregar Raca e ProdutividadeLeite
                .FirstOrDefaultAsync(a => a.Id == id);

            if (animal == null)
            {
                return NotFound();
            }
            return View(animal);
        }

        // POST: Animais/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Peso,Idade,Raca,ProdutividadeLeite")] Bovino animal)
        {
            if (id != animal.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Preserva o ProprietarioId original — o formulário não envia este campo
                    // sem isto o ProprietarioId ficaria null após o Edit
                    var original = await _context.Animais.AsNoTracking()
                        .FirstOrDefaultAsync(a => a.Id == id);
                    animal.ProprietarioId = original?.ProprietarioId;

                    _context.Update(animal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimalExists(animal.Id))
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
            return View(animal);
        }

        // GET: Animais/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animal = await _context.Animais
                .FirstOrDefaultAsync(m => m.Id == id);
            if (animal == null)
            {
                return NotFound();
            }

            return View(animal);
        }

        // POST: Animais/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var animal = await _context.Animais.FindAsync(id);
            if (animal != null)
            {
                _context.Animais.Remove(animal);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Verifica se um animal existe pelo ID — usado no tratamento de erros do Edit
        private bool AnimalExists(int id)
        {
            return _context.Animais.Any(e => e.Id == id);
        }
    }
}