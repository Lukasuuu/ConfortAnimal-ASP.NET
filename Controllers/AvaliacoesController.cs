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
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AvaliacoesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Admin vê tudo, Proprietario vê só os seus
            if (User.IsInRole("Admin"))
            {
                var avaliacoes = await _context.Avaliacoes
                    .Include(a => a.Proprietario)
                    .Include(a => a.Bovino)
                    .Include(a => a.Ambiente)
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

        // GET: Avaliacoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var avaliacao = await _context.Avaliacoes
                .FirstOrDefaultAsync(m => m.id == id);
            if (avaliacao == null)
            {
                return NotFound();
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
                var animais = await _context.Animais.ToListAsync();
                var ambientes = await _context.Ambiente.ToListAsync();

                ViewBag.Animais = new SelectList(animais, "Id", "Nome");
                ViewBag.Ambientes = new SelectList(ambientes, "id", "local");
            }
            else
            {
                // Proprietario vê só os seus
                var animais = await _context.Animais
                    .Where(a => a.ProprietarioId == userId)
                    .ToListAsync();

                var ambientes = await _context.Ambiente
                    .Where(a => a.ProprietarioId == userId)
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
                var ambiente = await _context.Ambiente.FindAsync(avaliacao.ambienteId);

                if (ambiente == null)
                {
                    ModelState.AddModelError(string.Empty, "Ambiente não encontrado.");
                    return View(avaliacao);
                }

                avaliacao.dataAvaliacao = DateTime.Now;

                double T = ambiente.temperatura;
                // ⚠️ Garante que UR está em % (0-100). Se estiver em decimal (0-1), usa: ambiente.umidade * 100
                double UR = ambiente.umidade;

                // Fórmula de Thom (1959) — padrão para bovinos
                double ITU = (1.8 * T + 32) - (0.55 - 0.0055 * UR) * (1.8 * T - 26.8);

                avaliacao.valorITU = Math.Round(ITU, 2);

                // Classificação de Baêta & Souza (2010) — 4 categorias
                avaliacao.resultado = ITU switch
                {
                    <= 74 => "Conforto",
                    <= 78 => "Alerta",
                    <= 88 => "Perigo",
                    _ => "Emergência"
                };

                avaliacao.ProprietarioId = _userManager.GetUserId(User);

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

            var avaliacao = await _context.Avaliacoes.FindAsync(id);
            if (avaliacao == null)
            {
                return NotFound();
            }
            return View(avaliacao);
        }

        // POST: Avaliacoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                    _context.Update(avaliacao);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AvaliacaoExists(avaliacao.id))
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
            return View(avaliacao);
        }

        // GET: Avaliacoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var avaliacao = await _context.Avaliacoes
                .FirstOrDefaultAsync(m => m.id == id);
            if (avaliacao == null)
            {
                return NotFound();
            }

            return View(avaliacao);
        }

        // POST: Avaliacoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var avaliacao = await _context.Avaliacoes.FindAsync(id);
            if (avaliacao != null)
            {
                _context.Avaliacoes.Remove(avaliacao);
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
