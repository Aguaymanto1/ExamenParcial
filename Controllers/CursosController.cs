using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExamenParcial.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ExamenParcial.Services;

using ExamenParcial.Models;

namespace ExamenParcial.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UltimoCursoService _ultimoCurso;
        private readonly CursoCacheService _cursoCache;

        public CursosController(ApplicationDbContext context, UserManager<IdentityUser> userManager, UltimoCursoService ultimoCurso, CursoCacheService cursoCache)
        {
            _context = context;
            _userManager = userManager;
            _ultimoCurso = ultimoCurso;
            _cursoCache = cursoCache;
            _ultimoCurso = ultimoCurso;
        }

        public async Task<IActionResult> Index(string nombre, int? minCreditos, int? maxCreditos, TimeSpan? inicio, TimeSpan? fin)
        {
            var query = _context.Cursos.Where(c => c.Activo).AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(c => c.Nombre.Contains(nombre));

            if (minCreditos.HasValue && minCreditos.Value >= 0)
                query = query.Where(c => c.Creditos >= minCreditos);

            if (maxCreditos.HasValue && maxCreditos.Value >= 0)
                query = query.Where(c => c.Creditos <= maxCreditos);

            if (inicio.HasValue && fin.HasValue && inicio < fin)
                query = query.Where(c => c.HorarioInicio >= inicio && c.HorarioFin <= fin);

            var cursos = await query.ToListAsync();
            return View(cursos);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id && c.Activo);
            if (curso == null) return NotFound();
            return View(curso);
        }
        [HttpPost]
        [Authorize] // Solo usuarios autenticados pueden inscribirse
        public async Task<IActionResult> Inscribirse(int cursoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "Debes iniciar sesión para inscribirte.";
                return RedirectToAction("Detalle", new { id = cursoId });
            }

            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == cursoId && c.Activo);
            if (curso == null)
            {
                TempData["Error"] = "El curso no existe o no está activo.";
                return RedirectToAction("Index");
            }

            // Validaciones
            var inscritos = await _context.Matriculas.CountAsync(m => m.CursoId == cursoId && m.Estado != EstadoMatricula.Cancelada);
            if (inscritos >= curso.CupoMaximo)
            {
                TempData["Error"] = "No hay cupos disponibles para este curso.";
                return RedirectToAction("Detalle", new { id = cursoId });
            }

            //horarios
            var misMatriculas = await _context.Matriculas
                .Include(m => m.Curso)
                .Where(m => m.UsuarioId == user.Id && m.Estado != EstadoMatricula.Cancelada)
                .ToListAsync();

            bool solapado = misMatriculas.Any(m =>
                (curso.HorarioInicio < m.Curso.HorarioFin && curso.HorarioFin > m.Curso.HorarioInicio)
            );

            if (solapado)
            {
                TempData["Error"] = "Ya tienes una matrícula en un curso que se cruza en horario.";
                return RedirectToAction("Detalle", new { id = cursoId });
            }

            //no duplicar matricula
            bool yaMatriculado = await _context.Matriculas.AnyAsync(m => m.UsuarioId == user.Id && m.CursoId == cursoId && m.Estado != EstadoMatricula.Cancelada);
            if (yaMatriculado)
            {
                TempData["Error"] = "Ya estás matriculado en este curso.";
                return RedirectToAction("Detalle", new { id = cursoId });
            }

            // crear estado pedneite
            var matricula = new Matricula
            {
                CursoId = cursoId,
                UsuarioId = user.Id,
                FechaRegistro = DateTime.Now,
                Estado = EstadoMatricula.Pendiente
            };

            _context.Matriculas.Add(matricula);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Matrícula registrada en estado Pendiente.";
            return RedirectToAction("Detalle", new { id = cursoId });
        }
        public async Task<IActionResult> Index(string? nombre, int? minCreditos, int? maxCreditos)
        {
            var cursos = await _cursoCache.GetCursosActivosAsync();

            if (!string.IsNullOrEmpty(nombre))
                cursos = cursos.Where(c => c.Nombre.Contains(nombre)).ToList();

            if (minCreditos.HasValue && minCreditos.Value >= 0)
                cursos = cursos.Where(c => c.Creditos >= minCreditos).ToList();

            if (maxCreditos.HasValue && maxCreditos.Value >= 0)
                cursos = cursos.Where(c => c.Creditos <= maxCreditos).ToList();

            return View(cursos);
        }



        public async Task<IActionResult> Create(Curso curso)
        {
            if (ModelState.IsValid)
            {
                _context.Cursos.Add(curso);
                await _context.SaveChangesAsync();

                await _cursoCache.InvalidateCacheAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Curso curso)
        {
            if (curso == null) return BadRequest();

            if (id != curso.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(curso);
                await _context.SaveChangesAsync();

                await _cursoCache.InvalidateCacheAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }
    }
}
