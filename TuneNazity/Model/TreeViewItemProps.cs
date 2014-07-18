using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace TuneNazity.Model
{
    public static class TreeViewItemProps
    {

        // NOTES IF YOU NEED GET MORE FROM A TREEVIEW 
        // If you ever need to access an attached property from an element inside the XML tree use the following syntax
        // Tag="{Binding (Model:TreeViewItemProps.IsFullPathToItem), RelativeSource={RelativeSource AncestorType={x:Type TreeViewItem}, Mode=FindAncestor}}" 
        // Here I'm using an Example of the element TAG from a checkbox but it can be from anything
        // Preffered to use the TAG of another element to store the same information in order to keep the page blendable - custum objects prevent the
        // rendering of pages.  Here is what I used instead:  It points to the TreeViewItem Container and the checkbox is inside a dataTemplate for the TreeviewItem.
        //  Tag="{Binding RelativeSource={RelativeSource Mode=FindAncestor, AncestorType={x:Type TreeViewItem}}, Path=Tag}"  
        // TreeViewItem itself could not be acceesed because the ChekBox CLick Event does not send the TreeViewContainer which has the info I needed in the TAG


        #region Folder Icon Type for TreeView


        public static readonly DependencyProperty IsRootLevelProperty =
            DependencyProperty.RegisterAttached(
                "IsRootLevel",
                typeof (string),
                typeof (TreeViewItemProps),
                new UIPropertyMetadata("False"));


        public static string GetIsRootLevel(DependencyObject obj)
        {
            return (string) obj.GetValue(IsRootLevelProperty);
        }


        public static void SetIsRootLevel(
            DependencyObject obj, string value)
        {
            obj.SetValue(IsRootLevelProperty, value);
        }

        
        #endregion Folder Icon Type for TreeView



        #region Selected Check Mark for Treeview


        public static readonly DependencyProperty IsFolderSelectedProperty =
            DependencyProperty.RegisterAttached(
                "IsFolderSelected",
                typeof(string),
                typeof(TreeViewItemProps),
                new UIPropertyMetadata("False"));


        public static string GetIsFolderSelected(DependencyObject obj)
        {
            return (string)obj.GetValue(IsFolderSelectedProperty);
        }


        public static void SetIsFolderSelected(
            DependencyObject obj, string value)
        {
            obj.SetValue(IsFolderSelectedProperty, value);
        }


        #endregion Selected Check Mark for Treeview



    }




}
