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
        private readonly UserManager<IdentityUser> _userManager;         // Campo para gerenciar usuários e obter informações sobre o usuário logado

        public AnimaisController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;                                 // Injeção de dependência para acessar o banco de dados e gerenciar usuários
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
            if (id == null) return NotFound();                 // Verifica se o ID foi fornecido

            var animal = await _context.Animais
                .OfType<Bovino>()
                .Include(a => a.Proprietario)
                .FirstOrDefaultAsync(m => m.Id == id); // Busca o animal com o ID especificado no banco de dados, incluindo os dados do proprietário

            if (animal == null) return NotFound();

            // Proprietario só vê os seus
            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User); // Obtém o ID do usuário logado
                if (animal.ProprietarioId != userId)       // Verifica se o animal pertence ao proprietário logado
                    return Forbid();
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
                    // Preserva o ProprietarioId original
                    var original = await _context.Animais.AsNoTracking() // evita que o EF Core rastreie a entidade original, permitindo apenas ler o ProprietarioId sem afetar o estado do contexto
                        .FirstOrDefaultAsync(a => a.Id == id);
                    animal.ProprietarioId = original?.ProprietarioId;

                    _context.Update(animal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)  // captura erros de concorrência (ex: outro usuário editou o mesmo registro)
                {
                    if (!AnimalExists(animal.Id))    // verifica se o animal ainda existe no banco de dados
                    {
                        return NotFound();            // se o animal foi deletado por outro usuário, retorna NotFound
                    }
                    else
                    {
                        throw;                       // se o animal existe, mas ocorreu um erro de concorrência, relança a exceção para ser tratada globalmente
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
                .FirstOrDefaultAsync(m => m.Id == id);          // Busca o animal com o ID especificado no banco de dados

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
            var animal = await _context.Animais.FindAsync(id);  // Busca o animal com o ID especificado no banco de dados
            if (animal != null)
            {
                _context.Animais.Remove(animal);               // Remove o animal do contexto, marcando-o para exclusão
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