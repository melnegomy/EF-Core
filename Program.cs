using BookstoreApp.Data;
using BookstoreApp.Entities;
using Microsoft.EntityFrameworkCore;

#region Entities

namespace BookstoreApp.Entities
{
    #region Book
    public class Book
    {
        #region Primary Key
        public int Id { get; set; }
        #endregion

        #region Properties
        public string Title { get; set; }
        public string ISBN { get; set; }
        public decimal Price { get; set; }
        public int Pages { get; set; }
        public int PublishedYear { get; set; }
        public bool IsInStock { get; set; }
        #endregion

        #region Foreign Keys
        public int AuthorId { get; set; }
        public int CategoryId { get; set; }
        #endregion

        #region Navigation Properties
        public Author Author { get; set; }
        public Category Category { get; set; }
        #endregion
    }
    #endregion

    #region Author
    public class Author
    {
        #region Primary Key
        public int Id { get; set; }
        #endregion

        #region Properties
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Biography { get; set; }
        public DateTime DateOfBirth { get; set; }
        #endregion

        #region Navigation Properties
        public ICollection<Book> Books { get; set; }
        #endregion
    }
    #endregion

    #region Category
    public class Category
    {
        #region Primary Key
        public int Id { get; set; }
        #endregion

        #region Properties
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        #endregion

        #region Navigation Properties
        public ICollection<Book> Books { get; set; }
        #endregion
    }
    #endregion
}

#endregion

#region DbContext

namespace BookstoreApp.Data
{
    public class BookstoreDbContext : DbContext
    {
        #region DbSets
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Category> Categories { get; set; }
        #endregion

        #region OnConfiguring
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Server=.;Database=ReadMoreBooksDB;Trusted_Connection=True;TrustServerCertificate=True;"
            );
        }
        #endregion
    }
}

#endregion

#region Program

class Program
{
    static void Main(string[] args)
    {
        #region Create Database
        using (var context = new BookstoreDbContext())
        {
            context.Database.EnsureCreated();
            Console.WriteLine("Database created successfully!");
        }
        #endregion

        #region Seed Sample Data
        using (var context = new BookstoreDbContext())
        {
            if (!context.Authors.Any())
            {
                var author = new Author
                {
                    FirstName = "Ahmed",
                    LastName = "Hassan",
                    Email = "ahmed@readmore.com",
                    Biography = "Bestselling author of fiction novels.",
                    DateOfBirth = new DateTime(1985, 6, 15)
                };

                var category = new Category
                {
                    Name = "Fiction",
                    Description = "Fictional stories and novels.",
                    IsActive = true
                };

                var book = new Book
                {
                    Title = "The Lost City",
                    ISBN = "978-3-16-148410-0",
                    Price = 19.99m,
                    Pages = 320,
                    PublishedYear = 2022,
                    IsInStock = true,
                    Author = author,
                    Category = category
                };

                context.Books.Add(book);
                context.SaveChanges();
                Console.WriteLine("Sample data seeded successfully!");
            }
        }
        #endregion

        #region Display Data
        using (var context = new BookstoreDbContext())
        {
            Console.WriteLine("\nBooks in Database:");
            Console.WriteLine("".PadRight(50, '-'));

            var books = context.Books
                               .Select(b => new
                               {
                                   b.Title,
                                   b.ISBN,
                                   b.Price,
                                   b.Pages,
                                   b.PublishedYear,
                                   b.IsInStock
                               }).ToList();

            foreach (var book in books)
            {
                Console.WriteLine($"Title    : {book.Title}");
                Console.WriteLine($"ISBN     : {book.ISBN}");
                Console.WriteLine($"Price    : ${book.Price}");
                Console.WriteLine($"Pages    : {book.Pages}");
                Console.WriteLine($"Year     : {book.PublishedYear}");
                Console.WriteLine($"In Stock : {(book.IsInStock ? "Yes" : "No")}");
                Console.WriteLine("".PadRight(50, '-'));
            }
        }
        #endregion

        Console.ReadLine();
    }
}

#endregion
