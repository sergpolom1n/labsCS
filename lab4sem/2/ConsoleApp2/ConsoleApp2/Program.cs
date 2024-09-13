using System.Collections.Concurrent;

public abstract class Person
{
    private int _id { get; }

    protected Person(int id)
    {
        this._id = id;
    }

    public int GetId() => _id;
}

public class Doctor : Person
{
    private readonly object _lock = new object();

    public Doctor(int indexDoctor) : base(indexDoctor) { }

    public async Task TreatPatient(Patient patient, int treatmentTime)
    {
        await Task.Delay(treatmentTime);
        Console.WriteLine($"Doctor {this.GetId()} treated patient {patient.GetId()} (sick: {patient._healthStatusSick}) for {treatmentTime} units.");
    }

    public async Task AssistDoctor(Doctor doctor, int assistanceTime)
    {
        await Task.Delay(assistanceTime);
        Console.WriteLine($"Doctor {this.GetId()} assisted Doctor {doctor.GetId()} for {assistanceTime} units.");
    }

    public async Task LogDoctorActivityAsync(string log)
    {
        lock (_lock)
        {
            using (StreamWriter writer = new StreamWriter("doctor_log.txt", append: true))
            {
                writer.WriteLine($"{DateTime.Now}: {log}");
            }
        }
    }
}

public class Patient : Person
{
    public readonly bool _healthStatusSick;

    public Patient(int indexPatient, bool isSick) : base(indexPatient)
    {
        this._healthStatusSick = isSick;
    }
}

public class Program
{
    public class InfectiousDiseasesDepartment
    {
        private int _capacityRoom;
        private ConcurrentQueue<Patient> _healthyQueue;
        private ConcurrentQueue<Patient> _sickQueue;
        private List<Patient> _inspectionRoom;
        private List<Doctor> _doctors;
        private int _treatmentTime;
        private SemaphoreSlim _semaphore;

        public InfectiousDiseasesDepartment(int N, int M, int T)
        {
            this._capacityRoom = N;
            _semaphore = new SemaphoreSlim(N);
            this._treatmentTime = T;
            _healthyQueue = new ConcurrentQueue<Patient>();
            _sickQueue = new ConcurrentQueue<Patient>();
            _inspectionRoom = new List<Patient>();
            _doctors = Enumerable.Range(0, M).Select(i => new Doctor(i)).ToList();
        }

        public async Task AddPatient(Patient patient)
        {
            if (_semaphore.CurrentCount > 0 && CanEnterRoom(patient))
            {
                await _semaphore.WaitAsync();
                _inspectionRoom.Add(patient);
                Console.WriteLine($"Patient {patient.GetId()} entered the inspection room (sick: {patient._healthStatusSick}).");
                await LogPatientEntryAsync(patient);
                await Task.Delay(2000); // Добавляем задержку в 2 секунды перед обработкой следующего пациента
                await HandlePatients();
            }
            else
            {
                if (patient._healthStatusSick)
                {
                    _sickQueue.Enqueue(patient);
                    Console.WriteLine($"Patient {patient.GetId()} (sick) queued.");
                }
                else
                {
                    _healthyQueue.Enqueue(patient);
                    Console.WriteLine($"Patient {patient.GetId()} (healthy) queued.");
                }
            }
        }

        private bool CanEnterRoom(Patient patient)
        {
            return _inspectionRoom.Count == 0 || _inspectionRoom.All(p => p._healthStatusSick == patient._healthStatusSick);
        }

        private async Task HandlePatients()
        {
            foreach (var doctor in _doctors)
            {
                if (_inspectionRoom.Count > 0)
                {
                    var patient = _inspectionRoom[0];
                    _inspectionRoom.RemoveAt(0);
                    _semaphore.Release();
                    await doctor.TreatPatient(patient, _treatmentTime);
                    // Вызываем метод AssistDoctor для оказания помощи другим докторам
                    foreach (var otherDoctor in _doctors)
                    {
                        if (otherDoctor != doctor)
                        {
                            await doctor.AssistDoctor(otherDoctor, _treatmentTime / 2); // Примерное время помощи
                        }
                    }
                    // Журналируем активность доктора
                    await doctor.LogDoctorActivityAsync($"Treated patient {patient.GetId()} (sick: {patient._healthStatusSick})");
                }
                else if (_healthyQueue.Count > 0 || _sickQueue.Count > 0) // Обрабатываем пациентов из очереди, если есть
                {
                    Patient nextPatient;
                    if (_healthyQueue.TryDequeue(out nextPatient))
                    {
                        _inspectionRoom.Add(nextPatient);
                    }
                    else if (_sickQueue.TryDequeue(out nextPatient))
                    {
                        _inspectionRoom.Add(nextPatient);
                    }
                    else
                    {
                        // Handle the case when both queues are empty
                    }

                    if (nextPatient != null)
                    {
                        Console.WriteLine($"Patient {nextPatient.GetId()} entered the inspection room (sick: {nextPatient._healthStatusSick}).");
                        await LogPatientEntryAsync(nextPatient);
                        await doctor.TreatPatient(nextPatient, _treatmentTime);
                        // Вызываем метод AssistDoctor для оказания помощи другим докторам
                        foreach (var otherDoctor in _doctors)
                        {
                            if (otherDoctor != doctor)
                            {
                                await doctor.AssistDoctor(otherDoctor, _treatmentTime / 2); // Примерное время помощи
                            }
                        }
                        // Журналируем активность доктора
                        await doctor.LogDoctorActivityAsync($"Treated patient {nextPatient.GetId()} (sick: {nextPatient._healthStatusSick})");
                    }
                }
            }
        }

        private async Task LogPatientEntryAsync(Patient patient)
        {
            using (StreamWriter writer = new StreamWriter("patient_log.txt", append: true))
            {
                await writer.WriteLineAsync($"{DateTime.Now}: Patient {patient.GetId()} entered inspection room (sick: {patient._healthStatusSick}).");
            }
        }

        public async Task Run(List<Patient> patients)
        {
            List<Task> patientTasks = new List<Task>();

            foreach (var patient in patients)
            {
                patientTasks.Add(AddPatient(patient));
            }

            await Task.WhenAll(patientTasks);
        }
    }
    public static async Task Main()
    {
        int N = 5;
        int M = 7;
        int T = 2000;

        InfectiousDiseasesDepartment department = new InfectiousDiseasesDepartment(N, M, T);

        List<Patient> patients = new List<Patient>
        {
            new Patient(1, false),
            new Patient(2, true),
            new Patient(3, false),
            new Patient(4, true),
            new Patient(5, false),
            new Patient(6, true),
            new Patient(7, true),
            new Patient(8, false),
            new Patient(9, false),
            new Patient(10, false),
            new Patient(11, true),
            new Patient(12, true),
            new Patient(13, true),
            new Patient(14, true),
            new Patient(15, false),
            new Patient(16, false),
            new Patient(17, false),
            new Patient(18, true),
            new Patient(19, true),
            new Patient(20, true),
            new Patient(21, true),
            new Patient(22, false),
            new Patient(23, false),
            new Patient(24, false),
            new Patient(25, true),
            new Patient(26, true),
            new Patient(27, true),
            new Patient(28, true),
            new Patient(29, false),
            new Patient(30, false)
        };

        await department.Run(patients);
    }
}
