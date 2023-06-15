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
using UnityOSC;

public class SmartiesDevice
{
	public int compa_version = 0;
	public bool restarted = false;

	private string _ip;
	private string _mac = null;
	private int _port;
	private bool _use_tcp;
	// private Smarties _smarties; not use (for now)
	private int _selectedPuck;
	private bool _selectedPuckDirty;

	private int _wa_width,_wa_height; 
	private int _tp_width,_tp_height; 
	private float _xpixelsBymm, _ypixelsBymm;

	OSCClient _client;

	public void sendHelloMsg(
		int w, int h, int gridx, int gridy, bool puretp, bool npb,
		bool nsb, bool ndi, bool nsi, bool pwa, int orient)
	{
		List<object> args = new List<object>();
		args.Add("1.0");
		args.Add(w);
		args.Add(h);
		args.Add(gridx);
		args.Add(gridy);
		args.Add((puretp)? 1:0);
		args.Add(Smarties.MY_COMPA_VERSION); // compa version of the proto
		args.Add((npb)? 1:0);
		args.Add((nsb)? 1:0);
		args.Add((ndi)? 1:0);
		args.Add((nsi)? 1:0);
		args.Add((pwa)? 1:0);
		args.Add(orient);
		_sendMessage("/HelloMessage", args);
	}

	public void sendWidgets(
		int wgridx, int wgridy, List<SmartiesWidget> widgets)
	{
		List<object> args = new List<object>();
	
		args.Add(wgridx);
		args.Add(wgridy);
		args.Add(widgets.Count);

		for (int i= 0; i < widgets.Count; i++)
		{
			SmartiesWidget sw = widgets[i];
			args.Add(sw.uid);
			args.Add(sw.type);
			args.Add(sw.visibility);
			args.Add(sw.posx); 
			args.Add(sw.posy); 
			args.Add(sw.width); 
			args.Add(sw.height); 
			args.Add(sw.label);
			args.Add(sw.labelOn);
			args.Add((sw.on)? 1:0);
			args.Add(sw.slider_value);
			args.Add(sw.items.Count);
			for (int j = 0; j < sw.items.Count; j++)
			{
				args.Add(sw.items[j]);
			}
			args.Add(sw.item);
			// since compa version 1
			if (compa_version >= 1)
			{
				args.Add(sw.checked_items.Count);
				for (int j = 0; j < sw.checked_items.Count; j++)
				{
					args.Add((sw.checked_items[j])? 1:0);
				}
			}
			if (compa_version >= 2)
			{	
				args.Add(sw.font_size);
			}
		}

		_sendMessage("/WidgetsList", args);
	}

	// -----------------------------------------------------------------------------------
	// conf

	public void sendSmartiesTouchEventsConf(bool v)
	{
		List<object> args = new List<object>();

		args.Add((v)? 1:0);

		_sendMessage("/SmartiesTouchEventsConf", args);
	}

	public void sendRawTouchEventsConf(bool v)
	{
		List<object> args = new List<object>();
	
		args.Add((v)? 1:0);

		_sendMessage("/RawTouchEventsConf", args);
	}


	// -----------------------------------------------------------------------------------
	// widgets
	 
	public void sendShowkeyboard(int uid, int num_letters)
	{
		List<object> args = new List<object>();

		args.Add(uid);
		args.Add(num_letters);

		_sendMessage("/ShowKeyboard", args);
	}

	public void sendWidgetLabel(int uid, string str)
	{
		List<object> args = new List<object>();
		
		args.Add(uid);
		args.Add(str);

		_sendMessage("/SetWidgetLabel", args);
	}

	public void sendWidgetFontSize(int uid, int size)
	{
		List<object> args = new List<object>();
		
		args.Add(uid);
		args.Add(size);

		_sendMessage("/SetWidgetFontSize", args);
	}

	public void sendWidgetItem(int uid, int item)
	{
		List<object> args = new List<object>();
		
		args.Add(uid);
		args.Add(item);

		_sendMessage("/SetWidgetItem", args);
	}

	public void sendWidgetValue(int uid, int value)
	{
		List<object> args = new List<object>();
		
		args.Add(uid);
		args.Add(value);

		_sendMessage("/SetWidgetValue", args);
	}

	public void sendWidgetOnState(int uid, bool state)
	{
		List<object> args = new List<object>();
		
		args.Add(uid);
		args.Add((state)? 1:0);

		_sendMessage("/SetWidgetOnState", args);
	}

	public void sendWidgetState(int uid, int state)
	{
		List<object> args = new List<object>();
		
		args.Add(uid);
		args.Add(state);

		_sendMessage("/SetWidgetVisibility", args);
	}


	public void replaceWidgetItemsList(int uid,  List<string> items)
	{
		List<object> args = new List<object>();

		args.Add(uid);
		args.Add(items.Count);
		for (int j = 0; j < items.Count; j++)
		{
			args.Add(items[j]);
		}

		_sendMessage("/ReplaceWidgetItemsList", args);
	}

	public void addItemInWidgetList(int uid, string item, int pos)
	{
		List<object> args = new List<object>();
		
		args.Add(uid);
		args.Add(item);
		args.Add(pos);

		_sendMessage("/AddItemInWidgetList", args);
	}


	public void removeItemInWidgetList(int uid, int pos)
	{
		List<object> args = new List<object>();
		
		args.Add(uid);
		args.Add(pos);

		_sendMessage("/RemoveItemInWidgetList", args);	
	}

	public void sendWidgetCheckedItems(int uid, List<bool> checked_items){
		List<object> args = new List<object>();
		

		args.Add(uid);
		args.Add(checked_items.Count);
		for (int j = 0; j < checked_items.Count; j++)
		{
			args.Add((checked_items[j])? 1:0);
		}
		_sendMessage("/SetWidgetCheckedItems", args);
	}

	public void sendNotification(int soundType, int soundDuration, int vibration, string str)
	{
		List<object> args = new List<object>();
		
		args.Add(soundType);
		args.Add(soundDuration);
		args.Add(vibration);
		if (str == null){
			str = "";
		}
		args.Add(str);

		_sendMessage("/Notification", args);	
	}

	// -----------------------------------------------------------------------------------
	// puck
	public void sendNewPuck(SmartiesPuck p, int sharing_policy)
	{
		int lockstatus = 0; // free
		if (p.selected_by_device != null)
		{
			lockstatus = (p.selected_by_device == this)? 2:1;
		}
		if (sharing_policy == Smarties.SMARTIES_SHARINGPOL_STRICT &&
		    p.sharing_policy_device != null && p.selected_by_device != this)
		{
			if (p.sharing_policy_device != this)
			{
				lockstatus = 1;
			}
		}
		if (lockstatus != 2 && sharing_policy == Smarties.SMARTIES_SHARINGPOL_PERMISSIVE)
		{
			lockstatus = 0;
		}

		List<object> args = new List<object>();

		args.Add(p.id);
		args.Add(p.x);
		args.Add(p.y);
		args.Add(p.cursor_type);
		args.Add(lockstatus);
		args.Add(p.color);
		args.Add(p.flags);

		//System.out.println("/NewPuck: " +  p.id + " " + lockstatus);
		_sendMessage("/NewPuck", args);
	}

	public void sendDeletePuck(SmartiesPuck p)
	{
		List<object> args = new List<object>();

		args.Add(p.id);

		_sendMessage("/DeletePuck", args);
	}

	public void sendStorePuck(SmartiesPuck p)
	{
		List<object> args = new List<object>();

		args.Add(p.id);

		_sendMessage("/StorePuck", args);
	}

	public void sendUnstorePuck(SmartiesPuck p)
	{
		List<object> args = new List<object>();

		args.Add(p.id);

		_sendMessage("/UnstorePuck", args);
	}

	public void sendMovePuck(SmartiesPuck p)
	{
		List<object> args = new List<object>();

		args.Add(p.id);
		args.Add(p.x);
		args.Add(p.y);

		_sendMessage("/MovePuck", args);
	}

	public void sendPuckLockStatus(SmartiesPuck p, int status)
	{
		List<object> args = new List<object>();

		args.Add(p.id);
		args.Add(status);

		_sendMessage("/PuckLockStatus", args);
	}

	public void sendPuckCursorType(SmartiesPuck p)
	{
		List<object> args = new List<object>();

		args.Add(p.id);
		args.Add(p.cursor_type);

		_sendMessage("/PuckCursorType", args);
	}
	    
	public void sendPuckColor(SmartiesPuck p)
	{
		List<object> args = new List<object>();

		args.Add(p.id);
		args.Add(p.color);

		_sendMessage("/PuckColor", args);
	}
	    
	public void sendHideLockedPucks(bool v)
	{
		List<object> args = new List<object>();

		args.Add((v)? 1:0);

		_sendMessage("/HideLockedPucks", args);
	}

	public void sendAccelerationParameters(float maxCDgainFactor, float minCDgainMM)
	{
		List<object> args = new List<object>();

		args.Add(maxCDgainFactor);
		args.Add(minCDgainMM);

		_sendMessage("/AccelerationParameters", args);
	}
    

	// -----------------------------------------------------------------------
	// basic
	/**
	 * Return the IP address of the device
	 */
	public string getIP() { return _ip; }
	/**
	 * Return the MAC address of the device.
	 */
	public string getMacAddress() { return _mac; }

	public void setMacAddress(string mac) {_mac = mac; }

	public int getSelectedPuckID() { return _selectedPuck; }

	public void setSelectedPuckID(int id) { 
		if (id != _selectedPuck) {
			_selectedPuck = id;
			_selectedPuckDirty = true;
		}
	}

	public bool isSelectedPuckDirty() { return _selectedPuckDirty; }

	public void setSelectedPuckDirty(bool v) { _selectedPuckDirty = v; }

	public void setWidgetsAreaSize(int w, int h) { _wa_width=w; _wa_height=h; }
	public void setTouchpadSize(int w, int h) { _tp_width=w; _tp_height=h; }
	public void setPixelsByMM(float xpixBymm, float ypixBymm) { _xpixelsBymm = xpixBymm; _ypixelsBymm = ypixBymm; }

	/**
	 * Return the width of the widget area 
	 */
	public int getWidgetsAreaWidth() { return _wa_width; }
	/**
	 * Return the height of the widget area 
	 */
	public int getWidgetsAreaHeight() { return _wa_height; }
	/**
	 * Return the width of the touchpad area 
	 */
	public int getTouchpadWidth() { return _tp_width; }
	/**
	 * Return the height of the touchpad area 
	 */
	public int getTouchpadHeight() { return _tp_height; }
	/**
	 * Return the number of pixels by mm in the x coordinate
	 */
	public float getXPixelsByMM() { return _xpixelsBymm; }
	/**
	 * Return the number of pixels by mm in the y coordinate
	 */
	public float getYPixelsByMM() { return _ypixelsBymm; }

	// -----------------------------------------------------------------------
	
	private void _sendMessage<T>(string address, List<T> values)
	{	
			OSCMessage message = new OSCMessage(Smarties.OSC_PREFIX_SMARTIES + address);
		
			foreach(T msgvalue in values)
			{
				message.Append(msgvalue);
			}
			
			_client.Send(message);
	}

	public void reconnect(int proto)
	{
		// FIXME  only udp here !!!!
		// if (false && _use_tcp)
		// {
		// 	if (proto != -1)
		// 	{
		// 		_use_tcp = (proto == 1);
		// 	}
		// 	connect();
		// }
	}

	void connect(){
		_client = new OSCClient(IPAddress.Parse(_ip), _port);
	}

	public SmartiesDevice(string ip, int port, Smarties s, int proto)
	{
		_ip = ip+""; 
		_port = port;
		_use_tcp = (proto == 1); // only udp here !!!!
		_client = null;
		//_smarties = s;
		_selectedPuck = -2;
		_selectedPuckDirty = false;
		restarted = false;

		connect();
	}
}