using Empleabilidad.Api.Auth;
using Xunit;

namespace Backend.Tests;

// Invariante de la frontera externa (gate `data`, desviación D1): el `sub` es la
// clave natural de la cuenta. Un perfil sin identidad estable NO es un perfil
// válido y debe rechazarse en la frontera, antes de tocar la provisión — si no,
// un proveedor defectuoso podría dar de alta una cuenta con `linkedin_sub = ''`.
// Se valida en el propio tipo para cubrir TODA implementación de ILinkedInClient.
public class LinkedInProfileTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Perfil_sin_sub_estable_es_rechazado(string? sub)
    {
        Assert.Throws<ArgumentException>(() => new LinkedInProfile(sub!, "Ana Torres"));
    }

    [Fact]
    public void Perfil_con_sub_valido_se_construye()
    {
        var profile = new LinkedInProfile("linkedin-sub-123", "Ana Torres");

        Assert.Equal("linkedin-sub-123", profile.Sub);
        Assert.Equal("Ana Torres", profile.Name);
    }

    // El nombre SÍ es opcional: LinkedIn puede no entregarlo según el scope
    // concedido, y eso no invalida la identidad (se persiste como NULL).
    [Fact]
    public void Perfil_sin_nombre_es_valido()
    {
        var profile = new LinkedInProfile("linkedin-sub-123", null);

        Assert.Null(profile.Name);
    }
}
