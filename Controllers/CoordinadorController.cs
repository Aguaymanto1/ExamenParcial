using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExamenParcial.Data;
using ExamenParcial.Models;

namespace ExamenParcial.Controllers
{
    [Authorize(Roles = "Coordinador")]
    public class CoordinadorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoordinadorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Panel principal
        public async Task<IActionResult> Index()
        {
            var cursos = await _context.Cursos.ToListAsync();
            return View(cursos);
        }

        // Crear curso
        public IActionResult Crear() => View();

        [HttpPost]
        public async Task<IActionResult> Crear(Curso curso)
        {
            if (ModelState.IsValid)
            {
                _context.Cursos.Add(curso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        // Editar curso
        public async Task<IActionResult> Editar(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();
            return View(curso);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Curso curso)
        {
            if (ModelState.IsValid)
            {
                _context.Cursos.Update(curso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        // Desactivar curso
        public async Task<IActionResult> Desactivar(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();
            curso.Activo = false;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Listar matrículas por curso
        public async Task<IActionResult> Matriculas(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Matriculas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null) return NotFound();

            return View(curso);
        }

        // Confirmar matrícula
        public async Task<IActionResult> ConfirmarMatricula(int id)
        {
            var matricula = await _context.Matriculas.FindAsync(id);
            if (matricula == null) return NotFound();

            matricula.Estado = EstadoMatricula.Confirmada;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Matriculas), new { id = matricula.CursoId });
        }

        // Cancelar matrícula
        public async Task<IActionResult> CancelarMatricula(int id)
        {
            var matricula = await _context.Matriculas.FindAsync(id);
            if (matricula == null) return NotFound();

            matricula.Estado = EstadoMatricula.Cancelada;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Matriculas), new { id = matricula.CursoId });
        }
    }
}

