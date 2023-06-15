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

public delegate void SmartiesUpdateHandler(Smarties s, SmartiesEvent e);

public class Smarties
{

	private int DEFAULT_PORT_SMARTIES = 57110;
	public static readonly int SMARTIES_OSC_OUT_PORT = 57120;
	
	public static readonly int MY_COMPA_VERSION = 2;

	public static readonly string OSC_PREFIX_SMARTIES = "/Smarties";

	public static readonly int SMARTIES_SHARINGPOL_MEDIUM     =  0;
	public static readonly int SMARTIES_SHARINGPOL_STRICT     =  1;
	public static readonly int SMARTIES_SHARINGPOL_PERMISSIVE =  2;
	public static readonly int SMARTIES_SHARINGPOL_CUSTOM     =  3;

	// FIXME: use that!
	public static readonly int SMARTIES_PUCK_STATUS_FREE      = 0;
	public static readonly int SMARTIES_PUCK_STATUS_LOCKED    = 1;
	public static readonly int SMARTIES_PUCK_STATUS_SELECTED  = 2;

	public static readonly int SMARTIES_DEVICE_ORIENTATION_LANDSCAPE = 0;
	public static readonly int SMARTIES_DEVICE_ORIENTATION_PORTRAIT  = 1;

	public readonly static int  SMARTIES_SOUND_NONE = 0;
	public readonly static int  SMARTIES_SOUND_NOTIFICATION = 1;
	public readonly static int  SMARTIES_SOUND_RINGTONE = 2;
	public readonly static int  SMARTIES_SOUND_ALARM = 3;

	private int port;
	private int _app_width, _app_height, _gridx, _gridy;
	private int _default_cursor_type;

	private Dictionary<string, SmartiesDevice> _devices;
	private Dictionary<int, SmartiesPuck> _pucks;

	private int _widgets_gridx, _widgets_gridy;
	private List<SmartiesWidget> _widgets;
	private int _widget_uid;

	private bool pureTouchpad = false;
	private bool pureWidgetArea = false;
	private bool onePuckByDevice = false;
	private bool noPuckButton = false;
	private bool noShareButton = false;
	private bool noDeleteIcon = false;
	private bool noStoreIcon = false;

	private int deviceOrientation = SMARTIES_DEVICE_ORIENTATION_LANDSCAPE;

	// conf
	private bool smartiesTouchEventsConf = true;
	private bool rawTouchEventsConf = false;

	private bool hideLockedPucks = false;

	private bool accelParamSeted = false;
	private float maxCDgainFactorConf = 1.0f;
	private float maxCDgainMMConf = 1.0f;


	// sharing policy
	private int sharingPolicyConf = SMARTIES_SHARINGPOL_MEDIUM;

	// ----------------------------------------------------------------------------
	// 
	public event SmartiesUpdateHandler SmartiesUpdate;

	// ----------------------------------------------------------------------------
	// send to several devices  
	private void _sendNewPuck(SmartiesPuck p, string ip)
	{

		// for (Map.Entry<String, SmartiesDevice> entry : _devices.entrySet())
		// String ip = entry.getKey();
		// SmartiesDevice d = entry.getValue();
		foreach (SmartiesDevice d in _devices.Values)
		{
			if (ip != null && ip == d.getIP())
			{
				continue;
			}
			d.sendNewPuck(p, sharingPolicyConf);
		}
	}


	private void _sendDeletePuck(SmartiesPuck p, string ip)
	{
		foreach (SmartiesDevice d in _devices.Values)
		{
			if (ip != null && ip == d.getIP())
			{
				continue;
			}
			d.sendDeletePuck(p);
		}
	}


	private void _sendStorePuck(SmartiesPuck p, string ip)
	{
		foreach (SmartiesDevice d in _devices.Values)
		{
			if (ip != null && ip == d.getIP())
			{
				continue;
			}
			d.sendStorePuck(p);
		}
	}

	private void _sendUnstorePuck(SmartiesPuck p, string ip)
	{
		foreach (SmartiesDevice d in _devices.Values)
		{
			if (ip != null && ip == d.getIP())
			{
				continue;
			}
			d.sendUnstorePuck(p);
		}
	}

	private void _sendMovePuck(SmartiesPuck p, string ip)
	{
		foreach (SmartiesDevice d in _devices.Values)
		{
			if (ip != null && ip == d.getIP())
			{
				continue;
			}
			d.sendMovePuck(p);
		}
	}

	private void _sendPuckLockStatus(SmartiesPuck p, SmartiesDevice ed, int status)
	{
		foreach (SmartiesDevice d in _devices.Values)
		{
			if (ed == d)
			{
				continue;
			}
			d.sendPuckLockStatus(p, status);
		}
	}

	// ----------------------------------------------------------------------------
	// add (check) devices and pucks

	private SmartiesPuck _addPuck(SmartiesDevice d, float x, float y)
	{
		int id = 0;
		while(true)
		{
			if (!_pucks.ContainsKey(id))
			{
				break;
			}
			id++;
		}
		SmartiesPuck p = new SmartiesPuck(d, id, x, y, _default_cursor_type);
		_pucks.Add(id, p);

		if (true) {
			Debug.Log("Added a Puck with id " + id + " from device: " + d.getIP());
		}

		return p;
	}

	private bool _checkDevice(string ip, bool force, int proto)
	{
		SmartiesDevice d = null;

		if (_devices.ContainsKey(ip)){
			d = _devices[ip];
		}

		if (d != null && !force)
		{
			// found
			return false;
		}

		if (d == null)
		{ 
			d = new SmartiesDevice(ip, SMARTIES_OSC_OUT_PORT, this, proto); 
			_devices.Add(ip, d);
		}
		else
		{
			d.restarted = true;
			// TCP reconnect
			d.reconnect(proto);
		}
		// start the "hello" proto ... and see below
		d.sendHelloMsg(
			_app_width, _app_height, _gridx, _gridy, pureTouchpad,
			noPuckButton, noShareButton, noDeleteIcon, noStoreIcon,
			pureWidgetArea, deviceOrientation);

		if (true) {
			Debug.Log("New Smarties Device: " + ip + " " + !force);
		}

		return true;
	}

	private bool _checkDevice(string ip, bool force)
	{
		return _checkDevice(ip, force, 1);
	}


	private bool _checkDevice(string ip)
	{
		return _checkDevice(ip, false, 1);
	}


	// ----------------------------------------------------------------------------
	// handle main msgs

    void _handleNewConnection(List<object> args){
    	string ip = (string) args[0];
    	int proto = 0;
    	if (args.Count > 1)
		{
			proto = (int)args[1];	
		}
    	Debug.Log("_handleNewConnection " + ip + " " + proto);
    	_checkDevice(ip, true, proto);
    }

    void _handleGetHelloMessage(List<object> args){
    	
    	string ip = (string)args[0];
		int wawidth = (int)args[1];   // dim of the widget area
		int waheight = (int)args[2];
		string mac = null;
		int tpwidth = 0;   // dim of the touch pad area
		int tpheight = 0;
		float xpixelsBymm =1;
		float ypixelsBymm =1;

		int compa_version = 0;
		if (args.Count > 3)
		{
			compa_version = (int)args[3];	
		}
		if (args.Count > 4)
		{
			mac = (string)args[4];	
		}
		if (args.Count > 6)
		{
			tpwidth = (int)args[5];   // dim of the touch pad area
			tpheight = (int)args[6];
		}
		if (args.Count > 8)
		{
			xpixelsBymm = (float)args[7];
			ypixelsBymm = (float)args[8];
		}

		_checkDevice(ip);
		SmartiesDevice d = _devices[ip];
		Debug.Log("Get Hello Msg: " + ip + " " + compa_version+ " " + d.restarted);

		d.compa_version = compa_version;
		d.setWidgetsAreaSize(wawidth, waheight);
		d.setTouchpadSize(tpwidth, tpheight);
		d.setPixelsByMM(xpixelsBymm, ypixelsBymm);

		SmartiesPuck p = getPuck(d.getSelectedPuckID()); // what is this ? for a restart !!
		if (p == null) d.setSelectedPuckID(-1);
		d.setMacAddress(mac);

		// send default conf
		d.sendSmartiesTouchEventsConf(smartiesTouchEventsConf);
		d.sendRawTouchEventsConf(rawTouchEventsConf);
		d.sendHideLockedPucks(hideLockedPucks);
		if (accelParamSeted)
		{
			d.sendAccelerationParameters(maxCDgainFactorConf, maxCDgainMMConf);
		}

		// send the widgets
		d.sendWidgets(_widgets_gridx,_widgets_gridy, _widgets);

		if (onePuckByDevice)
		{
			if (p == null)
			{
				float x = 0.45f; 
				float y = 0.45f;
				p = _addPuck(d, x, y);
			
				SmartiesEvent e = new SmartiesEvent();
				e.p = p; e.id = e.p.id; e.x = p.x; e.y = p.y;
				e.device = d;
				e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_CREATE;
				//System.out.println("_handleNewPuck: _fireEvent");
				_fireEvent(e);
			}
			// send back to the device so that it could have the id ...
			p.setSelectedByDevice(d);
			d.sendNewPuck(p, sharingPolicyConf);
		}
		else
		{
			// send the pucks to this device
			foreach (SmartiesPuck np in _pucks.Values)
			{
				d.sendNewPuck(np, sharingPolicyConf);
			}

			foreach (SmartiesPuck np in _pucks.Values)
			{
				if (np.stored)
				{
					d.sendStorePuck(np);
				}
			}
		}

		// 
		if (!d.restarted)
		{
			SmartiesEvent ce = new SmartiesEvent();
			ce.id = (p != null)? p.id:-1; ce.p = p;
			ce.device = d;
			ce.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_NEW_DEVICE;
			_fireEvent(ce);
		}
    	Debug.Log("_handleGetHelloMessage");
    }

    void _handleDeviceSizes(List<object> args) {
    	string ip = (string) args[0];
    	int wawidth = (int)args[1];   // dim of the widget area
		int waheight = (int)args[2];
		int tpwidth = (int)args[3];   // dim of the touch pad area
		int tpheight = (int)args[4];

		if (_checkDevice(ip))
		{
			// new device
			return;
		}
		
		SmartiesDevice d = _devices[ip];
		d.setWidgetsAreaSize(wawidth, waheight);
		d.setTouchpadSize(tpwidth, tpheight);

		SmartiesEvent e = new SmartiesEvent();
		e.p = null; e.id = -1; e.x = -1; e.y = -1;
		e.device = d;
		e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_DEVICE_SIZES;
		//System.out.println("_handleDeviceSizes: _fireEvent");
		_fireEvent(e);
    }

    void _handleFailToReadMessage(List<object> args) {
    	string ip = (string) args[0];
    	// TODO ... this never happen :)

    }
    
    void _handleDisconnect(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];

		SmartiesDevice d = getDevice(ip);

		if (d == null) return;

		Debug.Log("Disconnect: " + ip + " " + id);

		SmartiesPuck p = getPuck(id);
		if (p == null) return;
		if (sharingPolicyConf == SMARTIES_SHARINGPOL_STRICT)
		{
			p.setDeleted();
			// unselect
			d.setSelectedPuckID(-1);

			SmartiesEvent e = new SmartiesEvent();
			e.id = (p != null)? p.id:-1; e.p = p;
			e.device = d;
			e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_DELETE;
			_fireEvent(e);
			_sendDeletePuck(p, null);
		}
		else
		{
			_sendPuckLockStatus(p, d, 0);
		}

		_devices.Remove(ip);
    }

    void _handleNewPuck(List<object> args){
    	string ip = (string) args[0];
    	float x = (float)args[1]; 
		float y = (float)args[2];
		
		if (true)
		{
			Debug.Log("Received '/newPuck' message with args: "
				+ ip + " " + x + " " + y);
		}

		if (_checkDevice(ip))
		{
			// new device
			return;
		}

		SmartiesDevice d = _devices[ip];
		SmartiesPuck p = _addPuck(d, x, y);

		SmartiesEvent e = new SmartiesEvent();
		e.p = p; e.id = e.p.id; e.x = p.x; e.y = p.y;
		e.device = d;
		e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_CREATE;
		//System.out.println("_handleNewPuck: _fireEvent");
		_fireEvent(e);

		// send back to the device so that it could have the id ...
		d.sendNewPuck(p, sharingPolicyConf);

		// send to the other devices
		_sendNewPuck(p, ip);
    }

    void _handleSelectPuck(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];

		if (_checkDevice(ip))
		{
			// new device
			return;
		}

		SmartiesDevice d = getDevice(ip);
		SmartiesPuck p = getPuck(id);

		if (p != null)
		{
			if (p.selected_by_device != null && p.selected_by_device != d)
			{
				//  (FIXME SHARINGPOL)
				if (sharingPolicyConf != SMARTIES_SHARINGPOL_PERMISSIVE &&
				    sharingPolicyConf != SMARTIES_SHARINGPOL_CUSTOM)
				{
					Debug.Log("WARNNING in _handleSelectPuck " + sharingPolicyConf);
				}
			}
			
			p.selected_by_device = p.sharing_policy_device = d;
			if (sharingPolicyConf == SMARTIES_SHARINGPOL_MEDIUM ||
			    sharingPolicyConf ==  SMARTIES_SHARINGPOL_STRICT)
			{
				_sendPuckLockStatus(p, d, 1);
			}
		}

		int prev_selected_id = d.getSelectedPuckID();
		d.setSelectedPuckID(id);

		// should unlock prev_selected_id (FIXME SHARINGPOL)
		SmartiesPuck ps = getPuck(prev_selected_id);
		if (ps != null)
		{
			ps.selected_by_device = null;
			if (sharingPolicyConf == SMARTIES_SHARINGPOL_MEDIUM)
			{
				_sendPuckLockStatus(ps, d, 0);
			}
		}
		
		SmartiesEvent e = new SmartiesEvent();
		e.id = (p != null)? p.id:-1; e.p = p; 
		e.device = d;
		e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_SELECT;
		_fireEvent(e);
    }

    void _handleSharePuck(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];

		if (_checkDevice(ip))
		{
			// new device
			return;
		}

		SmartiesDevice d = getDevice(ip);
		SmartiesPuck p = getPuck(id);

		if (p != null)
		{
			p.selected_by_device = null;
			if (sharingPolicyConf ==  SMARTIES_SHARINGPOL_STRICT)
			{
				_sendPuckLockStatus(p, d, 0);
			}
		}

		SmartiesEvent e = new SmartiesEvent();
		e.id = (p != null)? p.id:-1; e.p = p; 
		e.device = d;
		e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_SHARE;
		_fireEvent(e);
    }

    void _handleDeletePuck(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesDevice d = getDevice(ip);
		SmartiesPuck p = getPuck(id);

		if (p == null) {
			return;
		}

		p.setDeleted();
		// unselect
		d.setSelectedPuckID(-1);

		SmartiesEvent e = new SmartiesEvent();
		e.id = (p != null)? p.id:-1; e.p = p;
		e.device = d;
		e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_DELETE;
		_fireEvent(e);

		// send to the other devices
		_sendDeletePuck(p, ip);
    }

    void _handleStorePuck(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];

		if (_checkDevice(ip))
		{
			// new device
			return;
		}

		SmartiesDevice d = getDevice(ip);
		SmartiesPuck p = getPuck(id);

		if (p == null)
		{
			// not found
			return;
		}

		// unselect FIXME strange ???
		int prev_selected_id = d.getSelectedPuckID();
		SmartiesPuck ps = getPuck(prev_selected_id);
		if (ps != null)
		{
			ps.selected_by_device = null;
			//_sendPuckLockStatus(ps, d, 0);
		}
		d.setSelectedPuckID(-1);

		p.setStored();

		SmartiesEvent e = new SmartiesEvent();
		e.id = p.id; e.p = p;
		e.device = d;
		e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_STORE;
		_fireEvent(e);

		// send to the other devices
		_sendStorePuck(p, ip);
    }

    void _handleUnstorePuck(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];

		if (_checkDevice(ip))
		{
			// new device
			return;
		}

		SmartiesDevice d = getDevice(ip);
		SmartiesPuck p = getPuck(id);

		if (p == null)
		{
			return;
		}

		p.setStored(false);

		// FIXME: should "select" ... no let the client do that

		SmartiesEvent e = new SmartiesEvent();
		e.id =  p.id; e.p = p; e.x = p.x; e.y = p.y;
		e.device = d;
		e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_UNSTORE;
		_fireEvent(e);

		// send to the other devices
		_sendUnstorePuck(p, ip);
    }

    void _handleStartMovePuck(List<object> args){
    	string ip = (string) args[0];
    	int id = (int)args[1];
		int mode = (int)args[2];

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesDevice d = getDevice(ip);
		SmartiesPuck p = getPuck(id);

		if (p == null)
		{
			return;
		}

		p.move_state = 1;

		SmartiesEvent e = new SmartiesEvent();
		e.id =  p.id; e.p = p; e.x = p.x; e.y = p.y;
		e.device = d;
		e.mode = (mode == 1)?
		      SmartiesEvent.SMARTIES_GESTUREMOD_DRAG :
		      SmartiesEvent.SMARTIES_GESTUREMOD_HOVER; 
		e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_START_MOVE;
		_fireEvent(e);
    }

    void _handleMovePuck(List<object> args){
    	string ip = (string) args[0];
    	int id = (int)args[1];
		float x = (float)args[2];
		float y = (float)args[3];
		int mode = (int)args[4];

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesDevice d = getDevice(ip);
		SmartiesPuck p = getPuck(id);

		if (p == null)
		{
			return;
		}

		if (p.isStored())
		{
			return;
		}

		p.setPosition(x, y);

		SmartiesEvent e = new SmartiesEvent();
		e.id =  p.id; e.p = p; e.x = p.x; e.y = p.y;
		e.device = d;
		e.mode = (mode == 1)?
		      SmartiesEvent.SMARTIES_GESTUREMOD_DRAG :
		      SmartiesEvent.SMARTIES_GESTUREMOD_HOVER; 
		e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_MOVE;
		_fireEvent(e);

		// should send back the position ...
		_sendMovePuck(p, ip);
    }

    void _handleEndMovePuck(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];
		int mode = (int)args[2];

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesDevice d = getDevice(ip);
		SmartiesPuck p = getPuck(id);

		if (p == null)
		{
			return;
		}

		p.move_state = 0;

		SmartiesEvent e = new SmartiesEvent();
		e.id =  p.id; e.p = p; e.x = p.x; e.y = p.y;
		e.device = d;
		e.mode = (mode == 1)? 
		     SmartiesEvent.SMARTIES_GESTUREMOD_DRAG : 
		     SmartiesEvent.SMARTIES_GESTUREMOD_HOVER; 
		e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_END_MOVE;
		_fireEvent(e);
    }

    // MultiFingerLongPress too
    void _handleMultiFingerMultiTaps(int type, List<object> args) {
    	string ip = (string) args[0];
		int id = (int)args[1];
		int nbr_taps = (int)args[2];
		int nbr_fingers = (int)args[3];
		float ctime = -1.0f;
		if (args.Count > 4)
		{
			ctime = (float)args[4];
		}

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesDevice d = getDevice(ip);
		SmartiesPuck p = getPuck(id);

		SmartiesEvent e = new SmartiesEvent();
		e.id =  (p != null)? p.id:-1; e.p = p;
		e.x =  (p == null)? 0:p.x; e.y = (p == null)? 0:p.y;
		e.device = d;
		e.num_taps = nbr_taps; e.num_fingers = nbr_fingers;
		e.duration = (long)ctime;
		e.type = type;
		_fireEvent(e);
    }

    void _handleStartMFPinch(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];
		int num_fingers = (int)args[2];
		float x = (float)args[3];
		float y = (float)args[4];
		float d = (float)args[5];
		float a = (float)args[6];
		int num_taps  = (int)args[7];
		int post_mode  = (int)args[8];

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesDevice dev = getDevice(ip);
		SmartiesPuck p = getPuck(id);

		SmartiesEvent e = new SmartiesEvent();
		e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_START_MFPINCH;
		e.id = (p != null)? p.id:-1; e.p = p; 
		e.device = dev;
		e.num_fingers = num_fingers;
		e.num_taps = num_taps;
		e.post_mode = post_mode;
		e.mode = (num_taps == 0)? SmartiesEvent.SMARTIES_GESTUREMOD_HOVER : SmartiesEvent.SMARTIES_GESTUREMOD_DRAG;
		e.x = x; e.y = y; 
		e.d = d;
		e.a = a;	
		_fireEvent(e);
    }

    void _handleStartMFMove(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];
		int num_fingers = (int)args[2];
		float x = (float)args[3];
		float y = (float)args[4];
		float d = (float)args[5];
		float a = (float)args[6];
		int num_taps  = (int)args[7];
		int post_mode  = (int)args[8];

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesDevice dev = getDevice(ip);
		SmartiesPuck p = getPuck(id);

		SmartiesEvent e = new SmartiesEvent();
		e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_START_MFMOVE;
		e.id = (p != null)? p.id:-1; e.p = p; 
		e.device = dev;
		e.num_fingers = num_fingers;
		e.num_taps = num_taps;
		e.post_mode = post_mode;
		e.mode = (num_taps == 0)? SmartiesEvent.SMARTIES_GESTUREMOD_HOVER : SmartiesEvent.SMARTIES_GESTUREMOD_DRAG;
		e.x = x; e.y = y; 
		e.d = d;
		e.a = a;	
		_fireEvent(e);
    }

    void _handleMFPinch(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];
		int num_fingers = (int)args[2];
		float x = (float)args[3];
		float y = (float)args[4];
		float d = (float)args[5];
		float a = (float)args[6];
		int num_taps  = (int)args[7];
		int post_mode  = (int)args[8];

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesDevice dev = getDevice(ip);
		SmartiesPuck p = getPuck(id);

		SmartiesEvent e = new SmartiesEvent();
		e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_MFPINCH;
		e.id = (p != null)? p.id:-1; e.p = p; 
		e.device = dev;
		e.num_fingers = num_fingers;
		e.num_taps = num_taps;
		e.post_mode = post_mode;
		e.mode = (num_taps == 0)? SmartiesEvent.SMARTIES_GESTUREMOD_HOVER : SmartiesEvent.SMARTIES_GESTUREMOD_DRAG;
		e.x = x; e.y = y; 
		e.d = d;
		e.a = a;	
		_fireEvent(e);
    }

    void _handleMFMove(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];
		int num_fingers = (int)args[2];
		float x = (float)args[3];
		float y = (float)args[4];
		float d = (float)args[5];
		float a = (float)args[6];
		int num_taps  = (int)args[7];
		int post_mode  = (int)args[8];

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesDevice dev = getDevice(ip);
		SmartiesPuck p = getPuck(id);

		SmartiesEvent e = new SmartiesEvent();
		e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_MFMOVE;
		e.id = (p != null)? p.id:-1; e.p = p; 
		e.device = dev;
		e.num_fingers = num_fingers;
		e.num_taps = num_taps;
		e.post_mode = post_mode;
		e.mode = (num_taps == 0)? SmartiesEvent.SMARTIES_GESTUREMOD_HOVER : SmartiesEvent.SMARTIES_GESTUREMOD_DRAG;
		e.x = x; e.y = y; 
		e.d = d;
		e.a = a;	
		_fireEvent(e);
    }

    void _handleEndMFPinch(List<object> args) {
    	string ip = (string) args[0];
		int id = (int)args[1];

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesDevice dev = getDevice(ip);
		SmartiesPuck p = getPuck(id);

		SmartiesEvent e = new SmartiesEvent();
		e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_END_MFPINCH;
		e.id = (p != null)? p.id:-1; e.p = p; 
		e.device = dev;
		_fireEvent(e);
    }

    void _handleEndMFMove(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesDevice dev = getDevice(ip);
		SmartiesPuck p = getPuck(id);

		SmartiesEvent e = new SmartiesEvent();
		e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_END_MFMOVE;
		e.id = (p != null)? p.id:-1; e.p = p; 
		e.device = dev;
		_fireEvent(e);
    }

    // ----------------------------------------------------------------------------
	// raw event

	void _handleRawTouch(int type, List<object> args) {
    	string ip = (string) args[0];
    	int idPuck = (int)args[1];
		float ctime = (float)args[2];
		int idFinger = (int)args[3];
		float x = (float)args[4];
		float y = (float)args[5];
			
		if (_checkDevice(ip)) {
			return;
		}
			
		SmartiesEvent e = new SmartiesEvent();
		e.p = getPuck(idPuck); e.id = (e.p != null)? e.p.id:-1; 
		e.device =getDevice(ip);
		e.type = type;
		e.x = x; e.y = y;
		e.client_time = (long)ctime;
		e.finger_id = idFinger;
		_fireEvent(e);
    }

	// ----------------------------------------------------------------------------
	// handle keyboard msg
	
	void _handleKeyDown(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1]; // smarties id
		int keycode = (int)args[2]; // 

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesDevice dev = getDevice(ip);
		SmartiesPuck p = getPuck(id);

		SmartiesEvent e = new SmartiesEvent();
		e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_KEYDOWN;
		e.id = (p != null)? p.id:-1; e.p = p; 
		e.device = dev;
		e.keycode = keycode;
		_fireEvent(e);
    }

	void _handleKeyUp(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1]; // smarties id
		int keycode = (int)args[2]; // 

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesDevice dev = getDevice(ip);
		SmartiesPuck p = getPuck(id);

		SmartiesEvent e = new SmartiesEvent();
		e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_KEYUP;
		e.id = (p != null)? p.id:-1; e.p = p; 
		e.device = dev;
		e.keycode = keycode;
		_fireEvent(e);
    }

	void _handleStringEdit(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];
		int widget_id = (int)args[2];
		string text = (string)args[3];

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesWidget w = getWidget(widget_id);

		SmartiesEvent e = new SmartiesEvent();
		e.p = getPuck(id); e.id = (e.p != null)? e.p.id:-1; 
		e.widget = w; e.device = getDevice(ip);
		e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_STRING_EDIT;
		e.text = text;
		_fireEvent(e);
    }

	// ----------------------------------------------------------------------------
	// handle widgets msg

	void _handleButtonClick(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];
		int widget_id = (int)args[2];

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesWidget w = getWidget(widget_id);
		if (w != null)
		{
			w.clicked++;
			w.touch_action = SmartiesWidget.SMARTIES_WIDGET_BUTTON_CLICK;
			w.pid = id;
			w.dirty = true;
			SmartiesEvent e = new SmartiesEvent();
			e.p = getPuck(id); e.id = (e.p != null)? e.p.id:-1; 
			e.widget = w; e.device = getDevice(ip);
			e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_WIDGET;
			_fireEvent(e);
		}
    }

	void _handleButtonDown(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];
		int widget_id = (int)args[2];
		float x = (float)args[3];
		float y = (float)args[4];

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesWidget w = getWidget(widget_id);
		if (w != null)
		{
			w.clicked++;
			w.touch_action = SmartiesWidget.SMARTIES_WIDGET_BUTTON_DOWN;
			w.bx = x; w.by = y;
			w.pid = id;
			w.dirty = true;
			SmartiesEvent e = new SmartiesEvent();
			e.p = getPuck(id); e.id = (e.p != null)? e.p.id:-1; 
			e.widget = w; e.device = getDevice(ip);
			e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_WIDGET;
			_fireEvent(e);
		}
    }

	void _handleButtonMove(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];
		int widget_id = (int)args[2];
		float x = (float)args[3];
		float y = (float)args[4];

		if (_checkDevice(ip))
		{
			// new device
			return;
		}

		SmartiesWidget w = getWidget(widget_id);
		if (w != null)
		{
			w.clicked++;
			w.touch_action = SmartiesWidget.SMARTIES_WIDGET_BUTTON_MOVE;
			w.bx = x; w.by = y;
			w.pid = id;
			w.dirty = true;
			SmartiesEvent e = new SmartiesEvent();
			e.p = getPuck(id); e.id = (e.p != null)? e.p.id:-1; 
			e.widget = w; e.device = getDevice(ip);
			e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_WIDGET;
			_fireEvent(e);
		}
			
    }

	void _handleButtonUp(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];
		int widget_id = (int)args[2];
		float x = (float)args[3];
		float y = (float)args[4];

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesWidget w = getWidget(widget_id);
		if (w != null)
		{
			w.clicked++;
			w.touch_action = SmartiesWidget.SMARTIES_WIDGET_BUTTON_UP;
			w.bx = x; w.by = y;
			w.pid = id;
			w.dirty = true;
			SmartiesEvent e = new SmartiesEvent();
			e.p = getPuck(id); e.id = (e.p != null)? e.p.id:-1; 
			e.widget = w; e.device = getDevice(ip);
			e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_WIDGET;
			_fireEvent(e);
		}
    }

	void _handleTogglebuttonClick(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];
		int widget_id = (int)args[2];
		int on = (int)args[3];

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesWidget w = getWidget(widget_id);
		if (w != null)
		{
			w.clicked++;
			w.on = (on > 0)? true:false;
			w.pid = id;
			w.dirty = true;
			SmartiesEvent e = new SmartiesEvent();
			e.p = getPuck(id); e.id = (e.p != null)? e.p.id:-1; 
			e.widget = w; e.device = getDevice(ip);
			e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_WIDGET;
			e.value = (w.on)? 1:0;
			_fireEvent(e);
		}
    }

	void _handleCheckboxClick(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];
		int widget_id = (int)args[2];
		int on = (int)args[3];

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesWidget w = getWidget(widget_id);
		if (w != null)
		{
			w.clicked++;
			w.on = (on > 0)? true:false;
			w.pid = id;
			w.dirty = true;
			SmartiesEvent e = new SmartiesEvent();
			e.p = getPuck(id); e.id = (e.p != null)? e.p.id:-1; 
			e.widget = w; e.device = getDevice(ip);
			e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_WIDGET;
			e.value = (w.on)? 1:0;
			_fireEvent(e);
		}
    }

	void _handleTextButton(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];
		int widget_id = (int)args[2];
		string text = (string)args[3];
		// comp
		int cancel = 0;
		if (args.Count >= 5)
		{
			cancel = (int)args[4];
		}

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesWidget w = getWidget(widget_id);
		if (w != null)
		{
			w.text = text;
			w.pid = id;
			w.cancel = (cancel > 0)? true:false; 
			w.dirty = true;
			SmartiesEvent e = new SmartiesEvent();
			e.p = getPuck(id); e.id = (e.p != null)? e.p.id:-1; 
			e.widget = w; e.device = getDevice(ip);
			e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_WIDGET;
			e.text = text;
			_fireEvent(e);
		}
    }

	void _handleSliderValue(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];
		int widget_id = (int)args[2];
		int value = (int)args[3];
		
		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesWidget w = getWidget(widget_id);
		if (w != null)
		{
			
			w.slider_value = value;
			w.pid = id;
			w.dirty = true;
			SmartiesEvent e = new SmartiesEvent();
			e.p = getPuck(id); e.id = (e.p != null)? e.p.id:-1; 
			e.widget = w; e.device = getDevice(ip);
			e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_WIDGET;
			e.value = value;
			_fireEvent(e);
		}
    }

	void _handleSpinnerItem(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];
		int widget_id = (int)args[2];
		int item = (int)args[3];

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesWidget w = getWidget(widget_id);
		if (w != null)
		{
			if (w.item != item)
			{
				w.item = item;
				w.pid = id;
				w.dirty = true;
				SmartiesEvent e = new SmartiesEvent();
				e.p = getPuck(id); e.id = (e.p != null)? e.p.id:-1; 
				e.widget = w; e.device = getDevice(ip);
				e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_WIDGET;
				e.value = (w.on)? 1:0;
				_fireEvent(e);
			}
		}
    }

	void _handleMultiChoiceItem(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];
		int widget_id = (int)args[2];
		int item = (int)args[3];
		bool checkedd = (bool)args[4];

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesWidget w = getWidget(widget_id);
		if (w != null)
		{
			w.cleanUpCheckedItems();
			if (w.item < 0 || w.item >= w.checked_items.Count)
			{
				// should never happen
				return;
			}
			w.checked_items[w.item] = checkedd;
			w.on = checkedd;
			w.item = item;
			w.pid = id;
			w.dirty = true;
			SmartiesEvent e = new SmartiesEvent();
			e.p = getPuck(id); e.id = (e.p != null)? e.p.id:-1; 
			e.widget = w; e.device = getDevice(ip);
			e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_WIDGET;
			e.value = (checkedd)? 1:0;
			_fireEvent(e);
		}
    }

	void _handlePopupMenuItem(List<object> args) {
    	string ip = (string) args[0];
    	int id = (int)args[1];
		int widget_id = (int)args[2];
		int item = (int)args[3];

		if (_checkDevice(ip))
		{
			return;
		}

		SmartiesWidget w = getWidget(widget_id);
		if (w != null)
		{
			w.item = item;
			w.pid = id;
			w.dirty = true;
			SmartiesEvent e = new SmartiesEvent();
			e.p = getPuck(id); e.id = (e.p != null)? e.p.id:-1; 
			e.widget = w; e.device = getDevice(ip);
			e.type = SmartiesEvent.SMARTIES_EVENTS_TYPE_WIDGET;
			e.value = (w.on)? 1:0;
			_fireEvent(e);
		}
    }

    // ----------------------------------------------------------------------------
	// get stuff

	public SmartiesPuck getPuck(int id)
	{
		if (!_pucks.ContainsKey(id)) {
			return null;
		}
		else{
			return _pucks[id];
		}
	}

	public Dictionary<int, SmartiesPuck> getPuckMapping()
	{
		return _pucks;
	}

	public SmartiesDevice getDevice(string ip)
	{
		if (!_devices.ContainsKey(ip)) {
			return null;
		}
		else{
			return _devices[ip];
		}
	}

	public Dictionary<string, SmartiesDevice> getDevicesMapping()
	{
		return _devices;
	}

	// ----------------------------------------------------------------------------
	// events
	private void _fireEvent(SmartiesEvent e)
	{
		// FIXME: improve for flushing certain events, touch and slider? 
		//        Use a queue and a timer?
		SmartiesUpdate(this, e);	
	}
	// ----------------------------------------------------------------------------
	// external source smarties "action"

	
	public void movePuck(int id, float x, float y)
	{
		SmartiesPuck p = getPuck(id);
		if (p == null || p.isStored()) return;

		p.setPosition(x, y);

		// should send back the position ...
		_sendMovePuck(p, null);
	}

	public void movePuck(SmartiesPuck p, float x, float y)
	{
		if (p == null || p.isStored()) return;
		movePuck(p.id,x,y);
	}

	public void deletePuck(int id)
	{
		SmartiesPuck p =  getPuck(id);
		if (p == null) return;	
		_pucks.Remove(id);
	}

	public void sendPuckCursorType(SmartiesPuck p, string ip, int cursor_type)
	{
		p.cursor_type = cursor_type;
		foreach (SmartiesDevice d in _devices.Values)
		{
			if (ip != null && ip != d.getIP())
			{
				continue;
			}
			d.sendPuckCursorType(p);
		}
	}

	public void sendPuckColor(SmartiesPuck p, string ip, int color)
	{
		p.color = color;
		foreach (SmartiesDevice d in _devices.Values)
		{
			if (ip != null && ip != d.getIP())
			{
				continue;
			}
			d.sendPuckColor(p);
		}
	}

	public SmartiesPuck createPuck(SmartiesDevice d, float x, float y, bool select)
	{
		SmartiesPuck p = _addPuck(d, x, y);
		if (d != null)
		{
			if (select)
			{
				p.setSelectedByDevice(d);
				d.setSelectedPuckID(p.id);
			}
			else
			{
				p.setSelectedByDevice(null);	
			}
			// send back to the device so that it could have the id ...
			d.sendNewPuck(p, sharingPolicyConf);
		}

		// send to the other devices
		_sendNewPuck(p, (d==null)? null:d.getIP());
		return p;
	}


	// ----------------------------------------------------------------------------
	// widgets management from the server should we do an event queue ?

	private SmartiesDevice _getSafeDevice(SmartiesDevice d)
	{
		SmartiesDevice safed = null;
		foreach (SmartiesDevice td in _devices.Values)
		{
			if (td == d)
			{
				safed = d;
				break;
			}
		}
		return safed;
	}

	public void  showKeyboard(int id, string ip, int num_keys)
	{
		SmartiesDevice d = getDevice(ip);
		if (d != null)
		{
			d.sendShowkeyboard(id, num_keys);
		}
	}

	public void  showKeyboard(int id, string ip)
	{
		showKeyboard(id, ip, 0);
	}

	public void  showKeyboard(int id, SmartiesDevice d, int num_keys)
	{
		
		SmartiesDevice safed = _getSafeDevice(d);
		if (safed != null)
		{
			d.sendShowkeyboard(id, num_keys);
		}
	}

	public void  showKeyboard(int id, SmartiesDevice d)
	{
		showKeyboard(id, d, 0);
	}

	public void  sendWidgetLabel(int wid, string str, SmartiesDevice dev)
	{
		SmartiesWidget w = getWidget(wid);
		if (w == null) return;
		if (dev == null)
		{
			w.label = str;
		}

		foreach (SmartiesDevice d in _devices.Values)
		{
			if (dev != null && d != dev)
			{
				continue;
			}
			d.sendWidgetLabel(wid, str);
		}
	}

	public void  sendWidgetLabel(int wid, string str, string ip)
	{
		SmartiesDevice dev = getDevice(ip);
		sendWidgetLabel(wid, str, dev);
	}

	public void  sendWidgetLabel(int wid, string str)
	{
		sendWidgetLabel(wid, str, (SmartiesDevice)null);
	}

	public void  sendWidgetFontSize(int wid, int size, SmartiesDevice dev)
	{
		SmartiesWidget w = getWidget(wid);
		if (w == null) return;
		if (dev == null)
		{
			w.font_size = size;
		}

		foreach (SmartiesDevice d in _devices.Values)
		{
			if (dev != null && d != dev)
			{
				continue;
			}
			d.sendWidgetFontSize(wid, size);
		}
	}

	public void sendWidgetFontSize(int wid, int size, string ip)
	{
		SmartiesDevice dev = getDevice(ip);
		sendWidgetFontSize(wid, size, dev);
	}

	public void  sendWidgetFontSize(int wid, int size)
	{
		sendWidgetFontSize(wid, size, (SmartiesDevice)null);
	}

	public void  sendWidgetItem(int wid, int item, SmartiesDevice dev)
	{
		SmartiesWidget w = getWidget(wid);
		if (w == null) return;
		if (dev == null)
		{
			w.item = item;
		}

		foreach (SmartiesDevice d in _devices.Values)
		{
			if (dev != null && d != dev)
			{
				continue;
			}
			d.sendWidgetItem(wid, item);
		}
	}

	public void  sendWidgetItem(int wid, int item, string ip)
	{
		SmartiesDevice dev = getDevice(ip);
		sendWidgetItem(wid, item, dev);
	}

	public void  sendWidgetItem(int wid, int item)
	{
		sendWidgetItem(wid, item, (SmartiesDevice)null);
	}

	public void  sendWidgetValue(int wid, int value, SmartiesDevice dev)
	{
		SmartiesWidget w = getWidget(wid);
		if (w == null) return;
		if (dev == null)
		{
			w.slider_value =value;
		}
		foreach (SmartiesDevice d in _devices.Values)
		{
			if (dev != null && d != dev)
			{
				continue;
			}
			d.sendWidgetValue(wid, value);
		}
	}

	public void  sendWidgetValue(int wid, int value, string ip)
	{
		SmartiesDevice dev = getDevice(ip);
		sendWidgetValue(wid, value, dev);
	}

	public void  sendWidgetValue(int wid, int value)
	{
		sendWidgetValue(wid, value, (SmartiesDevice)null);
	}

	public void  sendWidgetOnState(int wid, bool onstate, SmartiesDevice dev)
	{
		SmartiesWidget w = getWidget(wid);
		if (w == null) return;
		if (dev == null)
		{
			w.on = onstate;
		}

		foreach (SmartiesDevice d in _devices.Values)
		{
			if (dev != null && d != dev)
			{
				continue;
			}
			d.sendWidgetOnState(wid, onstate);
		}
	}

	public void  sendWidgetOnState(int wid, bool onstate, string ip)
	{
		SmartiesDevice dev = getDevice(ip);
		sendWidgetOnState(wid, onstate, dev);
	}

	public void  sendWidgetOnState(int wid, bool onstate)
	{
		sendWidgetOnState(wid, onstate, (SmartiesDevice)null);
	}

	public void  replaceWidgetItemsList(int wid, List<string> items)
	{
		SmartiesWidget w = getWidget(wid);
		if (w == null) return;

		w.items = items;
		foreach (SmartiesDevice d in _devices.Values)
		{
			d.replaceWidgetItemsList(wid, items);
		}
	}	

	public void  addItemInWidgetList(int wid, string item, int pos)
	{
		SmartiesWidget w = getWidget(wid);
		if (w == null) return;

		if (pos >= 0 && pos < w.items.Count)
		{
			w.items[pos] = item;
		}
		else
		{
			w.items.Add(item);	
		}

		foreach (SmartiesDevice d in _devices.Values)
		{
			d.addItemInWidgetList(wid, item, pos);
		}
	}

	public void  removeItemInWidgetList(int wid, int pos)
	{
		SmartiesWidget w = getWidget(wid);
		if (w == null) return;
		
		if (pos >= 0 && pos < w.items.Count)
		{
			w.items.RemoveAt(pos);
		}
		else if (w.items.Count > 0)
		{
			w.items.RemoveAt(w.items.Count-1);
		}

		foreach (SmartiesDevice d in _devices.Values)
		{
			d.removeItemInWidgetList(wid, pos);
		}
	}

	public void  sendWidgetCheckedItems(int wid, List<bool> checked_items, SmartiesDevice dev)
	{
		SmartiesWidget w = getWidget(wid);
		if (w == null) return;
		if (dev == null)
		{
			w.checked_items = checked_items;
			w.cleanUpCheckedItems();
		}

		foreach (SmartiesDevice d in _devices.Values)
		{
			if (dev != null && d != dev)
			{
				continue;
			}
			d.sendWidgetCheckedItems(wid, checked_items);
		}
	}

	public void  sendWidgetCheckedItems(int wid, List<bool> checked_items, string ip)
	{
		SmartiesDevice dev = getDevice(ip);
		sendWidgetCheckedItems(wid, checked_items, dev);
	}

	public void  sendWidgetCheckedItems(int wid, List<bool> checked_items)
	{
		sendWidgetCheckedItems(wid, checked_items, (SmartiesDevice)null);
	}

	public void  sendWidgetState(int wid, int state, SmartiesDevice dev)
	{
		SmartiesWidget w = getWidget(wid);
		if (w == null) return;
		if (dev == null)
		{
			w.visibility = state;
		}

		foreach (SmartiesDevice d in _devices.Values)
		{
			if (dev != null && d != dev)
			{
				continue;
			}
			d.sendWidgetState(wid, state);
		}
	}

	public void  sendWidgetState(int wid, int state, string ip)
	{
		SmartiesDevice dev = getDevice(ip);
		sendWidgetState(wid, state, dev);
	}

	public void  sendWidgetState(int wid, int state)
	{
		sendWidgetState(wid, state, (SmartiesDevice)null);
	}

	public void sendNotification(int soundType, int soundDuration, int vibration, string str,
		SmartiesDevice dev)
	{
		foreach (SmartiesDevice d in _devices.Values)
		{
			if (dev != null && d != dev)
			{
				continue;
			}
			d.sendNotification(soundType, soundDuration, vibration, str);
		}
	}

	public void sendNotification(int soundType, int soundDuration, int vibration, string str,
		string ip)
	{
		SmartiesDevice dev = getDevice(ip);
		sendNotification(soundType, soundDuration, vibration, str, dev);
	}

	public void sendNotification(int soundType, int soundDuration, int vibration, string str)
	{
		sendNotification(soundType, soundDuration, vibration, str, (SmartiesDevice)null);
	}


	// ----------------------------------------------------------------------------
	// widgets init and utils

	public void initWidgets(int wgridw, int wgridh)
	{
		_widgets_gridx = wgridw;
		_widgets_gridy = wgridh;
	}

	public SmartiesWidget addWidget(int type, string label, float x, float y, float w, float h)
	{
		_widget_uid++;
		SmartiesWidget widget =
			new SmartiesWidget(_widget_uid, type, label, x,y,w,h);
		_widgets.Add(widget);
		return widget;
	}

	public SmartiesWidget getWidget(int wid)
	{
		foreach (SmartiesWidget w in _widgets) {
			if (wid == w.uid) { return w; }
		}
		return null;
	}

	// ----------------------------------------------------------------------------
	// config stuff
	public void setPureTouchpad(bool v)
	{
		pureTouchpad = v;
	}

	public void createOnePuckByDevice(bool v)
	{
		onePuckByDevice = v;
	}

	public void setInterfaceButtons(bool pb, bool sb, bool di, bool si)
	{
		noPuckButton = !pb;
		noShareButton = !sb;
		noDeleteIcon = !di;
		noStoreIcon = !si;
	}

	public void setPureWidgetArea(bool v)
	{
		pureWidgetArea = v;
		setInterfaceButtons(false, false, false, false);
	}

	public void setDeviceOrientation(int v)
	{
		deviceOrientation = v;
	}

	public void setSharingPolicyStrict()
	{
		sharingPolicyConf = SMARTIES_SHARINGPOL_STRICT;
	}

	public void setSharingPolicyPermissive()
	{
		sharingPolicyConf = SMARTIES_SHARINGPOL_PERMISSIVE;
	}

	public void setSharingPolicyCustom()
	{
		sharingPolicyConf = SMARTIES_SHARINGPOL_CUSTOM;
	}

	public void setSmartiesTouchEventsConf(bool v, SmartiesDevice dev)
	{
	
		foreach (SmartiesDevice d in _devices.Values)
		{
			if (dev != null && d != dev)
			{
				continue;
			}
			d.sendSmartiesTouchEventsConf(v);
		}
	}

	public void setSmartiesTouchEventsConf(bool v)
	{
		smartiesTouchEventsConf = v;
		setSmartiesTouchEventsConf(v, null);
	}

	public void setRawTouchEventsConf(bool v, SmartiesDevice dev)
	{
	
		foreach (SmartiesDevice d in _devices.Values)
		{
			if (dev != null && d != dev)
			{
				continue;
			}
			d.sendRawTouchEventsConf(v);
		}
	}

	public void setRawTouchEventsConf(bool v)
	{
		rawTouchEventsConf = v;
		setRawTouchEventsConf(v, null);
	}

	public void setHideLockedPucks(bool v, SmartiesDevice dev)
	{
		foreach (SmartiesDevice d in _devices.Values)
		{
			if (dev != null && d != dev)
			{
				continue;
			}
			d.sendHideLockedPucks(v);
		}
	}

	public void setHideLockedPucks(bool v)
	{
		hideLockedPucks = v;
		setHideLockedPucks(v, null);
	}

	public void setAccelerationParameters(float maxCDgainFactor, float minCDgainMM, SmartiesDevice dev)
	{
		foreach (SmartiesDevice d in _devices.Values)
		{
			if (dev != null && d != dev)
			{
				continue;
			}
			d.sendAccelerationParameters(maxCDgainFactor, minCDgainMM);
		}
	}

	public void setAccelerationParameters(float maxCDgainFactor, float minCDgainMM)
	{
		accelParamSeted = true;
		maxCDgainFactorConf = maxCDgainFactor;
		maxCDgainMMConf = minCDgainMM;

	}

    // ---------------------------------------------------------------------
    // init, run ...

    void OnPacketReceived(OSCServer server, OSCPacket packet)
    {
		//Debug.Log ("packet received "+ packet.Address);

		if (packet.Address == "/Smarties/MovePuck"){
			_handleMovePuck(packet.Data);
		}
		else if (packet.Address == "/Smarties/MFPinch"){
			_handleMFPinch(packet.Data);
		}
		else if (packet.Address == "/Smarties/MFMove"){
			_handleMFMove(packet.Data);
		}
		else if (packet.Address == "/Smarties/RawMove"){
			_handleRawTouch(SmartiesEvent.SMARTIES_EVENTS_TYPE_RAW_MOVE, packet.Data);
		}
		else if (packet.Address == "/Smarties/SliderValue"){
			_handleSliderValue(packet.Data);
		}
		else if (packet.Address == "/Smarties/DeviceSizes"){
			_handleDeviceSizes(packet.Data);
		}
		else if (packet.Address == "/Smarties/FailToReadMessage"){
			_handleFailToReadMessage(packet.Data);
		}
		else if (packet.Address == "/Smarties/AskNewPuck"){
			_handleNewPuck(packet.Data);
		}
		else if (packet.Address == "/Smarties/SelectPuck"){
			_handleSelectPuck(packet.Data);
		}
		else if (packet.Address == "/Smarties/SharePuck"){
			_handleSharePuck(packet.Data);
		}
		else if (packet.Address == "/Smarties/MultiFingerMultiTaps"){
			_handleMultiFingerMultiTaps(SmartiesEvent.SMARTIES_EVENTS_TYPE_MULTI_TAPS, packet.Data);
		}
		else if (packet.Address == "/Smarties/MultiFingerInterMultiTape"){
			_handleMultiFingerMultiTaps(SmartiesEvent.SMARTIES_EVENTS_TYPE_TAP, packet.Data);
		}
		else if (packet.Address == "/Smarties/MultiFingerLongPress"){
			_handleMultiFingerMultiTaps(SmartiesEvent.SMARTIES_EVENTS_TYPE_LONGPRESS, packet.Data);
		}
		else if (packet.Address == "/Smarties/StartMovePuck"){
			_handleStartMovePuck(packet.Data);
		}
		else if (packet.Address == "/Smarties/StartMFPinch"){
			_handleStartMFPinch(packet.Data);
		}
		else if (packet.Address == "/Smarties/StartMFMove"){
			_handleStartMFMove(packet.Data);
		}
		else if (packet.Address == "/Smarties/EndMovePuck"){
			_handleEndMovePuck(packet.Data);
		}
		else if (packet.Address == "/Smarties/EndMFPinch"){
			_handleEndMFPinch(packet.Data);
		}
		else if (packet.Address == "/Smarties/EndMFMove"){
			_handleEndMFMove(packet.Data);
		}
		else if (packet.Address == "/Smarties/RawDown"){
			_handleRawTouch(SmartiesEvent.SMARTIES_EVENTS_TYPE_RAW_DOWN, packet.Data);
		}
		else if (packet.Address == "/Smarties/RawUp"){
			_handleRawTouch(SmartiesEvent.SMARTIES_EVENTS_TYPE_RAW_UP, packet.Data);
		}
		else if (packet.Address == "/Smarties/StorePuck"){
			_handleStorePuck(packet.Data);
		}
		else if (packet.Address == "/Smarties/UnstorePuck"){
			_handleUnstorePuck(packet.Data);
		}
		else if (packet.Address == "/Smarties/DeletePuck"){
			_handleDeletePuck(packet.Data);
		}
		else if (packet.Address == "/Smarties/KeyDown"){
			_handleKeyDown(packet.Data);
		}
		else if (packet.Address == "/Smarties/KeyUp"){
			_handleKeyUp(packet.Data);
		}
		else if (packet.Address == "/Smarties/ButtonClick"){
			_handleButtonClick(packet.Data);
		}
		else if (packet.Address == "/Smarties/ButtonDown"){
			_handleButtonDown(packet.Data);
		}
		else if (packet.Address == "/Smarties/ButtonMove"){
			_handleButtonMove(packet.Data);
		}
		else if (packet.Address == "/Smarties/ButtonUp"){
			_handleButtonUp(packet.Data);
		}
		else if (packet.Address == "/Smarties/CheckboxClick"){
			_handleCheckboxClick(packet.Data);
		}
		else if (packet.Address == "/Smarties/TextButton"){
			_handleTextButton(packet.Data);
		}
		else if (packet.Address == "/Smarties/StringEdit"){
			_handleStringEdit(packet.Data);
		}
		else if (packet.Address == "/Smarties/SpinnerItem"){
			_handleSpinnerItem(packet.Data);
		}
		else if (packet.Address == "/Smarties/PopupMenuItem"){
			_handlePopupMenuItem(packet.Data);
		}
		else if (packet.Address == "/Smarties/NewConnection"){
			_handleNewConnection(packet.Data);
		}
		else if (packet.Address == "/Smarties/GetHelloMessage"){
			_handleGetHelloMessage(packet.Data);
		}
    }

    public void Run(){
    	OSCServer server = new OSCServer(port);
		server.PacketReceivedEvent += OnPacketReceived;
    }

	//Smaries(int p, int aw, int ah, int tilew, int tileh){
	//	port = p;
	//}
	public Smarties(int aw, int ah, int tilew, int tileh)
	{
		port = DEFAULT_PORT_SMARTIES;
		_app_width = aw; _app_height = ah; _gridx = tilew; _gridy = tileh;

		_default_cursor_type = 0;

		pureTouchpad = false;
		onePuckByDevice = false;

		_devices  = new Dictionary<string, SmartiesDevice>();
		_pucks  = new Dictionary<int, SmartiesPuck>();

		_widgets_gridx = 1;
		_widgets_gridy = 1;
		_widgets = new List<SmartiesWidget>();
		_widget_uid = -1;

		SmartiesUpdate += delegate(Smarties s, SmartiesEvent e) { };
	}
}