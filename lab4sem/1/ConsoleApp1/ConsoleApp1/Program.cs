
class Program
    {
        
        public class FileTokenizer : IEnumerable<string>, IDisposable
        {
    private string _filePath;
    private HashSet<char> _delimiters;
    private StreamReader _reader; // работа с файлом через StreamReader
    private bool _disposed = false; // Флаг, указывающий, были ли освобождены ресурсы

    public FileTokenizer(string filePath, HashSet<char> delimiters)
    {
        _filePath = filePath; // Инициализируем путь к файлу
        _delimiters = delimiters; // Инициализируем множество символов-разделителей
        _reader = new StreamReader(_filePath); // Открываем файл для чтения
    }

    public IEnumerable<string> Tokenize()
    {
        List<string> tokens = new List<string>(); // Создаем список для хранения токенов
        using (StreamReader reader = new StreamReader(_filePath)) // Создаем новый StreamReader для чтения файла
        {
            string line;
            while ((line = reader.ReadLine()) != null) // Читаем файл построчно
            {
                // Разделяем строку на части по символам-разделителям
                var parts = line.Split(_delimiters.ToArray(), StringSplitOptions.RemoveEmptyEntries);
                // Добавляем каждую часть в список токенов
                foreach (var part in parts)
                {
                    tokens.Add(part);
                }
            }
        }
        // Возвращаем список токенов в виде IEnumerable<string>
        return tokens;
    }

    // GetEnumerator из интерфейса IEnumerable<string>
    public IEnumerator<string> GetEnumerator()
    {
        // Возвращаем перечислитель для списка токенов
        return Tokenize().GetEnumerator();
    }

    // GetEnumerator из интерфейса IEnumerable
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        // Возвращаем типизированный перечислитель для списка токенов
        return this.GetEnumerator();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed) // Проверяем, нужно ли освобождать ресурсы
        {
            if (disposing) // Если disposing=true, освобождаем управляемые ресурсы
            {
                _reader?.Close(); // Закрываем StreamReader, если он не нулевой
            }
            _disposed = true; // Устанавливаем флаг, что ресурсы были освобождены
        }
    }

    // Деструктор класса, вызывающий Dispose с false
    ~FileTokenizer()
    {
        Dispose(false);
    }

    // Реализация метода Dispose из интерфейса IDisposable
    public void Dispose()
    {
        // Освобождаем управляемые ресурсы и подавляем финализацию
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
        
        static void Main()
        {
        // множество символов-разделителей
        HashSet<char> delimiters = new HashSet<char> { ' ', '.', ',', ';', ':', '!', '\n', '\r', '\t' };
        string filePath = "/Users/sergey/Programming/C#/laba4sem/1/ConsoleApp1/ConsoleApp1/input.txt";

        // Используем блок using для создания объекта токенизатора и перебора токенов
        using (FileTokenizer tokenizer = new FileTokenizer(filePath, delimiters))
        {
            foreach (var token in tokenizer)
            {
                Console.WriteLine(token); // Выводим каждый токен на консоль
            }
        }
        // После выхода из using блоков Dispose будет вызван автоматически
    }
}