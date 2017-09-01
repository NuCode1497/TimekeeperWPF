using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using System.Reflection;

namespace TimekeeperDAL.EF
{
    //This annotation from PropertyChanged.Fody weaver injects INotifyPropertyChanged into each model.
    //Implementing INotifyProperyChanged creates an Observable Model that lets the UI update when data changes.
    [AddINotifyPropertyChangedInterface]
    public abstract class EntityBase : ObservableObject, IDataErrorInfo, INotifyDataErrorInfo, IEditableObject
    {
        [Key]
        public int Id { get; set; }

        [Timestamp]
        [MaxLength(8)]
        [Column(TypeName = "timestamp")]
        public byte[] RowVersion { get; set; }

        /// <summary>
        /// Implemented with INotifyPropertyChanged. Flagged true when a property is changed. You must manually flag false.
        /// </summary>
        [NotMapped]
        public bool IsChanged { get; set; }

        #region Validations
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        /// <summary>
        /// Gets a value that indicates whether the entity has validation errors.
        /// <returns> Returns true if the entity currently has validation errors; otherwise, false. </returns>
        /// </summary>
        [NotMapped]
        public bool HasErrors => _errors.Count != 0;

        /// <summary>
        /// Occurs when the validation errors have changed for a property or for the entire entity.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <summary>
        /// Gets the validation errors for a specified property or for the entire entity.
        /// </summary>
        /// <param name="propertyName">The name of the property to retrieve validation errors for; or null or System.String.Empty,
        /// to retrieve entity-level errors.</param>
        /// <returns>The validation errors for the property or entity.</returns>
        public IEnumerable GetErrors(string propertyName)
        {
            //If we get an empty column name, then just return all errors
            if (string.IsNullOrEmpty(propertyName))
            {
                return _errors.Values;
            }
            //otherwise return the errors for the given column
            return _errors.ContainsKey(propertyName) ? _errors[propertyName] : null;
        }
        
        // WPF binding engine doesn't use Error
        [NotMapped]
        public string Error { get; }

        // Gets the error message for the property with the given name. 
        // The indexer gets called each time the PropertyChanged event is raised on the object. 
        // If anything but string.Empty is returned, an error is presumed to exist on the property. 
        // We aren't using IDataErrorInfo for validation, therefore we should always return string.Empty. 
        // We are using IDataErrorInfo to make sure validation code leveraged by INotifyDataErrorInfo gets 
        // called every time PropertyChanged is raised.
        [NotMapped]
        public abstract string this[string columnName] { get; }

        protected void ClearErrors(string propertyName = "")
        {
            _errors.Remove(propertyName);
            OnErrorsChanged(propertyName);
        }
        protected void AddError(string propertyName, string error)
        {
            AddErrors(propertyName, new List<string> { error });
        }
        protected void AddErrors(string propertyName, IList<string> errors)
        {
            var changed = false;
            //If the dictionary doesn't have the given column, add it
            if (!_errors.ContainsKey(propertyName))
            {
                _errors.Add(propertyName, new List<string>());
                changed = true;
            }
            //Add errors to the dictionary, skip duplicates
            errors.ToList().ForEach(x =>
            {
                if (_errors[propertyName].Contains(x)) return;
                _errors[propertyName].Add(x);
                changed = true;
            });
            if (changed)
            {
                OnErrorsChanged(propertyName);
            }
        }
        // Needs to be called every time the dictionary changes
        protected void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        /// <summary>
        /// In WPF, we have to programmatically check for Data annotation-based validation errors.
        /// </summary>
        /// <typeparam name="T">implicit value type</typeparam>
        /// <param name="propertyName">property name</param>
        /// <param name="value">property value</param>
        /// <returns>Returns a list of the Data annotation-based validation errors.</returns>
        protected string[] GetErrorsFromAnnotations<T>(string propertyName, T value)
        {
            // The ValdationContext provides a context for checking a class for validation errors using the Validator.
            // The Validator allows you to check an object for attribute-based errors within a ValidationContext.

            // results of the validation checks
            var results = new List<ValidationResult>();

            // create a ValidationContext scoped to the property name passed in
            var vc = new ValidationContext(this, null, null) { MemberName = propertyName };

            // bool, if everything passes, returns true
            // if not, returns false and populates results list with errors
            var isValid = Validator.TryValidateProperty(value, vc, results);
            return (isValid) ? null : Array.ConvertAll(results.ToArray(), o => o.ErrorMessage);
        }
        #endregion

        #region IEditableObject
        //These functions are used in a collection that implements IEditableObject for change tracking.
        //For example, a CollectionView wrapper on the entity collection.
        private object ShadowClone;
        //Setting this flag from the VM because only the VM should handle change tracking. Not DataGrids.
        [NotMapped]
        public bool IsEditing { get; set; } = false;
        public void BeginEdit()
        {
            if (!IsEditing)
            {
                //Kage Bunshin no Jutsu
                //Create an object that looks like this object by copying mapped public properties
                ShadowClone = Activator.CreateInstance(GetType());
                CopyMappedProperties(this, ShadowClone);
            }
        }
        public void EndEdit()
        {
            if (IsEditing)
            {
                ShadowClone = null;
            }
        }
        public void CancelEdit()
        {
            if (IsEditing)
            {
                CopyMappedProperties(ShadowClone, this);
                ShadowClone = null;
            }
            IsChanged = false;
        }
        private void CopyMappedProperties(object source, object target)
        {
            if (source.GetType() != target.GetType())
                throw new ArgumentException("Objects must be the same type.");
            //Get mapped public properties
            var properties = from p in source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             where p.GetCustomAttributes(typeof(NotMappedAttribute), false).Length == 0
                             select p;
            foreach (var p in properties)
            {
                p.SetValue(target, p.GetValue(source));
            }
        }
        #endregion
    }
}
