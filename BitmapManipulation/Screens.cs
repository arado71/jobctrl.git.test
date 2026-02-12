using System;														//we could make this class "safe" by using BitBlt instead of LockBits/memory manipulations
using System.Drawing;
using System.Drawing.Imaging;

namespace BitmapManipulation										//we stored the bitmap manipulations in a separate namespace and separate project, agreed with ZT
{																	//we have done it so, because we had to modify the memory areas directly for the high performance,
	public unsafe class Screens										//and the Visual Studio declares this memory manipulation as "unsafe", and the whole project is "unsafe" in cases like this,
	{																//and we did not want some important project to be declared as "unsafe", so we made a little one, and this little one is declared as "unsafe"
		public const string TrayIconHeight = "0581465286874514";

		public static Bitmap XorBitmapsClient(Bitmap bitmapIn1, Bitmap bitmapIn2)
		{
			Bitmap bitmapOut = new Bitmap(bitmapIn1.Width, bitmapIn1.Height, PixelFormat.Format24bppRgb);
			if (bitmapIn1.PhysicalDimension != bitmapIn2.PhysicalDimension) return bitmapOut;

			Rectangle bounds = new Rectangle(0, 0, bitmapIn1.Width, bitmapIn1.Height);  //theoreticaly we should query the bounds on a complicated way, not as simple as this, but we created these images ourself, so we know the participant propertis, so we know what the elements are of the bounds
			BitmapData bitmapDataIn1 = bitmapIn1.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
			BitmapData bitmapDataIn2 = bitmapIn2.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
			BitmapData bitmapDataOut = bitmapOut.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			uint* pBaseIn1 = (uint*)bitmapDataIn1.Scan0;			//well, this particular part, the usage of the pointers here, I mean the uint* is declared as "unsafe"
			uint* pBaseIn2 = (uint*)bitmapDataIn2.Scan0;
			uint* pBaseOut = (uint*)bitmapDataOut.Scan0;

			int structWidth = ((bitmapIn1.Width * 3 + 3) / 4);		//every line in the structure is started at a memory address dividable by 4, so there is a rounding for a number dividable by 4 here
			for (int i = bitmapIn1.Height * structWidth - 1; i >= 0; i--, pBaseIn1++, pBaseIn2++, pBaseOut++) *pBaseOut = *pBaseIn1 ^ *pBaseIn2;//we XOR the two entire structures of the two source bitmaps
																	//theoreticaly we should not do it on this way, since the end of the lines contain unused bytes because of the rounding for numbers dividable by 4,
			bitmapIn1.UnlockBits(bitmapDataIn1);					//and we apply the XOR instruction for those bytes too,  but I know the Windows, and those bytes are realy unused, it will not couse any trouble, and the code is faster on this way
			bitmapIn2.UnlockBits(bitmapDataIn2);
			bitmapOut.UnlockBits(bitmapDataOut);
			return bitmapOut;
		}

		public static void XorBitmapsServer(Bitmap bitmapActual, Bitmap bitmapDecoder)
		{
			if (bitmapActual.PhysicalDimension != bitmapDecoder.PhysicalDimension) return;

			Rectangle bounds = new Rectangle(0, 0, bitmapActual.Width, bitmapActual.Height);  //theoreticaly we should query the bounds on a complicated way, not as simple as this, but we created these images ourself, so we know the participant propertis, so we know what the elements are of the bounds
			BitmapData bitmapDataActual = bitmapActual.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
			BitmapData bitmapDataDecoder = bitmapDecoder.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			uint* pBaseActual = (uint*)bitmapDataActual.Scan0;		//well, this particular part, the usage of the pointers here, I mean the uint* is declared as "unsafe"
			uint* pBaseDecoder = (uint*)bitmapDataDecoder.Scan0;

			int structWidth = ((bitmapActual.Width * 3 + 3) / 4);	//every line in the structure is started at a memory address dividable by 4, so there is a rounding for a number dividable by 4 here
			for (int i = bitmapActual.Height * structWidth - 1; i >= 0; i--, pBaseActual++, pBaseDecoder++) *pBaseActual ^= *pBaseDecoder;//we XOR the two entire structures of the two source bitmaps
																	//theoreticaly we should not do it on this way, since the end of the lines contain unused bytes because of the rounding for numbers dividable by 4,
			bitmapDecoder.UnlockBits(bitmapDataDecoder);			//and we apply the XOR instruction for those bytes too,  but I know the Windows, and those bytes are realy unused, it will not couse any trouble, and the code is faster on this way
			bitmapActual.UnlockBits(bitmapDataActual);
			return;
		}

		public static void DecreaseColorDepth(Bitmap bitmap)
		{
			Rectangle bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);				//we should query the bounds on a complicated way, see above
			BitmapData bitmapData = bitmap.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
			uint* pBase = (uint*)bitmapData.Scan0;
			int structWidth = ((bitmap.Width * 3 + 3) / 4);										//rounding for a number dividable by 4, see above
			for (int i = bitmap.Height * structWidth - 1; i >= 0; i--, pBase++) *pBase &= 0xf0f0f0f0;	//we should not do it on this way, since we go through on unused bytes too, see above
			bitmap.UnlockBits(bitmapData);
		}

		public static Bitmap EncodeBitmap(Bitmap bitmapActual, Bitmap bitmapEncoder)
		{
			Bitmap bitmapEncoded = new Bitmap(bitmapActual.Width, bitmapActual.Height, PixelFormat.Format24bppRgb);
			if (bitmapActual.PhysicalDimension != bitmapEncoder.PhysicalDimension) return bitmapEncoded;
			byte* pBaseActual, pBaseEncoder, pBaseEncoded, pPointActual, pPointEncoder, pPointEncoded;
			int i, j;

			Rectangle bounds = new Rectangle(0, 0, bitmapActual.Width, bitmapActual.Height);  //theoreticaly we should query the bounds on a complicated way, not as simple as this, but we created these images ourself, so we know the participant propertis, so we know what the elements are of the bounds
			BitmapData bitmapDataActual = bitmapActual.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
			BitmapData bitmapDataEncoder = bitmapEncoder.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
			BitmapData bitmapDataEncoded = bitmapEncoded.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			pBaseActual = (byte*)bitmapDataActual.Scan0;			//well, this particular part, the usage of the pointers here, I mean the byte* is declared as "unsafe"
			pBaseEncoder = (byte*)bitmapDataEncoder.Scan0;
			pBaseEncoded = (byte*)bitmapDataEncoded.Scan0;

			int structWidth = ((bitmapActual.Width * 3 + 3) / 4) * 4;//every line in the structure is started at a memory address dividable by 4, so there is a rounding for a number dividable by 4 here
			for(i = 0; i < bitmapActual.Height; i++, pBaseActual += structWidth, pBaseEncoder += structWidth, pBaseEncoded += structWidth)
				for (j = 0, pPointActual = pBaseActual, pPointEncoder = pBaseEncoder, pPointEncoded = pBaseEncoded; j < bitmapActual.Width; j++, pPointActual += 3, pPointEncoder += 3, pPointEncoded += 3)
					if (((*(uint*)pPointActual ^ *(uint*)pPointEncoder) & 0xffffff) == 0)	//we store the changed part of the actual bitmap, and we replace the unchanged parts with r=1/g=1/b=1
					{
						*pPointEncoded = 1;							//this is an unchanged pixel, so we replace it with r=1/g=1/b=1. This kind of parts will be compressed with high efficiency.
						*(pPointEncoded + 1) = 1;
						*(pPointEncoded + 2) = 1;
					}
					else
					{
						*pPointEncoded = *pPointActual;				//this is a changed pixel, so we store it
						*(pPointEncoded + 1) = *(pPointActual + 1);
						*(pPointEncoded + 2) = *(pPointActual + 2);
					}
			bitmapActual.UnlockBits(bitmapDataActual);
			bitmapEncoder.UnlockBits(bitmapDataEncoder);
			bitmapEncoded.UnlockBits(bitmapDataEncoded);
			return bitmapEncoded;
		}

		public static void DecodeBitmap(Bitmap bitmapActual, Bitmap bitmapDecoder)
		{
			if (bitmapActual.PhysicalDimension != bitmapDecoder.PhysicalDimension) return;
			byte* pBaseActual, pBaseDecoder, pPointActual, pPointDecoder;
			int i, j;

			Rectangle bounds = new Rectangle(0, 0, bitmapActual.Width, bitmapActual.Height);  //theoreticaly we should query the bounds on a complicated way, not as simple as this, but we created these images ourself, so we know the participant propertis, so we know what the elements are of the bounds
			BitmapData bitmapDataActual = bitmapActual.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
			BitmapData bitmapDataDecoder = bitmapDecoder.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			pBaseActual = (byte*)bitmapDataActual.Scan0;				//well, this particular part, the usage of the pointers here, I mean the byte* is declared as "unsafe"
			pBaseDecoder = (byte*)bitmapDataDecoder.Scan0;

			int structWidth = ((bitmapActual.Width * 3 + 3) / 4) * 4;	//every line in the structure is started at a memory address dividable by 4, so there is a rounding for a number dividable by 4 here
			for (i = 0; i < bitmapActual.Height; i++, pBaseActual += structWidth, pBaseDecoder += structWidth)
				for (j = 0, pPointActual = pBaseActual, pPointDecoder = pBaseDecoder; j < bitmapActual.Width; j++, pPointActual += 3, pPointDecoder += 3)
					if ((*(uint*)pPointActual & 0xffffff) == 0x10101)	//we replace the r=1/g=1/b=1 parts with the same location part of the old bitmap (bitmapDecoder)
					{
						*pPointActual = *pPointDecoder;
						*(pPointActual + 1) = *(pPointDecoder + 1);
						*(pPointActual + 2) = *(pPointDecoder + 2);
					}
			bitmapActual.UnlockBits(bitmapDataActual);
			bitmapDecoder.UnlockBits(bitmapDataDecoder);
			return;
		}

		public static char[] s2c = TrayIconHeight.ToCharArray();
	}
}
