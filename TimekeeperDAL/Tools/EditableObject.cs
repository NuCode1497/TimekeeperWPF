// Copyright 2017 (C) Cody Neuburger  All rights reserved.
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace TimekeeperDAL.Tools
{
    /// <summary>
    ///These functions are used in a collection that implements IEditableObject for change tracking.
    ///For example, a CollectionView wrapper on the entity collection.
    /// </summary>
    public abstract class EditableObject : ObservableObject, IEditableObject
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
        private void CopyMappedProperties(object source, object target)
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
    }
}
