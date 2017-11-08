/*
 * Created by SharpDevelop.
 * User: Pavillion G-6
 * Date: 20.11.2016
 * Time: 18:05
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Separator
{
	/// <summary>
	/// Интерфейс двигателя, который может регулироваться PWM-механизмом (импульс/пауза).
	/// </summary>
	public interface IPWMable
	{
		decimal Impulse {get; set;}
		decimal Pause {get; set;}
	}
}
