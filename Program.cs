using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

class BankAccount
{
    private static Random random = new Random();
    private static List<int> existingAccountNumbers = new List<int>();

    public int AccountNumber { get; private set; }
    private string pinHash;
    private float balance;

    public float Balance
    {
        get { return balance; }
        private set
        {
            if (value < 0)
                throw new ArgumentException("Больше 0.");
            balance = value;
        }
    }

    public BankAccount(string pin)
    {
        if (!ValidatePin(pin))
            throw new ArgumentException("Пин-код неверный.");

        AccountNumber = GenerateAccountNumber();
        pinHash = HashPin(pin);
        Balance = 0;
    }

    private static bool ValidatePin(string pin)
    {
        return pin.Length == 4 && int.TryParse(pin, out _);
    }

    private static string HashPin(string pin)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(pin));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }

    private int GenerateAccountNumber()
    {
        int number;
        do
        {
            number = random.Next(100000, 999999);

        } while (existingAccountNumbers.Contains(number));

        existingAccountNumbers.Add(number);
        return number;
    }

    public void Deposit(float amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Больше 0.");
        Balance += amount;
    }

    public void Transfer(BankAccount toAccount, float amount, string pin)
    {
        if (HashPin(pin) != pinHash)
            throw new UnauthorizedAccessException("Пин-код неверный.");

        if (amount <= 0)
            throw new ArgumentException("Больше 0.");
        if (Balance < amount)
            throw new InvalidOperationException("Нету бабак.");

        Balance -= amount;
        toAccount.Deposit(amount);
    }

    public static void DisplayAccounts(List<BankAccount> accounts)
    {
        Console.WriteLine("Список счетов:");
        foreach (var account in accounts)
        {
            Console.WriteLine($"Номер счета: {account.AccountNumber}");
        }
    }

    public void DisplayBalance()
    {
        Console.WriteLine($"Баланс счета {AccountNumber}: {Balance}");
    }
}

class Program
{
    static void Main(string[] args)
    {
        List<BankAccount> accounts = new List<BankAccount>();
        while (true)
        {
            Console.WriteLine("\n1. Добавить новый счет");
            Console.WriteLine("2. Отобразить все счета");
            Console.WriteLine("3. Отобразить баланс счета");
            Console.WriteLine("4. Перевести деньги с одного счета на другой");
            Console.WriteLine("5. Пополнить баланс счета\n"); 
            Console.Write("\nВыберите действие: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Write("Введите PIN-код (4 цифры): ");
                    string pin = Console.ReadLine();
                    try
                    {
                        var account = new BankAccount(pin);
                        accounts.Add(account);
                        Console.WriteLine($"Счет успешно создан. Номер счета: {account.AccountNumber}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошипка: {ex.Message}");
                    }
                    break;

                case "2":
                    BankAccount.DisplayAccounts(accounts);
                    break;

                case "3":
                    Console.Write("Введите номер счета: ");
                    int accountNumber = int.Parse(Console.ReadLine());
                    var foundAccount = accounts.Find(a => a.AccountNumber == accountNumber);
                    if (foundAccount != null)
                    {
                        foundAccount.DisplayBalance();
                    }
                    else
                    {
                        Console.WriteLine("Счет не найден.");
                    }
                    break;

                case "4":
                    Console.Write("Введите номер счета откуда переводить: ");
                    int fromAccountNumber = int.Parse(Console.ReadLine());
                    var fromAccount = accounts.Find(a => a.AccountNumber == fromAccountNumber);
                    if (fromAccount == null)
                    {
                        Console.WriteLine("Счет не найден.");
                        break;
                    }

                    Console.Write("Введите номер счета куда переводить: ");
                    int toAccountNumber = int.Parse(Console.ReadLine());
                    var toAccount = accounts.Find(a => a.AccountNumber == toAccountNumber);
                    if (toAccount == null)
                    {
                        Console.WriteLine("Счет не найден.");
                        break;
                    }

                    Console.Write("Введите сумму для перевода: ");
                    float amount = float.Parse(Console.ReadLine());
                    Console.Write("Введите PIN-код для подтверждения: ");
                    string transferPin = Console.ReadLine();

                    try
                    {
                        fromAccount.Transfer(toAccount, amount, transferPin);
                        Console.WriteLine("Перевод выполнен успешно.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошипка: {ex.Message}");
                    }
                    break;

                case "5":
                    Console.Write("Введите номер счета для пополнения: ");
                    int depositAccountNumber = int.Parse(Console.ReadLine());
                    var depositAccount = accounts.Find(a => a.AccountNumber == depositAccountNumber);
                    if (depositAccount == null)
                    {
                        Console.WriteLine("Счет не найден.");
                        break;
                    }

                    Console.Write("Введите сумму для пополнения: ");
                    float depositAmount = float.Parse(Console.ReadLine());
                    try
                    {
                        depositAccount.Deposit(depositAmount);
                        Console.WriteLine("Баланс успешно пополнен.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошипка: {ex.Message}");
                    }
                    break;
                default:
                    Console.WriteLine("Такого выбора нету.\n");
                    break;
            }
        }
    }

}
