using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.Models;
using TimekeeperDAL.EF;
using System.Windows.Data;
using System.Windows;
using System.Data.Entity;

namespace TimekeeperWPF
{
    public class FakeNotesViewModel : ViewModel<Note, FakeTimeKeeperContext>
    {
        public FakeNotesViewModel()
        {
            GetData();
        }
        public override string Name => "Notes";
        protected override async void GetData()
        {
            IsEnabled = false;
            IsLoading = true;
            Cancel();
            Deselect();
            Status = "Loading Data...";
            Items = new CollectionViewSource();
            OnPropertyChanged(nameof(View));
            Dispose();
            try
            {
                //await Task.Delay(3000);
                //throw new Exception("testing get data error");
                Context = new FakeTimeKeeperContext()
                {
                    Notes =
                    {
                        new Note { NoteDateTime = DateTime.Now, NoteText = "This is some test data."},
                        new Note { NoteDateTime = DateTime.Now, NoteText = "Testing 1 2 3."},
                        new Note { NoteDateTime = DateTime.Now, NoteText = "Did you ever hear the tragedy of Darth Plagueis The Wise? I thought not. It’s not a story the Jedi would tell you. It’s a Sith legend."},
                    }
                };
                await Context.Set(typeof(Note)).LoadAsync();
                Items.Source = Context.Set(typeof(Note)).Local;
                View.CustomSort = Sorter;
                OnPropertyChanged(nameof(View));
                Deselect();
                IsEnabled = true;
                Status = "Ready";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Loading Data", MessageBoxButton.OK, MessageBoxImage.Error);
                Status = "Failed to get data!";
            }
            IsLoading = false;
        }
    }
}
