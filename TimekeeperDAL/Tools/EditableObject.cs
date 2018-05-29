// Copyright 2017 (C) Cody Neuburger  All rights reserved.
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace TimekeeperDAL.Tools
{
    /// <summary>
    /// Contains the implementation of IEditableObject for change tracking by containers like CollectionView. 
    /// Also implements the Memento pattern for undo/redo change tracking lists.
    /// </summary>
    public abstract class EditableObject : ObservableObject, IEditableObject, IOriginator
    {
        private object ShadowClone;
        //Setting this flag from the VM because only the VM should handle change tracking. Not DataGrids.
        [NotMapped]
        public bool IsEditing { get; set; } = false;
        [NotMapped]
        public virtual bool IsEditable { get; set; } = true;
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
        //Mapped properties are those without the [NotMapped] annotation
        public static void CopyMappedProperties(object source, object target)
        {
            if (source.GetType() != target.GetType())
                throw new ArgumentException("Objects must be the same type.");
            //Get mapped public properties
            var properties = from p in source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             where p.GetCustomAttributes(typeof(NotMappedAttribute), false).Length == 0
                             && p.CanWrite
                             select p;
            foreach (var p in properties)
            {
                p.SetValue(target, p.GetValue(source));
            }
        }

        #region Memento
        //Memento Pattern implemented with CopyMappedProperties()
        private class Memento : IMemento
        {
            private readonly EditableObject Originator;
            private readonly object ShadowClone;
            public Memento(EditableObject originator)
            {
                Originator = originator;
                ShadowClone = Activator.CreateInstance(originator.GetType());
                CopyMappedProperties(originator, ShadowClone);
            }
            public void RestoreState()
            {
                CopyMappedProperties(ShadowClone, Originator);
            }
        }
        public IMemento State => new Memento(this);
        #endregion
    }
}
