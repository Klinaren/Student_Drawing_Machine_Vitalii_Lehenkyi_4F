using Student_Drawing_System.Models;

namespace Student_Drawing_System.Views;

public partial class StudentDrawingView : ContentPage
{
    private List<StudentModel> allStudents = new List<StudentModel>();
    private string filePath;

    public StudentDrawingView()
    {
        InitializeComponent();

#if WINDOWS
        filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 
            "students.txt"
        );
#else
        filePath = Path.Combine(FileSystem.AppDataDirectory, "students.txt");
#endif

        LoadData();
    }

    private void RefreshList()
    {
        StudentsCollectionView.ItemsSource = null;
        StudentsCollectionView.ItemsSource = allStudents;
    }

    private async Task<List<StudentModel>> ReadFromFile()
    {
        List<StudentModel> students = new List<StudentModel>();

        if (!File.Exists(filePath))
            return students;

        using (StreamReader reader = new StreamReader(filePath))
        {
            string currentClassName = null;
            string line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("[KLASA:") && line.EndsWith("]"))
                {
                    currentClassName = line.Substring(7, line.Length - 8);
                }
                else if (currentClassName != null)
                {
                    string[] data = line.Split('|');
                    if (data.Length == 2)
                    {
                        students.Add(new StudentModel
                        {
                            Name = data[0],
                            FamilyName = data[1],
                            ClassName = currentClassName
                        });
                    }
                }
            }
        }
        return students;
    }

    private async void LoadData()
    {
        allStudents = await ReadFromFile();
        StudentsCollectionView.ItemsSource = allStudents;
    }

    private async Task SaveToFile()
    {
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            var groupedByClass = allStudents.GroupBy(s => s.ClassName);

            foreach (var classGroup in groupedByClass)
            {
                await writer.WriteLineAsync($"[KLASA:{classGroup.Key}]");

                foreach (var student in classGroup)
                {
                    await writer.WriteLineAsync($"{student.Name}|{student.FamilyName}");
                }
                await writer.WriteLineAsync();
            }
        }
    }

    private async void AddStudentBtnClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
            string.IsNullOrWhiteSpace(FamilyNameEntry.Text) ||
            string.IsNullOrWhiteSpace(ClassEntry.Text))
        {
            await DisplayAlertAsync("Błąd", "Wypełnij wszystkie pola!", "OK");
            return;
        }

        allStudents.Add(new StudentModel
        {
            Name = NameEntry.Text,
            FamilyName = FamilyNameEntry.Text,
            ClassName = ClassEntry.Text
        });

        await SaveToFile();
        RefreshList();

        NameEntry.Text = "";
        FamilyNameEntry.Text = "";
        ClassEntry.Text = "";

        await DisplayAlertAsync("Sukces", "Uczeń został dodany!", "OK");
    }

    private async void DrawStudentBtnClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(DrawClassEntry.Text))
        {
            await DisplayAlertAsync("Błąd", "Wpisz klasę!", "OK");
            return;
        }

        var studentsInClass = allStudents.Where(s => s.ClassName == DrawClassEntry.Text).ToList();

        if (studentsInClass.Count == 0)
        {
            await DisplayAlertAsync("Błąd", "Brak uczniów w klasie!", "OK");
            return;
        }

        var student = studentsInClass[new Random().Next(studentsInClass.Count)];
        await DisplayAlertAsync("Wylosowano", $"{student.Name} {student.FamilyName}", "OK");
    }

    private async void EditStudentBtnClicked(object sender, EventArgs e)
    {
        var student = (StudentModel)((Button)sender).BindingContext;

        string newName = await DisplayPromptAsync("Edycja", "Nowe imię:", initialValue: student.Name);
        if (string.IsNullOrWhiteSpace(newName))
            return;

        string newFamilyName = await DisplayPromptAsync("Edycja", "Nowe nazwisko:", initialValue: student.FamilyName);
        if (string.IsNullOrWhiteSpace(newFamilyName))
            return;

        string newClassName = await DisplayPromptAsync("Edycja", "Nowa klasa:", initialValue: student.ClassName);
        if (string.IsNullOrWhiteSpace(newClassName))
            return;

        student.Name = newName;
        student.FamilyName = newFamilyName;
        student.ClassName = newClassName;

        await SaveToFile();
        RefreshList();

        await DisplayAlertAsync("Sukces", "Dane ucznia zostały zaktualizowane!", "OK");
    }

    private async void DeleteStudentBtnClicked(object sender, EventArgs e)
    {
        var student = (StudentModel)((Button)sender).BindingContext;

        bool confirm = await DisplayAlertAsync("Potwierdzenie",
            $"Usunąć {student.Name} {student.FamilyName}?", "Tak", "Nie");

        if (confirm)
        {
            allStudents.Remove(student);
            await SaveToFile();
            RefreshList();
            await DisplayAlertAsync("Sukces", "Uczeń został usunięty!", "OK");
        }
    }
}
