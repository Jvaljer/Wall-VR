/*
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 * MA 02110-1301 USA
 *
 */
/* 
 * Copyright Olivier Chapuis (CNRS), 2013. <chapuis@lri.fr>
 */

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

using UnityEngine;

public class SmartiesColors
{

static private readonly int[] pucksRGBColors = new int[] {
	0xFF9900, 0x91FF00, 0x00F7FF, 0xFF0000, 0xFF00AA, 0x9100FF, 0x1100FF,
	0x00CCFF, 0x00FF4D, 0xDDFF00};

/**
 * return  the color of the puck with identifier id.
 *
 * @param id  {@link SmartiesPuck} puck indentifier
 * @return a java.awt.Color color
 * 
 */
static public Color getPuckColorById(int id) 
{
	int c = getSmartieIntRGBColor(id);

    return new Color(
    	(float)(c >> 16)/255.0f,
		(float)((c & 0xFF00) >> 8)/255.0f,
		(float)((c & 0xFF))/255.0f,
		1.0f);
}
 
/**
 * return the default color of the puck with identifier id as an int.
 *
 * @param id  {@link SmartiesPuck} puck indentifier
 * @return  a int 
 * 
 */

static public int getSmartieIntRGBColor(int id) 
{
	int c = id;
	if (c >= pucksRGBColors.Length)
	{
		c =  c % pucksRGBColors.Length;
	}
    	return pucksRGBColors[c];
}
 
SmartiesColors() {}

}