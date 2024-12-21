using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace admin_sweetsoft_tech_support.Models;

public partial class RequestContext : DbContext
{
    public RequestContext()
    {
    }

    public RequestContext(DbContextOptions<RequestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblCustomer> TblCustomers { get; set; }

    public virtual DbSet<TblDepartment> TblDepartments { get; set; }

    public virtual DbSet<TblFaq> TblFaqs { get; set; }

    public virtual DbSet<TblPermission> TblPermissions { get; set; }

    public virtual DbSet<TblRequestTransfer> TblRequestTransfers { get; set; }

    public virtual DbSet<TblRequestsProcessing> TblRequestsProcessings { get; set; }

    public virtual DbSet<TblRole> TblRoles { get; set; }

    public virtual DbSet<TblSupportRequest> TblSupportRequests { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    public virtual DbSet<TblUserPermission> TblUserPermissions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=LAPTOP-J8U10551\\SQLEXPRESS;Initial Catalog=Request;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblCustomer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__TblCusto__CD65CB8517893AA8");

            entity.HasIndex(e => e.Email, "UQ__TblCusto__AB6E616418B9C130").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__TblCusto__F3DBC5722990DD89").IsUnique();

            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Company)
                .HasMaxLength(100)
                .HasColumnName("company");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.Product)
                .HasMaxLength(100)
                .HasColumnName("product");
            entity.Property(e => e.ResetToken)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("reset_token");
            entity.Property(e => e.ResetTokenExpiry)
                .HasColumnType("datetime")
                .HasColumnName("reset_token_expiry");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TaxCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tax_code");
            entity.Property(e => e.Token)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("token");
            entity.Property(e => e.TokenExpiry)
                .HasColumnType("datetime")
                .HasColumnName("token_expiry");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedUser).HasColumnName("updated_user");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("username");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.TblCustomerCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK__TblCustom__creat__38996AB5");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.TblCustomerUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK__TblCustom__updat__398D8EEE");
        });

        modelBuilder.Entity<TblDepartment>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__TblDepar__C223242204BBC91F");

            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(100)
                .HasColumnName("department_name");
            entity.Property(e => e.Status).HasColumnName("status");
        });

        modelBuilder.Entity<TblFaq>(entity =>
        {
            entity.HasKey(e => e.FaqId).HasName("PK__TblFaqs__66734BAF5D708E61");

            entity.Property(e => e.FaqId).HasColumnName("faq_id");
            entity.Property(e => e.Answer).HasColumnName("answer");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Question)
                .HasMaxLength(255)
                .HasColumnName("question");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<TblPermission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__TblPermi__E5331AFAFF0949E3");

            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.PermissionName)
                .HasMaxLength(100)
                .HasColumnName("permission_name");
        });

        modelBuilder.Entity<TblRequestTransfer>(entity =>
        {
            entity.HasKey(e => e.TransferId).HasName("PK__TblReque__78E6FD3308C60184");

            entity.ToTable("TblRequest_Transfers");

            entity.Property(e => e.TransferId).HasColumnName("transfer_id");
            entity.Property(e => e.FromDepartmentId).HasColumnName("from_department_id");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.Priority)
                .HasDefaultValue((short)1)
                .HasColumnName("priority");
            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.ToDepartmentId).HasColumnName("to_department_id");
            entity.Property(e => e.TransferredAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("transferred_at");
            entity.Property(e => e.TransferredBy).HasColumnName("transferred_by");

            entity.HasOne(d => d.FromDepartment).WithMany(p => p.TblRequestTransferFromDepartments)
                .HasForeignKey(d => d.FromDepartmentId)
                .HasConstraintName("FK__TblReques__from___4316F928");

            entity.HasOne(d => d.Request).WithMany(p => p.TblRequestTransfers)
                .HasForeignKey(d => d.RequestId)
                .HasConstraintName("FK__TblReques__reque__4222D4EF");

            entity.HasOne(d => d.ToDepartment).WithMany(p => p.TblRequestTransferToDepartments)
                .HasForeignKey(d => d.ToDepartmentId)
                .HasConstraintName("FK__TblReques__to_de__440B1D61");

            entity.HasOne(d => d.TransferredByNavigation).WithMany(p => p.TblRequestTransfers)
                .HasForeignKey(d => d.TransferredBy)
                .HasConstraintName("FK__TblReques__trans__44FF419A");
        });

        modelBuilder.Entity<TblRequestsProcessing>(entity =>
        {
            entity.HasKey(e => e.ProcessId).HasName("PK__TblReque__9446C3E13B31C46D");

            entity.ToTable("TblRequests_Processing");

            entity.Property(e => e.ProcessId).HasColumnName("process_id");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.IsCompleted)
                .HasDefaultValue((short)0)
                .HasColumnName("is_completed");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.ProcessedAt)
                .HasColumnType("datetime")
                .HasColumnName("processed_at");
            entity.Property(e => e.RequestId).HasColumnName("request_id");

            entity.HasOne(d => d.Department).WithMany(p => p.TblRequestsProcessings)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__TblReques__depar__49C3F6B7");

            entity.HasOne(d => d.Request).WithMany(p => p.TblRequestsProcessings)
                .HasForeignKey(d => d.RequestId)
                .HasConstraintName("FK__TblReques__reque__48CFD27E");
        });

        modelBuilder.Entity<TblRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__TblRoles__760965CC055271E8");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(100)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<TblSupportRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__TblSuppo__18D3B90F9E36131B");

            entity.ToTable("TblSupport_Requests");

            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.RequestDetails).HasColumnName("request_details");
            entity.Property(e => e.ResolvedAt)
                .HasColumnType("datetime")
                .HasColumnName("resolved_at");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Customer).WithMany(p => p.TblSupportRequests)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__TblSuppor__custo__3C69FB99");

            entity.HasOne(d => d.Department).WithMany(p => p.TblSupportRequests)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__TblSuppor__depar__3D5E1FD2");
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__TblUsers__B9BE370FC5C2A9EE");

            entity.HasIndex(e => e.Email, "UQ__TblUsers__AB6E61643FB65C5F").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__TblUsers__F3DBC572FE0456AD").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.IsAdmin)
                .HasDefaultValue(false)
                .HasColumnName("is_admin");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.ResetToken)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("reset_token");
            entity.Property(e => e.ResetTokenExpiry)
                .HasColumnType("datetime")
                .HasColumnName("reset_token_expiry");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedUser).HasColumnName("updated_user");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("username");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.InverseCreatedUserNavigation)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK__TblUsers__create__2F10007B");

            entity.HasOne(d => d.Department).WithMany(p => p.TblUsers)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__TblUsers__depart__2E1BDC42");

            entity.HasOne(d => d.Role).WithMany(p => p.TblUsers)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__TblUsers__role_i__2D27B809");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.InverseUpdatedUserNavigation)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK__TblUsers__update__300424B4");
        });

        modelBuilder.Entity<TblUserPermission>(entity =>
        {
            entity.HasKey(e => e.UserPermissionId).HasName("PK__TblUser___D98F4819FCF0FD52");

            entity.ToTable("TblUser_Permissions");

            entity.Property(e => e.UserPermissionId).HasColumnName("user_permission_id");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Permission).WithMany(p => p.TblUserPermissions)
                .HasForeignKey(d => d.PermissionId)
                .HasConstraintName("FK__TblUser_P__permi__33D4B598");

            entity.HasOne(d => d.User).WithMany(p => p.TblUserPermissions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__TblUser_P__user___32E0915F");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
