using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TimekeeperDAL.EF;
namespace TimekeeperWPF
{
    public class NotesViewModel : ViewModel<Note>
    {
        #region Fields
        #endregion
        public NotesViewModel() : base()
        {
            Sorter = new DateTimeSorter();
            LoadData();
        }
        #region Properties
        public override string Name => nameof(Context.Notes);
        public CollectionViewSource NoteTypesCollection { get; set; }
        public ObservableCollection<TaskType> NoteTypesSource => NoteTypesCollection?.Source as ObservableCollection<TaskType>;
        public ListCollectionView NoteTypesView => NoteTypesCollection?.View as ListCollectionView;
        #endregion
        #region Predicates
        protected override bool CanSave => IsEnabled && IsNotLoading && IsNotEditingItemOrAddingNew;
        #endregion
        #region Commands
        #endregion
        #region Actions
        protected override async System.Threading.Tasks.Task<ObservableCollection<Note>> GetDataAsync()
        {
            //await System.Threading.Tasks.Task.Delay(2000);
            Context = new TimeKeeperContext();
            NoteTypesCollection = new CollectionViewSource();
            await Context.Notes.LoadAsync();
            await Context.TaskTypes.LoadAsync();
            NoteTypesCollection.Source = Context.TaskTypes.Local;
            OnPropertyChanged(nameof(NoteTypesView));
            return Context.Notes.Local;
        }
        protected override void SaveAs()
        {
            DateTime selectedDate = DateTime.Today;
            var selection = from n in Source
                            where n.DateTime.Date == selectedDate.Date && n.TaskType.Name != "Test"
                            orderby n.DateTime
                            select n;

            if(selection.Count() == 0)
            {
                MessageBox.Show(String.Format("No notes were found for date {0}.", selectedDate), 
                    "Nothing to Save", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            string text = String.Format("Notes for {0}:\n", selectedDate.ToLongDateString());
            text += "------------------------------------------------------------------\n";
            foreach (var n in selection)
            {
                text += String.Format("{0,12} | {1,-7} | ", n.DateTime.ToLongTimeString(), n.TaskType.Name);
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

            var saveDlg = new SaveFileDialog { Filter = "Text Files |*.txt" };
            if (true == saveDlg.ShowDialog())
            {
                File.WriteAllText(saveDlg.FileName, text);
                Process.Start(saveDlg.FileName);
            }
        }
        protected override void EditSelected()
        {
            base.EditSelected();
            PreSelectNoteType();
        }
        protected override void AddNew()
        {
            base.AddNew();
            PreSelectNoteType();
        }
        private void PreSelectNoteType()
        {
            NoteTypesView?.MoveCurrentTo(NoteTypesSource.DefaultIfEmpty(NoteTypesSource[0])
                .First(t => t.Name == CurrentEditItem?.TaskType.Name));
        }
        #endregion
    }
}
