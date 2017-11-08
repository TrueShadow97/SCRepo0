/*
 * Created by SharpDevelop.
 * User: Pavillion G-6
 * Date: 20.11.2016
 * Time: 18:09
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Separator
{
	/// <summary>
	/// Интерфейс двигателя, поддерживающего задержки запуска и останова.
	/// </summary>
	public interface IDelayable
	{
		decimal StartDelay {get; set;}
		decimal StopDelay {get; set;}
		
		void ForceState();
	}
}
