using webapplab5.Models;

namespace webapplab5.Services {
    public interface ILibaryService {
       public Task ReadBooks();
        public Task ReadUsers();
        public Task<bool> AddBook(string title, string author, string isbn);
        public Task<List<Book>> ListBooks();
        public Task<bool> EditBook(int? id, string title, string author, string isbn);
        public Task<bool> DeleteBook(int? id);
        public Task<bool> AddUser(string name, string email);
        public Task<bool> EditUser(int id, string name, string email);
        public Task<bool> DeleteUser(int id);
        public Task<bool> BorrowBook(int userId, int bookId);
        public Task<bool> ReturnBook(int userId, int bookId);
    }
}