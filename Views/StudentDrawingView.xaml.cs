using Student_Drawing_System.Models;

namespace Student_Drawing_System.Views;

public partial class StudentDrawingView : ContentPage
{
    private List<StudentModel> allStudents = new List<StudentModel>();
    private string filePath;

    public StudentDrawingView()
    {
        InitializeComponent();
        filePath = Path.Combine(FileSystem.AppDataDirectory, "students.txt");
        LoadData();
    }

    private async void LoadData()
    {
        allStudents = await ReadFromFile();
        RefreshList();
    }

    private void RefreshList()
    {
        var collectionView = this.FindByName<CollectionView>("StudentsCollectionView");
        if (collectionView is null)
        {
            return;
        }

        collectionView.ItemsSource = null;
        collectionView.ItemsSource = allStudents;
    }

    private async void AddStudentBtnClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
            string.IsNullOrWhiteSpace(FamilyNameEntry.Text) ||
            string.IsNullOrWhiteSpace(ClassNameEntry.Text))
        {
            await DisplayAlertAsync("B³¹d", "Wype³nij wszystkie pola!", "OK");
            return;
        }

        allStudents.Add(new StudentModel
        {
            Name = NameEntry.Text,
            FamilyName = FamilyNameEntry.Text,
            ClassName = ClassNameEntry.Text
        });

        await SaveToFile();
        RefreshList();

        NameEntry.Text = "";
        FamilyNameEntry.Text = "";
        ClassNameEntry.Text = "";

        await DisplayAlertAsync("Sukces", "Uczeñ dodany!", "OK");
    }

    private async void DeleteStudent_Clicked(object sender, EventArgs e)
    {
        var student = (StudentModel)((Button)sender).BindingContext;

        bool confirm = await DisplayAlertAsync("Potwierdzenie",
            $"Usun¹æ {student.Name} {student.FamilyName}?", "Tak", "Nie");

        if (confirm)
        {
            allStudents.Remove(student);
            await SaveToFile();
            RefreshList();
            await DisplayAlertAsync("Sukces", "Uczeñ usuniêty!", "OK");
        }
    }


    private async void DrawStudentBtnClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(DrawClassEntry.Text))
        {
            await DisplayAlertAsync("B³¹d", "Wpisz klasê!", "OK");
            return;
        }

        var studentsInClass = allStudents.Where(s => s.ClassName == DrawClassEntry.Text).ToList();

        if (studentsInClass.Count == 0)
        {
            await DisplayAlertAsync("B³¹d", "Brak uczniów w tej klasie!", "OK");
            return;
        }

        var student = studentsInClass[new Random().Next(studentsInClass.Count)];
        await DisplayAlertAsync("Wylosowano", $"{student.Name} {student.FamilyName}", "OK");
    }

    private async Task SaveToFile()
    {
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            foreach (var student in allStudents)
            {
                await writer.WriteLineAsync($"{student.Name}|{student.FamilyName}|{student.ClassName}");
            }
        }
    }

    private async Task<List<StudentModel>> ReadFromFile()
    {
        List<StudentModel> students = new List<StudentModel>();

        if (!File.Exists(filePath))
            return students;

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                string[] data = line.Split('|');
                if (data.Length == 3)
                {
                    students.Add(new StudentModel
                    {
                        Name = data[0],
                        FamilyName = data[1],
                        ClassName = data[2]
                    });
                }
            }
        }
        return students;
    }
}