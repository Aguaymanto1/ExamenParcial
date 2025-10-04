using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExamenParcial.Data;
using ExamenParcial.Models;

namespace ExamenParcial.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CursosController(ApplicationDbContext context)
        {
            _context = context;
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
    }
}
