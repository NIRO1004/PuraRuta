using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PuraRuta.Models;

namespace PuraRuta.Data;

public partial class PuraRutaContext : DbContext
{
    public PuraRutaContext(DbContextOptions<PuraRutaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Canton> Cantons { get; set; }

    public virtual DbSet<Categorium> Categoria { get; set; }

    public virtual DbSet<Comunidad> Comunidads { get; set; }

    public virtual DbSet<Direccion> Direccions { get; set; }

    public virtual DbSet<Distrito> Distritos { get; set; }

    public virtual DbSet<EncuestaSatisfaccion> EncuestaSatisfaccions { get; set; }

    public virtual DbSet<EstadoReserva> EstadoReservas { get; set; }

    public virtual DbSet<Experiencium> Experiencia { get; set; }

    public virtual DbSet<FotoComunidad> FotoComunidads { get; set; }

    public virtual DbSet<FotoExperiencium> FotoExperiencia { get; set; }

    public virtual DbSet<Provincium> Provincia { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<RutaExperiencium> RutaExperiencia { get; set; }

    public virtual DbSet<Rutum> Ruta { get; set; }

    public virtual DbSet<TuristaCategorium> TuristaCategoria { get; set; }

    public virtual DbSet<Turistum> Turista { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Canton>(entity =>
        {
            entity.HasKey(e => e.IdCanton).HasName("PK__Canton__339BEA5706224F91");

            entity.ToTable("Canton");

            entity.HasIndex(e => e.IdProvincia, "IX_Canton_Provincia");

            entity.Property(e => e.IdCanton).HasColumnName("id_canton");
            entity.Property(e => e.IdProvincia).HasColumnName("id_provincia");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdProvinciaNavigation).WithMany(p => p.Cantons)
                .HasForeignKey(d => d.IdProvincia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Canton_Provincia");
        });

        modelBuilder.Entity<Categorium>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK__Categori__CD54BC5A44E0FE85");

            entity.HasIndex(e => e.Nombre, "UQ__Categori__72AFBCC67194F919").IsUnique();

            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.DescripcionCategoria)
                .HasMaxLength(150)
                .HasColumnName("descripcion_categoria");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Comunidad>(entity =>
        {
            entity.HasKey(e => e.IdComunidad).HasName("PK__Comunida__B6E50DE50DAD91DB");

            entity.ToTable("Comunidad");

            entity.HasIndex(e => e.IdDireccion, "IX_Comunidad_Direccion");

            entity.Property(e => e.IdComunidad).HasColumnName("id_comunidad");
            entity.Property(e => e.DescripcionComunidad)
                .HasMaxLength(250)
                .HasColumnName("descripcion_comunidad");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.IdDireccion).HasColumnName("id_direccion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdDireccionNavigation).WithMany(p => p.Comunidads)
                .HasForeignKey(d => d.IdDireccion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comunidad_Direccion");
        });

        modelBuilder.Entity<Direccion>(entity =>
        {
            entity.HasKey(e => e.IdDireccion).HasName("PK__Direccio__25C35D07DF068775");

            entity.ToTable("Direccion");

            entity.HasIndex(e => e.IdDistrito, "IX_Direccion_Distrito");

            entity.Property(e => e.IdDireccion).HasColumnName("id_direccion");
            entity.Property(e => e.DireccionExacta)
                .HasMaxLength(150)
                .HasColumnName("direccion_exacta");
            entity.Property(e => e.IdDistrito).HasColumnName("id_distrito");
            entity.Property(e => e.Latitud)
                .HasColumnType("decimal(10, 7)")
                .HasColumnName("latitud");
            entity.Property(e => e.Longitud)
                .HasColumnType("decimal(10, 7)")
                .HasColumnName("longitud");

            entity.HasOne(d => d.IdDistritoNavigation).WithMany(p => p.Direccions)
                .HasForeignKey(d => d.IdDistrito)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Direccion_Distrito");
        });

        modelBuilder.Entity<Distrito>(entity =>
        {
            entity.HasKey(e => e.IdDistrito).HasName("PK__Distrito__65E5575D504F47FE");

            entity.ToTable("Distrito");

            entity.HasIndex(e => e.IdCanton, "IX_Distrito_Canton");

            entity.Property(e => e.IdDistrito).HasColumnName("id_distrito");
            entity.Property(e => e.IdCanton).HasColumnName("id_canton");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdCantonNavigation).WithMany(p => p.Distritos)
                .HasForeignKey(d => d.IdCanton)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Distrito_Canton");
        });

        modelBuilder.Entity<EncuestaSatisfaccion>(entity =>
        {
            entity.HasKey(e => e.IdEncuesta).HasName("PK__Encuesta__013535C39CBD618C");

            entity.ToTable("Encuesta_Satisfaccion");

            entity.HasIndex(e => e.IdReserva, "UQ__Encuesta__423CBE5C75A45E84").IsUnique();

            entity.Property(e => e.IdEncuesta).HasColumnName("id_encuesta");
            entity.Property(e => e.Calificacion).HasColumnName("calificacion");
            entity.Property(e => e.Comentario)
                .HasMaxLength(250)
                .HasColumnName("comentario");
            entity.Property(e => e.FechaEncuesta).HasColumnName("fecha_encuesta");
            entity.Property(e => e.IdReserva).HasColumnName("id_reserva");
            entity.Property(e => e.Recomendaria).HasColumnName("recomendaria");

            entity.HasOne(d => d.IdReservaNavigation).WithOne(p => p.EncuestaSatisfaccion)
                .HasForeignKey<EncuestaSatisfaccion>(d => d.IdReserva)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Encuesta_Reserva");
        });

        modelBuilder.Entity<EstadoReserva>(entity =>
        {
            entity.HasKey(e => e.IdEstadoReserva).HasName("PK__Estado_R__D1D0D7E0F12CF924");

            entity.ToTable("Estado_Reserva");

            entity.HasIndex(e => e.Nombre, "UQ__Estado_R__72AFBCC66C1FA1D5").IsUnique();

            entity.Property(e => e.IdEstadoReserva).HasColumnName("id_estado_reserva");
            entity.Property(e => e.DescripcionEstadoReserva)
                .HasMaxLength(120)
                .HasColumnName("descripcion_estado_reserva");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Experiencium>(entity =>
        {
            entity.HasKey(e => e.IdExperiencia).HasName("PK__Experien__301735EDDC4413AB");

            entity.HasIndex(e => e.IdCategoria, "IX_Experiencia_Categoria");

            entity.HasIndex(e => e.IdComunidad, "IX_Experiencia_Comunidad");

            entity.HasIndex(e => e.IdDireccion, "IX_Experiencia_Direccion");

            entity.Property(e => e.IdExperiencia).HasColumnName("id_experiencia");
            entity.Property(e => e.CapacidadDiaria).HasColumnName("capacidad_diaria");
            entity.Property(e => e.DescripcionExperiencia)
                .HasMaxLength(250)
                .HasColumnName("descripcion_experiencia");
            entity.Property(e => e.Dificultad)
                .HasMaxLength(30)
                .HasColumnName("dificultad");
            entity.Property(e => e.DuracionHoras)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("duracion_horas");
            entity.Property(e => e.EdadMinima).HasColumnName("edad_minima");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.IdComunidad).HasColumnName("id_comunidad");
            entity.Property(e => e.IdDireccion).HasColumnName("id_direccion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(60)
                .HasColumnName("nombre");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio");
            entity.Property(e => e.TransportePublico).HasColumnName("transporte_publico");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Experiencia)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Experiencia_Categoria");

            entity.HasOne(d => d.IdComunidadNavigation).WithMany(p => p.Experiencia)
                .HasForeignKey(d => d.IdComunidad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Experiencia_Comunidad");

            entity.HasOne(d => d.IdDireccionNavigation).WithMany(p => p.Experiencia)
                .HasForeignKey(d => d.IdDireccion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Experiencia_Direccion");
        });

        modelBuilder.Entity<FotoComunidad>(entity =>
        {
            entity.HasKey(e => e.IdFotoComunidad).HasName("PK__Foto_Com__F57BF954103BFEF2");

            entity.ToTable("Foto_Comunidad");

            entity.HasIndex(e => e.IdComunidad, "IX_Foto_Comunidad_Comunidad");

            entity.Property(e => e.IdFotoComunidad).HasColumnName("id_foto_comunidad");
            entity.Property(e => e.DescripcionFotoComunidad)
                .HasMaxLength(150)
                .HasColumnName("descripcion_foto_comunidad");
            entity.Property(e => e.IdComunidad).HasColumnName("id_comunidad");
            entity.Property(e => e.RutaImagen)
                .HasMaxLength(200)
                .HasColumnName("ruta_imagen");

            entity.HasOne(d => d.IdComunidadNavigation).WithMany(p => p.FotoComunidads)
                .HasForeignKey(d => d.IdComunidad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Foto_Comunidad");
        });

        modelBuilder.Entity<FotoExperiencium>(entity =>
        {
            entity.HasKey(e => e.IdFotoExperiencia).HasName("PK__Foto_Exp__B79E3B7FA0AA0A1D");

            entity.ToTable("Foto_Experiencia");

            entity.HasIndex(e => e.IdExperiencia, "IX_Foto_Experiencia_Experiencia");

            entity.Property(e => e.IdFotoExperiencia).HasColumnName("id_foto_experiencia");
            entity.Property(e => e.DescripcionFotoExperiencia)
                .HasMaxLength(150)
                .HasColumnName("descripcion_foto_experiencia");
            entity.Property(e => e.IdExperiencia).HasColumnName("id_experiencia");
            entity.Property(e => e.RutaImagen)
                .HasMaxLength(200)
                .HasColumnName("ruta_imagen");

            entity.HasOne(d => d.IdExperienciaNavigation).WithMany(p => p.FotoExperiencia)
                .HasForeignKey(d => d.IdExperiencia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Foto_Experiencia");
        });

        modelBuilder.Entity<Provincium>(entity =>
        {
            entity.HasKey(e => e.IdProvincia).HasName("PK__Provinci__66C18BFD8E5628C6");

            entity.HasIndex(e => e.Nombre, "UQ__Provinci__72AFBCC6260DD76D").IsUnique();

            entity.Property(e => e.IdProvincia).HasColumnName("id_provincia");
            entity.Property(e => e.Nombre)
                .HasMaxLength(25)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.IdReserva).HasName("PK__Reserva__423CBE5DA656B24A");

            entity.ToTable("Reserva");

            entity.HasIndex(e => e.IdEstadoReserva, "IX_Reserva_Estado");

            entity.HasIndex(e => e.IdExperiencia, "IX_Reserva_Experiencia");

            entity.HasIndex(e => e.IdTurista, "IX_Reserva_Turista");

            entity.Property(e => e.IdReserva).HasColumnName("id_reserva");
            entity.Property(e => e.CantidadPersonas).HasColumnName("cantidad_personas");
            entity.Property(e => e.FechaReserva).HasColumnName("fecha_reserva");
            entity.Property(e => e.FechaVisita).HasColumnName("fecha_visita");
            entity.Property(e => e.IdEstadoReserva).HasColumnName("id_estado_reserva");
            entity.Property(e => e.IdExperiencia).HasColumnName("id_experiencia");
            entity.Property(e => e.IdTurista).HasColumnName("id_turista");
            entity.Property(e => e.PrecioTotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio_total");

            entity.HasOne(d => d.IdEstadoReservaNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdEstadoReserva)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reserva_Estado");

            entity.HasOne(d => d.IdExperienciaNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdExperiencia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reserva_Experiencia");

            entity.HasOne(d => d.IdTuristaNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdTurista)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reserva_Turista");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Rol__6ABCB5E044A072BB");

            entity.ToTable("Rol");

            entity.HasIndex(e => e.Nombre, "UQ__Rol__72AFBCC63B20E6F7").IsUnique();

            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.DescripcionRol)
                .HasMaxLength(100)
                .HasColumnName("descripcion_rol");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<RutaExperiencium>(entity =>
        {
            entity.HasKey(e => e.IdRutaExperiencia).HasName("PK__Ruta_Exp__16212F6A31DEC124");

            entity.ToTable("Ruta_Experiencia");

            entity.HasIndex(e => e.IdExperiencia, "IX_Ruta_Experiencia_Experiencia");

            entity.HasIndex(e => new { e.IdRuta, e.IdExperiencia }, "UQ_Ruta_Experiencia").IsUnique();

            entity.HasIndex(e => new { e.IdRuta, e.OrdenVisita }, "UQ_Ruta_Orden").IsUnique();

            entity.Property(e => e.IdRutaExperiencia).HasColumnName("id_ruta_experiencia");
            entity.Property(e => e.DistanciaKm)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("distancia_km");
            entity.Property(e => e.IdExperiencia).HasColumnName("id_experiencia");
            entity.Property(e => e.IdRuta).HasColumnName("id_ruta");
            entity.Property(e => e.OrdenVisita).HasColumnName("orden_visita");

            entity.HasOne(d => d.IdExperienciaNavigation).WithMany(p => p.RutaExperiencia)
                .HasForeignKey(d => d.IdExperiencia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ruta_Experiencia_Experiencia");

            entity.HasOne(d => d.IdRutaNavigation).WithMany(p => p.RutaExperiencia)
                .HasForeignKey(d => d.IdRuta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ruta_Experiencia_Ruta");
        });

        modelBuilder.Entity<Rutum>(entity =>
        {
            entity.HasKey(e => e.IdRuta).HasName("PK__Ruta__33C9344FD1A0C628");

            entity.HasIndex(e => e.IdTurista, "IX_Ruta_Turista");

            entity.Property(e => e.IdRuta).HasColumnName("id_ruta");
            entity.Property(e => e.DistanciaTotalKm)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("distancia_total_km");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaVisita).HasColumnName("fecha_visita");
            entity.Property(e => e.IdTurista).HasColumnName("id_turista");
            entity.Property(e => e.Nombre)
                .HasMaxLength(60)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdTuristaNavigation).WithMany(p => p.Ruta)
                .HasForeignKey(d => d.IdTurista)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ruta_Turista");
        });

        modelBuilder.Entity<TuristaCategorium>(entity =>
        {
            entity.HasKey(e => e.IdTuristaCategoria).HasName("PK__Turista___51C8D51414E12934");

            entity.ToTable("Turista_Categoria");

            entity.HasIndex(e => e.IdCategoria, "IX_Turista_Categoria_Categoria");

            entity.HasIndex(e => new { e.IdTurista, e.IdCategoria }, "UQ_Turista_Categoria").IsUnique();

            entity.Property(e => e.IdTuristaCategoria).HasColumnName("id_turista_categoria");
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.IdTurista).HasColumnName("id_turista");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.TuristaCategoria)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Turista_Categoria_Categoria");

            entity.HasOne(d => d.IdTuristaNavigation).WithMany(p => p.TuristaCategoria)
                .HasForeignKey(d => d.IdTurista)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Turista_Categoria_Turista");
        });

        modelBuilder.Entity<Turistum>(entity =>
        {
            entity.HasKey(e => e.IdTurista).HasName("PK__Turista__AF47812743C7A001");

            entity.HasIndex(e => e.IdDireccion, "IX_Turista_Direccion");

            entity.Property(e => e.IdTurista)
                .ValueGeneratedNever()
                .HasColumnName("id_turista");
            entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");
            entity.Property(e => e.IdDireccion).HasColumnName("id_direccion");
            entity.Property(e => e.TieneVehiculo).HasColumnName("tiene_vehiculo");

            entity.HasOne(d => d.IdDireccionNavigation).WithMany(p => p.Turista)
                .HasForeignKey(d => d.IdDireccion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Turista_Direccion");

            entity.HasOne(d => d.IdTuristaNavigation).WithOne(p => p.Turistum)
                .HasForeignKey<Turistum>(d => d.IdTurista)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Turista_Usuario");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__4E3E04ADB3F312AC");

            entity.ToTable("Usuario");

            entity.HasIndex(e => e.IdRol, "IX_Usuario_Rol");

            entity.HasIndex(e => e.Correo, "UQ__Usuario__2A586E0B0FECEA71").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Apellido1)
                .HasMaxLength(50)
                .HasColumnName("apellido1");
            entity.Property(e => e.Apellido2)
                .HasMaxLength(50)
                .HasColumnName("apellido2");
            entity.Property(e => e.ContrasenaHash)
                .HasMaxLength(255)
                .HasColumnName("contrasena_hash");
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .HasColumnName("correo");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Rol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
