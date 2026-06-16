using DeskMatch.CoreService.Domain.Companies;
using DeskMatch.CoreService.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace DeskMatch.CoreService.Infrastructure.Persistence;

/// <summary>
/// Carga datos iniciales en Development.
/// Idempotente: usa GUIDs fijos y verifica existencia antes de insertar.
///
/// Para ejecutar manualmente vía dotnet:
///   ASPNETCORE_ENVIRONMENT=Development dotnet run --project apps/core-service
///
/// Para ejecutar via Docker:
///   docker compose exec core-service dotnet run   (el seed corre al iniciar)
///
/// Para verificar en psql:
///   SELECT COUNT(*) FROM core."Companies";
///   SELECT COUNT(*) FROM core."Workspaces";
///   SELECT "Name","City","PricePerHour","Capacity" FROM core."Workspaces" ORDER BY "Name";
/// </summary>
public static class DatabaseSeeder
{
    // GUIDs fijos → idempotencia garantizada
    private static readonly Guid C1 = Guid.Parse("a1000000-0000-0000-0000-000000000001");
    private static readonly Guid C2 = Guid.Parse("a1000000-0000-0000-0000-000000000002");
    private static readonly Guid C3 = Guid.Parse("a1000000-0000-0000-0000-000000000003");
    private static readonly Guid C4 = Guid.Parse("a1000000-0000-0000-0000-000000000004");
    private static readonly Guid C5 = Guid.Parse("a1000000-0000-0000-0000-000000000005");

    // Owners de las empresas seed
    private static readonly Guid RogelioId = Guid.Parse("0673690c-4264-48db-b8e7-4258cca9445a"); // rogelio@datastar.com.ar
    private static readonly Guid Owner4 = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-000000000004");
    private static readonly Guid Owner5 = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-000000000005");

    public static async Task SeedAsync(CoreDbContext db)
    {
        // Si alguna empresa seed ya existe, no volvemos a insertar nada
        if (await db.Companies.AnyAsync(c => c.Id == C1))
            return;

        var companies = BuildCompanies();
        var workspaces = BuildWorkspaces();

        await db.Companies.AddRangeAsync(companies);
        await db.Workspaces.AddRangeAsync(workspaces);
        await db.SaveChangesAsync();
    }

    // ──────────────────────────────────────────────────────── Companies ──

    private static List<Company> BuildCompanies() =>
    [
        new Company(C1)
        {
            OwnerId = RogelioId,
            Name = "Palermo Hub",
            Description = "Espacios modernos de trabajo en el corazón de Palermo. WiFi de alta velocidad, café incluido y comunidad de profesionales.",
            ContactEmail = "hola@palermohub.com.ar",
            PhoneNumber = "+54 11 4555-1200",
            Location = "Palermo, Buenos Aires, Argentina",
            WebsiteUrl = "https://palermohub.com.ar",
            IsVerified = true,
            IsActive = true,
        },
        new Company(C2)
        {
            OwnerId = RogelioId,
            Name = "TechSpace BA",
            Description = "Coworking enfocado en tecnología y startups. Ambiente colaborativo, salas equipadas para videoconferencias y eventos.",
            ContactEmail = "info@techspaceba.com",
            PhoneNumber = "+54 11 4300-9900",
            Location = "San Telmo, Buenos Aires, Argentina",
            WebsiteUrl = "https://techspaceba.com",
            IsVerified = true,
            IsActive = true,
        },
        new Company(C3)
        {
            OwnerId = RogelioId,
            Name = "Corporate Center BA",
            Description = "Oficinas y salas de reunión en el microcentro porteño. Ideal para empresas que necesitan presencia en el centro financiero.",
            ContactEmail = "reservas@corporatecenterba.com",
            PhoneNumber = "+54 11 4500-7700",
            Location = "Microcentro, Buenos Aires, Argentina",
            WebsiteUrl = "https://corporatecenterba.com",
            IsVerified = true,
            IsActive = true,
        },
        new Company(C4)
        {
            OwnerId = Owner4,
            Name = "Belgrano Business",
            Description = "Espacios premium en Belgrano con recepción dedicada, estacionamiento privado y todas las comodidades ejecutivas.",
            ContactEmail = "contact@belgranobusiness.com.ar",
            PhoneNumber = "+54 11 4788-4400",
            Location = "Belgrano, Buenos Aires, Argentina",
            WebsiteUrl = "https://belgranobusiness.com.ar",
            IsVerified = true,
            IsActive = true,
        },
        new Company(C5)
        {
            OwnerId = Owner5,
            Name = "Rosario Cowork",
            Description = "El espacio de coworking más grande de Rosario. Tarifas accesibles, 24 horas, ideal para freelancers y equipos remotos.",
            ContactEmail = "hola@rosariocowork.com",
            PhoneNumber = "+54 341 555-8800",
            Location = "Rosario, Santa Fe, Argentina",
            WebsiteUrl = "https://rosariocowork.com",
            IsVerified = true,
            IsActive = true,
        },
    ];

    // ──────────────────────────────────────────────────────── Workspaces ──

    private static List<Workspace> BuildWorkspaces() =>
    [
        // ── Palermo Hub (C1) ──────────────────────────────────────────
        new Workspace(Guid.Parse("b2000000-0000-0000-0000-000000000001"))
        {
            CompanyId   = C1,
            Name        = "Oficina privada para 2 · Palermo",
            Description = "Oficina silenciosa con escritorios ergonómicos, luz natural y acceso privado. Perfecta para duplas que necesitan concentración.",
            Address     = "Gurruchaga 1455, Palermo",
            City        = "Buenos Aires",
            Country     = "Argentina",
            Latitude    = -34.5875,
            Longitude   = -58.4333,
            Capacity    = 2,
            PricePerHour = 25m,
            PricePerDay  = 150m,
            Amenities   = ["WiFi", "AC", "Café", "Escritorios individuales", "Sillas ergonómicas"],
            Images      = ["https://images.unsplash.com/photo-1593642632559-0c6d3fc62b89?w=800&q=80&auto=format&fit=crop"],
            IsActive    = true,
            Rating      = 4.7,
            ReviewCount = 18,
        },
        new Workspace(Guid.Parse("b2000000-0000-0000-0000-000000000002"))
        {
            CompanyId   = C1,
            Name        = "Sala de reunión ejecutiva · 8 personas",
            Description = "Sala equipada con TV 65\", videoconferencia, pizarra y servicio de café. Ideal para reuniones de equipo y presentaciones.",
            Address     = "Gurruchaga 1455, Palermo",
            City        = "Buenos Aires",
            Country     = "Argentina",
            Latitude    = -34.5875,
            Longitude   = -58.4333,
            Capacity    = 8,
            PricePerHour = 55m,
            PricePerDay  = 320m,
            Amenities   = ["WiFi", "AC", "Café", "TV", "Videoconferencia", "Pizarra", "Recepción"],
            Images      = ["https://images.unsplash.com/photo-1524758631624-e2822e304c36?w=800&q=80&auto=format&fit=crop"],
            IsActive    = true,
            Rating      = 4.9,
            ReviewCount = 42,
        },
        new Workspace(Guid.Parse("b2000000-0000-0000-0000-000000000003"))
        {
            CompanyId   = C1,
            Name        = "Box privado · Silencioso",
            Description = "Box individual completamente cerrado para trabajo de concentración máxima. Sin interrupciones, con escritorio y cargadores USB.",
            Address     = "Gurruchaga 1455, Palermo",
            City        = "Buenos Aires",
            Country     = "Argentina",
            Latitude    = -34.5875,
            Longitude   = -58.4333,
            Capacity    = 1,
            PricePerHour = 10m,
            PricePerDay  = 60m,
            Amenities   = ["WiFi", "AC", "Escritorios individuales"],
            Images      = ["https://images.unsplash.com/photo-1497366412874-3415097a27e7?w=800&q=80&auto=format&fit=crop"],
            IsActive    = true,
            Rating      = 4.5,
            ReviewCount = 29,
        },
        new Workspace(Guid.Parse("b2000000-0000-0000-0000-000000000004"))
        {
            CompanyId   = C1,
            Name        = "Sala con proyector · 20 personas",
            Description = "Sala amplia con proyector 4K, pantalla de 150\", sonido y aire acondicionado. Perfecto para presentaciones, capacitaciones y workshops.",
            Address     = "Gurruchaga 1455, Palermo",
            City        = "Buenos Aires",
            Country     = "Argentina",
            Latitude    = -34.5875,
            Longitude   = -58.4333,
            Capacity    = 20,
            PricePerHour = 80m,
            PricePerDay  = 480m,
            Amenities   = ["WiFi", "AC", "Proyector", "Café", "Pizarra", "Baños", "Equipamiento de audio"],
            Images      = ["https://images.unsplash.com/photo-1517502884422-41eaead166d4?w=800&q=80&auto=format&fit=crop"],
            IsActive    = true,
            Rating      = 4.8,
            ReviewCount = 35,
        },

        // ── TechSpace BA (C2) ────────────────────────────────────────
        new Workspace(Guid.Parse("b2000000-0000-0000-0000-000000000005"))
        {
            CompanyId   = C2,
            Name        = "Coworking flexible · San Telmo",
            Description = "Área de coworking abierta con puestos hot-desk. Café ilimitado, lockers, impresora y comunidad tech. Acceso por hora o día.",
            Address     = "Defensa 756, San Telmo",
            City        = "Buenos Aires",
            Country     = "Argentina",
            Latitude    = -34.6213,
            Longitude   = -58.3712,
            Capacity    = 30,
            PricePerHour = 12m,
            PricePerDay  = 70m,
            PricePerMonth = 1200m,
            Amenities   = ["WiFi", "Café", "Impresora", "AC", "Baños", "Escritorios individuales"],
            Images      = ["https://images.unsplash.com/photo-1497366811353-6870744d04b2?w=800&q=80&auto=format&fit=crop"],
            IsActive    = true,
            Rating      = 4.6,
            ReviewCount = 88,
        },
        new Workspace(Guid.Parse("b2000000-0000-0000-0000-000000000006"))
        {
            CompanyId   = C2,
            Name        = "Sala híbrida con cámara y micrófono",
            Description = "Sala equipada para meetings híbridos con cámara 4K, micrófonos de condensador y conexión a Zoom/Meet/Teams en un clic.",
            Address     = "Defensa 756, San Telmo",
            City        = "Buenos Aires",
            Country     = "Argentina",
            Latitude    = -34.6213,
            Longitude   = -58.3712,
            Capacity    = 10,
            PricePerHour = 65m,
            PricePerDay  = 380m,
            Amenities   = ["WiFi", "AC", "Videoconferencia", "TV", "Equipamiento de audio", "Café"],
            Images      = ["https://images.unsplash.com/photo-1573497019940-1c28c88b4f3e?w=800&q=80&auto=format&fit=crop"],
            IsActive    = true,
            Rating      = 4.7,
            ReviewCount = 21,
        },
        new Workspace(Guid.Parse("b2000000-0000-0000-0000-000000000007"))
        {
            CompanyId   = C2,
            Name        = "Espacio para workshop · 20 personas",
            Description = "Sala configurable para workshops y dinámicas grupales. Mesas modulares, pizarras móviles, proyector y espacio para circular.",
            Address     = "Defensa 756, San Telmo",
            City        = "Buenos Aires",
            Country     = "Argentina",
            Latitude    = -34.6213,
            Longitude   = -58.3712,
            Capacity    = 20,
            PricePerHour = 70m,
            PricePerDay  = 400m,
            Amenities   = ["WiFi", "AC", "Proyector", "Pizarra", "Café", "Baños"],
            Images      = ["https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=800&q=80&auto=format&fit=crop"],
            IsActive    = true,
            Rating      = 4.8,
            ReviewCount = 14,
        },
        new Workspace(Guid.Parse("b2000000-0000-0000-0000-000000000008"))
        {
            CompanyId   = C2,
            Name        = "Sala de entrevistas · 4 personas",
            Description = "Sala privada y cómoda para procesos de selección, entrevistas de trabajo y reuniones confidenciales. Insonorizada.",
            Address     = "Defensa 756, San Telmo",
            City        = "Buenos Aires",
            Country     = "Argentina",
            Latitude    = -34.6213,
            Longitude   = -58.3712,
            Capacity    = 4,
            PricePerHour = 20m,
            PricePerDay  = 110m,
            Amenities   = ["WiFi", "AC", "Café"],
            Images      = ["https://images.unsplash.com/photo-1497366216548-37526070297c?w=800&q=80&auto=format&fit=crop"],
            IsActive    = true,
            Rating      = 4.4,
            ReviewCount = 32,
        },

        // ── Corporate Center BA (C3) ─────────────────────────────────
        new Workspace(Guid.Parse("b2000000-0000-0000-0000-000000000009"))
        {
            CompanyId   = C3,
            Name        = "Auditorio corporativo · 100 personas",
            Description = "Auditorio de primer nivel con sistema de sonido profesional, proyección dual, escenario y butacas. Para congresos, lanzamientos y eventos.",
            Address     = "Florida 890, Microcentro",
            City        = "Buenos Aires",
            Country     = "Argentina",
            Latitude    = -34.6037,
            Longitude   = -58.3720,
            Capacity    = 100,
            PricePerHour = 200m,
            PricePerDay  = 1200m,
            Amenities   = ["WiFi", "AC", "Proyector", "Equipamiento de audio", "Baños", "Recepción", "Estacionamiento"],
            Images      = ["https://images.unsplash.com/photo-1505373877841-8d25f7d46678?w=800&q=80&auto=format&fit=crop"],
            IsActive    = true,
            Rating      = 4.9,
            ReviewCount = 11,
        },
        new Workspace(Guid.Parse("b2000000-0000-0000-0000-000000000010"))
        {
            CompanyId   = C3,
            Name        = "Sala para eventos corporativos · 50 personas",
            Description = "Salón multiuso para eventos, cocktails y presentaciones. Cocina equipada, servicio de catering disponible y decoración moderna.",
            Address     = "Florida 890, Microcentro",
            City        = "Buenos Aires",
            Country     = "Argentina",
            Latitude    = -34.6037,
            Longitude   = -58.3720,
            Capacity    = 50,
            PricePerHour = 150m,
            PricePerDay  = 900m,
            Amenities   = ["WiFi", "AC", "Proyector", "Cocina", "Baños", "Recepción", "Equipamiento de audio"],
            Images      = ["https://images.unsplash.com/photo-1511578314322-379afb476865?w=800&q=80&auto=format&fit=crop"],
            IsActive    = true,
            Rating      = 4.7,
            ReviewCount = 7,
        },
        new Workspace(Guid.Parse("b2000000-0000-0000-0000-000000000011"))
        {
            CompanyId   = C3,
            Name        = "Aula de capacitación · 30 personas",
            Description = "Aula corporativa con mesas tipo escuela, proyector, pizarra interactiva y conexión a internet dedicada. Ideal para trainings.",
            Address     = "Florida 890, Microcentro",
            City        = "Buenos Aires",
            Country     = "Argentina",
            Latitude    = -34.6037,
            Longitude   = -58.3720,
            Capacity    = 30,
            PricePerHour = 60m,
            PricePerDay  = 350m,
            Amenities   = ["WiFi", "AC", "Proyector", "Pizarra", "Café", "Baños", "Escritorios individuales"],
            Images      = ["https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=800&q=80&auto=format&fit=crop"],
            IsActive    = true,
            Rating      = 4.6,
            ReviewCount = 23,
        },
        new Workspace(Guid.Parse("b2000000-0000-0000-0000-000000000012"))
        {
            CompanyId   = C3,
            Name        = "Coworking nocturno 24hs · Microcentro",
            Description = "Acceso full 24 horas, 7 días. Seguridad permanente, café y snacks disponibles. Ideal para freelancers con horarios flexibles.",
            Address     = "Florida 890, Microcentro",
            City        = "Buenos Aires",
            Country     = "Argentina",
            Latitude    = -34.6037,
            Longitude   = -58.3720,
            Capacity    = 40,
            PricePerHour = 10m,
            PricePerDay  = 55m,
            PricePerMonth = 900m,
            Amenities   = ["WiFi", "AC", "Café", "Acceso 24 hs", "Baños", "Escritorios individuales"],
            Images      = ["https://images.unsplash.com/photo-1498050108023-c5249f4df085?w=800&q=80&auto=format&fit=crop"],
            IsActive    = true,
            Rating      = 4.3,
            ReviewCount = 55,
        },
        new Workspace(Guid.Parse("b2000000-0000-0000-0000-000000000013"))
        {
            CompanyId   = C3,
            Name        = "Sala con videoconferencia · 8 personas",
            Description = "Sala equipada para llamadas internacionales. Pantalla interactiva, cámara panorámica y cancelación de ruido integrada.",
            Address     = "Florida 890, Microcentro",
            City        = "Buenos Aires",
            Country     = "Argentina",
            Latitude    = -34.6037,
            Longitude   = -58.3720,
            Capacity    = 8,
            PricePerHour = 55m,
            PricePerDay  = 300m,
            Amenities   = ["WiFi", "AC", "Videoconferencia", "TV", "Café", "Recepción"],
            Images      = ["https://images.unsplash.com/photo-1556761175-4b46a572b786?w=800&q=80&auto=format&fit=crop"],
            IsActive    = true,
            Rating      = 4.8,
            ReviewCount = 19,
        },

        // ── Belgrano Business (C4) ───────────────────────────────────
        new Workspace(Guid.Parse("b2000000-0000-0000-0000-000000000014"))
        {
            CompanyId   = C4,
            Name        = "Oficina premium con estacionamiento · Belgrano",
            Description = "Oficina ejecutiva de alto estándar con estacionamiento privado, recepción dedicada y sala de espera para clientes.",
            Address     = "Av. Cabildo 1850, Belgrano",
            City        = "Buenos Aires",
            Country     = "Argentina",
            Latitude    = -34.5625,
            Longitude   = -58.4555,
            Capacity    = 6,
            PricePerHour = 80m,
            PricePerDay  = 480m,
            PricePerMonth = 7500m,
            Amenities   = ["WiFi", "AC", "Café", "Estacionamiento", "Recepción", "Sillas ergonómicas", "Baños"],
            Images      = ["https://images.unsplash.com/photo-1568992687947-868a62a9f521?w=800&q=80&auto=format&fit=crop"],
            IsActive    = true,
            Rating      = 4.9,
            ReviewCount = 27,
        },
        new Workspace(Guid.Parse("b2000000-0000-0000-0000-000000000015"))
        {
            CompanyId   = C4,
            Name        = "Sala de directorio · 12 personas",
            Description = "Sala de directorio con vista panorámica, mesa oval de madera, sillones de cuero y equipamiento completo para board meetings.",
            Address     = "Av. Cabildo 1850, Belgrano",
            City        = "Buenos Aires",
            Country     = "Argentina",
            Latitude    = -34.5625,
            Longitude   = -58.4555,
            Capacity    = 12,
            PricePerHour = 90m,
            PricePerDay  = 540m,
            Amenities   = ["WiFi", "AC", "TV", "Videoconferencia", "Café", "Recepción", "Estacionamiento", "Baños"],
            Images      = ["https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=800&q=80&auto=format&fit=crop"],
            IsActive    = true,
            Rating      = 5.0,
            ReviewCount = 9,
        },
        new Workspace(Guid.Parse("b2000000-0000-0000-0000-000000000016"))
        {
            CompanyId   = C4,
            Name        = "Oficina con recepción · 8 personas",
            Description = "Oficina semi-privada con recepcionista compartida, sala de espera de clientes y dirección fiscal disponible.",
            Address     = "Av. Cabildo 1850, Belgrano",
            City        = "Buenos Aires",
            Country     = "Argentina",
            Latitude    = -34.5625,
            Longitude   = -58.4555,
            Capacity    = 8,
            PricePerHour = 55m,
            PricePerDay  = 320m,
            PricePerMonth = 5000m,
            Amenities   = ["WiFi", "AC", "Café", "Recepción", "Estacionamiento", "Sillas ergonómicas"],
            Images      = ["https://images.unsplash.com/photo-1497215842964-222b01206355?w=800&q=80&auto=format&fit=crop"],
            IsActive    = true,
            Rating      = 4.6,
            ReviewCount = 31,
        },
        new Workspace(Guid.Parse("b2000000-0000-0000-0000-000000000017"))
        {
            CompanyId   = C4,
            Name        = "Espacio creativo · Belgrano",
            Description = "Loft de diseño con mesas altas, pizarras en toda la pared, impresora color y espacio abierto. Pensado para agencias y creativos.",
            Address     = "Av. Cabildo 1850, Belgrano",
            City        = "Buenos Aires",
            Country     = "Argentina",
            Latitude    = -34.5625,
            Longitude   = -58.4555,
            Capacity    = 15,
            PricePerHour = 45m,
            PricePerDay  = 260m,
            Amenities   = ["WiFi", "AC", "Pizarra", "Impresora", "Café", "Baños"],
            Images      = ["https://images.unsplash.com/photo-1547658719-da2b51169166?w=800&q=80&auto=format&fit=crop"],
            IsActive    = true,
            Rating      = 4.5,
            ReviewCount = 16,
        },

        // ── Rosario Cowork (C5) ──────────────────────────────────────
        new Workspace(Guid.Parse("b2000000-0000-0000-0000-000000000018"))
        {
            CompanyId   = C5,
            Name        = "Oficina económica compartida · Rosario",
            Description = "Puesto de trabajo compartido a precio accesible. Excelente para profesionales independientes que buscan salir de casa.",
            Address     = "Córdoba 1200, Rosario",
            City        = "Rosario",
            Country     = "Argentina",
            Latitude    = -32.9468,
            Longitude   = -60.6393,
            Capacity    = 4,
            PricePerHour = 8m,
            PricePerDay  = 45m,
            PricePerMonth = 650m,
            Amenities   = ["WiFi", "Café", "Impresora", "Baños"],
            Images      = ["https://images.unsplash.com/photo-1600508774634-4e11d34730e2?w=800&q=80&auto=format&fit=crop"],
            IsActive    = true,
            Rating      = 4.2,
            ReviewCount = 47,
        },
        new Workspace(Guid.Parse("b2000000-0000-0000-0000-000000000019"))
        {
            CompanyId   = C5,
            Name        = "Box privado · Rosario",
            Description = "Box cerrado de uso individual con llave propia. Ambiente silencioso, ideal para abogados, contadores y consultores.",
            Address     = "Córdoba 1200, Rosario",
            City        = "Rosario",
            Country     = "Argentina",
            Latitude    = -32.9468,
            Longitude   = -60.6393,
            Capacity    = 2,
            PricePerHour = 12m,
            PricePerDay  = 70m,
            PricePerMonth = 950m,
            Amenities   = ["WiFi", "AC", "Café", "Baños"],
            Images      = ["https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=800&q=80&auto=format&fit=crop"],
            IsActive    = true,
            Rating      = 4.4,
            ReviewCount = 22,
        },
        new Workspace(Guid.Parse("b2000000-0000-0000-0000-000000000020"))
        {
            CompanyId   = C5,
            Name        = "Auditorio chico · Charlas y meetups",
            Description = "Auditorio para 30 personas con proyector, micrófono inalámbrico y sillas apilables. Ideal para charlas tech, meetups y presentaciones.",
            Address     = "Córdoba 1200, Rosario",
            City        = "Rosario",
            Country     = "Argentina",
            Latitude    = -32.9468,
            Longitude   = -60.6393,
            Capacity    = 30,
            PricePerHour = 50m,
            PricePerDay  = 280m,
            Amenities   = ["WiFi", "AC", "Proyector", "Equipamiento de audio", "Baños", "Acceso 24 hs"],
            Images      = ["https://images.unsplash.com/photo-1540575467537-10dc95eb654b?w=800&q=80&auto=format&fit=crop"],
            IsActive    = true,
            Rating      = 4.6,
            ReviewCount = 13,
        },
    ];
}
