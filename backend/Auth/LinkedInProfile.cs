namespace Empleabilidad.Api.Auth;

/// Perfil mínimo devuelto por la frontera de LinkedIn (ILinkedInClient).
/// Sub = identificador estable del usuario en LinkedIn (clave natural de la cuenta).
///
/// El `Sub` se valida en la construcción: un perfil sin identidad estable no es un
/// perfil válido y no debe llegar nunca a la provisión (evitaría dar de alta una
/// cuenta con `linkedin_sub` vacío si el proveedor respondiera de forma defectuosa).
/// Validarlo aquí — y no en cada cliente — cubre toda implementación de la frontera.
/// El nombre SÍ es opcional: depende del scope concedido y se persiste como NULL.
public record LinkedInProfile
{
    public string Sub { get; }
    public string? Name { get; }

    public LinkedInProfile(string Sub, string? Name)
    {
        if (string.IsNullOrWhiteSpace(Sub))
            throw new ArgumentException(
                "El perfil de LinkedIn no trae un identificador estable (`sub`).", nameof(Sub));

        this.Sub = Sub;
        this.Name = Name;
    }

    public void Deconstruct(out string Sub, out string? Name)
    {
        Sub = this.Sub;
        Name = this.Name;
    }
}
