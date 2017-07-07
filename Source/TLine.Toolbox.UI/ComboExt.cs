namespace TripLine.Toolbox.UI
{
    using System;
    using System.Windows.Controls;

    public static class ComboExt
    {
        public static ComboBox SetEnum(this ComboBox combo, Type @enum)
        {
            foreach (var val in @enum.GetEnumValues())
            {
                combo.Items.Add(val);
            }

            return combo;
        }
    }
}