using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TimekeeperDAL.EF;
using TimekeeperWPF.Tools;

namespace TimekeeperWPF
{
    public class NotesViewModel : ViewModel<Note>
    {
        private ICommand _ContinueSaveCommand;
        private DateTime _SaveAsStart;
        private DateTime _SaveAsEnd;
        private string _SaveAsError;
        private bool _HasTimeTask;
        public NotesViewModel() : base()
        {
            Sorter = new NoteDateTimeSorterDesc();
        }
        public override string Name => nameof(Context.Notes) + " Editor";
        public CollectionViewSource TimeTasksCollection { get; protected set; }
        public ObservableCollection<TimeTask> TimeTasksSource => 
            TimeTasksCollection?.Source as ObservableCollection<TimeTask>;
        public ListCollectionView TimeTasksView => 
            TimeTasksCollection?.View as ListCollectionView;
        public DateTime SaveAsStart
        {
            get
            {
                return _SaveAsStart;
            }
            set
            {
                _SaveAsStart = value;
                OnPropertyChanged();
            }
        }
        public DateTime SaveAsEnd
        {
            get
            {
                return _SaveAsEnd;
            }
            set
            {
                _SaveAsEnd = value;
                OnPropertyChanged();
            }
        }
        public string SaveAsError
        {
            get
            {
                return _SaveAsError;
            }
            private set
            {
                _SaveAsError = value;
                OnPropertyChanged();
            }
        }
        public bool HasTimeTask
        {
            get { return _HasTimeTask; }
            set
            {
                _HasTimeTask = value;
                if (CurrentEditItem != null && value == false)
                {
                    CurrentEditItem.TimeTask = null;
                }
                OnPropertyChanged();
            }
        }
        public ICommand ContinueSaveCommand => _ContinueSaveCommand
            ?? (_ContinueSaveCommand = new RelayCommand(ap => ContinueSave(), pp => CanContinueSave));
        private bool CanContinueSave => SaveAsStart <= SaveAsEnd;
        protected override async Task GetDataAsync()
        {
            Context = new TimeKeeperContext();
            await Context.Notes.LoadAsync();
            Items.Source = Context.Notes.Local;

            TimeTasksCollection = new CollectionViewSource();
            await Context.TimeTasks.LoadAsync();
            TimeTasksCollection.Source = Context.TimeTasks.Local;
            OnPropertyChanged(nameof(TimeTasksView));
        }
        protected override void SaveAs()
        {
            SaveAsStart = DateTime.Now.Date;
            SaveAsEnd = DateTime.Now.Date;
            IsSaving = true;
            Status = "Select date range...";
            //expect view to handle changing start end
            //wait for continuesave command
        }
        private void ContinueSave()
        {
            Status = "Select file type...";
            var saveDlg = new SaveFileDialog { Filter = "Text Files |*.txt" };
            if (true == saveDlg.ShowDialog())
            {
                switch (saveDlg.FilterIndex)
                {
                    case 1:
                        SaveAsText(saveDlg);
                        break;
                        //other save cases
                }
                Status = String.Format("Created {0}", saveDlg.FileName);
            }
            else
            {
                Status = "Canceled";
            }
            
            IsSaving = false;
        }
        private void SaveAsText(SaveFileDialog saveDlg)
        {
            Status = "Saving as text...";
            var selection = from n in Source
                            where n.DateTime.Date >= SaveAsStart.Date
                            where n.DateTime.Date <= SaveAsEnd.Date
                            orderby n.DateTime
                            select n;

            if (selection.Count() == 0)
            {
                MessageBox.Show(String.Format("No notes were found for {0} to {1}.", 
                    SaveAsStart.ToShortDateString(), SaveAsEnd.ToShortDateString()),
                    "Nothing to Save", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            string text = "";
            for (DateTime d = SaveAsStart.Date; d <= SaveAsEnd.Date; d = d.AddDays(1))
            {
                var subSelection = from n in selection
                                   where n.DateTime.Date == d.Date
                                   select n;

                text += String.Format("Notes for {0}:\n", d.ToLongDateString());
                text += "------------------------------------------------------------------\n";

                foreach (var n in subSelection)
                {
                    text += String.Format("{0,12} | ", n.DateTime.ToLongTimeString());
                    var charCount = 0;
                    var lineLength = 51;
                    var words = n.Text.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var query = from word in words
                                group word by ((charCount += word.Length + 1) / lineLength) into line
                                select string.Join(" ", line);
                    var lines = query.ToList();
                    text += lines.First() + "\n";
                    foreach (var line in lines.Skip(1))
                    {
                        text += String.Format("{0,-25}{1}\n", " ", line);
                    }
                }
                text += "\n\n";
            }

            File.WriteAllText(saveDlg.FileName, text);
            Process.Start(saveDlg.FileName);
        }
        protected override void AddNew()
        {
            CurrentEditItem = new Note
            {
                DateTime = DateTime.Now,
                Text = "Your text here."
            };
            View.AddNewItem(CurrentEditItem);
            base.AddNew();
        }
        protected override void EndEdit()
        {
            base.EndEdit();
        }
    }
}
