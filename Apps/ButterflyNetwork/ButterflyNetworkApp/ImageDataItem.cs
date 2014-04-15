using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using Unm.DistributedSystem.ButterflyNetwork;

namespace Unm.DistributedSystem.ButterflyNetworkApp
{
	public class ImageDataItem : IDataItem<Image>
	{
		public string Title { get; set; }
		public Image Content { get; private set; }
		private int sizeOnDisk;

		public ImageDataItem()
		{
		}

		public ImageDataItem(string path)
		{
			Content = new Bitmap(1, 1); //Image.FromFile(path);		// TODO: TEMP
			Title = Path.GetFileNameWithoutExtension(path);
			var fileInfo = new FileInfo(path);
			sizeOnDisk = 5; //(int)fileInfo.Length;		// TODO: TEMP
		}

		public override int GetHashCode()
		{
			return Title.GetHashCode();
		}

		public int GetSize()
		{
			return (Title.Count() * sizeof(char)) + sizeOnDisk;
		}

		public object Clone()
		{
			return new ImageDataItem()
			{
				Title = Title == null ? null : (string)this.Title.Clone(),
				Content = Content == null ? null : (Image)this.Content.Clone(),
				sizeOnDisk = this.sizeOnDisk
			};
		}

		public override string ToString()
		{
			return "Title=" + Title;
		}
	}
}
