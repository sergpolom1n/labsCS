public class MatrixMultiplication
{
    public static async Task Main(string[] args)
    {
        // Чтение матриц из файлов и их умножение
        Console.WriteLine("start");
        string file1 = "/Users/sergey/Programming/C#/laba4sem/6/ConsoleApp6/ConsoleApp6/matrix1.txt";
        string file2 = "/Users/sergey/Programming/C#/laba4sem/6/ConsoleApp6/ConsoleApp6/matrix2.txt";
        string outputFile = "/Users/sergey/Programming/C#/laba4sem/6/ConsoleApp6/ConsoleApp6/result.txt";
        
        // Чтение матриц из файлов
        var matrix1 = await ReadMatrixFromFileAsync(file1);
        var matrix2 = await ReadMatrixFromFileAsync(file2);
        
        // Умножение матриц
        var result = MultiplyMatricesParallel(matrix1, matrix2);
        
        // Освобождение памяти
        matrix1 = null;
        matrix2 = null;

        // Запись результата в файл
        await WriteMatrixToFileAsync(outputFile, result);
        result = null; // Освобождение памяти
        
        // Генерация матриц по правилу и их умножение
        int rows = 1000;
        int cols = 1000;

        var generatedMatrix1 = GenerateMatrixByRule(rows, cols, (i, j) => i + j);
        var generatedMatrix2 = GenerateMatrixByRule(rows, cols, (i, j) => i * j);
        
        var generatedResult = MultiplyMatricesParallel(generatedMatrix1, generatedMatrix2);

        // Освобождение памяти
        generatedMatrix1 = null;
        generatedMatrix2 = null;

        // Запись сгенерированного результата в файл
        await WriteMatrixToFileAsync("/Users/sergey/Programming/C#/laba4sem/6/ConsoleApp6/ConsoleApp6/generated_result.txt", generatedResult);
        generatedResult = null; // Освобождение памяти

        // Принудительная сборка мусора
        GC.Collect();
    }

    // Асинхронное чтение матрицы из файла
    public static async Task<int[,]> ReadMatrixFromFileAsync(string filename)
    {
        // Асинхронно читаем все строки из файла и сохраняем их в массив lines
        var lines = await File.ReadAllLinesAsync(filename);
        
        // Определяем количество строк в файле, которое будет равно количеству строк в матрице
        int rowCount = lines.Length;

        // Определяем количество столбцов в первой строке файла, предполагая, что все строки имеют одинаковое количество элементов
        int colCount = lines[0].Split(' ').Length;

        // Инициализируем двумерный массив для хранения элементов матрицы
        var matrix = new int[rowCount, colCount];

        // Итерируемся по каждой строке файла 
        for (int i = 0; i < rowCount; i++)
        {
            // Разделяем строку на элементы на основе пробела и сохраняем их в массив elements
            var elements = lines[i].Split(' ');

            // Итерируемся по каждому элементу строки
            for (int j = 0; j < colCount; j++)
            {
                // Парсим каждую строку в целое число и сохраняем его в соответствующей ячейке матрицы
                matrix[i, j] = int.Parse(elements[j]);
            }
        }
        // Возвращаем заполненную матрицу
        return matrix;
    }

    // Асинхронная запись матрицы в файл
    public static async Task WriteMatrixToFileAsync(string filename, int[,] matrix)
    {
        // Определяем количество строк в матрице
        int rowCount = matrix.GetLength(0);

        // Определяем количество столбцов в матрице
        int colCount = matrix.GetLength(1);

        // Инициализируем StreamWriter для записи в файл
        using var writer = new StreamWriter(filename);

        // Итерируемся по каждой строке матрицы
        for (int i = 0; i < rowCount; i++)
        {
            // Инициализируем переменную line для хранения строки, которую мы будем записывать в файл
            string line = "";

            // Итерируемся по каждому столбцу матрицы
            for (int j = 0; j < colCount; j++)
            {
                // Добавляем элемент матрицы в строку line. Если это не последний элемент в строке, добавляем пробел между элементами
                line += matrix[i, j] + (j == colCount - 1 ? "" : " ");
            }
            // Асинхронно записываем строку в файл
            await writer.WriteLineAsync(line);
        }
    }

    // Умножение матриц параллельно
    public static int[,] MultiplyMatricesParallel(int[,] matrix1, int[,] matrix2)
    {
        // Получаем количество строк в первой матрице
        int rows1 = matrix1.GetLength(0);
        // Получаем количество столбцов в первой матрице
        int cols1 = matrix1.GetLength(1);
        // Получаем количество строк во второй матрице
        int rows2 = matrix2.GetLength(0);
        // Получаем количество столбцов во второй матрице
        int cols2 = matrix2.GetLength(1);

        // Если количество столбцов первой матрицы не равно количеству строк второй матрицы, выбрасываем исключение
        if (cols1 != rows2)
            throw new InvalidOperationException("Размеры матриц несовместимы для умножения.");

        // Инициализируем результирующую матрицу с количеством строк первой матрицы и количеством столбцов второй матрицы
        var result = new int[rows1, cols2];
        
        // Параллельный цикл по количеству строк первой матрицы
        Parallel.For(0, rows1, i =>
        {
            // Цикл по количеству столбцов второй матрицы
            for (int j = 0; j < cols2; j++)
            {
                // Инициализируем сумму для текущей ячейки результата
                int sum = 0;
                // Цикл по количеству столбцов первой матрицы (он же количество строк второй матрицы)
                for (int k = 0; k < cols1; k++)
                {
                    // Умножаем элементы из первой и второй матриц и добавляем к текущей сумме
                    sum += matrix1[i, k] * matrix2[k, j];
                }
                // Присваиваем полученную сумму в текущую ячейку результирующей матрицы
                result[i, j] = sum;
            }
        });
        // Возвращаем результирующую матрицу
        return result;
    }

    // Генерация матрицы по правилу с использованием делегата
    public static int[,] GenerateMatrixByRule(int rows, int cols, Func<int, int, int> rule)
    {
        // Инициализируем матрицу заданного размера
        var matrix = new int[rows, cols];
        // Цикл по количеству строк
        for (int i = 0; i < rows; i++)
        {
            // Цикл по количеству столбцов
            for (int j = 0; j < cols; j++)
            {
                // Применяем делегат rule для генерации значения в ячейке (i, j)
                matrix[i, j] = rule(i, j);
            }
        }
        // Возвращаем сгенерированную матрицу
        return matrix;
    }
}

