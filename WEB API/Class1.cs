using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace WEB_API
{
    public class UserController
    {

        public List<User> userList = new List<User>();

        public User Create(string login, string password, string name, int gender, DateTime? birhday, bool admin, string CreatedBy, bool isAdmin)
        {
            bool isLoginTaken = userList.Any(u => u.Login == login);
            if (isLoginTaken)
            {
                throw new ArgumentException("Логин уже занят другим пользователем.");
            }
            User user = new User()
            {
                Id = Guid.NewGuid(),
                Login = login,
                Password = password,
                Name = name,
                Gender = gender,
                BirthDay = birhday,
                CreatedBy = CreatedBy,
                CreatedOn = DateTime.Now,
                ModifiedOn = DateTime.Now,
                ModifiedBy = CreatedBy,
            };
            if (isAdmin)
            {
                user.Admin = admin;
            }
            userList.Add(user);
            return user;
        }
        public void Update(User user, string name, bool isAdmin, string requesterLogin, string requesterPassword)
        {
            if (!isAdmin && user.RevokedOn != DateTime.MinValue)
            {
                throw new InvalidOperationException("Недостаточно прав или пользователь удален.");
            }

            user.Name = name;
            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = requesterLogin;
        }
        public void Update(User user, int gender, bool isAdmin, string requesterLogin, string requesterPassword)
        {
            if (!isAdmin && user.RevokedOn != DateTime.MinValue)
            {
                throw new InvalidOperationException("Недостаточно прав или пользователь удален.");
            }

            user.Gender = gender;
            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = requesterLogin;
        }
        public void Update(User user, DateTime? birthDay, bool isAdmin, string requesterLogin, string requesterPassword)
        {
            if (!isAdmin && user.RevokedOn != DateTime.MinValue)
            {
                throw new InvalidOperationException("Недостаточно прав или пользователь удален.");
            }

            user.BirthDay = birthDay;
            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = requesterLogin;
        }

        public void Update(User user, bool isAdmin, string requesterLogin, string requesterPassword) // Восстановление пользователя
        {
            if (!isAdmin && user.RevokedOn != DateTime.MinValue)
            {
                throw new InvalidOperationException("Недостаточно прав или пользователь удален.");
            }
            user.RevokedOn = DateTime.MinValue;
            user.RevokedBy = null;
            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = requesterLogin;
        }

        public void ChangePassword(User user, string password, bool isAdmin, string requesterLogin, string requesterPassword)
        {
            if (!isAdmin && user.RevokedOn != DateTime.MinValue)
            {
                throw new InvalidOperationException("Недостаточно прав или пользователь удален.");
            }
            user.Password = password;
            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = requesterLogin;
        }

        public void ChangeLogin(User user, string login, bool isAdmin, string requesterLogin, string requesterPassword)
        {
            if (!isAdmin && user.RevokedOn != DateTime.MinValue)
            {
                throw new InvalidOperationException("Недостаточно прав или пользователь удален.");
            }

            bool isLoginTaken = userList.Any(u => u.Login == login && u.Id != user.Id);
            if (isLoginTaken)
            {
                throw new ArgumentException("Логин уже занят другим пользователем.");
            }

            user.Login = login;
            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = requesterLogin;
        }
        public List<User> Read(bool isAdmin, string requesterLogin, string requesterPassword)
        {
            if (!isAdmin)
                throw new InvalidOperationException("Недостаточно прав");
            return userList
            .Where(u => u.RevokedOn == DateTime.MinValue)
            .OrderBy(u => u.CreatedOn)
            .ToList();
        }
        public List<string> Read(string login, bool isAdmin, string requesterLogin, string requesterPassword)
        {
            if (!isAdmin)
                throw new InvalidOperationException("Недостаточно прав");
            var user = userList.FirstOrDefault(u => u.Login == login);
            if (user == null)
                throw new ArgumentException("Пользователь с таким логином не найден.");
            List<string> AboutUser = new List<string>() { user.Name, user.Gender.ToString(), user.BirthDay != null ? user.BirthDay.ToString() : "Не указана", user.RevokedOn == DateTime.MinValue ? "Активен" : "Неактивен" };
            return AboutUser;
        }
        public User Read(string login, string password, bool isAdmin, string requesterLogin, string requesterPassword)
        {
            if (requesterLogin != login)
                throw new InvalidOperationException("Недостаточно прав");
            var user = userList.FirstOrDefault(u => u.Login == login && u.Password == password && u.RevokedOn == DateTime.MinValue);
            if (user == null)
                throw new ArgumentException("Пользователь с таким логином и паролем не найден.");
            return user;
        }
        public List<User> Read(int Age, bool isAdmin, string requesterLogin, string requesterPassword)
        {
            if (!isAdmin)
                throw new InvalidOperationException("Недостаточно прав");
            var filteredUsers = userList
                                            .Where(u => u.BirthDay.HasValue &&
                                                        (DateTime.Now.Year - u.BirthDay.Value.Year) >= Age)
                                            .ToList();
            if (!filteredUsers.Any())
            {
                throw new ArgumentException($"Пользователи старше {Age} лет не найдены");
            }
            return filteredUsers;
        }
        
        public void Delete(int type, User user, string login, bool isAdmin, string requesterLogin, string requesterPassword) // type = 0 - мягкое, type = 1 полное
        {
            if (!isAdmin)
                throw new InvalidOperationException("Недостаточно прав");
            if (type == 0)
            {
                user.RevokedOn = DateTime.Now;
                user.RevokedBy = requesterLogin;
            }
            if (type == 1)
            {
                userList.Remove(user);
            }
        }
    }

    public class BaseEntity<Tkey>
    {
        public Tkey Id { get; set; }
        private string _login;
        public string Login {
            get { return _login; }
            set
            {
                if (!string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value)) _login = value;
                else throw new ArgumentException("Неправильный логин");
            }
        }
        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                if (Regex.IsMatch(value, @"^[a-zA-Z0-9]+$"))
                    _password = value;
                else throw new ArgumentException("Пароль должен содержать только английские буквы и цифры.");
            }
        }
        private string _username;
        public string Name
        {
            get { return _username; }
            set
            {
                if (Regex.IsMatch(value, @"^[a-zA-Zа-яА-Я]+$")) _username = value;
                else throw new ArgumentException("Имя должно содержать только английские и русские буквы.");
            }
        }
        private int _gender;
        public int Gender
        {
            get { return _gender; }
            set
            {
                if (value < 3 && value > -1) _gender = value;
                else throw new ArgumentException("Гендер должен быть от 0 до 2");
            }
        }
        public bool Admin { get; set; }
    }

    public class User : BaseEntity<Guid>
    {
        public DateTime? BirthDay { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime RevokedOn { get; set; }
        public string RevokedBy { get; set; }
    }
}
