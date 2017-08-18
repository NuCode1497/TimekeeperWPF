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
using TimekeeperWPF.Tools;

namespace TimekeeperWPF
{
    public class NotesViewModel : ViewModel<Note>
    {
        #region Fields
        private bool _HasSelectedLabel = false;
        private Label _SelectedLabel;
        private ICommand _DeleteLabelFromNoteCommand;
        private ICommand _AddLabelToNoteCommand;
        #endregion
        public NotesViewModel() : base()
        {
            Sorter = new DateTimeSorter();
            LoadData();
        }
        #region Properties
        public override string Name => nameof(Context.Notes);
        public CollectionViewSource NoteTypesCollection { get; set; }
        public CollectionViewSource LabelsCollection { get; set; }
        public ObservableCollection<TaskType> NoteTypesSource => NoteTypesCollection?.Source as ObservableCollection<TaskType>;
        public ObservableCollection<Label> LabelsSource => LabelsCollection?.Source as ObservableCollection<Label>;
        public ListCollectionView NoteTypesView => NoteTypesCollection?.View as ListCollectionView;
        public ListCollectionView LabelsView => LabelsCollection?.View as ListCollectionView;
        public Label SelectedLabel
        {
            get
            {
                return _SelectedLabel;
            }
            set
            {
                //Label must not be itself and must be in LabelsSource
                if ((value == _SelectedLabel) || (value != null && (!LabelsSource?.Contains(value) ?? false))) return;
                _SelectedLabel = value;
                if (SelectedLabel == null)
                {
                    HasSelectedLabel = false;
                }
                else
                {
                    HasSelectedLabel = true;
                }
                OnPropertyChanged();
            }
        }
        #endregion
        #region Conditions
        public bool HasSelectedLabel
        {
            get
            {
                return _HasSelectedLabel;
            }
            protected set
            {
                _HasSelectedLabel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNotSelectedLabel));
            }
        }
        public bool HasNotSelectedLabel => !HasSelectedLabel;
        #endregion
        #region Predicates
        private bool CanDeleteLabel(object o)
        {
            return o is Label;
        }
        private bool CanAddLabel => HasSelectedLabel;
        #endregion
        #region Commands
        public ICommand DeleteLabelFromNoteCommand => _DeleteLabelFromNoteCommand
            ?? (_DeleteLabelFromNoteCommand = new RelayCommand(ap => DeleteLabel(ap as Label), pp => CanDeleteLabel(pp)));
        public ICommand AddLabelToNoteCommand => _AddLabelToNoteCommand
            ?? (_AddLabelToNoteCommand = new RelayCommand(ap => AddLabel(), pp => CanAddLabel));
        #endregion
        #region Actions
        private void AddLabel()
        {
            //add label to current note label list
            //update filter to not show this item in label list
            OnPropertyChanged(nameof(LabelsView));
        }
        private void DeleteLabel(Label ap)
        {
            //remove label from current note label list
            //update filter to show this item in label list
            OnPropertyChanged(nameof(LabelsView));
        }
        protected override async System.Threading.Tasks.Task<ObservableCollection<Note>> GetDataAsync()
        {
            //await System.Threading.Tasks.Task.Delay(2000);
            Context = new TimeKeeperContext();
            NoteTypesCollection = new CollectionViewSource();
            LabelsCollection = new CollectionViewSource();
            await Context.Notes.LoadAsync();
            await Context.TaskTypes.LoadAsync();
            await Context.Labels.LoadAsync();
            NoteTypesCollection.Source = Context.TaskTypes.Local;
            LabelsCollection.Source = Context.Labels.Local;
            OnPropertyChanged(nameof(NoteTypesView));
            OnPropertyChanged(nameof(LabelsView));
            return Context.Notes.Local;
        }
        protected override void SaveAs()
        {
            DateTime selectedDate = DateTime.Today;
            var selection = from n in Source
                            where n.DateTime.Date == selectedDate.Date && n.TaskType.Name != "DBTest"
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
            //create filter on labelscollection that excludes CurrentEditItem.Labels
        }
        protected override void AddNew()
        {
            //clear labels filter
            base.AddNew();
            CurrentEditItem.DateTime = DateTime.Now;
            CurrentEditItem.Text = "Your text here.";
            CurrentEditItem.TaskType = 
                (from t in NoteTypesSource
                 where t.Name == "Note"
                 select t).DefaultIfEmpty(NoteTypesSource[0]).First();
            PreSelectNoteType();
        }
        #endregion
        #region Functions
        private void PreSelectNoteType()
        {
            NoteTypesView.MoveCurrentTo(
                (from t in NoteTypesSource
                 where t.Name == CurrentEditItem?.TaskType.Name
                 select t).DefaultIfEmpty(NoteTypesSource[0]).First());
        }
        #endregion
    }
}
