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
        [InlineData("   ", "5689", "Мария", 1, null, true, "Я", true)]
        [InlineData(null, "7890", "Леха", 2, "2000-05-15", false, "Пользователь", false)]
        public void Create_WithInValidLogin_ThrowsArgumentException(
            string login, string password, string name, int gender,
            string birthdayStr, bool admin, string createdBy, bool isAdmin)
        {
            var exception = "Неправильный логин";
            DateTime? birthday = DateTime.TryParse(birthdayStr, out var date) ? date : null;

            var ex = Assert.Throws<ArgumentException>(() =>
                        _controller.Create(login, password, name, gender, birthday, admin, createdBy, isAdmin));

            Assert.Equal(exception, ex.Message);
            Assert.Empty(_controller.userList);
        }
    }
}