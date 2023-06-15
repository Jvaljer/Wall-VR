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

public delegate bool SmartiesWidgetHandler(SmartiesWidget w, SmartiesEvent e, object o);

public class SmartiesWidget
{

public static readonly int SMARTIES_WIDGET_TYPE_TEXTVIEW      = 0;
public static readonly int SMARTIES_WIDGET_TYPE_BUTTON        = 1;
public static readonly int SMARTIES_WIDGET_TYPE_TOGGLE_BUTTON = 2;
public static readonly int SMARTIES_WIDGET_TYPE_TEXT_BUTTON   = 3;
public static readonly int SMARTIES_WIDGET_TYPE_CHECKBOX      = 4;
public static readonly int SMARTIES_WIDGET_TYPE_SLIDER        = 5;
public static readonly int SMARTIES_WIDGET_TYPE_SPINNER       = 6;
public static readonly int SMARTIES_WIDGET_TYPE_POPUPMENU     = 8;
public static readonly int SMARTIES_WIDGET_TYPE_MULTICHOICE   = 9;
public static readonly int SMARTIES_WIDGET_TYPE_INCTEXT_BUTTON   = 20;
public static readonly int  SMARTIES_WIDGET_TYPE_DUBUTTON  = 21;
public static readonly int  SMARTIES_WIDGET_TYPE_DMUBUTTON = 22;

public static readonly int SMARTIES_WIDGET_STATE_ENABLED   = 0;
public static readonly int SMARTIES_WIDGET_STATE_DISABLED  = 1;
public static readonly int SMARTIES_WIDGET_STATE_HIDDEN    = 2;

public static readonly int SMARTIES_WIDGET_BUTTON_CLICK  = 1;
public static readonly int SMARTIES_WIDGET_BUTTON_DOWN   = 2;
public static readonly int SMARTIES_WIDGET_BUTTON_UP     = 3;
public static readonly int SMARTIES_WIDGET_BUTTON_MOVE   = 4;


public int uid;
// description to be send to the clients by the server via osc
public int type;
public int visibility;
public string label;
public string labelOn;
public List<string> items; 
public  float posx;
public float posy;
public float width;
public float height;
// dynamic values to be updated by the clients via osc and used by the server
public bool on; // for toggle button
public string text; // last entred text, might be reset by the server
public bool cancel; // for ok/cancel dialog 
public int slider_value;
public int item; // spinner
public List<bool> checked_items; // multi_choice
public int touch_action; // for DUBUTTON
public float bx,by; // for DMUBUTTON
public int clicked;  // num clicked for button, should be rested by the server
public bool dirty; // server should take care ...
public int pid; // id of the active puck
public int font_size; // font size in ...

// FIXME !!!!

public SmartiesWidgetHandler  handler;

//public Object handler;
//std::tr1::function<bool (SmartiesWidget *, SmartiesEvent *, void *)> (handler);

public SmartiesWidget(
	int uid, int type, string lab, float posx, float posy, float width, float height)
{
	this.uid = uid; this.type = type;
	this.posx = posx; this.posy = posy;
	this.width = width; this.height = height;
	label = lab;
	labelOn = lab;

	visibility = SMARTIES_WIDGET_STATE_ENABLED;
	items = new List<string>();
	items.Clear();
	item = 0;
	checked_items = new List<bool>();
	checked_items.Clear();
	// clear
	clicked = 0;
	on = false;
	text = null;
	cancel = false;
	slider_value = 0;
	dirty = false;
	// handler = null; FIXME
	handler = null; // delegate(SmartiesWidget w, SmartiesEvent e, object o) { };
	pid = -1;
	font_size = 0;
}

public void cleanUpCheckedItems()
{
	if (checked_items.Count != items.Count)
	{
		List<bool> citems = new List<bool>();
		for (int j = 0; j < checked_items.Count; j++) 
		{
			citems.Add(checked_items[j]);
		}
		checked_items.Clear();
		for (int j = 0; j < items.Count; j++) 
		{
			if (j >= citems.Count)
			{
				checked_items.Add(false);
			}
			else
			{
				checked_items.Add(citems[j]);
			}
		}
	}
}

}
