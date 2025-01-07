using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Customer_sweetsoft_tech_support.Models;

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

    public virtual DbSet<TblRequestFeedback> TblRequestFeedbacks { get; set; }

    public virtual DbSet<TblRequestsProcessing> TblRequestsProcessings { get; set; }

    public virtual DbSet<TblRole> TblRoles { get; set; }

    public virtual DbSet<TblSupportRequest> TblSupportRequests { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=db12358.databaseasp.net; Database=db12358; User Id=db12358; Password=Y#p3=5Zg9Rm%; Encrypt=False; MultipleActiveResultSets=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblCustomer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__TblCusto__CD65CB859525CF1B");

            entity.HasIndex(e => e.Email, "UQ__TblCusto__AB6E6164612D0EC0").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__TblCusto__F3DBC572D8A403FA").IsUnique();

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
            entity.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .HasColumnName("is_delete");
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
                .HasConstraintName("FK__TblCustom__creat__571DF1D5");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.TblCustomerUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__TblCustom__updat__5812160E");
        });

        modelBuilder.Entity<TblDepartment>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__TblDepar__C2232422EAC7869C");

            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(100)
                .HasColumnName("department_name");
            entity.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .HasColumnName("is_delete");
            entity.Property(e => e.Status).HasColumnName("status");
        });

        modelBuilder.Entity<TblFaq>(entity =>
        {
            entity.HasKey(e => e.FaqId).HasName("PK__TblFaqs__66734BAFA423898C");

            entity.Property(e => e.FaqId).HasColumnName("faq_id");
            entity.Property(e => e.Answer).HasColumnName("answer");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.FaqThumbnail)
                .HasMaxLength(255)
                .HasColumnName("faq_thumbnail");
            entity.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .HasColumnName("is_delete");
            entity.Property(e => e.Question)
                .HasMaxLength(255)
                .HasColumnName("question");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<TblRequestFeedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__TblReque__7A6B2B8C4D72B22A");

            entity.ToTable("TblRequest_Feedbacks");

            entity.Property(e => e.FeedbackId).HasColumnName("feedback_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Feedback).HasColumnName("feedback");
            entity.Property(e => e.FeedbackType).HasColumnName("feedback_type");
            entity.Property(e => e.FromCustomerId).HasColumnName("from_customer_id");
            entity.Property(e => e.FromUserId).HasColumnName("from_user_id");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.ToCustomerId).HasColumnName("to_customer_id");
            entity.Property(e => e.ToUserId).HasColumnName("to_user_id");

            entity.HasOne(d => d.FromCustomer).WithMany(p => p.TblRequestFeedbackFromCustomers)
                .HasForeignKey(d => d.FromCustomerId)
                .HasConstraintName("FK__TblReques__from___71D1E811");

            entity.HasOne(d => d.FromUser).WithMany(p => p.TblRequestFeedbackFromUsers)
                .HasForeignKey(d => d.FromUserId)
                .HasConstraintName("FK__TblReques__from___70DDC3D8");

            entity.HasOne(d => d.Request).WithMany(p => p.TblRequestFeedbacks)
                .HasForeignKey(d => d.RequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TblReques__reque__6FE99F9F");

            entity.HasOne(d => d.ToCustomer).WithMany(p => p.TblRequestFeedbackToCustomers)
                .HasForeignKey(d => d.ToCustomerId)
                .HasConstraintName("FK__TblReques__to_cu__73BA3083");

            entity.HasOne(d => d.ToUser).WithMany(p => p.TblRequestFeedbackToUsers)
                .HasForeignKey(d => d.ToUserId)
                .HasConstraintName("FK__TblReques__to_us__72C60C4A");
        });

        modelBuilder.Entity<TblRequestsProcessing>(entity =>
        {
            entity.HasKey(e => e.ProcessId).HasName("PK__TblReque__9446C3E15B5ABDFD");

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
                .HasConstraintName("FK__TblReques__depar__6B24EA82");

            entity.HasOne(d => d.Request).WithMany(p => p.TblRequestsProcessings)
                .HasForeignKey(d => d.RequestId)
                .HasConstraintName("FK__TblReques__reque__6A30C649");
        });

        modelBuilder.Entity<TblRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__TblRoles__760965CC9A12B7E6");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(100)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<TblSupportRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__TblSuppo__18D3B90F980F3869");

            entity.ToTable("TblSupport_Requests");

            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .HasColumnName("is_delete");
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
                .HasConstraintName("FK__TblSuppor__custo__5BE2A6F2");

            entity.HasOne(d => d.Department).WithMany(p => p.TblSupportRequests)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__TblSuppor__depar__5CD6CB2B");
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__TblUsers__B9BE370F78F94392");

            entity.HasIndex(e => e.Email, "UQ__TblUsers__AB6E61646791A1E8").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__TblUsers__F3DBC572C80E810B").IsUnique();

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
            entity.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .HasColumnName("is_delete");
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
                .HasConstraintName("FK__TblUsers__create__44FF419A");

            entity.HasOne(d => d.Department).WithMany(p => p.TblUsers)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__TblUsers__depart__440B1D61");

            entity.HasOne(d => d.Role).WithMany(p => p.TblUsers)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__TblUsers__role_i__4316F928");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.InverseUpdatedUserNavigation)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK__TblUsers__update__45F365D3");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
