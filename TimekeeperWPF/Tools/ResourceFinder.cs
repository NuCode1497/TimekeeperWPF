using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xaml;

namespace TimekeeperWPF.Tools
{
    public class ResourceFinder : System.Windows.Markup.MarkupExtension
    {
        public object ResourceKey { get; set; }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            FrameworkElement frameworkElement;
            IDictionary dictionary;
            IRootObjectProvider rootObjectProvider = (IRootObjectProvider)serviceProvider
                .GetService(typeof(IRootObjectProvider));

            if (rootObjectProvider != null)
            {
                dictionary = rootObjectProvider.RootObject as IDictionary;

                if (dictionary != null)
                {
                    //return dictionary[ResourceKey];
                    //instead of finding the resource in the parent dictionary, we need to find the
                    //resource from the application's merged dictionaries
                    foreach (var dic in App.Current.Resources.MergedDictionaries)
                    {
                        var resource = dic[ResourceKey];
                        if (resource != null) return resource;
                    }
                }
                else
                {
                    frameworkElement = rootObjectProvider.RootObject as FrameworkElement;
                    if (frameworkElement != null)
                    {
                        return frameworkElement.TryFindResource(ResourceKey);
                    }
                }

            }
            return null;
        }
    }
}
