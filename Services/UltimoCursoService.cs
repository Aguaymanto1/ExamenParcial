namespace ExamenParcial.Services;
using ExamenParcial.Models;
using Microsoft.AspNetCore.Http;
public class UltimoCursoService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UltimoCursoService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetUltimoCurso(int id, string nombre)
    {
        var session = _httpContextAccessor.HttpContext!.Session;
        session.SetInt32("UltimoCursoId", id);
        session.SetString("UltimoCursoNombre", nombre);
    }

    public (int? Id, string? Nombre) GetUltimoCurso()
    {
        var session = _httpContextAccessor.HttpContext!.Session;
        return (
            session.GetInt32("UltimoCursoId"),
            session.GetString("UltimoCursoNombre")
        );
    }
}
