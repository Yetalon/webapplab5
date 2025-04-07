using webapplab5.Models;

namespace webapplab5.Services
{
    public class LibraryService : ILibaryService
    {
        public List<Book> books = new();
        public List<User> users = new();
        public Dictionary<User, List<Book>> borrowedBooks = new();
        public async Task ReadUsers()
        {
            try
            {
                foreach (var line in File.ReadLines("./Data/Users.csv"))
                {
                    var fields = line.Split(',');

                    if (fields.Length >= 3)
                    {
                        var user = new User
                        {
                            Id = int.Parse(fields[0].Trim()),
                            Name = fields[1].Trim(),
                            Email = fields[2].Trim()
                        };

                        users.Add(user);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        public async Task WriteUsersToCsv()
        {
            var lines = users.Select(u => $"{u.Id},{u.Name},{u.Email}");
            File.WriteAllLines("./Data/Users.csv", lines);
        }
        public async Task ReadBooks()
        {
            try
            {
                foreach (var line in File.ReadLines("./Data/Books.csv"))
                {
                    var fields = line.Split(',');

                    if (fields.Length >= 4)
                    {
                        var book = new Book
                        {
                            Id = int.Parse(fields[0].Trim()),
                            Title = fields[1].Trim(),
                            Author = fields[2].Trim(),
                            ISBN = fields[3].Trim()
                        };

                        books.Add(book);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        public async Task WriteBooksToCsv()
        {
            var lines = books.Select(b => $"{b.Id},{b.Title},{b.Author},{b.ISBN}");
            File.WriteAllLines("./Data/Books.csv", lines);
        }
        public async Task<bool> AddBook(string title, string author, string isbn)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author) || string.IsNullOrWhiteSpace(isbn))
            {
                return false;
            }
            try
            {
                int id = books.Any() ? books.Max(b => b.Id) + 1 : 1;
                Book book = new Book { Id = id, Title = title, Author = author, ISBN = isbn };
                books.Add(book);
                File.AppendAllText("./Data/Books.csv", $"{book.Id},{book.Title},{book.Author},{book.ISBN}\n");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n{ex}\n");
                return false;
            }
        }
        public async Task<bool> EditBook(int? id, string title, string author, string isbn)
        {
            if (id == null) return false;
            Book? book = books.FirstOrDefault(b => b.Id == id);
            if (book != null)
            {
                book.Title = title;
                book.Author = author;
                book.ISBN = isbn;
                await WriteBooksToCsv();
                return true;
            }
            return false;
        }
        public async Task<bool> DeleteBook(int? id)
        {
            if (id == null) return false;
            Book? book = books.FirstOrDefault(b => b.Id == id);
            if (book != null)
            {
                books.Remove(book);
                await WriteBooksToCsv();
                return true;
            }
            return false;
        }
        public async Task<List<Book>> ListBooks()
        {
            return books;
        }
        public async Task<bool> AddUser(string name, string email)
        {
            try
            {
                int id = users.Any() ? users.Max(u => u.Id) + 1 : 1;
                User user = new User { Id = id, Name = name, Email = email };
                users.Add(user);
                File.AppendAllText("./Data/Users.csv", $"{user.Id},{user.Name},{user.Email}\n");
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> EditUser(int id, string name, string email)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            User? user = users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.Name = name;
                user.Email = email;
                await WriteUsersToCsv();
                return true;
            }
            return false;
        }
        public async Task<bool> DeleteUser(int id)
        {
            User? user = users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                users.Remove(user);
                await WriteUsersToCsv();
                return true;
            }
            return false;
        }
        public async Task<bool> BorrowBook(int userId, int bookId)
        {
            Book? book = books.FirstOrDefault(b => b.Id == bookId);
            if (book != null && books.Count(b => b.Id == bookId) > 0)
            {
                User? user = users.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    if (!borrowedBooks.ContainsKey(user))
                    {
                        borrowedBooks[user] = new List<Book>();
                    }
                    borrowedBooks[user].Add(book);
                    books.Remove(book);
                    WriteBooksToCsv();
                    WriteBorrowedBooksToCsv();
                    return true;
                }
            }
            return false;
        }
        public async Task<bool> ReturnBook(int userId, int bookNumber)
        {
            User? user = users.FirstOrDefault(u => u.Id == userId);
            if (user != null && borrowedBooks.ContainsKey(user) && borrowedBooks[user].Count > 0)
            {
                Book bookToReturn = borrowedBooks[user][bookNumber - 1];
                borrowedBooks[user].RemoveAt(bookNumber - 1);
                books.Add(bookToReturn);
                WriteBooksToCsv();
                WriteBorrowedBooksToCsv();
                return true;
            }
            return false;
        }
        public async Task<Dictionary<User, List<Book>>> ListBorrowedBooks()
        {
            return borrowedBooks;
        }
        public async Task ReadBorrowedBooksFromCsv()
        {
            try
            {
                var lines = File.ReadLines("./Data/Borrowed.csv").ToList();
                Dictionary<User, List<Book>> borrowedBooks = new();
                User currentUser = null;
                List<Book> currentBooks = new();
                foreach (var line in lines)
                {
                    var fields = line.Split(',');
                    if (fields.Length == 3)
                    {
                        if (currentUser != null)
                        {
                            borrowedBooks.Add(currentUser, currentBooks);
                        }
                        currentUser = new User
                        {
                            Id = int.Parse(fields[0]),
                            Name = fields[1],
                            Email = fields[2]
                        };
                        currentBooks = new List<Book>();  
                    }
                    else if (fields.Length == 4 && currentUser != null)
                    {
                        var book = new Book
                        {
                            Id = int.Parse(fields[0]),
                            Title = fields[1],
                            Author = fields[2],
                            ISBN = fields[3]
                        };

                        currentBooks.Add(book);
                    }
                }
                if (currentUser != null)
                {
                    borrowedBooks.Add(currentUser, currentBooks);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public async Task WriteBorrowedBooksToCsv()
        {
            var lines = borrowedBooks.Select(borrowed => {
                var userLine = $"{borrowed.Key.Id},{borrowed.Key.Name},{borrowed.Key.Email}";
                var booksLines = borrowed.Value.Select(b => $"{b.Id},{b.Title},{b.Author},{b.ISBN}").ToList();
                return $"{userLine}\n{string.Join("\n", booksLines)}";
            });
            File.WriteAllLines("./Data/Borrowed.csv", lines);
        }
    }
}
