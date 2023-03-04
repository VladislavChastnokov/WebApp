namespace WebApp.Models
{
    public static class Helper
    {
        /// <summary>
        /// Метод позволяющий определить пройдена ли авторизация
        /// </summary>
        /// <param name="session">сессия</param>
        /// <returns>true - посетитель авторизован и имеет доступ к запрашиваемой странице, в противном случае - false</returns>
        public static bool ShowPage(ISession session, string page, out string redirectTo)
        {
            redirectTo = "Home";
            var allowed = false;
            var id = session.GetInt32("userId");
            if (id.HasValue)
            {
                var role = session.GetInt32("userRole");
                allowed = role.HasValue;
                if (role.HasValue)
                {
                    switch (role.Value)
                    {
                        case 1:
                            {
                                allowed = new string[] { "", "Administration", "Examination" }.Contains(page);
                                if (!allowed)
                                    redirectTo = "Administration";
                            }
                            break;
                        case 2:
                            {
                                allowed = new string[] { "", "Examination" }.Contains(page);
                                if (!allowed)
                                    redirectTo = "Examination";
                            }
                            break;
                    }
                }
            }
            return allowed;
        }
        /// <summary>
        /// Метод шифрования пароля
        /// </summary>
        /// <param name="input">пароль</param>
        /// <returns>вычисленных по алгоритну MD5 хэш</returns>
        public static byte[] CreateMD5(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return hashBytes;
            }
        }

        public static Type? GetType(string name)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse())
            {
                var tt = assembly.GetType(name);
                if (tt != null)
                {
                    return tt;
                }
            }

            return null;
        }
    }
}
