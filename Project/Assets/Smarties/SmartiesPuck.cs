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

public class SmartiesPuck
{

public static readonly int SMARTIESPUCK_CURSORTYPE_NONE   =   -1; 
public static readonly int SMARTIESPUCK_CURSORTYPE_CROSS  =    0;
public static readonly int SMARTIESPUCK_CURSORTYPE_ARROW  =    1;
public static readonly int SMARTIESPUCK_CURSORTYPE_POINT  =    2;
public static readonly int SMARTIESPUCK_CURSORTYPE_SELECT =    3;
public static readonly int SMARTIESPUCK_CURSORTYPE_TEXT   =    4;
public static readonly int SMARTIESPUCK_CURSORTYPE_CIRCLE =    5;
public static readonly int SMARTIESPUCK_CURSORTYPE_MOVE   =    6;

/**
 * Unique identifier (positive or zero). You must not change this value!
 */
public int id;
/**
 * x relative position of the puck in the interval [0,1], uses {@link Smarties#movePuck} to change the position of a puck.
 */
public float x;
/**
 * y relative position of the puck in the interval [0,1],
 * uses {@link Smarties#movePuck} to change the position of a puck.
 */
public float y;
/**
 * the icon to be drawn in the puck on the mobile client.
 * See the SMARTIESPUCK_CURSORTYPE_* constants and  {@link Smarties#sendPuckCursorType} to change it.
 */
public int cursor_type;
/**
 * the puck color as a RGB int (e.g., 0xFF00A0), see {@link Smarties#sendPuckColor} to change the default.
 */
public int color;
/**
 * not used for now.
 */
public int flags;
//
/**
 * used internaly (only ?)
 */
public int move_state;
/**
 * is the puck stored
 */
public bool stored;
/**
 * is the puck deleted.
 * <p>
 * 
 */
public bool deleted;
/**
 * The device where the puck is selected or null if there is no such a device. 
 * 
 */
public SmartiesDevice selected_by_device = null;
public SmartiesDevice sharing_policy_device = null;
//
/**
 * the "clipboard", uses this object to store what ever you want with your pucks.
 * 
 */
public object app_data;

public void setPosition(float x_, float y_)
{
	x = x_; y = y_;
}

public void setMoveState(int m)
{
	move_state = m;
}

public int getMoveState()
{
	return move_state;
}

public void setStored(bool v)
{
	stored = v;
}

public void setStored()
{
	stored = true;
}

public bool isStored()
{
	return stored;
}

public void setDeleted(bool v)
{
	deleted = v;
}

public void setDeleted()
{
	deleted = true;
}

public bool isDeleted()
{
	return deleted;
}

public void setSelectedByDevice(SmartiesDevice d)
{
	selected_by_device = d;
}

public SmartiesDevice getSelectedByDevice()
{
	return selected_by_device;
}

public void setSharingPolicyDevice(SmartiesDevice d)
{
	sharing_policy_device = d;
}

public SmartiesDevice getSharingPolicyDevice()
{
	return sharing_policy_device;
}


public SmartiesPuck(SmartiesDevice d, int id_, float x_, float y_, int type_)
{
	selected_by_device = sharing_policy_device = d;
	id = id_; x = x_; y = y_; cursor_type = type_;
	flags = 0;
	deleted = false;
	stored = false;
	move_state = 0;
	color = SmartiesColors.getSmartieIntRGBColor(id_); 
	app_data = null;
}

}