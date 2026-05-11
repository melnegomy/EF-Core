using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

// ==================== ENTITIES ====================

public enum CustomerType { Individual, Business }
public enum AccountType { Savings, Current, Business }
public enum OwnershipType { Primary, CoHolder }
public enum AccountStatus { Active, Closed }
public enum TransactionType { Deposit, Withdrawal, Transfer, Payment }

public class Branch
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public Manager Manager { get; set; }
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}

public class Manager
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime HireDate { get; set; }
    public string BranchCode { get; set; }
    public Branch Branch { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string NationalId { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public CustomerType CustomerType { get; set; }
    public ICollection<CustomerAccount> CustomerAccounts { get; set; } = new List<CustomerAccount>();
}

public class Account
{
    public string AccountNumber { get; set; }
    public AccountType AccountType { get; set; }
    public DateTime OpeningDate { get; set; }
    public decimal CurrentBalance { get; set; }
    public string BranchCode { get; set; }
    public Branch Branch { get; set; }
    public ICollection<CustomerAccount> CustomerAccounts { get; set; } = new List<CustomerAccount>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

public class CustomerAccount
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }
    public string AccountNumber { get; set; }
    public Account Account { get; set; }
    public OwnershipType OwnershipType { get; set; }
    public DateTime OwnershipStartDate { get; set; }
    public AccountStatus AccountStatus { get; set; }
}

public class Transaction
{
    public string TransactionNumber { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public TransactionType TransactionType { get; set; }
    public string Note { get; set; }
    public string AccountNumber { get; set; }
    public Account Account { get; set; }
}

// ==================== DB CONTEXT ====================

public class BankDbContext : DbContext
{
    public DbSet<Branch> Branches { get; set; }
    public DbSet<Manager> Managers { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<CustomerAccount> CustomerAccounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer("Server=.;Database=NationalBankDB;Trusted_Connection=True;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Branch>(e =>
        {
            e.HasKey(x => x.Code);
            e.Property(x => x.Code).HasMaxLength(20);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Address).HasMaxLength(200);
            e.Property(x => x.PhoneNumber).HasMaxLength(20);
        });

        b.Entity<Manager>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).IsRequired().HasMaxLength(100);
            e.Property(x => x.Email).HasMaxLength(100);
            e.Property(x => x.PhoneNumber).HasMaxLength(20);
            e.HasOne(x => x.Branch)
             .WithOne(x => x.Manager)
             .HasForeignKey<Manager>(x => x.BranchCode)
             .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Customer>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).IsRequired().HasMaxLength(100);
            e.Property(x => x.NationalId).IsRequired().HasMaxLength(20);
            e.HasIndex(x => x.NationalId).IsUnique();
            e.Property(x => x.Email).HasMaxLength(100);
            e.Property(x => x.PhoneNumber).HasMaxLength(20);
            e.Property(x => x.Address).HasMaxLength(200);
            e.Property(x => x.CustomerType).HasConversion<string>();
        });

        b.Entity<Account>(e =>
        {
            e.HasKey(x => x.AccountNumber);
            e.Property(x => x.AccountNumber).HasMaxLength(30);
            e.Property(x => x.AccountType).HasConversion<string>();
            e.Property(x => x.CurrentBalance).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Branch)
             .WithMany(x => x.Accounts)
             .HasForeignKey(x => x.BranchCode)
             .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<CustomerAccount>(e =>
        {
            e.HasKey(x => new { x.CustomerId, x.AccountNumber });
            e.Property(x => x.OwnershipType).HasConversion<string>();
            e.Property(x => x.AccountStatus).HasConversion<string>();
            e.HasOne(x => x.Customer)
             .WithMany(x => x.CustomerAccounts)
             .HasForeignKey(x => x.CustomerId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Account)
             .WithMany(x => x.CustomerAccounts)
             .HasForeignKey(x => x.AccountNumber)
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Transaction>(e =>
        {
            e.HasKey(x => x.TransactionNumber);
            e.Property(x => x.TransactionNumber).HasMaxLength(30);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.TransactionType).HasConversion<string>();
            e.Property(x => x.Note).HasMaxLength(300);
            e.HasOne(x => x.Account)
             .WithMany(x => x.Transactions)
             .HasForeignKey(x => x.AccountNumber)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ==================== SEED ====================

        b.Entity<Branch>().HasData(
            new Branch { Code = "CAI-01", Name = "Cairo Main Branch", Address = "10 Tahrir Square, Cairo", PhoneNumber = "0223456789" },
            new Branch { Code = "ALX-01", Name = "Alexandria Branch", Address = "5 Corniche St, Alexandria", PhoneNumber = "0312345678" },
            new Branch { Code = "GIZ-01", Name = "Giza Branch", Address = "22 Pyramids Rd, Giza", PhoneNumber = "0238765432" }
        );

        b.Entity<Manager>().HasData(
            new Manager { Id = 1, FullName = "Ahmed Ali", Email = "ahmed.ali@nationalbank.eg", PhoneNumber = "01001234567", HireDate = new DateTime(2015, 3, 10), BranchCode = "CAI-01" },
            new Manager { Id = 2, FullName = "Sara Hassan", Email = "sara.hassan@nationalbank.eg", PhoneNumber = "01009876543", HireDate = new DateTime(2018, 7, 1), BranchCode = "ALX-01" },
            new Manager { Id = 3, FullName = "Omar Youssef", Email = "omar.y@nationalbank.eg", PhoneNumber = "01112223344", HireDate = new DateTime(2020, 1, 15), BranchCode = "GIZ-01" }
        );
    }
}

// ==================== MENU ====================
public class Program
{
    static BankDbContext db = new BankDbContext();

    public static void Main(string[] args)
    {
        db.Database.EnsureCreated();
        db.Database.EnsureCreated();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("       National Bank — Management       ");
            Console.WriteLine("========================================");
            Console.WriteLine("  1) Add a new Customer");
            Console.WriteLine("  2) Open a new Account for a Customer");
            Console.WriteLine("  3) Update Account Status (Active / Closed)");
            Console.WriteLine("  4) Remove an Account from a Customer");
            Console.WriteLine("  5) List all Customers (with accounts)");
            Console.WriteLine("  0) Exit");
            Console.WriteLine("----------------------------------------");
            Console.Write("  Enter choice: ");

            if (!int.TryParse(Console.ReadLine(), out var choice))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid input. Please enter a number.");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to return to the menu...");
                Console.ReadKey();
                continue;
            }

            switch (choice)
            {
                case 1: AddCustomer(); break;
                case 2: OpenAccount(); break;
                case 3: UpdateAccountStatus(); break;
                case 4: RemoveAccountFromCustomer(); break;
                case 5: ListCustomers(); break;
                case 0: Console.WriteLine("Goodbye!"); return;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid choice.");
                    Console.ResetColor();
                    break;
            }

            Console.WriteLine("\nPress any key to return to the menu...");
            Console.ReadKey();
        }

        // ==================== OPERATIONS ====================

        void AddCustomer()
        {
            Console.WriteLine("\n--- Add New Customer ---");
            Console.Write("Full Name      : "); var name = Console.ReadLine();
            Console.Write("National ID    : "); var nid = Console.ReadLine();
            Console.Write("Date of Birth  : (yyyy-MM-dd) "); var dobStr = Console.ReadLine();
            Console.Write("Email          : "); var email = Console.ReadLine();
            Console.Write("Phone          : "); var phone = Console.ReadLine();
            Console.Write("Address        : "); var address = Console.ReadLine();

            if (!DateTime.TryParse(dobStr, out var dob))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid date format.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine("Customer Type:\n    1) Individual\n    2) Business");
            Console.Write("  Choice: ");
            if (!int.TryParse(Console.ReadLine(), out var typeChoice) || typeChoice < 1 || typeChoice > 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid choice.");
                Console.ResetColor();
                return;
            }

            var customer = new Customer
            {
                FullName = name,
                NationalId = nid,
                DateOfBirth = dob,
                Email = email,
                PhoneNumber = phone,
                Address = address,
                CustomerType = typeChoice == 1 ? CustomerType.Individual : CustomerType.Business
            };

            db.Customers.Add(customer);
            db.SaveChanges();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nCustomer created successfully. CustomerId = {customer.Id}");
            Console.ResetColor();
        }

        void OpenAccount()
        {
            Console.WriteLine("\n--- Open New Account ---");
            Console.Write("Account Number : "); var accNum = Console.ReadLine();
            Console.WriteLine("Account Type:\n    1) Savings\n    2) Current\n    3) Business");
            Console.Write("  Choice: ");

            if (!int.TryParse(Console.ReadLine(), out var typeChoice) || typeChoice < 1 || typeChoice > 3)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid account type.");
                Console.ResetColor();
                return;
            }

            Console.Write("Branch Code    : "); var branchCode = Console.ReadLine();
            Console.Write("Customer Id    : ");
            if (!int.TryParse(Console.ReadLine(), out var customerId))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid customer ID.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine("Ownership Role:\n    1) Primary\n    2) CoHolder");
            Console.Write("  Choice: ");
            if (!int.TryParse(Console.ReadLine(), out var roleChoice) || roleChoice < 1 || roleChoice > 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid ownership role.");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Validating branch '{branchCode}' and customer #{customerId}...");
            Console.ResetColor();

            if (db.Branches.Find(branchCode) == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Branch '{branchCode}' not found.");
                Console.ResetColor();
                return;
            }

            if (db.Customers.Find(customerId) == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Customer #{customerId} not found.");
                Console.ResetColor();
                return;
            }

            var accountType = typeChoice switch { 1 => AccountType.Savings, 2 => AccountType.Current, _ => AccountType.Business };
            var ownershipType = roleChoice == 1 ? OwnershipType.Primary : OwnershipType.CoHolder;

            db.Accounts.Add(new Account
            {
                AccountNumber = accNum,
                AccountType = accountType,
                OpeningDate = DateTime.Now,
                CurrentBalance = 0,
                BranchCode = branchCode
            });

            db.CustomerAccounts.Add(new CustomerAccount
            {
                CustomerId = customerId,
                AccountNumber = accNum,
                OwnershipType = ownershipType,
                OwnershipStartDate = DateTime.Now,
                AccountStatus = AccountStatus.Active
            });

            db.SaveChanges();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Account '{accNum}' created and linked to customer {customerId} as {ownershipType} owner.");
            Console.ResetColor();
        }

        void UpdateAccountStatus()
        {
            Console.WriteLine("\n--- Update Account Status ---");
            Console.Write("Account Number : "); var accNum = Console.ReadLine();
            Console.Write("Customer Id    : ");
            if (!int.TryParse(Console.ReadLine(), out var customerId))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid customer ID.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine("New Status:\n    1) Active\n    2) Closed");
            Console.Write("  Choice: ");
            if (!int.TryParse(Console.ReadLine(), out var statusChoice) || statusChoice < 1 || statusChoice > 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid status choice.");
                Console.ResetColor();
                return;
            }

            var link = db.CustomerAccounts.Find(customerId, accNum);
            if (link == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No matching account-customer link found.");
                Console.ResetColor();
                return;
            }

            link.AccountStatus = statusChoice == 1 ? AccountStatus.Active : AccountStatus.Closed;
            db.SaveChanges();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Status updated to {link.AccountStatus}.");
            Console.ResetColor();
        }

        void RemoveAccountFromCustomer()
        {
            Console.WriteLine("\n--- Remove Account From Customer ---");
            Console.Write("Account Number : "); var accNum = Console.ReadLine();
            Console.Write("Customer Id    : ");
            if (!int.TryParse(Console.ReadLine(), out var customerId))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid customer ID.");
                Console.ResetColor();
                return;
            }

            var link = db.CustomerAccounts.Find(customerId, accNum);
            if (link == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No matching account-customer link found.");
                Console.ResetColor();
                return;
            }

            db.CustomerAccounts.Remove(link);
            db.SaveChanges();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  Ownership link deleted.");
            Console.ResetColor();

            if (!db.CustomerAccounts.Any(ca => ca.AccountNumber == accNum))
            {
                var account = db.Accounts.Find(accNum);
                if (account != null) db.Accounts.Remove(account);
                db.SaveChanges();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"      That was the last owner — account '{accNum}' was also removed.");
                Console.ResetColor();
            }
        }

        void ListCustomers()
        {
            Console.WriteLine("\n--- All Customers ---\n");

            var customers = db.Customers
                .Include(c => c.CustomerAccounts)
                    .ThenInclude(ca => ca.Account)
                        .ThenInclude(a => a.Branch)
                .ToList();

            foreach (var c in customers)
            {
                Console.WriteLine($"  #{c.Id} {c.FullName} ({c.CustomerType})");
                if (!c.CustomerAccounts.Any())
                {
                    Console.WriteLine("      (no accounts)");
                }
                else
                {
                    foreach (var ca in c.CustomerAccounts)
                    {
                        var a = ca.Account;
                        Console.WriteLine($"      {a.AccountNumber,-12} {a.AccountType,-10} Balance: {a.CurrentBalance,12:F2}  {ca.OwnershipType,-10} {ca.AccountStatus,-8} @ {a.Branch?.Name}");
                    }
                }
            }
        }

    }
}