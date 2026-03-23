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
    public class AvaliacoesController : Controller
    {
        private readonly ApplicationDbContext _context;                           // Campo para acessar o banco de dados e realizar operações CRUD nas avaliações
        private readonly UserManager<IdentityUser> _userManager;                  // Campo para gerenciar usuários e obter informações sobre o usuário logado

        public AvaliacoesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;                                                   // Injeção de dependência para acessar o banco de dados e realizar operações CRUD nas avaliações
            _userManager = userManager;                                          // Injeção de dependência para acessar o banco de dados e gerenciar usuários
        }

        public async Task<IActionResult> Index()
        {
            // Admin vê tudo, Proprietario vê só os seus
            if (User.IsInRole("Admin"))
            {
                var avaliacoes = await _context.Avaliacoes
                    .Include(a => a.Proprietario)   //carrega os dados do proprietário relacionado à avaliação, permitindo que as informações do proprietário sejam acessíveis na view, como nome, email, etc.
                    .Include(a => a.Bovino)        //carrega os dados do bovino relacionado à avaliação, permitindo que as informações do bovino sejam acessíveis na view, como nome, raça, etc.
                    .Include(a => a.Ambiente)      //carrega os dados do ambiente relacionado à avaliação, permitindo que as informações do ambiente sejam acessíveis na view, como localização, temperatura, umidade, etc.
                    .ToListAsync();

                return View(avaliacoes);
            }

            if (User.IsInRole("Proprietario"))
            {
                var userId = _userManager.GetUserId(User); // Obtém o ID do usuário logado

                // Filtra só as avaliacoes do proprietário logado
                if (userId != null)
                {
                    var avaliacoes = await _context.Avaliacoes
                        .Include(a => a.Bovino)    // carrega o bovino
                        .Include(a => a.Ambiente)  // carrega o ambiente
                        .Where(a => a.ProprietarioId == userId)
                        .ToListAsync();
                    return View(avaliacoes);
                }
            }

            return View(new List<Avaliacao>()); // Retorna uma lista vazia se não houver usuário logado ou se o usuário não for Proprietário
        }

        // GET: Avaliacoes/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var avaliacao = await _context.Avaliacoes     // Busca a avaliação com o ID especificado no banco de dados
                .FirstOrDefaultAsync(m => m.id == id);

            if (avaliacao == null) return NotFound();

            //  Proprietario só vê os seus
            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User); // Obtém o ID do usuário logado
                if (avaliacao.ProprietarioId != userId)   // Verifica se a avaliação pertence ao proprietário logado
                    return Forbid();
            }

            return View(avaliacao);
        }

        // GET: Avaliacoes/Create
        public async Task<IActionResult> CreateAsync()
        {
            var userId = _userManager.GetUserId(User);

            if (User.IsInRole("Admin"))
            {
                // Admin vê tudo
                var animais = await _context.Animais.ToListAsync();     // Admin vê todos os animais, sem filtro por proprietário
                var ambientes = await _context.Ambiente.ToListAsync(); // Admin vê todos os ambientes, sem filtro por proprietário

                ViewBag.Animais = new SelectList(animais, "Id", "Nome");
                ViewBag.Ambientes = new SelectList(ambientes, "id", "local");
            }
            else
            {
                // Proprietario vê só os seus
                var animais = await _context.Animais
                    .Where(a => a.ProprietarioId == userId) // Filtra os animais para incluir apenas aqueles cujo ProprietarioId corresponde ao ID do usuário logado, garantindo que o proprietário veja apenas seus próprios animais
                    .ToListAsync();

                var ambientes = await _context.Ambiente
                    .Where(a => a.ProprietarioId == userId) // Filtra os ambientes para incluir apenas aqueles cujo ProprietarioId corresponde ao ID do usuário logado, garantindo que o proprietário veja apenas seus próprios ambientes
                    .ToListAsync();

                ViewBag.Animais = new SelectList(animais, "Id", "Nome");
                ViewBag.Ambientes = new SelectList(ambientes, "id", "local");
            }

            return View();
        }

        // POST: Avaliacoes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,bovinoId,ambienteId,dataAvaliacao")] Avaliacao avaliacao)
        {
            if (ModelState.IsValid)
            {
                var ambiente = await _context.Ambiente.FindAsync(avaliacao.ambienteId); // Busca o ambiente selecionado para obter os dados de temperatura e umidade necessários para calcular o ITU

                if (ambiente == null)
                {
                    ModelState.AddModelError(string.Empty, "Ambiente não encontrado."); // Adiciona um erro de modelo se o ambiente não for encontrado
                    return View(avaliacao);
                }

                avaliacao.dataAvaliacao = DateTime.Now; // Define a data da avaliação como o momento atual

                double T = ambiente.temperatura; // Temperatura em °C

                //Garante que UR está em % (0-100). Se estiver em decimal (0-1), usa: ambiente.umidade * 100
                double UR = ambiente.umidade;

                // Fórmula do Índice de Temperatura e Umidade (ITU)
                double ITU = (1.8 * T + 32) - (0.55 - 0.0055 * UR) * (1.8 * T - 26.8);

                avaliacao.valorITU = Math.Round(ITU, 2);

                // Classificação do conforto térmico baseada no ITU
                avaliacao.resultado = ITU switch 
                {
                    <= 74 => "Conforto",
                    <= 78 => "Alerta",
                    <= 88 => "Perigo",
                    _ => "Emergência"
                };

                avaliacao.ProprietarioId = _userManager.GetUserId(User); // Atribui o ID do usuário logado como ProprietarioId da avaliação, garantindo que a avaliação seja associada ao proprietário correto

                _context.Add(avaliacao);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(avaliacao);
        }

        // GET: Avaliacoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var avaliacao = await _context.Avaliacoes.FindAsync(id); // Busca a avaliação com o ID especificado no banco de dados
            if (avaliacao == null)
            {
                return NotFound();
            }
            return View(avaliacao);
        }

        // POST: Avaliacoes/Edit

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,bovinoId,ambienteId,valorITU,resultado,dataAvaliacao")] Avaliacao avaliacao)
        {
            if (id != avaliacao.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // ✅ Preserva o ProprietarioId original
                    var original = await _context.Avaliacoes.AsNoTracking() // evita rastreamento para obter o valor original sem afetar o estado do contexto
                        .FirstOrDefaultAsync(a => a.id == id);

                    avaliacao.ProprietarioId = original?.ProprietarioId;  // mantém o proprietário original, mesmo que o formulário não o envie

                    _context.Update(avaliacao);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AvaliacaoExists(avaliacao.id))   // Verifica se a avaliação ainda existe
                        return NotFound();                 // Se não existir, retorna erro 404
                    else
                        throw;                            // Em caso de concorrência, relança a exceção para ser tratada globalmente ou logada
                }
                return RedirectToAction(nameof(Index));
            }
            return View(avaliacao);
        }

        // GET: Avaliacoes/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var avaliacao = await _context.Avaliacoes
                .FirstOrDefaultAsync(m => m.id == id);      // Busca a avaliação com o ID especificado no banco de dados para exibição na confirmação de exclusão

            if (avaliacao == null)
            {
                return NotFound();
            }

            return View(avaliacao);
        }

        // POST: Avaliacoes/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var avaliacao = await _context.Avaliacoes.FindAsync(id);  // Busca a avaliação com o ID especificado no banco de dados para exclusão
            if (avaliacao != null)
            {
                _context.Avaliacoes.Remove(avaliacao);               // Remove a avaliação do contexto, marcando-a para exclusão no banco de dados
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AvaliacaoExists(int id)
        {
            return _context.Avaliacoes.Any(e => e.id == id);
        }
    }
}
