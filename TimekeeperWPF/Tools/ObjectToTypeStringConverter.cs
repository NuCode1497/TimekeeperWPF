// Copyright 2017 (C) Cody Neuburger  All rights reserved.
// https://stackoverflow.com/questions/1652341/wpf-trigger-based-on-object-type
using System;
using System.Windows.Data;
using System.Windows.Markup;
using TimekeeperDAL.Tools;
using System.Globalization;

namespace TimekeeperWPF.Tools
{
    public class ObjectToTypeConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.GetType();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("Can't convert back");
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
