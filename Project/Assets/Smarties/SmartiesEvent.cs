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

public class SmartiesEvent
{
	
/**
 * 
 */	
public const int SMARTIES_EVENTS_TYPE_NONE = 0;
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_MULTI_TAPS  = 1;
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_TAP         = 2;
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_LONGPRESS  = 3;
// TODO or not, How ???
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_DOWN  = 4;

/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_START_MOVE = 51;
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_MOVE       = 52;
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_END_MOVE   = 53;

/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_START_MFPINCH = 54;
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_MFPINCH       = 55;
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_END_MFPINCH   = 56;

/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_START_MFMOVE = 57;
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_MFMOVE       = 58;
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_END_MFMOVE   = 59;

// RAW TOUCH
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_RAW_DOWN   = 60;
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_RAW_UP     = 61;
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_RAW_MOVE   = 62;

// gesture modifiers
/**
 * 
 */
public const int SMARTIES_GESTUREMOD_HOVER  = 0;
/**
 * 
 */
public const int SMARTIES_GESTUREMOD_DRAG   = 1;

// keyboards
//public const int SMARTIES_EVENTS_TYPE_KEY      = 100;
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_KEYUP    = 101;
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_KEYDOWN  = 102;
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_STRING_EDIT  = 110;

// management events
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_CREATE    = 201;
/**
 * 
 */ 
public const int SMARTIES_EVENTS_TYPE_DELETE    = 202;
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_SELECT    = 203;
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_STORE     = 204;
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_UNSTORE   = 205;
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_SHARE    = 206;
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_NEW_DEVICE   = 300;
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_DEVICE_SIZES   = 301;


// widgets events
/**
 * 
 */
public const int SMARTIES_EVENTS_TYPE_WIDGET   = 1000;


/**
 * The event type: one of the SMARTIES_EVENTS_TYPE_* constants.
 */
public int type;
/**
 * identifier of the puck associated to this event (the selected puck),
 * or -1 if the is no associated puck.
 */
public int id;
 /**
 * the puck associated to this event (the selected puck),
 * or null if there is no associated puck. 
 *  Valid for all event types.
 */             
public SmartiesPuck p;        // selected puck (could be null)
/**
 * the device that sent this event.
 * Valid for all event types.
 */
public SmartiesDevice device; // device source
// -------
// smarties tap and gestures
/**
 * number of fingers down at start of a move/pinch gesture.
 * Valid for the the *MFMOVE and *MFPINCH event types.
 */
public int num_fingers;
/**
 * move/drag modifiers (= (num_tapes > 0)? 1:0)
 */
public int mode;
/**
 * number of taps for MULTITAP or num tapes before a [mf][move,drag,pitch] gestures.
 */
public int num_taps;
/**
 *  number of additionnal fingers down after a move/drag/pinch gesture started.
 */
public int post_mode;
/**
 * relative x position of the puck, of a contact point, or of the barycenter of the contact points.
 */
public float x;        // position, pitch distance, angle
/**
 * relative y position of the puck, of a contact point, or of the barycenter of the contact points.
 */
public float y;        // position, pitch distance, angle
/**
 * "pinch distance": sum of the distances from the contact points to the barycenter of
 * the contact points divided by the number of contact points.
 */
public float d;        // position, pitch distance, angle
/**
 * the angle between an horizontal line in the bottom of the device and the line defined by
 * by the two farest contacts points when the gesture start, NOT YET IMPLEMENTED.
 */
public float a; 

/**
 * 
 */
public long duration;
// -------
// for raw multi-touch
/**
 * finger id for raw multi-touch events.
 */
public int finger_id;
/**
 * time of the event in the mobile device for  raw multi-touch events.
 */
public long client_time;
// -------
// keyboard
/**
 * keycode (X Window, QT, GTK ... values).
 * Valid for the *KEY* event types.
 */
public int keycode;
// -------
// widgets
/**
 * The widget of the event.
 * Valide for the SMARTIES_EVENTS_TYPE_WIDGET event type only. 
 */
public SmartiesWidget widget;   // widget
/**
 * A value (slider, slected item, ...). But, just use the widget field.
 * Valide for the SMARTIES_EVENTS_TYPE_WIDGET event type only. 
 */
public int value;                // slider, item, etc.
/**
 * Some text.
 * Valid with the  SMARTIES_EVENTS_TYPE_STRING_EDIT only,
 * this type of event is associated to the
 * {@link SmartiesWidget#SMARTIES_WIDGET_TYPE_INCTEXT_BUTTON} widget type. 
 */
public string text;       // button name, annotation, etc.


public SmartiesEvent()
{
	type = SMARTIES_EVENTS_TYPE_NONE;
	id = -1;
	p = null;
	device = null;
	num_fingers = 0;
	mode = 0;
	num_taps = 0;
	post_mode = 0;
	x = -1; y = -1; d = 0; a = 0;
	duration = -1;
	finger_id = -1; client_time = 0;
	keycode = 0;
	widget = null;
	value = 0;
	text = null;
}

}
