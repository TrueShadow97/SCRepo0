/*
 * Created by SharpDevelop.
 * User: Pavillion G-6
 * Date: 20.11.2016
 * Time: 12:46
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Runtime.Serialization;

namespace Separator
{
	/// <summary>
	/// Description of DataModeException.
	/// </summary>
	public class DataModeException : Exception, ISerializable
	{
		public DataModeException()
		{
		}

	 	public DataModeException(string message) : base(message)
		{
		}

		public DataModeException(string message, Exception innerException) : base(message, innerException)
		{
		}

		// This constructor is needed for serialization.
		protected DataModeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}