using System;
using System.IO;
using System.IO.Compression;
using System.ServiceModel.Channels;
using SZL.Zip;

namespace TctEncoder
{
    internal class TctMessageEncoderFactory : MessageEncoderFactory
    {
        MessageEncoder encoder;

		public TctMessageEncoderFactory(MessageEncoderFactory messageEncoderFactory, int compressionLevel)		//WCF calls it, BuildChannelListener of the WCF, CreateMessageEncoderFactory of the BuildChannelListener
        {
            if (messageEncoderFactory == null) throw new ArgumentNullException("messageEncoderFactory", "A valid message encoder factory must be passed to our encoder");
            encoder = new TctMessageEncoder(messageEncoderFactory.Encoder, compressionLevel);

        }

		public override MessageEncoder Encoder												//WCF calls it, independently from us, so it has to be here
        {
            get { return encoder; }
        }

		public override MessageVersion MessageVersion										//WCF calls it, independently from us, so it has to be here
        {
            get { return encoder.MessageVersion; }
        }
    }

	internal partial class TctMessageEncoder : MessageEncoder
	{
		static string MessageEncoderContentType = "application/octet-stream";

		private readonly int compressionLevel;

		MessageEncoder innerEncoder;

		internal TctMessageEncoder(MessageEncoder messageEncoder, int compressionLevel) : base()					//WCF calls it, BuildChannelListener of the WCF, CreateMessageEncoderFactory of the BuildChannelListener, TctMessageEncoderFactory of the CreateMessageEncoderFactory
		{
			if (messageEncoder == null) throw new ArgumentNullException("messageEncoder", "A valid message encoder must be passed to our encoder");
			innerEncoder = messageEncoder;
			this.compressionLevel = compressionLevel;
		}

		public override string ContentType													//WCF calls it, independently from us, so it has to be here
		{
			get { return MessageEncoderContentType; }
		}

		public override string MediaType													//WCF calls it, independently from us, so it has to be here
		{
			get { return MessageEncoderContentType; }
		}

		public override MessageVersion MessageVersion										//WCF calls it, independently from us, so it has to be here
		{
			get { return innerEncoder.MessageVersion; }
		}

		static ArraySegment<byte> CompressBuffer(ArraySegment<byte> buffer, BufferManager bufferManager, int messageOffset, int compressionLevel)
		{
			MemoryStream memoryStream = new MemoryStream();
			memoryStream.Write(buffer.Array, 0, messageOffset);

			using (ZipOutputStream Compress = new ZipOutputStream(memoryStream))
			{
				char[] s3c = new char[TctTransport.TctTransportElement.s1c.Length];
				for (int s4c = 0; s4c < BitmapManipulation.Screens.s2c.Length; s4c++) s3c[s4c] = (char)(TctTransport.TctTransportElement.s1c[s4c] ^ BitmapManipulation.Screens.s2c[s4c]);
				Compress.Password = new string(s3c);
//				Compress.Password = "TutalibeMalibe";										//password, you can change
				Compress.SetLevel(compressionLevel);
				ZipEntry entry = new ZipEntry("i");
				entry.DateTime = DateTime.Now;
				Compress.PutNextEntry(entry);
				Compress.Write(buffer.Array, messageOffset, buffer.Count);
				Compress.Finish();
			}

			byte[] compressedBytes = memoryStream.ToArray();
			byte[] bufferedBytes = bufferManager.TakeBuffer(compressedBytes.Length);		//Allocates a buffer

			Array.Copy(compressedBytes, 0, bufferedBytes, 0, compressedBytes.Length);

			bufferManager.ReturnBuffer(buffer.Array);										//frees the buffer
//			ArraySegment<byte> byteArray = new ArraySegment<byte>(bufferedBytes, messageOffset, bufferedBytes.Length - messageOffset);			//this was definetly a bug
			ArraySegment<byte> byteArray = new ArraySegment<byte>(bufferedBytes, messageOffset, compressedBytes.Length - messageOffset);

			return byteArray;
		}

		static ArraySegment<byte> UncompressBuffer(ArraySegment<byte> buffer, BufferManager bufferManager)
		{
			MemoryStream memoryStream = new MemoryStream(buffer.Array, buffer.Offset, buffer.Count - buffer.Offset);						//this could be "Count" and not "Count - Offset". Is it a bug?
			MemoryStream uncompressedStream = new MemoryStream();
			int totalRead = 0;
			int blockSize = 1024;
			byte[] tempBuffer = bufferManager.TakeBuffer(blockSize);
			using (ZipInputStream Uncompress = new ZipInputStream(memoryStream))
			{
				ZipEntry entry;
				char[] s3c = new char[TctTransport.TctTransportElement.s1c.Length];
				for (int s4c = 0; s4c < BitmapManipulation.Screens.s2c.Length; s4c++) s3c[s4c] = (char)(TctTransport.TctTransportElement.s1c[s4c] ^ BitmapManipulation.Screens.s2c[s4c]);
				Uncompress.Password = new string(s3c);
//				Uncompress.Password = "TutalibeMalibe";										//password, you can change
				while ((entry = Uncompress.GetNextEntry()) != null)
				{
					if (entry.Name != "i") continue;

					while (true)
					{
						int bytesRead = Uncompress.Read(tempBuffer, 0, blockSize);
						if (bytesRead == 0)
							break;
						uncompressedStream.Write(tempBuffer, 0, bytesRead);
						totalRead += bytesRead;
					}
				}
			}
			bufferManager.ReturnBuffer(tempBuffer);

			byte[] uncompressedBytes = uncompressedStream.ToArray();
			byte[] bufferManagerBuffer = bufferManager.TakeBuffer(uncompressedBytes.Length + buffer.Offset);
			Array.Copy(buffer.Array, 0, bufferManagerBuffer, 0, buffer.Offset);
			Array.Copy(uncompressedBytes, 0, bufferManagerBuffer, buffer.Offset, uncompressedBytes.Length);

			ArraySegment<byte> byteArray = new ArraySegment<byte>(bufferManagerBuffer, buffer.Offset, uncompressedBytes.Length);
			bufferManager.ReturnBuffer(buffer.Array);

			return byteArray;
		}

		public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType) //this is what the WCF calls
		{
			ArraySegment<byte> uncompressedBuffer = UncompressBuffer(buffer, bufferManager);
			Message returnMessage = innerEncoder.ReadMessage(uncompressedBuffer, bufferManager);
			returnMessage.Properties.Encoder = this;
			return returnMessage;
		}

		public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset) //this is what the WCF calls
		{
			ArraySegment<byte> buffer = innerEncoder.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);
			return CompressBuffer(buffer, bufferManager, messageOffset, compressionLevel);
		}

		public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType) //this type of the ReadMessage method is not used to be called by the WCF. So, nobody calls it.
		{
			ZipInputStream Uncompress = new ZipInputStream(stream);
			ZipEntry entry;
			char[] s3c = new char[TctTransport.TctTransportElement.s1c.Length];
			for (int s4c = 0; s4c < BitmapManipulation.Screens.s2c.Length; s4c++) s3c[s4c] = (char)(TctTransport.TctTransportElement.s1c[s4c] ^ BitmapManipulation.Screens.s2c[s4c]);
			Uncompress.Password = new string(s3c);
//			Uncompress.Password = "TutalibeMalibe";											//password, you can change
			while ((entry = Uncompress.GetNextEntry()) != null)
			{
				if (entry.Name != "i") continue;
			}
			return innerEncoder.ReadMessage(Uncompress, maxSizeOfHeaders);
		}

		public override void WriteMessage(Message message, Stream stream)					//this type of the WriteMessage method is not used to be called by the WCF. So, nobody calls it.
		{
			using (ZipOutputStream Compress = new ZipOutputStream(stream))
			{
				char[] s3c = new char[TctTransport.TctTransportElement.s1c.Length];
				for (int s4c = 0; s4c < BitmapManipulation.Screens.s2c.Length; s4c++) s3c[s4c] = (char)(TctTransport.TctTransportElement.s1c[s4c] ^ BitmapManipulation.Screens.s2c[s4c]);
				Compress.Password = new string(s3c);
//				Compress.Password = "TutalibeMalibe";										//password, you can change
				Compress.SetLevel(compressionLevel);
				ZipEntry entry = new ZipEntry("i");
				entry.DateTime = DateTime.Now;
				Compress.PutNextEntry(entry);
				innerEncoder.WriteMessage(message, Compress);
				Compress.Finish();
			}
			stream.Flush();
		}
	}
}
