using WEB_API;

namespace Tests
{
    public class Tests
    {
        private readonly UserController _controller;

        public Tests()
        {
            _controller = new UserController();
        }

        [Theory]
        [InlineData("Derzkiy", "1234", "Дима", 0, "1990-01-01", false, "Бог", true)]
        [InlineData("Biba", "5689", "Мария", 1, null, true, "Я", true)]
        [InlineData("Boba", "7890", "Леха", 2, "2000-05-15", false, "Пользователь", false)]
        public void Create_WithValidData_ReturnsUserWithCorrectProperties(
            string login, string password, string name, int gender,
            string birthdayStr, bool admin, string createdBy, bool isAdmin)
        {
            DateTime? birthday = DateTime.TryParse(birthdayStr, out var date) ? date : null;

            var user = _controller.Create(login, password, name, gender, birthday, admin, createdBy, isAdmin);

            Assert.Equal(login, user.Login);
            Assert.Equal(password, user.Password);
            Assert.Equal(name, user.Name);
            Assert.Equal(gender, user.Gender);
            Assert.Equal(birthday, user.BirthDay);
            Assert.Equal(isAdmin && admin, user.Admin);
            Assert.Equal(createdBy, user.CreatedBy);
            Assert.Equal(DateTime.Now.Date, user.CreatedOn.Date);
            Assert.Single(_controller.userList);
        }

        [Theory]
        [InlineData("", "1234", "Дима", 0, "1990-01-01", false, "Бог", true)]
        [InlineData("   ", "5689", "Мария", 1, "1990-01-01", true, "Я", true)]
        [InlineData(null, "7890", "Леха", 2, "2000-05-15", false, "Пользователь", false)]
        [InlineData("Derzkiy", "!!!!!!", "Дима", 0, "1990-01-01", false, "Бог", true)]
        [InlineData("Biba", "А я рыба я рыба", "Мария", 1, "1990-01-01", true, "Я", true)]
        [InlineData("Boba", "$212466^$", "Леха", 2, "2000-05-15", false, "Пользователь", false)]
        [InlineData("Boba", "", "Леха", 2, "2000-05-15", false, "Пользователь", false)]
        [InlineData("Boba", " ", "Леха", 2, "2000-05-15", false, "Пользователь", false)]
        [InlineData("Boba", null, "Леха", 2, "2000-05-15", false, "Пользователь", false)]
        public void Create_WithInValidData_ThrowsArgumentException(
            string login, string password, string name, int gender,
            string birthdayStr, bool admin, string createdBy, bool isAdmin)
        {
            DateTime? birthday = DateTime.TryParse(birthdayStr, out var date) ? date : null;

            Assert.Throws<ArgumentException>(() =>
                        _controller.Create(login, password, name, gender, birthday, admin, createdBy, isAdmin));
            Assert.Empty(_controller.userList);
        }

        [Theory]
        [InlineData("Derzkiy", "1234", "Дима", 0, "1990-01-01", false, "Бог", true)]
        [InlineData("Biba", "5689", "Мария", 1, "1990-01-01", true, "Я", true)]
        [InlineData("Boba", "7890", "Леха", 2, "2000-05-15", false, "Пользователь", false)]
        public void Create_WithValidData_UpdateWithAdminRole_ReturnsUserWithUpdatedProperties(
    string login, string password, string name, int gender,
    string strBirthday, bool admin, string createdBy, bool isAdmin)
        {
            var birthday = DateTime.Parse(strBirthday);

            var user = _controller.Create(login, password, name, gender, birthday, admin, createdBy, isAdmin);

            _controller.Update(user, 1, true, "Admin", "123456");
            _controller.Update(user, "Pupa", true, "Admin", "123456");
            _controller.Update(user, DateTime.Parse("2011-05-15"), true, "Admin", "123456");
            _controller.ChangePassword(user, "2281337666", true, "Admin", "123456");
            _controller.ChangeLogin(user, "Boba", true, "Admin", "123456");

            Assert.Equal("Boba", user.Login);
            Assert.Equal("2281337666", user.Password);
            Assert.Equal(DateTime.Parse("2011-05-15"), user.BirthDay);
            Assert.Equal("Pupa", user.Name);
            Assert.Equal(1, user.Gender);
        }

        [Theory]
        [InlineData("Derzkiy", "1234", "Дима", 0, "1990-01-01", false, "Бог", true)]
        [InlineData("Biba", "5689", "Мария", 1, "1990-01-01", true, "Я", true)]
        [InlineData("Boba", "7890", "Леха", 2, "2000-05-15", false, "Пользователь", false)]
        public void Create_WithValidData_UpdateWithoutAdminRole_ThrowsArgumentException(
string login, string password, string name, int gender,
string strBirthday, bool admin, string createdBy, bool isAdmin)
        {
            var birthday = DateTime.Parse(strBirthday);

            var user = _controller.Create(login, password, name, gender, birthday, admin, createdBy, isAdmin);

            Assert.Throws<InvalidOperationException>(() => _controller.Update(user, 1, false, "Admin", "123456"));
            Assert.Throws<InvalidOperationException>(() => _controller.Update(user, "Pupa", false, "Admin", "123456"));
            Assert.Throws<InvalidOperationException>(() => _controller.Update(user, DateTime.Parse("2011-05-15"), false, "Admin", "123456"));
            Assert.Throws<InvalidOperationException>(() => _controller.ChangePassword(user, "2281337666", false, "Admin", "123456"));
            Assert.Throws<InvalidOperationException>(() => _controller.ChangeLogin(user, "Boba", false, "Admin", "123456"));
        }

        [Theory]
        [InlineData("Derzkiy", "1234", "Дима", 0, "1990-01-01", false, "Бог", true)]
        [InlineData("Biba", "5689", "Мария", 1, "1990-01-01", true, "Я", true)]
        [InlineData("Boba", "7890", "Леха", 2, "2000-05-15", false, "Пользователь", false)]
        public void Create_WithValidData_LightDeleteUsers(
string login, string password, string name, int gender,
string strBirthday, bool admin, string createdBy, bool isAdmin)
        {
            var birthday = DateTime.Parse(strBirthday);

            var user = _controller.Create(login, password, name, gender, birthday, admin, createdBy, isAdmin);

            _controller.Delete(0, user.Login, true, "Admin", "123456");
            Assert.NotNull(user.RevokedBy);
            Assert.NotEqual(user.RevokedOn, DateTime.MinValue);

        }

        [Theory]
        [InlineData("Derzkiy", "1234", "Дима", 0, "1990-01-01", false, "Бог", true)]
        [InlineData("Biba", "5689", "Мария", 1, "1990-01-01", true, "Я", true)]
        [InlineData("Boba", "7890", "Леха", 2, "2000-05-15", false, "Пользователь", false)]
        public void Create_WithValidData_HardDeleteUsers(
string login, string password, string name, int gender,
string strBirthday, bool admin, string createdBy, bool isAdmin)
        {
            var birthday = DateTime.Parse(strBirthday);

            var user = _controller.Create(login, password, name, gender, birthday, admin, createdBy, isAdmin);

            _controller.Delete(1, user.Login, true, "Admin", "123456");
            Assert.Empty(_controller.userList);

        }

        [Theory]
        [InlineData("Derzkiy", "1234", "Дима", 0, "1990-01-01", false, "Бог", true)]
        [InlineData("Biba", "5689", "Мария", 1, "1990-01-01", true, "Я", true)]
        [InlineData("Boba", "7890", "Леха", 2, "2000-05-15", false, "Пользователь", false)]
        public void Create_WithValidData_HardDeleteUsers_WithoutAdminRole_ThrowsInvalidOperationException(
string login, string password, string name, int gender,
string strBirthday, bool admin, string createdBy, bool isAdmin)
        {
            var birthday = DateTime.Parse(strBirthday);

            var user = _controller.Create(login, password, name, gender, birthday, admin, createdBy, isAdmin);

            Assert.Throws<InvalidOperationException>(() => _controller.Delete(1, user.Login, false, "Admin", "123456"));

        }


        [Theory]
        [InlineData("Derzkiy", "1234", "Дима", 0, "1990-01-01", false, "Бог", true)]
        [InlineData("Biba", "5689", "Мария", 1, "1990-01-01", true, "Я", true)]
        [InlineData("Boba", "7890", "Леха", 2, "2000-05-15", false, "Пользователь", false)]
        public void Create_WithValidData_LightDeleteUsers_WithoutAdminRole_ThrowsInvalidOperationException(
string login, string password, string name, int gender,
string strBirthday, bool admin, string createdBy, bool isAdmin)
        {
            var birthday = DateTime.Parse(strBirthday);

            var user = _controller.Create(login, password, name, gender, birthday, admin, createdBy, isAdmin);

            Assert.Throws<InvalidOperationException>(() => _controller.Delete(0, user.Login, false, "Admin", "123456"));

        }
        [Fact]
        public void HardDelete_NotExsistUsers()
        {

            Assert.Throws<InvalidOperationException>(() => _controller.Delete(1, "Vasya", false, "Admin", "123456"));

        }
        [Fact]
        public void LightDelete_NotExsistUsers()
        {

            Assert.Throws<InvalidOperationException>(() => _controller.Delete(0, "Vasya", false, "Admin", "123456"));

        }
        [Fact]
        public void ReadAllActiveUsers_WithAdminRole_ReturnsActiveUsersOrderedByCreatedDate()
        {
            _controller.Create("admin", "admin123", "Admin", 0, DateTime.Parse("1990-01-01"), true, "System", true);
            _controller.Create("user1", "pass1", "Вася", 1, DateTime.Parse("1995-01-01"), false, "admin", true);
            _controller.Create("user2", "pass2", "Петя", 2, null, false, "admin", true);

            var result = _controller.Read(true, "admin", "admin123");

            Assert.Equal(3, result.Count);
            Assert.Equal("admin", result[0].Login);
            Assert.Equal("user1", result[1].Login);
        }
    }
}