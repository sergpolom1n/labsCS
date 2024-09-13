using ValidatorDLL;
using System.Text.RegularExpressions;

namespace DataValidatorDemo
{
    class Program
    {
        #region RealizationData
        public class UserData
        {
            public string? Username { get; set; }
            public string? Fullname { get; set; }
            public string? Email { get; set; }
            public string? Password { get; set; }

        }

        public static UserData ParseUserData(List<string> data)
        {
            var userData = new UserData();

            foreach (var item in data)
            {
                var keyValue = item.Split(':');
                if (keyValue.Length == 2)
                {
                    switch (keyValue[0].Trim())
                    {
                        case "username":
                            userData.Username = keyValue[1].Trim();
                            break;
                        case "fullname":
                            userData.Fullname = keyValue[1].Trim();
                            break;
                        case "email":
                            userData.Email = keyValue[1].Trim();
                            break;
                        case "password":
                            userData.Password = keyValue[1].Trim();
                            break;
                    }
                }
            }

            return userData;
        }

        #endregion

        #region rules

        public class SampleValidationRule : IValidationRule<string>
        {
            public void Validate(string data)
            {
                if (string.IsNullOrWhiteSpace(data))
                {
                    throw new ValidationException("Data cannot be null or whitespace.");
                }

            }
        }
        public class UsernameValidationRule : IValidationRule<UserData>
        {
            public void Validate(UserData data)
            {
                if (string.IsNullOrWhiteSpace(data.Username))
                {
                    throw new ValidationException("The Username cannot be null or whitespace.");
                }
                if (!Regex.IsMatch(data.Username, @"^[a-zA-Z0-9_]+$"))
                {
                    throw new ValidationException("The username must contain only English letters and numbers (underscores \"_\" are allowed)");
                }
                if (data.Username.Length < 7)
                {
                    throw new ValidationException("The username must contain a minimum of 7 characters.");
                }
            }
        }

        public class FullnameValidationRule : IValidationRule<UserData>
        {
            public void Validate(UserData data)
            {
                if (string.IsNullOrWhiteSpace(data.Fullname))
                {
                    throw new ValidationException("The fullname cannot be null or whitespace.");
                }

                string[] fullnameArray = data.Fullname.Split(' ');

                foreach (string namePart in fullnameArray)
                {
                    // Проверяем первую букву на заглавную
                    if (!char.IsUpper(namePart[0]))
                    {
                        throw new ValidationException("The first letter of each name part in fullname must be uppercase.");
                    }

                    // Проверяем остальные буквы на строчные
                    for (int i = 1; i < namePart.Length; i++)
                    {
                        if (!char.IsLower(namePart[i]))
                        {
                            throw new ValidationException("The subsequent letters of each name part in fullname must be lowercase.");
                        }
                    }

                }


                if (data.Fullname.Length > 100)
                {
                    throw new ValidationException("The username must contain a maxsimum 100 of characters.");
                }

            }
        }

        public class EmailValidationRule : IValidationRule<UserData>
        {
            public void Validate(UserData data)
            {
                if (string.IsNullOrWhiteSpace(data.Email))
                {
                    throw new ValidationException("Email cannot be null or whitespace.");
                }
                if (!Regex.IsMatch(data.Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                {
                    throw new ValidationException("Invalid email format.");
                }
            }
        }

        public class PasswordValidationRule : IValidationRule<UserData>
        {
            public void Validate(UserData data)
            {
                if (string.IsNullOrWhiteSpace(data.Password))
                {
                    throw new ValidationException("Password cannot be null or whitespace.");
                }

                if (data.Password.Length < 15)
                {
                    throw new ValidationException("password must contain at least 15 characters");
                }

                if (!Regex.IsMatch(data.Password, @"^[a-zA-Z0-9_]+$"))
                {
                    throw new ValidationException("\r\nThe password must contain an uppercase and lowercase letter and the symbol \"_\"");
                }

            }
        }

        #endregion

        public static int Main()
        {
            try
            {

                var validator = new DataValidatorBuilder<string>().AddRule(new SampleValidationRule()).Build();

                var dataToValidateRegister = new List<string> { "Sample", "Good", "Super", "List" };

                foreach (var data in dataToValidateRegister)
                {
                    validator.Validate(data);
                    Console.WriteLine($"'{data}' is valid.");
                }

                var validator1 = new DataValidatorBuilder<UserData>()
                    .AddRule(new UsernameValidationRule())
                    .AddRule(new FullnameValidationRule())
                    .AddRule(new EmailValidationRule())
                    .AddRule(new PasswordValidationRule())
                    .Build();

                var dataToValidateRegister1 = new List<string> { "username:Sasha322", "fullname:Alexander Shevtsov", "email:kuma@gmail.com", "password:22321S3_234324DFSD34f" };

                var userData = ParseUserData(dataToValidateRegister1);

                validator1.Validate(userData);
                Console.WriteLine($"'{nameof(userData)}' is valid.");

                return 0;

            }
            catch (AggregateException exceptions)
            {
                foreach (var ex in exceptions.InnerExceptions)
                {
                    Console.WriteLine($"{ex.Message}");
                    return -2;
                }

                return -1;

            }

        }

    }
}