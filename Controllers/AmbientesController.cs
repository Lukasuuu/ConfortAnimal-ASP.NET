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
        private readonly UserManager<IdentityUser> _userManager; // Campo para gerenciar usuários e obter informações sobre o usuário logado

        public AmbientesController(ApplicationDbContext context, UserManager<IdentityUser> userManager) // Construtor do controlador, onde as dependências são injetadas
        {
            _context = context;
            _userManager = userManager;       // Injeção de dependência para acessar o banco de dados e gerenciar usuários
        }

        // GET: Ambientes
        public async Task<IActionResult> Index()
        {
            // Admin vê tudo, Proprietario vê só os seus
            if (User.IsInRole("Admin"))
            {
                var ambientes = await _context.Ambiente
                .Include(a => a.Proprietario)           //  carrega o utilizador associado a cada ambiente, permitindo acessar os dados do proprietário na view
                .ToListAsync();

                return View(ambientes);
            }

            
            var userId = _userManager.GetUserId(User); // Busca o ID do utilizador logado

            // Filtra só os ambientes do proprietário logado
            if (userId != null)
            {
                var ambientes = await _context.Ambiente
                    .Where(a => a.ProprietarioId == userId)  // Filtra os ambientes para incluir apenas aqueles cujo ProprietarioId corresponde ao ID do usuário logado, garantindo que o proprietário veja apenas seus próprios ambientes
                    .ToListAsync();

                return View(ambientes);
            }

            return View(new List<Ambiente>()); // Retorna uma lista vazia se não houver usuário logado ou se o usuário não for Proprietário
        }




        // GET: Ambientes/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ambiente = await _context.Ambiente          // Busca o ambiente com o ID especificado no banco de dados
                .FirstOrDefaultAsync(m => m.id == id);

            if (ambiente == null) return NotFound();

            // Proprietario só vê os seus
            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User); // Busca o ID do utilizador logado para verificar se ele é o proprietário do ambiente
                if (ambiente.ProprietarioId != userId)     // Verifica se o ambiente pertence ao proprietário logado, garantindo que um proprietário só possa acessar os detalhes de seus próprios ambientes
                    return Forbid();
            }

            return View(ambiente);
        }

        // GET: Ambientes/Create
        public async Task<IActionResult> CreateAsync()
        {
            return View();
        }

        // POST: Ambientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,temperatura,umidade,local,dataRegisto")] Ambiente ambiente)
        {
            if (ModelState.IsValid)
            {
                ambiente.ProprietarioId = _userManager.GetUserId(User); // Atribui o ID do usuário logado como ProprietarioId do ambiente, garantindo que o ambiente seja associado ao proprietário correto

                _context.Add(ambiente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ambiente);
        }

        // GET: Ambientes/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ambiente = await _context.Ambiente.FindAsync(id);  // Busca o ambiente com o ID especificado no banco de dados
            if (ambiente == null)
            {
                return NotFound();
            }
            return View(ambiente);
        }

        // POST: Ambientes/Edit
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
                    // Preserva o ProprietarioId original
                    var original = await _context.Ambiente.AsNoTracking()         // para não rastrear a entidade original, evitando conflitos de estado

                                         .FirstOrDefaultAsync(a => a.id == id);  // Busca o ambiente original do banco de dados

                    ambiente.ProprietarioId = original?.ProprietarioId;         // Mantém o ProprietarioId original, mesmo que o modelo editado não o inclua

                    _context.Update(ambiente);                                 // Atualiza o ambiente com os dados editados
                    await _context.SaveChangesAsync();                        // Salva as alterações no banco de dados
                }
                catch (DbUpdateConcurrencyException)                         // Captura a exceção de concorrência, que pode ocorrer se o ambiente tiver sido modificado ou excluído por outro processo
                {
                    if (!AmbienteExists(ambiente.id))                       // Verifica se o ambiente ainda existe no banco de dados

                        return NotFound();// Se o ambiente não existir mais, retorna NotFound 
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ambiente);
        }

        // GET: Ambientes/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ambiente = await _context.Ambiente
                .FirstOrDefaultAsync(m => m.id == id); // Busca o ambiente com o ID especificado no banco de dados

            if (ambiente == null)
            {
                return NotFound();
            }

            return View(ambiente);
        }

        // POST: Ambientes/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ambiente = await _context.Ambiente.FindAsync(id); // Busca o ambiente com o ID especificado no banco de dados
            if (ambiente != null)
            {
                _context.Ambiente.Remove(ambiente); // Remove o ambiente encontrado do contexto, marcando-o para exclusão
            }

            await _context.SaveChangesAsync();      // Salva as alterações no banco de dados, efetivando a exclusão do ambiente
            return RedirectToAction(nameof(Index)); // Redireciona o usuário de volta para a ação Index, que exibe a lista de ambientes
        }

        private bool AmbienteExists(int id)
        {
            return _context.Ambiente.Any(e => e.id == id); // Verifica se existe algum ambiente no banco de dados com o ID especificado, retornando true se existir ou false caso contrário
        }
    }
}
