using System;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Graphics.Drawables;

[assembly: ExportRenderer(typeof(ViewCell), typeof(Gitter.Android.Renderers.ExtendedViewCellRenderer))]

namespace Gitter.Android.Renderers
{
	public class ExtendedViewCellRenderer : ViewCellRenderer
	{
		protected override global::Android.Views.View GetCellCore(Cell item, global::Android.Views.View convertView, ViewGroup parent, Context context)
		{
			//Get Android's ListView 
			var thisCellsListView = (global::Android.Widget.ListView)parent;

			//The Xamarin.Forms.ListView
			//var tableParent = (ListView)base.ParentView;

			var colors = new int[] { Color.White.ToAndroid().ToArgb() , Color.Gray.ToAndroid().ToArgb(), Color.White.ToAndroid().ToArgb() };
			thisCellsListView.Divider = new GradientDrawable(GradientDrawable.Orientation.LeftRight, colors);
			thisCellsListView.DividerHeight = 1;

			var cell = base.GetCellCore(item, convertView, parent, context);
			return cell;
		}
	}
}