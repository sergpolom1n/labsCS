using System.Collections.Concurrent; // использование потокобезопасного словаря

// Класс, представляющий элемент кэша, содержащий значение и время создания элемента
public class CacheItem<T>
{
    public T Value { get; } // Свойство для хранения значения элемента кэша
    public DateTime CreatedAt { get; } // Свойство для хранения времени создания элемента

    // Конструктор класса CacheItem, принимающий значение в качестве параметра
    public CacheItem(T value)
    {
        Value = value; // Инициализация значения элемента кэша
        CreatedAt = DateTime.UtcNow; // Инициализация времени создания текущим временем в формате UTC
    }
}

// Основной класс кэша, реализующий логику хранения и извлечения элементов
public class Cache<T>
{
    private readonly TimeSpan _timeToLive; // Время жизни элемента в кэше
    private readonly uint _maxItems; // Максимальное количество элементов в кэше
    private readonly ConcurrentDictionary<string, CacheItem<T>> _cache; // Потокобезопасный словарь для хранения элементов

    // Конструктор класса Cache, принимающий время жизни элементов и максимальное количество элементов в кэше
    public Cache(TimeSpan timeToLive, uint maxItems)
    {
        _timeToLive = timeToLive; 
        _maxItems = maxItems;
        _cache = new ConcurrentDictionary<string, CacheItem<T>>(); // Инициализация словаря
    }

    // Метод для сохранения элемента в кэше
    public void Save(string key, T value)
    {
        if (_cache.ContainsKey(key)) // Проверка, существует ли уже ключ в кэше
            throw new ArgumentException("Ключ уже существует в кэше."); // Если ключ существует, выбрасывается исключение

        lock (_cache) // Блокировка словаря для обеспечения потокобезопасности
        {
            RemoveExpiredItems(); // Удаление устаревших элементов
            // Проверка, превышает ли количество элементов в кэше максимальное допустимое значение
            if (_cache.Count >= _maxItems)
            {
                // Поиск самого старого элемента в кэше и его удаление
                var oldestKey = _cache.OrderBy(kvp => kvp.Value.CreatedAt).First().Key;
                
                _cache.TryRemove(oldestKey, out _);
            }
            // Добавление нового элемента в кэш
            _cache.TryAdd(key, new CacheItem<T>(value));
        }
    }

    // Метод для получения элемента из кэша по ключу
    public T Get(string key)
    {
        // Если элемент с указанным ключом найден в кэше
        if (_cache.TryGetValue(key, out CacheItem<T>? item))
        {
            // Проверка, не истекло ли время жизни элемента
            if (DateTime.UtcNow - item.CreatedAt <= _timeToLive)
            {
                return item.Value; // Возвращение значения элемента
            }
            else
            {
                // Удаление устаревшего элемента и выброс исключения
                _cache.TryRemove(key, out _);
                throw new KeyNotFoundException("Ключ не существует в кэше или истек его срок действия.");
            }
        }
        else
        {
            throw new KeyNotFoundException("Ключ не существует в кэше"); // Если элемент не найден, выброс исключения
        }
    }

    // Метод для удаления устаревших элементов из кэша
    private void RemoveExpiredItems()
    {
        // Поиск ключей элементов, время жизни которых истекло
        var keysToRemove = _cache.Where(kvp => DateTime.UtcNow - kvp.Value.CreatedAt > _timeToLive).Select(kvp => kvp.Key).ToList();
        
        foreach (var key in keysToRemove) // Для каждого ключа из списка устаревших
        {
            _cache.TryRemove(key, out _); // Удаление элемента из кэша
        }
    }
}

class Program
{
    static void Main()
    {
        var cache = new Cache<string>(TimeSpan.FromSeconds(2), 3);
        try
        {
            cache.Save("key1", "value1");
            cache.Save("key2", "value2");
            cache.Save("key3", "value3");
            
            Console.WriteLine("key1: " + cache.Get("key1")); // Вывод: value1
            Console.WriteLine("key2: " + cache.Get("key2")); // Вывод: value2
            Console.WriteLine("key3: " + cache.Get("key3")); // Вывод: value3

            // Подождем 3 секунды для истечения времени жизни
            Thread.Sleep(3000);

            // Попытаемся получить элементы после истечения времени жизни
            try
            {
                Console.WriteLine("key1: " + cache.Get("key1"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // Должно выкинуть исключение KeyNotFoundException
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        
        // Добавим новый элемент, чтобы проверить замену самого старого
        try
        {
            cache.Save("key1", "value1");
            cache.Save("key2", "value2");
            cache.Save("key3", "value3");
            cache.Save("key4", "value4");
            Console.WriteLine("key4: " + cache.Get("key4")); // Вывод: value4

            // Пытаемся получить оставшиеся значения
            try
            {
                Console.WriteLine("key1: " + cache.Get("key1"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); 
            }
            Console.WriteLine("key2: " + cache.Get("key2"));
            Console.WriteLine("key3: " + cache.Get("key3"));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
