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
    public class NotesViewModel : TypedLabeledEntitiesViewModel<Note>
    {
        #region Fields
        private ICommand _ContinueSaveCommand;
        private DateTime _SaveAsStart;
        private DateTime _SaveAsEnd;
        private string _SaveAsError;
        #endregion
        public NotesViewModel() : base()
        {
            Sorter = new NoteDateTimeSorter();
        }
        #region Properties
        public override string Name => nameof(Context.Notes) + " Editor";
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
        #endregion
        #region Conditions
        #endregion
        #region Commands
        public ICommand ContinueSaveCommand => _ContinueSaveCommand
            ?? (_ContinueSaveCommand = new RelayCommand(ap => ContinueSave(), pp => CanContinueSave));
        #endregion
        #region Predicates
        private bool CanContinueSave => SaveAsStart <= SaveAsEnd;
        #endregion
        #region Actions
        protected override async Task GetDataAsync()
        {
            //await Task.Delay(2000);
            
            Context = new TimeKeeperContext();
            await Context.Notes.LoadAsync();
            Items.Source = Context.Notes.Local;
            
            await base.GetDataAsync();
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
                            && n.DateTime.Date <= SaveAsEnd.Date
                            && n.TaskType.Name != "DBTest"
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
                text += "\n\n";
            }

            File.WriteAllText(saveDlg.FileName, text);
            Process.Start(saveDlg.FileName);
        }
        protected override void AddNew()
        {
            base.AddNew();
            CurrentEditItem.DateTime = DateTime.Now;
            CurrentEditItem.Text = "Your text here.";
        }
        #endregion
    }
}
