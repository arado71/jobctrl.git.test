using System;
using System.Xml;
using System.Configuration;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;

namespace TctEncoder
{
	public class TctBindingElementExtension : BindingElementExtensionElement										//This class will be called by the App.config
	{
		public override Type BindingElementType																		//WCF calls it, independently from us, so it has to be here
		{
			get { return typeof(TctMessageEncodingBindingElement); }
		}

		[ConfigurationProperty("innerMessageEncoding", DefaultValue = "binaryMessageEncoding")]						///**/Default is changed to "binaryMessageEncoding"
		public string InnerMessageEncoding																			//WCF calls it, independently from us, so it has to be here
		{
			get { return (string)base["innerMessageEncoding"]; }
			set { base["innerMessageEncoding"] = value; }
		}

		[ConfigurationProperty("readerQuotas")]
		public XmlDictionaryReaderQuotasElement ReaderQuotas
		{
			get { return (XmlDictionaryReaderQuotasElement) base["readerQuotas"]; }
		}

		[ConfigurationProperty("compressionLevel", DefaultValue = "9")]
		public string CompressionLevel
		{
			get { return (string)base["compressionLevel"]; }
			set { base["compressionLevel"] = value; }
		}

		public override void ApplyConfiguration(BindingElement bindingElement)										//WCF calls it, independently from us, so it has to be here
		{
			TctMessageEncodingBindingElement binding = (TctMessageEncodingBindingElement)bindingElement;
			PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
			if (propertyInfo["innerMessageEncoding"].ValueOrigin != PropertyValueOrigin.Default)
			{
				switch (this.InnerMessageEncoding)
				{
					case "textMessageEncoding":
						binding.InnerMessageEncodingBindingElement = new TextMessageEncodingBindingElement();
						break;
					case "binaryMessageEncoding":
						binding.InnerMessageEncodingBindingElement = new BinaryMessageEncodingBindingElement();
						break;
				}
			}
			if (propertyInfo["compressionLevel"].ValueOrigin != PropertyValueOrigin.Default) binding.CompressionLevel = int.Parse(CompressionLevel);
		}

		protected override BindingElement CreateBindingElement()													//WCF calls it, independently from us, so it has to be here
		{
			TctMessageEncodingBindingElement bindingElement = new TctMessageEncodingBindingElement();
			this.ApplyConfiguration(bindingElement);
			return bindingElement;
		}
	}

    public sealed class TctMessageEncodingBindingElement : MessageEncodingBindingElement							//it is called by the CreateBindingElement method of the previous class, this establishes the connection with the other computer, this installs the Factory
    {
		MessageEncodingBindingElement innerBindingElement;															//created by the TctBindingElementExtension.CreateBindingElement, this is a new BinaryMessageEncodingBindingElement

        public TctMessageEncodingBindingElement() : this(new BinaryMessageEncodingBindingElement())					///**/it is called by the CreateBindingElement. Changed to "BinaryMessageEncodingBindingElement".
		{
		}

		public TctMessageEncodingBindingElement(MessageEncodingBindingElement messageEncoderBindingElement)			//it is called by the CreateBindingElement.
        {
            this.innerBindingElement = messageEncoderBindingElement;
        }

        public MessageEncodingBindingElement InnerMessageEncodingBindingElement										//it is called only by us, so it is not necessary, top of that, we us it only for storing data into an our property
        {
            get { return innerBindingElement; }
            set { innerBindingElement = value; }
        }

		public XmlDictionaryReaderQuotas ReaderQuotas
		{
			get
			{
				BinaryMessageEncodingBindingElement el1 = innerBindingElement as BinaryMessageEncodingBindingElement;
				if (el1 != null) return el1.ReaderQuotas;
				TextMessageEncodingBindingElement el2 = innerBindingElement as TextMessageEncodingBindingElement;
				if (el2 != null) return el2.ReaderQuotas;
				return null;
			}
		}

	    private int compressionLevel = 9;
	    public int CompressionLevel
	    {
		    get { return compressionLevel; }
		    set
		    {
				if (value < 0 || value > 9) throw new ArgumentOutOfRangeException("CompressionLevel", "Compression level must be between 0 and 9.");
				compressionLevel = value;
		    }
	    }

		public override MessageEncoderFactory CreateMessageEncoderFactory()											//WCF calls it, BuildChannelListener of the WCF, this installs the Factory
        {
            return new TctMessageEncoderFactory(innerBindingElement.CreateMessageEncoderFactory(), CompressionLevel);
        }

		public override MessageVersion MessageVersion																//WCF calls it, independently from us, so it has to be here
        {
            get { return innerBindingElement.MessageVersion; }
            set { innerBindingElement.MessageVersion = value; }
        }

		public override BindingElement Clone()																		//WCF calls it, independently from us, so it has to be here
        {
            return new TctMessageEncodingBindingElement(this.innerBindingElement) { CompressionLevel = CompressionLevel };
        }

		public override T GetProperty<T>(BindingContext context)													//WCF calls it, independently from us, so it has to be here
        {
            if (typeof(T) == typeof(XmlDictionaryReaderQuotas))
            {
                return innerBindingElement.GetProperty<T>(context);
            }
            else 
            {
                return base.GetProperty<T>(context);
            }
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)				//it is never called, but the VS and the .NET demand of the existing of this method
        {
            if (context == null) throw new ArgumentNullException("context");
            context.BindingParameters.Add(this);
			GetProperty<XmlDictionaryReaderQuotas>(context).MaxArrayLength = 10000000;								///**/new line, it is necessary for transfering bigger objects
			return context.BuildInnerChannelFactory<TChannel>();
        }

		public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)			//WCF calls it, independently from us, so it has to be here
        {
            if (context == null) throw new ArgumentNullException("context");
            context.BindingParameters.Add(this);
			GetProperty<XmlDictionaryReaderQuotas>(context).MaxArrayLength = 10000000;								///**/new line, it is necessary for transfering bigger objects
			return context.BuildInnerChannelListener<TChannel>();
        }

		public override bool CanBuildChannelListener<TChannel>(BindingContext context)								//WCF calls it, independently from us, so it has to be here
        {
            if (context == null) throw new ArgumentNullException("context");
            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }
    }
}
