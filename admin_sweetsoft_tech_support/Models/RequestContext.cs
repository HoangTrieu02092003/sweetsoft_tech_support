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

    public virtual DbSet<TblActivityLog> TblActivityLogs { get; set; }

    public virtual DbSet<TblAuditLog> TblAuditLogs { get; set; }

    public virtual DbSet<TblCustomer> TblCustomers { get; set; }

    public virtual DbSet<TblDepartment> TblDepartments { get; set; }

    public virtual DbSet<TblFaq> TblFaqs { get; set; }

    public virtual DbSet<TblLog> TblLogs { get; set; }

    public virtual DbSet<TblNotification> TblNotifications { get; set; }

    public virtual DbSet<TblPermission> TblPermissions { get; set; }

    public virtual DbSet<TblRequestTransfer> TblRequestTransfers { get; set; }

    public virtual DbSet<TblRequestsProcessing> TblRequestsProcessings { get; set; }

    public virtual DbSet<TblRole> TblRoles { get; set; }

    public virtual DbSet<TblRolePermission> TblRolePermissions { get; set; }

    public virtual DbSet<TblSession> TblSessions { get; set; }

    public virtual DbSet<TblSupportRequest> TblSupportRequests { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    public virtual DbSet<TblUserPermission> TblUserPermissions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=laptop-eaqci85g;Initial Catalog=Request;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblActivityLog>(entity =>
        {
            entity.HasKey(e => e.ActivityId).HasName("PK__TblActiv__482FBD63AFA1A05A");

            entity.Property(e => e.ActivityId).HasColumnName("activity_id");
            entity.Property(e => e.Action)
                .HasMaxLength(100)
                .HasColumnName("action");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Request).WithMany(p => p.TblActivityLogs)
                .HasForeignKey(d => d.RequestId)
                .HasConstraintName("FK__TblActivi__reque__778AC167");

            entity.HasOne(d => d.User).WithMany(p => p.TblActivityLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__TblActivi__user___787EE5A0");
        });

        modelBuilder.Entity<TblAuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditId).HasName("PK__TblAudit__5AF33E337EBA9344");

            entity.Property(e => e.AuditId).HasColumnName("audit_id");
            entity.Property(e => e.ActionType)
                .HasMaxLength(50)
                .HasColumnName("action_type");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("changed_at");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
            entity.Property(e => e.NewValue).HasColumnName("new_value");
            entity.Property(e => e.OldValue).HasColumnName("old_value");
            entity.Property(e => e.RecordId).HasColumnName("record_id");
            entity.Property(e => e.TableName)
                .HasMaxLength(100)
                .HasColumnName("table_name");

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.TblAuditLogs)
                .HasForeignKey(d => d.ChangedBy)
                .HasConstraintName("FK__TblAuditL__chang__534D60F1");
        });

        modelBuilder.Entity<TblCustomer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__TblCusto__CD65CB858286021A");

            entity.HasIndex(e => e.Email, "UQ__TblCusto__AB6E616443D85CFE").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__TblCusto__F3DBC57215A6A79E").IsUnique();

            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Company)
                .HasMaxLength(100)
                .HasColumnName("company");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
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
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
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
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("username");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TblCustomerCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__TblCustom__creat__5BE2A6F2");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.TblCustomerUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__TblCustom__updat__5CD6CB2B");
        });

        modelBuilder.Entity<TblDepartment>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__TblDepar__C2232422F562A369");

            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(100)
                .HasColumnName("department_name");
            entity.Property(e => e.Status).HasColumnName("status");
        });

        modelBuilder.Entity<TblFaq>(entity =>
        {
            entity.HasKey(e => e.FaqId).HasName("PK__TblFaqs__66734BAFB0FFB05E");

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

        modelBuilder.Entity<TblLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__TblLogs__9E2397E08AE396A6");

            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.Action)
                .HasMaxLength(255)
                .HasColumnName("action");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.TblLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__TblLogs__user_id__4F7CD00D");
        });

        modelBuilder.Entity<TblNotification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__TblNotif__E059842F4B9D824B");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Message)
                .HasMaxLength(255)
                .HasColumnName("message");
            entity.Property(e => e.Status)
                .HasDefaultValue((short)0)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.TblNotifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__TblNotifi__user___73BA3083");
        });

        modelBuilder.Entity<TblPermission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__TblPermi__E5331AFA436999AE");

            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.PermissionName)
                .HasMaxLength(100)
                .HasColumnName("permission_name");
        });

        modelBuilder.Entity<TblRequestTransfer>(entity =>
        {
            entity.HasKey(e => e.TransferId).HasName("PK__TblReque__78E6FD33CB68C278");

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
                .HasConstraintName("FK__TblReques__from___66603565");

            entity.HasOne(d => d.Request).WithMany(p => p.TblRequestTransfers)
                .HasForeignKey(d => d.RequestId)
                .HasConstraintName("FK__TblReques__reque__656C112C");

            entity.HasOne(d => d.ToDepartment).WithMany(p => p.TblRequestTransferToDepartments)
                .HasForeignKey(d => d.ToDepartmentId)
                .HasConstraintName("FK__TblReques__to_de__6754599E");

            entity.HasOne(d => d.TransferredByNavigation).WithMany(p => p.TblRequestTransfers)
                .HasForeignKey(d => d.TransferredBy)
                .HasConstraintName("FK__TblReques__trans__68487DD7");
        });

        modelBuilder.Entity<TblRequestsProcessing>(entity =>
        {
            entity.HasKey(e => e.ProcessId).HasName("PK__TblReque__9446C3E1E78DAA16");

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
                .HasConstraintName("FK__TblReques__depar__6D0D32F4");

            entity.HasOne(d => d.Request).WithMany(p => p.TblRequestsProcessings)
                .HasForeignKey(d => d.RequestId)
                .HasConstraintName("FK__TblReques__reque__6C190EBB");
        });

        modelBuilder.Entity<TblRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__TblRoles__760965CCB960403F");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(100)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<TblRolePermission>(entity =>
        {
            entity.HasKey(e => e.RolePermissionId).HasName("PK__TblRole___B1E85A105F337F5C");

            entity.ToTable("TblRole_Permissions");

            entity.Property(e => e.RolePermissionId).HasColumnName("role_permission_id");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Permission).WithMany(p => p.TblRolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .HasConstraintName("FK__TblRole_P__permi__47DBAE45");

            entity.HasOne(d => d.Role).WithMany(p => p.TblRolePermissions)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__TblRole_P__role___46E78A0C");
        });

        modelBuilder.Entity<TblSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__TblSessi__69B13FDC49103D88");

            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expires_at");
            entity.Property(e => e.SessionToken)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("session_token");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.TblSessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__TblSessio__user___571DF1D5");
        });

        modelBuilder.Entity<TblSupportRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__TblSuppo__18D3B90F1BC108EE");

            entity.ToTable("TblSupport_Requests");

            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Product)
                .HasMaxLength(255)
                .HasColumnName("product");
            entity.Property(e => e.RequestDetails).HasColumnName("request_details");
            entity.Property(e => e.RequestTitle)
                .HasMaxLength(30)
                .HasColumnName("request_title");
            entity.Property(e => e.ResolvedAt)
                .HasColumnType("datetime")
                .HasColumnName("resolved_at");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Customer).WithMany(p => p.TblSupportRequests)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__TblSuppor__custo__5FB337D6");

            entity.HasOne(d => d.Department).WithMany(p => p.TblSupportRequests)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__TblSuppor__depar__60A75C0F");
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__TblUsers__B9BE370FED024A2D");

            entity.HasIndex(e => e.Email, "UQ__TblUsers__AB6E6164149B6015").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__TblUsers__F3DBC572EE4BD978").IsUnique();

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
            entity.Property(e => e.FailedLoginAttempts)
                .HasDefaultValue(0)
                .HasColumnName("failed_login_attempts");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.IsAdmin)
                .HasDefaultValue(false)
                .HasColumnName("is_admin");
            entity.Property(e => e.LastLogin)
                .HasColumnType("datetime")
                .HasColumnName("last_login");
            entity.Property(e => e.LockoutTime)
                .HasColumnType("datetime")
                .HasColumnName("lockout_time");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
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
                .HasConstraintName("FK__TblUsers__create__4316F928");

            entity.HasOne(d => d.Department).WithMany(p => p.TblUsers)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__TblUsers__depart__4222D4EF");

            entity.HasOne(d => d.Role).WithMany(p => p.TblUsers)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__TblUsers__role_i__412EB0B6");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.InverseUpdatedUserNavigation)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK__TblUsers__update__440B1D61");
        });

        modelBuilder.Entity<TblUserPermission>(entity =>
        {
            entity.HasKey(e => e.UserPermissionId).HasName("PK__TblUser___D98F48198370CC97");

            entity.ToTable("TblUser_Permissions");

            entity.Property(e => e.UserPermissionId).HasColumnName("user_permission_id");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Permission).WithMany(p => p.TblUserPermissions)
                .HasForeignKey(d => d.PermissionId)
                .HasConstraintName("FK__TblUser_P__permi__4BAC3F29");

            entity.HasOne(d => d.User).WithMany(p => p.TblUserPermissions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__TblUser_P__user___4AB81AF0");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
